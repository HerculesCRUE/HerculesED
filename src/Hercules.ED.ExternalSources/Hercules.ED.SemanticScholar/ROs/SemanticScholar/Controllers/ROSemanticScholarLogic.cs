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
using SemanticScholarAPI.ROs.SemanticScholar.Models.Inicial;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using SemanticScholarAPI.ROs.SemanticScholar.Models;
using Gnoss.ApiWrapper;
using System.IO;

namespace SemanticScholarAPI.ROs.SemanticScholar.Controllers
{
    public class ROSemanticScholarLogic
    {
        private static string RUTA_OAUTH = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config";
        private static ResourceApi mResourceApi = null;

        private static ResourceApi ResourceApi
        {
            get
            {
                while (mResourceApi == null)
                {
                    try
                    {
                        mResourceApi = new ResourceApi(RUTA_OAUTH);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("No se ha podido iniciar ResourceApi");
                        Console.WriteLine($"Contenido OAuth: {File.ReadAllText(RUTA_OAUTH)}");
                        Thread.Sleep(10000);
                    }
                }
                return mResourceApi;
            }
        }

        protected string bareer;
       
        protected Dictionary<string, string> headers = new Dictionary<string, string>();

        public ROSemanticScholarLogic()
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
                        catch(Exception ex)
                        {
                            intentos--;
                            if (intentos == 0)
                            {
                                ResourceApi.Log.Error($@"[ERROR] {DateTime.Now} {ex.Message} {ex.StackTrace}");
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

        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Publication getPublications(string name, string uri = "graph/v1/paper/{0}?fields=externalIds,title,abstract,url,venue,year,referenceCount,citationCount,authors,authors.name,authors.externalIds")
        {
            Uri url = new Uri("https://api.semanticscholar.org/" + string.Format(uri, name));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
            ROSemanticScholarControllerJSON info = new ROSemanticScholarControllerJSON(this);
            Publication sol = info.cambioDeModeloPublicacion(objInicial);
            sol.doi = name;

            return sol;
        }


        public Tuple<Publication, List<PubReferencias>> getReferencias(string pDoi)
        {
            Tuple<Publication, List<PubReferencias>> tupla = null;
            Publication publicacionPrincipal = new Publication();
            List<PubReferencias> publications = new List<PubReferencias>();

            try
            {
                Uri url = new Uri($@"https://api.semanticscholar.org/v1/paper/{pDoi}");
                string result = httpCall(url.ToString(), "GET", headers).Result;
                Root objInicial = JsonConvert.DeserializeObject<Root>(result);
                SemanticScholarObj data = JsonConvert.DeserializeObject<SemanticScholarObj>(result);
                ROSemanticScholarControllerJSON info = new ROSemanticScholarControllerJSON(this);
                publicacionPrincipal = info.cambioDeModeloPublicacion(objInicial);
                publications = info.getReferences(data);
                tupla = new Tuple<Publication, List<PubReferencias>>(publicacionPrincipal, publications);
            }
            catch(Exception ex)
            {
                ResourceApi.Log.Error($@"[ERROR] {DateTime.Now} {ex.Message} {ex.StackTrace}");
            }

            return tupla;
        }
    }
}
