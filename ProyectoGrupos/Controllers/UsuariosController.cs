using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGrupos.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly DbContext _context;

        public UsuariosController(DbContext context)
        {
            _context = context;
        }

        // GET: Usuario
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());
        }

        [HttpPost]
        public IActionResult UpdateRole([FromBody] UpdateRoleRequest request)
        {
            var user = _context.Usuarios.Find(request.UserId);
            if (user != null)
            {
                user.Rol = request.NewRole;
                _context.SaveChanges();

                return Ok();
            }
            return NotFound();
        }

        public class UpdateRoleRequest
        {
            public int UserId { get; set; }
            public string NewRole { get; set; }
        }

    }
}
