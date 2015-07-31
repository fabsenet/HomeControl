using System;
using System.Configuration;
using System.ServiceProcess;
using Amqp;
using Amqp.Listener;
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


            // uncomment the following to write frame traces
            //Trace.TraceLevel = TraceLevel.Frame;
            //Trace.TraceListener = (f, a) => Console.WriteLine(DateTime.Now.ToString("[hh:ss.fff]") + " " + string.Format(f, a));

            Uri addressUri = new Uri(ConfigurationManager.ConnectionStrings["AMQP"].ConnectionString);
            _host = new ContainerHost(new Uri[] { addressUri }, null, addressUri.UserInfo);
            _host.Open();
           _log.Information("Container host is listening on {Host}:{Port}", addressUri.Host, addressUri.Port);

            _host.RegisterMessageProcessor("message_processor", new MessageProcessor());
            _host.RegisterRequestProcessor("request_processor", new RequestProcessor());
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

    class MessageProcessor : IMessageProcessor
    {
        private readonly ILogger _log;

        public MessageProcessor()
        {
            _log = Log.ForContext<MessageProcessor>();
        }

        int IMessageProcessor.Credit
        {
            get { return 300; }
        }

        void IMessageProcessor.Process(MessageContext messageContext)
        {
            _log.Verbose("Received message {@message}", messageContext.Message);

            messageContext.Complete();
        }
    }

    class RequestProcessor : IRequestProcessor
    {
        private readonly ILogger _log;
        public RequestProcessor()
        {
            _log = Log.ForContext<RequestProcessor>();
        }

        void IRequestProcessor.Process(RequestContext requestContext)
        {
            _log.Verbose("Received message {@message}", requestContext.Message);

            Message response = new Message("welcome");
            requestContext.Complete(response);
        }
    }
}