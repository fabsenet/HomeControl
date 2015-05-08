using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Serilog;

namespace Web.Controllers
{
    [RequireHttps]
    [Authorize]
    public class HomeController : Controller
    {
        [LogAction]
        public ActionResult Index()
        {
            return View();
        }
    }
}