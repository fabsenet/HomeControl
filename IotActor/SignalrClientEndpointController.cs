using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Core;
using HomeControl.Shared.Model;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;
using Newtonsoft.Json;

namespace IotActor
{
    public class SignalrClientEndpointController
    {
        private const string DeviceConfigReplyTo = "device_config_reply";
        private readonly ObservableCollection<LedOnOffSwitch> _ledOnOffSwitches;
        private readonly CoreDispatcher _dispatcher;

        private readonly CancellationTokenSource _token = new CancellationTokenSource();
        private readonly Dictionary<int, LedOnOffSwitch> _leds = new Dictionary<int, LedOnOffSwitch>();
        private readonly IHubProxy _hubProxy;
        private readonly HubConnection _hubConnection;

        public SignalrClientEndpointController(ObservableCollection<LedOnOffSwitch> ledOnOffSwitches, CoreDispatcher dispatcher)
        {
            _ledOnOffSwitches = ledOnOffSwitches;
            _dispatcher = dispatcher;

            _hubConnection = new HubConnection(Config.SignalrHubUrl);
            _hubProxy = _hubConnection.CreateHubProxy("DeviceHub");

            _hubConnection.Received += msg => Debug.WriteLine("Received from HubConnection: " + msg);

            _hubProxy.On<string>("Configure", (message) => Debug.WriteLine(String.Format("MsgName: {0}, Message: {1}", null, message)));
            _hubConnection.Start().Wait();

            RequestDeviceConfiguration();
        }

        private void RequestDeviceConfiguration()
        {
            var deviceName = DeviceName;
            var deviceConfigRequest = new DeviceConfigRequest() {DeviceName = deviceName};

            _hubProxy.Invoke("Hello", JsonConvert.SerializeObject(deviceConfigRequest)).Wait();

        }

        private static string DeviceName
        {
            get
            {
                var device = new EasClientDeviceInformation();
                var deviceName = device.FriendlyName;
                return deviceName;
            }
        }

        //private void OnDeviceConfigReceive(ReceiverLink receiver, Message message)
        //{
        //    Debug.WriteLine("Received device config response: " + message.Body);
        //    var deviceConfigResponse = JsonConvert.DeserializeObject<DeviceConfigResponse>((string) message.Body);

        //    if (deviceConfigResponse.LedOnOffSwitches != null)
        //    {
        //        foreach (var ledOnOffSwitchConfiguration in deviceConfigResponse.LedOnOffSwitches)
        //        {
        //            var led = new LedOnOffSwitch(ledOnOffSwitchConfiguration.PinNumber, _token.Token, _dispatcher);
        //            led.SetState(ledOnOffSwitchConfiguration.InitialState);
        //            _leds[ledOnOffSwitchConfiguration.PinNumber] = led;
        //            var result = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _ledOnOffSwitches.Add(led));
        //        }
        //    }
        //}
        
    }

    class MyHttpClient : DefaultHttpClient
    {
        protected override HttpMessageHandler CreateHandler()
        {
            var handler = (DefaultHttpHandler)base.CreateHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            return handler;
        }
    }

}