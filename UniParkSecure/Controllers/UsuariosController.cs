using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using UniParkSecure.Models;

namespace UniParkSecure.Controllers
{
    [Route("[controller]/[action]")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> _userManager;

        public UsuariosController(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult GetEmbeddings()
        {
            var usuarios = _userManager.Users
                .AsEnumerable()
                .Where(u => u.PlantillaFacial != null && u.PlantillaFacial.Length > 0)
                .Select(u => new
                {
                    nombre = u.Email,
                    embedding = JsonConvert.DeserializeObject<float[]>(Encoding.UTF8.GetString(u.PlantillaFacial))
                });

            return Json(usuarios);
        }



    }
}
