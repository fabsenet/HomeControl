using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using Raven.Client.Document;
using Serilog;

namespace HomeControl.CentralService
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLogger();
            var isRunningAsService = args.Any(a => a == "/s");
            Log.Verbose("isRunningAsService = {isRunningAsService}", isRunningAsService);
            var service = new MessageReceiverService();
            if (isRunningAsService)
            {
                ServiceBase.Run(service);
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
                .WriteTo.RavenDB(logs)
                .WriteTo.Trace()
                .WriteTo.ColoredConsole()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("App", "HomeControl.CentralService")
                .CreateLogger();

            logger.Debug("The {application} startet at {StartTime} on server {ServerName}", Process.GetCurrentProcess().ProcessName, DateTime.Now, Environment.MachineName);

            Log.Logger = logger;
        }
    }
}
