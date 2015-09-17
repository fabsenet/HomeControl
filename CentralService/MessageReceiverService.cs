using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using Amqp.Types;
using Serilog;

namespace HomeControl.CentralService
{
    public class MessageReceiverService : ServiceBase
    {
        private readonly ILogger _log;
        private ContainerHost _host;

        public MessageReceiverService()
        {
            _log = Log.ForContext<MessageReceiverService>();
            this.ServiceName = "MessageReceiverService";

        }

        protected override void OnStart(string[] args)
        {
            _log.Debug("Service starting. Command line is {CommandLine}", Environment.CommandLine);

            Uri addressUri = new Uri(ConfigurationManager.ConnectionStrings["AMQP"].ConnectionString);
            _host = new ContainerHost(new Uri[] { addressUri }, null, addressUri.UserInfo);
            _host.Open();
            
           _log.Information("Container host is listening on {Host}:{Port}", addressUri.Host, addressUri.Port);
            
            _host.RegisterRequestProcessor("device_config", new DeviceConfigProcessor());
            _log.Verbose("Finished handler registration.");
        }

        protected override void OnStop()
        {
            _log.Debug("Service stopping.");
            _host.Close();
            _log.Debug("Service stopped.");
        }
        internal void RunInteractively()
        {
            OnStart(null);
            Console.WriteLine("Press enter to stop the application!");
            Console.ReadLine();
            OnStop();
        }
    }
}