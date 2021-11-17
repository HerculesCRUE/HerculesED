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
                    request.Headers.TryAddWithoutValidation("X-ApiKey", bareer);
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
        /// <param date="Year-Moth-day"></param>

        /// <returns></returns>
        public List<Publication> getPublications(string name, string date="1500-01-01", string uri = "api/wos/?databaseId=WOK&usrQuery=AI=({0})&count=100&firstRecord={1}&publishTimeSpan={2}%2B2022-12-31")
        {
            ROWoSControllerJSON info = new ROWoSControllerJSON(this);
            int n = 0;
            List<Publication> sol = new List<Publication>();
            int result = 1;
            int cardinalidad =1;
            while (cardinalidad >= result )
            {
                Uri url = new Uri(baseUri + string.Format(uri, name, result.ToString(),date));
                n++;
                result = 100 * n;
                string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                //Console.Write(info_publication);
                Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
                cardinalidad = objInicial.Data.Records.records.REC.Count();
                List<Publication> nuevas = info.getListPublicatio(objInicial);
                sol.AddRange(nuevas);
            }
           
            return sol;
        }
    }
}
