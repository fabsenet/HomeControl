using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Raven.Client;
using Serilog;
using Web.Models;

namespace Web.Controllers
{
    [RequireHttps]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDocumentStore _documentStore;

        public HomeController(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        [LogAction]
        public ActionResult Index()
        {
            using (var session = _documentStore.OpenSession())
            {
                var model = new HomeControllerIndexModel()
                {
                    Pings = session.Query<Ping>().OrderByDescending(p => p.LastOnlineTime).ToList()
                };
                return View(model);
            }
        }

        [LogAction]
        public ActionResult TestSignalr()
        {
            return View();
        }
    }
}