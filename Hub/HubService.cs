using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using HomeControl.Hub.Util;
using HomeControl.Shared.Model;
using HomeControl.Shared.Model.Interfaces;
using JetBrains.Annotations;
using NetMQ;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Converters;
using Serilog;

namespace HomeControl.Hub
{
    public partial class HubService : ServiceBase
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Task _workingTask;
        private readonly ILogger _log;

        public HubService([NotNull] ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            _log = log;
        }

        protected override void OnStart(string[] args)
        {
            StartHub();
        }
        protected override void OnStop()
        {
            StopHub();
        }

        public void StartHub()
        {
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                return settings;
            });
            _workingTask = HubRunner();
        }

        private async Task HubRunner()
        {
            try
            {
                using (var context = NetMQContext.Create())
                {
                    var socket = context.CreateRouterSocket();
                    socket.Bind("tcp://*:5556");

                    while (!_tokenSource.IsCancellationRequested)
                    {
                        if (socket.HasIn)
                        {
                            //handle msg
                            var msg = socket.ReceiveMultipartMessage();
                            var replyMsgs = HandleOneMsg(msg);
                            foreach (var replyMsg in replyMsgs)
                            {
                                socket.SendMultipartMessage(replyMsg);
                            }
                        }
                        else
                        {
                            //wait
                            await Task.Delay(50);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                _log.Error(ex, "HubRunner exiting because of an unhandled exception.");
            }
        }

        private IEnumerable<NetMQMessage> HandleOneMsg(NetMQMessage mqMsg)
        {
            var senderIdentity = mqMsg.Pop().ToByteArray();
            var msgType = mqMsg.Pop().ConvertToString();
            var msgJson = mqMsg.Pop().ConvertToString();
            var sampleType = typeof (ConfigurationRequest);
            var fullTypeName = string.Format("{0}.{1}", sampleType.Namespace, msgType);
            var type = sampleType.Assembly.GetType(fullTypeName);
            if (type == null)
            {
                _log.Error("Could not find a type for {FullName} in assembly {Assembly}", fullTypeName, sampleType.Assembly.FullName);
                yield break;
            }

            var msg = JsonConvert.DeserializeObject(msgJson, type);

            _log.Verbose("Handling one message of type {MessageType}. FullMessage is {FullMessage}", msgType, msgJson);

            //hard coded handler list
            //TODO make this more dynamic
            IMessage response;
            NetMQMessage otherMessage = null;

            switch (msgType)
            {
                case nameof(ConfigurationRequest):
                    response = HandleConfigurationRequest((ConfigurationRequest) msg, senderIdentity);
                    break;

                case nameof(InputChangedTelemetry):
                    response = HandleInputChangedTelemetry((InputChangedTelemetry) msg, out otherMessage);
                    break;

                default:
                    response = null;
                    _log.Warning("Cannot handle the message type {MessageType}", msgType);
                    break;
            }

            if (otherMessage != null)
            {
                yield return otherMessage;
            }

            if (response != null)
            {
                var responseMessage = new NetMQMessage();
                responseMessage.Append(senderIdentity);
                responseMessage.Append(response.GetType().Name);
                responseMessage.Append(JsonConvert.SerializeObject(response));

                yield return responseMessage;
            }
        }

        private IMessage HandleInputChangedTelemetry(InputChangedTelemetry inputChangedTelemetry, out NetMQMessage otherMessage)
        {
            otherMessage = null;

            if (inputChangedTelemetry.Name == "Set Light"
                && inputChangedTelemetry.NewValue>=0f
                && inputChangedTelemetry.NewValue<=1f
                && _senderIdentitiesByHostname.ContainsKey("raspberrypi"))
            {
                var ledCommand = new LedOnOffSetStateCommand() {PinNumber = 12, Value = inputChangedTelemetry.NewValue};

                otherMessage = ledCommand.ToMqMessage(_senderIdentitiesByHostname["raspberrypi"]);
            }
            return null;
        }

        private readonly Dictionary<string, byte[]> _senderIdentitiesByHostname = new Dictionary<string, byte[]>(); 

        private ConfigurationResponse HandleConfigurationRequest(ConfigurationRequest configurationRequest, byte[] senderIdentity)
        {
            _senderIdentitiesByHostname[configurationRequest.Hostname] = senderIdentity;

            //hard coded for now
            //todo: add config database
            var response = new ConfigurationResponse();
            switch (configurationRequest.Hostname)
            {
                case "xenon":
                    response.GpioPins.Add(new GpioPin() {Type = GpioPinTypeEnum.PwmOut, PinNumber = 12, Frequency = 200, InitialValue = 0.8f, Name = "2D Diffuser"});
                    response.GpioPins.Add(new GpioPin() {Type = GpioPinTypeEnum.Input, PinNumber = -1, Name = "Set Light"});
                    return response;

                case "raspberrypi":
                    response.GpioPins.Add(new GpioPin() {Type = GpioPinTypeEnum.PwmOut, PinNumber = 12, Frequency = 200, InitialValue = 0.8f, Name = "2D Diffuser"});
                    return response;

                default:
                    _log.Warning("unconfigured host {Hostname} requesting a configuration!", configurationRequest.Hostname);
                    return null; //no response
            }
        }

        public void StopHub()
        {
            _tokenSource.Cancel();
            try
            {
                _workingTask?.Wait();
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _log.Error(ex, "The stopping of the hub throwed an exception");
            }
        }


    }
}
