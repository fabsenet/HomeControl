using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using JetBrains.Annotations;
using Microsoft.Owin;
using NetMQ;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;
using Raven.Client.Document;
using Web;
using Web.Controllers;

[assembly: OwinStartup(typeof(Startup))] 
namespace Web
{
  public class Startup
    {
        [UsedImplicitly]
        public void Configuration(IAppBuilder app)
        {
            //app.MapSignalR();
            var documentStore = new DocumentStore { ConnectionStringName = "HomeControlDB" }.Initialize();

            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                return settings;
            });
            //       Task.Factory.StartNew(() => ZeroMqTest());

            AreaRegistration.RegisterAllAreas();

            ControllerBuilder.Current.SetControllerFactory(new MvcConfig(documentStore));
            GlobalConfiguration.Configure(new WebApiConfig(documentStore).Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /*
              private async Task ZeroMqTest()
              {
                    using (var context = NetMQContext.Create())
                    using (var socket = context.CreateRouterSocket())
                    {
                        socket.Bind("tcp://*:5556");
                        while (true)
                        {
                            var msg = socket.ReceiveMultipartMessage();

                            Debug.WriteLine("Received msg with {0} frames", msg.FrameCount);

                            for (int i = 0; i < msg.FrameCount; i++)
                            {
                                if (i == 0)
                                {
                                    Debug.WriteLine("identity: " + Convert.ToBase64String(msg[i].Buffer));
                                }
                                else
                                {
                                    Debug.WriteLine("frame "+i+": " + msg[i].ConvertToString());
                                }
                            }
                            Debug.WriteLine(".");

                            var identity = msg[0].Buffer;

                            var configMsg = new NetMQMessage();
                            configMsg.Append(identity);
                            configMsg.Append("ConfigurationResponse");
                            configMsg.Append(JsonConvert.SerializeObject(new
                                                                         {
                                                                             GpioPins = new object[]
                                                                                        {
                                                                                            new
                                                                                            {
                                                                                                PinNumber= 12,
                                                                                                Type= "PwmOut",
                                                                                                InitialValue= 0.3,
                                                                                            }
                                                                                        }
                                                                         }));
                            socket.SendMultipartMessage(configMsg);

                            await Task.Delay(TimeSpan.FromSeconds(1));

                            var setPinValue = new NetMQMessage();
                            setPinValue.Append(identity);
                            setPinValue.Append("SetPinValue");
                            setPinValue.Append(JsonConvert.SerializeObject(new
                                                                           {
                                                                               PinNumber = 12,
                                                                               Value = 0.8,
                                                                           }));
                            socket.SendMultipartMessage(setPinValue);

                            await Task.Delay(TimeSpan.FromSeconds(1));

                            setPinValue = new NetMQMessage();
                            setPinValue.Append(identity);
                            setPinValue.Append("SetPinValue");
                            setPinValue.Append(JsonConvert.SerializeObject(new
                                                                           {
                                                                               PinNumber = 12,
                                                                               Value = 0,
                                                                           }));
                            socket.SendMultipartMessage(setPinValue);

                            await Task.Delay(TimeSpan.FromSeconds(1));

                            setPinValue = new NetMQMessage();
                            setPinValue.Append(identity);
                            setPinValue.Append("SetPinValue");
                            setPinValue.Append(JsonConvert.SerializeObject(new
                                                                           {
                                                                               PinNumber = 12,
                                                                               Value = 1,
                                                                           }));
                            socket.SendMultipartMessage(setPinValue);

                            Debug.WriteLine(DateTime.Now);
                        }
                    }
                }
        */
    }
}