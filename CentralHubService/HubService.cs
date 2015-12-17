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
using JetBrains.Annotations;
using NetMQ;
using Raven.Abstractions.Extensions;
using Raven.Imports.Newtonsoft.Json;
using Serilog;

namespace CentralHubService
{
    public partial class HubService : ServiceBase
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Task _workingTask;
        private readonly ILogger _log;

        public HubService([NotNull] ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            _log = log;
        }

        protected override void OnStart(string[] args)
        {
            StartHub();
        }
        protected override void OnStop()
        {
            StopHub();
        }

        public void StartHub()
        {
            _workingTask = HubRunner();
        }

        private async Task HubRunner()
        {
            using (var context = NetMQ.NetMQContext.Create())
            {
                var socket = context.CreateRouterSocket();
                socket.Bind("tcp://*:10209");

                while (!_tokenSource.IsCancellationRequested)
                {
                    if (socket.HasIn)
                    {
                        //handle msg
                        var msg = socket.ReceiveMultipartMessage();
                        var replyMsg = HandleOneMsg(msg);
                        if(replyMsg!=null) socket.SendMultipartMessage(replyMsg);
                    }
                    else
                    {
                        //wait
                        await Task.Delay(50);
                    }
                }
            }
        }

        private NetMQMessage HandleOneMsg(NetMQMessage msg)
        {
            var msgType = msg.Pop().ConvertToString();
            var msgJson = msg.Pop().ConvertToString();
            JsonConvert.DeserializeObject(msgJson, Type.GetType(msgType))
        }

        public void StopHub()
        {
            _tokenSource.Cancel();
            try
            {
                _workingTask?.Wait();
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _log.Error(ex, "The stopping of the hub throwed an exception");
            }
        }


    }
}
