using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniParkSecure.Models;
using UniParkSecure.Data;
using System.Threading.Tasks;

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
            var registrosQuery = _context.Registros.AsQueryable();

            // Filtrar por fecha
            if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out DateTime fechaFiltro))
            {
                registrosQuery = registrosQuery
                    .Where(r => r.FechaEntrada.Date == fechaFiltro
                             || (r.FechaSalida.HasValue && r.FechaSalida.Value.Date == fechaFiltro));
            }

            // Filtrar por hora
            if (!string.IsNullOrEmpty(hora) && TimeSpan.TryParse(hora, out TimeSpan horaFiltro))
            {
                registrosQuery = registrosQuery
                    .Where(r => r.FechaEntrada.TimeOfDay == horaFiltro
                             || (r.FechaSalida.HasValue && r.FechaSalida.Value.TimeOfDay == horaFiltro));
            }

            var registros = await registrosQuery
                .OrderByDescending(r => r.FechaEntrada)
                .ToListAsync();

            var historial = new List<dynamic>();

            foreach (var r in registros)
            {
                var user = await _userManager.FindByIdAsync(r.UserId);

                // Filtrar por usuario después de obtener el usuario
                if (!string.IsNullOrEmpty(usuario) && (user == null || !user.UserName.Contains(usuario)))
                    continue;

                historial.Add(new
                {
                    Nombre = user?.UserName ?? "N/A",
                    Apellido = "-", // ya no se busca en user
                    DUI = r.DUI,
                    Placa = r.Placa,
                    Correo = user?.Email ?? "N/A",
                    HoraEntrada = r.FechaEntrada.ToString("g"),
                    HoraSalida = r.FechaSalida?.ToString("g") ?? "-"
                });

            }

            ViewBag.Historial = historial;

            // Mantener los filtros en los inputs
            ViewData["usuario"] = usuario;
            ViewData["fecha"] = fecha;
            ViewData["hora"] = hora;

            return View();
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

       
        public IActionResult DashboardParqueos() => View();

        
        public IActionResult DashboardUsuario() => View();
    }
}
