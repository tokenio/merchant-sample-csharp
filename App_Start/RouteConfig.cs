using System.Web.Mvc;
using System.Web.Routing;

namespace mechant_sample_csharp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "Default",
                "{action}/{id}",
                new {controller = "Application", action = "Index", id = UrlParameter.Optional}
            );
        }
    }
}