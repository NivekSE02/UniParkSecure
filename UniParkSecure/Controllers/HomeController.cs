 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniParkSecure.Models;
using UniParkSecure.Data;
using System.Threading.Tasks;
using System.Linq;

namespace UniParkSecure.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(SignInManager<Usuario> signInManager, UserManager<Usuario> userManager, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        // Accesibles sin login
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction("DashboardAdmin");
                return RedirectToAction("DashboardUsuario");
            }

            ModelState.AddModelError("", "Email o contraseña incorrectos");
            return View();
        }

        [AllowAnonymous]
        public IActionResult Register() => View();

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DashboardAdmin(string usuario, string fecha, string hora)
        {
            try
            {
                var registrosQuery = _context.Registros
                    .Include(r => r.User) // Include User for accessing AspNetUsers
                    .Include(r => r.Sector) // Include Sector if you need Sector.Nombre
                    .AsQueryable();

                // Filtro por fecha (rango del día) - evita uso de Date (más portable)
                if (!string.IsNullOrWhiteSpace(fecha) && DateTime.TryParse(fecha, out DateTime fechaFiltro))
                {
                    var dayStart = fechaFiltro.Date;
                    var dayEnd = dayStart.AddDays(1);
                    registrosQuery = registrosQuery.Where(r =>
                        (r.FechaEntrada >= dayStart && r.FechaEntrada < dayEnd) ||
                        (r.FechaSalida.HasValue && r.FechaSalida.Value >= dayStart && r.FechaSalida.Value < dayEnd));
                }

                // Filtro por hora (hora + minuto exactos)
                if (!string.IsNullOrWhiteSpace(hora) && TimeSpan.TryParse(hora, out TimeSpan horaFiltro))
                {
                    int h = horaFiltro.Hours;
                    int m = horaFiltro.Minutes;
                    registrosQuery = registrosQuery.Where(r =>
                        (r.FechaEntrada.Hour == h && r.FechaEntrada.Minute == m) ||
                        (r.FechaSalida.HasValue && r.FechaSalida.Value.Hour == h && r.FechaSalida.Value.Minute == m));
                }

                // Filtro por usuario (username o email) - case insensitive usando ToLower
                if (!string.IsNullOrWhiteSpace(usuario))
                {
                    var usuarioLower = usuario.ToLower();
                    registrosQuery = registrosQuery.Where(r =>
                        (r.User != null && (
                            (r.User.UserName != null && r.User.UserName.ToLower().Contains(usuarioLower)) ||
                            (r.User.Email != null && r.User.Email.ToLower().Contains(usuarioLower))
                        )));
                }

                var registros = await registrosQuery
                    .OrderByDescending(r => r.FechaEntrada)
                    .Select(r => new
                    {
                        r.Id,
                        UserId = r.UserId,
                        UserName = r.User != null ? r.User.UserName : "N/A",
                        Email = r.User != null ? r.User.Email : "N/A",
                        DUI = r.DUI ?? "N/A",
                        Placa = r.Placa ?? "N/A",
                        FotoPath = r.FotoPath ?? "N/A",
                        SectorNombre = r.Sector != null ? r.Sector.Nombre : "N/A",
                        FechaEntrada = r.FechaEntrada,
                        FechaSalida = r.FechaSalida
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var historial = registros.Select(r => new
                {
                    Nombre = r.UserName,
                    Apellido = "-", // Static placeholder as in original
                    DUI = r.DUI,
                    Placa = r.Placa,
                    Correo = r.Email,
                    HoraEntrada = r.FechaEntrada.ToString("g"),
                    HoraSalida = r.FechaSalida.HasValue ? r.FechaSalida.Value.ToString("g") : "-",
                    SectorNombre = r.SectorNombre
                }).ToList<dynamic>();

                ViewBag.Historial = historial;
                ViewData["usuario"] = usuario;
                ViewData["fecha"] = fecha;
                ViewData["hora"] = hora;

                return View();
            }
            catch (Exception ex)
            {
                // Log the error (consider using a logging framework like Serilog)
                ModelState.AddModelError("", $"Error al cargar el dashboard: {ex.Message}");
                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> GuardarMatricula(string matricula)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.Matricula = matricula;
            await _userManager.UpdateAsync(user);

            TempData["Mensaje"] = "✅ Matrícula actualizada correctamente.";
            return RedirectToAction("DashboardUsuario");
        }

        public IActionResult DashboardParqueosUsuario()
        {
            return View();
        }

        public IActionResult DashboardEntrada() => View();
        public IActionResult DashboardSalida() => View();


        [Authorize(Roles = "Admin")]
        public IActionResult DashboardCam() => View();

       
        [Authorize(Roles = "Admin")]
        public IActionResult DashboardParqueos()
        {
            var sectores = _context.Sectores.AsNoTracking().ToList();
            return View(sectores); // Vista mostrará posiciones y usará datos reales
        }

        [HttpGet]
        public IActionResult SectoresStatus()
        {
            var data = _context.Sectores.AsNoTracking()
                .Select(s => new { s.Id, s.Nombre, s.TotalEspacios, s.Disponibles })
                .ToList();
            return Json(data);
        }

        
        public IActionResult DashboardUsuario() => View();
    }
}
