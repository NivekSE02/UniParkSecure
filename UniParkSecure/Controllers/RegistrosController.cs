using Microsoft.AspNetCore.Mvc;
using UniParkSecure.Data;
using UniParkSecure.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UniParkSecure.Controllers
{
    public class RegistrosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Historial()
        {
            // Si no está autenticado redirigir a login
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Home");

            var email = User.Identity!.Name;
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Login", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return RedirectToAction("Login", "Home");

            var registros = await _context.Registros
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.FechaEntrada)
                .ToListAsync();

            return View(registros);
        }

        // 📌 Vista para elegir sector (se llama desde DashboardCam)
        [HttpGet]
        public IActionResult ElegirSector(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("DashboardCam", "Home");

            ViewBag.Email = email;
            return View();
        }

        // 📌 Crear entrada directa (ya incluye sector)
        [HttpPost]
        public IActionResult CreateEntrada([FromBody] EntradaRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || request.SectorId == 0)
                return Json(new { mensaje = "Datos inválidos" });

            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return Json(new { mensaje = "Usuario no encontrado" });

            var registroExistente = _context.Registros
                .FirstOrDefault(r => r.UserId == user.Id && r.FechaSalida == null);
            if (registroExistente != null)
                return Json(new { mensaje = "Ya existe una entrada activa" });

            var sector = _context.Sectores.FirstOrDefault(s => s.Id == request.SectorId);
            if (sector == null)
                return Json(new { mensaje = "Sector no válido" });
            if (sector.Disponibles <= 0)
                return Json(new { mensaje = "No hay espacios disponibles en el sector" });

            // Crear registro incluyendo DUI (antes no se asignaba y quedaba null)
            var registro = new Registro
            {
                UserId = user.Id,
                DUI = user.DUI, // Copiamos el DUI del usuario
                FechaEntrada = DateTime.Now,
                FechaSalida = null,
                SectorId = request.SectorId
            };

            // Actualizar disponibilidad localmente (antes era trigger)
            sector.Disponibles -= 1;

            _context.Registros.Add(registro);
            _context.SaveChanges();

            return Json(new { mensaje = "Entrada registrada con éxito" });
        }

        // 📌 Registrar salida
        [HttpPost]
        public IActionResult CreateSalida([FromBody] SalidaRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
                return Json(new { mensaje = "Datos inválidos" });

            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return Json(new { mensaje = "Usuario no encontrado" });

            var registro = _context.Registros
                .FirstOrDefault(r => r.UserId == user.Id && r.FechaSalida == null);
            if (registro == null)
                return Json(new { mensaje = "No hay registro de entrada activo" });

            registro.FechaSalida = DateTime.Now;

            // Incrementar cupo disponible (antes trigger de salida)
            if (registro.SectorId.HasValue)
            {
                var sector = _context.Sectores.FirstOrDefault(s => s.Id == registro.SectorId.Value);
                if (sector != null)
                {
                    sector.Disponibles += 1;
                }
            }

            _context.SaveChanges();

            return Json(new { mensaje = "Salida registrada con éxito" });
        }
    }

    // DTOs para requests
    public class EntradaRequest
    {
        public string Email { get; set; }
        public int SectorId { get; set; }
    }

    public class SalidaRequest
    {
        public string Email { get; set; }
    }
}
