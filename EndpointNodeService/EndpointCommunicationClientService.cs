using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Serilog;

namespace HomeControl.EndpointNodeService
{
    public class EndpointCommunicationClientService : ServiceBase
    {
        private readonly ILogger _log;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _pingTask;
        private SenderLink _sender;
        private Session _session;
        private Connection _connection;

        public EndpointCommunicationClientService()
        {
            _log = Log.ForContext<EndpointCommunicationClientService>();
            ServiceName = "EndpointCommunicationClientService";
        }

        protected override void OnStart(string[] args)
        {
            _log.Debug("Service starting. Command line is {CommandLine}", Environment.CommandLine);
            _cancellationTokenSource = new CancellationTokenSource();

            string address = ConfigurationManager.ConnectionStrings["AMQP"].ConnectionString;
            _connection = new Connection(new Address(address));
            _session = new Session(_connection);
            _sender = new SenderLink(_session, "message-client", "message_processor");

            _pingTask = PingContiniously(_cancellationTokenSource.Token);
        }

        private async Task PingContiniously(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await PingToService(token);
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private async Task PingToService(CancellationToken token)
        {
            _log.Debug("Pinging");
            try
            {
                Message message = new Message("hello");
                message.Properties = new Properties() { MessageId = "msg" };
                message.ApplicationProperties = new ApplicationProperties();
                message.ApplicationProperties["sn"] = 17;
                await _sender.SendAsync(message);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "PingToService() failed with exception");
            }
        }

        protected override void OnStop()
        {
            _log.Debug("Service stopping.");
            _cancellationTokenSource?.Cancel();
            _pingTask?.Wait();

            _sender.Close();
            _session.Close();
            _connection.Close();

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
