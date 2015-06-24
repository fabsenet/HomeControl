using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace EndpointNodeService
{
    public partial class EndpointCommunicationClientService : ServiceBase
    {
        private readonly ILogger _log;

        public EndpointCommunicationClientService()
        {
            _log = Log.ForContext<EndpointCommunicationClientService>();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log.Debug("Service starting. Command line is {CommandLine}", Environment.CommandLine);
            var cts = new CancellationTokenSource();



            var pingTask = PingContiniously(cts.Token);
            cts.CancelAfter(TimeSpan.FromMinutes(2));
            pingTask.Wait();

        }

        private async Task PingContiniously(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Factory.StartNew(PingToService, token);
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }

        private void PingToService()
        {
            _log.Debug("Pinging");
            try
            {

            }
            catch (Exception ex)
            {
                _log.Error(ex, "PingToService() failed with exception");
            }
        }

        protected override void OnStop()
        {
            _log.Debug("Service stopping.");
        }

        internal void Run()
        {
            OnStart(null);
            OnStop();
        }
    }
}
