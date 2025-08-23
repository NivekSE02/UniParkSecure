using System.ComponentModel.DataAnnotations;

namespace UniParkSecure.Models
{




    public class RegisterViewModel
    {

        [Required(ErrorMessage = "El nombre completo es requerido.")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos.")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        [RegularExpression(@".+@catolica\.edu\.sv$", ErrorMessage = "El correo debe terminar en @catolica.edu.sv.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DUI es requerido.")]
        [Display(Name = "DUI")]
        public string DUI { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es requerida.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Debes capturar un rostro.")]
        public string PlantillaFacialBase64 { get; set; } = string.Empty;

    }

}
