using FigShareAPI.Controllers;
using FigShareAPI.Models.Data;
using Gnoss.ApiWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FigShareAPI.Models
{
    public class FigShare
    {
        private static string RUTA_OAUTH = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{System.IO.Path.DirectorySeparatorChar}ConfigOAuth{System.IO.Path.DirectorySeparatorChar}OAuthV3.config";
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
                        Console.WriteLine($"Contenido OAuth: {System.IO.File.ReadAllText(RUTA_OAUTH)}");
                        Thread.Sleep(10000);
                    }
                }
                return mResourceApi;
            }
        }

        // Configuración.
        readonly ConfigService _Configuracion;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        public FigShare(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Contrucción de la cabecera de envío.
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pToken"></param>
        /// <param name="pMethod"></param>
        /// <param name="pHeaders"></param>
        /// <returns></returns>
        protected async Task<string> httpCall(string pUrl, string pToken, string pMethod = "GET", Dictionary<string, string> pHeaders = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(pMethod), pUrl))
                {
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("Host", "api.figshare.com");
                    request.Headers.Add("Authorization", pToken);

                    if (pHeaders != null && pHeaders.Count > 0)
                    {
                        foreach (var item in pHeaders)
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
        /// Obtiene la lista de IDs de los ROs que son públicos.
        /// </summary>
        /// <param name="pToken">Token de usuario.</param>
        /// <returns>Lista de identificadores.</returns>
        public List<int> getIdentifiers(string pToken)
        {
            // Petición.
            Uri url = new Uri($@"{_Configuracion.GetUrlBase()}/account/articles");
            string result = httpCall(url.ToString(), pToken, pMethod: "GET").Result;
            List<ArticleScheme> listaArticulos = JsonConvert.DeserializeObject<List<ArticleScheme>>(result);

            // Obtención de IDs de los ROs que son PÚBLICOS.
            // TODO: De momento está puesto tanto para públicos como privados.
            List<int> listaIdentificadores = new List<int>();
            foreach (ArticleScheme articulo in listaArticulos)
            {
                if (articulo.published_date != null)
                {
                    listaIdentificadores.Add(articulo.id);
                }
            }

            return listaIdentificadores;
        }

        /// <summary>
        /// Obtiene los datos más detallados de los IDs de la lista.
        /// </summary>
        /// <param name="pListaIdentificadores">Lista de IDs a obtener los datos.</param>
        /// <param name="pToken">Token de usuario.</param>
        /// <returns>Lista de datos.</returns>
        public List<Article> getData(List<int> pListaIdentificadores, string pToken)
        {
            List<Article> listaArticulos = new List<Article>();

            foreach (int id in pListaIdentificadores)
            {
                // Petición.
                Uri url = new Uri($@"{_Configuracion.GetUrlBase()}/account/articles/{id}");
                string result = httpCall(url.ToString(), pToken, pMethod: "GET").Result;
                listaArticulos.Add(JsonConvert.DeserializeObject<Article>(result));
            }

            return listaArticulos;
        }

        /// <summary>
        /// Obtiene los datos necesarios para los RO en EDMA.
        /// </summary>
        /// <param name="pListaArticulos">Lista de articulos con los datos completos.</param>
        /// <returns>Lista con los datos necesarios.</returns>
        public List<RO> getROs(List<Article> pListaArticulos)
        {
            List<RO> listaROs = new List<RO>();
            foreach (Article articulo in pListaArticulos)
            {
                RO researchObject = new RO();
                if (articulo.id != null)
                {
                    researchObject.id = articulo.id;
                }
                if (!string.IsNullOrEmpty(articulo.defined_type_name))
                {
                    researchObject.tipo = articulo.defined_type_name;
                }
                if (!string.IsNullOrEmpty(articulo.title))
                {
                    researchObject.titulo = articulo.title;
                }
                if (!string.IsNullOrEmpty(articulo.description))
                {
                    researchObject.descripcion = Regex.Replace(articulo.description, "<.*?>", string.Empty);
                }
                if (!string.IsNullOrEmpty(articulo.figshare_url))
                {
                    researchObject.url = articulo.figshare_url;
                }
                if (articulo.files != null && articulo.files.Any())
                {
                    foreach (File item in articulo.files)
                    {
                        if (item.name.EndsWith(".pdf"))
                        {
                            researchObject.urlPdf = item.download_url;
                        }
                    }
                }
                if (articulo.published_date != null)
                {
                    researchObject.fechaPublicacion = articulo.published_date.ToString();
                }
                if (!string.IsNullOrEmpty(articulo.doi))
                {
                    researchObject.doi = articulo.doi;
                }
                if (articulo.tags != null && articulo.tags.Any())
                {
                    researchObject.etiquetas = articulo.tags;
                }
                if (articulo.authors != null && articulo.authors.Any())
                {
                    researchObject.autores = new List<Person>();
                    foreach (Author autor in articulo.authors)
                    {
                        Person person = new Person();
                        if (autor.id != null)
                        {
                            person.id = autor.id;
                        }
                        if (!string.IsNullOrEmpty(autor.full_name))
                        {
                            person.nombreCompleto = autor.full_name;
                        }
                        if (!string.IsNullOrEmpty(autor.orcid_id))
                        {
                            person.orcid = autor.orcid_id;
                        }
                        researchObject.autores.Add(person);
                    }
                }
                if (articulo.license != null && !string.IsNullOrEmpty(articulo.license.name))
                {
                    researchObject.licencia = articulo.license.name;
                }

                // Enriquecimiento
                string dataEnriquecimientoSinPdf = JsonConvert.SerializeObject(obtenerObjEnriquecimiento(researchObject));
                string dataEnriquecimientoConPdf = JsonConvert.SerializeObject(obtenerObjEnriquecimientoPdf(researchObject));               
                researchObject.etiquetasEnriquecidas = getDescriptores(dataEnriquecimientoSinPdf, "specific");
                if (researchObject.etiquetasEnriquecidas != null && !researchObject.etiquetasEnriquecidas.Any())
                {
                    researchObject.etiquetasEnriquecidas = null;
                }

                if (!string.IsNullOrEmpty(researchObject.urlPdf))
                {
                    researchObject.categoriasEnriquecidas = getDescriptores(dataEnriquecimientoConPdf, "thematic");
                }
                else
                {
                    researchObject.categoriasEnriquecidas = getDescriptores(dataEnriquecimientoSinPdf, "thematic");
                }                    
                if (researchObject.categoriasEnriquecidas != null && !researchObject.categoriasEnriquecidas.Any())
                {
                    researchObject.categoriasEnriquecidas = null;
                }

                listaROs.Add(researchObject);
            }
            if (listaROs.Any())
            {
                return listaROs;
            }
            else
            {
                return null;
            }
        }

        public ObjEnriquecimientoConPdf obtenerObjEnriquecimientoPdf(RO pRo)
        {
            if (!string.IsNullOrEmpty(pRo.titulo) && !string.IsNullOrEmpty(pRo.descripcion))
            {
                ObjEnriquecimientoConPdf objEnriquecimiento = new ObjEnriquecimientoConPdf();
                objEnriquecimiento.rotype = "papers";
                objEnriquecimiento.title = pRo.titulo;
                objEnriquecimiento.abstract_ = pRo.descripcion;

                if (!string.IsNullOrEmpty(pRo.urlPdf))
                {
                    objEnriquecimiento.pdfurl = pRo.urlPdf;
                }

                if (pRo.autores != null && pRo.autores.Any())
                {
                    string nombresAutores = string.Empty;
                    foreach (Person persona in pRo.autores)
                    {
                        nombresAutores += persona.nombreCompleto + " & ";
                    }
                    //objEnriquecimiento.author_name = nombresAutores.Substring(0, nombresAutores.LastIndexOf(" & "));
                }

                return objEnriquecimiento;
            }
            else
            {
                return null;
            }
        }

        public ObjEnriquecimientoSinPdf obtenerObjEnriquecimiento(RO pRo)
        {
            if (!string.IsNullOrEmpty(pRo.titulo) && !string.IsNullOrEmpty(pRo.descripcion))
            {
                ObjEnriquecimientoSinPdf objEnriquecimiento = new ObjEnriquecimientoSinPdf();
                objEnriquecimiento.rotype = "papers";
                objEnriquecimiento.title = pRo.titulo;
                objEnriquecimiento.abstract_ = pRo.descripcion;

                if (pRo.autores != null && pRo.autores.Any())
                {
                    string nombresAutores = string.Empty;
                    foreach (Person persona in pRo.autores)
                    {
                        nombresAutores += persona.nombreCompleto + " & ";
                    }
                    //objEnriquecimiento.author_name = nombresAutores.Substring(0, nombresAutores.LastIndexOf(" & "));
                }

                return objEnriquecimiento;
            }
            else
            {
                return null;
            }
        }

        public List<string> getDescriptores(string pDataEnriquecimiento, string pTipo)
        {
            // Petición.
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            string result = string.Empty;
            var contentData = new StringContent(pDataEnriquecimiento, System.Text.Encoding.UTF8, "application/json");

            int intentos = 3;
            while (true)
            {
                try
                {
                    response = client.PostAsync($@"{_Configuracion.GetUrlBaseEnriquecimiento()}/{pTipo}", contentData).Result;
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

            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
            }

            if (!string.IsNullOrEmpty(result))
            {
                Topics_enriquecidos data = null;
                try
                {
                    data = JsonConvert.DeserializeObject<Topics_enriquecidos>(result);
                }
                catch (Exception ex)
                {
                    ResourceApi.Log.Error($@"[ERROR] {DateTime.Now} {ex.Message} {ex.StackTrace}");
                    return null;
                }

                if (data != null)
                {
                    HashSet<string> listaTopics = new HashSet<string>();
                    foreach (Knowledge_enriquecidos item in data.topics)
                    {
                        listaTopics.Add(item.word);
                    }
                    return listaTopics.ToList();
                }
            }

            return null;
        }
    }
}
