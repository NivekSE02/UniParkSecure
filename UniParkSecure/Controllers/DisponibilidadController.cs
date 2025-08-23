using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniParkSecure.Data;

namespace UniParkSecure.Controllers
{
    public class DisponibilidadController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisponibilidadController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sectores = await _context.Sectores.ToListAsync();
            return View(sectores);
        }
    }
}