using Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupos.Models;

namespace ProyectoGrupos.Controllers
{
    public class CalendarioController : Controller
    {
        private readonly DbContext _context;
        public CalendarioController(DbContext context)
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

        [HttpGet]
        public async Task<IActionResult> Index(int grupoId)
        {
            //grupoId = 1;
            var usuarioId = await GetUserId();
            var grupo = await _context.Grupos
                .Include(g => g.Actividades)
                .FirstOrDefaultAsync(g => g.IdGrupo == grupoId);

            if (grupo == null)
                return NotFound();

            var esAdministrador = await _context.GruposIntegrantes
                .AnyAsync(gi => gi.IdGrupo == grupoId &&
                         gi.IdUsuario == usuarioId &&
                         gi.AdministrarGrupo);

            var eventos = grupo.Actividades.Select(a => new CalendarEventViewModel
            {
                IdActividad = a.IdActividad,
                IdGrupo = a.IdGrupo,
                Titulo = a.Titulo,
                Descripcion = a.Descripcion,
                FechaActividad = a.FechaActividad,
                FechaCreacion = a.FechaCreacion,
                Color = "#007bff", // Default color
                EsAdministrador = esAdministrador
            }).ToList();

            var viewModel = new CalendarViewModel
            {
                GrupoId = grupo.IdGrupo,
                GrupoNombre = grupo.Nombre,
                Eventos = eventos
            };

            ViewBag.EsAdministrador = esAdministrador;
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(int grupoId, string titulo,
            string descripcion, DateTime fechaActividad, string color)
        {
            var usuarioId = await GetUserId();
            // Verificar permisos de administrador
            var esAdministrador = await _context.GruposIntegrantes
                .AnyAsync(gi => gi.IdGrupo == grupoId &&
                         gi.IdUsuario == usuarioId &&
                         gi.AdministrarGrupo);

            if (!esAdministrador)
                return Forbid();

            var actividad = new Actividad
            {
                IdGrupo = grupoId,
                Titulo = titulo,
                Descripcion = descripcion,
                FechaActividad = fechaActividad,
                CreadoPor = usuarioId
            };

            _context.Actividades.Add(actividad);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
