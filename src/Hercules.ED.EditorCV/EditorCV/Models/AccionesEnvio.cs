using Gnoss.ApiWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EditorCV.Models
{
    public abstract class AccionesEnvio
    {
        #region --- Constantes   
        protected static string RUTA_OAUTH = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config";
        protected static ResourceApi mResourceApi = new ResourceApi(RUTA_OAUTH);
        protected static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Models{Path.DirectorySeparatorChar}Utils{Path.DirectorySeparatorChar}prefijos.json";
        protected static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));
        #endregion

        /// <summary>
        /// Obtención del token.
        /// </summary>
        /// <returns></returns>
        protected string GetTokenCSP(ConfigService pConfig)
        {
            Uri url = new Uri(pConfig.GetUrlToken());
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("username", pConfig.GetUsernameEsbCsp()),
                new KeyValuePair<string, string>("password", pConfig.GetPasswordEsbCsp()),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            string result = httpCall(url.ToString(), "POST", content).Result;
            var json = JObject.Parse(result);

            return json["access_token"].ToString();
        }

        /// <summary>
        /// Obtención del token.
        /// </summary>
        /// <returns></returns>
        protected string GetTokenPRC(ConfigService pConfig)
        {
            Uri url = new Uri(pConfig.GetUrlToken());
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("username", pConfig.GetUsernameEsbPrc()),
                new KeyValuePair<string, string>("password", pConfig.GetPasswordEsbPrc()),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            string result = httpCall(url.ToString(), "POST", content).Result;
            var json = JObject.Parse(result);

            return json["access_token"].ToString();
        }

        /// <summary>
        /// Llamada para la obtención del token.
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pMethod"></param>
        /// <param name="pBody"></param>
        /// <returns></returns>
        protected async Task<string> httpCall(string pUrl, string pMethod, FormUrlEncodedContent pBody)
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
                return string.Empty;
            }
        }
    }
}
