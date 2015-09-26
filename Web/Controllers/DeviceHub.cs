using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeControl.Shared.Contract;
using HomeControl.Shared.Model;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Raven.Client;
using Web.Models;

namespace Web.Controllers
{
    public class DeviceHub : Hub<IDeviceHubClient>, IDeviceHubServer
    {
        private readonly IDocumentStore _documentStore;

        private class DeviceConnection
        {
            public string DeviceName { get; set; }
            public string ConnectionId { get; set; }
        }
        private static readonly List<DeviceConnection> _connectedDevices = new List<DeviceConnection>();

        public static string GetConnectionIdForDeviceName(string deviceName)
        {
            return _connectedDevices.Where(cd => cd.DeviceName == deviceName)
                .Select(cd => cd.ConnectionId)
                .FirstOrDefault();
        }

        private void AddDevice(string deviceName, string connectionId)
        {
            _connectedDevices.RemoveAll(dc => dc.DeviceName == deviceName || dc.ConnectionId == connectionId);
            _connectedDevices.Add(new DeviceConnection() {DeviceName = deviceName, ConnectionId = connectionId});

            SetOnline(deviceName);
        }

        private void RemoveDevice(string connectionId)
        {
            var device = _connectedDevices.FirstOrDefault(dc => dc.ConnectionId == connectionId);
            if (device != null)
            {
                SetOffline(device.DeviceName);
            }
            _connectedDevices.RemoveAll(dc => dc.ConnectionId == connectionId);
        }

        private void SetOnline(string deviceName)
        {
            using (var session = _documentStore.OpenSession())
            {
                var pingId = "DeviceConfigs/" + deviceName;
                var ping = session.Load<DeviceConfig>(pingId);
                if (ping == null)
                {
                    ping = new DeviceConfig { Id = pingId, Hostname = deviceName };
                    session.Store(ping);
                }

                ping.LastOnlineTime = DateTime.UtcNow;
                ping.IsCurrentlyOnline = true;
                session.SaveChanges();
            }
        }

        private void SetOffline(string deviceName)
        {
            using (var session = _documentStore.OpenSession())
            {
                var pingId = "DeviceConfigs/" + deviceName;
                var ping = session.Load<DeviceConfig>(pingId);

                if (ping == null) return;
                
                ping.LastOnlineTime = DateTime.UtcNow - TimeSpan.FromSeconds(30);
                ping.IsCurrentlyOnline = false;
                session.SaveChanges();
            }

        }

        public DeviceHub([NotNull] IDocumentStore documentStore)
        {
            if (documentStore == null) throw new ArgumentNullException(nameof(documentStore));
            _documentStore = documentStore;
        }

        [UsedImplicitly]
        public void Hello(string deviceConfigRequestJson)
        {
            var request = JsonConvert.DeserializeObject<DeviceConfigRequest>(deviceConfigRequestJson);
            Debug.WriteLine("received hello from device: "+request.DeviceName);

            //todo: read actual config here
            var deviceConfigResponse = new DeviceConfigResponse();
            deviceConfigResponse.LedOnOffSwitches.Add(new LedOnOffSwitchConfiguration() {InitialState = false,PinNumber = 18});
            deviceConfigResponse.ApplicationlevelPingTimeSpan = TimeSpan.FromSeconds(7);

            AddDevice(request.DeviceName, Context.ConnectionId);
            Clients.Caller.Configure(JsonConvert.SerializeObject(deviceConfigResponse));
        }

        [UsedImplicitly]
        public void PingResponse(string pingResponseCommand)
        {
            var command = JsonConvert.DeserializeObject<PingResponse>(pingResponseCommand);
            SetOnline(command.HostName);
        }

        public override Task OnConnected()
        {
            Debug.WriteLine("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            RemoveDevice(Context.ConnectionId);
            Debug.WriteLine(string.Format("Client disconnected: {0}, stop was called: {1}", Context.ConnectionId, stopCalled));
            return base.OnDisconnected(stopCalled);
        }
    }
}