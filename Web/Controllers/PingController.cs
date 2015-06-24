using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Raven.Client;
using Serilog;
using Web.Models;

namespace Web.Controllers
{
    public class PingController : ApiController
    {
        private readonly IDocumentStore _documentStore;
        private readonly ILogger _log;
        
        public PingController(IDocumentStore documentStore)
        {
            if (documentStore == null) throw new ArgumentNullException(nameof(documentStore));

            _log = Log.ForContext<PingController>();
            _documentStore = documentStore;
        }

        public HttpResponseMessage Post([FromBody] string hostName)
        {
            _log.Debug("Received ping for host {hostName}", hostName);

            try
            {
                if (string.IsNullOrWhiteSpace(hostName))
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
                using (var session = _documentStore.OpenSession())
                {
                    var pingId = "Pings/" + hostName;
                    var ping = session.Load<Ping>(pingId);
                    if (ping == null)
                    {
                        ping = new Ping {Id = pingId, Hostname = hostName};
                        session.Store(ping);
                    }

                    ping.LastOnlineTime = DateTime.UtcNow;
                    session.SaveChanges();
                }


                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Posting a Ping to hostname {hostname} failed with an exception.", hostName);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
