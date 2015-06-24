using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Raven.Client;
using Raven.Client.Document;
using Web.Controllers;

namespace Web
{
    public class WebApiConfig
    {
        private readonly IDocumentStore _documentStore;

        public WebApiConfig(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public void Register(HttpConfiguration config)
        {
            // Web-API-Konfiguration und -Dienste

            // Web-API-Routen
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


            config.DependencyResolver = new MyDependencyResolver(_documentStore);
        }
    }


    public class MyDependencyResolver : IDependencyResolver {
        private readonly IDocumentStore _documentStore;

        public MyDependencyResolver(IDocumentStore documentStore)
        {
            if (documentStore == null) throw new ArgumentNullException(nameof(documentStore));
            _documentStore = documentStore;
        }

        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof (PingController))
            {
                return new PingController(_documentStore);
            }
            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }

        public IDependencyScope BeginScope()
        {
            return new MyDependencyResolver(_documentStore);
        }
    }
}
