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
                "RedeemTransfer",
                "redeem-transfer",
                new { controller = "Application", action = "RedeemTransfer" }
            );

            routes.MapRoute(
                "RedeemTransferPopup",
                "redeem-transfer-popup",
                new {controller = "Application", action = "RedeemTransferPopup" }
            );

            routes.MapRoute(
                "OneStepPayment",
                "one-step-payment",
               new { controller = "Application", action = "OneStepPayment" }
            );

            routes.MapRoute(
                "OneStepPaymentPopup",
                "one-step-payment-popup",
               new { controller = "Application", action = "OneStepPaymentPopup" }
            );

            routes.MapRoute(
                "RedeemOneStepPayment",
                "redeem-one-step-payment",
               new { controller = "Application", action = "RedeemOneStepPayment" }
            );

            routes.MapRoute(
                "RedeemOneStepPaymentPopup",
                "redeem-one-step-payment-popup",
               new { controller = "Application", action = "RedeemOneStepPaymentPopup" }
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
                "CrossBorder",
                "cross-border",
               new { controller = "Application", action = "CrossBorder" }
            );

            routes.MapRoute(
                "RedeemCrossBorder",
                "redeem-cross-border",
                new { controller = "Application", action = "RedeemCrossBorder" }
            );

            routes.MapRoute(
                "CrossBorderPopup",
                "cross-border-popup",
               new { controller = "Application", action = "CrossBorderPopup" }
            );

            routes.MapRoute(
                "RedeemCrossBorderPopup",
                "redeem-cross-border-popup",
                new { controller = "Application", action = "RedeemCrossBorderPopup" }
            );

            routes.MapRoute(
                "SetTransferDestinations",
                "transferDestinations",
               new { controller = "Application", action = "SetTransferDestinations" }
            );

            routes.MapRoute(
                "Default",
                "{action}/{id}",
                new {controller = "Application", action = "Index", id = UrlParameter.Optional}
            );
        }
    }
}