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
using PublicationConnect.ROs.Publications;
using PublicationConnect.ROs.Publications.Models;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using PublicationConnect.Controllers;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;
//using Newtonsoft.Json.Linq.JObject;



namespace PublicationConnect.ROs.Publications.Controllers
{
    public class ROPublicationLogic : PublicationInterface
    {
        protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        protected string baseUri { get; set; }


        // protected List<Publication> publications = new List<Publication>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROPublicationLogic(string baseUri)
        {

            this.baseUri = "http://localhost:5000/WoS/GetROs?orcid={0}";

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
        public List<Publication> getPublications(string name)
        {
            //consulta a WoS
            Console.Write(string.Format("http://localhost:5000/WoS/GetROs?orcid={0}", name));
            Uri url = new Uri(string.Format(baseUri, name));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            List<Publication> objInicial_woS = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            foreach (Publication pub in objInicial_woS)
            {
                string doi = pub.doi;
                url = new Uri(string.Format("http://localhost:5002/OpenCitations/GetROs?doi={0}", doi));
                info_publication = httpCall(url.ToString(), "GET", headers).Result;
                Publication objInicial_OpenCitatons = JsonConvert.DeserializeObject<Publication>(info_publication);
                if(objInicial_OpenCitatons.bibliografia!=null){
                List<Publication> bibliografia = new List<Publication>();
                foreach (Publication pub_bib in objInicial_OpenCitatons.bibliografia)
                {
                    doi = pub_bib.doi;
                    url = new Uri(string.Format("http://localhost:5003/SemanticScholar/GetROs?doi={0}", doi));
                    info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    Console.Write(info_publication);
                    Publication objInicial_SemanticScholar = JsonConvert.DeserializeObject<Publication>(info_publication);

                    bibliografia.Add(objInicial_SemanticScholar);
                    

                }
                pub.bibliografia = bibliografia;
                }
                if(objInicial_OpenCitatons.citas!=null){
                List<Publication> citas = new List<Publication>();
                foreach (Publication pub_cita in objInicial_OpenCitatons.citas)
                {
                    doi = pub_cita.doi;
                    url = new Uri(string.Format("http://localhost:5003/SemanticScholar/GetROs?doi={0}", doi));
                    info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    Publication objInicial_SemanticScholar = JsonConvert.DeserializeObject<Publication>(info_publication);
                    if(objInicial_SemanticScholar!=null){
                    citas.Add(objInicial_SemanticScholar);
                    }
                }
                pub.citas = citas;
                }
                
            }
            //consulta a WoS

            // CAMBIO DE MODELO -- PAra ello llamamos al controlador de cambio de modelo! 
            //ROPublicationControllerJSON info = new ROPublicationControllerJSON(this);
            //List<Publication> sol = info.getListPublicatio();
            return objInicial_woS;
        }
    }
}
