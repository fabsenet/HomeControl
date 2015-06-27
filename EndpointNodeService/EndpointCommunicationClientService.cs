using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace EndpointNodeService
{
    public partial class EndpointCommunicationClientService : ServiceBase
    {
        private readonly ILogger _log;
        private readonly HttpClient _httpClient;

        public EndpointCommunicationClientService()
        {
            _log = Log.ForContext<EndpointCommunicationClientService>();
            InitializeComponent();


            _httpClient = new HttpClient(new HttpClientHandler()
            {
                Credentials = new NetworkCredential("a", "b"),
                UseDefaultCredentials = false,
                PreAuthenticate = true
            });

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.BaseAddress = new Uri("https://fabse.net/HomeControl/");
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
                await PingToService(token);
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }

        private async Task PingToService(CancellationToken token)
        {
            _log.Debug("Pinging");
            try
            {
                    var pingResult = await _httpClient.PostAsJsonAsync("api/Ping", Environment.MachineName, token);
                    pingResult.EnsureSuccessStatusCode();
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
