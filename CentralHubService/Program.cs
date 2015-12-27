using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Document;
using Serilog;

namespace CentralHubService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            var log = SetupLogging();

            log.Debug("the process runs interactively: {Interactive}", Environment.UserInteractive);
            var serviceToRun = new HubService(log.ForContext<HubService>());
            if (Environment.UserInteractive)
            {
                var t = new Thread(serviceToRun.StartHub);
                Console.WriteLine("press enter to stop the hub...");
                Console.ReadLine();
                log.Information("stopping hub service.");
                serviceToRun.StopHub();
                log.Information("stopped hub service.");
            }
            else
            {
                log.Information("Running the service.");
                ServiceBase.Run(serviceToRun);
            }
        }

        private static ILogger SetupLogging()
        {
            //var logs = new DocumentStore {ConnectionStringName = "SerilogRavenDB"}.Initialize();

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                //.WriteTo.RavenDB(logs)
                .WriteTo.Trace()
                .WriteTo.ColoredConsole()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("App", "HomeControl.CentralHubService")
                .CreateLogger();

            logger.Debug("The {application} startet at {StartTime} on server {ServerName}", Process.GetCurrentProcess().ProcessName, DateTime.Now, Environment.MachineName);

            return Log.Logger = logger;
        }
    }
}
