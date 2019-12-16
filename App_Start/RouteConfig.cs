using System.Web.Mvc;
using System.Web.Routing;

namespace merchant_sample_csharp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "TransferPopup",
                "transfer-popup",
                new {controller = "Application", action = "TransferPopup"}
            );

            routes.MapRoute(
                "RedeemPopup",
                "redeem-popup",
                new {controller = "Application", action = "RedeemPopup"}
            );

            routes.MapRoute(
                "StandingOrder",
                "standing-order",
                new { controller = "Application", action = "StandingOrder" }
            );

            routes.MapRoute(
                "RedeemStandingOrder",
                "redeem-standing-order",
                new { controller = "Application", action = "RedeemStandingOrder" }
            );


            routes.MapRoute(
                "StandingOrderPopup",
                "standing-order-popup",
                new { controller = "Application", action = "StandingOrderPopup" }
            );

            routes.MapRoute(
                "RedeemStandingOrderPopup",
                "redeem-standing-order-popup",
                new { controller = "Application", action = "RedeemStandingOrderPopup" }
            );

            routes.MapRoute(
                "FutureDated",
                "future-dated",
               new { controller = "Application", action = "FutureDated" }
            );

            routes.MapRoute(
                "FutureDatedPopup",
                "future-dated-popup",
               new { controller = "Application", action = "FutureDatedPopup" }
            );

            routes.MapRoute(
                "RedeemFutureDated",
                "redeem-future-dated",
               new { controller = "Application", action = "RedeemFutureDated" }
            );

            routes.MapRoute(
                "RedeemFutureDatedPopup",
                "redeem-future-dated-popup",
               new { controller = "Application", action = "RedeemFutureDatedPopup" }
            );

            routes.MapRoute(
                "Default",
                "{action}/{id}",
                new {controller = "Application", action = "Index", id = UrlParameter.Optional}
            );
        }
    }
}