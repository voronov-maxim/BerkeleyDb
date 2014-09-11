using BerkeleyDbWebApiServer.Controllers;
using Owin;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace BerkeleyDbWebApiSelfHost
{
    class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            var route = (System.Web.Http.Routing.HttpRoute)config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}");
            config.Services.Replace(typeof(IAssembliesResolver), new BerkeleyDbAssembliesResolver());
            //config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            //config.Services.Replace(typeof(IHttpControllerActivator), new BerkeleyControllerFactory(config.Services.GetHttpControllerActivator()));

            //config.MapODataServiceRoute("odata", "odata", BerkeleyEdmModel.GetModel());
            appBuilder.UseWebApi(config);
        }
    }

    public sealed class BerkeleyDbAssembliesResolver : IAssembliesResolver
    {
        public System.Collections.Generic.ICollection<Assembly> GetAssemblies()
        {
            return new Assembly[] { typeof(DatabaseController).Assembly };
        }
    }
}
