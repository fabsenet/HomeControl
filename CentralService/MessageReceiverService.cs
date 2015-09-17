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

            //_host.RegisterMessageProcessor("message_processor", new MessageProcessor());
            //_host.RegisterRequestProcessor("request_processor", new RequestProcessor());
            _host.RegisterRequestProcessor("device_config", new DeviceConfigProcessor());
            _host.RegisterLinkProcessor(new LinkProcessor());
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

    //class MessageProcessor : IMessageProcessor
    //{
    //    private readonly ILogger _log;

    //    public MessageProcessor()
    //    {
    //        _log = Log.ForContext<MessageProcessor>();
    //    }

    //    int IMessageProcessor.Credit
    //    {
    //        get { return 300; }
    //    }

    //    void IMessageProcessor.Process(MessageContext messageContext)
    //    {
    //        _log.Verbose("Received message {@message}", messageContext.Message);

    //        messageContext.Complete();
    //    }
    //}

    //class RequestProcessor : IRequestProcessor
    //{
    //    private readonly ILogger _log;
    //    public RequestProcessor()
    //    {
    //        _log = Log.ForContext<RequestProcessor>();
    //    }

    //    public void Process(RequestContext requestContext)
    //    {
    //        _log.Verbose("Received message {@message}", requestContext.Message);

    //        Message response = new Message("welcome");
    //        requestContext.Complete(response);
    //    }
    //}


    class LinkProcessor : ILinkProcessor
    {
        public void Process(AttachContext attachContext)
        {
            // start a task to process this request
            var task = this.ProcessAsync(attachContext);
        }

        async Task ProcessAsync(AttachContext attachContext)
        {
            // simulating an async operation required to complete the task
            await Task.Delay(100);

            if (attachContext.Attach.LinkName == "")
            {
                // how to fail the attach request
                attachContext.Complete(new Error() { Condition = ErrorCode.InvalidField, Description = "Empty link name not allowed." });
            }
            else if (attachContext.Link.Role)
            {
                attachContext.Complete(new IncomingLinkEndpoint(), 300);
            }
            else
            {
                attachContext.Complete(new OutgoingLinkEndpoint(), 0);
            }
        }
    }

    class IncomingLinkEndpoint : LinkEndpoint
    {
        public override void OnMessage(MessageContext messageContext)
        {
            // this can also be done when an async operation, if required, is done
            messageContext.Complete();
        }

        public override void OnFlow(FlowContext flowContext)
        {
        }

        public override void OnDisposition(DispositionContext dispositionContext)
        {
        }
    }

    class OutgoingLinkEndpoint : LinkEndpoint
    {
        public override void OnFlow(FlowContext flowContext)
        {
            for (int i = 0; i < flowContext.Messages; i++)
            {
                var message = new Message("Hello!");
                message.Properties = new Properties() { Subject = "Welcome Message" };
                flowContext.Link.SendMessage(message, null);
            }
        }

        public override void OnDisposition(DispositionContext dispositionContext)
        {
            if (!dispositionContext.Settled)
            {
                dispositionContext.Link.DisposeMessage(dispositionContext.Message, new Accepted(), true);
            }
        }
    }
}