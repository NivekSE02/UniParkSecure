using Emgu.CV.Ocl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using UniParkSecure.Data;
using UniParkSecure.Models;

namespace UniParkSecure.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<Usuario> _userManager;

        public AdminController(UserManager<Usuario> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> GuardarRostro([FromBody] EmbeddingRequest request)
        {
            if (request.Embedding == null || request.Embedding.Length == 0)
                return BadRequest("No se recibió embedding");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Guardar embedding como JSON string en bytes
            user.PlantillaFacial = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request.Embedding));
            await _userManager.UpdateAsync(user);

            return Ok(new { success = true, mensaje = "Rostro guardado" });
        }

        [HttpPost]
        public IActionResult VerificarRostro([FromBody] EmbeddingRequest request)
        {
            if (request.Embedding == null || request.Embedding.Length == 0)
                return BadRequest("No se recibió embedding");

            var usuarios = _userManager.Users.Where(u => u.PlantillaFacial != null).ToList();

            foreach (var u in usuarios)
            {
                var existing = JsonSerializer.Deserialize<float[]>(System.Text.Encoding.UTF8.GetString(u.PlantillaFacial));
                if (existing != null && CompararEmbeddings(existing, request.Embedding))
                    return Ok(new { existe = true, nombre = u.NombreCompleto });
            }

            return Ok(new { existe = false });
        }

        private bool CompararEmbeddings(float[] a, float[] b)
        {
            if (a.Length != b.Length) return false;
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += Math.Pow(a[i] - b[i], 2);

            double distancia = Math.Sqrt(sum);
            return distancia < 0.6; // umbral típico face-api.js
        }

        public class EmbeddingRequest
        {
            public float[]? Embedding { get; set; }

        }

        [HttpGet]
        public IActionResult GetEmbeddings()
        {
            var usuarios = _dbContext.Usuarios
                .Where(u => u.PlantillaFacial != null) // Esto sí es translatable a SQL
                .AsEnumerable() // Desde aquí todo será LINQ en memoria
                .Where(u => u.PlantillaFacial.Length > 0) // Ahora sí puedes usar Length
                .Select(u =>
                {
                    string json = Encoding.UTF8.GetString(u.PlantillaFacial ?? Array.Empty<byte>());

                    if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("["))
                        return null;

                    float[] embedding;
                    try
                    {
                        embedding = JsonSerializer.Deserialize<float[]>(json);
                    }
                    catch
                    {
                        return null;
                    }

                    return new
                    {
                        nombre = u.Email,
                        embedding = embedding
                    };
                })
                .Where(x => x != null)
                .ToList();

            return Json(usuarios);
        }


        public IActionResult DashboardCam() => View();
    }
}
