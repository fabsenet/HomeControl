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

            SendMsgs(Clients.Caller);
            //Clients.Caller.configure("led on pin 18!");
        }

        private async Task SendMsgs(dynamic caller)
        {
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                caller.configure("led on pin 18!");
                caller.Configure("led on pin 18!");
            }
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