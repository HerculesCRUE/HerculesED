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
        //protected Dictionary<string, Tuple<Liststring>,List<atring>,List<string>>  dic_orcid; //= new Dictionary<string, Tuple<Liststring>,List<atring>,List<string>>();


        public ROPublicationLogic(string baseUri)
        {

            //this.baseUri = "http://localhost:5000/WoS/GetROs?orcid={0}";
            //this.bareer = bareer;


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
            string path = @"C:\Users\mpuer\Desktop\pruebaGNOSSSS.json";
            File.WriteAllText(path, info);
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
                Console.Write("\n");
                Console.Write("Salida palabras enriquecidas:\n");
                Console.Write(info_publication);
                Console.Write("\n");
                if (info_publication != null)
                {
                    // try
                    //{
                    //todo! 
                    palabras_enriquecidas objInic = JsonConvert.DeserializeObject<palabras_enriquecidas>(info_publication);
                    return objInic.topics;
                    // }
                    ///catch { return null; }
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
                    //todo! esto se podria perfecccionar pero de momento es lo que hay! 
                    // por ejemplo se puede hacer que no pueda estar la misma persona dos vece sy cosas asi... 
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
                    //todo! esto se podria perfecccionar pero de momento es lo que hay! 
                    // por ejemplo se puede hacer que no pueda estar la misma persona dos vece sy cosas asi... 
                    foreach (Person persona in pub.seqOfAuthors)
                    { 
                        if(persona.name!=null){
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
                        }}
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
                    Console.Write("\n");
                    Console.Write("Salida topics entiquecidos\n");
                    Console.Write(info_publication);
                    Console.Write("\n");
                    if (info_publication != null)
                    {
                        // try
                        //{
                        Topics_enriquecidos objInic = JsonConvert.DeserializeObject<Topics_enriquecidos>(info_publication);
                        return objInic.topics;
                        //}
                        //catch { return null; }
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
                Console.Write("\n");
                Console.Write("Entrada de peticion " + uri + "\n");
                Console.Write(info);
                Console.Write("\n");

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
                    // ninguna de las dos publicaciones esnull!!! 
                    int count_1 = 0;
                    int count_2 = 0;
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
                            //pub.seqOfAuthors = unificarUsuariosConNombreSimple(pub_1.seqOfAuthors, pub_2.seqOfAuthors);
                            //todo comporbacion de que son o no son iguales.
                            //pub.seqOfAuthors.AddRange(pub_2.seqOfAuthors);
                            pub.seqOfAuthors= pub_1.seqOfAuthors;
                            pub.seqOfAuthors.AddRange(pub_2.seqOfAuthors);
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

                    if (pub_1.hasPublicationVenue != null)
                    {
                        pub.hasPublicationVenue = pub_1.hasPublicationVenue;
                    }
                    else
                    {
                        pub.hasPublicationVenue = pub_2.hasPublicationVenue;
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
                    if (bo)
                    {
                        pub.pdf = llamada_Zenodo(pub.doi);

                        pub.topics_enriquecidos = enriquedicmiento(pub);
                        pub.freetextKeyword_enriquecidas = enriquedicmiento_pal(pub);
                    }

                    return pub;

                }
            }


        }

        public Publication llamada_Semantic_Scholar(string doi)
        {
            Uri url = new Uri(string.Format("http://localhost:5004/SemanticScholar/GetROs?doi={0}", doi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Publication objInicial_SemanticScholar = JsonConvert.DeserializeObject<Publication>(info_publication);
            return objInicial_SemanticScholar;
        }

        public Publication llamada_open_citations(string doi)
        {
            Uri url = new Uri(string.Format("http://localhost:5003/OpenCitations/GetROs?doi={0}", doi));
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
            Uri url = new Uri(string.Format("http://localhost:5002/CrossRef/GetROs?doi={0}", doi));
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
            Uri url = new Uri(string.Format("http://localhost:5001/Scopus/GetROs?orcid={0}&date={1}", orcid, date));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            List<Publication> objInicial_Scopus = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            return objInicial_Scopus;
        }

        public List<Publication> llamada_WoS(string orcid, string date)
        {
            Uri url = new Uri(string.Format("http://localhost:5000/WoS/GetROs?orcid={0}&date={1}", orcid, date));
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
                    Uri url = new Uri(string.Format("http://localhost:5005/Zenodo/GetROs?ID={0}", name));
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
        public string formato_estanda_string_name(string namee)
        {
            //if (namee.Contains("-") || namee.Contains(",")) { return null; }
            //else
            //{
            //string[] i = name.Split(",");
            //List<string> devolver = new List<string>();
            //string nam;
            //foreach(string namee in i){
            //List<string> caracteres= new List<string>(){"á", "é", "ñ", "í","ú","'","ó","-"};

            namee = namee.Replace("á", "a");
            namee = namee.Replace("ó", "o");
            namee = namee.Replace("-", " ");
            namee = namee.Replace("ú", "u");
            namee = namee.Replace("í", "i");
            namee = namee.Replace("ñ", "n");
            namee = namee.Replace("ó", "o");
            namee = namee.Replace("é", "e");
            //ä
            namee = namee.Replace("´", "");
            namee = namee.Replace("'", "");
            namee = namee.Replace("~", "");
            //todo, si funciona igual hay que considerar mas plaabras! 
            //devolver.Add(nam);
            // }
            return namee;
            //  }

        }

        
        public List<string> nombre_completo(string name, string apellido)
        {

            //conseguimos todos los fromas posibles de un nombre simple
            //ejemplo -> Pepe -> Pepe P. P son las formas posibles. 
            string nombre_normalizado = formato_estanda_string_name(name);
            List<string> given = new List<string>() { nombre_normalizado, nombre_normalizado.Substring(0, 1), nombre_normalizado.Substring(0, 1) + "." };
            Console.Write(given[1]);
            apellido = formato_estanda_string_name(apellido);

            List<string> nomb_completos = new List<string>();
            // given apellido
            foreach (string given_forma in given)
            {
                //foreach (string apellido_forma in apellido)
                //{

                //    Console.Write(".....................------------------------");
                nomb_completos.Add(given_forma + " " + apellido);
                nomb_completos.Add(apellido + " " + given_forma);
                nomb_completos.Add(apellido + ", " + given_forma);
                //}
            }
            Console.Write(".....................------------------------");

            return nomb_completos;
        }

        public Person unificar_dos_personas(Person persona_1, Person persona_2)
        {
            if (persona_1.ORCID == null)
            {
                if (persona_2.ORCID != null)
                {
                    persona_1.ORCID = persona_2.ORCID;
                }
            }
            if (persona_2.IDs != null)
            {
                if (persona_1.IDs != null)
                {
                    persona_1.IDs.AddRange(persona_2.IDs);
                }
                else
                {// persona_1.IDs=null
                    persona_1.IDs = persona_2.IDs;
                }
            }

            if (persona_2.links != null)
            {
                if (persona_1.links != null)
                {
                    persona_1.links.AddRange(persona_2.links);
                }
                else
                {
                    persona_1.links = persona_2.links;
                }
            }
            
        return persona_1;

        }

        public List<Person> unificarUsuariosConNombreSimple(List<Person> listado_1, List<Person> listado_2)
        {
            List<Person> devolver = new List<Person>();
            foreach (Person persona_1 in listado_1)
            {
                //aqui faltaria algun if
                if (persona_1.name.given != null && persona_1.name.given.Count > 0 && persona_1.name.familia != null && persona_1.name.familia.Count > 0)
                {
                    // el given y familia si tienen algo solo tienen 1... 
                    if (!persona_1.name.familia[0].Contains("-") && !persona_1.name.given[0].Contains(" ") && !persona_1.name.familia[0].Contains(" "))
                    {
                        //los nombres son bonitos 
                        string name = persona_1.name.given[0];
                        string apellido = persona_1.name.familia[0];
                        List<string> nombre_completo_1 = new List<string>();
                        nombre_completo_1 = nombre_completo(name, apellido);
                        persona_1.name.nombre_completo = nombre_completo_1;

                        Person persona_eliminar = new Person();
                        if (listado_2.Count >= 1)
                        {
                            for (int i = 0; i < listado_2.Count; i++)
                            {
                                Person person_2 = listado_2[i];

                                if (person_2.name.nombre_completo != null)
                                {
                                    foreach (string name_completo_persona_2 in person_2.name.nombre_completo)
                                    {
                                        if (nombre_completo_1.Contains(formato_estanda_string_name(name_completo_persona_2)))
                                        {
                                            //detertada una persona duplicada por lo que toda la infromacion la pasamos a una unica persona. -> combinacion de datos! 
                                            //llegados aqui deberiamos aber que uno de los orcis es null. 
                                            if (persona_1.ORCID == null)
                                            {
                                                if (person_2.ORCID != null)
                                                {
                                                    persona_1.ORCID = person_2.ORCID;
                                                }
                                            }
                                            if (person_2.IDs != null)
                                            {
                                                if (persona_1.IDs != null)
                                                {
                                                    persona_1.IDs.AddRange(person_2.IDs);
                                                }
                                                else
                                                {
                                                    persona_1.IDs = person_2.IDs;
                                                }
                                            }

                                            if (person_2.links != null)
                                            {
                                                if (persona_1.links != null)
                                                {
                                                    persona_1.links.AddRange(person_2.links);
                                                }
                                                else
                                                {
                                                    persona_1.links = person_2.links;
                                                }
                                            }
                                            persona_eliminar = person_2;
                                            //  listado_2.Remove(person_2);
                                            // }
                                        }

                                    }
                                }
                            }
                            listado_2.Remove(persona_eliminar);
                        }
                    }
                    else { devolver.Add(persona_1); }
                }
                else { devolver.Add(persona_1); }
            }
            return listado_1;
        }
    }

}
