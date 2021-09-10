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
        /// <param name="ID">The user of the repositories</param>
        /// <param uri="uri">The uri for the call</param>
        // AU-ID ( "Buján, David"   24474045300 )
        /// <returns></returns>
        public string getStringPublication(string name, string uri = "content/abstract/scopus_id/{0}")//AU-ID?{0}")
        {
            Uri url = new Uri(baseUri + string.Format(uri, name));
            string info_publicationn = httpCall(url.ToString(), "GET", headers).Result;
            return info_publicationn;
        }

        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param id="scopus_ID">The user of the repositories</param>
        /// <param year="year">The user of the repositories</param>
        /// <param uri="uri">The uri for the call</param>
        // AU-ID ( "Buján, David"   24474045300 )
        /// <returns></returns>
        public List<Publication> getPublications(string name, string year = "1500", string uri = "content/search/scopus?query=AU-ID ( {0})&AFT({1})")//AU-ID?{0}")
        {
            Uri url = new Uri(baseUri + string.Format(uri, name, year));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            ROScopusControllerJSON info = new ROScopusControllerJSON(this);
            List<Publication> sol = info.getListPublicatio(info_publication);
            return sol;
        }
    }
}
