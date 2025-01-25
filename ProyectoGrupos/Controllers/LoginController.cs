using Entidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupos.Models;
using System.Security.Claims;

namespace ProyectoGrupos.Controllers
{
    public class LoginController : Controller
    {
        private readonly DbContext _context;
        public LoginController(DbContext context)
        {
            _context = context;
        }
        public ActionResult IniciarSesion()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }

        public async Task<bool> ValidarUsuario(LoginViewModel model)
        {
            var Usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Nombre == model.Nombre);

            if (Usuario == null)
            {
                return false;
            }
            bool verificar = BCrypt.Net.BCrypt.Verify(model.Contraseña, Usuario.Contraseña);

            return verificar;
        }
        public async Task<string> GetRol(string usuario)
        {
            var empleado = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Nombre == usuario);
            return empleado.Rol;
        }
        [HttpPost]
        public async Task<IActionResult> IniciarSesion(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    if (!await ValidarUsuario(model))
                    {

                        TempData["Error"] = "Usuario o contraseña incorrectos";
                        return RedirectToAction("Index");
                    }
                    var rol = await GetRol(model.Nombre);
                    var claims = new List<Claim>
                     {
                         new Claim(ClaimTypes.Name, model.Nombre),
                         new Claim(ClaimTypes.Role, rol)
                     };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    // _notifyService.Success($"Bienvenido {model.Usuario}");
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error al autenticar: {ex.Message}");
                }
            }

            return BadRequest(model);
        }

        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Verificar que la contraseña y la confirmación coincidan
                if (model.Contraseña != model.ConfirmarContraseña)
                {
                    ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
                    return View(model);
                }

                // Hashear la contraseña con BCrypt
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Contraseña, workFactor: 12);

                // Crear la entidad de usuario
                var usuario = new Usuario
                {
                    Nombre = model.Nombre,
                    Email = model.Email,
                    Contraseña = hashedPassword
                };

                // Guardar el usuario en la base de datos
                _context.Add(usuario);
                await _context.SaveChangesAsync();


                //_notifyService.Success("Usuario creado con éxito");
                return RedirectToAction("IniciarSesion");
            }


            return View(model);
        }
    }
}
