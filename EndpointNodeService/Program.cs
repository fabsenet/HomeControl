using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Raven.Client.Document;
using Serilog;

namespace HomeControl.EndpointNodeService
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        private static void Main(string[] args)
        {
            var isRunningAsService = args.Any(a => a == "/s");

            ConfigureLogger();
            var service = new EndpointCommunicationClientService();
            if (isRunningAsService)
            {
                ServiceBase.Run(new ServiceBase[]{service});
            }
            else
            {
                service.RunInteractively();
            }

            (Log.Logger as IDisposable)?.Dispose();
        }

        private static void ConfigureLogger()
        {
            var logs = new DocumentStore { ConnectionStringName = "SerilogRavenDB" }.Initialize();

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Trace()
                .WriteTo.ColoredConsole()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("App", "HomeControl.EndpointNodeService")
                .CreateLogger();

            logger.Debug("The {application} startet at {StartTime} on server {ServerName}", Process.GetCurrentProcess().ProcessName, DateTime.Now, Environment.MachineName);

            Log.Logger = logger;
        }
    }
}
