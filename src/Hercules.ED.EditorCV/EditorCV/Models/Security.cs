using Gnoss.ApiWrapper;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using EditorCV.Models.Utils;

namespace EditorCV.Models
{
    public static class Security
    {
        static UserApi mUserApi = UtilityCV.mUserApi;

        public static bool CheckUser(Guid pUserId,HttpRequest pHttpRequest)
        {
            if (pUserId == Guid.Empty)
            {
                return false;
            }
            string cookie = pHttpRequest.Cookies["_UsuarioActual"];
            Guid userIdCookie = Guid.Empty;
            try
            {
                userIdCookie = mUserApi.GetUserIDFromCookie(cookie);
            }
            catch (Exception)
            {
                //
            }
            return userIdCookie==pUserId;
        }

        public static bool CheckUsers(List<Guid> pUsersId, HttpRequest pHttpRequest)
        {
            if (pUsersId==null || pUsersId.Count==0)
            {
                return false;
            }
            string cookie = pHttpRequest.Cookies["_UsuarioActual"];
            Guid userIdCookie = Guid.Empty;
            try
            {
                userIdCookie = mUserApi.GetUserIDFromCookie(cookie);
            }
            catch (Exception)
            {
                //
            }
            return pUsersId.Contains(userIdCookie);
        }
    }
}
