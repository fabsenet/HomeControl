using System.Web.Mvc;
using Raven.Client;

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
            return View();
        }
    }
}