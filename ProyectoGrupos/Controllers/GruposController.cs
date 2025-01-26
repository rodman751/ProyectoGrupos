using Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupos.Models;

namespace ProyectoGrupos.Controllers
{
    //[Authorize]
    public class GruposController : Controller
    {
        private readonly DbContext _context;


        public GruposController(DbContext context)
        {
            _context = context;

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
               Estado = g.Estado
           })
           .ToListAsync();
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
                return BadRequest("El grupo ya tiene el número máximo de integrantes");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            if (await _context.GruposIntegrantes.AnyAsync(gi => gi.IdGrupo == grupoId && gi.IdUsuario == usuario.IdUsuario))
                return BadRequest("El usuario ya es miembro del grupo");
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
            _context.GruposIntegrantes.Update(grupoIntegrante);
            await _context.SaveChangesAsync();

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


            _context.GruposIntegrantes.Add(grupoIntegrante);
            nuevoGrupo.NumeroActualIntegrantes++;

            await _context.SaveChangesAsync();

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


            return RedirectToAction("Index");
        }

    }
}
