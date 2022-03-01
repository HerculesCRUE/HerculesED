using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OAI_PMH.Services
{
    class Token
    {
        private static readonly string tokenUrl = "https://sgi.demo.treelogic.com/auth/realms/sgi/protocol/openid-connect/token";
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


        protected static async Task<string> httpCall(string pUrl, string pMethod, FormUrlEncodedContent pBody)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(pMethod), pUrl))
                {
                    request.Content = pBody;

                    int intentos = 3;
                    while (true)
                    {
                        try
                        {                            
                            response = await httpClient.SendAsync(request);
                            break;
                        }
                        catch
                        {
                            intentos--;
                            if (intentos == 0)
                            {
                                throw;
                            }
                            else
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
            }
            if (response.Content != null)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "";
            }
        }

        private static string GetToken()
        {
            Uri url = new Uri(tokenUrl);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("username", "visor-csp"),
                new KeyValuePair<string, string>("password", "visor-csp-2021"),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            string result = httpCall(url.ToString(), "POST", content).Result;

            var json = JObject.Parse(result);
            accessToken = json["access_token"].ToString();
            refreshToken = json["refresh_token"].ToString();
            lastUpdate = DateTime.UtcNow;

            return accessToken;
        }

        private static string RefreshToken()
        {
            Uri url = new Uri(tokenUrl);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            string result = httpCall(url.ToString(), "POST", content).Result;

            var json = JObject.Parse(result);
            accessToken = json["access_token"].ToString();

            return accessToken;
        }
    }
}