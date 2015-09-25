using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HomeControl.Shared.Model;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Raven.Client;
using Serilog;
using Web.Models;

namespace Web.Controllers
{
    [RequireHttps]
    [System.Web.Mvc.Authorize]
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
                var allDeviceConfigs = session.Query<DeviceConfig>()
                    .OrderByDescending(p => p.LastOnlineTime)
                    .ToList();
                var model = new HomeControllerIndexModel()
                            {
                                DeviceConfigsOnline = allDeviceConfigs
                                    .Where(d => d.ConsideredOnline)
                                    .OrderBy(p => p.Hostname)
                                    .ToList(),
                                DeviceConfigsOffline = allDeviceConfigs
                                    .Where(d => !d.ConsideredOnline)
                                    .OrderByDescending(p => p.LastOnlineTime)
                                    .ToList(),
                            };
                return View(model);
            }
        }

        [LogAction]
        public ActionResult TestSignalr()
        {
            return View();
        }

        public ActionResult SetLight(string deviceName, bool desiredState)
        {
            var deviceHub = GlobalHost.ConnectionManager.GetHubContext<DeviceHub>();
            var command = new LedOnOffSetStateCommand() {DesiredState = desiredState, PinNumber = 18};
            deviceHub.Clients.All.ledOnOffSetStateCommand(JsonConvert.SerializeObject(command));

            return RedirectToAction("Index");
        }
    }
}