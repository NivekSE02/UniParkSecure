using Microsoft.AspNetCore.Identity;

namespace UniParkSecure.Models
{
    public class Usuario : IdentityUser
    {
        public string DUI { get; set; } = string.Empty;
        public byte[]? PlantillaFacial { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Apellidos { get; set; }
        public string? Matricula { get; set; }

    }
}
