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
using PublicationConnect.ROs.Publications;
using PublicationConnect.ROs.Publications.Models;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using PublicationConnect.Controllers;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;
//using Newtonsoft.Json.Linq.JObject;
using System.IO;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using System.Text;
using ExcelDataReader;
using System.Linq;
using PublicationAPI.Controllers;
using Microsoft.Extensions.Configuration;

namespace PublicationConnect.ROs.Publications.Controllers
{
    public class ROPublicationLogic : PublicationInterface
    {
        List<string> advertencia = new List<string>();
        protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        protected string baseUri { get; set; }


        // protected List<Publication> publications = new List<Publication>();
        protected List<string> dois_principales = new List<string>();
        protected List<string> dois_bibliografia = new List<string>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> metricas_scopus;

        //protected Dictionary<string, Tuple<Liststring>,List<atring>,List<string>>  dic_orcid; //= new Dictionary<string, Tuple<Liststring>,List<atring>,List<string>>();

        // Configuración.
        readonly ConfigService _Configuracion;

        public ROPublicationLogic(string baseUri, ConfigService pConfig, Dictionary<string, Dictionary<string, Dictionary<string, Tuple<string, string, string>>>> metricas_scopus)
        {

            //this.baseUri = "http://localhost:5000/WoS/GetROs?orcid={0}";
            //this.bareer = bareer;
            _Configuracion = pConfig;
            this.metricas_scopus = metricas_scopus;
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
            List<Publication> objInicial_Scopus = llamada_Scopus(name, date);
            //consulta a WoS 
            List<Publication> objInicial_woS = llamada_WoS(name, date);

            if (objInicial_woS.Count >= 1)
            {
                //Console.Write("hey");
                foreach (Publication pub in objInicial_woS)
                {
                    this.dois_bibliografia = new List<string>();
                    this.advertencia = pub.problema;
                    string doi = pub.doi;
                    dois_principales.Add(doi);
                    Publication objInicial_semanticScholar = llamada_Semantic_Scholar(pub.doi);
                    Publication pub_completa = compatacion(pub, objInicial_semanticScholar, true);
                    Publication objInicial_CrossRef = llamada_CrossRef(doi);
                    pub_completa = compatacion(pub_completa, objInicial_CrossRef, true);
                    //pub_completa.pdf = llamada_Zenodo(pub_completa.doi);
                    if (pub_completa.pdf == "")
                    {
                        pub_completa.pdf = null;
                    }
                    // pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
                    //pub_completa.freetextKeyword_enriquecidas=enriquedicmiento_pal(pub_completa);

                    pub_completa = completar_bib(pub_completa);
                    pub_completa = obtener_bib_citas(pub_completa);
                    if (objInicial_Scopus.Count >= 1)
                    {
                        foreach (Publication pub_scopus in objInicial_Scopus)
                        {
                            if (pub_scopus.doi != null)
                            {
                                if (pub_scopus.doi == pub_completa.doi)
                                {
                                    pub_completa = compatacion(pub_completa, pub_scopus, true);
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
                        Publication pub_completa = compatacion(pub_scopus, objInicial_semanticScholar, false);
                        Publication objInicial_CrossRef = llamada_CrossRef(doi);
                        pub_completa = compatacion(pub_completa, objInicial_CrossRef, true);
                        //pub_completa.pdf = llamada_Zenodo(pub_completa.title);
                        // if (pub_completa.pdf == "")
                        // {
                        //     pub_completa.pdf = null;
                        // }
                        // pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
                        //pub_completa.freetextKeyword_enriquecidas=enriquedicmiento_pal(pub_completa);

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
            //todo! 
            File.WriteAllText(@"C:\Users\mpuer\Desktop\pruebaGNOSSSS.json", info);
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
                //Console.Write("\n");
                //Console.Write("Salida palabras enriquecidas:\n");
                //Console.Write(info_publication);
                //Console.Write("\n");
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
            string info;
            if (pub.title != null & pub.hasPublicationVenue.name != null & pub.Abstract != null & pub.seqOfAuthors != null)
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
                if (info != null)
                {
                    string info_publication = httpCall_2("thematic", info);
                    //Console.Write("\n");
                    //Console.Write("Salida topics entiquecidos\n");
                    //Console.Write(info_publication);
                    //Console.Write("\n");
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

        public string httpCall_2(string uri, string info)
        {
            try
            {
                HttpResponseMessage response = null;
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromDays(1);
                //Console.Write("\n");
                //Console.Write("Entrada de peticion " + uri + "\n");
                //Console.Write(info);
                //Console.Write("\n");

                var contentData = new StringContent(info, System.Text.Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromDays(1);
                response = client.PostAsync("http://herculesapi.elhuyar.eus/" + uri, contentData).Result;

                // response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result;
                return result;

            }
            catch { return null; }
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
                    Publication pub_completa = compatacion(pub_2, objInicial_SemanticScholar, true);

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
                        Publication pub_completa = compatacion(pub_2, objInicial_SemanticScholar, true);

                        if (pub_completa != null)
                        {
                            //pub_completa.pdf = llamada_Zenodo(pub_completa.doi);
                            // if (pub_completa.pdf == "")
                            // {
                            //     pub_completa.pdf = null;
                            // }
                            //  pub_completa.topics_enriquecidos = enriquedicmiento(pub_completa);
                            //pub_completa.freetextKeyword_enriquecidas=enriquedicmiento_pal(pub_completa);

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
                    Publication pub_final_bib = compatacion(pub_crossRef, pub_semntic_scholar, true);

                    if (pub_final_bib != null)
                    {
                        // pub_final_bib.pdf = llamada_Zenodo(pub_final_bib.doi);
                        // if (pub_final_bib.pdf == "")
                        // {
                        //     pub_final_bib.pdf = null;
                        // }
                        // pub_final_bib.topics_enriquecidos = enriquedicmiento(pub_final_bib);
                        // pub_final_bib.freetextKeyword_enriquecidas=enriquedicmiento_pal(pub_final_bib);

                        bib.Add(pub_final_bib);
                    }
                }
                pub.bibliografia = bib;
                return pub;
            }
            return pub;
        }
        public Publication compatacion(Publication pub_1, Publication pub_2, Boolean bo)
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
                        pub.dataIssued = pub_2.dataIssued;
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
                        pub.seqOfAuthors = pub_1.seqOfAuthors;
                        if (pub_2.seqOfAuthors != null)
                        {
                            pub.seqOfAuthors = unir_autores(pub_1.seqOfAuthors, pub_2.seqOfAuthors);
                            //pub.seqOfAuthors = unificarUsuariosConNombreSimple(pub_1.seqOfAuthors, pub_2.seqOfAuthors);
                            //todo comporbacion de que son o no son iguales.
                            //pub.seqOfAuthors.AddRange(pub_2.seqOfAuthors);
                            // pub.seqOfAuthors = pub_1.seqOfAuthors;
                            //pub.seqOfAuthors.AddRange(pub_2.seqOfAuthors);
                        }
                    }
                    else
                    {
                        pub.seqOfAuthors = pub_2.seqOfAuthors;
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
                        pub.hasPublicationVenue = pub_1.hasPublicationVenue;//= metrica_journal(pub_1.hasPublicationVenue, pub.dataIssued.datimeTime, pub.topics_enriquecidos);
                    }
                    else
                    {
                        pub.hasPublicationVenue = pub_2.hasPublicationVenue; //= metrica_journal(pub_2.hasPublicationVenue, pub.dataIssued.datimeTime, pub.topics_enriquecidos);
                    }
                    if (bo)
                    {
                        pub.pdf = llamada_Zenodo(pub.doi);

                        pub.topics_enriquecidos = enriquedicmiento(pub);
                        pub.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub);
                    }
                    if (pub.dataIssued != null & pub.hasPublicationVenue.issn != null)
                    {
                        pub.hasPublicationVenue = metrica_journal(pub.hasPublicationVenue, pub.dataIssued.datimeTime, pub.topics_enriquecidos);
                    }
                    return pub;
                }
            }
        }


        public List<Person> unir_autores(List<Person> conjunto_1, List<Person> conjunto_2)
        {
            List<Person> lista_autores_no_iguales = new List<Person>();

            foreach (Person person_2 in conjunto_2)
            {
                Boolean unificado = false;
                string orcid_unificado = person_2.ORCID;
                string name = person_2.name.given[0];
                string familia = person_2.name.familia[0];
                string completo = person_2.name.nombre_completo[0];
                string ids = person_2.IDs[0];
                string links = person_2.links[0];


                for (int i = 0; i < conjunto_1.Count(); i++)
                {
                    Person person = conjunto_1[i];
                    string orcid = person.ORCID;
                    List<string> list_name = person.name.given;
                    List<string> list_familia = person.name.familia;
                    List<string> list_nombre_completo = person.name.nombre_completo;
                    List<string> list_ids = person.IDs;
                    List<string> list_links = person.links;

                    if (orcid != null & orcid_unificado == orcid)
                    {
                        conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                        unificado = true;
                    }
                    else if (ids != null & list_ids != null)
                    {
                        if (list_ids.Contains(ids))
                        {
                            conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                            unificado = true;

                        }

                    }
                    else
                    {
                        if (list_nombre_completo.Count > 0)
                        {
                            // cuando en los datos de la persona unificada tenemos el nombre completo 
                            //foreach (string nombre_completo in list_nombre_completo)
                            for (int k = 0; k < list_nombre_completo.Count; k++)
                            {
                                string nombre_completo = list_nombre_completo[k];
                                if (name != "" & familia != "")
                                {
                                    if (GetNameSimilarity(name + " " + familia, nombre_completo) > 0.87 ||
                                        GetNameSimilarity(name.Substring(0, 1) + "." + " " + familia, nombre_completo) > 0.87 & name.Substring(0, 1) == nombre_completo.Substring(0, 1) ||
                                        GetNameSimilarity(familia + ", " + name.Substring(0, 1) + ".", nombre_completo) > 0.87 & nombre_completo.Substring(nombre_completo.Count() - 2, 2) == name.Substring(0, 1) + "." ||
                                        GetNameSimilarity(familia + " " + name.Substring(0, 1) + ".", nombre_completo) > 0.87)
                                    {
                                        if (ids != "" & list_ids.Count > 0 & !list_ids.Contains(ids))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                                            unificado = true;
                                        }
                                    }
                                }
                                if (completo != "")
                                {
                                    if (GetNameSimilarity(completo, nombre_completo) > 0.9)
                                    {
                                        if (ids != "" & list_ids.Count > 0 & !list_ids.Contains(ids))
                                        {
                                            Console.Write("------------------------------------------------------------------");
                                            continue;
                                        }
                                        else
                                        {
                                            conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                                            unificado = true;
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
                                        if (GetNameSimilarity(name + " " + familia, name_unificado + " " + list_familia[k]) > 0.87)
                                        {
                                            if (ids != "" & list_ids.Count > 0 & !list_ids.Contains(ids))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                                                unificado = true;
                                            }


                                        }
                                    }
                                    if (completo != "")
                                    {

                                        if (GetNameSimilarity(name_unificado + " " + list_familia[k], completo) > 0.87 ||
                                            GetNameSimilarity(name_unificado.Substring(0, 1) + ". " + familia, completo) > 0.87 & name_unificado.Substring(0, 1) == completo.Substring(0, 1) ||
                                            GetNameSimilarity(list_familia[k] + ", " + name_unificado.Substring(0, 1) + ".", completo) > 0.87 & completo.Substring(completo.Count() - 2, 2) == name_unificado.Substring(0, 1) + "." ||
                                            GetNameSimilarity(list_familia[k] + " " + name_unificado.Substring(0, 1) + ".", completo) > 0.87)
                                        {
                                            if (ids != "" & list_ids.Count > 0 & !list_ids.Contains(ids))
                                            {
                                                continue;
                                            }
                                            else
                                            {

                                                conjunto_1[i] = unir_dos_identidades_unicas(person, person_2);
                                                unificado = true;
                                            }
                                        }

                                    }
                                }

                            }
                        }
                        //llegado a este punto podemos suponer que los id no iguales ni los orcid y que ademas una de las dos personas no tiene id
                    }
                }
                if (unificado == false)
                {
                    conjunto_1.Add(person_2);
                }

                //comprobar si persona es o no persona_comparar
                //si son la misma -> 
            }
            return conjunto_1;
        }



        public Person unir_dos_identidades_unicas(Person person, Person person_2)
        {
            string orcid_unificado = person.ORCID;
            List<string> list_name = person.name.given;
            List<string> list_familia = person.name.familia;
            List<string> list_nombre_completo = person.name.nombre_completo;
            List<string> list_ids = person.IDs;
            List<string> list_links = person.links;

            string orcid = person_2.ORCID;
            List<string> list_name_id2 = person_2.name.given;
            List<string> list_familia_id2 = person_2.name.familia;
            List<string> list_nombre_completo_id2 = person_2.name.nombre_completo;
            List<string> list_ids_id_2 = person_2.IDs;
            List<string> list_links_id_2 = person_2.links;

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
                foreach (string completo_ids2 in list_nombre_completo_id2)
                {
                    if (!list_nombre_completo.Contains(completo_ids2))
                    {
                        list_nombre_completo.Add(completo_ids2);
                    }
                }
            }
            if (list_ids_id_2.Count > 0)
            {
                foreach (string ids2 in list_ids_id_2)
                {
                    if (!list_ids.Contains(ids2))
                    {
                        list_ids.Add(ids2);
                    }
                }
            }
            if (list_links_id_2.Count > 0)
            {
                foreach (string links2 in list_links_id_2)
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
                //foreach (string issn in journal_inicial.issn)
                //{
                //    string issn_sin_gion = issn.Replace("-", string.Empty);
                //   Console.Write(issn_sin_gion);
                //    Console.Write("\n");
                //   if(issn_sin_gion.Substring(0,1)=="0"){
                //      issn_sin_gion= issn_sin_gion.Substring(1);
                // }
                if (this.metricas_scopus[año].Keys.ToList().Contains(journal_inicial.name.ToLower()))
                {
                    Console.Write("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

                    Dictionary<string, Tuple<string, string, string>> diccionario_areas = this.metricas_scopus[año][journal_inicial.name.ToLower()];
                    Console.Write(journal_inicial.name.ToLower());
                    //todo! meter la eleccion entre areas tematicas! 
                    // if(areas_Tematicas!=null){
                    // foreach (string area_revista in diccionario_areas.Keys.ToList())
                    //{
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
                                //}else{
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
                    journal_inicial.hasMetric = metricas_revista;

                    //  }

                    //        }
                    //    }
                    // }

                }
                //     }
                // }
            }
            // }
            return journal_inicial;
        }

        public Publication llamada_Semantic_Scholar(string doi)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlSemanticScholar() + "SemanticScholar/GetROs?doi={0}", doi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Publication objInicial_SemanticScholar = JsonConvert.DeserializeObject<Publication>(info_publication);
            return objInicial_SemanticScholar;
        }

        public Publication llamada_open_citations(string doi)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlOpenCitations() + "OpenCitations/GetROs?doi={0}", doi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
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
            List<Publication> objInicial_Scopus = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            return objInicial_Scopus;
        }

        public List<Publication> llamada_WoS(string orcid, string date)
        {
            Uri url = new Uri(string.Format(_Configuracion.GetUrlWos() + "WoS/GetROs?orcid={0}&date={1}", orcid, date));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
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
        // public string formato_estanda_string_name(string namee)
        // {

        //     namee = namee.Replace("á", "a");
        //     namee = namee.Replace("ó", "o");
        //     namee = namee.Replace("-", " ");
        //     namee = namee.Replace("ú", "u");
        //     namee = namee.Replace("í", "i");
        //     namee = namee.Replace("ñ", "n");
        //     namee = namee.Replace("ó", "o");
        //     namee = namee.Replace("é", "e");
        //     //ä
        //     namee = namee.Replace("´", "");
        //     namee = namee.Replace("'", "");
        //     namee = namee.Replace("~", "");
        //     //todo, si funciona igual hay que considerar mas plaabras! 
        //     //devolver.Add(nam);
        //     // }
        //     return namee;
        //     //  }

        // }


        // public List<string> nombre_completo(string name, string apellido)
        // {

        //     //conseguimos todos los fromas posibles de un nombre simple
        //     //ejemplo -> Pepe -> Pepe P. P son las formas posibles. 
        //     string nombre_normalizado = formato_estanda_string_name(name);
        //     List<string> given = new List<string>() { nombre_normalizado, nombre_normalizado.Substring(0, 1), nombre_normalizado.Substring(0, 1) + "." };
        //     Console.Write(given[1]);
        //     apellido = formato_estanda_string_name(apellido);

        //     List<string> nomb_completos = new List<string>();
        //     // given apellido
        //     foreach (string given_forma in given)
        //     {
        //         //foreach (string apellido_forma in apellido)
        //         //{

        //         //    Console.Write(".....................------------------------");
        //         nomb_completos.Add(given_forma + " " + apellido);
        //         nomb_completos.Add(apellido + " " + given_forma);
        //         nomb_completos.Add(apellido + ", " + given_forma);
        //         //}
        //     }
        //     Console.Write(".....................------------------------");

        //     return nomb_completos;
        // }

        // public Person unificar_dos_personas(Person persona_1, Person persona_2)
        // {
        //     if (persona_1.ORCID == null)
        //     {
        //         if (persona_2.ORCID != null)
        //         {
        //             persona_1.ORCID = persona_2.ORCID;
        //         }
        //     }
        //     if (persona_2.IDs != null)
        //     {
        //         if (persona_1.IDs != null)
        //         {
        //             persona_1.IDs.AddRange(persona_2.IDs);
        //         }
        //         else
        //         {// persona_1.IDs=null
        //             persona_1.IDs = persona_2.IDs;
        //         }
        //     }

        //     if (persona_2.links != null)
        //     {
        //         if (persona_1.links != null)
        //         {
        //             persona_1.links.AddRange(persona_2.links);
        //         }
        //         else
        //         {
        //             persona_1.links = persona_2.links;
        //         }
        //     }

        // return persona_1;

        // }

        // public List<Person> unificarUsuariosConNombreSimple(List<Person> listado_1, List<Person> listado_2)
        // {
        //     List<Person> devolver = new List<Person>();
        //     foreach (Person persona_1 in listado_1)
        //     {
        //         //aqui faltaria algun if
        //         if (persona_1.name.given != null && persona_1.name.given.Count > 0 && persona_1.name.familia != null && persona_1.name.familia.Count > 0)
        //         {
        //             // el given y familia si tienen algo solo tienen 1... 
        //             if (!persona_1.name.familia[0].Contains("-") && !persona_1.name.given[0].Contains(" ") && !persona_1.name.familia[0].Contains(" "))
        //             {
        //                 //los nombres son bonitos 
        //                 string name = persona_1.name.given[0];
        //                 string apellido = persona_1.name.familia[0];
        //                 List<string> nombre_completo_1 = new List<string>();
        //                 nombre_completo_1 = nombre_completo(name, apellido);
        //                 persona_1.name.nombre_completo = nombre_completo_1;

        //                 Person persona_eliminar = new Person();
        //                 if (listado_2.Count >= 1)
        //                 {
        //                     for (int i = 0; i < listado_2.Count; i++)
        //                     {
        //                         Person person_2 = listado_2[i];

        //                         if (person_2.name.nombre_completo != null)
        //                         {
        //                             foreach (string name_completo_persona_2 in person_2.name.nombre_completo)
        //                             {
        //                                 if (nombre_completo_1.Contains(formato_estanda_string_name(name_completo_persona_2)))
        //                                 {
        //                                     //detertada una persona duplicada por lo que toda la infromacion la pasamos a una unica persona. -> combinacion de datos! 
        //                                     //llegados aqui deberiamos aber que uno de los orcis es null. 
        //                                     if (persona_1.ORCID == null)
        //                                     {
        //                                         if (person_2.ORCID != null)
        //                                         {
        //                                             persona_1.ORCID = person_2.ORCID;
        //                                         }
        //                                     }
        //                                     if (person_2.IDs != null)
        //                                     {
        //                                         if (persona_1.IDs != null)
        //                                         {
        //                                             persona_1.IDs.AddRange(person_2.IDs);
        //                                         }
        //                                         else
        //                                         {
        //                                             persona_1.IDs = person_2.IDs;
        //                                         }
        //                                     }

        //                                     if (person_2.links != null)
        //                                     {
        //                                         if (persona_1.links != null)
        //                                         {
        //                                             persona_1.links.AddRange(person_2.links);
        //                                         }
        //                                         else
        //                                         {
        //                                             persona_1.links = person_2.links;
        //                                         }
        //                                     }
        //                                     persona_eliminar = person_2;
        //                                     //  listado_2.Remove(person_2);
        //                                     // }
        //                                 }

        //                             }
        //                         }
        //                     }
        //                     listado_2.Remove(persona_eliminar);
        //                 }
        //             }
        //             else { devolver.Add(persona_1); }
        //         }
        //         else { devolver.Add(persona_1); }
        //     }
        //     return listado_1;
        // }
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


    }

}
