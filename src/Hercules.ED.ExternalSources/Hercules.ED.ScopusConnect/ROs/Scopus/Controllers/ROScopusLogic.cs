using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using ScopusConnect.ROs.Scopus.Models;
using ScopusConnect.ROs.Scopus.Models.Inicial;
using System.Threading;

namespace ScopusConnect.ROs.Scopus.Controllers
{
    public class ROScopusLogic : ScopusInterface
    {

        protected string bareer;

        protected Dictionary<string, string> headers = new Dictionary<string, string>();

        public static string apiKey = "75f4ab3fac56f42ac83cdeb7c98882ca";

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
                        catch
                        {
                            intentos--;
                            if (intentos == 0)
                            {
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
        /// <param name="uri"></param>
        /// <returns></returns>
        public List<Publication> getPublications(string name, string date = "1500-01-01", string uri = "content/search/scopus?query=ORCID(\"{0}\")&date={1}%&start={2}")//AU-ID?{0}")
        {
            // Fecha requerida para Scopus.
            string date_scopus = date.Substring(0, 4) + "-" + (DateTime.Now.Date.Year + 1).ToString();

            ROScopusControllerJSON info = new ROScopusControllerJSON(this);

            int n = 0;
            List<Publication> listaResultados = new List<Publication>();
            int result = 1;
            int cardinalidad = 1;
            while (cardinalidad >= result)
            {
                uri = "content/search/scopus?query=ORCID(\"{0}\")&apikey={1}&date={2}&start={3}";
                Uri url = new Uri("https://api.elsevier.com/" + string.Format(uri, name, apiKey, date_scopus, result.ToString()));
                n++;
                result = 200 * n;

                string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                if (!info_publication.StartsWith("{\"service-error\":"))
                {
                    Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
                    List<Publication> nuevas = info.getListPublicatio(objInicial, date);
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
        public Publication getPublicationDoi(string pDoi)
        {
            // Objeto publicación.
            Publication publicacionFinal = null;

            try
            {
                // Clase.
                ROScopusControllerJSON info = new ROScopusControllerJSON(this);

                // Petición.
                Uri url = new Uri($@"https://api.elsevier.com/content/search/scopus?apikey={apiKey}&query=DOI({pDoi})");
                string result = httpCall(url.ToString(), "GET", headers).Result;

                // Obtención de datos.
                if (!string.IsNullOrEmpty(result) && !result.StartsWith("{\"service-error\":"))
                {
                    Root objInicial = JsonConvert.DeserializeObject<Root>(result);
                    PublicacionInicial publicacionInicial = objInicial.SearchResults.entry[0];
                    publicacionFinal = info.cambioDeModeloPublicacion(publicacionInicial, true);
                }
            }
            catch (Exception)
            {
                return publicacionFinal;
            }

            return publicacionFinal;
        }

        public float getHIndex(string pOrcid)
        {
            try
            {
                // Clase.
                ROScopusControllerJSON info = new ROScopusControllerJSON(this);

                // Petición.
                Uri url = new Uri($@"https://api.elsevier.com/analytics/scival/author/orcid/{pOrcid}?apikey={apiKey}&httpAccept=application/json");
                string result = httpCall(url.ToString(), "GET", headers).Result;

                // Obtención de datos.
                if (!string.IsNullOrEmpty(result) && !result.StartsWith("{\"service-error\":"))
                {

                }
            }
            catch (Exception)
            {
                return 0;
            }

            return 0;
        }
    }
}
