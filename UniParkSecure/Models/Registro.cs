using Microsoft.AspNetCore.Identity;
using System;

namespace UniParkSecure.Models
{
    public class Registro
    {
        public int Id { get; set; }
        public string? UserId { get; set; } // Nullable for visitor records
        public Usuario? User { get; set; } // Navigation property to Usuario
        public string? DUI { get; set; }
        public string? Placa { get; set; }
        public string? FotoPath { get; set; }
        public int? SectorId { get; set; }
        public Sector? Sector { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
    }
}