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
using ScopusConnect.ROs.Scopus;
using ScopusConnect.ROs.Scopus.Models;
using ScopusConnect.ROs.Scopus.Models.Inicial;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Linq.JObject;

namespace ScopusConnect.ROs.Scopus.Controllers
{
    public class ROScopusLogic : ScopusInterface
    {
        protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        protected string baseUri { get; set; }


        // protected List<Publication> publications = new List<Publication>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROScopusLogic(string baseUri, string bareer)
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
                    request.Headers.TryAddWithoutValidation("X-ELS-APIKey", bareer);
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
        /// <param date="date">year-month-day</param>

        /// <returns></returns>
        public List<Publication> getPublications(string name, string date = "1500-01-01", string uri = "content/search/scopus?query=ORCID(\"{0}\")&count=200&date={1}%&start={2}")//AU-ID?{0}")
        {
            string date_scopus = date.Substring(0, 4) + "-" + (DateTime.Now.Date.Year+1).ToString();
            
            ROScopusControllerJSON info = new ROScopusControllerJSON(this);
        
            int n = 0;
            List<Publication> sol = new List<Publication>();
            int result = 1;
            int cardinalidad =1;
            while (cardinalidad >= result )
            {
    
                uri = "content/search/scopus?query=ORCID(\"{0}\")&count=200&date={1}&start={2}";
                Uri url = new Uri(baseUri + string.Format(uri, name, date_scopus, result.ToString()));
                n=n+1;
                result = 200*n;

                String info_publication = httpCall(url.ToString(), "GET", headers).Result;
                //Console.Write(info_publication);
                if (info_publication != "{\"service-error\":{\"status\":{\"statusCode\":\"INVALID_INPUT\",\"statusText\":\"Error translating query\"}}}")
                {            
                    Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
                    List<Publication> nuevas = info.getListPublicatio(objInicial, date);
                    sol.AddRange(nuevas);
                    cardinalidad = objInicial.SearchResults.entry.Count;
                }
            }
            return sol;
        }
    }
}
