using Microsoft.AspNetCore.SignalR;
 
namespace DCR
{
    public class PendingTransactionsHub: Hub
    {
        private static IHubContext<PendingTransactionsHub>? _hubContext;
        public PendingTransactionsHub(IHubContext<PendingTransactionsHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public static void SendUpdateNotification()
        {
            if (_hubContext is not null) {
                _hubContext.Clients.All.SendAsync("update");
            }
        }
    }
}