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
using WoSConnect.ROs.WoS;
using WoSConnect.ROs.WoS.Models;
using WoSConnect.ROs.WoS.Models.Inicial;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Linq.JObject;



namespace WoSConnect.ROs.WoS.Controllers
{
    public class ROWoSLogic : WoSInterface
    {
        protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        protected string baseUri { get; set; }


        // protected List<Publication> publications = new List<Publication>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROWoSLogic(string baseUri, string bareer)
        {
            this.baseUri = baseUri;
            this.bareer = bareer;

        }

        // TODO: Esto no se si abra que cambiarlo o no.... 
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
                    //request.Headers.TryAddWithoutValidation("X-ApiKey", bareer);
                    //request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");

                    if (headers != null && headers.Count > 0)
                    {
                        // if (headers.ContainsKey("Authorization"))
                        // {
                        //     request.Headers.TryAddWithoutValidation("Authorization", headers["Authorization"]);
                        // }
                        foreach (var item in headers)
                        {
                            request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    try
                    {
                        response = await httpClient.SendAsync(request);
                    }
                    catch (System.Exception)
                    {
                        throw new Exception("Error in the http call");
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

        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<Publication> getPublications(string name, string uri = "publications?orcid=0000-0001-8055-6823")
        {
            Uri url = new Uri(baseUri + string.Format(uri, name));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            // MODELO DEVUELTO 
            Console.Write(info_publication);
            Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
            // CAMBIO DE MODELO -- PAra ello llamamos al controlador de cambio de modelo! 
            ROWoSControllerJSON info = new ROWoSControllerJSON(this);
            List<Publication> sol = info.getListPublicatio(objInicial);
            return sol;
        }
    }
}
