using AspNetCoreHero.ToastNotification.Abstractions;
using Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupos.Servicios;

namespace ProyectoGrupos.Controllers
{
    public class ActividadesController : Controller
    {
        private readonly DbContext _context;
        public INotyfService _notifyService { get; }
        private readonly IEmailService _emailService;
        public ActividadesController(DbContext context, INotyfService notifyService, IEmailService emailService)
        {
            _context = context;
            _notifyService = notifyService;
            _emailService = emailService;
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
        //public IActionResult Create(Actividad actividad)
        //{
        //    var usuario = GetUserId();
        //    if (ModelState.IsValid)
        //    {
        //        actividad.FechaCreacion = DateTime.Now;
        //        actividad.CreadoPor = usuario.Result;
        //        _context.Actividades.Add(actividad);
        //        _context.SaveChanges();
        //        _notifyService.Success("Actividad creada correctamente");

        //        return RedirectToAction("Index", "Calendario", new { grupoId = actividad.IdGrupo });
        //    }
        //    ViewBag.IdGrupo = actividad.IdGrupo;
        //    _notifyService.Error("Error al crear la actividad");
        //    return View(actividad);
        //}


        public async Task<IActionResult> Create(Actividad actividad)
        {
            var usuario = GetUserId();
            if (ModelState.IsValid)
            {
                actividad.FechaCreacion = DateTime.Now;
                actividad.CreadoPor = usuario.Result;
                _context.Actividades.Add(actividad);
                await _context.SaveChangesAsync();

                // Notificación de éxito
                _notifyService.Success("Actividad creada correctamente");

                // Enviar correos a todos los miembros del grupo
                var grupo = await _context.Grupos
                                           .Include(g => g.GruposIntegrantes)
                                           .ThenInclude(gi => gi.Usuario)
                                           .FirstOrDefaultAsync(g => g.IdGrupo == actividad.IdGrupo);

                if (grupo != null)
                {
                    foreach (var integrante in grupo.GruposIntegrantes)
                    {
                        var emailModel = new EmailModel
                        {
                            To = integrante.Usuario.Email, // Asegúrate de que este sea un correo válido
                            Subject = $"¡Nueva actividad en el grupo {grupo.Nombre}!",
                            Body = $@"
                    <html>
                        <head>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    background-color: #f4f4f9;
                                    color: #333;
                                    margin: 0;
                                    padding: 0;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 0 auto;
                                    background-color: #fff;
                                    padding: 20px;
                                    border-radius: 8px;
                                    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                                }}
                                h1 {{
                                    color: #4CAF50;
                                    text-align: center;
                                }}
                                p {{
                                    font-size: 16px;
                                    line-height: 1.6;
                                }}
                                .btn {{
                                    display: inline-block;
                                    padding: 10px 20px;
                                    background-color: #4CAF50;
                                    color: #fff;
                                    text-align: center;
                                    text-decoration: none;
                                    border-radius: 4px;
                                    margin-top: 20px;
                                }}
                                .footer {{
                                    text-align: center;
                                    font-size: 12px;
                                    color: #777;
                                    margin-top: 30px;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h1>¡Nueva actividad en el grupo {grupo.Nombre}!</h1>
                                <p>Se ha creado una nueva actividad: <strong>{actividad.Titulo}</strong>.</p>
                                <p><strong>Descripción:</strong> {actividad.Descripcion}</p>
                                <p><strong>Fecha Fin de la actividad:</strong> {actividad.FechaActividad.ToString("dd MMM yyyy HH:mm")}</p>
                                <p>¡No olvides participar!</p>
                                
                                <div class='footer'>
                                    <p>Este correo fue enviado automáticamente. Si no reconoces este mensaje, por favor ignóralo.</p>
                                    <p>&copy; {DateTime.Now.Year} Tu Empresa - Todos los derechos reservados.</p>
                                </div>
                            </div>
                        </body>
                    </html>",
                            IsHtml = true
                        };

                        await _emailService.SendEmailAsync(emailModel);
                    }
                }

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
