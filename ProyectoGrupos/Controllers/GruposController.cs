using AspNetCoreHero.ToastNotification.Abstractions;
using Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupos.Models;

namespace ProyectoGrupos.Controllers
{
    [Authorize]
    public class GruposController : Controller
    {
        private readonly DbContext _context;
        public INotyfService _notifyService { get; }

        public GruposController(DbContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        private async Task<int> GetUserId()
        {
            // Obtenemos el nombre de usuario del contexto del usuario autenticado
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("El usuario no está autenticado.");
            }

            // Buscamos el usuario en la base de datos por el nombre de usuario
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Nombre == username);

            if (usuario == null)
            {
                throw new InvalidOperationException("El usuario no existe en la base de datos.");
            }

            // Retornamos el Id del usuario
            return usuario.IdUsuario;
        }
        public async Task<IActionResult> Index()
        {
            var userId = await GetUserId();

            // Lista de grupos con información sobre si el usuario es creador o administrador
            var grupos = await _context.Grupos
                .Where(g => g.IdCreador == userId || g.GruposIntegrantes.Any(gi => gi.IdUsuario == userId))
                .Select(g => new GrupoDTO
                {
                    IdGrupo = g.IdGrupo,
                    Nombre = g.Nombre,
                    Descripcion = g.Descripcion,
                    NumeroMaximoIntegrantes = g.NumeroMaximoIntegrantes,
                    NumeroActualIntegrantes = g.NumeroActualIntegrantes,
                    FechaCreacion = g.FechaCreacion,
                    Estado = g.Estado,
                    EsCreador = g.IdCreador == userId, // Verifica si el usuario es el creador del grupo
                    EsAdministrador = g.GruposIntegrantes.Any(gi => gi.IdUsuario == userId && gi.AdministrarGrupo) // Verifica si es administrador
                })
                .ToListAsync();

            // Pasar la lista de grupos al modelo de la vista
            return View(grupos);
        }


        public async Task<IActionResult> Manage(int id)
        {
            var grupo = await _context.Grupos
                .Include(g => g.GruposIntegrantes)
                .ThenInclude(gi => gi.Usuario)
                .FirstOrDefaultAsync(g => g.IdGrupo == id);

            if (grupo == null)
                return NotFound();
            bool esCreador = grupo.IdCreador == await GetUserId();
            var userid = await GetUserId();
            bool esAdministrador = grupo.GruposIntegrantes.Any(gi => gi.IdUsuario == userid && gi.AdministrarGrupo);
            if (esCreador)
            {
                ViewBag.EsCreador = true;
            }
            else { esCreador = false; }
            if (esAdministrador)
            {
                ViewBag.EsAdministrador = true;
            }
            else { esAdministrador = false; }
            var viewModel = new GroupManagementViewModel
            {
                Group = grupo,
                GroupMembers = grupo.GruposIntegrantes.Select(gi => new UsuarioGrupoDto
                {
                    IdUsuario = gi.Usuario.IdUsuario,
                    Nombre = gi.Usuario.Nombre,
                    Email = gi.Usuario.Email,
                    EsAdministrador = gi.AdministrarGrupo
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(int grupoId, string email)
        {
            var Grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.IdGrupo == grupoId);
            if (Grupo == null)
                return NotFound("Grupo no encontrado");

            if (Grupo.NumeroActualIntegrantes >= Grupo.NumeroMaximoIntegrantes)
            {
                _notifyService.Information("El grupo ya tiene el número máximo de integrantes");
                return RedirectToAction("Manage", new { id = grupoId });
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
            {
                _notifyService.Information("Usuario no encontrado");
                return RedirectToAction("Manage", new { id = grupoId });
            }

            if (await _context.GruposIntegrantes.AnyAsync(gi => gi.IdGrupo == grupoId && gi.IdUsuario == usuario.IdUsuario))
            {
                _notifyService.Information("El usuario ya es miembro del grupo");
                return RedirectToAction("Manage", new { id = grupoId });
            }
            var grupoIntegrante = new Entidades.GrupoIntegrante
            {
                IdGrupo = grupoId,
                IdUsuario = usuario.IdUsuario,
                AdministrarGrupo = false
            };
            Grupo.NumeroActualIntegrantes++;
            _context.Grupos.Update(Grupo);
            _context.GruposIntegrantes.Add(grupoIntegrante);
            await _context.SaveChangesAsync();
            _notifyService.Success("Usuario agregado correctamente");
            return RedirectToAction("Manage", new { id = grupoId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int grupoId, int userId)
        {
            var grupoIntegrante = await _context.GruposIntegrantes
                .FirstOrDefaultAsync(gi => gi.IdGrupo == grupoId && gi.IdUsuario == userId);

            if (grupoIntegrante == null)
                return NotFound("Integrante no encontrado");

            var Grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.IdGrupo == grupoId);
            if (Grupo == null)
                return NotFound("Grupo no encontrado");

            Grupo.NumeroActualIntegrantes--;
            _context.Grupos.Update(Grupo);
            _context.GruposIntegrantes.Remove(grupoIntegrante);
            await _context.SaveChangesAsync();
            _notifyService.Success("Usuario eliminado correctamente");
            return RedirectToAction("Manage", new { id = grupoId });
        }
        [HttpPost]
        public async Task<IActionResult> ChangeAdmin(int grupoId, int userId)
        {
            var grupoIntegrante = await _context.GruposIntegrantes
                .FirstOrDefaultAsync(gi => gi.IdGrupo == grupoId && gi.IdUsuario == userId);

            if (grupoIntegrante == null)
                return NotFound("Integrante no encontrado");

            grupoIntegrante.AdministrarGrupo = true;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == userId);
            usuario.Rol = "Colaborador";
            _context.GruposIntegrantes.Update(grupoIntegrante);
            await _context.SaveChangesAsync();
            _notifyService.Success("Usuario ahora es administrador");
            return RedirectToAction("Manage", new { id = grupoId });
        }

        [HttpPost]
        public async Task<IActionResult> CrearGrupo(GrupoDTO grupo)
        {

            var username = User.Identity.Name;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Nombre == username);

            if (usuario == null)
            {
                // Si no se encuentra el usuario, devuelve un error
                return Unauthorized("Usuario no encontrado");
            }

            // Crear el nuevo grupo
            var nuevoGrupo = new Entidades.Grupo
            {
                Nombre = grupo.Nombre,
                Descripcion = grupo.Descripcion,
                NumeroMaximoIntegrantes = grupo.NumeroMaximoIntegrantes,
                NumeroActualIntegrantes = 0,
                IdCreador = usuario.IdUsuario,
                FechaCreacion = DateTime.Now,
                Estado = "Activo"
            };

            _context.Grupos.Add(nuevoGrupo);
            await _context.SaveChangesAsync();

            var grupoIntegrante = new Entidades.GrupoIntegrante
            {
                IdGrupo = nuevoGrupo.IdGrupo,
                IdUsuario = usuario.IdUsuario,
                AdministrarGrupo = true
            };


            usuario.Rol = "Colaborador";
            _context.GruposIntegrantes.Add(grupoIntegrante);
            _context.Usuarios.Update(usuario);
            nuevoGrupo.NumeroActualIntegrantes++;

            await _context.SaveChangesAsync();
            _notifyService.Success("Grupo creado correctamente");
            // Redirigir a la vista de index después de guardar todo
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> EliminarGrupo(int id)
        {
            var grupo = await _context.Grupos.Include(g => g.GruposIntegrantes) // Incluimos los integrantes
                                              .FirstOrDefaultAsync(g => g.IdGrupo == id);

            if (grupo == null)
            {
                // Si no se encuentra el grupo, devolver un error
                return NotFound("Grupo no encontrado");
            }

            // Eliminar los registros de GruposIntegrantes asociados al grupo
            _context.GruposIntegrantes.RemoveRange(grupo.GruposIntegrantes);

            // Ahora eliminar el grupo
            _context.Grupos.Remove(grupo);


            await _context.SaveChangesAsync();

            _notifyService.Success("Grupo eliminado correctamente");
            return RedirectToAction("Index");
        }

    }
}
