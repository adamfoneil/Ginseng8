using Ginseng.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
    public class AppNotification : Hub
    {
        public async Task Broadcast(Notification notification)
        {
            //await Clients.All.SendAsync()
        }
    }
}
