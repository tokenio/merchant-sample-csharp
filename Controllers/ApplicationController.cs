using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Google.Protobuf;
using Tokenio;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Security;
using Member = Tokenio.Member;
using TokenRequest = Tokenio.TokenRequest;

namespace merchant_sample_csharp.Controllers
{
    public class ApplicationController : Controller
    {
        private static String rootLocation = AppDomain.CurrentDomain.BaseDirectory;
        
        // Connect to Token's development sandbox
        private static readonly TokenClient tokenClient = InitializeSDK();
        
        // If we're running the first time, create a new member (Token user account)
        // for this test merchant.
        // If we're running again, log in the previously-created member.
        private static Member merchantMember;
        
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
            var queryData = Request.QueryString;
            var destination = JsonParser.Default.Parse<BankAccount>("{\"sepa\":{\"iban\":\"DE16700222000072880129\"}}");

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

            return GetMerchantMember().FlatMap(mem => mem.GetFirstAlias()
                .FlatMap(alias => mem.StoreTokenRequest(
                    // Create a token request to be stored
                    TokenRequest.TransferTokenRequestBuilder(amount, currency)
                        .SetDescription(description)
                        .AddDestination(new TransferEndpoint
                        {
                            Account = destination
                        })
                        .SetRefId(refId)
                        .SetToAlias(alias)
                        .SetToMemberId(mem.MemberId())
                        .SetRedirectUrl(redirectUrl)
                        .SetCsrfToken(csrfToken)
                        .build()))
                // generate the Token request URL to redirect to
                .FlatMap(requestId => tokenClient.GenerateTokenRequestUrl(requestId))
                .Map(url =>
                {
                    // send a 302 redirect
                    Response.StatusCode = 302;
                    return new RedirectResult(url);
                }));
        }

        [HttpPost]
        public Task<string> TransferPopup(TokenRequestModel formData)
        {
            var destination = JsonParser.Default.Parse<BankAccount>("{\"sepa\":{\"iban\":\"DE16700222000072880129\"}}");

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

            return GetMerchantMember().FlatMap(mem => mem.GetFirstAlias()
                .FlatMap(alias => mem.StoreTokenRequest(
                    // Create a token request to be stored
                    TokenRequest.TransferTokenRequestBuilder(amount, currency)
                        .SetDescription(description)
                        .AddDestination(new TransferEndpoint
                        {
                            Account = destination
                        })
                        .SetRefId(refId)
                        .SetToAlias(alias)
                        .SetToMemberId(mem.MemberId())
                        .SetRedirectUrl(redirectUrl)
                        .SetCsrfToken(csrfToken)
                        .build()))
                // generate the Token request URL to redirect to
                .FlatMap(requestId => tokenClient.GenerateTokenRequestUrl(requestId)));
        }

        [HttpGet]
        public Task<string> Redeem()
        {
            var callbackUrl = Request.Url.ToString();
            
            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies["csrf_token"];

            return GetMerchantMember()
                // check CSRF token and retrieve state and token ID from callback parameters
                .FlatMap(mem => tokenClient.ParseTokenRequestCallbackUrl(callbackUrl, csrfToken.Value)
                    // get the token and check its validity
                    .FlatMap(callback => mem.GetToken(callback.TokenId))
                    // redeem the token at the server to move the funds
                    .FlatMap(mem.RedeemToken)
                    .Map(transfer => "Success! Redeemed transfer " + transfer.Id));
        }

        [HttpGet]
        public Task<string> RedeemPopup()
        {
            var queryParams = Request.QueryString;

            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies["csrf_token"];

            return GetMerchantMember()
                // check CSRF token and retrieve state and token ID from callback parameters
                .FlatMap(mem => tokenClient.ParseTokenRequestCallbackParams(queryParams, csrfToken.Value)
                    // get the token and check its validity
                    .FlatMap(callback => mem.GetToken(callback.TokenId))
                    // redeem the token at the server to move the funds
                    .FlatMap(mem.RedeemToken)
                    .Map(transfer => "Success! Redeemed transfer " + transfer.Id));
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
