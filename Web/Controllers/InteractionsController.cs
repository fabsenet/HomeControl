using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Compilation;
using System.Web.Http;
using HomeControl.Shared.Model;
using NetMQ;
using NetMQ.Sockets;
using Raven.Imports.Newtonsoft.Json;

namespace Web.Controllers
{
    public class InteractionsController : ApiController
    {
        private static readonly object _lockObject = new object();

        private static volatile NetMQContext _context;
        private static DealerSocket _socket;
        private static ConfigurationResponse _configuration;

        public InteractionsController()
        {
            EnsureConnection();
        }
        // GET api/<controller>
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, _configuration.GpioPins);
        }

        private void EnsureConnection()
        {
            if (_context == null)
            {
                lock (_lockObject)
                {
                    if (_context == null)
                    {
                        //thread safe init
                        var ctx = NetMQContext.Create();
                        _socket = ctx.CreateDealerSocket();
                        _socket.Connect("tcp://xenon:5556");

                        //sending configrequest
                        var mqMessage = new NetMQMessage();
                        mqMessage.Append(nameof(ConfigurationRequest));
                        mqMessage.Append(JsonConvert.SerializeObject(new ConfigurationRequest() {CreateDate = DateTime.UtcNow, Hostname = Environment.MachineName}, Formatting.None));
                        _socket.SendMultipartMessage(mqMessage);

                        //wait sync for response
                        var mqConfigResponse = _socket.ReceiveMultipartMessage();
                        Trace.Assert(mqConfigResponse[0].ConvertToString()==nameof(ConfigurationResponse), "The response is not the expected ConfigurationResponse");
                        _configuration = JsonConvert.DeserializeObject<ConfigurationResponse>(mqConfigResponse[1].ConvertToString());

                        _context = ctx;
                    }
                }
            }
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            //limit value to [0..100]
            id = Math.Max(Math.Min(100, id), 0);

            var inputChanged = new InputChangedTelemetry()
                               {
                                   CreateDate = DateTime.UtcNow,
                                   Hostname = Environment.MachineName,
                                   Name = "Set Light",
                                   PinNumber = -1,
                                   NewValue = id / 100f
                               };

            var mqMessage = new NetMQMessage();
            mqMessage.Append(nameof(InputChangedTelemetry));
            mqMessage.Append(JsonConvert.SerializeObject(inputChanged, Formatting.None));
            _socket.SendMultipartMessage(mqMessage);

            var response = _socket.ReceiveMultipartMessage();

            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}