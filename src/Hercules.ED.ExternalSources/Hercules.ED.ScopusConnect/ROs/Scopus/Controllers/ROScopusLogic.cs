using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using ScopusConnect.ROs.Scopus.Models;
using ScopusConnect.ROs.Scopus.Models.Inicial;
using System.Threading;
using Gnoss.ApiWrapper;
using System.IO;

namespace ScopusConnect.ROs.Scopus.Controllers
{
    public class ROScopusLogic
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

        protected Dictionary<string, string> headers = new();

        private static readonly string apiKey = "75f4ab3fac56f42ac83cdeb7c98882ca";

        /// <summary>
        /// A Http calls function
        /// </summary>
        /// <param name="url">the http call url</param>
        /// <param name="method">Crud method for the call</param>
        /// <param name="headers">The headers for the call</param>
        /// <returns></returns>
        protected async Task<string> HttpCall(string url, string method = "GET", Dictionary<string, string> headers = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(method), url))
                {
                    request.Headers.Add("X-ELS-APIKey", apiKey);
                    request.Headers.Add("Accept", "application/json");

                    if (headers != null && headers.Count > 0)
                    {
                        foreach (var item in headers)
                        {
                            request.Headers.Add(item.Key, item.Value);
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
                        catch (Exception ex)
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
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<Publication> GetPublications(string name, string date)
        {
            // Fecha requerida para Scopus.
            string date_scopus = date.Substring(0, 4) + "-" + (DateTime.Now.Date.Year + 1).ToString();

            ROScopusControllerJSON info = new(this);

            int n = 0;
            List<Publication> listaResultados = new();
            int result = 1;
            int cardinalidad = 1;
            while (cardinalidad >= result)
            {
                string uri = "content/search/scopus?query=ORCID(\"{0}\")&apikey={1}&date={2}&start={3}";
                Uri url = new("https://api.elsevier.com/" + string.Format(uri, name, apiKey, date_scopus, result.ToString()));
                n++;
                result = 200 * n;

                string info_publication = HttpCall(url.ToString(), "GET", headers).Result;
                if (!info_publication.StartsWith("{\"service-error\":"))
                {
                    Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
                    List<Publication> nuevas = info.GetListPublications(objInicial);
                    listaResultados.AddRange(nuevas);
                    cardinalidad = 0;
                    if (objInicial.SearchResults.entry != null)
                    {
                        cardinalidad = objInicial.SearchResults.entry.Count;
                    }
                }
            }
            return listaResultados;
        }

        /// <summary>
        /// Obtiene una publicación mediante un DOI.
        /// </summary>
        /// <param name="pDoi">Identificador DOI de la publicación a obtener.</param>
        /// <returns>Publicación obtenida. (Null --> Error)</returns>
        public Publication GetPublicationDoi(string pDoi)
        {
            // Objeto publicación.
            Publication publicacionFinal = null;

            try
            {
                // Clase.
                ROScopusControllerJSON info = new(this);

                // Petición.
                Uri url = new($@"https://api.elsevier.com/content/search/scopus?apikey={apiKey}&query=DOI({pDoi})");
                string result = HttpCall(url.ToString(), "GET", headers).Result;

                // Obtención de datos.
                if (!string.IsNullOrEmpty(result) && !result.StartsWith("{\"service-error\":"))
                {
                    Root objInicial = JsonConvert.DeserializeObject<Root>(result);
                    PublicacionInicial publicacionInicial = objInicial.SearchResults.entry[0];
                    publicacionFinal = info.CambioDeModeloPublicacion(publicacionInicial);
                }
            }
            catch (Exception ex)
            {
                ResourceApi.Log.Error($@"[ERROR] {DateTime.Now} {ex.Message} {ex.StackTrace}");
                return publicacionFinal;
            }

            return publicacionFinal;
        }
    }
}
