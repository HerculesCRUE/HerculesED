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
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Linq.JObject;



namespace ScopusConnect.ROs.Scopus.Controllers
{
    public class ROScopusLogic : ScopusInterface
    {
        protected string bareer;
        ROScopusControllerJSON info = new ROScopusControllerJSON();
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
        /// <param name="user_orcid">The user of the repositories</param>
        /// <param name="uri">The uri for the call</param>
        /// <returns></returns>
        public List<Publication> getAllPublication(string user_orcid, string uri = "content/author/orcid/{0}")
        {
            List<Publication> hey = new List<Publication>();
            return hey;
        }


        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="author_id">The user of the repositories</param>
        /// <param name="uri">The uri for the call</param>
        /// <returns></returns>
        public Author_maite Author(string author_id, string uri = "content/author/author_id/{0}")
        { 

            Uri url = new Uri(baseUri + string.Format(uri, author_id));
            Console.Write(url);
            string info_author = httpCall(url.ToString(), "GET", headers).Result;
            Console.Write(info_author);
            JObject data = JObject.Parse(info_author);

            string objecto_aModelar = info.getInfoAuthor(data);
            
            //Console.Write(objecto_aModelar);
            //data["author-retrieval-response"];
            //JObject json = JObject.Parse(info_author);

            //Console.Write( data["author-retrieval-response"][0]);
            Author_maite correspondingAuthor = new Author_maite();
            //Console.Write(correspondingAuthor);
            //List<AuthorRetrievalResponse> a = new  List<AuthorRetrievalResponse>();

            try
            {
                correspondingAuthor = JsonConvert.DeserializeObject<Author_maite>(objecto_aModelar);
                //Console.Write(correspondingAuthor);
               // a= correspondingAuthor.AuthorRetrievalResponse;
                
            }
            catch (System.Exception)
            {
                throw new Exception("Error when deserialize the respositories: " + objecto_aModelar);
            }
            // Get all data from each repository
            //for (int i = 0; i < publications.Count; i++)
            //{
            //    //publications[i].
            //}
            return correspondingAuthor;
        }
         /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="doi">The user of the repositories</param>
        /// <param name="uri">The uri for the call</param>
        /// <returns></returns>
     //   public Publication Publication(string name, string uri = "")//
       public Publication Publication(string doi, string uri = "content/abstract/doi/{0}")
    { 

            Uri url = new Uri(baseUri + string.Format(uri, doi));
            Console.Write(url);
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Console.Write(info_publication);
            //info_publication.
            //JObject json = JObject.Parse(info_author);
            //Console.Write(info_publication);
            Root_Publication publication_info = new Root_Publication();
            Console.Write(info_publication);
            Publication a = new  Publication();

            try
            {
                publication_info = JsonConvert.DeserializeObject<Root_Publication>(info_publication);
                //Console.Write(publication_info);
                a= publication_info.AbstractsRetrievalResponse;
               
                
            }
            catch (System.Exception)
            {
                throw new Exception("Error when deserialize the respositories: " + info_publication);
            }

            return a;
        }
        public Publication Publication_2(string name, string uri = "content/search/scopus?query=AU-ID?{0}")
    { 

            Uri url = new Uri(baseUri + string.Format(uri, name));
            //Console.Write(url);
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Console.Write(info_publication);
            //info_publication.
            //JObject json = JObject.Parse(info_author);
            //Console.Write(info_publication);
            Root_Publication publication_info = new Root_Publication();
            Console.Write(info_publication);
            Publication a = new  Publication();

            try
            {
                publication_info = JsonConvert.DeserializeObject<Root_Publication>(info_publication);
                //Console.Write(publication_info);
                a= publication_info.AbstractsRetrievalResponse;
               
                
            }
            catch (System.Exception)
            {
                throw new Exception("Error when deserialize the respositories: " + info_publication);
            }

            return a;
        }


        public JsonResult getRepositoryLastUpdate(string repositoryId)
        {
            return new JsonResult("");
        }


    }
}
