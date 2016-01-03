using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Serilog;

namespace HomeControl.EndpointNodeService
{
    public class EndpointCommunicationClientService : ServiceBase
    {
        private readonly ILogger _log;
        private CancellationTokenSource _cancellationTokenSource;
        private NetMQContext _context;
        private DealerSocket _socket;

        public EndpointCommunicationClientService()
        {
            _log = Log.ForContext<EndpointCommunicationClientService>();
            ServiceName = "EndpointCommunicationClientService";
        }

        protected override void OnStart(string[] args)
        {
            _log.Debug("Service starting. Command line is {CommandLine}", Environment.CommandLine);
            _cancellationTokenSource = new CancellationTokenSource();

            var address = ConfigurationManager.ConnectionStrings["Hub"].ConnectionString;
            _context = NetMQContext.Create();
            _socket = _context.CreateDealerSocket();
            _socket.Connect(address);
        }

        protected override void OnStop()
        {
            _log.Debug("Service stopping.");
            _cancellationTokenSource?.Cancel();

            var stoppingTask = Task.Run(() =>
            {
                _socket.Dispose();
                _context.Dispose();
            });

            stoppingTask.Wait(TimeSpan.FromSeconds(10));

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
