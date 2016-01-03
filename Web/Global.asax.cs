using System;
using System.Web;
using Raven.Client.Document;
using Serilog;
using Serilog.Events;

namespace Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ConfigureLogging();
        }

        private static void ConfigureLogging()
        {
            var logs = new DocumentStore {ConnectionStringName = "SerilogRavenDB"}.Initialize();

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RavenDB(logs, LogEventLevel.Warning, expiration: TimeSpan.FromDays(7))
                .WriteTo.Trace()
                .CreateLogger()
                .ForContext("App", "HomeControl.Web");

            logger.Debug("The WEB application startet at {StartTime} on server {ServerName}", DateTime.Now, Environment.MachineName);

            Log.Logger = logger;
        }
    }
}
