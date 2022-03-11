using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using ZenodoConnect.ROs.Zenodo;
using ZenodoConnect.ROs.Zenodo.Models;
using ZenodoConnect.ROs.Zenodo.Models.Inicial;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using ZenodoAPI.ROs.Zenodo.Models;
//using Newtonsoft.Json.Linq.JObject;

namespace ZenodoConnect.ROs.Zenodo.Controllers
{
    public class ROZenodoLogic : ZenodoInterface
    {
        protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        //protected string baseUri { get; set; }


        // protected List<Publication> publications = new List<Publication>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();

        public ROZenodoLogic()
        {
            //this.bareer = bareer;
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
                                //throw;
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
        /// <param name="ID"></param>
        /// <returns></returns>?access_token=ACCESS_TOKEN
        public string getPublications(string name, string uri = "?q=doi:\"{0}\"")
        {
            Uri url = new Uri("https://zenodo.org/api/records/" + string.Format(uri, name));

            string info_publication = httpCall(url.ToString(), "GET", headers).Result;

            try
            {
                Root_2 objInicial = JsonConvert.DeserializeObject<Root_2>("{\"data\":" + info_publication + "}");

                 if (objInicial != null & objInicial != new Root_2())
                {
                    if (objInicial.data != null & objInicial.data.Count > 0)
                    {
                        if (objInicial.data[0] != null)
                        {
                            if (objInicial.data[0].files != null & objInicial.data[0].files.Count > 0)
                            {
                                if (objInicial.data[0].files[0] != null)
                                {
                                    if (objInicial.data[0].files[0].links != null)
                                    {
                                        if (objInicial.data[0].files[0].links.download != null)
                                        {
                                            if (objInicial.data[0].files[0].links.download.EndsWith(".pdf"))
                                            {
                                                return objInicial.data[0].files[0].links.download;
                                            }
                                            else { return null; }
                                        }
                                    }
                                }

                            }
                        }

                    }
                }
                return null;
            }
            catch { return null; }
        }
    }
}
