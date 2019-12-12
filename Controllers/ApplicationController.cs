using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Security;
using Tokenio.Utils;
using Member = Tokenio.Tpp.Member;
using TokenClient = Tokenio.Tpp.TokenClient;
using TokenRequest = Tokenio.TokenRequests.TokenRequest;

namespace merchant_sample_csharp.Controllers
{
    public class ApplicationController : Controller
    {
        private static string rootLocation = AppDomain.CurrentDomain.BaseDirectory;

        // Connect to Token's development sandbox
        private static readonly TokenClient tokenClient = InitializeSDK();

        // If we're running the first time, create a new member (Token user account)
        // for this test merchant.
        // If we're running again, log in the previously-created member.
        private static Member merchantMember = InitializeMember(tokenClient);

        /// <summary>
        /// Returns index page of sample
        /// </summary>
        /// <returns>Index page</returns>
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public RedirectResult Transfer()
        {
            var queryData = Request.QueryString;
            var destination = new TransferDestination
            {
                Sepa = new TransferDestination.Types.Sepa
                {
                    Bic = "bic",
                    Iban = "DE16700222000072880129"

                },
                CustomerData = new CustomerData
                {
                    LegalNames = { "merchant-sample-csharp" }
                }
            };

            var amount = Convert.ToDouble(queryData["amount"]);
            var currency = queryData["currency"];
            var description = queryData["description"];

            // generate CSRF token
            var csrfToken = Util.Nonce();

            // generate a reference ID for the token
            var refId = Util.Nonce();

            var cookie = new HttpCookie("csrf_token") {Value = csrfToken};
            // set CSRF token in browser cookie
            Response.Cookies.Add(cookie);

            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem");

            var transferToken = TokenRequest.TransferTokenRequestBuilder(amount, currency)
                        .SetDescription(description)
                        .AddDestination(destination)
                        .SetRefId(refId)
                        .SetToAlias(merchantMember.GetFirstAliasBlocking())
                        .SetToMemberId(merchantMember.MemberId())
                        .SetRedirectUrl(redirectUrl)
                        .SetCsrfToken(csrfToken)
                        .Build();
            string requestId = merchantMember.StoreTokenRequestBlocking(transferToken);

            string tokenRequestUrl = tokenClient.GenerateTokenRequestUrlBlocking(requestId);
            Response.StatusCode = 302;
            return new RedirectResult(tokenRequestUrl);
        }

        [HttpPost]
        public string TransferPopup(TokenRequestModel formData)
        {
            var destination = new TransferDestination
            {
                Sepa = new TransferDestination.Types.Sepa
                {
                    Bic = "bic",
                    Iban = "DE16700222000072880129"

                },
                CustomerData = new CustomerData
                {
                    LegalNames = { "merchant-sample-csharp" }
                }
            };
            var amount = Convert.ToDouble(formData.amount);
            var currency = formData.currency;
            var description = formData.description;

            // generate CSRF token
            var csrfToken = Util.Nonce();

            // generate a reference ID for the token
            var refId = Util.Nonce();

            var cookie = new HttpCookie("csrf_token") {Value = csrfToken};
            // set CSRF token in browser cookie
            Response.Cookies.Add(cookie);

            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-popup");

            var transferToken = TokenRequest.TransferTokenRequestBuilder(amount, currency)
                        .SetDescription(description)
                        .AddDestination(destination)
                        .SetRefId(refId)
                        .SetToAlias(merchantMember.GetFirstAliasBlocking())
                        .SetToMemberId(merchantMember.MemberId())
                        .SetRedirectUrl(redirectUrl)
                        .SetCsrfToken(csrfToken)
                        .Build();

            string requestId = merchantMember.StoreTokenRequestBlocking(transferToken);

            string tokenRequestUrl = tokenClient.GenerateTokenRequestUrlBlocking(requestId);
            Response.StatusCode = 200;
            return tokenRequestUrl;
        }

        [HttpGet]
        public RedirectResult StandingOrder()
        {
            var queryData = Request.QueryString;
            var destination = new TransferDestination
            {
                Sepa = new TransferDestination.Types.Sepa
                {
                    Bic = "bic",
                    Iban = "DE16700222000072880129"

                },
                CustomerData = new CustomerData
                {
                    LegalNames = { "merchant-sample-csharp" }
                }
            };

            var amount = Convert.ToDouble(queryData["amount"]);
            var currency = queryData["currency"];
            var description = queryData["description"];

            // generate CSRF token
            var csrfToken = Util.Nonce();

            // generate a reference ID for the token
            var refId = Util.Nonce();

            var cookie = new HttpCookie("csrf_token") { Value = csrfToken };
            // set CSRF token in browser cookie
            Response.Cookies.Add(cookie);

            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-standing-order");

            DateTime startDate = DateTime.Now;
            DateTime endDate = startDate.AddYears(1);

            var request = TokenRequest.StandingOrderRequestBuilder(
            amount,
            currency,
            "MNTH",
            startDate.ToString("yyyy-MM-dd"),
            endDate.ToString("yyyy-MM-dd"),
            new List<TransferDestination>() { destination })
            .SetDescription(description)
            .AddDestination(destination)
            .SetRefId(refId)
            .SetToAlias(merchantMember.GetFirstAliasBlocking())
            .SetToMemberId(merchantMember.MemberId())
            .SetRedirectUrl(redirectUrl)
            .SetCsrfToken(csrfToken)
            .Build();
            string requestId = merchantMember.StoreTokenRequestBlocking(request);
            string tokenRequestUrl = tokenClient.GenerateTokenRequestUrlBlocking(requestId);

            Response.StatusCode = 302;
            return new RedirectResult(tokenRequestUrl);
        }

        [HttpPost]
        public string StandingOrderPopUp(TokenRequestModel formData)
        {
            var destination = new TransferDestination
            {
                Sepa = new TransferDestination.Types.Sepa
                {
                    Bic = "bic",
                    Iban = "DE16700222000072880129"

                },
                CustomerData = new CustomerData
                {
                    LegalNames = { "merchant-sample-csharp" }
                }
            };

            var queryData = Request.QueryString;

            var amount = Convert.ToDouble(formData.amount);
            var currency = formData.currency;
            var description = formData.description;

            // generate CSRF token
            var csrfToken = Util.Nonce();

            // generate a reference ID for the token
            var refId = Util.Nonce();

            var cookie = new HttpCookie("csrf_token") { Value = csrfToken };
            // set CSRF token in browser cookie
            Response.Cookies.Add(cookie);

            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-standing-order-popup");

            DateTime startDate = DateTime.Now;
            DateTime endDate = startDate.AddYears(1);

            var request = TokenRequest.StandingOrderRequestBuilder(
            amount,
            currency,
            "MNTH",
            startDate.ToString("yyyy-MM-dd"),
            endDate.ToString("yyyy-MM-dd"),
            new List<TransferDestination>() { destination })
            .SetDescription(description)
            .AddDestination(destination)
            .SetRefId(refId)
            .SetToAlias(merchantMember.GetFirstAliasBlocking())
            .SetToMemberId(merchantMember.MemberId())
            .SetRedirectUrl(redirectUrl)
            .SetCsrfToken(csrfToken)
            .Build();
            string requestId = merchantMember.StoreTokenRequestBlocking(request);
            string tokenRequestUrl = tokenClient.GenerateTokenRequestUrlBlocking(requestId);

            Response.StatusCode = 200;
            return tokenRequestUrl;
        }

        [HttpGet]
        public string Redeem()
        {
            var callbackUrl = Request.Url.ToString();

            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies["csrf_token"];

            // check CSRF token and retrieve state and token ID from callback parameters
            var callback = tokenClient.ParseTokenRequestCallbackUrlBlocking(
                    callbackUrl,
                    csrfToken.Value);

            //get the token and check its validity
            var token = merchantMember.GetTokenBlocking(callback.TokenId);

            //redeem the token at the server to move the funds
            var transfer = merchantMember.RedeemTokenBlocking(token);

            Response.StatusCode = 200;
            return string.Format("Success! Redeemed transfer {0}", transfer.Id);
        }

        [HttpGet]
        public string RedeemPopup()
        {
            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies["csrf_token"];

            // check CSRF token and retrieve state and token ID from callback parameters
            var callback = tokenClient.ParseTokenRequestCallbackUrlBlocking(
                    Request.Url.AbsoluteUri,
                    csrfToken.Value);

            //get the token and check its validity
            var token = merchantMember.GetTokenBlocking(callback.TokenId);

            //redeem the token at the server to move the funds
            var transfer = merchantMember.RedeemTokenBlocking(token);
            Response.StatusCode = 200;
            return string.Format("Success! Redeemed transfer {0}", transfer.Id);
        }

        [HttpGet]
        public string RedeemStandingOrder()
        {
            var callbackUrl = Request.Url.ToString();

            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies["csrf_token"];

            // check CSRF token and retrieve state and token ID from callback parameters
            var callback = tokenClient.ParseTokenRequestCallbackUrlBlocking(
                    callbackUrl,
                    csrfToken.Value);

            //redeem the token at the server to move the funds
            var standingOrderSubmission = merchantMember
                    .RedeemStandingOrderTokenBlocking(callback.TokenId);
            Response.StatusCode = 200;
            return string.Format("Success! Redeemed transfer {0}", standingOrderSubmission.Id);
        }

        [HttpGet]
        public string RedeemStandingOrderPopup()
        {
            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies["csrf_token"];

            // check CSRF token and retrieve state and token ID from callback parameters
            var callback = tokenClient.ParseTokenRequestCallbackUrlBlocking(
                    Request.Url.AbsoluteUri,
                    csrfToken.Value);

            //redeem the token at the server to move the funds
            var standingOrderSubmission = merchantMember
                    .RedeemStandingOrderTokenBlocking(callback.TokenId);

            Response.StatusCode = 200;
            return string.Format("Success! Redeemed transfer {0}", standingOrderSubmission.Id);
        }

        /// <summary>
        /// Initializes the SDK, pointing it to the specified environment and the directory where keys are being stored.
        /// </summary>
        /// <returns>TokenClient SDK instance</returns>
        [NonAction]
        private static TokenClient InitializeSDK()
        {
            var key = Directory.CreateDirectory(Path.Combine(rootLocation, "keys"));

            return TokenClient.NewBuilder()
                .ConnectTo(TokenCluster.GetCluster(TokenCluster.TokenEnv.Sandbox))
                .Port(443)
                .Timeout(10 * 60 * 1000) // Set high for easy debugging.
                 // This KeyStore reads private keys from files.
                 // Here, it's set up to read the ./keys dir.
                .WithKeyStore(new UnsecuredFileSystemKeyStore(key.FullName))
                .Build();
        }

        /// <summary>
        /// Using a TokenClient SDK client and the member ID of a previously-created
        /// Member (whose private keys we have stored locally).
        /// </summary>
        /// <param name="tokenClient">SDK</param>
        /// <param name="memberId">ID of Member</param>
        /// <returns>Logged-in member</returns>
        [NonAction]
        private static Member LoadMember(TokenClient tokenClient, string memberId)
        {
            try
            {
                return tokenClient.GetMemberBlocking(memberId);
            }
            catch (KeyNotFoundException)
            {
                // it looks like we have a key but the member it belongs to does not exist in the DB
                throw new Exception("Couldn't log in saved member, not found. Remove keys dir and try again.");
            }
        }

        /// <summary>
        /// Using a TokenClient SDK client, create a new Member.
        /// This has the side effect of storing the new Member's private
        /// keys in the ./keys directory.
        /// </summary>
        /// <param name="tokenClient">SDK</param>
        /// <returns>newly-created member</returns>
        [NonAction]
        private static Member CreateMember(TokenClient tokenClient)
        {
            Alias alias = new Alias
            {
                // use uppercase to test normalization
                Value = "mcsharp-" + Util.Nonce().ToUpper() + "+noverify@example.com",
                Type = Alias.Types.Type.Email
            };
            Member member = tokenClient.CreateMemberBlocking(alias);
            // A member's profile has a display name and picture.
            // The Token UI shows this (and the alias) to the user when requesting access.
            member.SetProfileBlocking(new Profile
            {
                DisplayNameFirst = "Merchant Demo"
            });

            byte[] pict = System.IO.File.ReadAllBytes(Path.Combine(rootLocation, "Content/southside.png"));
            member.SetProfilePictureBlocking("image/png", pict);
            return member;
            // The newly-created member is automatically logged in.
        }

        /// <summary>
        /// Log in existing member or create new member.
        /// </summary>
        /// <param name="tokenClient">Token SDK client</param>
        /// <returns>Logged-in member</returns>
        [NonAction]
        private static Member InitializeMember(TokenClient tokenClient)
        {
            var keyDir = Directory.GetFiles(Path.Combine(rootLocation, "keys"));

            var memberIds = keyDir.Where(d => d.Contains("_")).Select(d => d.Replace("_", ":"));

            return !memberIds.Any()
                ? CreateMember(tokenClient)
                : LoadMember(tokenClient, Path.GetFileName(memberIds.First()));

        }

        public class TokenRequestModel
        {
            public string merchantId { get; set; }
            public double amount { get; set; }
            public string currency { get; set; }
            public string description { get; set; }
            public string destination { get; set; }
        }
    }
}
