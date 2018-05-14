using System.Web.Http;

namespace peshitta.nl.Api.Config
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config )
        {
           config.Routes.IgnoreRoute("noaxd", "{resource}.axd/{*pathInfo}");

            // Web API routes
            config.MapHttpAttributeRoutes();
            //config.Routes.MapHttpRoute(
            //        name: "DefaultApi",
            //        routeTemplate: "api/{controller}/{id}",
            //        defaults: new { id = RouteParameter.Optional }
            //    );
        }
    }
}