extern alias ApiWrapper;

using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.MvcCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Authentication;
using System;
using ApiWrapper::Gnoss.ApiWrapper;

namespace Gnoss.Web.Login.SAML
{
    [AllowAnonymous]
    [Route("Auth")]
    public class AuthController : Controller
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configOAuth/OAuthV3.config");
        const string relayStateReturnUrl = "ReturnUrl";
        private Saml2Configuration config;

        public AuthController(IOptions<Saml2Configuration> configAccessor)
        {
            config = configAccessor.Value;
        }

        [HttpGet, HttpPost]
        [Route("Login")]
        public IActionResult Login(string returnUrl = null, string token = null)
        {
            mResourceApi.Log.Info($"9.-AuthController Login");
            var binding = new Saml2RedirectBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") }, { "token", token } });

            return binding.Bind(new Saml2AuthnRequest(config)).ToActionResult();
        }

        [HttpGet, HttpPost]
        [Route("AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService()
        {

            mResourceApi.Log.Info($"10.-AuthController AssertionConsumerService");
            var binding = new Saml2PostBinding();
            var saml2AuthnResponse = new Saml2AuthnResponse(config);

            binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
            }
            byte[] a = new byte[4];
            a[0] = 1;
            HttpContext.Session.Set("aaa", a);
            byte[] b;
            HttpContext.Session.TryGetValue("aaa",out b);
            mResourceApi.Log.Info("asdasd:"b[0].ToString());



            //binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            await saml2AuthnResponse.CreateSession(HttpContext, lifetime: new TimeSpan(0, 0, 5), claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));

            var relayStateQuery = binding.GetRelayStateQuery();
            var returnUrl = relayStateQuery.ContainsKey(relayStateReturnUrl) ? relayStateQuery[relayStateReturnUrl] : Url.Content("~/");
            mResourceApi.Log.Info("Redirect: " + returnUrl);
            return Redirect(returnUrl);
        }

        [HttpGet, HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            //return Redirect(config.SingleLogoutDestination.Scheme + "://" + config.SingleLogoutDestination.Host + "/cas/logout?service="+ config.Issuer);
            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = await new Saml2LogoutRequest(config, User).DeleteSession(HttpContext);
            return binding.Bind(saml2LogoutRequest).ToActionResult();
        }
    }
}
