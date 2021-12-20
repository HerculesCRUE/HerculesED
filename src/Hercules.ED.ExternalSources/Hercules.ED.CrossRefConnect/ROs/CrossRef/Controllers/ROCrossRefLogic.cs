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
//using Newtonsoft.Json.Linq.JObject;WoS
using CrossRefAPI.Controllers;


namespace CrossRefConnect.ROs.CrossRef.Controllers
{
    public class ROCrossRefLogic : CrossRefInterface
    {
        //protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        protected string baseUri { get; set; }
        readonly ConfigService _Configuracion;


        // protected List<Publication> publications = new List<Publication>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROCrossRefLogic(ConfigService pConfig)
        {

            _Configuracion = pConfig;
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
                return null;
            }

        }

        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="Doi"></param>
        /// <returns></returns>
        public Publication getPublications(string name, Boolean articulo_primer_order=true, string uri = "works/{0}")
        {
            Uri url = new Uri("https://api.crossref.org/" + string.Format(uri, name));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            // MODELO DEVUELTO 
            if(info_publication=="Resource not found." || info_publication.StartsWith("<html>")){
                return null;
            }
            Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
            // CAMBIO DE MODELO -- PAra ello llamamos al controlador de cambio de modelo! 
            ROCrossRefControllerJSON info = new ROCrossRefControllerJSON(this);
            Publication sol = info.cambioDeModeloPublicacion(objInicial.message, name, articulo_primer_order);
            return sol;
        }
    }
}
