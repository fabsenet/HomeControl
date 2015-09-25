using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Core;
using HomeControl.Shared.Model;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;
using Newtonsoft.Json;
using Polly;

namespace IotActor
{
    public class SignalrClientEndpointController
    {
        private readonly ObservableCollection<LedOnOffSwitch> _ledOnOffSwitches;
        private readonly CoreDispatcher _dispatcher;

        private readonly CancellationTokenSource _token = new CancellationTokenSource();
        private readonly Dictionary<int, LedOnOffSwitch> _leds = new Dictionary<int, LedOnOffSwitch>();
        private readonly IHubProxy _hubProxy;
        private readonly HubConnection _hubConnection;

        private readonly Policy _policy = Policy.Handle<Exception>().RetryForever(ex => Task.Delay(1000).Wait());

        public SignalrClientEndpointController(ObservableCollection<LedOnOffSwitch> ledOnOffSwitches, CoreDispatcher dispatcher)
        {
            _ledOnOffSwitches = ledOnOffSwitches;
            _dispatcher = dispatcher;

            _hubConnection = new HubConnection(Config.SignalrHubUrl);

            if (!string.IsNullOrEmpty(Config.SignalrHubUserName))
            {
                _hubConnection.Credentials = new NetworkCredential(Config.SignalrHubUserName, Config.SignalrHubPassword);
            }

            //endlessly try to reconnect
            _hubConnection.Closed += () => _policy.Execute(() => _hubConnection.Start().Wait());

            _hubProxy = _hubConnection.CreateHubProxy("DeviceHub");

            _hubConnection.Received += msg => Debug.WriteLine("Received from HubConnection: " + msg);

            _hubProxy.On<string>("Configure", message => OnDeviceConfigReceive(JsonConvert.DeserializeObject<DeviceConfigResponse>(message)));
            _hubProxy.On<string>("LedOnOffSetStateCommand", message => OnLedOnOffSetStateCommand(JsonConvert.DeserializeObject<LedOnOffSetStateCommand>(message)));

            _policy.Execute(() => _hubConnection.Start().Wait());

            RequestDeviceConfiguration();
        }

        private void OnLedOnOffSetStateCommand(LedOnOffSetStateCommand ledOnOffSetStateCommand)
        {
            LedOnOffSwitch led;
            if (_leds.TryGetValue(ledOnOffSetStateCommand.PinNumber, out led))
            {
                led.SetState(ledOnOffSetStateCommand.DesiredState);
            }
        }

        private void RequestDeviceConfiguration()
        {
            var deviceName = DeviceName;
            var deviceConfigRequest = new DeviceConfigRequest() {DeviceName = deviceName};

            _hubProxy.Invoke("Hello", JsonConvert.SerializeObject(deviceConfigRequest)).Wait();

        }

        private string DeviceName
        {
            get
            {
                var device = new EasClientDeviceInformation();
                var deviceName = device.FriendlyName;
                return deviceName;
            }
        }

        private CancellationTokenSource _tokenSource;
        private Task _pingTask;

        private void OnDeviceConfigReceive(DeviceConfigResponse deviceConfigResponse)
        {
            Debug.WriteLine("Received device config response: " + deviceConfigResponse);

            if (_tokenSource != null)
            {
                //there was an earlier config, rewind it
                _tokenSource.Cancel();
            }
            _tokenSource = new CancellationTokenSource();

            if (deviceConfigResponse.LedOnOffSwitches != null)
            {
                foreach (var ledOnOffSwitchConfiguration in deviceConfigResponse.LedOnOffSwitches)
                {
                    var led = new LedOnOffSwitch(ledOnOffSwitchConfiguration.PinNumber, _token.Token, _dispatcher);
                    led.SetState(ledOnOffSwitchConfiguration.InitialState);
                    _leds[ledOnOffSwitchConfiguration.PinNumber] = led;
                    var result = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _ledOnOffSwitches.Add(led));
                }
            }
            if (deviceConfigResponse.ApplicationlevelPingTimeSpan != null)
            {
                _pingTask = PingContiniously(deviceConfigResponse.ApplicationlevelPingTimeSpan.Value, _tokenSource.Token);
            }
        }

        private async Task PingContiniously(TimeSpan interval, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(interval, token);

                _policy.Execute(() => SendOnePing().Wait(token));
            }
        }

        private async Task SendOnePing()
        {
            try
            {
                var pingResponse = new PingResponse()
                                            {
                                                HostName = DeviceName,
                                                PingTime = DateTime.UtcNow
                                            };

                await _hubProxy.Invoke("PingResponse", JsonConvert.SerializeObject(pingResponse));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ping failed: "+ex);
                throw;
            }
        }
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