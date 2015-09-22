using System;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;

namespace Web.Controllers
{
    public class DeviceHub : Hub
    {
        [UsedImplicitly]
        public void Hello(string deviceName)
        {
            Clients.Caller.configure("led on pin 18!");
        }
    }
}