using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniParkSecure.Data;
using UniParkSecure.Models;

namespace UniParkSecure.Controllers
{
    public class RegistrosController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ApplicationDbContext _context;

        public RegistrosController(UserManager<Usuario> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalida([FromBody] EntradaRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var registro = await _context.Registros
                .Where(r => r.UserId == user.Id && r.FechaSalida == null)
                .OrderByDescending(r => r.FechaEntrada)
                .FirstOrDefaultAsync();

            if (registro == null)
                return NotFound(new { mensaje = "No hay registro activo para este usuario" });

            registro.FechaSalida = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"✅ Salida registrada para {user.Email}" });
        }


        
        public async Task<IActionResult> Historial()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var registros = await _context.Registros
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.FechaEntrada)
                .ToListAsync();

            return View(registros);
        }





        [HttpPost]
        public async Task<IActionResult> CreateEntrada([FromBody] EntradaRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            // Evitar múltiples entradas activas
            var registroExistente = await _context.Registros
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.FechaSalida == null);

            if (registroExistente != null)
            {
                // Redirigir si ya existe un registro activo
                return Ok(new
                {
                    registroId = registroExistente.Id,
                    mensaje = "Usuario ya tiene una entrada activa",
                    redirect = Url.Action("ElegirSector", "Registros", new { email = user.Email })
                });
            }

            var registro = new Registro
            {
                UserId = user.Id,
                FechaEntrada = DateTime.Now,
                DUI = user.DUI,
                Placa = "N/A",
                FotoPath = "N/A"
            };

            _context.Registros.Add(registro);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                registroId = registro.Id,
                mensaje = "Nueva entrada creada",
                redirect = Url.Action("ElegirSector", "Registros", new { email = user.Email })
            });
        }

        [HttpGet]
        public IActionResult ElegirSector(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarSector([FromBody] SectorRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var registro = await _context.Registros
                .Where(r => r.UserId == user.Id && r.FechaSalida == null)
                .OrderByDescending(r => r.FechaEntrada)
                .FirstOrDefaultAsync();

            if (registro == null)
                return NotFound(new { mensaje = "Registro no encontrado" });

            registro.SectorId = request.SectorId;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Bienvenido! Usted se dirige a: {request.SectorId}" });
        }
    }

    public class EntradaRequest
    {
        public string Email { get; set; }
    }

    public class SectorRequest
    {
        public string Email { get; set; }
        public int SectorId { get; set; }
    }
}
