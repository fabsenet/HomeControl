using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
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



            AreaRegistration.RegisterAllAreas();

            ControllerBuilder.Current.SetControllerFactory(new MvcConfig(documentStore));
            GlobalConfiguration.Configure(new WebApiConfig(documentStore).Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}