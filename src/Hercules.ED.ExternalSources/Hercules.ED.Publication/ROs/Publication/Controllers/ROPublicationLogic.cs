using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using PublicationConnect.ROs.Publications.Models;
using System.Text;
using PublicationAPI.Controllers;
using Serilog;
using PublicationAPI.ROs.Publication.Models;
using System.Threading;
using Person = PublicationConnect.ROs.Publications.Models.Person;
using System.Text.RegularExpressions;

namespace PublicationConnect.ROs.Publications.Controllers
{
    /// <summary>
    /// ROPublicationLogic.
    /// </summary>
    public class ROPublicationLogic
    {
        // Listado de DOIs.
        private readonly List<string> dois_principales = new();

        // Headers para las peticiones.
        private readonly Dictionary<string, string> headers = new();

        // Configuración.
        readonly ConfigService _Configuracion;

        public ROPublicationLogic(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

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
                httpClient.Timeout = TimeSpan.FromHours(24);
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
                    try
                    {
                        response = await httpClient.SendAsync(request);
                    }
                    catch (Exception)
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
                return String.Empty;
            }
        }

        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="pOrcid">Código ORCID del autor.</param>
        /// <param name="pDate">Fecha de obtención de publicaciones.</param>
        /// <param name="pDoi">Código DOI de la publicación.</param>
        /// <param name="pNombreCompletoAutor">Nombre completo del autor.</param>
        /// <returns></returns>
        public List<Publication> GetPublications(string pOrcid, string pDate = "1500-01-01", string pDoi = null, string pNombreCompletoAutor = null)
        {
            // Diccionario con las peticiones.
            Dictionary<string, string> dicZenodo = new();

            // Lista para almacenar las publicaciones resultantes.
            List<Publication> resultado = new();

            // Lista con los datos obtenidos por fuentes externas.
            List<PublicacionScopus> objInicial_Scopus = null;
            List<Publication> objInicial_woS = null;
            List<Publication> objInicial_openAire = null;

            if (pDoi != null) // Recuperar una publicación por DOI.
            {
                try
                {
                    Console.WriteLine("Haciendo petición a Wos...");
                    objInicial_woS = LlamadaWoSDoi(pDoi);
                }
                catch (Exception error)
                {
                    Console.WriteLine("No se ha podido recuperar los datos de Wos...");
                    Log.Error("No se ha podido recuperar los datos de Wos...");
                    Log.Error(error.Message);
                }

                try
                {
                    Console.WriteLine("Haciendo petición a Scopus...");
                    objInicial_Scopus = LlamadaScopusDoi(pDoi);
                }
                catch (Exception error)
                {
                    Console.WriteLine("No se ha podido recuperar los datos de Scopus...");
                    Log.Error("No se ha podido recuperar los datos de Scopus...");
                    Log.Error(error.Message);
                }

                try
                {
                    Console.WriteLine("Haciendo petición a OpenAire...");
                    objInicial_openAire = LlamadaOpenAireDoi(pDoi);
                }
                catch (Exception error)
                {
                    Console.WriteLine("No se ha podido recuperar los datos de OpenAire...");
                    Log.Error("No se ha podido recuperar los datos de OpenAire...");
                    Log.Error(error.Message);
                }
            }
            else // Recuperar las publicaciones de un autor desde 'X' fecha.
            {
                try
                {
                    Console.WriteLine("Haciendo petición a Wos...");
                    objInicial_woS = LlamadaWoS(pOrcid, pDate);
                }
                catch (Exception error)
                {
                    Console.WriteLine("No se ha podido recuperar los datos de Wos...");
                    Log.Error("No se ha podido recuperar los datos de Wos...");
                    Log.Error(error.Message);
                }

                try
                {
                    Console.WriteLine("Haciendo petición a Scopus...");
                    objInicial_Scopus = LlamadaScopus(pOrcid, pDate);
                }
                catch (Exception error)
                {
                    Console.WriteLine("No se ha podido recuperar los datos de Scopus...");
                    Log.Error("No se ha podido recuperar los datos de Scopus...");
                    Log.Error(error.Message);
                }

                try
                {
                    Console.WriteLine("Haciendo petición a OpenAire...");
                    objInicial_openAire = LlamadaOpenAire(pOrcid, pDate);
                }
                catch (Exception error)
                {
                    Console.WriteLine("No se ha podido recuperar los datos de OpenAire...");
                    Log.Error("No se ha podido recuperar los datos de OpenAire...");
                    Log.Error(error.Message);
                }
            }

            #region --- WOS
            int contadorPubWos = 1;
            try
            {
                if (objInicial_woS != null && objInicial_woS.Any())
                {
                    foreach (Publication pub in objInicial_woS)
                    {
                        Console.WriteLine($@"[WoS] Publicación {contadorPubWos}/{objInicial_woS.Count}");

                        // Inserción de DOI en la lista de DOIs.
                        if (pub.Doi != null && !string.IsNullOrEmpty(pub.Doi))
                        {
                            this.dois_principales.Add(pub.Doi.ToLower());
                        }

                        Publication pub_completa = pub;

                        // SemanticScholar.
                        ConsultaSemanticScholar(pub, ref pub_completa);

                        // Zenodo - Archivos pdf...
                        ConsultaZenodo(pub, ref pub_completa, dicZenodo);

                        // Completar información faltante con las publicaciones de Scopus.
                        if (objInicial_Scopus != null && objInicial_Scopus.Any())
                        {
                            foreach (PublicacionScopus pub_scopus in objInicial_Scopus)
                            {
                                if (pub_scopus != null)
                                {
                                    Publication pubScopus = ObtenerPublicacionDeScopus(pub_scopus);
                                    if (pub_scopus.doi != null && !string.IsNullOrEmpty(pub_completa.Doi) && pub_scopus.doi.ToLower() == pub_completa.Doi.ToLower())
                                    {
                                        pub_completa = Compactacion(pub_completa, pubScopus);
                                    }
                                }
                            }
                        }

                        // Completar información faltante con las publicaciones de OpenAire.
                        if (objInicial_openAire != null && objInicial_openAire.Any())
                        {
                            foreach (Publication pub_openAire in objInicial_openAire)
                            {
                                if (pub_openAire != null && pub_openAire.Doi != null && !string.IsNullOrEmpty(pub_completa.Doi) && pub_openAire.Doi.ToLower() == pub_completa.Doi.ToLower())
                                {
                                    pub_completa = Compactacion(pub_completa, pub_openAire);
                                }
                            }
                        }

                        // Unificar Autores.
                        pub_completa = CompararAutores(pub_completa);

                        // Se guarda la publicación.
                        resultado.Add(pub_completa);

                        contadorPubWos++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.Error(e.Message);
            }

            Console.WriteLine($@"[WoS] Publicaciones procesadas");
            #endregion

            #region --- Scopus
            int contadoPubScopus = 1;
            try
            {
                if (objInicial_Scopus != null && objInicial_Scopus.Any())
                {
                    // Llamada Scopus para completar publicaciones que no se hayan obtenido de WoS.
                    foreach (PublicacionScopus pub_scopus in objInicial_Scopus)
                    {
                        Console.WriteLine($@"[Scopus] Publicación {contadoPubScopus}/{objInicial_Scopus.Count}");
                        if (pub_scopus != null && !string.IsNullOrEmpty(pub_scopus.doi) && !dois_principales.Contains(pub_scopus.doi.ToLower()))
                        {
                            Publication pubScopus = ObtenerPublicacionDeScopus(pub_scopus);

                            if (pub_scopus.doi != null && !string.IsNullOrEmpty(pub_scopus.doi))
                            {
                                this.dois_principales.Add(pub_scopus.doi.ToLower());
                            }

                            Publication pub_completa = pubScopus;

                            // SemanticScholar.
                            ConsultaSemanticScholar(pubScopus, ref pub_completa);

                            // Zenodo - Archivos pdf...
                            ConsultaZenodo(pubScopus, ref pub_completa, dicZenodo);

                            // Completar información faltante con las publicaciones de OpenAire.
                            if (objInicial_openAire != null && objInicial_openAire.Any())
                            {
                                foreach (Publication pub_openAire in objInicial_openAire)
                                {
                                    if (pub_openAire != null && pub_openAire.Doi != null && !string.IsNullOrEmpty(pub_completa.Doi) && pub_openAire.Doi.ToLower() == pub_completa.Doi.ToLower())
                                    {
                                        pub_completa = Compactacion(pub_completa, pub_openAire);
                                    }
                                }
                            }

                            // Unificar Autores.
                            pub_completa = CompararAutores(pub_completa);

                            // Se guarda la publicación.
                            resultado.Add(pub_completa);
                        }

                        contadoPubScopus++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.Error(e.Message);
            }

            Console.WriteLine($@"[Scopus] Publicaciones procesadas");
            #endregion

            #region --- OpenAire
            int contadorPubOpenAire = 1;
            try
            {
                if (objInicial_openAire != null && objInicial_openAire.Any())
                {
                    // Llamada OpenAire para completar publicaciones que no se hayan obtenido de WoS/Scopus.
                    foreach (Publication pub in objInicial_openAire)
                    {
                        Console.WriteLine($@"[OpenAire] Publicación {contadorPubOpenAire}/{objInicial_openAire.Count}");
                        if (pub != null && !string.IsNullOrEmpty(pub.Doi) && !dois_principales.Contains(pub.Doi.ToLower()))
                        {
                            if (pub.Doi != null && !string.IsNullOrEmpty(pub.Doi))
                            {
                                this.dois_principales.Add(pub.Doi.ToLower());
                            }

                            Publication pub_completa = pub;

                            // SemanticScholar.
                            ConsultaSemanticScholar(pub, ref pub_completa);

                            // Zenodo - Archivos pdf...
                            ConsultaZenodo(pub, ref pub_completa, dicZenodo);

                            // Unificar Autores.
                            pub_completa = CompararAutores(pub_completa);

                            // Se guarda la publicación.
                            resultado.Add(pub_completa);
                        }
                        contadorPubOpenAire++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.Error(e.Message);
            }

            Console.WriteLine($@"[OpenAire] Publicaciones procesadas");
            #endregion

            // Comprobación de que las publicaciones estén bien formadas.
            List<Publication> listaPubsFinal = new();
            foreach (Publication publicacion in resultado)
            {
                if (publicacion != null && !string.IsNullOrEmpty(publicacion.Title) && publicacion.SeqOfAuthors != null && publicacion.SeqOfAuthors.Any() && publicacion.Title != "One or more validation errors occurred.")
                {
                    if (!string.IsNullOrEmpty(pNombreCompletoAutor))
                    {
                        bool encontrado = false;
                        foreach (Person persona in publicacion.SeqOfAuthors)
                        {
                            if (persona.ORCID == pOrcid || pDoi != null)
                            {
                                encontrado = true;
                                break;
                            }
                        }

                        if (encontrado)
                        {
                            listaPubsFinal.Add(publicacion);
                        }
                    }
                    else
                    {
                        listaPubsFinal.Add(publicacion);
                    }
                }
            }

            // Enriquecimiento.
            foreach (Publication publicacion in listaPubsFinal)
            {
                // Limpieza del título y descripción.
                publicacion.Title = Regex.Replace(publicacion.Title, "<.*?>", string.Empty);
                if (publicacion.Abstract != null)
                {
                    publicacion.Abstract = Regex.Replace(publicacion.Abstract, "<.*?>", string.Empty);
                }

                // Preparación del objeto a enviar para el enriquecimiento.
                string jsonData = string.Empty;
                if (string.IsNullOrEmpty(publicacion.Pdf))
                {
                    jsonData = JsonConvert.SerializeObject(ObtenerObjEnriquecimiento(publicacion));
                }
                else
                {
                    jsonData = JsonConvert.SerializeObject(ObtenerObjEnriquecimientoPdf(publicacion));
                }

                if (!string.IsNullOrEmpty(jsonData))
                {
                    // Peticiones.
                    Console.WriteLine("Obteniendo topics enriquecidos...");
                    Dictionary<string, string> listaTopics = GetDescriptores(jsonData, "thematic");
                    Console.WriteLine("Obteniendo freeTextKeywords enriquecidos...");
                    Dictionary<string, string> listaEtiquetas = GetDescriptores(jsonData, "specific");

                    if (listaTopics != null && listaTopics.Any())
                    {
                        publicacion.Topics_enriquecidos = new List<KnowledgeEnriquecidos>();
                        foreach (KeyValuePair<string, string> item in listaTopics)
                        {
                            KnowledgeEnriquecidos topic = new();
                            topic.Word = item.Key;
                            topic.Porcentaje = item.Value;
                            publicacion.Topics_enriquecidos.Add(topic);
                        }
                    }
                    else
                    {
                        publicacion.Topics_enriquecidos = null;
                    }

                    if (listaEtiquetas != null && listaEtiquetas.Any())
                    {
                        publicacion.FreetextKeyword_enriquecidas = new List<KnowledgeEnriquecidos>();
                        foreach (KeyValuePair<string, string> item in listaEtiquetas)
                        {
                            KnowledgeEnriquecidos topic = new();
                            topic.Word = item.Key;
                            topic.Porcentaje = item.Value;
                            publicacion.FreetextKeyword_enriquecidas.Add(topic);
                        }
                    }
                    else
                    {
                        publicacion.FreetextKeyword_enriquecidas = null;
                    }
                }
            }

            // Comprobar si está el nombre entre los autores (Editor CV, carga de publicaciones).
            if (!string.IsNullOrEmpty(pNombreCompletoAutor))
            {
                float umbral = 0.5f;
                bool valido = false;

                foreach (Publication publicacion in listaPubsFinal)
                {
                    foreach (Person persona in publicacion.SeqOfAuthors)
                    {
                        string nombreCompleto = persona.Name.Nombre_completo.First();

                        // Comprueba si el nombre del autor corresponde con alguno de la publicación.
                        float resultadoSimilaridad = GetNameSimilarity(pNombreCompletoAutor, nombreCompleto);

                        if (resultadoSimilaridad >= umbral)
                        {
                            valido = true;
                            break;
                        }
                    }
                }

                // Si no supera el umbral, es que no hemos reconocido a la persona por el nombre.
                if (!valido)
                {
                    listaPubsFinal = new List<Publication>();
                }
            }

            return listaPubsFinal;
        }

        /// <summary>
        /// Construye los datos obtenidos de SemanticScholar.
        /// </summary>
        /// <param name="pPubLectura">Publicación inicial.</param>
        /// <param name="pPubCompleta">Publicación resultante.</param>
        public void ConsultaSemanticScholar(Publication pPubLectura, ref Publication pPubCompleta)
        {
            int contadorSemanticScholar = 1;
            Tuple<Publication, List<PubReferencias>> dataSemanticScholar = new(null, null);
            while (dataSemanticScholar.Item2 == null && contadorSemanticScholar <= 5)
            {
                Console.WriteLine($@"[WoS] Haciendo petición a SemanticScholar ({contadorSemanticScholar})...");
                dataSemanticScholar = LlamadaRefSemanticScholar(pPubLectura.Doi);
                contadorSemanticScholar++;

                // Si se obtienen datos, es válido.
                if (dataSemanticScholar == null || !string.IsNullOrEmpty(dataSemanticScholar.Item1.Title) && dataSemanticScholar.Item2 == null)
                {
                    break;
                }

                // En el caso que venga vacío, esperamos 10 segundos a volverlo a intentar.
                if (dataSemanticScholar.Item2 == null && contadorSemanticScholar != 5)
                {
                    Thread.Sleep(10000);
                }
            }

            Console.WriteLine("[WoS] Comparación (SemanticScholar)...");
            pPubCompleta.DataOriginList = new HashSet<string>() { "WoS" };
            if (dataSemanticScholar != null && dataSemanticScholar.Item2 != null)
            {
                pPubCompleta = Compactacion(pPubLectura, dataSemanticScholar.Item1);
                pPubCompleta.Bibliografia = dataSemanticScholar.Item2;
            }
        }

        /// <summary>
        /// Construye los datos obtenidos de Zenodo.
        /// </summary>
        /// <param name="pPubLectura">Publicación inicial.</param>
        /// <param name="pPubCompleta">Publicación resultante.</param>
        /// <param name="pDicZenodo">Diccionario de Zenodo.</param>
        public void ConsultaZenodo(Publication pPubLectura, ref Publication pPubCompleta, Dictionary<string, string> pDicZenodo)
        {
            Console.WriteLine("[WoS] Haciendo petición a Zenodo...");
            pPubCompleta.Pdf = LlamadaZenodo(pPubLectura.Doi, pDicZenodo);
            if (pPubCompleta.Pdf == string.Empty)
            {
                pPubCompleta.Pdf = null;
            }
            else
            {
                pPubCompleta.DataOriginList.Add("Zenodo");
            }
        }

        /// <summary>
        /// Compara las dos publicaciones por parámetro y las une dándole prioridad a la primera.
        /// </summary>
        /// <param name="pub_1">Publicación número uno (con prioridad).</param>
        /// <param name="pub_2">Publicación número dos.</param>
        /// <returns></returns>
        public Publication Compactacion(Publication pub_1, Publication pub_2)
        {
            Publication pub = new();
            pub.DataOriginList = pub_1.DataOriginList;

            if (pub_1 == null && pub_2 == null)
            {
                return null;
            }
            if (pub_1 == null && pub_2 != null)
            {
                return pub_2;
            }
            if (pub_1 != null && pub_2 == null)
            {
                return pub_1;
            }


            bool pub1 = false;
            bool pub2 = false;
            if (!string.IsNullOrEmpty(pub_1.TypeOfPublication))
            {
                pub.TypeOfPublication = pub_1.TypeOfPublication;
                pub1 = true;
            }
            else
            {
                pub.TypeOfPublication = pub_2.TypeOfPublication;
                pub2 = true;
            }

            if (!string.IsNullOrEmpty(pub_1.Title))
            {
                pub.Title = pub_1.Title;
                pub1 = true;
            }
            else
            {
                pub.Title = pub_2.Title;
                pub2 = true;
            }

            if (!string.IsNullOrEmpty(pub_1.Abstract))
            {
                pub.Abstract = pub_1.Abstract;
                pub1 = true;
            }
            else
            {
                pub.Abstract = pub_2.Abstract;
                pub2 = true;
            }

            if (pub_1.FreetextKeywords != null && pub_1.FreetextKeywords.Any())
            {
                pub.FreetextKeywords = pub_1.FreetextKeywords;
                pub1 = true;
                if (pub_2.FreetextKeywords != null)
                {
                    pub.FreetextKeywords.AddRange(pub_2.FreetextKeywords);
                    pub2 = true;
                }
            }
            else
            {
                pub.FreetextKeywords = pub_2.FreetextKeywords;
                pub2 = true;
            }

            if (!string.IsNullOrEmpty(pub_1.Language))
            {
                pub.Language = pub_1.Language;
                pub1 = true;
            }
            else
            {

                pub.Language = pub_2.Language;
                pub2 = true;
            }

            // Si es un capitulo de libro, no necesita DOI (Da problemas en el motor de desambiguación).
            if (pub.TypeOfPublication != "Chapter")
            {
                if (pub_1.Doi != null)
                {
                    pub.Doi = pub_1.Doi;
                    pub1 = true;
                }
                else
                {
                    pub.Doi = pub_2.Doi;
                    pub2 = true;
                }
            }

            if (pub_1.DataIssued != null)
            {
                pub.DataIssued = pub_1.DataIssued;
                pub1 = true;
            }
            else
            {
                if (pub_2.DataIssued != null)
                {
                    pub.DataIssued = pub_2.DataIssued;
                    pub2 = true;
                }
                else { pub.DataIssued = null; }
            }
            if (pub_1.Url != null && pub_1.Url.Any())
            {
                pub.Url = pub_1.Url;
                pub1 = true;
                if (pub_2.Url != null)
                {
                    foreach (string item in pub_2.Url)
                    {
                        pub.Url.Add(item);
                        pub2 = true;
                    }
                }
            }
            else
            {
                pub.Url = pub_2.Url;
                pub2 = true;
            }
            if (pub_1.CorrespondingAuthor != null)
            {
                pub.CorrespondingAuthor = pub_1.CorrespondingAuthor;
                pub1 = true;
            }
            else
            {
                pub.CorrespondingAuthor = pub_2.CorrespondingAuthor;
                pub2 = true;
            }

            pub.SeqOfAuthors = new List<Models.Person>();
            if (pub_1.SeqOfAuthors != null && pub_1.SeqOfAuthors.Count > 0)
            {
                pub.SeqOfAuthors.AddRange(pub_1.SeqOfAuthors);
                pub1 = true;
            }
            if (pub_2.SeqOfAuthors != null && pub_2.SeqOfAuthors.Count > 0)
            {
                pub.SeqOfAuthors.AddRange(pub_2.SeqOfAuthors);
                pub2 = true;
            }

            if (pub_1.HasKnowledgeAreas != null && pub_1.HasKnowledgeAreas.Any())
            {
                pub.HasKnowledgeAreas = pub_1.HasKnowledgeAreas;
                pub1 = true;
                if (pub_2.HasKnowledgeAreas != null)
                {
                    pub.HasKnowledgeAreas.AddRange(pub_2.HasKnowledgeAreas);
                    pub2 = true;
                }
            }
            else
            {
                pub.HasKnowledgeAreas = pub_2.HasKnowledgeAreas;
                pub2 = true;
            }

            if (!string.IsNullOrEmpty(pub_1.PageEnd))
            {
                pub.PageEnd = pub_1.PageEnd;
                pub1 = true;
            }
            else
            {
                pub.PageEnd = pub_2.PageEnd;
                pub2 = true;
            }

            if (!string.IsNullOrEmpty(pub_1.PageStart))
            {
                pub.PageStart = pub_1.PageStart;
                pub1 = true;
            }
            else
            {
                pub.PageStart = pub_2.PageStart;
                pub2 = true;
            }

            if (!string.IsNullOrEmpty(pub_1.Volume))
            {
                pub.Volume = pub_1.Volume;
                pub1 = true;
            }
            else
            {
                pub.Volume = pub_2.Volume;
                pub2 = true;
            }

            if (!string.IsNullOrEmpty(pub_1.ArticleNumber))
            {
                pub.ArticleNumber = pub_1.ArticleNumber;
                pub1 = true;
            }
            else
            {
                pub.ArticleNumber = pub_2.ArticleNumber;
                pub2 = true;
            }

            if (pub_1.OpenAccess != null)
            {
                pub.OpenAccess = pub_1.OpenAccess;
                pub1 = true;
            }
            else
            {
                pub.OpenAccess = pub_2.OpenAccess;
                pub2 = true;
            }

            if (pub_1.IDs != null && pub_1.IDs.Any())
            {
                pub.IDs = pub_1.IDs;
                pub1 = true;
                if (pub_2.IDs != null)
                {
                    pub.IDs.AddRange(pub_2.IDs);
                    pub2 = true;
                }
            }
            else
            {
                pub.IDs = pub_2.IDs;
                pub2 = true;
            }

            if (!string.IsNullOrEmpty(pub_1.PresentedAt))
            {
                pub.PresentedAt = pub_1.PresentedAt;
                pub1 = true;
            }
            else
            {
                pub.PresentedAt = pub_2.PresentedAt;
                pub2 = true;
            }

            if (pub_1.Conferencia != null)
            {
                pub.Conferencia = pub_1.Conferencia;
                pub1 = true;
            }
            else
            {
                pub.Conferencia = pub_2.Conferencia;
                pub2 = true;
            }

            Dictionary<string, Models.PublicationMetric> dicMetricas = new();

            if (pub_1.HasMetric != null && pub_1.HasMetric.Any())
            {
                foreach (Models.PublicationMetric metrica in pub_1.HasMetric)
                {
                    if (!dicMetricas.ContainsKey(metrica.MetricName))
                    {
                        dicMetricas.Add(metrica.MetricName, metrica);
                        pub1 = true;
                    }
                }
            }
            if (pub_2.HasMetric != null && pub_2.HasMetric.Any())
            {
                foreach (Models.PublicationMetric metrica in pub_2.HasMetric)
                {
                    if (!dicMetricas.ContainsKey(metrica.MetricName))
                    {
                        dicMetricas.Add(metrica.MetricName, metrica);
                        pub2 = true;
                    }
                }
            }

            pub.HasMetric = new List<Models.PublicationMetric>();
            foreach (KeyValuePair<string, Models.PublicationMetric> metrica in dicMetricas)
            {
                pub.HasMetric.Add(metrica.Value);
            }

            pub.HasPublicationVenue = new Models.Source();
            HashSet<string> listaIssn = new();

            if (pub_1.HasPublicationVenue != null && !string.IsNullOrEmpty(pub_1.HasPublicationVenue.Type))
            {
                pub.HasPublicationVenue.Type = pub_1.HasPublicationVenue.Type;
                pub1 = true;
            }
            else if (pub_2.HasPublicationVenue != null && !string.IsNullOrEmpty(pub_2.HasPublicationVenue.Type))
            {
                pub.HasPublicationVenue.Type = pub_2.HasPublicationVenue.Type;
                pub2 = true;
            }

            if (pub_1.HasPublicationVenue != null && !string.IsNullOrEmpty(pub_1.HasPublicationVenue.Eissn))
            {
                pub.HasPublicationVenue.Eissn = pub_1.HasPublicationVenue.Eissn;
                pub1 = true;
            }
            else if (pub_2.HasPublicationVenue != null && !string.IsNullOrEmpty(pub_2.HasPublicationVenue.Eissn))
            {
                pub.HasPublicationVenue.Eissn = pub_2.HasPublicationVenue.Eissn;
                pub2 = true;
            }

            if (pub_1.HasPublicationVenue != null && !string.IsNullOrEmpty(pub_1.HasPublicationVenue.Name))
            {
                pub.HasPublicationVenue.Name = pub_1.HasPublicationVenue.Name;
                pub1 = true;
            }
            else if (pub_2.HasPublicationVenue != null && !string.IsNullOrEmpty(pub_2.HasPublicationVenue.Name))
            {
                pub.HasPublicationVenue.Name = pub_2.HasPublicationVenue.Name;
                pub2 = true;
            }

            if (pub_1.HasPublicationVenue != null && pub_1.HasPublicationVenue.Issn != null && pub_1.HasPublicationVenue.Issn.Count > 0)
            {
                foreach (string item in pub_1.HasPublicationVenue.Issn)
                {
                    if (item != pub.HasPublicationVenue.Eissn)
                    {
                        listaIssn.Add(item);
                        pub1 = true;
                    }
                }
            }

            if (pub_2.HasPublicationVenue != null && pub_2.HasPublicationVenue.Issn != null && pub_2.HasPublicationVenue.Issn.Count > 0)
            {
                foreach (string item in pub_2.HasPublicationVenue.Issn)
                {
                    if (item != pub.HasPublicationVenue.Eissn)
                    {
                        listaIssn.Add(item);
                        pub2 = true;
                    }
                }
            }

            pub.HasPublicationVenue.Issn = listaIssn.ToList();

            if (!string.IsNullOrEmpty(pub_1.Pdf))
            {
                pub.Pdf = pub_1.Pdf;
                pub1 = true;
            }
            else
            {
                pub.Pdf = pub_2.Pdf;
                pub2 = true;
            }

            if (pub_1.Topics_enriquecidos != null && pub_1.Topics_enriquecidos.Any())
            {
                pub.Topics_enriquecidos = pub_1.Topics_enriquecidos;
                pub1 = true;
            }
            else
            {
                pub.Topics_enriquecidos = pub_2.Topics_enriquecidos;
                pub2 = true;
            }


            if (pub_1.FreetextKeyword_enriquecidas != null && pub_1.FreetextKeyword_enriquecidas.Any())
            {
                pub.FreetextKeyword_enriquecidas = pub_1.FreetextKeyword_enriquecidas;
                pub1 = true;
            }
            else
            {
                pub.FreetextKeyword_enriquecidas = pub_2.FreetextKeyword_enriquecidas;
                pub2 = true;
            }

            if (pub_1.Bibliografia != null && pub_1.Bibliografia.Any())
            {
                pub.Bibliografia = pub_1.Bibliografia;
                pub1 = true;
            }
            else
            {
                pub.Bibliografia = pub_2.Bibliografia;
                pub2 = true;
            }

            if (pub1 && pub_1.DataOrigin != null)
            {
                pub.DataOriginList.Add(pub_1.DataOrigin);
            }
            if (pub2)
            {
                pub.DataOriginList.Add(pub_2.DataOrigin);
            }

            return pub;
        }

        /// <summary>
        /// Hace la llamada al API de SemanticScholar.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación a consultar.</param>
        /// <returns>Objeto Publication con los datos obtenidos.</returns>
        public Tuple<Publication, List<PubReferencias>> LlamadaRefSemanticScholar(string pDoi)
        {
            Tuple<Publication, List<PubReferencias>> objInicial_SemanticScholar = null;

            try
            {
                if (!string.IsNullOrEmpty(pDoi))
                {
                    // URL a la petición.
                    Uri url = new(string.Format(_Configuracion.GetUrlSemanticScholar() + "SemanticScholar/GetReferences?pDoi={0}", pDoi));

                    string info_publication = HttpCall(url.ToString(), "GET", headers).Result;
                    objInicial_SemanticScholar = JsonConvert.DeserializeObject<Tuple<Publication, List<PubReferencias>>>(info_publication);
                }
            }
            catch (Exception e)
            {
                Log.Error("Petición SemanticScholar --> " + e);
                return objInicial_SemanticScholar;
            }

            return objInicial_SemanticScholar;
        }

        /// <summary>
        /// Llamada al API de Web of Science mediante un código de autor.
        /// </summary>
        /// <param name="pOrcid">ORCID del autor.</param>
        /// <param name="date">Fecha.</param>
        /// <returns>Publicación(es) con los datos obtenidos.</returns>
        public List<PublicacionScopus> LlamadaScopus(string pOrcid, string date)
        {
            Uri url = new(string.Format(_Configuracion.GetUrlScopus() + "Scopus/GetROs?orcid={0}&date={1}", pOrcid, date));
            string info_publication = HttpCall(url.ToString(), "GET", headers).Result;
            List<PublicacionScopus> objInicial_Scopus = null;
            try
            {
                objInicial_Scopus = JsonConvert.DeserializeObject<List<PublicacionScopus>>(info_publication);
            }
            catch (Exception e)
            {
                Log.Error("Petición Scopus --> " + e);
                return objInicial_Scopus;
            }
            return objInicial_Scopus;
        }

        /// <summary>
        /// Llamada al API de Scopus mediante un código DOI.
        /// </summary>
        /// <param name="pDoi">ID de la publicación.</param>
        /// <returns>Publicación(es) con los datos obtenidos.</returns>
        public List<PublicacionScopus> LlamadaScopusDoi(string pDoi)
        {
            Uri url = new(string.Format(_Configuracion.GetUrlScopus() + "Scopus/GetPublicationByDOI?pDoi={0}", pDoi));
            string info_publication = HttpCall(url.ToString(), "GET", headers).Result;
            List<PublicacionScopus> objInicial_Scopus = null;
            try
            {
                PublicacionScopus publicacion = JsonConvert.DeserializeObject<PublicacionScopus>(info_publication);
                objInicial_Scopus = new List<PublicacionScopus>() { publicacion };
            }
            catch (Exception e)
            {
                Log.Error("Petición Scopus --> " + e);
                return objInicial_Scopus;
            }
            return objInicial_Scopus;
        }

        /// <summary>
        /// Llamada al API de Web of Science mediante un código de autor.
        /// </summary>
        /// <param name="pOrcid">ORCID del autor.</param>
        /// <param name="date">Fecha.</param>
        /// <returns>Publicación(es) con los datos obtenidos.</returns>
        public List<Publication> LlamadaWoS(string pOrcid, string date)
        {
            Uri url = new(string.Format(_Configuracion.GetUrlWos() + "WoS/GetROs?orcid={0}&date={1}", pOrcid, date));

            int contadorVeces = 0;
            string info_publication;
            while (true)
            {
                info_publication = HttpCall(url.ToString(), "GET", headers).Result;
                if (contadorVeces == 5 || !string.IsNullOrEmpty(info_publication))
                {
                    break;
                }
                contadorVeces++;
                Thread.Sleep(3000);
            }

            List<Publication> objInicial_woS = null;
            try
            {
                objInicial_woS = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            }
            catch (Exception e)
            {
                Log.Error("Petición WoS --> " + e);
                return objInicial_woS;
            }
            return objInicial_woS;
        }

        /// <summary>
        /// Llamada al API de Web of Science mediante un código DOI.
        /// </summary>
        /// <param name="pDoi">ID de la publicación.</param>
        /// <returns>Publicación(es) con los datos obtenidos.</returns>
        public List<Publication> LlamadaWoSDoi(string pDoi)
        {
            Uri url = new(string.Format(_Configuracion.GetUrlWos() + "WoS/GetRoByDoi?pDoi={0}", pDoi));
            string info_publication = HttpCall(url.ToString(), "GET", headers).Result;
            List<Publication> objInicial_woS = null;
            try
            {
                Publication publicacion = JsonConvert.DeserializeObject<Publication>(info_publication);
                if (publicacion != null)
                {
                    objInicial_woS = new List<Publication>() { publicacion };
                }
            }
            catch (Exception e)
            {
                Log.Error("Petición WoS --> " + e);
                return objInicial_woS;
            }
            return objInicial_woS;
        }

        /// <summary>
        /// Llamada al API de OpenAire mediante un código de autor.
        /// </summary>
        /// <param name="pOrcid">ORCID del autor.</param>
        /// <param name="date">Fecha.</param>
        /// <returns>Publicación(es) con los datos obtenidos.</returns>
        public List<Publication> LlamadaOpenAire(string pOrcid, string date)
        {
            Uri url = new(string.Format(_Configuracion.GetUrlOpenAire() + "OpenAire/GetROs?orcid={0}&date={1}", pOrcid, date));
            string info_publication = HttpCall(url.ToString(), "GET", headers).Result;
            List<Publication> objInicial_openAire = null;
            try
            {
                objInicial_openAire = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            }
            catch (Exception e)
            {
                Log.Error("Petición OpenAire --> " + e);
                return objInicial_openAire;
            }
            return objInicial_openAire;
        }

        /// <summary>
        /// Llamada al API de OpenAire mediante un código DOI.
        /// </summary>
        /// <param name="pDoi">ID de la publicación.</param>
        /// <returns>Publicación(es) con los datos obtenidos.</returns>
        public List<Publication> LlamadaOpenAireDoi(string pDoi)
        {
            Uri url = new(string.Format(_Configuracion.GetUrlOpenAire() + "OpenAire/GetRoByDoi?pDoi={0}", pDoi));
            string info_publication = HttpCall(url.ToString(), "GET", headers).Result;
            List<Publication> objInicial_openAire = null;
            try
            {
                Publication publicacion = JsonConvert.DeserializeObject<Publication>(info_publication);
                if (publicacion != null)
                {
                    objInicial_openAire = new List<Publication>() { publicacion };
                }
            }
            catch (Exception e)
            {
                Log.Error("Petición OpenAire --> " + e);
                return objInicial_openAire;
            }
            return objInicial_openAire;
        }

        /// <summary>
        /// Hace la llamada al API de Zenodo.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación a consultar.</param>
        /// <param name="pDic">Diccionario con los objetos de Zenodo.</param>
        /// <returns>String con la URL del archivo PDF obtenido.</returns>
        public string LlamadaZenodo(string pDoi, Dictionary<string, string> pDic)
        {
            string urlPdf = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(pDoi))
                {
                    // URL a la petición.
                    Uri url = new(string.Format(_Configuracion.GetUrlZenodo() + "Zenodo/GetROs?ID={0}", pDoi));

                    // Comprobación de la petición.
                    if (!pDic.ContainsKey(pDoi))
                    {
                        string info_publication = HttpCall(url.ToString(), "GET", headers).Result.Replace("\"", "");
                        if (!string.IsNullOrEmpty(info_publication) && info_publication.EndsWith(".pdf"))
                        {
                            urlPdf = info_publication;
                        }
                        pDic[pDoi] = urlPdf;
                    }
                    else
                    {
                        urlPdf = pDic[pDoi];
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Petición Zenodo --> " + e);
                return urlPdf;
            }

            return urlPdf;
        }

        /// <summary>
        /// Mediante una publicación, compara todos sus autores entre ellos para quitar duplicados y fusionar la información.
        /// En Scopus, no nos vienen los autores.
        /// </summary>
        /// <param name="pPublicacion">Publicación a fusionar autores.</param>
        /// <returns>Publicación con los autores fusionados.</returns>
        public Publication CompararAutores(Publication pPublicacion)
        {
            // Prioridad: WOS > OpenAire > SemanticScholar
            Dictionary<string, List<Person>> dicPersonas = new();
            dicPersonas.Add("WoS", new List<Person>());
            dicPersonas.Add("OpenAire", new List<Person>());
            dicPersonas.Add("SemanticScholar", new List<Person>());

            // Peso.
            double umbral = 0.7;

            foreach (Person persona in pPublicacion.SeqOfAuthors)
            {
                if (persona.Name != null)
                {
                    dicPersonas[persona.Fuente].Add(persona);
                }
            }

            List<Person> listaPersonasDefinitivas = new();

            // Unir personas 
            foreach (Person persona in dicPersonas["WoS"])
            {
                Person personaFinal = persona;

                foreach (Person personaCrossRef in dicPersonas["OpenAire"])
                {
                    // Comprobación por ORCID
                    if (!string.IsNullOrEmpty(personaFinal.ORCID) && personaFinal.ORCID == personaCrossRef.ORCID)
                    {
                        personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                        break;
                    }

                    // Comprobración por nombre completo
                    ComprobarNombreCompleto(personaFinal);

                    string nombreCompleto2 = string.Empty;
                    if (personaCrossRef.Name.Given != null && personaCrossRef.Name.Given.Any())
                    {
                        nombreCompleto2 += personaCrossRef.Name.Given[0] + " ";
                    }
                    if (personaCrossRef.Name.Familia != null && personaCrossRef.Name.Familia.Any())
                    {
                        nombreCompleto2 += personaCrossRef.Name.Familia[0];
                    }
                    if (!string.IsNullOrEmpty(nombreCompleto2))
                    {
                        personaCrossRef.Name.Nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                    }

                    if (personaFinal.Name.Nombre_completo != null && personaFinal.Name.Nombre_completo.Any() && personaCrossRef.Name.Nombre_completo != null && personaCrossRef.Name.Nombre_completo.Any() && GetNameSimilarity(personaFinal.Name.Nombre_completo[0], personaCrossRef.Name.Nombre_completo[0]) >= umbral)
                    {
                        personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                        break;
                    }

                    if (personaFinal.Name.Given != null && personaFinal.Name.Given.Any() && !string.IsNullOrEmpty(personaFinal.Name.Given[0]) && personaFinal.Name.Familia != null && personaFinal.Name.Familia.Any() && !string.IsNullOrEmpty(personaFinal.Name.Familia[0]))
                    {
                        personaFinal.Name.Nombre_completo[0] = $@"{personaFinal.Name.Given[0]} {personaFinal.Name.Familia[0]}";
                    }
                }

                foreach (Person personaSemantic in dicPersonas["SemanticScholar"])
                {
                    // Comprobación por ORCID
                    if (!string.IsNullOrEmpty(personaFinal.ORCID) && personaFinal.ORCID == personaSemantic.ORCID)
                    {
                        personaFinal = UnirPersonas(personaFinal, personaSemantic);
                        break;
                    }

                    // Comprobración por nombre completo
                    string nombreCompleto1 = string.Empty;
                    if (personaSemantic.Name.Given != null && personaFinal.Name.Given != null && personaFinal.Name.Given.Any())
                    {
                        nombreCompleto1 += personaFinal.Name.Given[0] + " ";
                    }
                    if (personaFinal.Name.Familia != null && personaFinal.Name.Familia.Any())
                    {
                        nombreCompleto1 += personaFinal.Name.Familia[0];
                    }
                    if (!string.IsNullOrEmpty(nombreCompleto1))
                    {
                        personaFinal.Name.Nombre_completo = new List<string>() { nombreCompleto1.Trim() };
                    }

                    string nombreCompleto2 = string.Empty;
                    if (personaSemantic.Name.Given != null && personaSemantic.Name.Given.Any())
                    {
                        nombreCompleto2 += personaSemantic.Name.Given[0] + " ";
                    }
                    if (personaSemantic.Name.Familia != null && personaSemantic.Name.Familia.Any())
                    {
                        nombreCompleto2 += personaSemantic.Name.Familia[0];
                    }
                    if (!string.IsNullOrEmpty(nombreCompleto2))
                    {
                        personaSemantic.Name.Nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                    }

                    if (personaFinal.Name.Nombre_completo != null && personaFinal.Name.Nombre_completo.Any() && personaSemantic.Name.Nombre_completo != null && personaSemantic.Name.Nombre_completo.Any() && GetNameSimilarity(personaFinal.Name.Nombre_completo[0], personaSemantic.Name.Nombre_completo[0]) >= umbral)
                    {
                        personaFinal = UnirPersonas(personaFinal, personaSemantic);
                        break;
                    }

                    personaFinal.Name.Nombre_completo[0] = $@"{personaFinal.Name.Given[0]} {personaFinal.Name.Familia[0]}";
                }

                listaPersonasDefinitivas.Add(personaFinal);
            }

            // Encontrar el autor
            foreach (Person persona in listaPersonasDefinitivas)
            {
                Person personaFinal = persona;

                // Comprobación por ORCID
                if (!string.IsNullOrEmpty(personaFinal.ORCID) && personaFinal.ORCID == pPublicacion.CorrespondingAuthor.ORCID)
                {
                    pPublicacion.CorrespondingAuthor = personaFinal;
                    break;
                }

                // Comprobración por nombre completo
                ComprobarNombreCompleto(personaFinal);

                string nombreCompleto2 = string.Empty;
                if (pPublicacion.CorrespondingAuthor.Name.Given != null && pPublicacion.CorrespondingAuthor.Name.Given.Any())
                {
                    nombreCompleto2 += pPublicacion.CorrespondingAuthor.Name.Given[0] + " ";
                }
                if (pPublicacion.CorrespondingAuthor.Name.Familia != null && pPublicacion.CorrespondingAuthor.Name.Familia.Any())
                {
                    nombreCompleto2 += pPublicacion.CorrespondingAuthor.Name.Familia[0];
                }
                if (!string.IsNullOrEmpty(nombreCompleto2))
                {
                    pPublicacion.CorrespondingAuthor.Name.Nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                }

                if (personaFinal.Name.Nombre_completo != null && personaFinal.Name.Nombre_completo.Any() && pPublicacion.CorrespondingAuthor.Name.Nombre_completo != null && pPublicacion.CorrespondingAuthor.Name.Nombre_completo.Any() && GetNameSimilarity(personaFinal.Name.Nombre_completo[0], pPublicacion.CorrespondingAuthor.Name.Nombre_completo[0]) >= umbral)
                {
                    pPublicacion.CorrespondingAuthor = personaFinal;
                    break;
                }

                if (!string.IsNullOrEmpty(pPublicacion.CorrespondingAuthor.Nick) && GetNameSimilarity(personaFinal.Name.Nombre_completo[0], pPublicacion.CorrespondingAuthor.Nick) >= 0.01)
                {
                    pPublicacion.CorrespondingAuthor = UnirPersonas(personaFinal, pPublicacion.CorrespondingAuthor);
                    break;
                }
                if (personaFinal.Name.Given != null && personaFinal.Name.Given.Any() && !string.IsNullOrEmpty(personaFinal.Name.Given[0]) && personaFinal.Name.Familia != null && personaFinal.Name.Familia.Any() && !string.IsNullOrEmpty(personaFinal.Name.Familia[0]))
                {
                    personaFinal.Name.Nombre_completo[0] = $@"{personaFinal.Name.Given[0]} {personaFinal.Name.Familia[0]}";
                }
            }

            pPublicacion.SeqOfAuthors = listaPersonasDefinitivas;
            return pPublicacion;
        }

        /// <summary>
        /// Comprueba el nombre completo de la primera persona.
        /// </summary>
        /// <param name="personaFinal">Persona.</param>
        private static void ComprobarNombreCompleto(Person personaFinal)
        {
            string nombreCompleto1 = "";
            if (personaFinal.Name.Given != null && personaFinal.Name.Given.Any())
            {
                nombreCompleto1 += personaFinal.Name.Given[0] + " ";
            }
            if (personaFinal.Name.Familia != null && personaFinal.Name.Familia.Any())
            {
                nombreCompleto1 += personaFinal.Name.Familia[0];
            }
            if (!string.IsNullOrEmpty(nombreCompleto1))
            {
                personaFinal.Name.Nombre_completo = new List<string>() { nombreCompleto1.Trim() };
            }
        }

        /// <summary>
        /// Permite unificar la información de dos personas que son la misma.
        /// </summary>
        /// <param name="pPersonaFinal">Persona con prioridad (WoS).</param>
        /// <param name="pPersonaAUnir">Persona a unificar.</param>
        /// <returns></returns>
        public Person UnirPersonas(Person pPersonaFinal, Person pPersonaAUnir)
        {
            // ORCID
            if (string.IsNullOrEmpty(pPersonaFinal.ORCID))
            {
                pPersonaFinal.ORCID = pPersonaAUnir.ORCID;
            }

            // Orden
            if (pPersonaFinal.Orden == null)
            {
                pPersonaFinal.Orden = pPersonaAUnir.Orden;
            }

            // Firma
            if (string.IsNullOrEmpty(pPersonaFinal.Nick))
            {
                pPersonaFinal.Nick = pPersonaAUnir.Nick;
            }

            // IDs
            HashSet<string> listaIds = new();
            if (pPersonaFinal.IDs != null && pPersonaFinal.IDs.Any())
            {
                foreach (string item in pPersonaFinal.IDs)
                {
                    listaIds.Add(item);
                }
            }
            if (pPersonaAUnir.IDs != null && pPersonaAUnir.IDs.Any())
            {
                foreach (string item in pPersonaAUnir.IDs)
                {
                    listaIds.Add(item);
                }
            }
            if (listaIds.Any())
            {
                pPersonaFinal.IDs = listaIds.ToList();
            }

            // Enlaces
            HashSet<string> listaLinks = new();
            if (pPersonaFinal.Links != null && pPersonaFinal.Links.Any())
            {
                foreach (string item in pPersonaFinal.Links)
                {
                    listaLinks.Add(item);
                }
            }
            if (pPersonaAUnir.Links != null && pPersonaAUnir.Links.Any())
            {
                foreach (string item in pPersonaAUnir.Links)
                {
                    listaLinks.Add(item);
                }
            }
            if (listaLinks.Any())
            {
                pPersonaFinal.Links = listaLinks.ToList();
            }

            // Nombre            
            if (pPersonaFinal.Name.Given == null || !pPersonaFinal.Name.Given.Any())
            {
                if (pPersonaAUnir.Name != null && pPersonaAUnir.Name.Given != null && pPersonaAUnir.Name.Given.Any())
                {
                    pPersonaFinal.Name.Given = pPersonaAUnir.Name.Given;
                }
                else if (pPersonaAUnir.Name != null && pPersonaAUnir.Name.Given == null && pPersonaAUnir.Name.Nombre_completo != null && pPersonaAUnir.Name.Nombre_completo.Any())
                {
                    pPersonaFinal.Name.Given = new List<string>() { pPersonaAUnir.Name.Nombre_completo[0].Split(" ")[0] };
                }
            }
            if (pPersonaFinal.Name.Familia == null || !pPersonaFinal.Name.Familia.Any())
            {
                if (pPersonaAUnir.Name != null && pPersonaAUnir.Name.Familia != null && pPersonaAUnir.Name.Familia.Any())
                {
                    pPersonaFinal.Name.Familia = pPersonaAUnir.Name.Familia;
                }
                else if (pPersonaAUnir.Name != null && pPersonaAUnir.Name.Familia == null && pPersonaAUnir.Name.Nombre_completo != null && pPersonaAUnir.Name.Nombre_completo.Any())
                {
                    pPersonaFinal.Name.Familia = new List<string>() { pPersonaAUnir.Name.Nombre_completo[0].Split(" ")[1] };
                }
            }
            if (pPersonaFinal.Name.Given != null && pPersonaFinal.Name.Given.Any() && !string.IsNullOrEmpty(pPersonaFinal.Name.Given[0]) && pPersonaFinal.Name.Familia != null && pPersonaFinal.Name.Familia.Any() && !string.IsNullOrEmpty(pPersonaFinal.Name.Familia[0]))
            {
                pPersonaFinal.Name.Nombre_completo[0] = $@"{pPersonaFinal.Name.Given[0]} {pPersonaFinal.Name.Familia[0]}";
            }

            // Nick
            if (string.IsNullOrEmpty(pPersonaFinal.Nick) && !string.IsNullOrEmpty(pPersonaAUnir.Nick))
            {
                pPersonaFinal.Nick = pPersonaAUnir.Nick;
            }

            return pPersonaFinal;
        }

        /// <summary>
        /// Convierte el objeto de Scopus personalizado al objeto de Publicación.
        /// </summary>
        /// <param name="pPublicacionScopus">Datos a convertir.</param>
        /// <returns>Objeto publicación.</returns>
        public Publication ObtenerPublicacionDeScopus(PublicacionScopus pPublicacionScopus)
        {
            Publication publicacion = new();
            publicacion.TypeOfPublication = pPublicacionScopus.typeOfPublication;
            publicacion.Title = pPublicacionScopus.title;
            publicacion.Doi = pPublicacionScopus.doi;
            publicacion.DataIssued = new();
            publicacion.DataIssued.DatimeTime = pPublicacionScopus.datimeTime;
            publicacion.Url = new HashSet<string>();
            foreach (string item in pPublicacionScopus.url)
            {
                publicacion.Url.Add(item);
            }
            if (pPublicacionScopus.correspondingAuthor != null)
            {
                publicacion.CorrespondingAuthor = new();
                publicacion.CorrespondingAuthor.Nick = pPublicacionScopus.correspondingAuthor.nick;
            }
            publicacion.PageStart = pPublicacionScopus.pageStart;
            publicacion.PageEnd = pPublicacionScopus.pageEnd;
            publicacion.Volume = pPublicacionScopus.volume;
            publicacion.ArticleNumber = pPublicacionScopus.articleNumber;
            publicacion.OpenAccess = pPublicacionScopus.openAccess;
            publicacion.IDs = new();
            publicacion.IDs.Add(pPublicacionScopus.scopusID);
            publicacion.DataOrigin = "Scopus";
            if (pPublicacionScopus.hasPublicationVenue != null)
            {
                publicacion.HasPublicationVenue = new();
                publicacion.HasPublicationVenue.Type = pPublicacionScopus.hasPublicationVenue.type;
                publicacion.HasPublicationVenue.Name = pPublicacionScopus.hasPublicationVenue.name;
                publicacion.HasPublicationVenue.Issn = pPublicacionScopus.hasPublicationVenue.issn;
                publicacion.HasPublicationVenue.Eissn = pPublicacionScopus.hasPublicationVenue.eissn;
            }
            if (pPublicacionScopus.hasMetric != null)
            {
                publicacion.HasMetric = new List<Models.PublicationMetric>();
                foreach (PublicationAPI.ROs.Publication.Models.PublicationMetric item in pPublicacionScopus.hasMetric)
                {
                    Models.PublicationMetric metrica = new();
                    metrica.CitationCount = item.citationCount;
                    metrica.MetricName = item.metricName;
                    publicacion.HasMetric.Add(metrica);
                }
            }

            return publicacion;
        }


        #region --- Similaridad en los nombres.
        public float GetNameSimilarity(string pFirma, string pTarget)
        {
            pFirma = ObtenerTextosFirmasNormalizadas(pFirma);
            pTarget = ObtenerTextosFirmasNormalizadas(pTarget);

            //Almacenamos los scores de cada una de las palabras
            List<float> scores = new();

            string[] pFirmaNormalizadoSplit = pFirma.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] pTargetNormalizadoSplit = pTarget.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            string[] source = pFirmaNormalizadoSplit;
            string[] target = pTargetNormalizadoSplit;

            int indexTarget = 0;
            for (int i = 0; i < source.Length; i++)
            {
                //Similitud real
                float score = 0;
                string wordSource = source[i];
                bool wordSourceInicial = wordSource.Length == 1;
                for (int j = indexTarget; j < target.Length; j++)
                {
                    string wordTarget = target[j];
                    bool wordTargetInicial = wordTarget.Length == 1;
                    //Alguna de las dos es inicial
                    if (wordSourceInicial || wordTargetInicial)
                    {
                        if (wordSourceInicial != wordTargetInicial)
                        {
                            //No son las dos iniciales
                            if (wordSource[0] == wordTarget[0])
                            {
                                score = 0.5f;
                                indexTarget = j + 1;
                                break;
                            }
                        }
                        else
                        {
                            //Son las dos iniciales
                            score = 0.75f;
                            indexTarget = j + 1;
                            break;
                        }
                    }
                    float scoreSingleName = CompareSingleName(wordSource, wordTarget);
                    if (scoreSingleName > 0)
                    {
                        score = scoreSingleName;
                        indexTarget = j + 1;
                        break;
                    }
                }
                scores.Add(score);
            }
            if (scores.Count > 0)
            {
                return scores.Sum() / source.Length;
            }
            return 0;
        }

        private static string ObtenerTextosFirmasNormalizadas(string pText)
        {
            pText = pText.ToLower();
            pText = pText.Trim();
            if (pText.Contains(","))
            {
                pText = (pText.Substring(pText.IndexOf(",") + 1)).Trim() + " " + (pText.Substring(0, pText.IndexOf(","))).Trim();
            }
            pText = pText.Replace("-", " ");
            string textoNormalizado = pText.Normalize(NormalizationForm.FormD);
            Regex reg = new("[^a-zA-Z ]");
            string textoSinAcentos = reg.Replace(textoNormalizado, "");
            while (textoSinAcentos.Contains(" del "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" del ", " ");
            }
            while (textoSinAcentos.Contains(" de "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" de ", " ");
            }
            while (textoSinAcentos.Contains(" la "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" la ", " ");
            }
            while (textoSinAcentos.Contains("  "))
            {
                textoSinAcentos = textoSinAcentos.Replace("  ", " ");
            }

            return textoSinAcentos.Trim();
        }

        private static float CompareSingleName(string pNameA, string pNameB)
        {
            HashSet<string> ngramsNameA = GetNGramas(pNameA, 2);
            HashSet<string> ngramsNameB = GetNGramas(pNameB, 2);
            float tokens_comunes = ngramsNameA.Intersect(ngramsNameB).Count();
            float union_tokens = ngramsNameA.Union(ngramsNameB).Count();
            float coeficiente_jackard = tokens_comunes / union_tokens;
            return coeficiente_jackard;
        }

        private static HashSet<string> GetNGramas(string pText, int pNgramSize)
        {
            HashSet<string> ngramas = new();
            int textLength = pText.Length;
            if (pNgramSize == 1)
            {
                for (int i = 0; i < textLength; i++)
                {
                    ngramas.Add(pText[i].ToString());
                }
                return ngramas;
            }

            HashSet<string> ngramasaux = new();
            for (int i = 0; i < textLength; i++)
            {
                foreach (string ngram in ngramasaux.ToList())
                {
                    string ngamaux = ngram + pText[i];
                    if (ngamaux.Length == pNgramSize)
                    {
                        ngramas.Add(ngamaux);
                    }
                    else
                    {
                        ngramasaux.Add(ngamaux);
                    }
                    ngramasaux.Remove(ngram);
                }
                ngramasaux.Add(pText[i].ToString());
                if (i < pNgramSize)
                {
                    foreach (string ngrama in ngramasaux)
                    {
                        if (ngrama.Length == i + 1)
                        {
                            ngramas.Add(ngrama);
                        }
                    }
                }
            }
            for (int i = (textLength - pNgramSize) + 1; i < textLength; i++)
            {
                if (i >= pNgramSize)
                {
                    ngramas.Add(pText.Substring(i));
                }
            }
            return ngramas;
        }
        #endregion

        #region --- Enriquecimiento
        public static ObjEnriquecimientoConPdf ObtenerObjEnriquecimientoPdf(Publication pPub)
        {
            if (!string.IsNullOrEmpty(pPub.Title) && !string.IsNullOrEmpty(pPub.Abstract))
            {
                ObjEnriquecimientoConPdf objEnriquecimiento = new();
                objEnriquecimiento.rotype = "papers";
                objEnriquecimiento.title = pPub.Title;
                objEnriquecimiento.abstract_ = pPub.Abstract;

                if (!string.IsNullOrEmpty(pPub.Pdf))
                {
                    objEnriquecimiento.pdfurl = pPub.Pdf;
                }

                return objEnriquecimiento;
            }
            else
            {
                return null;
            }
        }

        public static ObjEnriquecimientoSinPdf ObtenerObjEnriquecimiento(Publication pPub)
        {
            if (!string.IsNullOrEmpty(pPub.Title))
            {
                ObjEnriquecimientoSinPdf objEnriquecimiento = new("papers", pPub.Title, pPub.Abstract);

                if (string.IsNullOrEmpty(objEnriquecimiento.abstract_))
                {
                    objEnriquecimiento.abstract_ = pPub.Title;
                }

                return objEnriquecimiento;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, string> GetDescriptores(string pDataEnriquecimiento, string pTipo)
        {
            // Petición.
            HttpResponseMessage response;
            HttpClient client = new();
            string result = string.Empty;
            var contentData = new StringContent(pDataEnriquecimiento, Encoding.UTF8, "application/json");

            int intentos = 10;
            while (true)
            {
                try
                {
                    response = client.PostAsync($@"{_Configuracion.GetUrlEnriquecimiento()}/{pTipo}", contentData).Result;
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
                        Thread.Sleep(intentos * 1000);
                    }
                }
            }

            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
            }

            if (!string.IsNullOrEmpty(result))
            {
                TopicsEnriquecidos data;
                try
                {
                    data = JsonConvert.DeserializeObject<TopicsEnriquecidos>(result);
                }
                catch (Exception)
                {
                    return null;
                }

                if (data != null)
                {
                    Dictionary<string, string> dicTopics = new();
                    foreach (KnowledgeEnriquecidos item in data.Topics)
                    {
                        if (!dicTopics.ContainsKey(item.Word))
                        {
                            dicTopics.Add(item.Word, item.Porcentaje);
                        }
                    }
                    return dicTopics;
                }
            }

            return null;
        }
        #endregion
    }
}
