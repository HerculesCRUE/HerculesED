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
        public List<string> dois_principales = new List<string>();

        // Headers para las peticiones.
        public Dictionary<string, string> headers = new Dictionary<string, string>();

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
            Dictionary<string, string> dicZenodo = new Dictionary<string, string>();

            // Lista para almacenar las publicaciones resultantes.
            List<Publication> resultado = new List<Publication>();

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
                        if (pub.doi != null && !string.IsNullOrEmpty(pub.doi))
                        {
                            this.dois_principales.Add(pub.doi.ToLower());
                        }

                        // SemanticScholar.
                        int contadorSemanticScholar = 1;
                        Tuple<Publication, List<PubReferencias>> dataSemanticScholar = new Tuple<Publication, List<PubReferencias>>(null, null);
                        while (dataSemanticScholar.Item2 == null && contadorSemanticScholar <= 5)
                        {
                            Console.WriteLine($@"[WoS] Haciendo petición a SemanticScholar ({contadorSemanticScholar})...");
                            dataSemanticScholar = LlamadaRefSemanticScholar(pub.doi);
                            contadorSemanticScholar++;

                            // Si se obtienen datos, es válido.
                            if (dataSemanticScholar == null || !string.IsNullOrEmpty(dataSemanticScholar.Item1.title) && dataSemanticScholar.Item2 == null)
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
                        Publication pub_completa = pub;
                        pub_completa.dataOriginList = new HashSet<string>() { "WoS" };
                        if (dataSemanticScholar != null && dataSemanticScholar.Item2 != null)
                        {
                            pub_completa = Compactacion(pub, dataSemanticScholar.Item1);
                            pub_completa.bibliografia = dataSemanticScholar.Item2;
                        }

                        // Zenodo - Archivos pdf...
                        Console.WriteLine("[WoS] Haciendo petición a Zenodo...");
                        pub_completa.pdf = LlamadaZenodo(pub.doi, dicZenodo);
                        if (pub_completa.pdf == string.Empty)
                        {
                            pub_completa.pdf = null;
                        }
                        else
                        {
                            pub_completa.dataOriginList.Add("Zenodo");
                        }

                        // Completar información faltante con las publicaciones de Scopus.
                        if (objInicial_Scopus != null && objInicial_Scopus.Any())
                        {
                            foreach (PublicacionScopus pub_scopus in objInicial_Scopus)
                            {
                                if (pub_scopus != null)
                                {
                                    Publication pubScopus = ObtenerPublicacionDeScopus(pub_scopus);
                                    if (pub_scopus.doi != null && !string.IsNullOrEmpty(pub_completa.doi) && pub_scopus.doi.ToLower() == pub_completa.doi.ToLower())
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
                                if (pub_openAire != null && pub_openAire.doi != null && !string.IsNullOrEmpty(pub_completa.doi) && pub_openAire.doi.ToLower() == pub_completa.doi.ToLower())
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

                            // SemanticScholar.
                            int contadorSemanticScholar = 1;
                            Tuple<Publication, List<PubReferencias>> dataSemanticScholar = new Tuple<Publication, List<PubReferencias>>(null, null);
                            while (dataSemanticScholar.Item2 == null && contadorSemanticScholar <= 5)
                            {
                                Console.WriteLine($@"[Scopus] Haciendo petición a SemanticScholar ({contadorSemanticScholar})...");
                                dataSemanticScholar = LlamadaRefSemanticScholar(pubScopus.doi);
                                contadorSemanticScholar++;

                                // Si se obtienen datos, es válido.
                                if (dataSemanticScholar == null || !string.IsNullOrEmpty(dataSemanticScholar.Item1.title) && dataSemanticScholar.Item2 == null)
                                {
                                    break;
                                }

                                // En el caso que venga vacío, esperamos 10 segundos a volverlo a intentar.
                                if (dataSemanticScholar.Item2 == null && contadorSemanticScholar != 5)
                                {
                                    Thread.Sleep(10000);
                                }
                            }

                            Console.WriteLine("[Scopus] Comparación (SemanticScholar)...");
                            Publication pub_completa = pubScopus;
                            pub_completa.dataOriginList = new HashSet<string>() { "Scopus" };
                            if (dataSemanticScholar != null && dataSemanticScholar.Item2 != null)
                            {
                                pub_completa = Compactacion(pubScopus, dataSemanticScholar.Item1);
                                pub_completa.bibliografia = dataSemanticScholar.Item2;
                            }

                            // Zenodo - Archivos pdf...
                            Console.WriteLine("[Scopus] Haciendo petición a Zenodo...");
                            pub_completa.pdf = LlamadaZenodo(pub_completa.doi, dicZenodo);
                            if (pub_completa.pdf == String.Empty)
                            {
                                pub_completa.pdf = null;
                            }
                            else
                            {
                                pub_completa.dataOriginList.Add("Zenodo");
                            }

                            // Completar información faltante con las publicaciones de OpenAire.
                            if (objInicial_openAire != null && objInicial_openAire.Any())
                            {
                                foreach (Publication pub_openAire in objInicial_openAire)
                                {
                                    if (pub_openAire != null && pub_openAire.doi != null && !string.IsNullOrEmpty(pub_completa.doi) && pub_openAire.doi.ToLower() == pub_completa.doi.ToLower())
                                    {
                                        pub_completa = Compactacion(pub_completa, pub_openAire);
                                    }
                                }
                            }

                            // Unificar Autores.
                            pub_completa = CompararAutoresCitasReferencias(pub_completa);

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
                        if (pub != null && !string.IsNullOrEmpty(pub.doi) && !dois_principales.Contains(pub.doi.ToLower()))
                        {
                            if (pub.doi != null && !string.IsNullOrEmpty(pub.doi))
                            {
                                this.dois_principales.Add(pub.doi.ToLower());
                            }

                            // SemanticScholar.
                            int contadorSemanticScholar = 1;
                            Tuple<Publication, List<PubReferencias>> dataSemanticScholar = new Tuple<Publication, List<PubReferencias>>(null, null);
                            while (dataSemanticScholar.Item2 == null && contadorSemanticScholar <= 5)
                            {
                                Console.WriteLine($@"[OpenAire] Haciendo petición a SemanticScholar ({contadorSemanticScholar})...");
                                dataSemanticScholar = LlamadaRefSemanticScholar(pub.doi);
                                contadorSemanticScholar++;

                                // Si se obtienen datos, es válido.
                                if (dataSemanticScholar == null || !string.IsNullOrEmpty(dataSemanticScholar.Item1.title) && dataSemanticScholar.Item2 == null)
                                {
                                    break;
                                }

                                // En el caso que venga vacío, esperamos 10 segundos a volverlo a intentar.
                                if (dataSemanticScholar.Item2 == null && contadorSemanticScholar != 5)
                                {
                                    Thread.Sleep(10000);
                                }
                            }

                            Console.WriteLine("[OpenAire] Comparación (SemanticScholar)...");
                            Publication pub_completa = pub;
                            pub_completa.dataOriginList = new HashSet<string>() { "OpenAire" };
                            if (dataSemanticScholar != null && dataSemanticScholar.Item2 != null)
                            {
                                pub_completa = Compactacion(pub, dataSemanticScholar.Item1);
                                pub_completa.bibliografia = dataSemanticScholar.Item2;
                            }

                            // Zenodo - Archivos pdf...
                            Console.WriteLine("[OpenAire] Haciendo petición a Zenodo...");
                            pub_completa.pdf = LlamadaZenodo(pub_completa.doi, dicZenodo);
                            if (pub_completa.pdf == string.Empty)
                            {
                                pub_completa.pdf = null;
                            }
                            else
                            {
                                pub_completa.dataOriginList.Add("Zenodo");
                            }

                            // Unificar Autores.
                            pub_completa = CompararAutoresCitasReferencias(pub_completa);

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
            List<Publication> listaPubsFinal = new List<Publication>();
            foreach (Publication publicacion in resultado)
            {
                if (publicacion != null && !string.IsNullOrEmpty(publicacion.title) && publicacion.seqOfAuthors != null && publicacion.seqOfAuthors.Any() && publicacion.title != "One or more validation errors occurred.")
                {
                    if (!string.IsNullOrEmpty(pNombreCompletoAutor))
                    {
                        bool encontrado = false;
                        foreach (Person persona in publicacion.seqOfAuthors)
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
                publicacion.title = Regex.Replace(publicacion.title, "<.*?>", string.Empty);
                if (publicacion.Abstract != null)
                {
                    publicacion.Abstract = Regex.Replace(publicacion.Abstract, "<.*?>", string.Empty);
                }

                // Preparación del objeto a enviar para el enriquecimiento.
                string jsonData = string.Empty;
                if (string.IsNullOrEmpty(publicacion.pdf))
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
                        publicacion.topics_enriquecidos = new List<Knowledge_enriquecidos>();
                        foreach (KeyValuePair<string, string> item in listaTopics)
                        {
                            Knowledge_enriquecidos topic = new Knowledge_enriquecidos();
                            topic.word = item.Key;
                            topic.porcentaje = item.Value;
                            publicacion.topics_enriquecidos.Add(topic);
                        }
                    }
                    else
                    {
                        publicacion.topics_enriquecidos = null;
                    }

                    if (listaEtiquetas != null && listaEtiquetas.Any())
                    {
                        publicacion.freetextKeyword_enriquecidas = new List<Knowledge_enriquecidos>();
                        foreach (KeyValuePair<string, string> item in listaEtiquetas)
                        {
                            Knowledge_enriquecidos topic = new Knowledge_enriquecidos();
                            topic.word = item.Key;
                            topic.porcentaje = item.Value;
                            publicacion.freetextKeyword_enriquecidas.Add(topic);
                        }
                    }
                    else
                    {
                        publicacion.freetextKeyword_enriquecidas = null;
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
                    foreach (Person persona in publicacion.seqOfAuthors)
                    {
                        string nombreCompleto = persona.name.nombre_completo.First();

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
        /// Compara las dos publicaciones por parámetro y las une dándole prioridad a la primera.
        /// </summary>
        /// <param name="pub_1">Publicación número uno (con prioridad).</param>
        /// <param name="pub_2">Publicación número dos.</param>
        /// <returns></returns>
        public Publication Compactacion(Publication pub_1, Publication pub_2)
        {
            Publication pub = new Publication();
            pub.dataOriginList = pub_1.dataOriginList;
            if (pub_1 == null)
            {
                if (pub_2 == null)
                {
                    return null;
                }
                else { return pub_2; }
            }
            else
            {
                if (pub_2 == null)
                {
                    return pub_1;
                }
                else
                {
                    bool pub1 = false;
                    bool pub2 = false;
                    if (!string.IsNullOrEmpty(pub_1.typeOfPublication))
                    {
                        pub.typeOfPublication = pub_1.typeOfPublication;
                        pub1 = true;
                    }
                    else
                    {
                        pub.typeOfPublication = pub_2.typeOfPublication;
                        pub2 = true;
                    }

                    if (!string.IsNullOrEmpty(pub_1.title))
                    {
                        pub.title = pub_1.title;
                        pub1 = true;
                    }
                    else
                    {
                        pub.title = pub_2.title;
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

                    if (pub_1.freetextKeywords != null && pub_1.freetextKeywords.Any())
                    {
                        pub.freetextKeywords = pub_1.freetextKeywords;
                        pub1 = true;
                        if (pub_2.freetextKeywords != null)
                        {
                            pub.freetextKeywords.AddRange(pub_2.freetextKeywords);
                            pub2 = true;
                        }
                    }
                    else
                    {
                        pub.freetextKeywords = pub_2.freetextKeywords;
                        pub2 = true;
                    }

                    if (!string.IsNullOrEmpty(pub_1.language))
                    {
                        pub.language = pub_1.language;
                        pub1 = true;
                    }
                    else
                    {

                        pub.language = pub_2.language;
                        pub2 = true;
                    }

                    // Si es un capitulo de libro, no necesita DOI (Da problemas en el motor de desambiguación).
                    if (pub.typeOfPublication != "Chapter")
                    {
                        if (pub_1.doi != null)
                        {
                            pub.doi = pub_1.doi;
                            pub1 = true;
                        }
                        else
                        {
                            pub.doi = pub_2.doi;
                            pub2 = true;
                        }
                    }

                    if (pub_1.dataIssued != null)
                    {
                        pub.dataIssued = pub_1.dataIssued;
                        pub1 = true;
                    }
                    else
                    {
                        if (pub_2.dataIssued != null)
                        {
                            pub.dataIssued = pub_2.dataIssued;
                            pub2 = true;
                        }
                        else { pub.dataIssued = null; }
                    }
                    if (pub_1.url != null && pub_1.url.Any())
                    {
                        pub.url = pub_1.url;
                        pub1 = true;
                        if (pub_2.url != null)
                        {
                            foreach (string item in pub_2.url)
                            {
                                pub.url.Add(item);
                                pub2 = true;
                            }
                        }
                    }
                    else
                    {
                        pub.url = pub_2.url;
                        pub2 = true;
                    }
                    if (pub_1.correspondingAuthor != null)
                    {
                        pub.correspondingAuthor = pub_1.correspondingAuthor;
                        pub1 = true;
                    }
                    else
                    {
                        pub.correspondingAuthor = pub_2.correspondingAuthor;
                        pub2 = true;
                    }

                    pub.seqOfAuthors = new List<Models.Person>();
                    if (pub_1.seqOfAuthors != null && pub_1.seqOfAuthors.Count > 0)
                    {
                        pub.seqOfAuthors.AddRange(pub_1.seqOfAuthors);
                        pub1 = true;
                    }
                    if (pub_2.seqOfAuthors != null && pub_2.seqOfAuthors.Count > 0)
                    {
                        pub.seqOfAuthors.AddRange(pub_2.seqOfAuthors);
                        pub2 = true;
                    }

                    if (pub_1.hasKnowledgeAreas != null && pub_1.hasKnowledgeAreas.Any())
                    {
                        pub.hasKnowledgeAreas = pub_1.hasKnowledgeAreas;
                        pub1 = true;
                        if (pub_2.hasKnowledgeAreas != null)
                        {
                            pub.hasKnowledgeAreas.AddRange(pub_2.hasKnowledgeAreas);
                            pub2 = true;
                        }
                    }
                    else
                    {
                        pub.hasKnowledgeAreas = pub_2.hasKnowledgeAreas;
                        pub2 = true;
                    }

                    if (!string.IsNullOrEmpty(pub_1.pageEnd))
                    {
                        pub.pageEnd = pub_1.pageEnd;
                        pub1 = true;
                    }
                    else
                    {
                        pub.pageEnd = pub_2.pageEnd;
                        pub2 = true;
                    }

                    if (!string.IsNullOrEmpty(pub_1.pageStart))
                    {
                        pub.pageStart = pub_1.pageStart;
                        pub1 = true;
                    }
                    else
                    {
                        pub.pageStart = pub_2.pageStart;
                        pub2 = true;
                    }

                    if (!string.IsNullOrEmpty(pub_1.volume))
                    {
                        pub.volume = pub_1.volume;
                        pub1 = true;
                    }
                    else
                    {
                        pub.volume = pub_2.volume;
                        pub2 = true;
                    }

                    if (!string.IsNullOrEmpty(pub_1.articleNumber))
                    {
                        pub.articleNumber = pub_1.articleNumber;
                        pub1 = true;
                    }
                    else
                    {
                        pub.articleNumber = pub_2.articleNumber;
                        pub2 = true;
                    }

                    if (pub_1.openAccess != null)
                    {
                        pub.openAccess = pub_1.openAccess;
                        pub1 = true;
                    }
                    else
                    {
                        pub.openAccess = pub_2.openAccess;
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

                    if (!string.IsNullOrEmpty(pub_1.presentedAt))
                    {
                        pub.presentedAt = pub_1.presentedAt;
                        pub1 = true;
                    }
                    else
                    {
                        pub.presentedAt = pub_2.presentedAt;
                        pub2 = true;
                    }

                    if (pub_1.conferencia != null)
                    {
                        pub.conferencia = pub_1.conferencia;
                        pub1 = true;
                    }
                    else
                    {
                        pub.conferencia = pub_2.conferencia;
                        pub2 = true;
                    }

                    Dictionary<string, Models.PublicationMetric> dicMetricas = new Dictionary<string, Models.PublicationMetric>();

                    if (pub_1.hasMetric != null && pub_1.hasMetric.Any())
                    {
                        foreach (Models.PublicationMetric metrica in pub_1.hasMetric)
                        {
                            if (!dicMetricas.ContainsKey(metrica.metricName))
                            {
                                dicMetricas.Add(metrica.metricName, metrica);
                                pub1 = true;
                            }
                        }
                    }
                    if (pub_2.hasMetric != null && pub_2.hasMetric.Any())
                    {
                        foreach (Models.PublicationMetric metrica in pub_2.hasMetric)
                        {
                            if (!dicMetricas.ContainsKey(metrica.metricName))
                            {
                                dicMetricas.Add(metrica.metricName, metrica);
                                pub2 = true;
                            }
                        }
                    }

                    pub.hasMetric = new List<Models.PublicationMetric>();
                    foreach (KeyValuePair<string, Models.PublicationMetric> metrica in dicMetricas)
                    {
                        pub.hasMetric.Add(metrica.Value);
                    }

                    pub.hasPublicationVenue = new Models.Source();
                    HashSet<string> listaIssn = new HashSet<string>();

                    if (pub_1.hasPublicationVenue != null && !string.IsNullOrEmpty(pub_1.hasPublicationVenue.type))
                    {
                        pub.hasPublicationVenue.type = pub_1.hasPublicationVenue.type;
                        pub1 = true;
                    }
                    else if (pub_2.hasPublicationVenue != null && !string.IsNullOrEmpty(pub_2.hasPublicationVenue.type))
                    {
                        pub.hasPublicationVenue.type = pub_2.hasPublicationVenue.type;
                        pub2 = true;
                    }

                    if (pub_1.hasPublicationVenue != null && !string.IsNullOrEmpty(pub_1.hasPublicationVenue.eissn))
                    {
                        pub.hasPublicationVenue.eissn = pub_1.hasPublicationVenue.eissn;
                        pub1 = true;
                    }
                    else if (pub_2.hasPublicationVenue != null && !string.IsNullOrEmpty(pub_2.hasPublicationVenue.eissn))
                    {
                        pub.hasPublicationVenue.eissn = pub_2.hasPublicationVenue.eissn;
                        pub2 = true;
                    }

                    if (pub_1.hasPublicationVenue != null && !string.IsNullOrEmpty(pub_1.hasPublicationVenue.name))
                    {
                        pub.hasPublicationVenue.name = pub_1.hasPublicationVenue.name;
                        pub1 = true;
                    }
                    else if (pub_2.hasPublicationVenue != null && !string.IsNullOrEmpty(pub_2.hasPublicationVenue.name))
                    {
                        pub.hasPublicationVenue.name = pub_2.hasPublicationVenue.name;
                        pub2 = true;
                    }

                    if (pub_1.hasPublicationVenue != null && pub_1.hasPublicationVenue.issn != null && pub_1.hasPublicationVenue.issn.Count > 0)
                    {
                        foreach (string item in pub_1.hasPublicationVenue.issn)
                        {
                            if (item != pub.hasPublicationVenue.eissn)
                            {
                                listaIssn.Add(item);
                                pub1 = true;
                            }
                        }
                    }

                    if (pub_2.hasPublicationVenue != null && pub_2.hasPublicationVenue.issn != null && pub_2.hasPublicationVenue.issn.Count > 0)
                    {
                        foreach (string item in pub_2.hasPublicationVenue.issn)
                        {
                            if (item != pub.hasPublicationVenue.eissn)
                            {
                                listaIssn.Add(item);
                                pub2 = true;
                            }
                        }
                    }

                    pub.hasPublicationVenue.issn = listaIssn.ToList();

                    if (!string.IsNullOrEmpty(pub_1.pdf))
                    {
                        pub.pdf = pub_1.pdf;
                        pub1 = true;
                    }
                    else
                    {
                        pub.pdf = pub_2.pdf;
                        pub2 = true;
                    }

                    if (pub_1.topics_enriquecidos != null && pub_1.topics_enriquecidos.Any())
                    {
                        pub.topics_enriquecidos = pub_1.topics_enriquecidos;
                        pub1 = true;
                    }
                    else
                    {
                        pub.topics_enriquecidos = pub_2.topics_enriquecidos;
                        pub2 = true;
                    }


                    if (pub_1.freetextKeyword_enriquecidas != null && pub_1.freetextKeyword_enriquecidas.Any())
                    {
                        pub.freetextKeyword_enriquecidas = pub_1.freetextKeyword_enriquecidas;
                        pub1 = true;
                    }
                    else
                    {
                        pub.freetextKeyword_enriquecidas = pub_2.freetextKeyword_enriquecidas;
                        pub2 = true;
                    }

                    if (pub_1.bibliografia != null && pub_1.bibliografia.Any())
                    {
                        pub.bibliografia = pub_1.bibliografia;
                        pub1 = true;
                    }
                    else
                    {
                        pub.bibliografia = pub_2.bibliografia;
                        pub2 = true;
                    }

                    if (pub1 && pub_1.dataOrigin != null)
                    {
                        pub.dataOriginList.Add(pub_1.dataOrigin);
                    }
                    if (pub2)
                    {
                        pub.dataOriginList.Add(pub_2.dataOrigin);
                    }

                    return pub;
                }
            }
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
                    Uri url = new Uri(string.Format(_Configuracion.GetUrlSemanticScholar() + "SemanticScholar/GetReferences?pDoi={0}", pDoi));

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
            Uri url = new Uri(string.Format(_Configuracion.GetUrlScopus() + "Scopus/GetROs?orcid={0}&date={1}", pOrcid, date));
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
            Uri url = new Uri(string.Format(_Configuracion.GetUrlScopus() + "Scopus/GetPublicationByDOI?pDoi={0}", pDoi));
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
            Uri url = new Uri(string.Format(_Configuracion.GetUrlWos() + "WoS/GetROs?orcid={0}&date={1}", pOrcid, date));

            int contadorVeces = 0;
            string info_publication = string.Empty;
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
            Uri url = new Uri(string.Format(_Configuracion.GetUrlWos() + "WoS/GetRoByDoi?pDoi={0}", pDoi));
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
            Uri url = new Uri(string.Format(_Configuracion.GetUrlOpenAire() + "OpenAire/GetROs?orcid={0}&date={1}", pOrcid, date));
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
            Uri url = new Uri(string.Format(_Configuracion.GetUrlOpenAire() + "OpenAire/GetRoByDoi?pDoi={0}", pDoi));
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
                    Uri url = new Uri(string.Format(_Configuracion.GetUrlZenodo() + "Zenodo/GetROs?ID={0}", pDoi));

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
        /// </summary>
        /// <param name="pPublicacion">Publicación a fusionar autores.</param>
        /// <returns>Publicación con los autores fusionados.</returns>
        public Publication CompararAutores(Publication pPublicacion)
        {
            // Prioridad: WOS > OpenAire > SemanticScholar
            Dictionary<string, List<Person>> dicPersonas = new Dictionary<string, List<Person>>();
            dicPersonas.Add("WoS", new List<Person>());
            dicPersonas.Add("OpenAire", new List<Person>());
            dicPersonas.Add("SemanticScholar", new List<Person>());

            // Peso.
            double umbral = 0.7;

            foreach (Person persona in pPublicacion.seqOfAuthors)
            {
                if (persona.name != null)
                {
                    dicPersonas[persona.fuente].Add(persona);
                }
            }

            List<Person> listaPersonasDefinitivas = new List<Person>();

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
                    string nombreCompleto1 = string.Empty;
                    if (personaFinal.name.given != null && personaFinal.name.given.Any())
                    {
                        nombreCompleto1 += personaFinal.name.given[0] + " ";
                    }
                    if (personaFinal.name.familia != null && personaFinal.name.familia.Any())
                    {
                        nombreCompleto1 += personaFinal.name.familia[0];
                    }
                    if (!string.IsNullOrEmpty(nombreCompleto1))
                    {
                        personaFinal.name.nombre_completo = new List<string>() { nombreCompleto1.Trim() };
                    }

                    string nombreCompleto2 = string.Empty;
                    if (personaCrossRef.name.given != null && personaCrossRef.name.given.Any())
                    {
                        nombreCompleto2 += personaCrossRef.name.given[0] + " ";
                    }
                    if (personaCrossRef.name.familia != null && personaCrossRef.name.familia.Any())
                    {
                        nombreCompleto2 += personaCrossRef.name.familia[0];
                    }
                    if (!string.IsNullOrEmpty(nombreCompleto2))
                    {
                        personaCrossRef.name.nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                    }

                    if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && personaCrossRef.name.nombre_completo != null && personaCrossRef.name.nombre_completo.Any() && GetNameSimilarity(personaFinal.name.nombre_completo[0], personaCrossRef.name.nombre_completo[0]) >= umbral)
                    {
                        personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                        break;
                    }

                    if (personaFinal.name.given != null && personaFinal.name.given.Any() && !string.IsNullOrEmpty(personaFinal.name.given[0]) && personaFinal.name.familia != null && personaFinal.name.familia.Any() && !string.IsNullOrEmpty(personaFinal.name.familia[0]))
                    {
                        personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
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
                    if (personaSemantic.name.given != null && personaFinal.name.given != null && personaFinal.name.given.Any())
                    {
                        nombreCompleto1 += personaFinal.name.given[0] + " ";
                    }
                    if (personaFinal.name.familia != null && personaFinal.name.familia.Any())
                    {
                        nombreCompleto1 += personaFinal.name.familia[0];
                    }
                    if (!string.IsNullOrEmpty(nombreCompleto1))
                    {
                        personaFinal.name.nombre_completo = new List<string>() { nombreCompleto1.Trim() };
                    }

                    string nombreCompleto2 = string.Empty;
                    if (personaSemantic.name.given != null && personaSemantic.name.given.Any())
                    {
                        nombreCompleto2 += personaSemantic.name.given[0] + " ";
                    }
                    if (personaSemantic.name.familia != null && personaSemantic.name.familia.Any())
                    {
                        nombreCompleto2 += personaSemantic.name.familia[0];
                    }
                    if (!string.IsNullOrEmpty(nombreCompleto2))
                    {
                        personaSemantic.name.nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                    }

                    if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && personaSemantic.name.nombre_completo != null && personaSemantic.name.nombre_completo.Any() && GetNameSimilarity(personaFinal.name.nombre_completo[0], personaSemantic.name.nombre_completo[0]) >= umbral)
                    {
                        personaFinal = UnirPersonas(personaFinal, personaSemantic);
                        break;
                    }

                    personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                }

                listaPersonasDefinitivas.Add(personaFinal);
            }

            // Encontrar el autor
            foreach (Person persona in listaPersonasDefinitivas)
            {
                Models.Person personaFinal = persona;

                // Comprobación por ORCID
                if (!string.IsNullOrEmpty(personaFinal.ORCID) && personaFinal.ORCID == pPublicacion.correspondingAuthor.ORCID)
                {
                    pPublicacion.correspondingAuthor = personaFinal;
                    break;
                }

                // Comprobración por nombre completo
                string nombreCompleto1 = string.Empty;
                if (personaFinal.name.given != null && personaFinal.name.given.Any())
                {
                    nombreCompleto1 += personaFinal.name.given[0] + " ";
                }
                if (personaFinal.name.familia != null && personaFinal.name.familia.Any())
                {
                    nombreCompleto1 += personaFinal.name.familia[0];
                }
                if (!string.IsNullOrEmpty(nombreCompleto1))
                {
                    personaFinal.name.nombre_completo = new List<string>() { nombreCompleto1.Trim() };
                }

                string nombreCompleto2 = string.Empty;
                if (pPublicacion.correspondingAuthor.name.given != null && pPublicacion.correspondingAuthor.name.given.Any())
                {
                    nombreCompleto2 += pPublicacion.correspondingAuthor.name.given[0] + " ";
                }
                if (pPublicacion.correspondingAuthor.name.familia != null && pPublicacion.correspondingAuthor.name.familia.Any())
                {
                    nombreCompleto2 += pPublicacion.correspondingAuthor.name.familia[0];
                }
                if (!string.IsNullOrEmpty(nombreCompleto2))
                {
                    pPublicacion.correspondingAuthor.name.nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                }

                if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && pPublicacion.correspondingAuthor.name.nombre_completo != null && pPublicacion.correspondingAuthor.name.nombre_completo.Any() && GetNameSimilarity(personaFinal.name.nombre_completo[0], pPublicacion.correspondingAuthor.name.nombre_completo[0]) >= umbral)
                {
                    pPublicacion.correspondingAuthor = personaFinal;
                    break;
                }

                if (!string.IsNullOrEmpty(pPublicacion.correspondingAuthor.nick) && GetNameSimilarity(personaFinal.name.nombre_completo[0], pPublicacion.correspondingAuthor.nick) >= 0.01)
                {
                    pPublicacion.correspondingAuthor = UnirPersonas(personaFinal, pPublicacion.correspondingAuthor);
                    break;
                }
                if (personaFinal.name.given != null && personaFinal.name.given.Any() && !string.IsNullOrEmpty(personaFinal.name.given[0]) && personaFinal.name.familia != null && personaFinal.name.familia.Any() && !string.IsNullOrEmpty(personaFinal.name.familia[0]))
                {
                    personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                }
            }

            pPublicacion.seqOfAuthors = listaPersonasDefinitivas;
            return pPublicacion;
        }

        /// <summary>
        /// Mediante una publicación (cita o referencia), compara todos sus autores entre ellos para quitar duplicados y fusionar la información.
        /// </summary>
        /// <param name="pPublicacion">Publicación a fusionar autores.</param>
        /// <returns>Publicación con los autores fusionados.</returns>
        public Publication CompararAutoresCitasReferencias(Publication pPublicacion)
        {
            // Prioridad --> SemanticScholar > OpenAire > CrossRef
            Dictionary<string, List<Models.Person>> dicPersonas = new Dictionary<string, List<Models.Person>>();
            dicPersonas.Add("WoS", new List<Models.Person>());
            dicPersonas.Add("OpenAire", new List<Models.Person>());
            dicPersonas.Add("SemanticScholar", new List<Models.Person>());
            dicPersonas.Add("CrossRef", new List<Models.Person>());

            double umbral = 0.7;

            if (pPublicacion.seqOfAuthors != null && pPublicacion.seqOfAuthors.Any())
            {
                foreach (Models.Person persona in pPublicacion.seqOfAuthors)
                {
                    dicPersonas[persona.fuente].Add(persona);
                }

                List<Models.Person> listaPersonasDefinitivas = new List<Models.Person>();

                // Unir personas 
                foreach (Models.Person persona in dicPersonas["SemanticScholar"])
                {
                    Models.Person personaFinal = persona;

                    foreach (Models.Person personaCrossRef in dicPersonas["OpenAire"])
                    {
                        // Comprobación por ORCID
                        if (!string.IsNullOrEmpty(personaFinal.ORCID) && personaFinal.ORCID == personaCrossRef.ORCID)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                            break;
                        }

                        // Comprobración por nombre completo
                        string nombreCompleto1 = string.Empty;
                        if (personaFinal.name.given != null && personaFinal.name.given.Any())
                        {
                            nombreCompleto1 += personaFinal.name.given[0] + " ";
                        }
                        if (personaFinal.name.familia != null && personaFinal.name.familia.Any())
                        {
                            nombreCompleto1 += personaFinal.name.familia[0];
                        }
                        if (!string.IsNullOrEmpty(nombreCompleto1))
                        {
                            personaFinal.name.nombre_completo = new List<string>() { nombreCompleto1.Trim() };
                        }

                        string nombreCompleto2 = string.Empty;
                        if (personaCrossRef.name.given != null && personaCrossRef.name.given.Any())
                        {
                            nombreCompleto2 += personaCrossRef.name.given[0] + " ";
                        }
                        if (personaCrossRef.name.familia != null && personaCrossRef.name.familia.Any())
                        {
                            nombreCompleto2 += personaCrossRef.name.familia[0];
                        }
                        if (!string.IsNullOrEmpty(nombreCompleto2))
                        {
                            personaCrossRef.name.nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                        }

                        if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() &&
                            personaCrossRef.name.nombre_completo != null && personaCrossRef.name.nombre_completo.Any() &&
                            GetNameSimilarity(personaFinal.name.nombre_completo[0], personaCrossRef.name.nombre_completo[0]) >= umbral)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                            break;
                        }

                        if (personaFinal.name.given != null && personaFinal.name.given.Any() && !string.IsNullOrEmpty(personaFinal.name.given[0]) && personaFinal.name.familia != null && personaFinal.name.familia.Any() && !string.IsNullOrEmpty(personaFinal.name.familia[0]))
                        {
                            personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                        }
                    }

                    foreach (Models.Person personaCrossRef in dicPersonas["CrossRef"])
                    {
                        // Comprobación por ORCID
                        if (!string.IsNullOrEmpty(personaFinal.ORCID) && personaFinal.ORCID == personaCrossRef.ORCID)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                            break;
                        }

                        // Comprobración por nombre completo
                        string nombreCompleto1 = string.Empty;
                        if (personaFinal.name.given != null && personaFinal.name.given.Any())
                        {
                            nombreCompleto1 += personaFinal.name.given[0] + " ";
                        }
                        if (personaFinal.name.familia != null && personaFinal.name.familia.Any())
                        {
                            nombreCompleto1 += personaFinal.name.familia[0];
                        }
                        if (!string.IsNullOrEmpty(nombreCompleto1))
                        {
                            personaFinal.name.nombre_completo = new List<string>() { nombreCompleto1.Trim() };
                        }

                        string nombreCompleto2 = string.Empty;
                        if (personaCrossRef.name.given != null && personaCrossRef.name.given.Any())
                        {
                            nombreCompleto2 += personaCrossRef.name.given[0] + " ";
                        }
                        if (personaCrossRef.name.familia != null && personaCrossRef.name.familia.Any())
                        {
                            nombreCompleto2 += personaCrossRef.name.familia[0];
                        }
                        if (!string.IsNullOrEmpty(nombreCompleto2))
                        {
                            personaCrossRef.name.nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                        }

                        if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() &&
                            personaCrossRef.name.nombre_completo != null && personaCrossRef.name.nombre_completo.Any() &&
                            GetNameSimilarity(personaFinal.name.nombre_completo[0], personaCrossRef.name.nombre_completo[0]) >= umbral)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                            break;
                        }
                        if (personaFinal.name.given != null && personaFinal.name.given.Any() && !string.IsNullOrEmpty(personaFinal.name.given[0]) && personaFinal.name.familia != null && personaFinal.name.familia.Any() && !string.IsNullOrEmpty(personaFinal.name.familia[0]))
                        {
                            personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                        }
                    }

                    listaPersonasDefinitivas.Add(personaFinal);
                }

                // ÑAPA
                if (!dicPersonas["SemanticScholar"].Any())
                {
                    foreach (Person personaOpenAire in dicPersonas["OpenAire"])
                    {
                        Person personaFinal = personaOpenAire;

                        // Comprobación por ORCID
                        if (!string.IsNullOrEmpty(personaFinal.ORCID) && personaFinal.ORCID == personaOpenAire.ORCID)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaOpenAire);
                            break;
                        }

                        // Comprobración por nombre completo
                        string nombreCompleto1 = string.Empty;
                        if (personaFinal.name.given != null && personaFinal.name.given.Any())
                        {
                            nombreCompleto1 += personaFinal.name.given[0] + " ";
                        }
                        if (personaFinal.name.familia != null && personaFinal.name.familia.Any())
                        {
                            nombreCompleto1 += personaFinal.name.familia[0];
                        }
                        if (!string.IsNullOrEmpty(nombreCompleto1))
                        {
                            personaFinal.name.nombre_completo = new List<string>() { nombreCompleto1.Trim() };
                        }

                        string nombreCompleto2 = string.Empty;
                        if (personaOpenAire.name.given != null && personaOpenAire.name.given.Any())
                        {
                            nombreCompleto2 += personaOpenAire.name.given[0] + " ";
                        }
                        if (personaOpenAire.name.familia != null && personaOpenAire.name.familia.Any())
                        {
                            nombreCompleto2 += personaOpenAire.name.familia[0];
                        }
                        if (!string.IsNullOrEmpty(nombreCompleto2))
                        {
                            personaOpenAire.name.nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                        }

                        if (personaFinal.name.given != null && personaFinal.name.given.Any() && !string.IsNullOrEmpty(personaFinal.name.given[0]) && personaFinal.name.familia != null && personaFinal.name.familia.Any() && !string.IsNullOrEmpty(personaFinal.name.familia[0]))
                        {
                            personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                        }

                        listaPersonasDefinitivas.Add(personaFinal);
                    }
                }

                // Encontrar el autor
                foreach (Person persona in listaPersonasDefinitivas)
                {
                    Person personaFinal = persona;

                    // Comprobación por ORCID
                    if (!string.IsNullOrEmpty(personaFinal.ORCID) && personaFinal.ORCID == pPublicacion.correspondingAuthor.ORCID)
                    {
                        pPublicacion.correspondingAuthor = personaFinal;
                        break;
                    }

                    // Comprobración por nombre completo
                    string nombreCompleto1 = string.Empty;
                    if (personaFinal.name.given != null && personaFinal.name.given.Any())
                    {
                        nombreCompleto1 += personaFinal.name.given[0] + " ";
                    }
                    if (personaFinal.name.familia != null && personaFinal.name.familia.Any())
                    {
                        nombreCompleto1 += personaFinal.name.familia[0];
                    }
                    if (!string.IsNullOrEmpty(nombreCompleto1))
                    {
                        personaFinal.name.nombre_completo = new List<string>() { nombreCompleto1.Trim() };
                    }

                    string nombreCompleto2 = string.Empty;
                    if (pPublicacion.correspondingAuthor != null && pPublicacion.correspondingAuthor.name != null && pPublicacion.correspondingAuthor.name.given != null && pPublicacion.correspondingAuthor.name.given.Any())
                    {
                        nombreCompleto2 += pPublicacion.correspondingAuthor.name.given[0] + " ";
                    }
                    if (pPublicacion.correspondingAuthor != null && pPublicacion.correspondingAuthor.name != null && pPublicacion.correspondingAuthor.name.familia != null && pPublicacion.correspondingAuthor.name.familia.Any())
                    {
                        nombreCompleto2 += pPublicacion.correspondingAuthor.name.familia[0];
                    }
                    if (!string.IsNullOrEmpty(nombreCompleto2))
                    {
                        pPublicacion.correspondingAuthor.name.nombre_completo = new List<string>() { nombreCompleto2.Trim() };
                    }

                    if (!string.IsNullOrEmpty(pPublicacion.correspondingAuthor.nick) && GetNameSimilarity(personaFinal.name.nombre_completo[0], pPublicacion.correspondingAuthor.nick) >= 0.01)
                    {
                        pPublicacion.correspondingAuthor = UnirPersonas(personaFinal, pPublicacion.correspondingAuthor);
                        break;
                    }

                    if (personaFinal.name.given != null && personaFinal.name.given.Any() && !string.IsNullOrEmpty(personaFinal.name.given[0]) && personaFinal.name.familia != null && personaFinal.name.familia.Any() && !string.IsNullOrEmpty(personaFinal.name.familia[0]))
                    {
                        personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                    }
                }

                pPublicacion.seqOfAuthors = listaPersonasDefinitivas;
            }

            return pPublicacion;
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
            if (pPersonaFinal.orden == null)
            {
                pPersonaFinal.orden = pPersonaAUnir.orden;
            }

            // Firma
            if (string.IsNullOrEmpty(pPersonaFinal.nick))
            {
                pPersonaFinal.nick = pPersonaAUnir.nick;
            }

            // IDs
            HashSet<string> listaIds = new HashSet<string>();
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
            HashSet<string> listaLinks = new HashSet<string>();
            if (pPersonaFinal.links != null && pPersonaFinal.links.Any())
            {
                foreach (string item in pPersonaFinal.links)
                {
                    listaLinks.Add(item);
                }
            }
            if (pPersonaAUnir.links != null && pPersonaAUnir.links.Any())
            {
                foreach (string item in pPersonaAUnir.links)
                {
                    listaLinks.Add(item);
                }
            }
            if (listaLinks.Any())
            {
                pPersonaFinal.links = listaLinks.ToList();
            }

            // Nombre            
            if (pPersonaFinal.name.given == null || !pPersonaFinal.name.given.Any())
            {
                if (pPersonaAUnir.name != null && pPersonaAUnir.name.given != null && pPersonaAUnir.name.given.Any())
                {
                    pPersonaFinal.name.given = pPersonaAUnir.name.given;
                }
                else if (pPersonaAUnir.name != null && pPersonaAUnir.name.given == null && pPersonaAUnir.name.nombre_completo != null && pPersonaAUnir.name.nombre_completo.Any())
                {
                    pPersonaFinal.name.given = new List<string>() { pPersonaAUnir.name.nombre_completo[0].Split(" ")[0] };
                }
            }
            if (pPersonaFinal.name.familia == null || !pPersonaFinal.name.familia.Any())
            {
                if (pPersonaAUnir.name != null && pPersonaAUnir.name.familia != null && pPersonaAUnir.name.familia.Any())
                {
                    pPersonaFinal.name.familia = pPersonaAUnir.name.familia;
                }
                else if (pPersonaAUnir.name != null && pPersonaAUnir.name.familia == null && pPersonaAUnir.name.nombre_completo != null && pPersonaAUnir.name.nombre_completo.Any())
                {
                    pPersonaFinal.name.familia = new List<string>() { pPersonaAUnir.name.nombre_completo[0].Split(" ")[1] };
                }
            }
            if (pPersonaFinal.name.given != null && pPersonaFinal.name.given.Any() && !string.IsNullOrEmpty(pPersonaFinal.name.given[0]) && pPersonaFinal.name.familia != null && pPersonaFinal.name.familia.Any() && !string.IsNullOrEmpty(pPersonaFinal.name.familia[0]))
            {
                pPersonaFinal.name.nombre_completo[0] = $@"{pPersonaFinal.name.given[0]} {pPersonaFinal.name.familia[0]}";
            }

            // Nick
            if (string.IsNullOrEmpty(pPersonaFinal.nick) && !string.IsNullOrEmpty(pPersonaAUnir.nick))
            {
                pPersonaFinal.nick = pPersonaAUnir.nick;
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
            Publication publicacion = new Publication();
            publicacion.typeOfPublication = pPublicacionScopus.typeOfPublication;
            publicacion.title = pPublicacionScopus.title;
            publicacion.doi = pPublicacionScopus.doi;
            publicacion.dataIssued = new Models.DateTimeValue();
            publicacion.dataIssued.datimeTime = pPublicacionScopus.datimeTime;
            publicacion.url = new HashSet<string>();
            foreach (string item in pPublicacionScopus.url)
            {
                publicacion.url.Add(item);
            }
            if (pPublicacionScopus.correspondingAuthor != null)
            {
                publicacion.correspondingAuthor = new Models.Person();
                publicacion.correspondingAuthor.nick = pPublicacionScopus.correspondingAuthor.nick;
            }
            publicacion.pageStart = pPublicacionScopus.pageStart;
            publicacion.pageEnd = pPublicacionScopus.pageEnd;
            publicacion.volume = pPublicacionScopus.volume;
            publicacion.articleNumber = pPublicacionScopus.articleNumber;
            publicacion.openAccess = pPublicacionScopus.openAccess;
            publicacion.IDs = new List<string>();
            publicacion.IDs.Add(pPublicacionScopus.scopusID);
            publicacion.dataOrigin = "Scopus";
            if (pPublicacionScopus.hasPublicationVenue != null)
            {
                publicacion.hasPublicationVenue = new Models.Source();
                publicacion.hasPublicationVenue.type = pPublicacionScopus.hasPublicationVenue.type;
                publicacion.hasPublicationVenue.name = pPublicacionScopus.hasPublicationVenue.name;
                publicacion.hasPublicationVenue.issn = pPublicacionScopus.hasPublicationVenue.issn;
                publicacion.hasPublicationVenue.eissn = pPublicacionScopus.hasPublicationVenue.eissn;
            }
            if (pPublicacionScopus.hasMetric != null)
            {
                publicacion.hasMetric = new List<Models.PublicationMetric>();
                foreach (PublicationAPI.ROs.Publication.Models.PublicationMetric item in pPublicacionScopus.hasMetric)
                {
                    Models.PublicationMetric metrica = new Models.PublicationMetric();
                    metrica.citationCount = item.citationCount;
                    metrica.metricName = item.metricName;
                    publicacion.hasMetric.Add(metrica);
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
            List<float> scores = new List<float>();

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

        private string ObtenerTextosFirmasNormalizadas(string pText)
        {
            pText = pText.ToLower();
            pText = pText.Trim();
            if (pText.Contains(","))
            {
                pText = (pText.Substring(pText.IndexOf(",") + 1)).Trim() + " " + (pText.Substring(0, pText.IndexOf(","))).Trim();
            }
            pText = pText.Replace("-", " ");
            string textoNormalizado = pText.Normalize(NormalizationForm.FormD);
            Regex reg = new Regex("[^a-zA-Z ]");
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

        private float CompareSingleName(string pNameA, string pNameB)
        {
            HashSet<string> ngramsNameA = GetNGramas(pNameA, 2);
            HashSet<string> ngramsNameB = GetNGramas(pNameB, 2);
            float tokens_comunes = ngramsNameA.Intersect(ngramsNameB).Count();
            float union_tokens = ngramsNameA.Union(ngramsNameB).Count();
            float coeficiente_jackard = tokens_comunes / union_tokens;
            return coeficiente_jackard;
        }

        private HashSet<string> GetNGramas(string pText, int pNgramSize)
        {
            HashSet<string> ngramas = new HashSet<string>();
            int textLength = pText.Length;
            if (pNgramSize == 1)
            {
                for (int i = 0; i < textLength; i++)
                {
                    ngramas.Add(pText[i].ToString());
                }
                return ngramas;
            }

            HashSet<string> ngramasaux = new HashSet<string>();
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
            if (!string.IsNullOrEmpty(pPub.title) && !string.IsNullOrEmpty(pPub.Abstract))
            {
                ObjEnriquecimientoConPdf objEnriquecimiento = new ObjEnriquecimientoConPdf();
                objEnriquecimiento.rotype = "papers";
                objEnriquecimiento.title = pPub.title;
                objEnriquecimiento.abstract_ = pPub.Abstract;

                if (!string.IsNullOrEmpty(pPub.pdf))
                {
                    objEnriquecimiento.pdfurl = pPub.pdf;
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
            if (!string.IsNullOrEmpty(pPub.title))
            {
                ObjEnriquecimientoSinPdf objEnriquecimiento = new ObjEnriquecimientoSinPdf();
                objEnriquecimiento.rotype = "papers";
                objEnriquecimiento.title = pPub.title;
                objEnriquecimiento.abstract_ = pPub.Abstract;

                if (string.IsNullOrEmpty(objEnriquecimiento.abstract_))
                {
                    objEnriquecimiento.abstract_ = pPub.title;
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
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            string result = string.Empty;
            var contentData = new StringContent(pDataEnriquecimiento, System.Text.Encoding.UTF8, "application/json");

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
                Topics_enriquecidos data = null;
                try
                {
                    data = JsonConvert.DeserializeObject<Topics_enriquecidos>(result);
                }
                catch (Exception)
                {
                    return null;
                }

                if (data != null)
                {
                    Dictionary<string, string> dicTopics = new Dictionary<string, string>();
                    foreach (Knowledge_enriquecidos item in data.topics)
                    {
                        if (!dicTopics.ContainsKey(item.word))
                        {
                            dicTopics.Add(item.word, item.porcentaje);
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
