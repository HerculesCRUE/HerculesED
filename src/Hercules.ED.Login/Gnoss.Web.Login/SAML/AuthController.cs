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
using Gnoss.Web.Login.Open.SAML;

namespace Gnoss.Web.Login.SAML
{
    [AllowAnonymous]
    [Route("Auth")]
    public class AuthController : Controller
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        const string relayStateReturnUrl = "ReturnUrl";
        private Saml2Configuration config;
        readonly ConfigServiceSAML mConfigServiceSAML;

        public AuthController(IOptions<Saml2Configuration> configAccessor, ConfigServiceSAML configServiceSAML)
        {
            config = configAccessor.Value;
            mConfigServiceSAML = configServiceSAML;
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

            //binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);            
            await saml2AuthnResponse.CreateSession(HttpContext, lifetime: new TimeSpan(0, 0, 5), claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));

            var relayStateQuery = binding.GetRelayStateQuery();

            string token = relayStateQuery["token"];
            string returnUrl = relayStateQuery["ReturnUrl"];
            return Redirect(Url.Content(@$"~/{mConfigServiceSAML.GetUrlServiceInDomain()}LoginSAML") + "?returnUrl=" + returnUrl + "&token=" + token);
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
