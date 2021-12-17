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
using SemanticScholarConnect.ROs.SemanticScholar;
using SemanticScholarConnect.ROs.SemanticScholar.Models;
using SemanticScholarConnect.ROs.SemanticScholar.Models.Inicial;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Linq.JObject;



namespace SemanticScholarConnect.ROs.SemanticScholar.Controllers
{
    public class ROSemanticScholarLogic : SemanticScholarInterface
    {
        protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        protected string baseUri { get; set; }
   // public Dictionary<string, Tuple<string,string,string,string,string,string>>  autores_orcid; //= LeerDatosExcel_autores(@"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hercules-ED_autores.xlsx");



        // protected List<Publication> publications = new List<Publication>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROSemanticScholarLogic(string baseUri )//, string bareer)
        {
            this.baseUri = baseUri;
           // this.autores_orcid = autores_orcid;
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
        public Publication getPublications(string name, string uri = "graph/v1/paper/{0}?fields=externalIds,title,abstract,url,venue,year,referenceCount,citationCount,authors,authors.name,authors.externalIds")
        {
            Uri url = new Uri(baseUri + string.Format(uri, name));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            // MODELO DEVUELTO 
            Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
            // CAMBIO DE MODELO -- PAra ello llamamos al controlador de cambio de modelo! 
            ROSemanticScholarControllerJSON info = new ROSemanticScholarControllerJSON(this);
            Publication sol = info.cambioDeModeloPublicacion(objInicial);
            sol.doi=name;
           
            return sol;
        }
    }
}
