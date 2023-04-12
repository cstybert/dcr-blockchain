using Microsoft.AspNetCore.SignalR;
 
namespace DCR
{
    public class BlockHub: Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("update", message);
        }
    }
}