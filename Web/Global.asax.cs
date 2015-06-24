using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Raven.Client.Document;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ConfigureLogging();


            var documentStore = new DocumentStore { ConnectionStringName = "HomeControlDB" }.Initialize();

            AreaRegistration.RegisterAllAreas();

            ControllerBuilder.Current.SetControllerFactory(new MvcConfig(documentStore));
            GlobalConfiguration.Configure(new WebApiConfig(documentStore).Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }

        private static void ConfigureLogging()
        {
            var logs = new DocumentStore {ConnectionStringName = "SerilogRavenDB"}.Initialize();

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RavenDB(logs)
                .WriteTo.Trace()
                .CreateLogger()
                .ForContext("App", "HomeControl.Web");

            logger.Debug("The WEB application startet at {StartTime} on server {ServerName}", DateTime.Now, Environment.MachineName);

            Log.Logger = logger;
        }
    }
}
