using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using ZenodoConnect.ROs.Zenodo.Models.Inicial;
using System.Threading;

namespace ZenodoConnect.ROs.Zenodo.Controllers
{
    public class ROZenodoLogic : ZenodoInterface
    {
        protected string bareer;

        protected Dictionary<string, string> headers = new Dictionary<string, string>();

        public ROZenodoLogic()
        {
        }

        /// <summary>
        /// A Http calls function
        /// </summary>
        /// <param name="url">the http call url</param>
        /// <param name="method">Crud method for the call</param>
        /// <param name="headers">The headers for the call</param>
        /// <returns></returns>
        protected async Task<string> httpCall(string url, string method = "GET", Dictionary<string, string> headers = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(method), url))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");
                    if (headers != null && headers.Count > 0)
                    {
                        foreach (var item in headers)
                        {
                            request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }

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
                                return null;
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
                return null;
            }

        }

        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string getPublications(string name)
        {
            Uri url = new Uri("https://zenodo.org/api/records/?q=doi:\"" + name + "\"");

            string info_publication = httpCall(url.ToString(), "GET", headers).Result;

            try
            {
                Root_2 objInicial = JsonConvert.DeserializeObject<Root_2>("{\"data\":" + info_publication + "}");

                if (objInicial != null && objInicial != new Root_2() &&
                    objInicial.data != null && objInicial.data.Count > 0 && objInicial.data[0] != null &&
                    objInicial.data[0].files != null && objInicial.data[0].files.Count > 0 && objInicial.data[0].files[0] != null &&
                    objInicial.data[0].files[0].links != null && objInicial.data[0].files[0].links.download != null &&
                    objInicial.data[0].files[0].links.download.EndsWith(".pdf"))
                {
                    return objInicial.data[0].files[0].links.download;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
