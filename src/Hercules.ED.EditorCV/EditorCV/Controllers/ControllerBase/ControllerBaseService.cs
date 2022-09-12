using Gnoss.ApiWrapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EditorCV.Controllers
{
    public abstract class ControllerBaseService : ControllerBase
    {
        private static readonly Gnoss.ApiWrapper.UserApi mUserApi = new Gnoss.ApiWrapper.UserApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        protected bool CheckUser(HttpRequest pRequest, Guid pIdUsuario)
        {
            try
            {
                Guid userID = mUserApi.GetUserIDFromCookie(Request.Cookies["_UsuarioActual"]);
                return pIdUsuario == userID;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
