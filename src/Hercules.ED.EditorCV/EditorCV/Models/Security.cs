using Gnoss.ApiWrapper;
using System;
using Microsoft.AspNetCore.Http;

namespace EditorCV.Models
{
    public static class Security
    {
        static UserApi mUserApi = new UserApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        public static bool CheckUser(Guid pUserId,HttpRequest pHttpRequest)
        {
            string cookie = pHttpRequest.Cookies["_UsuarioActual"];
            Guid userIdCookie = Guid.Empty;
            try
            {
                userIdCookie = mUserApi.GetUserIDFromCookie(cookie);
            }
            catch (Exception ex)
            {
                
            }
            return userIdCookie==pUserId;
        }
    }
}
