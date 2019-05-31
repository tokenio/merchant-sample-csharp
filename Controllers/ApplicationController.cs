﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        [System.Web.Mvc.HttpGet]
        public RedirectResult Transfer()
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
            
            // create the token request
            var request = TokenRequest.TransferTokenRequestBuilder(amount, currency)
                .SetDescription(description)
                .AddDestination(new TransferEndpoint
                {
                    Account = destination
                })
                .SetRefId(refId)
                .SetToAlias(merchantMember.GetFirstAliasBlocking())
                .SetToMemberId(merchantMember.MemberId())
                .SetRedirectUrl(redirectUrl)
                .SetCsrfToken(csrfToken)
                .build();

            var requestId = merchantMember.StoreTokenRequestBlocking(request);

            //generate Token Request URL to redirect to
            var tokenRequestUrl = tokenClient.GenerateTokenRequestUrlBlocking(requestId);
            
            //send a 302 redirect
            Response.StatusCode = 302;
            return new RedirectResult(tokenRequestUrl);
        }

        [System.Web.Mvc.HttpPost]
        public string TransferPopup(TokenRequestModel formData)
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
            
            // create the token request
            var request = TokenRequest.TransferTokenRequestBuilder(amount, currency)
                .SetDescription(description)
                .AddDestination(new TransferEndpoint
                {
                    Account = destination
                })
                .SetRefId(refId)
                .SetToAlias(merchantMember.GetFirstAliasBlocking())
                .SetToMemberId(merchantMember.MemberId())
                .SetRedirectUrl(redirectUrl)
                .SetCsrfToken(csrfToken)
                .build();

            var requestId = merchantMember.StoreTokenRequestBlocking(request);

            //generate Token Request URL to redirect to
            var tokenRequestUrl = tokenClient.GenerateTokenRequestUrlBlocking(requestId);
            
            //send tokenRequestUrl
            return tokenRequestUrl;
        }

        [System.Web.Mvc.HttpGet]
        public string Redeem()
        {
            var queryParams = Request.QueryString.ToString();
            
            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies["csrf_token"];
            
            // check CSRF token and retrieve state and token ID from callback parameters
            TokenRequestCallback callback = tokenClient.ParseTokenRequestCallbackUrlBlocking(
                queryParams, 
                csrfToken.Value);

            //get the token and check its validity
            var token = merchantMember.GetTokenBlocking(callback.TokenId);
            
            //redeem the token at the server to move the funds
            var transfer = merchantMember.RedeemTokenBlocking(token);

            return "Success! Redeemed transfer " + transfer.Id;
        }

        [System.Web.Mvc.HttpGet]
        public string RedeemPopup()
        {
            var queryParams = Request.QueryString.ToString();
            
            // retrieve CSRF token from browser cookie
            var csrfToken = Request.Cookies["csrf_token"];
            
            // check CSRF token and retrieve state and token ID from callback parameters
            TokenRequestCallback callback = tokenClient.ParseTokenRequestCallbackUrlBlocking(
                queryParams, 
                csrfToken.Value);

            //get the token and check its validity
            var token = merchantMember.GetTokenBlocking(callback.TokenId);
            
            //redeem the token at the server to move the funds
            var transfer = merchantMember.RedeemTokenBlocking(token);

            return "Success! Redeemed transfer " + transfer.Id;
        }
        
        /// <summary>
        /// Initializes the SDK, pointing it to the specified environment and the directory where keys are being stored.
        /// </summary>
        /// <returns>TokenClient SDK instance</returns>
        [System.Web.Mvc.NonAction]
        private static TokenClient InitializeSDK()
        {
            var key = Directory.CreateDirectory("./keys");

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
        [System.Web.Mvc.NonAction]
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
        [System.Web.Mvc.NonAction]
        private static Member CreateMember(TokenClient tokenClient)
        {
            Alias alias = new Alias
            {
                // use uppercase to test normalization
                Value = "mcsharp-" + Util.Nonce().ToUpper() + "+noverify@example.com",
                Type = Alias.Types.Type.Email
            };

            var member = tokenClient.CreateMemberBlocking(alias);
            // A member's profile has a display name and picture.
            // The Token UI shows this (and the alias) to the user when requesting access.
            member.SetProfile(new Profile
            {
                DisplayNameFirst = "Demo Merchant"
            });
            byte[] pict = System.IO.File.ReadAllBytes("Content/southside.png");
            member.SetProfilePictureBlocking("image/png", pict);
            return member;
        }

        /// <summary>
        /// Log in existing member or create new member.
        /// </summary>
        /// <param name="tokenClient">Token SDK client</param>
        /// <returns>Logged-in member</returns>
        [System.Web.Mvc.NonAction]
        private static Member InitializeMember(TokenClient tokenClient)
        {
            var keyDir = Directory.GetFiles("./keys");

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