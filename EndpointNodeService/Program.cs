using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Document;
using Serilog;

namespace EndpointNodeService
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main(string[] args)
        {
            var isRunningAsService = args.Any(a => a == "/s");

            ConfigureLogger();
                var service = new EndpointCommunicationClientService();
            if (isRunningAsService)
            {
                var ServicesToRun = new ServiceBase[]
                {
                    service
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                service.Run();
            }
        }

        private static void ConfigureLogger()
        {
            var logs = new DocumentStore { ConnectionStringName = "SerilogRavenDB" }.Initialize();

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RavenDB(logs)
                .WriteTo.Trace()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("App", "HomeControl.EndpointNodeService")
                .CreateLogger();

            logger.Debug("The WEB application startet at {StartTime} on server {ServerName}", DateTime.Now, Environment.MachineName);

            Log.Logger = logger;
        }
    }
}
