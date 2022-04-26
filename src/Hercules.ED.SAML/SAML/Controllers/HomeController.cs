using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hercules_SAML.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections;
using Hercules_SAML.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Principal;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using System.Text;
using System.Web;
using Hercules_SAML.Services;
using Gnoss.ApiWrapper.Model;

namespace Hercules_SAML.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        readonly ConfigService _ConfigService;
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configOAuth/OAuthV3.config");
        private static readonly CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configOAuth/OAuthV3.config");
        private static readonly UserApi mUserApi = new UserApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configOAuth/OAuthV3.config");

        public HomeController(ILogger<HomeController> logger, ConfigService configService)
        {
            _logger = logger;
            _ConfigService = configService;
        }

        public IActionResult Index(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                if (User != null)
                {
                    //Loguear
                    return new RedirectResult(LoguearUsuario(User, returnUrl));
                }
                else
                {
                    //Si no hay usuario redirigimos al login 
                    Response.Redirect(Url.Content("~/Auth/Login") + "?returnUrl=" + Url.Content("~/") + "?returnUrl=" + returnUrl);
                }
            }

            return View();
        }

        private string LoguearUsuario(ClaimsPrincipal pUser,string pReturnUrl)
        {
            string email = "";
            //TODO coger de claims
            email = "skarmeta@um.es";

            mCommunityApi.Log.Info("numClaims:" + pUser.Claims.ToList());

            foreach (Claim claim in pUser.Claims.ToList())
            {
                mCommunityApi.Log.Info("CLAIM TYPE: '" + claim.Type + "' CLAIMVALUE: '" + claim.Value.Trim().ToLower() + "'");
                //TODO claims                
            }

            //comprobamos si existe la persona del email
            string person=mResourceApi.VirtuosoQuery("select ?person", @$"where{{
                                        ?person <http://w3id.org/roh/isActive> 'true'.
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <https://www.w3.org/2006/vcard/ns#email> '{email}'.
                        }}", "person").results.bindings.FirstOrDefault()?["person"].value;

            string UrlServicioLoginGnoss = _ConfigService.GetUrlServicioLoginGnoss(); ;
            string UrlComunidadGnoss = _ConfigService.GetCommunityURL();
            string UrlLogout = UrlComunidadGnoss+"/desconectar";

            if (string.IsNullOrEmpty(person))
            {
                //No existe ninguna persona aociada al correo
                mCommunityApi.Log.Info("Redirigir a la home");
                return UrlComunidadGnoss;//UrlComunidadGnoss;                    
            }
            else
            {
                mCommunityApi.Log.Info("LOGUEAMOS");
                //Comprobamos si existe usuario para la persona
                string user = mResourceApi.VirtuosoQuery("select ?user", @$"where{{
                                        <{person}> <http://w3id.org/roh/gnossUser> ?user.
                        }}", "person").results.bindings.FirstOrDefault()?["user"].value;

                User usuario = null;
                if (!string.IsNullOrEmpty(user))
                {                    
                    //Obtenemos el usuario
                    Guid userID = new Guid(user.Substring(user.LastIndexOf("/") + 1));
                    usuario = mUserApi.GetUserById(userID);
                }

                if (usuario==null)
                {
                    usuario = new User();
                    usuario.email = email;
                    usuario.password = CreatePassword();
                    usuario.name = "name";
                    usuario.last_name = "last_name";
                    usuario = mUserApi.CreateUser(usuario);
                    //Modificamos persona para asignar ususario

                    //Insertamos
                    Dictionary<Guid, List<TriplesToInclude>> triples = new() { { mResourceApi.GetShortGuid(person), new List<TriplesToInclude>() } };
                    TriplesToInclude t = new();
                    t.Predicate = "http://w3id.org/roh/gnossUser";
                    t.NewValue = "http://gnoss/" + usuario.user_id.ToString().ToUpper();
                    triples[mResourceApi.GetShortGuid(person)].Add(t);
                    mResourceApi.InsertPropertiesLoadedResources(triples);
                }

                mCommunityApi.Log.Info("Pedimos login token");
                string loginToken = mUserApi.GetLoginTokenForEmail(usuario.email);
                mCommunityApi.Log.Info("login token:" + loginToken);
                string logoutUrl = UrlLogout;
                mCommunityApi.Log.Info("logoutUrl:" + logoutUrl);
                mCommunityApi.Log.Info("urlRedirect:" + pReturnUrl);                
                mCommunityApi.Log.Info($"{UrlServicioLoginGnoss}/externallogin.aspx?loginToken={loginToken}&redirect={HttpUtility.UrlEncode(pReturnUrl)}&logout={HttpUtility.UrlEncode(logoutUrl)}");                
                return $"{UrlServicioLoginGnoss}/externallogin.aspx?loginToken={loginToken}&redirect={HttpUtility.UrlEncode(pReturnUrl)}&logout={HttpUtility.UrlEncode(logoutUrl)}";
            }
        }

        private string CreatePassword()
        {
            int length = 5;
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string validNum = "1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
                res.Append(validNum[rnd.Next(validNum.Length)]);
            }
            return res.ToString();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
