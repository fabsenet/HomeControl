using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using HomeControl.Shared.Contract;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetLight(string deviceName, bool desiredState, int pinNumber)
        {
            //send command to actual device
            var connectedDevice = GetClient(deviceName);
            if (connectedDevice == null) return Content("Device not connected :-(");

            var command = new LedOnOffSetStateCommand()
                          {
                              DesiredState = desiredState,
                              PinNumber = pinNumber
                          };
            connectedDevice.LedOnOffSetStateCommand(JsonConvert.SerializeObject(command));

            //update known state of device
            using (var session = _documentStore.OpenSession())
            {
                var device = session.Load<DeviceConfig>("DeviceConfigs/" + deviceName);
                device.LedStatesByPinNumber[pinNumber] = desiredState;
                session.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        private static IHubContext<IDeviceHubClient> DeviceHubContext => GlobalHost.ConnectionManager.GetHubContext<DeviceHub, IDeviceHubClient>();

        public ActionResult TransitionPowerState(string devicename, PowerStateEnum desiredPowerState)
        {
            var command = new TransitionPowerStateCommand() { DesiredPowerState = desiredPowerState};
            DeviceHubContext.Clients.All.LedOnOffSetStateCommand(JsonConvert.SerializeObject(command));

            return RedirectToAction("Index");
        }

        private IDeviceHubClient GetClient(string deviceName)
        {//todo use method
            var connectionId = DeviceHub.GetConnectionIdForDeviceName(deviceName);
            if (connectionId == null) return null;

            return DeviceHubContext.Clients.Client(connectionId);
        }
    }
}