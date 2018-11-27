using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SE2Final
{
    public class Server : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}