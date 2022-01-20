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
            Dictionary<string, Publication> dicSemanticScholar = new Dictionary<string, Publication>();
            Dictionary<string, Publication> dicOpenCitations = new Dictionary<string, Publication>();
            Dictionary<string, Publication> dicCrossRef = new Dictionary<string, Publication>();
            Dictionary<string, string> dicZenodo = new Dictionary<string, string>();

            //Declaro el Resultado
            List<Publication> resultado = new List<Publication>();
            List<PublicacionScopus> objInicial_Scopus = null;
            List<Publication> objInicial_woS = null;

            if (pDoi != null)
            {
                Log.Information("Haciendo petición a Scopus...");
                objInicial_Scopus = llamada_Scopus_Doi(pDoi);
                Log.Information("Haciendo petición a Wos...");
                objInicial_woS = llamada_WoS_Doi(pDoi);
            }
            else
            {
                Log.Information("Haciendo petición a Scopus...");
                objInicial_Scopus = llamada_Scopus(name, date);
                Log.Information("Haciendo petición a Wos...");
                objInicial_woS = llamada_WoS(name, date);
            }

            int contadorPubWos = 0;
            if (objInicial_woS != null && objInicial_woS.Count >= 1)
            {
                foreach (Publication pub in objInicial_woS)
                {
                    Log.Information($@"[WoS] Publicación {contadorPubWos}/{objInicial_woS.Count}");
                    this.dois_bibliografia = new List<string>();
                    this.advertencia = pub.problema;
                    string doi = pub.doi;
                    this.dois_principales.Add(doi);
                    Log.Information("[WoS] Haciendo petición a SemanticScholar...");
                    Publication objInicial_semanticScholar = llamadaSemanticScholar(pub.doi, dicSemanticScholar);
                    Log.Information("[WoS] Comparación (SemanticScholar)...");
                    Publication pub_completa = compatacion(pub, objInicial_semanticScholar);
                    Log.Information("[WoS] Haciendo petición a CrossRef...");
                    Publication objInicial_CrossRef = llamadaCrossRef(doi, dicCrossRef);
                    Log.Information("[WoS] Comparación (CrossRef)...");
                    pub_completa = compatacion(pub_completa, objInicial_CrossRef);
                    if (objInicial_CrossRef != null)
                    {
                        pub_completa.bibliografia = objInicial_CrossRef.bibliografia;
                    }
                    Log.Information("[WoS] Haciendo petición a Zenodo...");
                    pub_completa.pdf = llamadaZenodo(pub.doi, dicZenodo);
                    Log.Information("[WoS] Obteniendo topics enriquecidos...");
                    pub_completa.topics_enriquecidos = enriquedicmiento(pub);
                    Log.Information("[WoS] Obteniendo freeTextKeywords enriquecidos...");
                    pub_completa.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub);
                    //if (pub.dataIssued != null && pub.hasPublicationVenue != null && pub.hasPublicationVenue.issn != null)
                    //{
                    //    Log.Information("[WoS] Obteniendo métrica...");
                    //    pub.hasPublicationVenue = metrica_journal(pub.hasPublicationVenue, pub.dataIssued.datimeTime, pub.topics_enriquecidos);
                    //}
                    if (pub_completa.pdf == "")
                    {
                        pub_completa.pdf = null;
                    }
                    Log.Information("[WoS] Obteniendo bibliografia...");
                    pub_completa = completar_bib(pub_completa, dicOpenCitations, dicSemanticScholar, dicCrossRef, dicZenodo);
                    Log.Information("[WoS] Obteniendo citas...");
                    pub_completa = obtener_bib_citas(pub_completa, dicOpenCitations, dicSemanticScholar, dicCrossRef, dicZenodo);
                    if (objInicial_Scopus != null && objInicial_Scopus.Count >= 1)
                    {
                        foreach (PublicacionScopus pub_scopus in objInicial_Scopus)
                        {
                            if (pub_scopus != null)
                            {
                                Publication pubScopus = ObtenerPublicacionDeScopus(pub_scopus);
                                if (pub_scopus.doi != null)
                                {
                                    if (pub_scopus.doi == pub_completa.doi)
                                    {
                                        pub_completa = compatacion(pub_completa, pubScopus);
                                        //todo combinar los erroees! ni puta idea de como hacerlo proqe depende de lo que juntes y lo que no! 
                                    }
                                }
                            }
                        }
                    }

                    // Unificar Autores
                    pub_completa = CompararAutores(pub_completa);

                    if (pub_completa.title != "One or more validation errors occurred.") // TODO
                    {
                        resultado.Add(pub_completa);
                    }
                    contadorPubWos++;
                }
            }

            //int contadoPubScopus = 0;
            //if (objInicial_Scopus != null && objInicial_Scopus.Count >= 1)
            //{
            //    //llamada Scopus para completar publicaciones. 
            //    foreach (PublicacionScopus pub_scopus in objInicial_Scopus)
            //    {
            //        Log.Information($@"[Scopus] Publicación {contadoPubScopus}/{objInicial_Scopus.Count}");
            //        if (!dois_principales.Contains(pub_scopus.doi))
            //        {
            //            Publication pubScopus = ObtenerPublicacionDeScopus(pub_scopus);
            //            this.dois_bibliografia = new List<string>();
            //            string doi = pub_scopus.doi;
            //            this.dois_principales.Add(doi);
            //            Log.Information("[Scopus] Haciendo petición a SemanticScholar...");
            //            Publication objInicial_semanticScholar = llamadaSemanticScholar(pub_scopus.doi, ref dicSemanticScholar);
            //            Log.Information("[Scopus] Comparación (SemanticScholar)...");
            //            Publication pub_completa = compatacion(pubScopus, objInicial_semanticScholar);
            //            Log.Information("[Scopus] Haciendo petición a CrossRef...");
            //            Publication objInicial_CrossRef = llamadaCrossRef(doi, ref dicCrossRef);
            //            Log.Information("[Scopus] Comparación (CrossRef)...");
            //            pub_completa = compatacion(pub_completa, objInicial_CrossRef);
            //            if (objInicial_CrossRef != null)
            //            {
            //                pub_completa.bibliografia = objInicial_CrossRef.bibliografia;
            //            }
            //            Log.Information("[Scopus] Haciendo petición a Zenodo...");
            //            pub_completa.pdf = llamadaZenodo(pub_completa.doi, ref dicZenodo);
            //            Log.Information("[Scopus] Obteniendo topics enriquecidos...");
            //            pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
            //            Log.Information("[Scopus] Obteniendo freeTextKeywords enriquecidos...");
            //            pub_completa.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_completa);
            //            if (pub_completa.dataIssued != null & pub_completa.hasPublicationVenue.issn != null)
            //            {
            //                Log.Information("[Scopus] Obteniendo métrica...");
            //                pub_completa.hasPublicationVenue = metrica_journal(pub_completa.hasPublicationVenue, pub_completa.dataIssued.datimeTime, pub_completa.topics_enriquecidos);
            //            }
            //            if (pub_completa.pdf == "")
            //            {
            //                pub_completa.pdf = null;
            //            }
            //            Log.Information("[Scopus] Obteniendo bibliografia...");
            //            pub_completa = completar_bib(pub_completa, ref dicOpenCitations, ref dicSemanticScholar, ref dicCrossRef, ref dicZenodo);
            //            Log.Information("[Scopus] Obteniendo citas...");
            //            pub_completa = obtener_bib_citas(pub_completa, ref dicOpenCitations, ref dicSemanticScholar, ref dicCrossRef, ref dicZenodo);
            //            pub_completa.problema = this.advertencia;
            //            if (pub_completa != null)
            //            {
            //                resultado.Add(pub_completa);
            //            }
            //        }
            //        contadoPubScopus++;
            //    }

            //}

            //string info = JsonConvert.SerializeObject(resultado);
            //string path = _Configuracion.GetRutaJsonSalida();
            //Log.Information("Escribiendo datos en fichero...");
            //File.WriteAllText($@"Files/ejemplo.json", info);
            return resultado;

        }
        public List<Knowledge_enriquecidos> enriquedicmiento_pal(Publication pub)
        {
            string info;
            enriquecimiento_palabras a = new enriquecimiento_palabras();
            if (pub.title != null & pub.Abstract != null)
            {
                a.rotype = "papers";
                a.title = pub.title;
                a.abstract_ = pub.Abstract;


                info = JsonConvert.SerializeObject(a);
                string info_publication = httpCall_2("specific", info);
                if (info_publication != null)
                {

                    palabras_enriquecidas objInic = JsonConvert.DeserializeObject<palabras_enriquecidas>(info_publication);
                    return objInic.topics;

                }
                return null;
            }
            else { return null; }

        }



        public List<Knowledge_enriquecidos> enriquedicmiento(Publication pub)
        {
            string info = null;
            if (pub != null)
            {
                try
                {
                    if (pub.title != null & pub.hasPublicationVenue != null & pub.Abstract != null & pub.seqOfAuthors != null & pub.seqOfAuthors != new List<Models.Person>())
                    {
                        if (pub.hasPublicationVenue.name != null)
                        {
                            if (pub.pdf != null)
                            {
                                enriquecimiento a = new enriquecimiento();
                                a.rotype = "papers";
                                a.pdfurl = pub.pdf;
                                a.title = pub.title;
                                a.abstract_ = pub.Abstract;
                                a.journal = pub.hasPublicationVenue.name;
                                string names = "";
                                foreach (Models.Person persona in pub.seqOfAuthors)
                                {
                                    if (persona.name != null && persona.name.nombre_completo != null)
                                    {
                                        if (persona.name.nombre_completo.Count > 0)
                                        {
                                            string name = persona.name.nombre_completo[0];
                                            if (name != null)
                                            {
                                                if (names == "")
                                                {
                                                    names = names + name;
                                                }
                                                else
                                                {
                                                    names = names + " & " + name;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (names != "")
                                {
                                    a.author_name = names;
                                    info = JsonConvert.SerializeObject(a);
                                }
                                else { info = null; }
                            }
                            else
                            {
                                enriquecimiento_sin_pdf a = new enriquecimiento_sin_pdf();
                                a.rotype = "papers";
                                a.title = pub.title;
                                a.abstract_ = pub.Abstract;
                                a.journal = pub.hasPublicationVenue.name;

                                string names = "";
                                foreach (Models.Person persona in pub.seqOfAuthors)
                                {
                                    if (persona.name != null)
                                    {
                                        if (persona.name.nombre_completo != null)
                                        {
                                            if (persona.name.nombre_completo.Count > 0)
                                            {
                                                string name = persona.name.nombre_completo[0];
                                                if (name != null)
                                                {
                                                    if (names == "")
                                                    {
                                                        names = names + name;
                                                    }
                                                    else
                                                    {
                                                        names = names + " & " + name;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (names != "")
                                {
                                    a.author_name = names;
                                    info = JsonConvert.SerializeObject(a);
                                }
                                else { info = null; }

                            }
                        }
                        if (info != null)
                        {
                            string info_publication = httpCall_2("thematic", info);
                            if (info_publication != null)
                            {

                                Topics_enriquecidos objInic = JsonConvert.DeserializeObject<Topics_enriquecidos>(info_publication);
                                return objInic.topics;

                            }
                            return null;
                        }
                        else { return null; }
                    }
                    else { return null; }
                }
                catch
                {
                    //string infoo = JsonConvert.SerializeObject(pub);
                    //Console.Write(infoo);
                }

            }
            else { return null; }
            return null;
        }

        public string httpCall_2(string uri, string info)
        {
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromHours(24);
            string result = string.Empty;

            var contentData = new StringContent(info, System.Text.Encoding.UTF8, "application/json");
            client.Timeout = TimeSpan.FromDays(1);

            int contadorIntentos = 1;
            while (result == "")
            {
                if (contadorIntentos > 3)
                {
                    break;
                }

                response = client.PostAsync("http://herculesapi.elhuyar.eus/" + uri, contentData).Result;
                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }
                contadorIntentos++;
            }

            return result;
        }


        public Publication obtener_bib_citas(Publication pub, Dictionary<string, Publication> pDicOpenCitations, Dictionary<string, Publication> pDicSemanticScholar, Dictionary<string, Publication> pDicCrossRef, Dictionary<string, string> pDicZenodo)
        {
            string doi = pub.doi;

            // Consulta Open Citations 
            Publication objInicial_OpenCitatons = llamadaOpenCitations(doi, pDicOpenCitations);
            List<Publication> citas = new List<Publication>();

            if (objInicial_OpenCitatons != null && objInicial_OpenCitatons.citas != null)
            {
                foreach (Publication pub_cita in objInicial_OpenCitatons.citas)
                {

                    string doi_cita = pub_cita.doi;
                    Publication objInicial_SemanticScholar = llamadaSemanticScholar(doi_cita, pDicSemanticScholar);
                    Publication pub_2 = this.llamadaCrossRef(doi_cita, pDicCrossRef);
                    Publication pub_completa = compatacion(pub_2, objInicial_SemanticScholar);

                    // No necesitamos estos datos.
                    pub_completa.citas = null;
                    pub_completa.bibliografia = null;

                    if (pub_completa != null)
                    {
                        pub_completa.pdf = llamadaZenodo(pub_completa.doi, pDicZenodo);
                        pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
                        pub_completa.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_completa);
                        //if (pub_completa.dataIssued != null & pub_completa.hasPublicationVenue.issn != null)
                        //{
                        //    pub_completa.hasPublicationVenue = metrica_journal(pub_completa.hasPublicationVenue, pub_completa.dataIssued.datimeTime, pub_completa.topics_enriquecidos);
                        //}
                        if (pub_completa.pdf == "")
                        {
                            pub_completa.pdf = null;
                        }
                        if (pub_completa != null)
                        {
                            pub_completa = CompararAutoresCitasReferencias(pub_completa);
                            citas.Add(pub_completa);
                        }
                    }
                }
            }
            if (citas.Count > 0)
            {
                if (pub.citas == null)
                {
                    pub.citas = citas;
                }
                else
                {
                    pub.citas.AddRange(citas);
                }
            }

            //List<Publication> bibliografia = new List<Publication>();
            //if (objInicial_OpenCitatons != null && objInicial_OpenCitatons.bibliografia != null)
            //{
            //    foreach (Publication pub_bib in objInicial_OpenCitatons.bibliografia)
            //    {
            //        string doi_bib = pub_bib.doi;
            //        if (!this.dois_bibliografia.Contains(doi_bib))
            //        {
            //            this.dois_bibliografia.Add(doi_bib);

            //            //llamada Semantic Scholar 
            //            Publication objInicial_SemanticScholar = llamadaSemanticScholar(doi_bib, ref pDicSemanticScholar);
            //            Publication pub_2 = this.llamadaCrossRef(doi_bib, ref pDicCrossRef);
            //            Publication pub_completa = compatacion(pub_2, objInicial_SemanticScholar);
            //            if (pub_completa != null)
            //            {
            //                pub_completa.pdf = llamadaZenodo(pub_completa.doi, ref pDicZenodo);
            //                pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
            //                pub_completa.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_completa);
            //                if (pub_completa.dataIssued != null && pub_completa.hasPublicationVenue != null && pub_completa.hasPublicationVenue.issn != null)
            //                {
            //                    pub_completa.hasPublicationVenue = metrica_journal(pub_completa.hasPublicationVenue, pub_completa.dataIssued.datimeTime, pub_completa.topics_enriquecidos);
            //                }
            //                if (pub_completa.pdf == "")
            //                {
            //                    pub_completa.pdf = null;
            //                }
            //                if (pub_completa != null)
            //                {
            //                    bibliografia.Add(pub_completa);
            //                }
            //            }
            //        }

            //    }
            //}
            //if (bibliografia.Count > 0)
            //{
            //    if (pub.bibliografia == null)
            //    {
            //        pub.bibliografia = bibliografia;
            //    }
            //    else
            //    {
            //        pub.bibliografia.AddRange(bibliografia);
            //    }
            //}
            return pub;
        }

        public Publication completar_bib(Publication pub, Dictionary<string, Publication> pDicOpenCitations, Dictionary<string, Publication> pDicSemanticScholar, Dictionary<string, Publication> pDicCrossRef, Dictionary<string, string> pDicZenodo)
        {
            List<Publication> bib = new List<Publication>();
            if (pub.bibliografia != null)
            {
                foreach (Publication pub_bib in pub.bibliografia)
                {
                    if (!string.IsNullOrEmpty(pub_bib.doi))
                    {
                        string doi_bib = pub_bib.doi;
                        this.dois_bibliografia.Add(doi_bib);
                        Publication pub_semntic_scholar = this.llamadaSemanticScholar(doi_bib, pDicSemanticScholar);
                        Publication pub_crossRef = this.llamadaCrossRef(doi_bib, pDicCrossRef);
                        Publication pub_final_bib = compatacion(pub_crossRef, pub_semntic_scholar);

                        // No necesitamos estos datos.
                        pub_final_bib.citas = null;
                        pub_final_bib.bibliografia = null;

                        if (pub_final_bib != null)
                        {
                            pub_final_bib.pdf = llamadaZenodo(pub_final_bib.doi, pDicZenodo);
                            if (string.IsNullOrEmpty(pub_final_bib.pdf))
                            {
                                pub_final_bib.pdf = null;
                            }

                            //Console.Write(pub_final_bib.dataIssued);
                            //Console.Write("\n");
                            pub_final_bib.topics_enriquecidos = enriquedicmiento(pub_final_bib);
                            pub_final_bib.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_final_bib);
                            //Console.Write(pub_final_bib);
                            // try
                            //{
                            //if (pub_final_bib.dataIssued != null)
                            //{
                            //    if (pub_final_bib.hasPublicationVenue != null)
                            //    {
                            //        pub_final_bib.hasPublicationVenue = metrica_journal(pub_final_bib.hasPublicationVenue, pub_final_bib.dataIssued.datimeTime, pub_final_bib.topics_enriquecidos);
                            //    }
                            //}
                            //    }catch{
                            //    string info = JsonConvert.SerializeObject(pub_final_bib);
                            //    Console.Write(info);
                            //}


                            if (pub_final_bib != null)
                            {
                                pub_final_bib = CompararAutoresCitasReferencias(pub_final_bib);
                                bib.Add(pub_final_bib);
                            }
                        }
                    }
                }
                pub.bibliografia = bib;
                return pub;
            }
            return pub;
        }
        public Publication compatacion(Publication pub_1, Publication pub_2)
        {
            Publication pub = new Publication();
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
                    if (pub_1.typeOfPublication != null)
                    {
                        pub.typeOfPublication = pub_1.typeOfPublication;
                    }
                    else
                    {
                        pub.typeOfPublication = pub_2.typeOfPublication;
                    }
                    if (pub_1.title != null)
                    {
                        pub.title = pub_1.title;
                    }
                    else
                    {
                        pub.title = pub_2.title;
                    }

                    if (pub_1.Abstract != null)
                    {
                        pub.Abstract = pub_1.Abstract;
                    }
                    else
                    {
                        pub.Abstract = pub_2.Abstract;
                    }

                    if (pub_1.freetextKeywords != null)
                    {
                        pub.freetextKeywords = pub_1.freetextKeywords;
                        if (pub_2.freetextKeywords != null)
                        {
                            pub.freetextKeywords.AddRange(pub_2.freetextKeywords);
                        }
                    }
                    else
                    {
                        pub.freetextKeywords = pub_2.freetextKeywords;
                    }

                    if (pub_1.language != null)
                    {
                        pub.language = pub_1.language;
                    }
                    else
                    {
                        pub.language = pub_2.language;
                    }
                    if (pub_1.doi != null)
                    {
                        pub.doi = pub_1.doi;
                    }
                    else
                    {
                        pub.doi = pub_2.doi;
                    }
                    if (pub_1.dataIssued != null)
                    {
                        pub.dataIssued = pub_1.dataIssued;
                    }
                    else
                    {
                        if (pub_2.dataIssued != null)
                        {
                            pub.dataIssued = pub_2.dataIssued;
                        }
                        else { pub.dataIssued = null; }
                    }
                    if (pub_1.url != null)
                    {
                        pub.url = pub_1.url;
                        if (pub_2.url != null)
                        {
                            foreach (string item in pub_2.url)
                            {
                                pub.url.Add(item);
                            }
                        }
                    }
                    else
                    {
                        pub.url = pub_2.url;
                    }
                    if (pub_1.correspondingAuthor != null)
                    {
                        pub.correspondingAuthor = pub_1.correspondingAuthor;
                    }
                    else
                    {
                        pub.correspondingAuthor = pub_2.correspondingAuthor;
                    }

                    pub.seqOfAuthors = new List<Models.Person>();
                    if (pub_1.seqOfAuthors != null && pub_1.seqOfAuthors.Count > 0)
                    {
                        pub.seqOfAuthors.AddRange(pub_1.seqOfAuthors);
                    }
                    if (pub_2.seqOfAuthors != null && pub_2.seqOfAuthors.Count > 0)
                    {
                        pub.seqOfAuthors.AddRange(pub_2.seqOfAuthors);
                    }

                    if (pub_1.hasKnowledgeAreas != null)
                    {
                        pub.hasKnowledgeAreas = pub_1.hasKnowledgeAreas;
                        if (pub_2.hasKnowledgeAreas != null)
                        {
                            pub.hasKnowledgeAreas.AddRange(pub_2.hasKnowledgeAreas);
                        }
                    }
                    else
                    {
                        pub.hasKnowledgeAreas = pub_2.hasKnowledgeAreas;
                    }
                    if (pub_1.pageEnd != null)
                    {
                        pub.pageEnd = pub_1.pageEnd;
                    }
                    else
                    {
                        pub.pageEnd = pub_2.pageEnd;
                    }
                    if (pub_1.pageStart != null)
                    {
                        pub.pageStart = pub_1.pageStart;
                    }
                    else
                    {
                        pub.pageStart = pub_2.pageStart;
                    }

                    if (pub_1.IDs != null)
                    {
                        pub.IDs = pub_1.IDs;
                        if (pub_2.IDs != null)
                        {
                            pub.IDs.AddRange(pub_2.IDs);
                        }
                    }
                    else
                    {
                        pub.IDs = pub_2.IDs;
                    }
                    if (pub_1.presentedAt != null)
                    {
                        pub.presentedAt = pub_1.presentedAt;
                    }
                    else
                    {
                        pub.presentedAt = pub_2.presentedAt;
                    }

                    Dictionary<string, Models.PublicationMetric> dicMetricas = new Dictionary<string, Models.PublicationMetric>();

                    if (pub_1.hasMetric != null && pub_1.hasMetric.Any())
                    {
                        foreach (Models.PublicationMetric metrica in pub_1.hasMetric)
                        {
                            if (!dicMetricas.ContainsKey(metrica.metricName))
                            {
                                dicMetricas.Add(metrica.metricName, metrica);
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

                    if (!string.IsNullOrEmpty(pub_1.hasPublicationVenue.type))
                    {
                        pub.hasPublicationVenue.type = pub_1.hasPublicationVenue.type;
                    }
                    else if (pub_2.hasPublicationVenue != null && !string.IsNullOrEmpty(pub_2.hasPublicationVenue.type))
                    {
                        pub.hasPublicationVenue.type = pub_2.hasPublicationVenue.type;
                    }

                    if (!string.IsNullOrEmpty(pub_1.hasPublicationVenue.eissn))
                    {
                        pub.hasPublicationVenue.eissn = pub_1.hasPublicationVenue.eissn;
                    }
                    else if (pub_2.hasPublicationVenue != null && !string.IsNullOrEmpty(pub_2.hasPublicationVenue.eissn))
                    {
                        pub.hasPublicationVenue.eissn = pub_2.hasPublicationVenue.eissn;
                    }

                    if (!string.IsNullOrEmpty(pub_1.hasPublicationVenue.name))
                    {
                        pub.hasPublicationVenue.name = pub_1.hasPublicationVenue.name;
                    }
                    else if (pub_2.hasPublicationVenue != null && !string.IsNullOrEmpty(pub_2.hasPublicationVenue.name))
                    {
                        pub.hasPublicationVenue.name = pub_2.hasPublicationVenue.name;
                    }

                    if (pub_1.hasPublicationVenue.issn != null && pub_1.hasPublicationVenue.issn.Count > 0)
                    {
                        foreach (string item in pub_1.hasPublicationVenue.issn)
                        {
                            if (item != pub.hasPublicationVenue.eissn)
                            {
                                listaIssn.Add(item);
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
                            }
                        }
                    }

                    pub.hasPublicationVenue.issn = listaIssn.ToList();

                    if (!string.IsNullOrEmpty(pub_1.pdf))
                    {
                        pub.pdf = pub_1.pdf;
                    }
                    else
                    {
                        pub.pdf = pub_2.pdf;
                    }

                    if (pub_1.topics_enriquecidos != null)
                    {
                        pub.topics_enriquecidos = pub_1.topics_enriquecidos;
                    }
                    else
                    {
                        pub.topics_enriquecidos = pub_2.topics_enriquecidos;
                    }

                    if (pub_1.freetextKeyword_enriquecidas != null)
                    {
                        pub.freetextKeyword_enriquecidas = pub_1.freetextKeyword_enriquecidas;
                    }
                    else
                    {
                        pub.freetextKeyword_enriquecidas = pub_2.freetextKeyword_enriquecidas;
                    }

                    if (pub_1.bibliografia != null)
                    {
                        pub.bibliografia = pub_1.bibliografia;
                    }
                    else
                    {
                        pub.bibliografia = pub_2.bibliografia;
                    }

                    if (pub_1.citas != null)
                    {
                        pub.citas = pub_1.citas;
                    }
                    else
                    {
                        pub.citas = pub_2.citas;
                    }

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
                    return pub;
                }
            }
        }


        public List<Models.Person> unir_autores(List<Models.Person> conjunto_1, List<Models.Person> conjunto_2)
        {
            //Console.Write("Iniciando unir autores--------------------------------------");
            List<Models.Person> lista_autores_no_iguales = new List<Models.Person>();
            List<Models.Person> conjunto = new List<Models.Person>();

            //Console.Write(conjunto.Count() + "\n");

            foreach (Models.Person person_2 in conjunto_2)
            {
                Boolean unificado = false;
                string orcid_unificado = "";
                if (person_2.ORCID != null) { orcid_unificado = person_2.ORCID; }
                string name = "";
                string familia = "";
                string completo = "";
                string links = "";
                string ids = "";
                if (person_2.name != null)
                {
                    if (person_2.name.given != null && person_2.name.given.Count > 0)
                    {
                        name = person_2.name.given[0];
                    }
                    if (person_2.name.familia != null && person_2.name.familia.Count > 0)
                    {
                        familia = person_2.name.familia[0];
                    }
                    if (person_2.name.nombre_completo != null && person_2.name.nombre_completo.Count > 0)
                    {
                        completo = person_2.name.nombre_completo[0];
                    }
                }
                if (person_2.IDs != null && person_2.IDs.Count > 0)
                {
                    ids = person_2.IDs[0];
                }
                if (person_2.links != null && person_2.links.Count > 0)
                {

                    links = person_2.links[0];
                }
                for (int i = 0; i < conjunto_1.Count(); i++)
                {
                    Models.Person person = conjunto_1[i];
                    string orcid = "";
                    if (person.ORCID != null)
                    {
                        orcid = person.ORCID;
                    }
                    List<string> list_name = new List<string>();
                    List<string> list_familia = new List<string>();
                    List<string> list_nombre_completo = new List<string>();
                    if (person.name != null)
                    {
                        if (person.name.given != null && person.name.given.Count > 0)
                        {
                            list_name = person.name.given;
                        }
                        if (person.name.familia != null && person.name.familia.Count > 0)
                        {

                            list_familia = person.name.familia;
                        }
                        if (person.name.nombre_completo != null && person.name.nombre_completo.Count > 0)
                        {
                            list_nombre_completo = person.name.nombre_completo;

                        }
                    }

                    List<string> list_ids = new List<string>();
                    if (person.IDs != null)
                    {
                        list_ids = person.IDs;
                    }
                    List<string> list_links = new List<string>();
                    if (person.links != null)
                    {
                        list_links = person.links;
                    }

                    if (orcid != "" & orcid_unificado != "" & orcid_unificado == orcid)
                    {
                        if (unificado != true)
                        {
                            Console.Write("numero -1");
                            conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                            unificado = true;
                            break;
                        }
                    }
                    else if (ids != "" & list_ids.Count > 0)
                    {
                        if (list_ids.Contains(ids))
                        {

                            Console.Write("numero 0");
                            conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                            unificado = true;
                            break;

                        }

                    }
                    else
                    {
                        if (list_nombre_completo.Count > 0)
                        {
                            // cuando en los datos de la persona unificada tenemos el nombre completo 
                            for (int k = 0; k < list_nombre_completo.Count; k++)
                            {
                                string nombre_completo = list_nombre_completo[k];
                                if (name != "" & familia != "")
                                {
                                    if (GetNameSimilarity(name + " " + familia, nombre_completo) > 0.9 ||
                                        (GetNameSimilarity(name.Substring(0, 1) + "." + " " + familia, nombre_completo) > 0.9 & name.Substring(0, 1) == nombre_completo.Substring(0, 1)) ||
                                        (GetNameSimilarity(familia + ", " + name.Substring(0, 1) + ".", nombre_completo) > 0.9 & nombre_completo.Substring(nombre_completo.Count() - 2, 2) == name.Substring(0, 1) + ".") ||
                                        GetNameSimilarity(familia + " " + name.Substring(0, 1) + ".", nombre_completo) > 0.9)
                                    {
                                        if (ids != "" & list_ids.Count > 0 & !list_ids.Contains(ids))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            Console.Write("numero 1");

                                            // Console.Write(nombre_completo);
                                            conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                                            unificado = true;
                                            break;
                                        }
                                    }
                                }
                                else if (completo != "")
                                {
                                    if (GetNameSimilarity(completo, nombre_completo) > 0.9)
                                    {
                                        if (ids != "" & list_ids.Count > 0 & !list_ids.Contains(ids))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            Console.Write("numero 2");
                                            conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                                            unificado = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // cuando en los datos de la persona unificada tenemos el nombre completo 
                        else if (list_name.Count > 0 & list_familia.Count > 0)
                        {
                            for (int j = 0; j < list_name.Count; j++)
                            {
                                string name_unificado = list_name[j];
                                for (int k = 0; k < list_familia.Count; k++)
                                {
                                    if (name != "" & familia != "")
                                    {
                                        if (GetNameSimilarity(name + " " + familia, name_unificado + " " + list_familia[k]) > 0.9)
                                        {
                                            if (ids != "" & list_ids.Count > 0 & !list_ids.Contains(ids))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                Console.Write("numero 3");
                                                conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                                                unificado = true;
                                                break;
                                            }
                                            //}


                                        }
                                    }
                                    if (completo != "")
                                    {

                                        if (GetNameSimilarity(name_unificado + " " + list_familia[k], completo) > 0.9 ||
                                            (GetNameSimilarity(name_unificado.Substring(0, 1) + ". " + familia, completo) > 0.9 & name_unificado.Substring(0, 1) == completo.Substring(0, 1)) ||
                                            (GetNameSimilarity(list_familia[k] + ", " + name_unificado.Substring(0, 1) + ".", completo) > 0.9 & completo.Substring(completo.Count() - 2, 2) == name_unificado.Substring(0, 1) + ".") ||
                                            GetNameSimilarity(list_familia[k] + " " + name_unificado.Substring(0, 1) + ".", completo) > 0.9)
                                        {
                                            if (ids != "" & list_ids.Count > 0 & !list_ids.Contains(ids))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (unificado != true)
                                                {
                                                    Console.Write("numero 4");
                                                    conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                                                    unificado = true;
                                                    break;
                                                }
                                            }
                                        }

                                    }
                                }

                            }
                        }


                        //conjunto.Add(conjunto_1[i]);
                    }

                }
                if (unificado == false)
                {
                    lista_autores_no_iguales.Add(person_2);
                }
            }

            conjunto_1.AddRange(lista_autores_no_iguales);
            return conjunto_1;
        }



        public Models.Person unir_dos_identidades_unicas(Models.Person person, Models.Person person_2)
        {
            Console.Write("------------------------\n");
            //string orcid_unificado = person.ORCID;
            //List<string> list_name = person.name.given;
            //List<string> list_familia = person.name.familia;
            //List<string> list_nombre_completo = person.name.nombre_completo;
            //List<string> list_ids = person.IDs;
            //List<string> list_links = person.links;

            // string orcid = person_2.ORCID;
            // List<string> list_name_id2 = person_2.name.given;
            // List<string> list_familia_id2 = person_2.name.familia;
            // List<string> list_nombre_completo_id2 = person_2.name.nombre_completo;
            // List<string> list_ids_id_2 = person_2.IDs;
            // List<string> list_links_id_2 = person_2.links;
            string orcid = "";
            if (person_2.ORCID != null)
            {
                orcid = person_2.ORCID;
            }
            List<string> list_name_id2 = new List<string>();
            List<string> list_familia_id2 = new List<string>();
            List<string> list_nombre_completo_id2 = new List<string>();
            if (person_2.name != null)
            {
                if (person_2.name.given != null)
                {

                    list_name_id2 = person_2.name.given;
                }
                if (person_2.name.familia != null)
                {

                    list_familia_id2 = person_2.name.familia;
                }
                if (person_2.name.nombre_completo != null)
                {

                    list_nombre_completo_id2 = person_2.name.nombre_completo;
                }
            }

            List<string> list_ids_id2 = new List<string>();
            if (person_2.IDs != null)
            {
                list_ids_id2 = person_2.IDs;
            }
            List<string> list_link_id2 = new List<string>();
            if (person_2.links != null)
            {
                list_link_id2 = person_2.links;
            }

            string orcid_unificado = "";
            if (person.ORCID != null)
            {
                orcid_unificado = person.ORCID;
            }
            List<string> list_name = new List<string>();
            List<string> list_familia = new List<string>();
            List<string> list_nombre_completo = new List<string>();
            if (person.name != null)
            {
                if (person.name.given != null)
                {

                    list_name = person.name.given;
                }
                if (person.name.familia != null)
                {

                    list_familia = person.name.familia;
                }
                if (person.name.nombre_completo != null)
                {

                    list_nombre_completo = person.name.nombre_completo;
                }
            }

            List<string> list_ids = new List<string>();
            if (person.IDs != null)
            {
                list_ids = person.IDs;
            }
            List<string> list_links = new List<string>();
            if (person.links != null)
            {
                list_links = person.links;
            }


            if (list_name_id2.Count > 0)
            {
                foreach (string name_ids2 in list_name_id2)
                {
                    if (!list_name_id2.Contains(name_ids2))
                    {
                        list_name.Add(name_ids2);
                    }
                }
            }
            if (list_familia_id2.Count > 0)
            {
                foreach (string familia_ids2 in list_familia_id2)
                {
                    if (!list_familia.Contains(familia_ids2))
                    {
                        list_familia.Add(familia_ids2);
                    }
                }
            }
            if (list_nombre_completo_id2.Count > 0)
            {
                foreach (string ai in list_nombre_completo) { Console.Write(ai); Console.Write("\n"); }
                foreach (string completo_ids2 in list_nombre_completo_id2)
                {
                    if (!list_nombre_completo.Contains(completo_ids2))
                    {
                        //Console.Write(completo_ids2);
                        //Console.Write("\n");
                        list_nombre_completo.Add(completo_ids2);
                    }
                }
            }
            if (list_ids_id2.Count > 0)
            {
                foreach (string ids2 in list_ids_id2)
                {
                    if (!list_ids.Contains(ids2))
                    {
                        list_ids.Add(ids2);
                    }
                }
            }
            if (list_link_id2.Count > 0)
            {
                foreach (string links2 in list_link_id2)
                {
                    if (!list_links.Contains(links2))
                    {
                        list_links.Add(links2);
                    }
                }

            }
            Models.Person unificada = new Models.Person();
            unificada.IDs = list_ids;
            unificada.links = list_links;
            unificada.ORCID = orcid_unificado;
            Name nombre = new Name();
            nombre.given = list_name;
            nombre.familia = list_familia;
            nombre.nombre_completo = list_nombre_completo;
            unificada.name = nombre;
            return unificada;
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
        /// Hace la llamada al API de CrossRef.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación a consultar.</param>
        /// <returns>Objeto Publication con los datos obtenidos.</returns>
        public Publication llamadaCrossRef(string pDoi, Dictionary<string, Publication> pDic)
        {
            Publication objInicial_CrossRef = null;

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
                        objInicial_CrossRef = JsonConvert.DeserializeObject<Publication>(info_publication);
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

        public List<PublicacionScopus> llamada_Scopus(string orcid, string date)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlScopus() + "Scopus/GetROs?orcid={0}&date={1}", orcid, date));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            //Log.Information("Respuesta Scopus --> " + info_publication);
            List<PublicacionScopus> objInicial_Scopus = null;
            try
            {
                objInicial_Scopus = JsonConvert.DeserializeObject<List<PublicacionScopus>>(info_publication);
            }
            catch (Exception error)
            {
                return objInicial_Scopus;
            }
            return objInicial_Scopus;
        }

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
            catch (Exception error)
            {
                return objInicial_Scopus;
            }
            return objInicial_Scopus;
        }

        public List<Publication> llamada_WoS(string orcid, string date)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlWos() + "WoS/GetROs?orcid={0}&date={1}", orcid, date));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            //Log.Information("Respuesta WoS --> " + info_publication);
            List<Publication> objInicial_woS = null;
            try
            {
                objInicial_woS = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            }
            catch (Exception error)
            {
                return objInicial_woS;
            }
            return objInicial_woS;
        }

        public List<Publication> llamada_WoS_Doi(string pDoi)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlWos() + "WoS/GetRoByDoi?pDoi={0}", pDoi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            //Log.Information("Respuesta WoS --> " + info_publication);
            List<Publication> objInicial_woS = null;
            try
            {
                Publication publicacion = JsonConvert.DeserializeObject<Publication>(info_publication);
                objInicial_woS = new List<Publication>() { publicacion };
            }
            catch (Exception error)
            {
                return objInicial_woS;
            }
            return objInicial_woS;
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
                        string info_publication = httpCall(url.ToString(), "GET", headers).Result;
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

        public Publication CompararAutores(Publication pPublicacion)
        {
            // Prioridad --> WOS > SemanticScholar > CrossRef
            Dictionary<string, List<Models.Person>> dicPersonas = new Dictionary<string, List<Models.Person>>();
            dicPersonas.Add("WoS", new List<Models.Person>());
            dicPersonas.Add("SemanticScholar", new List<Models.Person>());
            dicPersonas.Add("CrossRef", new List<Models.Person>());

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
            }

            pPublicacion.seqOfAuthors = listaPersonasDefinitivas;
            return pPublicacion;
        }

        public Publication CompararAutoresCitasReferencias(Publication pPublicacion)
        {
            // Prioridad --> SemanticScholar > CrossRef
            Dictionary<string, List<Models.Person>> dicPersonas = new Dictionary<string, List<Models.Person>>();
            dicPersonas.Add("WoS", new List<Models.Person>());
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

                    if (personaFinal.name.nombre_completo != null && personaFinal.name.nombre_completo.Any() && pPublicacion.correspondingAuthor != null && pPublicacion.correspondingAuthor.name != null && pPublicacion.correspondingAuthor.name.nombre_completo != null && pPublicacion.correspondingAuthor.name.nombre_completo.Any())
                    {
                        if (GetNameSimilarity(personaFinal.name.nombre_completo[0], pPublicacion.correspondingAuthor.name.nombre_completo[0]) >= umbral)
                        {
                            pPublicacion.correspondingAuthor = personaFinal;
                            break;
                        }
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
                pPersonaFinal.name.given = pPersonaAUnir.name.given;
            }
            if (pPersonaFinal.name.familia == null || !pPersonaFinal.name.familia.Any())
            {
                pPersonaFinal.name.familia = pPersonaAUnir.name.familia;
            }

            return pPersonaFinal;
        }

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
            publicacion.correspondingAuthor = new Models.Person();
            publicacion.correspondingAuthor.nick = pPublicacionScopus.correspondingAuthor.nick;
            publicacion.pageStart = pPublicacionScopus.pageStart;
            publicacion.pageEnd = pPublicacionScopus.pageEnd;
            publicacion.IDs = new List<string>();
            publicacion.IDs.Add(pPublicacionScopus.scopusID);
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
    }
}
