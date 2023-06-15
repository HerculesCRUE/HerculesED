using Gnoss.ApiWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZenodoAPI.Controllers;
using ZenodoAPI.Middleware;
using ZenodoAPI.Models.Data;

namespace ZenodoAPI.Models
{
    public class Zenodo
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


        // Lista de tipos de ROs a obtener.
        private static readonly List<string> listaTiposROs = new List<string>() { "poster", "presentation", "dataset", "image", "video", "software", "lesson" };

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        public Zenodo(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Llamada.
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pMethod"></param>
        /// <param name="pHeader"></param>
        /// <param name="pBody"></param>
        /// <returns></returns>
        protected async Task<string> httpCall(string pUrl, string pMethod, Dictionary<string, string> pHeader = null, FormUrlEncodedContent pBody = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(pMethod), pUrl))
                {
                    if (pMethod == "GET")
                    {
                        if (pHeader != null && pHeader.Any())
                        {
                            foreach (KeyValuePair<string, string> item in pHeader)
                            {
                                request.Headers.Add(item.Key, item.Value);
                            }
                        }
                    }
                    else if (pMethod == "POST")
                    {
                        request.Content = pBody;
                    }

                    int intentos = 10;
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
                                Thread.Sleep(5000);
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
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtiene los datos de la fuente de Zenodo.
        /// </summary>
        /// <param name="pOrcid">ID ORCID.</param>
        /// <returns>Lista de datos.</returns>
        public List<ResearchObject> getPublicationsOrcid(string pOrcid)
        {
            List<ResearchObject> listaObjetos = new List<ResearchObject>();
            int page = 1;
            int size = 10;
            bool last = false;

            // Cabecera.
            Dictionary<string, string> head = new Dictionary<string, string>();
            head.Add("Accept", "application/json");

            while (!last)
            {
                Uri url = new Uri($@"{_Configuracion.GetUrlZenodo()}?q=creators.orcid:%22{pOrcid}%22&page={page}&size={size}");
                string result = httpCall(url.ToString(), "GET", pHeader: head).Result;
                List<ResearchObject> data = JsonConvert.DeserializeObject<List<ResearchObject>>(result);
                foreach (ResearchObject ro in data)
                {
                    if (listaTiposROs.Contains(ro.metadata.upload_type))
                    {
                        listaObjetos.Add(ro);
                    }
                }
                page++;

                // Última petición...
                if (data.Count < size)
                {
                    last = true;
                }
            }

            return listaObjetos;
        }

        /// <summary>
        /// Devuelve una lista de objetos con los datos necesarios para la carga en BBDD.
        /// </summary>
        /// <param name="pOrcid">Código ORCID de la persona a obtener información.</param>
        /// <returns>Lista de datos.</returns>
        public List<OntologyRO> getOntologyData(string pOrcid)
        {
            // Lista con los datos devueltos por Zenodo.
            List<ResearchObject> listaResearchObjects = getPublicationsOrcid(pOrcid);

            // Lista con los datos necesarios para la carga de ROs.
            List<OntologyRO> listaObjetosRos = new List<OntologyRO>();

            foreach (ResearchObject ro in listaResearchObjects)
            {
                OntologyRO data = new OntologyRO();

                // ID.
                data.id = ro.id;

                // DOI.
                if (!string.IsNullOrEmpty(ro.doi))
                {
                    data.doi = ro.doi;
                }

                // URL.
                if (ro.links != null && !string.IsNullOrEmpty(ro.links.html))
                {
                    data.url = ro.links.html;
                }

                // URL del archivo adjunto.
                if (ro.files != null && ro.files.Any())
                {
                    data.urlData = new List<string>();
                    foreach (File item in ro.files)
                    {
                        if (item.links != null && !string.IsNullOrEmpty(item.links.download))
                        {
                            data.urlData.Add(item.links.download);
                        }
                    }
                    if (!data.urlData.Any())
                    {
                        data.urlData = null;
                    }
                }

                if (ro.metadata != null)
                {
                    // Título.
                    if (!string.IsNullOrEmpty(ro.metadata.title))
                    {
                        data.titulo = ro.metadata.title;
                    }

                    // Tipo.
                    if (!string.IsNullOrEmpty(ro.metadata.upload_type))
                    {
                        data.tipo = ro.metadata.upload_type;
                    }

                    // Descripción.
                    if (!string.IsNullOrEmpty(ro.metadata.description))
                    {
                        data.descripcion = System.Web.HttpUtility.HtmlDecode(Regex.Replace(ro.metadata.description, "<.*?>", string.Empty));
                    }

                    // Fecha publicación.
                    if (!string.IsNullOrEmpty(ro.metadata.publication_date))
                    {
                        data.fechaPublicacion = ro.metadata.publication_date;
                    }

                    // Licencia.
                    if (!string.IsNullOrEmpty(ro.metadata.license))
                    {
                        data.licencia = ro.metadata.license;
                    }

                    // Personas.
                    if (ro.metadata.creators != null && ro.metadata.creators.Any())
                    {
                        List<Person> listaPersonas = new List<Person>();
                        foreach (Creator item in ro.metadata.creators)
                        {
                            Person persona = new Person();
                            if (!string.IsNullOrEmpty(item.orcid))
                            {
                                persona.orcid = item.orcid;
                            }
                            if (!string.IsNullOrEmpty(item.name))
                            {
                                persona.nombreCompleto = item.name;
                            }
                            listaPersonas.Add(persona);
                        }
                        data.autores = listaPersonas;
                    }

                    // Enriquecimiento
                    string dataEnriquecimiento = JsonConvert.SerializeObject(obtenerObjEnriquecimiento(ro));
                    data.etiquetasEnriquecidas = getDescriptores(dataEnriquecimiento, "specific");
                    if (data.etiquetasEnriquecidas != null && !data.etiquetasEnriquecidas.Any())
                    {
                        data.etiquetasEnriquecidas = null;
                    }
                    data.categoriasEnriquecidas = getDescriptores(dataEnriquecimiento, "thematic");
                    if (data.categoriasEnriquecidas != null && !data.categoriasEnriquecidas.Any())
                    {
                        data.categoriasEnriquecidas = null;
                    }
                }

                // Si no tiene título, no es un recurso válido.
                if (!string.IsNullOrEmpty(data.titulo))
                {
                    listaObjetosRos.Add(data);
                }
            }

            if (!listaObjetosRos.Any())
            {
                return null;
            }
            else
            {
                return listaObjetosRos;
            }
        }

        /// <summary>
        /// Contruye el objeto que utiliza el enriquecimiento.
        /// </summary>
        /// <param name="pRo">Datos necesarios para el enriquecimiento.</param>
        /// <returns>Objeto para el enriquecimiento.</returns>
        public ObjEnriquecimiento obtenerObjEnriquecimiento(ResearchObject pRo)
        {
            if (!string.IsNullOrEmpty(pRo.metadata.title) && !string.IsNullOrEmpty(pRo.metadata.description))
            {
                ObjEnriquecimiento objEnriquecimiento = new ObjEnriquecimiento();
                objEnriquecimiento.rotype = "papers";
                objEnriquecimiento.title = pRo.metadata.title;
                objEnriquecimiento.abstract_ = System.Web.HttpUtility.HtmlDecode(Regex.Replace(pRo.metadata.description, "<.*?>", string.Empty));
                return objEnriquecimiento;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene los descriptores temáticos y específicos que han sido enriquecidos.
        /// </summary>
        /// <param name="pDataEnriquecimiento">Datos para el proceso de enriquecimiento.</param>
        /// <param name="pTipo">Tipo: specific / thematic</param>
        /// <returns>Lista de descriptores enriquecidos.</returns>
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
