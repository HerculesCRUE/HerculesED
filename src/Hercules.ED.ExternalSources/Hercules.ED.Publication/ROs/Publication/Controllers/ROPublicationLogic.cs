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

namespace PublicationConnect.ROs.Publications.Controllers
{
    public class ROPublicationLogic : PublicationInterface
    {
        List<string> advertencia = new List<string>();
        protected string bareer;
        protected string baseUri { get; set; }

        public List<string> dois_principales = new List<string>();
        public List<string> dois_bibliografia = new List<string>();
        public Dictionary<string, string> headers = new Dictionary<string, string>();
        public Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> metricas_scopus;
        public Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> metricas_scie;
        public Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> metricas_ssci;


        // Configuración.
        readonly ConfigService _Configuracion;

        public ROPublicationLogic(ConfigService pConfig)
        {
            _Configuracion = pConfig;

            Log.Information("Leyendo Excel SCOPUS...");
            this.metricas_scopus = LeerDatosExcel_Scopus(@"Files/Scopus_journal_metric.xlsx");
            Log.Information("Leyendo Excel SCIE WOS...");
            this.metricas_scie = LeerDatosExcel_WoS(@"Files/JCR_SCIE_2020.xlsx");
            Log.Information("Leyendo Excel SSCI WOS...");
            this.metricas_ssci = LeerDatosExcel_WoS(@"Files/JCR_SSCI_2020.xlsx");
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
        public List<Publication> getPublications(string name, string date = "1500-01-01")
        {
            //Declaro el Resultado
            List<Publication> resultado = new List<Publication>();
            Log.Information("Haciendo petición a Scopus...");
            List<Publication> objInicial_Scopus = llamada_Scopus(name, date);
            Log.Information("Haciendo petición a Wos...");
            List<Publication> objInicial_woS = llamada_WoS(name, date);

            if (objInicial_woS != null && objInicial_woS.Count >= 1)
            {
                foreach (Publication pub in objInicial_woS)
                {
                    Log.Information("Lista dois bibliografia...");
                    this.dois_bibliografia = new List<string>();
                    Log.Information("Advertencia...");
                    this.advertencia = pub.problema;
                    string doi = pub.doi;
                    Log.Information("Lista dois principales...");
                    this.dois_principales.Add(doi);
                    Log.Information("Haciendo petición a SemanticScholar...");
                    Publication objInicial_semanticScholar = llamada_Semantic_Scholar(pub.doi);
                    Log.Information("Comparación...");
                    Publication pub_completa = compatacion(pub, objInicial_semanticScholar);
                    Log.Information("Haciendo petición a CrossRef...");
                    Publication objInicial_CrossRef = llamada_CrossRef(doi);
                    pub_completa = compatacion(pub_completa, objInicial_CrossRef);
                    if (objInicial_CrossRef != null)
                    {
                        pub_completa.bibliografia = objInicial_CrossRef.bibliografia;
                    }
                    pub.pdf = llamada_Zenodo(pub.doi);
                    pub.topics_enriquecidos = enriquedicmiento(pub);
                    pub.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub);
                    if (pub.dataIssued != null & pub.hasPublicationVenue.issn != null)
                    {
                        pub.hasPublicationVenue = metrica_journal(pub.hasPublicationVenue, pub.dataIssued.datimeTime, pub.topics_enriquecidos);
                    }
                    if (pub_completa.pdf == "")
                    {
                        pub_completa.pdf = null;
                    }
                    pub_completa = completar_bib(pub_completa);
                    pub_completa = obtener_bib_citas(pub_completa);
                    if (objInicial_Scopus != null && objInicial_Scopus.Count >= 1)
                    {
                        foreach (Publication pub_scopus in objInicial_Scopus)
                        {
                            if (pub_scopus.doi != null)
                            {
                                if (pub_scopus.doi == pub_completa.doi)
                                {
                                    pub_completa = compatacion(pub_completa, pub_scopus);
                                    //todo combinar los erroees! ni puta idea de como hacerlo proqe depende de lo que juntes y lo que no! 
                                }
                            }
                        }
                    }
                    resultado.Add(pub_completa);

                }
            }
            if (objInicial_Scopus.Count >= 1)
            {
                //llamada Scopus para completar publicaciones. 
                foreach (Publication pub_scopus in objInicial_Scopus)
                {
                    this.advertencia = pub_scopus.problema;
                    if (!dois_principales.Contains(pub_scopus.doi))
                    {
                        this.dois_bibliografia = new List<string>();
                        this.advertencia = pub_scopus.problema;
                        string doi = pub_scopus.doi;
                        dois_principales.Add(doi);
                        Publication objInicial_semanticScholar = llamada_Semantic_Scholar(pub_scopus.doi);
                        Publication pub_completa = compatacion(pub_scopus, objInicial_semanticScholar);
                        Publication objInicial_CrossRef = llamada_CrossRef(doi);

                        pub_completa = compatacion(pub_completa, objInicial_CrossRef);
                        if (objInicial_CrossRef != null)
                        {
                            pub_completa.bibliografia = objInicial_CrossRef.bibliografia;
                        }
                        pub_completa.pdf = llamada_Zenodo(pub_completa.doi);
                        pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
                        pub_completa.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_completa);
                        if (pub_completa.dataIssued != null & pub_completa.hasPublicationVenue.issn != null)
                        {
                            pub_completa.hasPublicationVenue = metrica_journal(pub_completa.hasPublicationVenue, pub_completa.dataIssued.datimeTime, pub_completa.topics_enriquecidos);
                        }
                        if (pub_completa.pdf == "")
                        {
                            pub_completa.pdf = null;
                        }
                        pub_completa = completar_bib(pub_completa);
                        pub_completa = obtener_bib_citas(pub_completa);
                        pub_completa.problema = this.advertencia;
                        if (pub_completa != null)
                        {
                            resultado.Add(pub_completa);
                        }
                    }
                }

            }
            string info = JsonConvert.SerializeObject(resultado);
            string path = _Configuracion.GetRutaJsonSalida();
            File.WriteAllText(@"Files/Resultado_final.json", info);
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
                    if (pub.title != null & pub.hasPublicationVenue != null & pub.Abstract != null & pub.seqOfAuthors != null & pub.seqOfAuthors != new List<Person>())
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
                                foreach (Person persona in pub.seqOfAuthors)
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
                                foreach (Person persona in pub.seqOfAuthors)
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
            //try
            //{
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromDays(1);

            var contentData = new StringContent(info, System.Text.Encoding.UTF8, "application/json");
            client.Timeout = TimeSpan.FromDays(1);
            response = client.PostAsync("http://herculesapi.elhuyar.eus/" + uri, contentData).Result;

            // response.EnsureSuccessStatusCode();
            string result = response.Content.ReadAsStringAsync().Result;
            return result;

            //}
            //catch { return null; }
        }


        public Publication obtener_bib_citas(Publication pub)
        {
            string doi = pub.doi;

            // Consulta Open Citations 
            Publication objInicial_OpenCitatons = llamada_open_citations(doi);
            List<Publication> citas = new List<Publication>();

            if (objInicial_OpenCitatons.citas != null)
            {
                foreach (Publication pub_cita in objInicial_OpenCitatons.citas)
                {
                    string doi_cita = pub_cita.doi;
                    Publication objInicial_SemanticScholar = llamada_Semantic_Scholar(doi_cita);
                    Publication pub_2 = this.llamada_CrossRef(doi_cita);
                    Publication pub_completa = compatacion(pub_2, objInicial_SemanticScholar);

                    pub_completa.pdf = llamada_Zenodo(pub_completa.doi);
                    pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
                    pub_completa.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_completa);
                    if (pub_completa.dataIssued != null & pub_completa.hasPublicationVenue.issn != null)
                    {
                        pub_completa.hasPublicationVenue = metrica_journal(pub_completa.hasPublicationVenue, pub_completa.dataIssued.datimeTime, pub_completa.topics_enriquecidos);
                    }
                    if (pub_completa.pdf == "")
                    {
                        pub_completa.pdf = null;
                    }
                    if (pub_completa != null)
                    {
                        citas.Add(pub_completa);
                    }
                }
            }
            if (citas.Count > 0)
            {
                pub.citas = citas;
            }

            List<Publication> bibliografia = new List<Publication>();
            if (objInicial_OpenCitatons.bibliografia != null)
            {
                foreach (Publication pub_bib in objInicial_OpenCitatons.bibliografia)
                {
                    string doi_bib = pub_bib.doi;
                    if (!this.dois_bibliografia.Contains(doi_bib))
                    {
                        this.dois_bibliografia.Add(doi_bib);

                        //llamada Semantic Scholar 
                        Publication objInicial_SemanticScholar = llamada_Semantic_Scholar(doi_bib);
                        Publication pub_2 = this.llamada_CrossRef(doi_bib);
                        Publication pub_completa = compatacion(pub_2, objInicial_SemanticScholar);
                        pub_completa.pdf = llamada_Zenodo(pub_completa.doi);
                        pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
                        pub_completa.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub_completa);
                        if (pub_completa.dataIssued != null & pub_completa.hasPublicationVenue.issn != null)
                        {
                            pub_completa.hasPublicationVenue = metrica_journal(pub_completa.hasPublicationVenue, pub_completa.dataIssued.datimeTime, pub_completa.topics_enriquecidos);
                        }
                        if (pub_completa.pdf == "")
                        {
                            pub_completa.pdf = null;
                        }
                        if (pub_completa != null)
                        {
                            bibliografia.Add(pub_completa);
                        }
                    }

                }
            }
            if (bibliografia.Count > 0)
            {
                if (pub.bibliografia == null)
                {
                    pub.bibliografia = bibliografia;
                }
                else
                {
                    pub.bibliografia.AddRange(bibliografia);
                }
            }
            return pub;
        }

        public Publication completar_bib(Publication pub)
        {
            List<Publication> bib = new List<Publication>();
            if (pub.bibliografia != null)
            {
                foreach (Publication pub_bib in pub.bibliografia)
                {

                    string doi_bib = pub_bib.doi;
                    this.dois_bibliografia.Add(doi_bib);
                    Publication pub_semntic_scholar = this.llamada_Semantic_Scholar(doi_bib);
                    Publication pub_crossRef = this.llamada_CrossRef(doi_bib);
                    Publication pub_final_bib = compatacion(pub_crossRef, pub_semntic_scholar);
                    pub_final_bib.pdf = llamada_Zenodo(pub_final_bib.doi);
                    if (pub_final_bib.pdf == "")
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
                    if (pub_final_bib.dataIssued != null)
                    {
                        if (pub_final_bib.hasPublicationVenue != null)
                        {
                            pub_final_bib.hasPublicationVenue = metrica_journal(pub_final_bib.hasPublicationVenue, pub_final_bib.dataIssued.datimeTime, pub_final_bib.topics_enriquecidos);
                        }
                    }
                    //    }catch{
                    //    string info = JsonConvert.SerializeObject(pub_final_bib);
                    //    Console.Write(info);
                    //}


                    if (pub_final_bib != null)
                    {
                        bib.Add(pub_final_bib);
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
                            pub.url.AddRange(pub_2.url);
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

                    if (pub_1.seqOfAuthors != null)
                    {
                        if (pub_2.seqOfAuthors != null & pub_2.seqOfAuthors != new List<Person>())
                        {
                            pub.seqOfAuthors = unir_autores(pub_1.seqOfAuthors, pub_2.seqOfAuthors);

                        }
                        else
                        {
                            pub.seqOfAuthors = pub_1.seqOfAuthors;
                        }
                    }
                    else
                    {
                        if (pub_2.seqOfAuthors != null & pub_2.seqOfAuthors != new List<Person>())
                        {
                            pub.seqOfAuthors = pub_2.seqOfAuthors;
                        }
                        else
                        {
                            pub.seqOfAuthors = null;
                            //Console.Write("HOLI\n");
                        }
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

                    if (pub_1.hasMetric != null)
                    {
                        pub.hasMetric = pub_1.hasMetric;
                    }
                    if (pub_2.hasMetric != null)
                    {
                        if (pub.hasMetric != null)
                        {
                            pub.hasMetric.AddRange(pub_2.hasMetric);
                        }
                        else { pub.hasMetric = pub_2.hasMetric; }
                    }
                    if (pub_1.hasPublicationVenue != null)
                    {
                        pub.hasPublicationVenue = pub_1.hasPublicationVenue;
                    }
                    else
                    {
                        pub.hasPublicationVenue = pub_2.hasPublicationVenue;
                    }
                    // if (pub_1.bibliografia != null)
                    // {
                    //     if (pub_2.bibliografia != null)
                    //     {
                    //         //Esto no va a apasar! 
                    //     }
                    //     else
                    //     {
                    //         pub.bibliografia = pub_1.bibliografia;
                    //     }
                    // }
                    // else
                    // {
                    //     if (pub_2.bibliografia != null)
                    //     {
                    //         pub.bibliografia = pub_2.bibliografia;
                    //     }
                    //     else
                    //     {
                    //         pub.bibliografia = null;
                    //     }

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


        public List<Person> unir_autores(List<Person> conjunto_1, List<Person> conjunto_2)
        {
            //Console.Write("Iniciando unir autores--------------------------------------");
            List<Person> lista_autores_no_iguales = new List<Person>();
            List<Person> conjunto = new List<Person>();

            //Console.Write(conjunto.Count() + "\n");

            foreach (Person person_2 in conjunto_2)
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
                    if (person_2.name.given != null)
                    {
                        name = person_2.name.given[0];
                    }
                    if (person_2.name.familia != null)
                    {
                        familia = person_2.name.familia[0];
                    }
                    if (person_2.name.nombre_completo != null)
                    {
                        completo = person_2.name.nombre_completo[0];
                    }
                }
                if (person_2.IDs != null)
                {
                    ids = person_2.IDs[0];
                }
                if (person_2.links != null)
                {

                    links = person_2.links[0];
                }
                for (int i = 0; i < conjunto_1.Count(); i++)
                {
                    Person person = conjunto_1[i];
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

                    if (orcid != "" & orcid_unificado !="" & orcid_unificado == orcid)
                    {
                        if (unificado != true)
                        {
                            Console.Write("numero -1");
                            conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                            unificado = true;
                            break;
                        }
                    }
                    else if (ids != "" & list_ids.Count>0)
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



        public Person unir_dos_identidades_unicas(Person person, Person person_2)
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
            Person unificada = new Person();
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


        public Source metrica_journal(Source journal_inicial, string fecha, List<Knowledge_enriquecidos> areas_Tematicas)
        {
            string año = fecha.Substring(0, 4);
            List<JournalMetric> metricas_revista = new List<JournalMetric>();
            JournalMetric metrica_revista_scopus = new JournalMetric();
            if (this.metricas_scopus.Keys.ToList().Contains(año))
            {
                if (journal_inicial.name != null)
                {
                    if (this.metricas_scopus[año].Keys.ToList().Contains(journal_inicial.name.ToLower()))
                    {

                        Dictionary<string, Tuple<string, string, string>> diccionario_areas = this.metricas_scopus[año][journal_inicial.name.ToLower()];

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

            if (this.metricas_scie.Keys.ToList().Contains(año))
            {
                if (this.metricas_scie[año].Keys.ToList().Contains(journal_inicial.name.ToLower()))
                {

                    Dictionary<string, Tuple<string, string, string>> diccionario_areas = this.metricas_scie[año][journal_inicial.name.ToLower()];

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

            if (this.metricas_ssci.Keys.ToList().Contains(año))
            {
                if (this.metricas_ssci[año].Keys.ToList().Contains(journal_inicial.name.ToLower()))
                {

                    Dictionary<string, Tuple<string, string, string>> diccionario_areas = this.metricas_ssci[año][journal_inicial.name.ToLower()];

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

        public Publication llamada_Semantic_Scholar(string doi)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlSemanticScholar() + "SemanticScholar/GetROs?doi={0}", doi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Log.Information("Respuesta SemanticScholar --> " + info_publication);
            Publication objInicial_SemanticScholar = JsonConvert.DeserializeObject<Publication>(info_publication);
            return objInicial_SemanticScholar;
        }

        public Publication llamada_open_citations(string doi)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlOpenCitations() + "OpenCitations/GetROs?doi={0}", doi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Log.Information("Respuesta OpenCitations --> " + info_publication);
            if (info_publication == null)
            {
                return null;
            }
            Publication objInicial_OpenCitatons = JsonConvert.DeserializeObject<Publication>(info_publication);
            return objInicial_OpenCitatons;
        }

        public Publication llamada_CrossRef(string doi)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlCrossRef() + "CrossRef/GetROs?doi={0}", doi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Log.Information("Respuesta CrossRef --> " + info_publication);
            if (info_publication == null)
            {
                return null;
            }
            Publication objInicial_CrossRef = JsonConvert.DeserializeObject<Publication>(info_publication);
            return objInicial_CrossRef;
        }
        public List<Publication> llamada_Scopus(string orcid, string date)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlScopus() + "Scopus/GetROs?orcid={0}&date={1}", orcid, date));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Log.Information("Respuesta Scopus --> " + info_publication);
            List<Publication> objInicial_Scopus = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            return objInicial_Scopus;
        }

        public List<Publication> llamada_WoS(string orcid, string date)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlWos() + "WoS/GetROs?orcid={0}&date={1}", orcid, date));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Log.Information("Respuesta WoS --> " + info_publication);
            List<Publication> objInicial_woS = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            return objInicial_woS;
        }

        public string llamada_Zenodo(string name)
        {
            try
            {
                if (name != null)
                {
                    Uri url = new Uri(string.Format(_Configuracion.GetUrlZenodo() + "Zenodo/GetROs?ID={0}", name));
                    string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    Log.Information("Respuesta Zenodo --> " + info_publication);
                    if (info_publication == null | !info_publication.EndsWith(".pdf"))
                    {
                        return null;
                    }
                    else { return info_publication; }
                }
                else { return null; }
            }
            catch { return null; }
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


    }

}
