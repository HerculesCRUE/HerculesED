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
//using Newtonsoft.Json.Linq.JObject;



namespace ZenodoConnect.ROs.Zenodo.Controllers
{
    public class ROZenodoLogic : ZenodoInterface
    {
        protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        protected string baseUri { get; set; }


        // protected List<Publication> publications = new List<Publication>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROZenodoLogic(string baseUri)//, string bareer)
        {
            this.baseUri = baseUri;
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
                    Console.Write(headers);
                    //request.Headers.TryAddWithoutValidation("access_token", bareer);
                    // request.Headers.TryAddWithoutValidation("q", name);
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");
                    Console.Write(request);
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
                        return null;
                        //throw new Exception("Error in the http call");
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
            //https://zenodo.org/api/records/?q=doi:%2210.3217/jucs-022-07-0896%22
            Uri url = new Uri(baseUri + string.Format(uri, name));

            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            //Console.Write(info_publication);
            //string path = @"C:\Users\mpuer\Desktop\pruebaGNOSSSS.json";
            //File.WriteAllText(path, info_publication);
            //string[] h= info_publication.Split("[");
            //string a = info_publication.Remove(1);
            //string b = info_publication.Remove(1,info_publication.Count()-1);
            // MODELO DEVUELTO 
            try
            {
                Root_2 objInicial = JsonConvert.DeserializeObject<Root_2>("{\"data\":" + info_publication + "}");

                // CAMBIO DE MODELO -- PAra ello llamamos al controlador de cambio de modelo! 
                //ROZenodoControllerJSON info = new ROZenodoControllerJSON(this);
                // List<Publication> sol = info.getListPublicatio(objInicial);
                if (objInicial != null & objInicial != new Root_2())
                {
                    Console.Write("1\n");
                    if (objInicial.data != null & objInicial.data.Count > 0)
                    {
                        Console.Write("2\n");
                        if (objInicial.data[0] != null)
                        {
                            Console.Write("3\n");
                            if (objInicial.data[0].files != null & objInicial.data[0].files.Count > 0)
                            {
                                Console.Write("4\n");
                                if (objInicial.data[0].files[0] != null)
                                {
                                    Console.Write("5\n");
                                    if (objInicial.data[0].files[0].links != null)
                                    {
                                        Console.Write("6\n");
                                        if (objInicial.data[0].files[0].links.download != null)
                                        {
                                            Console.Write("7\n");
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
