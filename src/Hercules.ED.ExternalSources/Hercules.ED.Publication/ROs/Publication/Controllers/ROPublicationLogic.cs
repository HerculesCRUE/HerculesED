using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using PublicationConnect.ROs.Publications.Models;
using System.IO;
using System.Data;
using System.Text;
using ExcelDataReader;
using PublicationAPI.Controllers;
using Serilog;
using PublicationAPI.ROs.Publication.Models;
using System.Threading;
using Person = PublicationConnect.ROs.Publications.Models.Person;
using System.Text.RegularExpressions;

namespace PublicationConnect.ROs.Publications.Controllers
{
    public class ROPublicationLogic
    {
        List<string> advertencia = new List<string>();
        protected string bareer;
        protected string baseUri { get; set; }

        public List<string> dois_principales = new List<string>();
        public List<string> dois_bibliografia = new List<string>();
        public Dictionary<string, string> headers = new Dictionary<string, string>();
        public static Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> metricas_scopus = LeerDatosExcel_Scopus(@"Files/Scopus_journal_metric.xlsx");
        public static Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> metricas_scie = LeerDatosExcel_WoS(@"Files/JCR_SCIE_2020.xlsx");
        public static Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> metricas_ssci = LeerDatosExcel_WoS(@"Files/JCR_SSCI_2020.xlsx");

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
        protected async Task<string> httpCall(string url, string method = "GET", Dictionary<string, string> headers = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromHours(24);
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
        /// <param date="year-month-day"></param>
        /// <returns></returns>
        public List<Publication> getPublications(string name, string date = "1500-01-01", string pDoi = null)
        {
            // Diccionario con las peticiones.
            Tuple<string, Dictionary<Publication, List<PubReferencias>>> dicSemanticScholar;
            Dictionary<string, string> dicZenodo = new Dictionary<string, string>();

            // Lista para almacenar las publicaciones resultantes.
            List<Publication> resultado = new List<Publication>();

            List<PublicacionScopus> objInicial_Scopus = null;
            List<Publication> objInicial_woS = null;
            List<Publication> objInicial_openAire = null;

            if (pDoi != null) // Recuperar una publicación por DOI.
            {
                try
                {
                    Log.Information("Haciendo petición a Scopus...");
                    objInicial_Scopus = llamada_Scopus_Doi(pDoi);
                }
                catch (Exception e)
                {
                    Log.Information("No se ha podido recuperar los datos de Scopus...");
                }

                try
                {
                    Log.Information("Haciendo petición a Wos...");
                    objInicial_woS = llamada_WoS_Doi(pDoi);
                }
                catch (Exception e)
                {
                    Log.Information("No se ha podido recuperar los datos de Wos...");
                }

                try
                {
                    Log.Information("Haciendo petición a OpenAire...");
                    objInicial_openAire = llamada_OpenAire_Doi(pDoi);
                }
                catch (Exception e)
                {
                    Log.Information("No se ha podido recuperar los datos de OpenAire...");
                }
            }
            else // Recuperar las publicaciones de un autor desde 'X' fecha.
            {
                try
                {
                    Log.Information("Haciendo petición a Scopus...");
                    objInicial_Scopus = llamada_Scopus(name, date);
                }
                catch (Exception e)
                {
                    Log.Information("No se ha podido recuperar los datos de Scopus...");
                }

                try
                {
                    Log.Information("Haciendo petición a Wos...");
                    objInicial_woS = llamada_WoS(name, date);
                }
                catch (Exception e)
                {
                    Log.Information("No se ha podido recuperar los datos de Wos...");
                }

                try
                {
                    Log.Information("Haciendo petición a OpenAire...");
                    objInicial_openAire = llamada_OpenAire(name, date);
                }
                catch (Exception e)
                {
                    Log.Information("No se ha podido recuperar los datos de OpenAire...");
                }
            }

            int contadorPubWos = 1;
            try
            {
                if (objInicial_woS != null && objInicial_woS.Any())
                {
                    foreach (Publication pub in objInicial_woS)
                    {
                        Log.Information($@"[WoS] Publicación {contadorPubWos}/{objInicial_woS.Count}");

                        this.dois_bibliografia = new List<string>();
                        if (pub.doi != null && !string.IsNullOrEmpty(pub.doi))
                        {
                            this.dois_principales.Add(pub.doi.ToLower());
                        }

                        // SemanticScholar
                        int contadorSemanticScholar = 1;
                        Tuple<Publication, List<PubReferencias>> dataSemanticScholar = new Tuple<Publication, List<PubReferencias>>(null, null);
                        while (dataSemanticScholar.Item2 == null && contadorSemanticScholar <= 5)
                        {
                            Log.Information($@"[WoS] Haciendo petición a SemanticScholar ({contadorSemanticScholar})...");
                            dataSemanticScholar = ObtenerPubSemanticScholar(pub);
                            if (dataSemanticScholar == null || !string.IsNullOrEmpty(dataSemanticScholar.Item1.title) && dataSemanticScholar.Item2 == null)
                            {
                                break;
                            }
                            contadorSemanticScholar++;
                            if (dataSemanticScholar.Item2 == null && contadorSemanticScholar != 5)
                            {
                                Thread.Sleep(10000); // TODO: Revisar tema de los tiempos de las peticiones.
                            }
                        }

                        Log.Information("[WoS] Comparación (SemanticScholar)...");
                        Publication pub_completa = pub;
                        pub_completa.dataOriginList = new HashSet<string>() { "WoS" };
                        if (dataSemanticScholar != null && dataSemanticScholar.Item2 != null)
                        {
                            pub_completa = compatacion(pub, dataSemanticScholar.Item1);
                            pub_completa.bibliografia = dataSemanticScholar.Item2;
                        }

                        // Zenodo - Archivos pdf...
                        Log.Information("[WoS] Haciendo petición a Zenodo...");
                        pub_completa.pdf = llamadaZenodo(pub.doi, dicZenodo);
                        if (pub_completa.pdf == "")
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
                                    if (pub_scopus.doi != null && !string.IsNullOrEmpty(pub_completa.doi))
                                    {
                                        if (pub_scopus.doi.ToLower() == pub_completa.doi.ToLower())
                                        {
                                            pub_completa = compatacion(pub_completa, pubScopus);
                                        }
                                    }
                                }
                            }
                        }

                        // Completar información faltante con las publicaciones de OpenAire.
                        if (objInicial_openAire != null && objInicial_openAire.Any())
                        {
                            foreach (Publication pub_openAire in objInicial_openAire)
                            {
                                if (pub_openAire != null)
                                {
                                    if (pub_openAire.doi != null && !string.IsNullOrEmpty(pub_completa.doi))
                                    {
                                        if (pub_openAire.doi.ToLower() == pub_completa.doi.ToLower())
                                        {
                                            pub_completa = compatacion(pub_completa, pub_openAire);
                                        }
                                    }
                                }
                            }
                        }

                        // Unificar Autores
                        pub_completa = CompararAutores(pub_completa);

                        // Enriquecimiento
                        pub_completa.title = Regex.Replace(pub_completa.title, "<.*?>", String.Empty);
                        if (pub_completa.Abstract != null)
                        {
                            pub_completa.Abstract = Regex.Replace(pub_completa.Abstract, "<.*?>", String.Empty);
                        }

                        string jsonData = string.Empty;
                        if (string.IsNullOrEmpty(pub_completa.pdf))
                        {
                            jsonData = JsonConvert.SerializeObject(obtenerObjEnriquecimiento(pub_completa));
                        }
                        else
                        {
                            jsonData = JsonConvert.SerializeObject(obtenerObjEnriquecimiento(pub_completa));
                            // TODO: Cuando se envía PDF, no obtiene etiquetas. Si no se envía, si que obtienen.
                            //jsonData = JsonConvert.SerializeObject(obtenerObjEnriquecimientoPdf(pub_completa));
                        }

                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            Log.Information("[WoS] Obteniendo topics enriquecidos...");
                            Dictionary<string, string> listaTopics = getDescriptores(jsonData, "thematic");
                            Log.Information("[WoS] Obteniendo freeTextKeywords enriquecidos...");
                            Dictionary<string, string> listaEtiquetas = getDescriptores(jsonData, "specific");

                            if (listaTopics != null && listaTopics.Any())
                            {
                                pub_completa.topics_enriquecidos = new List<Knowledge_enriquecidos>();
                                foreach (KeyValuePair<string, string> item in listaTopics)
                                {
                                    Knowledge_enriquecidos topic = new Knowledge_enriquecidos();
                                    topic.word = item.Key;
                                    topic.porcentaje = item.Value;
                                    pub_completa.topics_enriquecidos.Add(topic);
                                }
                            }
                            else
                            {
                                pub_completa.topics_enriquecidos = null;
                            }

                            if (listaEtiquetas != null && listaEtiquetas.Any())
                            {
                                pub_completa.freetextKeyword_enriquecidas = new List<Knowledge_enriquecidos>();
                                foreach (KeyValuePair<string, string> item in listaEtiquetas)
                                {
                                    Knowledge_enriquecidos topic = new Knowledge_enriquecidos();
                                    topic.word = item.Key;
                                    topic.porcentaje = item.Value;
                                    pub_completa.freetextKeyword_enriquecidas.Add(topic);
                                }
                            }
                            else
                            {
                                pub_completa.freetextKeyword_enriquecidas = null;
                            }
                        }

                        if (pub_completa != null && !string.IsNullOrEmpty(pub_completa.title) && pub_completa.seqOfAuthors != null && pub_completa.seqOfAuthors.Any() && pub_completa.title != "One or more validation errors occurred.") // TODO
                        {
                            bool encontrado = false;
                            foreach (Person persona in pub_completa.seqOfAuthors)
                            {
                                if (persona.ORCID == name || pDoi != null)
                                {
                                    encontrado = true;
                                    break;
                                }
                            }

                            if (encontrado)
                            {
                                resultado.Add(pub_completa);
                            }
                        }

                        contadorPubWos++;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            Log.Information($@"[WoS] Publicaciones procesadas");

            int contadoPubScopus = 1;
            try
            {
                if (objInicial_Scopus != null && objInicial_Scopus.Any())
                {
                    // Llamada Scopus para completar publicaciones que no se hayan obtenido de WoS.
                    foreach (PublicacionScopus pub_scopus in objInicial_Scopus)
                    {
                        Log.Information($@"[Scopus] Publicación {contadoPubScopus}/{objInicial_Scopus.Count}");
                        if (pub_scopus != null && !string.IsNullOrEmpty(pub_scopus.doi) && !dois_principales.Contains(pub_scopus.doi.ToLower()))
                        {
                            Publication pubScopus = ObtenerPublicacionDeScopus(pub_scopus);

                            this.dois_bibliografia = new List<string>();
                            if (pub_scopus.doi != null && !string.IsNullOrEmpty(pub_scopus.doi))
                            {
                                this.dois_principales.Add(pub_scopus.doi.ToLower());
                            }

                            // SemanticScholar
                            int contadorSemanticScholar = 1;
                            Tuple<Publication, List<PubReferencias>> dataSemanticScholar = new Tuple<Publication, List<PubReferencias>>(null, null);
                            while (dataSemanticScholar.Item2 == null && contadorSemanticScholar <= 5)
                            {
                                Log.Information($@"[Scopus] Haciendo petición a SemanticScholar ({contadorSemanticScholar})...");
                                dataSemanticScholar = ObtenerPubSemanticScholar(pubScopus);
                                contadorSemanticScholar++;
                                if (dataSemanticScholar == null || !string.IsNullOrEmpty(dataSemanticScholar.Item1.title) && dataSemanticScholar.Item2 == null)
                                {
                                    break;
                                }
                                if (dataSemanticScholar.Item2 == null && contadorSemanticScholar != 5)
                                {
                                    Thread.Sleep(10000); // TODO: Revisar tema de los tiempos de las peticiones.
                                }
                            }

                            Log.Information("[Scopus] Comparación (SemanticScholar)...");
                            Publication pub_completa = pubScopus;
                            pub_completa.dataOriginList = new HashSet<string>() { "Scopus" };
                            if (dataSemanticScholar != null && dataSemanticScholar.Item2 != null)
                            {
                                pub_completa = compatacion(pubScopus, dataSemanticScholar.Item1);
                                pub_completa.bibliografia = dataSemanticScholar.Item2;
                            }

                            // Zenodo - Archivos pdf...
                            Log.Information("[Scopus] Haciendo petición a Zenodo...");
                            pub_completa.pdf = llamadaZenodo(pub_completa.doi, dicZenodo);
                            if (pub_completa.pdf == "")
                            {
                                pub_completa.pdf = null;
                            }
                            else
                            {
                                pub_completa.dataOriginList.Add("Zenodo");
                            }

                            // Enriquecimiento
                            pub_completa.title = Regex.Replace(pub_completa.title, "<.*?>", String.Empty);
                            if (pub_completa.Abstract != null)
                            {
                                pub_completa.Abstract = Regex.Replace(pub_completa.Abstract, "<.*?>", String.Empty);
                            }

                            string jsonData = string.Empty;
                            if (string.IsNullOrEmpty(pub_completa.pdf))
                            {
                                jsonData = JsonConvert.SerializeObject(obtenerObjEnriquecimiento(pub_completa));
                            }
                            else
                            {
                                jsonData = JsonConvert.SerializeObject(obtenerObjEnriquecimiento(pub_completa));
                                // TODO: Cuando se envía PDF, no obtiene etiquetas. Si no se envía, si que obtienen.
                                //jsonData = JsonConvert.SerializeObject(obtenerObjEnriquecimientoPdf(pub_completa));
                            }

                            if (!string.IsNullOrEmpty(jsonData))
                            {
                                Log.Information("[Scopus] Obteniendo topics enriquecidos...");
                                Dictionary<string, string> listaTopics = getDescriptores(jsonData, "thematic");
                                Log.Information("[Scopus] Obteniendo freeTextKeywords enriquecidos...");
                                Dictionary<string, string> listaEtiquetas = getDescriptores(jsonData, "specific");

                                if (listaTopics != null && listaTopics.Any())
                                {
                                    pub_completa.topics_enriquecidos = new List<Knowledge_enriquecidos>();
                                    foreach (KeyValuePair<string, string> item in listaTopics)
                                    {
                                        Knowledge_enriquecidos topic = new Knowledge_enriquecidos();
                                        topic.word = item.Key;
                                        topic.porcentaje = item.Value;
                                        pub_completa.topics_enriquecidos.Add(topic);
                                    }
                                }
                                else
                                {
                                    pub_completa.topics_enriquecidos = null;
                                }

                                if (listaEtiquetas != null && listaEtiquetas.Any())
                                {
                                    pub_completa.freetextKeyword_enriquecidas = new List<Knowledge_enriquecidos>();
                                    foreach (KeyValuePair<string, string> item in listaEtiquetas)
                                    {
                                        Knowledge_enriquecidos topic = new Knowledge_enriquecidos();
                                        topic.word = item.Key;
                                        topic.porcentaje = item.Value;
                                        pub_completa.freetextKeyword_enriquecidas.Add(topic);
                                    }
                                }
                                else
                                {
                                    pub_completa.freetextKeyword_enriquecidas = null;
                                }
                            }

                            // Completar información faltante con las publicaciones de OpenAire.
                            if (objInicial_openAire != null && objInicial_openAire.Any())
                            {
                                foreach (Publication pub_openAire in objInicial_openAire)
                                {
                                    if (pub_openAire != null)
                                    {
                                        if (pub_openAire.doi != null && !string.IsNullOrEmpty(pub_completa.doi))
                                        {
                                            if (pub_openAire.doi.ToLower() == pub_completa.doi.ToLower())
                                            {
                                                pub_completa = compatacion(pub_completa, pub_openAire);
                                            }
                                        }
                                    }
                                }
                            }

                            // Unificar Autores
                            pub_completa = CompararAutoresCitasReferencias(pub_completa);

                            if (pub_completa != null && !string.IsNullOrEmpty(pub_completa.title) && pub_completa.seqOfAuthors != null && pub_completa.seqOfAuthors.Any())
                            {
                                bool encontrado = false;
                                foreach (Person persona in pub_completa.seqOfAuthors)
                                {
                                    if (persona.ORCID == name || pDoi != null)
                                    {
                                        encontrado = true;
                                        break;
                                    }
                                }

                                if (encontrado)
                                {
                                    resultado.Add(pub_completa);
                                }
                            }
                        }
                        contadoPubScopus++;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            Log.Information($@"[Scopus] Publicaciones procesadas");

            int contadorPubOpenAire = 1;
            try
            {
                if (objInicial_openAire != null && objInicial_openAire.Any())
                {
                    // Llamada OpenAire para completar publicaciones que no se hayan obtenido de WoS/Scopus.
                    foreach (Publication pub in objInicial_openAire)
                    {
                        Log.Information($@"[OpenAire] Publicación {contadorPubOpenAire}/{objInicial_openAire.Count}");
                        if (pub != null && !string.IsNullOrEmpty(pub.doi) && !dois_principales.Contains(pub.doi.ToLower()))
                        {
                            this.dois_bibliografia = new List<string>();
                            if (pub.doi != null && !string.IsNullOrEmpty(pub.doi))
                            {
                                this.dois_principales.Add(pub.doi.ToLower());
                            }

                            // SemanticScholar
                            int contadorSemanticScholar = 1;
                            Tuple<Publication, List<PubReferencias>> dataSemanticScholar = new Tuple<Publication, List<PubReferencias>>(null, null);
                            while (dataSemanticScholar.Item2 == null && contadorSemanticScholar <= 5)
                            {
                                Log.Information($@"[OpenAire] Haciendo petición a SemanticScholar ({contadorSemanticScholar})...");
                                dataSemanticScholar = ObtenerPubSemanticScholar(pub);
                                contadorSemanticScholar++;
                                if (dataSemanticScholar == null || !string.IsNullOrEmpty(dataSemanticScholar.Item1.title) && dataSemanticScholar.Item2 == null)
                                {
                                    break;
                                }
                                if (dataSemanticScholar.Item2 == null && contadorSemanticScholar != 5)
                                {
                                    Thread.Sleep(10000); // TODO: Revisar tema de los tiempos de las peticiones.
                                }
                            }

                            Log.Information("[OpenAire] Comparación (SemanticScholar)...");
                            Publication pub_completa = pub;
                            pub_completa.dataOriginList = new HashSet<string>() { "OpenAire" };
                            if (dataSemanticScholar != null && dataSemanticScholar.Item2 != null)
                            {
                                pub_completa = compatacion(pub, dataSemanticScholar.Item1);
                                pub_completa.bibliografia = dataSemanticScholar.Item2;
                            }

                            // Zenodo - Archivos pdf...
                            Log.Information("[OpenAire] Haciendo petición a Zenodo...");
                            pub_completa.pdf = llamadaZenodo(pub_completa.doi, dicZenodo);
                            if (pub_completa.pdf == "")
                            {
                                pub_completa.pdf = null;
                            }
                            else
                            {
                                pub_completa.dataOriginList.Add("Zenodo");
                            }

                            // Enriquecimiento
                            pub_completa.title = Regex.Replace(pub_completa.title, "<.*?>", String.Empty);
                            if (pub_completa.Abstract != null)
                            {
                                pub_completa.Abstract = Regex.Replace(pub_completa.Abstract, "<.*?>", String.Empty);
                            }

                            string jsonData = string.Empty;
                            if (string.IsNullOrEmpty(pub_completa.pdf))
                            {
                                jsonData = JsonConvert.SerializeObject(obtenerObjEnriquecimiento(pub_completa));
                            }
                            else
                            {
                                jsonData = JsonConvert.SerializeObject(obtenerObjEnriquecimiento(pub_completa));
                                // TODO: Cuando se envía PDF, no obtiene etiquetas. Si no se envía, si que obtienen.
                                //jsonData = JsonConvert.SerializeObject(obtenerObjEnriquecimientoPdf(pub_completa));
                            }

                            if (!string.IsNullOrEmpty(jsonData))
                            {
                                Log.Information("[OpenAire] Obteniendo topics enriquecidos...");
                                Dictionary<string, string> listaTopics = getDescriptores(jsonData, "thematic");
                                Log.Information("[OpenAire] Obteniendo freeTextKeywords enriquecidos...");
                                Dictionary<string, string> listaEtiquetas = getDescriptores(jsonData, "specific");

                                if (listaTopics != null && listaTopics.Any())
                                {
                                    pub_completa.topics_enriquecidos = new List<Knowledge_enriquecidos>();
                                    foreach (KeyValuePair<string, string> item in listaTopics)
                                    {
                                        Knowledge_enriquecidos topic = new Knowledge_enriquecidos();
                                        topic.word = item.Key;
                                        topic.porcentaje = item.Value;
                                        pub_completa.topics_enriquecidos.Add(topic);
                                    }
                                }
                                else
                                {
                                    pub_completa.topics_enriquecidos = null;
                                }

                                if (listaEtiquetas != null && listaEtiquetas.Any())
                                {
                                    pub_completa.freetextKeyword_enriquecidas = new List<Knowledge_enriquecidos>();
                                    foreach (KeyValuePair<string, string> item in listaEtiquetas)
                                    {
                                        Knowledge_enriquecidos topic = new Knowledge_enriquecidos();
                                        topic.word = item.Key;
                                        topic.porcentaje = item.Value;
                                        pub_completa.freetextKeyword_enriquecidas.Add(topic);
                                    }
                                }
                                else
                                {
                                    pub_completa.freetextKeyword_enriquecidas = null;
                                }
                            }

                            // Unificar Autores
                            pub_completa = CompararAutoresCitasReferencias(pub_completa);

                            if (pub_completa != null && !string.IsNullOrEmpty(pub_completa.title) && pub_completa.seqOfAuthors != null && pub_completa.seqOfAuthors.Any())
                            {
                                bool encontrado = false;
                                foreach (Person persona in pub_completa.seqOfAuthors)
                                {
                                    if (persona.ORCID == name || pDoi != null)
                                    {
                                        encontrado = true;
                                        break;
                                    }
                                }

                                if (encontrado)
                                {
                                    resultado.Add(pub_completa);
                                }
                            }
                        }
                        contadorPubOpenAire++;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            Log.Information($@"[OpenAire] Publicaciones procesadas");

            //string info = JsonConvert.SerializeObject(resultado);
            //string path = _Configuracion.GetRutaJsonSalida();
            //Log.Information("Escribiendo datos en fichero...");
            //File.WriteAllText($@"Files/{name}___{date}.json", info);
            return resultado;
        }
        //public List<Knowledge_enriquecidos> enriquedicmiento_pal(Publication pub)
        //{
        //    string info;
        //    enriquecimiento_palabras a = new enriquecimiento_palabras();
        //    if (pub.title != null & pub.Abstract != null)
        //    {
        //        a.rotype = "papers";
        //        a.title = pub.title;
        //        a.abstract_ = pub.Abstract;
        //        a.journal = null;

        //        info = JsonConvert.SerializeObject(a);
        //        string info_publication = httpCall_2("specific", info);
        //        if (!string.IsNullOrEmpty(info_publication))
        //        {
        //            palabras_enriquecidas objInic = JsonConvert.DeserializeObject<palabras_enriquecidas>(info_publication);
        //            return objInic.topics;

        //        }
        //        return null;
        //    }
        //    else { return null; }

        //}



        //public List<Knowledge_enriquecidos> enriquedicmiento(Publication pub)
        //{
        //    string info = null;
        //    if (pub != null)
        //    {
        //        try
        //        {
        //            if (pub.title != null & pub.hasPublicationVenue != null & pub.Abstract != null & pub.seqOfAuthors != null & pub.seqOfAuthors != new List<Models.Person>())
        //            {
        //                if (pub.hasPublicationVenue.name != null)
        //                {
        //                    if (pub.pdf != null)
        //                    {
        //                        enriquecimiento a = new enriquecimiento();
        //                        a.rotype = "papers";
        //                        a.pdfurl = pub.pdf;
        //                        a.title = pub.title;
        //                        a.abstract_ = pub.Abstract;
        //                        a.journal = pub.hasPublicationVenue.name;
        //                        //string names = "";
        //                        //foreach (Models.Person persona in pub.seqOfAuthors)
        //                        //{
        //                        //    if (persona.name != null && persona.name.nombre_completo != null)
        //                        //    {
        //                        //        if (persona.name.nombre_completo.Count > 0)
        //                        //        {
        //                        //            string name = persona.name.nombre_completo[0];
        //                        //            if (name != null)
        //                        //            {
        //                        //                if (names == "")
        //                        //                {
        //                        //                    names = names + name;
        //                        //                }
        //                        //                else
        //                        //                {
        //                        //                    names = names + " & " + name;
        //                        //                }
        //                        //            }
        //                        //        }
        //                        //    }
        //                        //}
        //                        //if (names != "")
        //                        //{
        //                        //    a.author_name = names;
        //                        //    info = JsonConvert.SerializeObject(a);
        //                        //}
        //                        //else { info = null; }

        //                        info = JsonConvert.SerializeObject(a);
        //                    }
        //                    else
        //                    {
        //                        enriquecimiento_sin_pdf a = new enriquecimiento_sin_pdf();
        //                        a.rotype = "papers";
        //                        a.title = pub.title;
        //                        a.abstract_ = pub.Abstract;
        //                        a.journal = pub.hasPublicationVenue.name;

        //                        //string names = "";
        //                        //foreach (Models.Person persona in pub.seqOfAuthors)
        //                        //{
        //                        //    if (persona.name != null)
        //                        //    {
        //                        //        if (persona.name.nombre_completo != null)
        //                        //        {
        //                        //            if (persona.name.nombre_completo.Count > 0)
        //                        //            {
        //                        //                string name = persona.name.nombre_completo[0];
        //                        //                if (name != null)
        //                        //                {
        //                        //                    if (names == "")
        //                        //                    {
        //                        //                        names = names + name;
        //                        //                    }
        //                        //                    else
        //                        //                    {
        //                        //                        names = names + " & " + name;
        //                        //                    }
        //                        //                }
        //                        //            }
        //                        //        }
        //                        //    }
        //                        //}
        //                        //if (names != "")
        //                        //{
        //                        //    a.author_name = names;
        //                        //    info = JsonConvert.SerializeObject(a);
        //                        //}
        //                        //else { info = null; }

        //                        info = JsonConvert.SerializeObject(a);
        //                    }
        //                }
        //                if (info != null)
        //                {
        //                    string info_publication = httpCall_2("thematic", info);
        //                    if (!string.IsNullOrEmpty(info_publication))
        //                    {

        //                        Topics_enriquecidos objInic = JsonConvert.DeserializeObject<Topics_enriquecidos>(info_publication);
        //                        return objInic.topics;

        //                    }
        //                    return null;
        //                }
        //                else { return null; }
        //            }
        //            else { return null; }
        //        }
        //        catch
        //        {
        //            //string infoo = JsonConvert.SerializeObject(pub);
        //            //Console.Write(infoo);
        //        }

        //    }
        //    else { return null; }
        //    return null;
        //}

        public string httpCall_2(string uri, string info)
        {
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromHours(24);
            string result = string.Empty;

            var contentData = new StringContent(info, System.Text.Encoding.UTF8, "application/json");
            client.Timeout = TimeSpan.FromDays(1);

            int intentos = 10;
            while (true)
            {
                try
                {
                    response = client.PostAsync(_Configuracion.GetUrlEnriquecimiento() + uri, contentData).Result;
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
                        Thread.Sleep(5000);
                    }
                }
            }

            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
            }

            return result;
        }

        /// <summary>
        /// Obtiene las citas de una publicación WoS. A su vez, enriquece las etiquetas y categorías.
        /// </summary>
        /// <param name="pPublicacion">Publicación a obtener las citas.</param>
        ///// <returns></returns>
        //public Publication ObtenerCitas(Publication pPublicacion)
        //{
        //    foreach (string item in pPublicacion.IDs)
        //    {
        //        if (item.ToLower().Contains("wos"))
        //        {
        //            pPublicacion.citas = PeticionWosCitas(item.Split(':')[1]);
        //        }
        //    }

        //    if (pPublicacion.citas != null && pPublicacion.citas.Any())
        //    {
        //        foreach (Publication pub in pPublicacion.citas)
        //        {
        //            pub.topics_enriquecidos = enriquedicmiento(pub);
        //            pub.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub);
        //        }
        //    }

        //    return pPublicacion;
        //}

        //public Publication ObtenerCitasOpenCitations(Publication pub, Dictionary<string, Publication> pDicOpenCitations, Dictionary<string, Publication> pDicSemanticScholar, Dictionary<string, Publication> pDicCrossRef, Dictionary<string, string> pDicZenodo)
        //{
        //    string doi = pub.doi;

        //    // Consulta Open Citations 
        //    Publication objInicial_OpenCitatons = llamadaOpenCitations(doi, pDicOpenCitations);
        //    List<Publication> citas = new List<Publication>();

        //    if (objInicial_OpenCitatons != null && objInicial_OpenCitatons.citas != null)
        //    {
        //        foreach (Publication pub_cita in objInicial_OpenCitatons.citas)
        //        {

        //            string doi_cita = pub_cita.doi;
        //            Publication objInicial_SemanticScholar = llamadaSemanticScholar(doi_cita, pDicSemanticScholar);
        //            Publication pub_2 = this.llamadaCrossRef(doi_cita, pDicCrossRef);
        //            Publication pub_completa = compatacion(pub_2, objInicial_SemanticScholar);

        //            // No necesitamos estos datos.
        //            pub_completa.citas = null;
        //            pub_completa.bibliografia = null;

        //            if (pub_completa != null)
        //            {
        //                pub_completa.pdf = llamadaZenodo(pub_completa.doi, pDicZenodo);

        //                // Enriquecimiento
        //                pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
        //                pub_completa.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_completa);

        //                //if (pub_completa.dataIssued != null & pub_completa.hasPublicationVenue.issn != null)
        //                //{
        //                //    pub_completa.hasPublicationVenue = metrica_journal(pub_completa.hasPublicationVenue, pub_completa.dataIssued.datimeTime, pub_completa.topics_enriquecidos);
        //                //}
        //                if (pub_completa.pdf == "")
        //                {
        //                    pub_completa.pdf = null;
        //                }
        //                if (pub_completa != null)
        //                {
        //                    pub_completa = CompararAutoresCitasReferencias(pub_completa);
        //                    citas.Add(pub_completa);
        //                }
        //            }
        //        }
        //    }
        //    if (citas.Count > 0)
        //    {
        //        if (pub.citas == null)
        //        {
        //            pub.citas = citas;
        //        }
        //        else
        //        {
        //            pub.citas.AddRange(citas);
        //        }
        //    }

        //    // No hace falta, ya que cogemos la bibliografía de CrossRef.
        //    //List<Publication> bibliografia = new List<Publication>();
        //    //if (objInicial_OpenCitatons != null && objInicial_OpenCitatons.bibliografia != null)
        //    //{
        //    //    foreach (Publication pub_bib in objInicial_OpenCitatons.bibliografia)
        //    //    {
        //    //        string doi_bib = pub_bib.doi;
        //    //        if (!this.dois_bibliografia.Contains(doi_bib))
        //    //        {
        //    //            this.dois_bibliografia.Add(doi_bib);

        //    //            //llamada Semantic Scholar 
        //    //            Publication objInicial_SemanticScholar = llamadaSemanticScholar(doi_bib, pDicSemanticScholar);
        //    //            Publication pub_2 = this.llamadaCrossRef(doi_bib, pDicCrossRef);
        //    //            Publication pub_completa = compatacion(pub_2, objInicial_SemanticScholar);

        //    //            // No necesitamos estos datos.
        //    //            pub_completa.citas = null;
        //    //            pub_completa.bibliografia = null;

        //    //            if (pub_completa != null)
        //    //            {
        //    //                pub_completa.pdf = llamadaZenodo(pub_completa.doi, pDicZenodo);
        //    //                pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
        //    //                pub_completa.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_completa);
        //    //                //if (pub_completa.dataIssued != null && pub_completa.hasPublicationVenue != null && pub_completa.hasPublicationVenue.issn != null)
        //    //                //{
        //    //                //    pub_completa.hasPublicationVenue = metrica_journal(pub_completa.hasPublicationVenue, pub_completa.dataIssued.datimeTime, pub_completa.topics_enriquecidos);
        //    //                //}
        //    //                if (pub_completa.pdf == "")
        //    //                {
        //    //                    pub_completa.pdf = null;
        //    //                }
        //    //                if (pub_completa != null)
        //    //                {
        //    //                    bibliografia.Add(pub_completa);
        //    //                }
        //    //            }
        //    //        }

        //    //    }
        //    //}
        //    //if (bibliografia.Count > 0)
        //    //{
        //    //    if (pub.bibliografia == null)
        //    //    {
        //    //        pub.bibliografia = bibliografia;
        //    //    }
        //    //    else
        //    //    {
        //    //        pub.bibliografia.AddRange(bibliografia);
        //    //    }
        //    //}
        //    return pub;
        //}


        public Tuple<Publication, List<PubReferencias>> ObtenerPubSemanticScholar(Publication pub)
        {
            return llamadaRefSemanticScholar(pub.doi);
        }

        //public Publication completar_bib(Publication pub, Dictionary<string, Publication> pDicOpenCitations, Dictionary<string, Publication> pDicSemanticScholar, Dictionary<string, Publication> pDicCrossRef, Dictionary<string, string> pDicZenodo)
        //{
        //    List<Publication> bib = new List<Publication>();
        //    if (pub.bibliografia != null)
        //    {
        //        foreach (Publication pub_bib in pub.bibliografia)
        //        {
        //            if (!string.IsNullOrEmpty(pub_bib.doi))
        //            {
        //                string doi_bib = pub_bib.doi;
        //                this.dois_bibliografia.Add(doi_bib);
        //                Publication pub_semntic_scholar = this.llamadaSemanticScholar(doi_bib, pDicSemanticScholar);
        //                Publication pub_crossRef = this.llamadaCrossRef(doi_bib, pDicCrossRef);
        //                Publication pub_final_bib = compatacion(pub_crossRef, pub_semntic_scholar);

        //                // No necesitamos estos datos.
        //                pub_final_bib.citas = null;
        //                pub_final_bib.bibliografia = null;

        //                if (pub_final_bib != null)
        //                {
        //                    pub_final_bib.pdf = llamadaZenodo(pub_final_bib.doi, pDicZenodo);
        //                    if (string.IsNullOrEmpty(pub_final_bib.pdf))
        //                    {
        //                        pub_final_bib.pdf = null;
        //                    }

        //                    //Console.Write(pub_final_bib.dataIssued);
        //                    //Console.Write("\n");

        //                    // Enriquecimiento
        //                    pub_final_bib.topics_enriquecidos = enriquedicmiento(pub_final_bib);
        //                    pub_final_bib.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_final_bib);

        //                    //Console.Write(pub_final_bib);
        //                    // try
        //                    //{
        //                    //if (pub_final_bib.dataIssued != null)
        //                    //{
        //                    //    if (pub_final_bib.hasPublicationVenue != null)
        //                    //    {
        //                    //        pub_final_bib.hasPublicationVenue = metrica_journal(pub_final_bib.hasPublicationVenue, pub_final_bib.dataIssued.datimeTime, pub_final_bib.topics_enriquecidos);
        //                    //    }
        //                    //}
        //                    //    }catch{
        //                    //    string info = JsonConvert.SerializeObject(pub_final_bib);
        //                    //    Console.Write(info);
        //                    //}


        //                    if (pub_final_bib != null)
        //                    {
        //                        pub_final_bib = CompararAutoresCitasReferencias(pub_final_bib);
        //                        bib.Add(pub_final_bib);
        //                    }
        //                }
        //            }
        //        }
        //        pub.bibliografia = bib;
        //        return pub;
        //    }
        //    return pub;
        //}

        public Publication compatacion(Publication pub_1, Publication pub_2)
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

                    // Si es un capitulo de libro, no necesita DOI. (Da problemas en el motor de desambiguación.)
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

                    //if (pub_1.citas != null)
                    //{
                    //    pub.citas = pub_1.citas;
                    //}
                    //else
                    //{
                    //    pub.citas = pub_2.citas;
                    //}

                    //}

                    // if (bo)
                    // {
                    //     pub.pdf = llamada_Zenodo(pub.doi);
                    //     pub.topics_enriquecidos = enriquedicmiento(pub);
                    //     pub.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub);
                    // }
                    // if (pub.dataIssued != null & pub.hasPublicationVenue.issn != null)
                    // {
                    //     pub.hasPublicationVenue = metrica_journal(pub.hasPublicationVenue, pub.dataIssued.datimeTime, pub.topics_enriquecidos);
                    // }
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

        public Models.Source metrica_journal(Models.Source journal_inicial, string fecha, List<Knowledge_enriquecidos> areas_Tematicas)
        {
            string año = fecha.Substring(0, 4);
            List<JournalMetric> metricas_revista = new List<JournalMetric>();
            JournalMetric metrica_revista_scopus = new JournalMetric();
            if (metricas_scopus.Keys.ToList().Contains(año))
            {
                if (journal_inicial.name != null)
                {
                    if (metricas_scopus[año].Keys.ToList().Contains(journal_inicial.name.ToLower()))
                    {

                        Dictionary<string, Tuple<string, string, string>> diccionario_areas = metricas_scopus[año][journal_inicial.name.ToLower()];

                        string area = diccionario_areas.Keys.ToList()[0];
                        Boolean boole = false;
                        if (areas_Tematicas != null)
                        {
                            foreach (Knowledge_enriquecidos are_tematica_enriquecida in areas_Tematicas)
                            {
                                if (diccionario_areas.Keys.ToList().Contains(are_tematica_enriquecida.word.ToLower()))
                                {
                                    {
                                        area = are_tematica_enriquecida.word.ToLower();
                                        boole = true;
                                    }
                                }
                            }
                        }
                        //TODO: Unificar los cuartiles. Devolver 'Q1' o 1, pero no devolver de ambos tipos.
                        if (boole == false)
                        {
                            string quartil_inicial = "4";
                            foreach (string area_revista in diccionario_areas.Keys.ToList())
                            {
                                Tuple<string, string, string> tuple = diccionario_areas[area_revista];
                                string Q = tuple.Item1;
                                if (quartil_inicial == "4")
                                {
                                    if (Q == "1")
                                    {
                                        area = area_revista;
                                    }
                                    else if (Q == "2")
                                    {
                                        area = area_revista;
                                    }
                                    else if (Q == "3")
                                    {
                                        area = area_revista;
                                    }
                                }
                                else if (quartil_inicial == "3")
                                {
                                    if (Q == "1")
                                    {
                                        area = area_revista;
                                    }
                                    else if (Q == "2")
                                    {
                                        area = area_revista;
                                    }
                                }
                                else if (quartil_inicial == "2")
                                {
                                    if (Q == "1")
                                    {
                                        area = area_revista;
                                    }

                                }
                                else if (quartil_inicial == "1")
                                {
                                    area = area_revista;
                                }

                            }
                        }
                        Tuple<string, string, string> tupla = diccionario_areas[area];

                        metrica_revista_scopus.impactFactor = tupla.Item3;
                        metrica_revista_scopus.quartile = tupla.Item1;

                        metrica_revista_scopus.ranking = tupla.Item2;
                        metrica_revista_scopus.impactFactorName = "SJR";
                        metricas_revista.Add(metrica_revista_scopus);

                    }
                }
            }
            //------------ lo mismo con la matrica de WoS-SCIE.
            JournalMetric metrica_revista_woS_SCIE = new JournalMetric();

            if (metricas_scie.Keys.ToList().Contains(año))
            {
                if (metricas_scie[año].Keys.ToList().Contains(journal_inicial.name.ToLower()))
                {

                    Dictionary<string, Tuple<string, string, string>> diccionario_areas = metricas_scie[año][journal_inicial.name.ToLower()];

                    string area = diccionario_areas.Keys.ToList()[0];
                    Boolean boole = false;
                    if (areas_Tematicas != null)
                    {
                        foreach (Knowledge_enriquecidos are_tematica_enriquecida in areas_Tematicas)
                        {
                            if (diccionario_areas.Keys.ToList().Contains(are_tematica_enriquecida.word.ToLower()))
                            {
                                {
                                    area = are_tematica_enriquecida.word.ToLower();
                                    boole = true;
                                }
                            }
                        }
                    }
                    if (boole == false)
                    {
                        string quartil_inicial = "Q4";
                        foreach (string area_revista in diccionario_areas.Keys.ToList())
                        {
                            Tuple<string, string, string> tuple = diccionario_areas[area_revista];
                            string Q = tuple.Item1;
                            if (quartil_inicial == "Q4")
                            {
                                if (Q == "Q1")
                                {
                                    area = area_revista;
                                }
                                else if (Q == "Q2")
                                {
                                    area = area_revista;
                                }
                                else if (Q == "Q3")
                                {
                                    area = area_revista;
                                }
                            }
                            else if (quartil_inicial == "Q3")
                            {
                                if (Q == "Q1")
                                {
                                    area = area_revista;
                                }
                                else if (Q == "Q2")
                                {
                                    area = area_revista;
                                }
                            }
                            else if (quartil_inicial == "Q2")
                            {
                                if (Q == "Q1")
                                {
                                    area = area_revista;
                                }

                            }
                            else if (quartil_inicial == "Q1")
                            {
                                area = area_revista;
                            }

                        }
                    }
                    Tuple<string, string, string> tupla = diccionario_areas[area];

                    metrica_revista_woS_SCIE.impactFactor = tupla.Item3;
                    metrica_revista_woS_SCIE.quartile = tupla.Item1;

                    metrica_revista_woS_SCIE.ranking = tupla.Item2;
                    metrica_revista_woS_SCIE.impactFactorName = "JIF-SCIE";
                    metricas_revista.Add(metrica_revista_woS_SCIE);

                }

            }

            //------------ lo mismo con la matrica de WoS-SCIE.
            JournalMetric metrica_revista_woS_SSCI = new JournalMetric();

            if (metricas_ssci.Keys.ToList().Contains(año))
            {
                if (metricas_ssci[año].Keys.ToList().Contains(journal_inicial.name.ToLower()))
                {

                    Dictionary<string, Tuple<string, string, string>> diccionario_areas = metricas_ssci[año][journal_inicial.name.ToLower()];

                    string area = diccionario_areas.Keys.ToList()[0];
                    Boolean boole = false;
                    if (areas_Tematicas != null)
                    {
                        foreach (Knowledge_enriquecidos are_tematica_enriquecida in areas_Tematicas)
                        {
                            if (diccionario_areas.Keys.ToList().Contains(are_tematica_enriquecida.word.ToLower()))
                            {
                                {
                                    area = are_tematica_enriquecida.word.ToLower();
                                    boole = true;
                                }

                            }
                        }
                    }
                    if (boole == false)
                    {
                        string quartil_inicial = "Q4";
                        foreach (string area_revista in diccionario_areas.Keys.ToList())
                        {
                            Tuple<string, string, string> tuple = diccionario_areas[area_revista];
                            string Q = tuple.Item1;
                            if (quartil_inicial == "Q4")
                            {
                                if (Q == "Q1")
                                {
                                    area = area_revista;
                                }
                                else if (Q == "Q2")
                                {
                                    area = area_revista;
                                }
                                else if (Q == "Q3")
                                {
                                    area = area_revista;
                                }
                            }
                            else if (quartil_inicial == "Q3")
                            {
                                if (Q == "Q1")
                                {
                                    area = area_revista;
                                }
                                else if (Q == "Q2")
                                {
                                    area = area_revista;
                                }
                            }
                            else if (quartil_inicial == "Q2")
                            {
                                if (Q == "Q1")
                                {
                                    area = area_revista;
                                }

                            }
                            else if (quartil_inicial == "Q1")
                            {
                                area = area_revista;
                            }

                        }
                    }
                    Tuple<string, string, string> tupla = diccionario_areas[area];

                    metrica_revista_woS_SSCI.impactFactor = tupla.Item3;
                    metrica_revista_woS_SSCI.quartile = tupla.Item1;


                    metrica_revista_woS_SSCI.ranking = tupla.Item2;
                    metrica_revista_woS_SSCI.impactFactorName = "JIF-SSCI";
                    metricas_revista.Add(metrica_revista_woS_SSCI);

                }

            }
            if (metricas_revista.Count > 0)
            {
                journal_inicial.hasMetric = metricas_revista;
            }
            return journal_inicial;
        }

        public List<Publication> PeticionWosCitas(string pIdWos)
        {
            List<Publication> listaPublicaciones = new List<Publication>();

            try
            {
                if (!string.IsNullOrEmpty(pIdWos))
                {
                    // URL a la petición.
                    Uri url = new Uri(string.Format(_Configuracion.GetUrlWos() + "WoS/GetCitesByWosId?pWosId={0}", pIdWos));

                    string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    //Log.Information("Respuesta SemanticScholar --> " + info_publication);
                    listaPublicaciones = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
                }
            }
            catch (Exception e)
            {
                Log.Error("Petición SemanticScholar --> " + e);
                return listaPublicaciones;
            }

            return listaPublicaciones;
        }

        /// <summary>
        /// Hace la llamada al API de SemanticScholar.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación a consultar.</param>
        /// <returns>Objeto Publication con los datos recuperados.</returns>
        public Publication llamadaSemanticScholar(string pDoi, Dictionary<string, Publication> pDic)
        {
            Publication objInicial_SemanticScholar = null;

            try
            {
                if (!string.IsNullOrEmpty(pDoi))
                {
                    // URL a la petición.
                    Uri url = new Uri(string.Format(_Configuracion.GetUrlSemanticScholar() + "SemanticScholar/GetROs?doi={0}", pDoi));

                    // Comprobación de la petición.
                    if (!pDic.ContainsKey(pDoi))
                    {
                        string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                        //Log.Information("Respuesta SemanticScholar --> " + info_publication);
                        objInicial_SemanticScholar = JsonConvert.DeserializeObject<Publication>(info_publication);
                        pDic[pDoi] = objInicial_SemanticScholar;
                    }
                    else
                    {
                        objInicial_SemanticScholar = pDic[pDoi];
                    }
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
        /// Hace la llamada al API de OpenCitations.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación a consultar.</param>
        /// <returns>Objeto Publication con los datos recuperados.</returns>
        public Publication llamadaOpenCitations(string pDoi, Dictionary<string, Publication> pDic)
        {
            Publication objInicial_OpenCitatons = null;

            try
            {
                if (!string.IsNullOrEmpty(pDoi))
                {
                    // URL a la petición.
                    Uri url = new Uri(string.Format(_Configuracion.GetUrlOpenCitations() + "OpenCitations/GetROs?doi={0}", pDoi));

                    // Comprobación de la petición.
                    if (!pDic.ContainsKey(pDoi))
                    {
                        string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                        //Log.Information("Respuesta OpenCitations --> " + info_publication);
                        objInicial_OpenCitatons = JsonConvert.DeserializeObject<Publication>(info_publication);
                        pDic[pDoi] = objInicial_OpenCitatons;
                    }
                    else
                    {
                        objInicial_OpenCitatons = pDic[pDoi];
                    }
                }

            }
            catch (Exception e)
            {
                Log.Error("Petición OpenCitations --> " + e);
                return objInicial_OpenCitatons;
            }

            return objInicial_OpenCitatons;
        }

        /// <summary>
        /// Hace la llamada al API de SemanticScholar.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación a consultar.</param>
        /// <returns>Objeto Publication con los datos obtenidos.</returns>
        public Tuple<Publication, List<PubReferencias>> llamadaRefSemanticScholar(string pDoi)
        {
            Tuple<Publication, List<PubReferencias>> objInicial_SemanticScholar = null;

            try
            {
                if (!string.IsNullOrEmpty(pDoi))
                {
                    // URL a la petición.
                    Uri url = new Uri(string.Format(_Configuracion.GetUrlSemanticScholar() + "SemanticScholar/GetReferences?pDoi={0}", pDoi));

                    string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    //Log.Information("Respuesta CrossRef --> " + info_publication);
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
        /// Hace la llamada al API de CrossRef.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación a consultar.</param>
        /// <returns>Objeto Publication con los datos obtenidos.</returns>
        public List<PubReferencias> llamadaCrossRef(string pDoi, Dictionary<string, List<PubReferencias>> pDic)
        {
            List<PubReferencias> objInicial_CrossRef = null;

            try
            {
                if (!string.IsNullOrEmpty(pDoi))
                {
                    // URL a la petición.
                    Uri url = new Uri(string.Format(_Configuracion.GetUrlCrossRef() + "CrossRef/GetROs?doi={0}", pDoi));

                    // Comprobación de la petición.
                    if (!pDic.ContainsKey(pDoi))
                    {
                        string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                        //Log.Information("Respuesta CrossRef --> " + info_publication);
                        objInicial_CrossRef = JsonConvert.DeserializeObject<List<PubReferencias>>(info_publication);
                        pDic[pDoi] = objInicial_CrossRef;
                    }
                    else
                    {
                        objInicial_CrossRef = pDic[pDoi];
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Petición CrossRef --> " + e);
                return objInicial_CrossRef;
            }

            return objInicial_CrossRef;
        }

        /// <summary>
        /// Llamada al API de Web of Science mediante un código de autor.
        /// </summary>
        /// <param name="pOrcid">ORCID del autor.</param>
        /// <returns>Publicación(es) con los datos obtenidos.</returns>
        public List<PublicacionScopus> llamada_Scopus(string pOrcid, string date)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlScopus() + "Scopus/GetROs?orcid={0}&date={1}", pOrcid, date));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            //Log.Information("Respuesta Scopus --> " + info_publication);
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
        public List<PublicacionScopus> llamada_Scopus_Doi(string pDoi)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlScopus() + "Scopus/GetPublicationByDOI?pDoi={0}", pDoi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            //Log.Information("Respuesta Scopus --> " + info_publication);
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
        /// <returns>Publicación(es) con los datos obtenidos.</returns>
        public List<Publication> llamada_WoS(string pOrcid, string date)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlWos() + "WoS/GetROs?orcid={0}&date={1}", pOrcid, date));

            int contadorVeces = 0;
            string info_publication = String.Empty;
            while (true)
            {
                info_publication = httpCall(url.ToString(), "GET", headers).Result;
                if (contadorVeces == 5 || !string.IsNullOrEmpty(info_publication))
                {
                    break;
                }
                contadorVeces++;
                Thread.Sleep(3000);
            }

            //Log.Information("Respuesta WoS --> " + info_publication);
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
        public List<Publication> llamada_WoS_Doi(string pDoi)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlWos() + "WoS/GetRoByDoi?pDoi={0}", pDoi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            //Log.Information("Respuesta WoS --> " + info_publication);
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
        /// <returns>Publicación(es) con los datos obtenidos.</returns>
        public List<Publication> llamada_OpenAire(string pOrcid, string date)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlOpenAire() + "OpenAire/GetROs?orcid={0}&date={1}", pOrcid, date));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            //Log.Information("Respuesta OpenAire --> " + info_publication);
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
        public List<Publication> llamada_OpenAire_Doi(string pDoi)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlOpenAire() + "OpenAire/GetRoByDoi?pDoi={0}", pDoi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            //Log.Information("Respuesta OpenAire --> " + info_publication);
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
        /// <returns>String con la URL del archivo PDF obtenido.</returns>
        public string llamadaZenodo(string pDoi, Dictionary<string, string> pDic)
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
                        string info_publication = httpCall(url.ToString(), "GET", headers).Result.Replace("\"", "");
                        //Log.Information("Respuesta Zenodo --> " + info_publication);
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

        public static Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> LeerDatosExcel_WoS(string pRuta)
        {
            DataSet ds = new DataSet();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var stream = System.IO.File.Open(pRuta, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                        }
                    });
                }
            }
            //año -> area_tematica
            Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> diccionario_final = new Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>>();
            foreach (DataTable tabla in ds.Tables)
            {
                if (tabla.TableName != "JCR_SCIE_counts" & tabla.Namespace != "Journal_Title_Changes")
                {

                    string[] palabras = tabla.TableName.Split("_");
                    string año = palabras[palabras.Count() - 1].Substring(0, 4);
                    Dictionary<string, Tuple<string, string, string>> area_tematica = new Dictionary<string, Tuple<string, string, string>>();
                    //QUARTILE_RANK, CATEGORY_RANKING, IMPACT_FACTOR
                    //CATEGORY_DESCRIPTION
                    Dictionary<string, Dictionary<string, Tuple<string, string, string>>> titulo = new Dictionary<string, Dictionary<string, Tuple<string, string, string>>>();
                    foreach (DataRow fila in ds.Tables[tabla.TableName].Rows)
                    {
                        //No e puede hacer con el issn (columas:  Print ISSN, E-ISSN)
                        Tuple<string, string, string> tupla = new Tuple<string, string, string>(
                            fila["QUARTILE_RANK"].ToString(), fila["CATEGORY_RANKING"].ToString(), fila["IMPACT_FACTOR"].ToString());
                        if (titulo.Keys.Contains(fila["TITLE"].ToString().ToLower()))
                        {
                            area_tematica = titulo[fila["TITLE"].ToString().ToLower()];
                        }
                        else
                        {
                            area_tematica = new Dictionary<string, Tuple<string, string, string>>();
                        }
                        area_tematica[fila["CATEGORY_DESCRIPTION"].ToString().ToLower()] = tupla;
                        titulo[fila["TITLE"].ToString().ToLower()] = area_tematica;
                    }
                    diccionario_final[año] = titulo;
                }
            }
            return diccionario_final;
        }


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
                //int desplazamiento = 0;
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
                                //desplazamiento = Math.Abs(j - i);
                                break;
                            }
                        }
                        else
                        {
                            //Son las dos iniciales
                            score = 0.75f;
                            indexTarget = j + 1;
                            //desplazamiento = Math.Abs(j - i);
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
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^a-zA-Z ]");
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

        public static Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> LeerDatosExcel_Scopus(string pRuta)
        {
            DataSet ds = new DataSet();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var stream = System.IO.File.Open(pRuta, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                        }
                    });
                }
            }
            //año -> area_tematica
            Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> diccionario_final = new Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>>();
            foreach (DataTable tabla in ds.Tables)
            {
                if (tabla.TableName != "About CiteScore" & tabla.Namespace != "ASJC codes")
                {

                    string[] palabras = tabla.TableName.Split(" ");
                    string año = palabras[palabras.Count() - 1];
                    Dictionary<string, Tuple<string, string, string>> area_tematica = new Dictionary<string, Tuple<string, string, string>>();
                    // area_tematica ->  SJR,  Quartile, RANK,
                    Dictionary<string, Dictionary<string, Tuple<string, string, string>>> titulo = new Dictionary<string, Dictionary<string, Tuple<string, string, string>>>();
                    foreach (DataRow fila in ds.Tables[tabla.TableName].Rows)
                    {
                        //No e puede hacer con el issn (columas:  Print ISSN, E-ISSN)
                        //Columnas usadas: Title,  Scopus Sub-Subject Area,
                        Tuple<string, string, string> tupla = new Tuple<string, string, string>(
                            fila["Quartile"].ToString(), fila["RANK"].ToString(), fila["SJR"].ToString());
                        if (titulo.Keys.Contains(fila["Title"].ToString().ToLower()))
                        {
                            area_tematica = titulo[fila["Title"].ToString().ToLower()];
                        }
                        else
                        {
                            area_tematica = new Dictionary<string, Tuple<string, string, string>>();
                        }
                        area_tematica[fila["Scopus Sub-Subject Area"].ToString().ToLower()] = tupla;
                        titulo[fila["Title"].ToString().ToLower()] = area_tematica;
                    }
                    diccionario_final[año] = titulo;
                }
            }
            return diccionario_final;
        }

        /// <summary>
        /// Mediante una publicación, compara todos sus autores entre ellos para quitar duplicados y fusionar la información.
        /// </summary>
        /// <param name="pPublicacion">Publicación a fusionar autores.</param>
        /// <returns>Publicación con los autores fusionados.</returns>
        public Publication CompararAutores(Publication pPublicacion)
        {
            // Prioridad --> WOS > OpenAire > SemanticScholar > CrossRef
            Dictionary<string, List<Models.Person>> dicPersonas = new Dictionary<string, List<Models.Person>>();
            dicPersonas.Add("WoS", new List<Models.Person>());
            dicPersonas.Add("OpenAire", new List<Models.Person>());
            dicPersonas.Add("SemanticScholar", new List<Models.Person>());
            dicPersonas.Add("CrossRef", new List<Models.Person>());


            // Peso.
            double umbral = 0.7;

            foreach (Models.Person persona in pPublicacion.seqOfAuthors)
            {
                if (persona.name != null)
                {
                    dicPersonas[persona.fuente].Add(persona);
                }
            }

            List<Models.Person> listaPersonasDefinitivas = new List<Models.Person>();

            // Unir personas 
            foreach (Models.Person persona in dicPersonas["WoS"])
            {
                Models.Person personaFinal = persona;

                foreach (Models.Person personaCrossRef in dicPersonas["OpenAire"])
                {
                    // Comprobación por ORCID
                    if (!string.IsNullOrEmpty(personaFinal.ORCID))
                    {
                        if (personaFinal.ORCID == personaCrossRef.ORCID)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                            break;
                        }
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

                    if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && personaCrossRef.name.nombre_completo != null && personaCrossRef.name.nombre_completo.Any())
                    {
                        if (GetNameSimilarity(personaFinal.name.nombre_completo[0], personaCrossRef.name.nombre_completo[0]) >= umbral)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                            break;
                        }
                    }

                    if (personaFinal.name.given != null && personaFinal.name.given.Any() && !string.IsNullOrEmpty(personaFinal.name.given[0]) && personaFinal.name.familia != null && personaFinal.name.familia.Any() && !string.IsNullOrEmpty(personaFinal.name.familia[0]))
                    {
                        personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                    }
                }

                foreach (Models.Person personaSemantic in dicPersonas["SemanticScholar"])
                {
                    // Comprobación por ORCID
                    if (!string.IsNullOrEmpty(personaFinal.ORCID))
                    {
                        if (personaFinal.ORCID == personaSemantic.ORCID)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaSemantic);
                            break;
                        }
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

                    if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && personaSemantic.name.nombre_completo != null && personaSemantic.name.nombre_completo.Any())
                    {
                        if (GetNameSimilarity(personaFinal.name.nombre_completo[0], personaSemantic.name.nombre_completo[0]) >= umbral)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaSemantic);
                            break;
                        }
                    }

                    personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                }

                foreach (Models.Person personaCrossRef in dicPersonas["CrossRef"])
                {
                    // Comprobación por ORCID
                    if (!string.IsNullOrEmpty(personaFinal.ORCID))
                    {
                        if (personaFinal.ORCID == personaCrossRef.ORCID)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                            break;
                        }
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

                    if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && personaCrossRef.name.nombre_completo != null && personaCrossRef.name.nombre_completo.Any())
                    {
                        if (GetNameSimilarity(personaFinal.name.nombre_completo[0], personaCrossRef.name.nombre_completo[0]) >= umbral)
                        {
                            personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                            break;
                        }
                    }

                    if (personaFinal.name.given != null && personaFinal.name.given.Any() && !string.IsNullOrEmpty(personaFinal.name.given[0]) && personaFinal.name.familia != null && personaFinal.name.familia.Any() && !string.IsNullOrEmpty(personaFinal.name.familia[0]))
                    {
                        personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                    }
                }

                listaPersonasDefinitivas.Add(personaFinal);
            }

            // Encontrar el autor
            foreach (Models.Person persona in listaPersonasDefinitivas)
            {
                Models.Person personaFinal = persona;

                // Comprobación por ORCID
                if (!string.IsNullOrEmpty(personaFinal.ORCID))
                {
                    if (personaFinal.ORCID == pPublicacion.correspondingAuthor.ORCID)
                    {
                        pPublicacion.correspondingAuthor = personaFinal;
                        break;
                    }
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

                if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && pPublicacion.correspondingAuthor.name.nombre_completo != null && pPublicacion.correspondingAuthor.name.nombre_completo.Any())
                {
                    if (GetNameSimilarity(personaFinal.name.nombre_completo[0], pPublicacion.correspondingAuthor.name.nombre_completo[0]) >= umbral)
                    {
                        pPublicacion.correspondingAuthor = personaFinal;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(pPublicacion.correspondingAuthor.nick))
                {
                    if (GetNameSimilarity(personaFinal.name.nombre_completo[0], pPublicacion.correspondingAuthor.nick) >= 0.01)
                    {
                        pPublicacion.correspondingAuthor = UnirPersonas(personaFinal, pPublicacion.correspondingAuthor);
                        break;
                    }
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
                        if (!string.IsNullOrEmpty(personaFinal.ORCID))
                        {
                            if (personaFinal.ORCID == personaCrossRef.ORCID)
                            {
                                personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                                break;
                            }
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

                        if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && personaCrossRef.name.nombre_completo != null && personaCrossRef.name.nombre_completo.Any())
                        {
                            if (GetNameSimilarity(personaFinal.name.nombre_completo[0], personaCrossRef.name.nombre_completo[0]) >= umbral)
                            {
                                personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                                break;
                            }
                        }

                        if (personaFinal.name.given != null && personaFinal.name.given.Any() && !string.IsNullOrEmpty(personaFinal.name.given[0]) && personaFinal.name.familia != null && personaFinal.name.familia.Any() && !string.IsNullOrEmpty(personaFinal.name.familia[0]))
                        {
                            personaFinal.name.nombre_completo[0] = $@"{personaFinal.name.given[0]} {personaFinal.name.familia[0]}";
                        }
                    }

                    foreach (Models.Person personaCrossRef in dicPersonas["CrossRef"])
                    {
                        // Comprobación por ORCID
                        if (!string.IsNullOrEmpty(personaFinal.ORCID))
                        {
                            if (personaFinal.ORCID == personaCrossRef.ORCID)
                            {
                                personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                                break;
                            }
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

                        if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && personaCrossRef.name.nombre_completo != null && personaCrossRef.name.nombre_completo.Any())
                        {
                            if (GetNameSimilarity(personaFinal.name.nombre_completo[0], personaCrossRef.name.nombre_completo[0]) >= umbral)
                            {
                                personaFinal = UnirPersonas(personaFinal, personaCrossRef);
                                break;
                            }
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
                    foreach (Models.Person personaOpenAire in dicPersonas["OpenAire"])
                    {
                        Models.Person personaFinal = personaOpenAire;

                        // Comprobación por ORCID
                        if (!string.IsNullOrEmpty(personaFinal.ORCID))
                        {
                            if (personaFinal.ORCID == personaOpenAire.ORCID)
                            {
                                personaFinal = UnirPersonas(personaFinal, personaOpenAire);
                                break;
                            }
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
                foreach (Models.Person persona in listaPersonasDefinitivas)
                {
                    Models.Person personaFinal = persona;

                    // Comprobación por ORCID
                    if (!string.IsNullOrEmpty(personaFinal.ORCID))
                    {
                        if (personaFinal.ORCID == pPublicacion.correspondingAuthor.ORCID)
                        {
                            pPublicacion.correspondingAuthor = personaFinal;
                            break;
                        }
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

                    if (!string.IsNullOrEmpty(pPublicacion.correspondingAuthor.nick))
                    {
                        if (GetNameSimilarity(personaFinal.name.nombre_completo[0], pPublicacion.correspondingAuthor.nick) >= 0.01)
                        {
                            pPublicacion.correspondingAuthor = UnirPersonas(personaFinal, pPublicacion.correspondingAuthor);
                            break;
                        }
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
        /// TODO: Revisar la unión de personas que tengan nombre y no apellido.
        /// </summary>
        /// <param name="pPersonaFinal">Persona con prioridad (WoS).</param>
        /// <param name="pPersonaAUnir">Persona a unificar.</param>
        /// <returns></returns>
        public Models.Person UnirPersonas(Models.Person pPersonaFinal, Models.Person pPersonaAUnir)
        {
            // ORCID
            if (string.IsNullOrEmpty(pPersonaFinal.ORCID))
            {
                pPersonaFinal.ORCID = pPersonaAUnir.ORCID;
            }

            // Fuente
            //pPersonaFinal.fuente = "Hércules"; // Fuente de unificación.

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



        public static ObjEnriquecimientoConPdf obtenerObjEnriquecimientoPdf(Publication pPub)
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

        public static ObjEnriquecimientoSinPdf obtenerObjEnriquecimiento(Publication pPub)
        {
            if (!string.IsNullOrEmpty(pPub.title) && !string.IsNullOrEmpty(pPub.Abstract))
            {
                ObjEnriquecimientoSinPdf objEnriquecimiento = new ObjEnriquecimientoSinPdf();
                objEnriquecimiento.rotype = "papers";
                objEnriquecimiento.title = pPub.title;
                objEnriquecimiento.abstract_ = pPub.Abstract;
                return objEnriquecimiento;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, string> getDescriptores(string pDataEnriquecimiento, string pTipo)
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
                catch (Exception e)
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
    }
}
