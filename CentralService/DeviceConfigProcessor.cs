using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using HomeControl.Shared.Model;
using Newtonsoft.Json;
using Serilog;

namespace HomeControl.CentralService
{
    class DeviceConfigProcessor : IRequestProcessor
    {
        private readonly ILogger _log;
        public DeviceConfigProcessor()
        {
            _log = Log.ForContext<DeviceConfigProcessor>();
        }

        public void Process(RequestContext requestContext)
        {
            _log.Verbose("Received message {@message}", requestContext.Message);

            var request = JsonConvert.DeserializeObject<DeviceConfigRequest>(requestContext.Message.Body as string);

            var deviceName = request.DeviceName;
            //get config here based on device name

            var response = new DeviceConfigResponse()
                           {
                               LedOnOffSwitches = new List<LedOnOffSwitchConfiguration>()
                                                  {
                                                      new LedOnOffSwitchConfiguration()
                                                      {
                                                          PinNumber = 11,
                                                          InitialState = false
                                                      },
                                                      new LedOnOffSwitchConfiguration()
                                                      {
                                                          PinNumber = 18,
                                                          InitialState = false
                                                      },
                                                      new LedOnOffSwitchConfiguration()
                                                      {
                                                          PinNumber = 13,
                                                          InitialState = true
                                                      },
                                                  }
                           };

            Message responseMsg = new Message(JsonConvert.SerializeObject(response));
            _log.Verbose("Replying with {@message}", responseMsg);

            var link = requestContext.Link;

            //requestContext.Complete(responseMsg);
            link.SendMessage(responseMsg, null);

            Task.Delay(1000).ContinueWith((_) =>
                                          {
                                              for (int i = 0; i < 200; i++)
                                              {
                                                  Task.Delay(1000).Wait();
                                                  _log.Information("Sending led on off command! Number {number}", i);
                                                  var command = new LedOnOffSetStateCommand()
                                                                {
                                                                    PinNumber = 18,
                                                                    DesiredState = i%2==0
                                                                };
                                                  var commandJson = JsonConvert.SerializeObject(command);

                                                  var msg = new Message(commandJson)
                                                            {
                                                                ApplicationProperties = new ApplicationProperties()
                                                                                        {
                                                                                            ["CommandType"] = nameof(LedOnOffSetStateCommand)
                                                                                        }
                                                            };

                                                  link.SendMessage(msg, null); //<== throws AmqpException: Operation is not valid due to the current state of the object.
                                              }
                                          });
        }
    }
}