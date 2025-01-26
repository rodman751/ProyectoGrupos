using AspNetCoreHero.ToastNotification.Abstractions;
using Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGrupos.Controllers
{
    public class ActividadesController : Controller
    {
        private readonly DbContext _context;
        public INotyfService _notifyService { get; }

        public ActividadesController(DbContext context, INotyfService notifyService)
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
        public ActionResult Index(int grupoId)
        {
            ViewBag.IdGrupo = grupoId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Actividad actividad)
        {
            var usuario = GetUserId();
            if (ModelState.IsValid)
            {
                actividad.FechaCreacion = DateTime.Now;
                actividad.CreadoPor = usuario.Result;
                _context.Actividades.Add(actividad);
                _context.SaveChanges();
                _notifyService.Success("Actividad creada correctamente");

                return RedirectToAction("Index", "Calendario", new { grupoId = actividad.IdGrupo });
            }
            ViewBag.IdGrupo = actividad.IdGrupo;
            _notifyService.Error("Error al crear la actividad");
            return View(actividad);
        }

        // Acción para editar una actividad
        public IActionResult Edit(int id)
        {
            var actividad = _context.Actividades.Find(id);
            if (actividad == null)
            {
                return NotFound();
            }
            // Eliminar segundos y milisegundos de la fecha
            actividad.FechaActividad = actividad.FechaActividad.AddSeconds(-actividad.FechaActividad.Second)
                                                               .AddMilliseconds(-actividad.FechaActividad.Millisecond);

            return View(actividad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Actividad actividad)
        {
            //if (id != actividad.IdActividad)
            //{
            //    return NotFound();
            //}

            if (ModelState.IsValid)
            {


                _context.Actividades.Update(actividad);

                _context.SaveChanges();
                _notifyService.Success("Actividad actualizada correctamente");
                return RedirectToAction("Index", "Calendario", new { grupoId = actividad.IdGrupo });

            }
            _notifyService.Error("Error al actualizar la actividad");
            return View(actividad);
        }

        public IActionResult Delete(int idActividad)
        {
            var actividad = _context.Actividades.Find(idActividad);
            if (actividad == null)
            {
                return NotFound();
            }
            _context.Actividades.Remove(actividad);
            _context.SaveChanges();
            _notifyService.Success("Actividad eliminada correctamente");
            return RedirectToAction("Index", "Calendario", new { grupoId = actividad.IdGrupo });
        }


    }
}
