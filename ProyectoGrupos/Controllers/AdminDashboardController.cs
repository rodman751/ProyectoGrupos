using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupos.Models;

namespace ProyectoGrupos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly DbContext _context;

        public AdminDashboardController(DbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Dashboard statistics
            var dashboardStats = new DashboardViewModel
            {
                TotalUsers = await _context.Usuarios.CountAsync(),
                TotalGroups = await _context.Grupos.CountAsync(),
                TotalActivities = await _context.Actividades.CountAsync(),

                // Group distribution by state
                GroupStateDistribution = await _context.Grupos
                    .GroupBy(g => g.Estado)
                    .Select(g => new GroupStateDistribution
                    {
                        Estado = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync(),

                // User roles distribution
                UserRoleDistribution = await _context.Usuarios
                    .GroupBy(u => u.Rol)
                    .Select(u => new UserRoleDistribution
                    {
                        Rol = u.Key,
                        Count = u.Count()
                    })
                    .ToListAsync(),

                // Recent activities
                RecentActivities = await _context.Actividades
                    .OrderByDescending(a => a.FechaCreacion)
                    .Take(10)
                    .ToListAsync(),

                // Group member trends
                GroupMemberTrends = await _context.Grupos
                    .Select(g => new GroupMemberTrend
                    {
                        NombreGrupo = g.Nombre,
                        NumeroIntegrantes = g.NumeroActualIntegrantes
                    })
                    .ToListAsync()
            };

            return View(dashboardStats);
        }


        // Table data views
        public async Task<IActionResult> TableView(string tableName)
        {
            dynamic tableData = tableName switch
            {
                "Usuarios" => await _context.Usuarios.ToListAsync(),
                "Grupos" => await _context.Grupos.ToListAsync(),
                "Actividades" => await _context.Actividades.ToListAsync(),
                "GruposIntegrantes" => await _context.GruposIntegrantes.ToListAsync(),
                _ => null
            };

            ViewBag.TableName = tableName;
            return View(tableData);
        }
    }
}
