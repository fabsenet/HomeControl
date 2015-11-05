using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using NetMQ;
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
            var documentStore = new DocumentStore { ConnectionStringName = "HomeControlDB" }.Initialize();

            GlobalHost.DependencyResolver.Register(typeof (DeviceHub), () => new DeviceHub(documentStore));

            app.MapSignalR();

            Task.Factory.StartNew(() => ZeroMqTest());

            AreaRegistration.RegisterAllAreas();

            ControllerBuilder.Current.SetControllerFactory(new MvcConfig(documentStore));
            GlobalConfiguration.Configure(new WebApiConfig(documentStore).Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

      private void ZeroMqTest()
      {
            using (var context = NetMQContext.Create())
            using (var socket = context.CreateRouterSocket())
            {
                socket.Bind("tcp://*:1030");
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

                    socket.SendFrame(msg[0].Buffer, true); //identity
                    socket.SendFrame("Thanks!", true);
                    socket.SendFrame(DateTime.Now.ToString("O"));
                    Debug.WriteLine("!");
                }
            }
        }
    }
}