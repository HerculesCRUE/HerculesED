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
using CrossRefConnect.ROs.CrossRef;
using CrossRefConnect.ROs.CrossRef.Models;
using CrossRefConnect.ROs.CrossRef.Models.Inicial;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using CrossRefAPI.ROs.CrossRef.Models;
using Gnoss.ApiWrapper;
using System.IO;
//using Newtonsoft.Json.Linq.JObject;WoS


namespace CrossRefConnect.ROs.CrossRef.Controllers
{
    public class ROCrossRefLogic
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

        //protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        protected string baseUri { get; set; }


        // protected List<Publication> publications = new List<Publication>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROCrossRefLogic()
        {
            //this.bareer = bareer;
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
                    // request.Headers.TryAddWithoutValidation("X-ApiKey", bareer);
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
                return null;
            }

        }

        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="pDoi"></param>
        /// <param name="pUri"></param>
        /// <returns></returns>
        public List<PubReferencias> getPublications(string pDoi, string pUri = "works/{0}")
        {
            Uri url = new Uri("https://api.crossref.org/" + string.Format(pUri, pDoi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;

            if (info_publication == "Resource not found." || info_publication.StartsWith("<html>"))
            {
                return null;
            }

            Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
            ROCrossRefControllerJSON info = new ROCrossRefControllerJSON(this);
            return info.ObtenerReferencias(objInicial.message);
        }

        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="pDoi"></param>
        /// <param name="pUri"></param>
        /// <returns></returns>
        public Root getEnrichmentPublication(string pDoi, string pUri = "works/{0}")
        {
            Uri url = new Uri("https://api.crossref.org/" + string.Format(pUri, pDoi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;

            if (info_publication == "Resource not found." || info_publication.StartsWith("<html>"))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Root>(info_publication);            
        }
    }
}
