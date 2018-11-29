using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SE2Final.Classes;
using System.Collections.Generic;

namespace SE2Final
{
    public class Server : Hub
    {

        private List<DocFile> content;

        public async Task SendMessage(string user, string message)
        {
            //  Call database
            //  Build JSON 
            //  Send JSON
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        //  Create a leave methods that clears all locks
    }
}