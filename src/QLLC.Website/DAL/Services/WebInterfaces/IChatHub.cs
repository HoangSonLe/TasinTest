using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IChatHub
    {
        Task ForceLogout(string message);
        Task BroadcastMessage(string name, string message);

    }

    [Authorize]
    public class ChatHub : Hub<IChatHub>
    {

        public void Send(string name, string message)
        {
            Clients.All.BroadcastMessage(name, message);
        }
    }
}
