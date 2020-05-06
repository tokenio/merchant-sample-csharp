using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Tokenio;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Security;
using Tokenio.Tpp.TokenRequests;
using Tokenio.Utils;
using Member = Tokenio.Tpp.Member;
using TokenClient = Tokenio.Tpp.TokenClient;
using TokenRequest = Tokenio.TokenRequests.TokenRequest;
using DestinationCase = Tokenio.Proto.Common.TransferInstructionsProtos.TransferDestination.DestinationOneofCase;
using KeyNotFoundException= Tokenio.Security.KeyNotFoundException;

namespace merchant_sample_csharp.Controllers
{
    public class ApplicationController : Controller
    {
        private static string rootLocation = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string CSRF_TOKEN_KEY = "csrf_token";

        // Connect to Token's development sandbox
        private static readonly TokenClient tokenClient = InitializeSDK();
        private static string tokenRequestId = "";
        // If we're running the first time, create a new member (Token user account)
        // for this test merchant.
        // If we're running again, log in the previously-created member.
        private static Member merchantMember;

        private static TransferDestination destination = new TransferDestination
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


        /// <summary>
        /// Returns index page of sample
        /// </summary>
        /// <returns>Index page</returns>
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public Task<RedirectResult> Transfer()
        {
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-transfer");

            return InitializeTokenRequestUrl(
                   Request.QueryString,
                   redirectUrl,
                   Response,
                   "DEFAULT").Map(url => {
                        // send a 302 redirect
                        Response.StatusCode = 302;
                       return new RedirectResult(url);
                   });
        }

        [HttpPost]
        public Task<string> TransferPopup(TokenRequestModel formData)
        {
            NameValueCollection queryData = new NameValueCollection();
            formData.GetType().GetProperties()
                .ToList()
                .ForEach(property => queryData.Add(property.Name, property.GetValue(formData, null)?.ToString()));

            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-transfer-popup");

            return InitializeTokenRequestUrl(
                   queryData,
                   redirectUrl,
                   Response,
                   "DEFAULT");
        }

        [HttpGet]
        public Task<RedirectResult> OneStepPayment()
        {
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redirect-one-step-payment");

            return InitializeTokenRequestUrl(
                   Request.QueryString,
                   redirectUrl,
                   Response,
                   "ONE_STEP").Map(url => {
                       // send a 302 redirect
                       Response.StatusCode = 302;
                       return new RedirectResult(url);
                   });
        }

        [HttpPost]
        public Task<string> OneStepPaymentPopup(TokenRequestModel formData)
        {
            NameValueCollection queryData = new NameValueCollection();
            formData.GetType().GetProperties()
                .ToList()
                .ForEach(property => queryData.Add(property.Name, property.GetValue(formData, null)?.ToString()));

            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redirect-one-step-payment-popup");

            return InitializeTokenRequestUrl(
                   queryData,
                   redirectUrl,
                   Response,
                   "ONE_STEP");
        }

        [HttpGet]
        public Task<RedirectResult> StandingOrder()
        {
            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-standing-order");

            return InitializeStandingOrderTokenRequestUrl(
                    Request.QueryString,
                    redirectUrl,
                    Response).Map(url => {
                        // send a 302 redirect
                        Response.StatusCode = 302;
                        return new RedirectResult(url);
                    });
        }

        [HttpPost]
        public Task<string> StandingOrderPopUp(TokenRequestModel formData)
        {
            NameValueCollection queryData = new NameValueCollection();
            formData.GetType().GetProperties()
                .ToList()
                .ForEach(property => queryData.Add(property.Name, property.GetValue(formData, null)?.ToString()));

            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-standing-order-popup");

            return InitializeStandingOrderTokenRequestUrl(
                    queryData,
                    redirectUrl,
                    Response);
        }

        [HttpGet]
        public Task<RedirectResult> FutureDated()
        {
            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-future-dated");

            return InitializeTokenRequestUrl(
                   Request.QueryString,
                   redirectUrl,
                   Response,
                   "FUTURE_DATED").Map(url => {
                       // send a 302 redirect
                       Response.StatusCode = 302;
                       return new RedirectResult(url);
                   });
        }

        [HttpPost]
        public Task<string> FutureDatedPopup(TokenRequestModel formData)
        {
            NameValueCollection queryData = new NameValueCollection();
            formData.GetType().GetProperties()
                .ToList()
                .ForEach(property => queryData.Add(property.Name, property.GetValue(formData, null)?.ToString()));

            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-future-dated-popup");
            return InitializeTokenRequestUrl(
                    queryData,
                    redirectUrl,
                    Response,
                    "FUTURE_DATED");
        }

        [HttpGet]
        public Task<RedirectResult> CrossBorder()
        {
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-cross-border");

            return InitializeTokenRequestUrl(
                   Request.QueryString,
                   redirectUrl,
                   Response,
				   "DEFAULT").Map(url => {
                       // send a 302 redirect
                       Response.StatusCode = 302;
                       return new RedirectResult(url);
                   });
        }

        [HttpPost]
        public Task<string> CrossBorderPopup(TokenRequestModel formData)
        {
            NameValueCollection queryData = new NameValueCollection();
            formData.GetType().GetProperties()
                .ToList()
                .ForEach(property => queryData.Add(property.Name, property.GetValue(formData, null)?.ToString()));

            // generate Redirect Url
            var redirectUrl = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "redeem-cross-border-popup");

            return InitializeTokenRequestUrl(
                   queryData,
                   redirectUrl,
                   Response,
				   "DEFAULT");
        }

        [NonAction]
        public Task<string> Redeem()
        {
            var callbackUrl = Request.Url.ToString();

            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies[CSRF_TOKEN_KEY];

            // check CSRF token and retrieve state and token ID from callback parameters
            return GetMerchantMember()
                // check CSRF token and retrieve state and token ID from callback parameters
                .FlatMap(mem => tokenClient.ParseTokenRequestCallbackUrl(callbackUrl, csrfToken.Value)
                    // get the token and check its validity
                    .FlatMap(callback => mem.GetToken(callback.TokenId))
                    // redeem the token at the server to move the funds
                    .FlatMap(mem.RedeemToken)
                    .Map(transfer => "Success! Redeemed transfer " + transfer.Id));
        }

        [NonAction]
        public Task<string> RedeemPopup()
        {
            var queryParams = Request.QueryString;

            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies[CSRF_TOKEN_KEY];

            // check CSRF token and retrieve state and token ID from callback parameters
            return GetMerchantMember()
               // check CSRF token and retrieve state and token ID from callback parameters
               .FlatMap(mem => tokenClient.ParseTokenRequestCallbackParams(queryParams, csrfToken.Value)
                   // get the token and check its validity
                   .FlatMap(callback => mem.GetToken(callback.TokenId))
                   // redeem the token at the server to move the funds
                   .FlatMap(mem.RedeemToken)
                   .Map(transfer => "Success! Redeemed transfer " + transfer.Id));
        }

        [HttpGet]
        public Task<string> RedeemTransfer()
        {
            return Redeem();
        }

        [HttpGet]
        public Task<string> RedeemTransferPopup()
        {
            return RedeemPopup();
        }

        [HttpGet]
        public Task<string> RedirectOneStepPayment()
        {
            return Task.FromResult("Success! One Step Payment " + Request.QueryString.Get("transferId"));
        }

        [HttpGet]
        public Task<string> RedeemStandingOrder()
        {
            var callbackUrl = Request.Url.ToString();

            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies[CSRF_TOKEN_KEY];

            // check CSRF token and retrieve state and token ID from callback parameters
            return GetMerchantMember()
                // check CSRF token and retrieve state and token ID from callback parameters
                .FlatMap(mem => tokenClient.ParseTokenRequestCallbackUrl(callbackUrl, csrfToken.Value)
                    // redeem the token at the server to move the funds
                    .FlatMap(callback => mem.RedeemStandingOrderToken(callback.TokenId))
                    .Map(transfer => "Success! Redeemed transfer " + transfer.Id));
        }

        [HttpGet]
        public Task<string> RedeemStandingOrderPopup()
        {
            var queryParams = Request.QueryString;

            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies[CSRF_TOKEN_KEY];

            // check CSRF token and retrieve state and token ID from callback parameters
            return GetMerchantMember()
                // check CSRF token and retrieve state and token ID from callback parameters
                .FlatMap(mem => tokenClient.ParseTokenRequestCallbackParams(queryParams, csrfToken.Value)
                    // redeem the token at the server to move the funds
                    .FlatMap(callback => mem.RedeemStandingOrderToken(callback.TokenId))
                    .Map(transfer => "Success! Redeemed transfer " + transfer.Id));
        }

        [HttpGet]
        public Task<string> RedeemFutureDated()
        {
            return Redeem();
        }

        [HttpGet]
        public Task<string> RedeemFutureDatedPopup()
        {
            return RedeemPopup();
        }

        [HttpGet]
        public Task<string> RedeemCrossBorder()
        {
            return Redeem();
        }

        [HttpGet]
        public Task<string> RedeemCrossBorderPopup()
        {
            return RedeemPopup();
        }

        private static Task<string> InitializeTokenRequestUrl(
            NameValueCollection queryData,
            string callbackUrl,
            HttpResponseBase response,
            string transferType)
        {
            var bankId = "ngp-cbi-05034";
            var source = new TransferEndpoint
            {
                Account = new BankAccount
                {
                    Iban = new BankAccount.Types.Iban
                    {
                        Iban_ = "IT77O0848283352871412938123"
                    }
                },
                BankId = bankId
            };

            var amount = Convert.ToDouble(queryData["amount"]);
            var currency = queryData["currency"];
            var description = queryData["description"];

            // generate CSRF token
            var csrfToken = Util.Nonce();

            // generate a reference ID for the token
            var refId = Util.Nonce();

            var cookie = new HttpCookie(CSRF_TOKEN_KEY) { Value = csrfToken };
            // set CSRF token in browser cookie
            response.Cookies.Add(cookie);
            return GetMerchantMember().FlatMap(mem => mem.GetFirstAlias()
            .FlatMap(alias => {
                // Create a token request to be stored
                var tokenRequestBuilder = TokenRequest.TransferTokenRequestBuilder(amount, currency)
                      .SetDescription(description)
                      .SetRefId(refId)
                      .SetToAlias(alias)
                      .SetToMemberId(mem.MemberId())
                      .SetRedirectUrl(callbackUrl)
                      .SetCsrfToken(csrfToken);
                if (transferType == "FUTURE_DATED")
                {
                    //Sets the execution day of payment for after 2 days. 
                    DateTime startDate = DateTime.Now;
                    string executionDate = startDate.AddDays(2).ToString("yyyyMMdd");
                    tokenRequestBuilder.AddDestination(destination);
                    tokenRequestBuilder.SetExecutionDate(executionDate);
                }
                else if (transferType == "ONE_STEP")
                {
                    tokenRequestBuilder.SetSource(source);
                    tokenRequestBuilder.SetBankId(bankId);
                    tokenRequestBuilder.AddDestination(destination);

                }
                else
                {
                    tokenRequestBuilder.AddDestination(destination);
                }

                return mem.StoreTokenRequest(tokenRequestBuilder.Build())
                  // generate the Token request URL to redirect to
                  .FlatMap(requestId => {
                      tokenRequestId = requestId;
                      return tokenClient.GenerateTokenRequestUrl(requestId);
                      });
            }));
        }

        private static Task<string> InitializeStandingOrderTokenRequestUrl(
            NameValueCollection queryData,
            string callbackUrl,
            HttpResponseBase response)
        {
            var amount = Convert.ToDouble(queryData["amount"]);
            var currency = queryData["currency"];
            var description = queryData["description"];

            // generate CSRF token
            var csrfToken = Util.Nonce();

            // generate a reference ID for the token
            var refId = Util.Nonce();

            var cookie = new HttpCookie(CSRF_TOKEN_KEY) { Value = csrfToken };
            // set CSRF token in browser cookie
            response.Cookies.Add(cookie);

            DateTime startDate = DateTime.Now;
            DateTime endDate = startDate.AddYears(1);

            return GetMerchantMember().FlatMap(mem => mem.GetFirstAlias()
              .FlatMap(alias => mem.StoreTokenRequest(
                  // Create a token request to be stored
                  TokenRequest.StandingOrderRequestBuilder(
                            amount,
                            currency,
                            "MNTH",
                            startDate.ToString("yyyy-MM-dd"),
                            endDate.ToString("yyyy-MM-dd"),
                            new List<TransferDestination> { destination })
                        .SetDescription(description)
                        .AddDestination(destination)
                        .SetRefId(refId)
                        .SetToAlias(alias)
                        .SetToMemberId(mem.MemberId())
                        .SetRedirectUrl(callbackUrl)
                        .SetCsrfToken(csrfToken)
                        .Build()))
                  // generate the Token request URL to redirect to
                  .FlatMap(requestId => tokenClient.GenerateTokenRequestUrl(requestId)));
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
        private static Task<Member> LoadMember(TokenClient tokenClient, string memberId)
        {
            try
            {
                return tokenClient.GetMember(memberId);
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
        private static Task<Member> CreateMember(TokenClient tokenClient)
        {
            Alias alias = new Alias
            {
                // use uppercase to test normalization
                Value = "mcsharp-" + Util.Nonce().ToUpper() + "+noverify@example.com",
                Type = Alias.Types.Type.Email
            };

            return tokenClient.CreateMember(alias)
                .FlatMap(async (mem) =>
                {
                    // A member's profile has a display name and picture.
                    // The Token UI shows this (and the alias) to the user when requesting access.
                    await mem.SetProfile(new Profile
                    {
                        DisplayNameFirst = "Demo Merchant"
                    });
                    byte[] pict = System.IO.File.ReadAllBytes(Path.Combine(rootLocation, "Content/southside.png"));
                    await mem.SetProfilePicture("image/png", pict);

                    return mem;
                });
        }

        /// <summary>
        /// Log in existing member or create new member.
        /// </summary>
        /// <param name="tokenClient">Token SDK client</param>
        /// <returns>Logged-in member</returns>
        [NonAction]
        private static Task<Member> InitializeMember(TokenClient tokenClient)
        {
            var keyDir = Directory.GetFiles(Path.Combine(rootLocation, "keys"));

            var memberIds = keyDir.Where(d => d.Contains("_")).Select(d => d.Replace("_", ":"));

            return !memberIds.Any()
                ? CreateMember(tokenClient)
                : LoadMember(tokenClient, Path.GetFileName(memberIds.First()));

        }

        [NonAction]
        private static Task<Member> GetMerchantMember()
        {
            return merchantMember != null
                ? Task.FromResult(merchantMember)
                : InitializeMember(tokenClient)
                    .Map(mem =>
                    {
                        merchantMember = mem;
                        return mem;
                    });
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
