using System;
using System.Web.Mvc;
using System.Web.Routing;
using Raven.Client;
using Web.Controllers;

namespace Web
{
    public class MvcConfig : DefaultControllerFactory
    {
        private readonly IDocumentStore _documentStore;

        public MvcConfig(IDocumentStore documentStore)
        {
            if (documentStore == null) throw new ArgumentNullException(nameof(documentStore));
            _documentStore = documentStore;
        }

        public override IController CreateController(RequestContext requestContext, string controllerName)
            {
                if (controllerName == nameof(HomeController).Replace("Controller", ""))
                {
                    // ... then create a new controller and set up the dependency
                    return new HomeController(_documentStore);
                }

                // Otherwise, fallback to the default implementation
                return base.CreateController(requestContext, controllerName);
            }
    }
}