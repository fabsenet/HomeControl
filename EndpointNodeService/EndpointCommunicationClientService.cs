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
        private CancellationTokenSource _cancellationTokenSource;
        private Task _pingTask;

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
            _cancellationTokenSource = new CancellationTokenSource();

            _pingTask = PingContiniously(_cancellationTokenSource.Token);
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        private async Task PingContiniously(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await PingToService(token);
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
                catch (TaskCanceledException)
                {
                }
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
            _cancellationTokenSource?.Cancel();
            _pingTask?.Wait();
            _log.Debug("Service stopped.");
        }

        internal void Run()
        {
            OnStart(null);
            Console.WriteLine("Press enter to stop the application!");
            Console.ReadLine();
            OnStop();
        }
    }
}
