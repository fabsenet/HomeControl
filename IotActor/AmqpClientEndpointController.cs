using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Core;
using Amqp;
using Amqp.Framing;
using HomeControl.Shared.Model;
using Newtonsoft.Json;

namespace IotActor
{
    public class AmqpClientEndpointController
    {
        private const string DeviceConfigReplyTo = "device_config_reply";
        private readonly ObservableCollection<LedOnOffSwitch> _ledOnOffSwitches;
        private readonly CoreDispatcher _dispatcher;
        readonly Connection _connection;
        readonly Session _session;
        readonly SenderLink _deviceConfigSender;
        readonly ReceiverLink _dataReceiver;

        private readonly CancellationTokenSource _token = new CancellationTokenSource();
        private readonly Dictionary<int, LedOnOffSwitch> _leds = new Dictionary<int, LedOnOffSwitch>();
        private ReceiverLink _deviceConfigReceiver;

        public AmqpClientEndpointController(ObservableCollection<LedOnOffSwitch> ledOnOffSwitches, CoreDispatcher dispatcher)
        {
            _ledOnOffSwitches = ledOnOffSwitches;
            _dispatcher = dispatcher;
            _connection = new Connection(new Address(Config.AmqpHostUri));
            _session = new Session(_connection);

            var attach = new Attach()
                         {
                             Source = new Source() { Address = "device_config"},
                             Target = new Target() { Address = DeviceConfigReplyTo }
                         };

            _deviceConfigReceiver = new ReceiverLink(_session, "client-config-recv-link", attach, null);
            _deviceConfigReceiver.Start(50, OnDeviceConfigReceive);

            _deviceConfigSender = new SenderLink(_session, "client-config-send-link", "device_config");

            _dataReceiver = new ReceiverLink(_session, "client-command-recv-link", "command");
            _dataReceiver.Start(50, OnCommandMessage);

            RequestDeviceConfiguration();
        }

        private void RequestDeviceConfiguration()
        {
            var device = new EasClientDeviceInformation();
            var deviceName = device.FriendlyName;
            var deviceConfigRequest = new DeviceConfigRequest() {DeviceName = deviceName};

            var msg = new Message(JsonConvert.SerializeObject(deviceConfigRequest))
                      {
                          Properties = new Properties() {ReplyTo = DeviceConfigReplyTo}
                      };
            _deviceConfigSender.Send(msg);
        }

        private void OnDeviceConfigReceive(ReceiverLink receiver, Message message)
        {
            Debug.WriteLine("Received device config response: " + message.Body);
            var deviceConfigResponse = JsonConvert.DeserializeObject<DeviceConfigResponse>((string) message.Body);

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
        }

        private void OnCommandMessage(ReceiverLink receiverLink, Message message)
        {
            var commandTypeName = message.ApplicationProperties["CommandType"] as string;
            switch (commandTypeName)
            {
                case nameof(LedOnOffSetStateCommand):
                    var ledOnOffSetStateCommand = JsonConvert.DeserializeObject<LedOnOffSetStateCommand>((string) message.Body);
                    _leds[ledOnOffSetStateCommand.PinNumber].SetState(ledOnOffSetStateCommand.DesiredState);
                    break;

                default:
                    throw new Exception("No idea how to handle command type '"+commandTypeName+"'");
            }
            var shouldBeOn = (string) message.ApplicationProperties["Light"] == "on";
            //_led.SetState(shouldBeOn);
        }
    }

}