using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Services
{
    class Token
    {
        private static readonly string tokenUrl = "http://sgi.ic.corp.treelogic.com/auth/realms/sgi/protocol/openid-connect/token";
        private static string accessToken;
        private static string refreshToken;
        private static DateTime lastUpdate;

        public static string CheckToken()
        {
            if (lastUpdate != default)
            {
                TimeSpan diff = DateTime.UtcNow.Subtract(lastUpdate);
                if (diff.TotalSeconds > 300 && diff.TotalSeconds < 1800)
                {
                    accessToken = RefreshToken();
                }
            }
            else
            {
                accessToken = GetToken();
            }
            return accessToken;
        }

        private static string GetToken()
        {
            var client = new RestClient(tokenUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "client_id=front&username=visor-csp&password=visor-csp&grant_type=password", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            accessToken = json["access_token"].ToString();
            refreshToken = json["refresh_token"].ToString();
            lastUpdate = DateTime.UtcNow;
            return accessToken;
        }

        private static string RefreshToken()
        {
            var client = new RestClient(tokenUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "client_id=front&grant_type=refresh_token&refresh_token=" + refreshToken, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            accessToken = json["access_token"].ToString();
            return accessToken;
        }
    }
}