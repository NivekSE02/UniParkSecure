using Microsoft.AspNetCore.SignalR;

namespace UniParkSecure.Hubs
{
    public class ParqueoHub : Hub
    {
        public async Task ActualizarDisponibilidad(string sector, int disponibles)
        {
            await Clients.All.SendAsync("ActualizarDisponibilidad", sector, disponibles);
        }
    }
}