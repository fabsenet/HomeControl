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
            requestContext.Complete(responseMsg);


            //not yet working
            //Task.Delay(1000).ContinueWith((_) =>
            //                              {
            //                                  _log.Information("Sending led on off command!");
            //                                  var command = new LedOnOffSetStateCommand()
            //                                                {
            //                                                    PinNumber = 18,
            //                                                    DesiredState = true
            //                                                };
            //                                  var msg = new Message(JsonConvert.SerializeObject(command))
            //                                            {
            //                                                ApplicationProperties = new ApplicationProperties()
            //                                                                        {
            //                                                                            ["CommandType"] = nameof(LedOnOffSetStateCommand)
            //                                                                        }
            //                                            };
            //                                  requestContext.Link.SendMessage(msg, new ByteBuffer(10000, true));
            //                              });
        }
    }
}