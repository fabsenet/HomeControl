using System;
using System.Diagnostics;
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
            Debug.WriteLine("received hello: "+deviceName);

            Clients.Caller.Configure("led on pin 18!");
        }

        public override Task OnConnected()
        {
            Debug.WriteLine("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            Debug.WriteLine(string.Format("Client disconnected: {0}, stop was called: {1}", Context.ConnectionId, stopCalled));
            return base.OnDisconnected(stopCalled);
        }
    }
}