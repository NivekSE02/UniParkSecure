using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using UniParkSecure.Models;
using UniParkSecure.ViewModels;
using Newtonsoft.Json;

namespace UniParkSecure.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;

        public AccountController(SignInManager<Usuario> signInManager, UserManager<Usuario> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Nuevo vector recibido en JSON (desde el
            //
            //
            //
            // string)
            var nuevoVector = JsonConvert.DeserializeObject<float[]>(model.PlantillaFacialBase64);

            // Traer todos los usuarios de la DB usando UserManager
            var usuarios = _userManager.Users.ToList();

            foreach (var user in usuarios)
            {
                if (user.PlantillaFacial != null && user.PlantillaFacial.Length > 0)
                {
                    // Convertir bytes -> string -> float[]
                    var json = System.Text.Encoding.UTF8.GetString(user.PlantillaFacial);

                    // Evitar intentar deserializar basura
                    if (!json.TrimStart().StartsWith("["))
                        continue;

                    var vectorGuardado = JsonConvert.DeserializeObject<float[]>(json);

                    double distancia = CalcularDistanciaEuclidiana(nuevoVector, vectorGuardado);

                    Console.WriteLine($"Distancia entre {user.Email} y nuevo registro: {distancia}");

                    if (distancia < 0.45)
                    {
                        ModelState.AddModelError("", "❌ Los datos biométricos ya han sido registrados.");
                        return View(model);
                    }
                }
            }

            // Registrar usuario
            var newUser = new Usuario
            {
                UserName = model.Email,
                Email = model.Email,
                NombreCompleto = model.NombreCompleto,
                Apellidos = model.Apellidos,
                DUI = model.DUI,
                PlantillaFacial = System.Text.Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(nuevoVector) // Guardar SIEMPRE como JSON válido
                )
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CambiarPassword(string oldPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                TempData["Mensaje"] = "✅ Contraseña cambiada correctamente.";
                return RedirectToAction("DashboardUsuario");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View("DashboardUsuario");
        }


        // Función para comparar dos vectores de 128
        private double CalcularDistanciaEuclidiana(float[] v1, float[] v2)
        {
            double suma = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                double diff = v1[i] - v2[i];
                suma += diff * diff;
            }
            return Math.Sqrt(suma);
        }
    }
}
