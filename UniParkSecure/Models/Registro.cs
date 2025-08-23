namespace UniParkSecure.Models
{
    public class Registro
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int SectorId { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string Placa { get; set; }
        public string DUI { get; set; }
        public string FotoPath { get; set; }
    }
}