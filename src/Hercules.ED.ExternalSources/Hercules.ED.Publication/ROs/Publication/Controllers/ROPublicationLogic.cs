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
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
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
            if (objInicial_Scopus.Count >= 1)
            {
                foreach (Publication pub in objInicial_woS)
                {
                    this.advertencia = pub.problema;
                    string doi = pub.doi;
                    dois_principales.Add(doi);
                    Publication pub_completa = Obtener_publicacion_dependiendo_grafo(pub, true);
                    if (pub_completa != null)
                    {
                        if (objInicial_Scopus.Count >= 1)
                        {

                            foreach (Publication pub_scopus in objInicial_Scopus)
                            {
                                if (pub_scopus.doi != null)
                                {
                                    if (pub_scopus.doi == pub_completa.doi)
                                    {
                                        Console.Write(pub_scopus);
                                        Console.Write("\n");
                                        //todo combinar los erroees! ni puta idea de como hacerlo proqe depende de lo que juntes y lo que no! 
                                        if (pub_completa.IDs != null & pub_scopus.IDs != null)
                                        {
                                            pub_completa.IDs.AddRange(pub_scopus.IDs);
                                        }
                                        else if (pub_completa.IDs == null & pub_scopus.IDs != null)
                                        {
                                            pub_completa.IDs = pub_scopus.IDs;
                                        }
                                        if (pub_completa.hasMetric != null & pub_scopus.hasMetric != null)
                                        {
                                            pub_completa.hasMetric.AddRange(pub_scopus.hasMetric);
                                        }
                                        else if (pub_completa.hasMetric == null & pub_scopus.hasMetric != null)
                                        {
                                            pub_completa.hasMetric = pub_scopus.hasMetric;
                                        }
                                    }
                                }
                            }
                        }
                        resultado.Add(pub_completa);
                    }
                    this.advertencia = null;
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
                        dois_principales.Add(pub_scopus.doi);
                        Publication pub_completa = Obtener_publicacion_dependiendo_grafo(pub_scopus, true);
                        if (pub_completa != null)
                        {
                            resultado.Add(pub_completa);
                        }
                    }
                    this.advertencia = null;
                }

            }
            return resultado;

        }

        //en esta funcion lo que hacemos es dependiedo de si esta o no y de como este en el graof de concomiento 
        //completar los datos que tenemos de la publicacion. 
        //el booleano de principal es true si la publicacion que queremos meter en el grafo es principal 
        public Publication Obtener_publicacion_dependiendo_grafo(Publication pub, Boolean principal, string fuente = "Open Citations")
        {

            if (principal)
            {
                Publication objInicial_semanticScholar = llamada_Semantic_Scholar(pub.doi);
                if (pub.hasMetric != null)
                {
                    if (objInicial_semanticScholar.hasMetric != null)
                    {
                        pub.hasMetric.AddRange(objInicial_semanticScholar.hasMetric);
                    }
                }
                else { pub.hasMetric = objInicial_semanticScholar.hasMetric; }
                Publication pub_completa = obtener_bib_citas(pub);
                return pub_completa;
            }
            else
            {
                //si no es principal (no ha sido obtenida por WoS )y no esta en el grafo debemos llamar a funciones para completar esta funcion 
                if (fuente == "Open Citations")
                {

                    Publication pub_2 = this.llamada_CrossRef(pub.doi);
                    Publication pub_1 = this.llamada_Semantic_Scholar(pub.doi);
                    Publication pub_final_bib = compatacion(pub_1, pub_2);
                    if (pub_final_bib != null)
                    {
                        return (pub_final_bib);
                    }

                }
                if (fuente == "CrossRef")
                {
                    //todo es que aqui nunca entra porque no esta esto implementado todavia. 
                    return pub;
                }
                if (fuente == "SemanticScholar")
                {
                    Publication pub_2 = this.llamada_CrossRef(pub.doi);
                    Publication pub_final_bib = compatacion(pub, pub_2);
                    if (pub_final_bib != null)
                    {
                        return (pub_final_bib);
                    }

                }
                //en caso de que pase algo raro devolbemos lo que tenemos
                return pub;
            }
            // }
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

                    //llamada Semantic Scholar 
                    Publication objInicial_SemanticScholar = llamada_Semantic_Scholar(doi_cita);
                    Publication pub_completa = Obtener_publicacion_dependiendo_grafo(objInicial_SemanticScholar, false, "SemanticScholar");
                    if (pub_completa != null)
                    {
                        citas.Add(pub_completa);
                    }
                }
            }
            if (citas != new List<Publication>())
            {
                pub.citas = citas;
            }
            List<Publication> bibliografia = new List<Publication>();
            List<string> dois_bibliografia = new List<string>();
            if (objInicial_OpenCitatons.bibliografia != null)
            {
                foreach (Publication pub_bib in objInicial_OpenCitatons.bibliografia)
                {
                    string doi_bib = pub_bib.doi;
                    dois_bibliografia.Add(doi_bib);

                    //llamada Semantic Scholar 
                    Publication objInicial_SemanticScholar = llamada_Semantic_Scholar(doi_bib);
                    Publication pub_completa = Obtener_publicacion_dependiendo_grafo(objInicial_SemanticScholar, false, "SemanticScholar");
                    if (pub_completa != null)
                    {
                        bibliografia.Add(pub_completa);
                    }
                }
            }
            //obteneemos toda la bibliografia de CrossREF
            Publication objInicial_CrossRef = llamada_CrossRef(doi);

            if (objInicial_CrossRef != null)
            {
                if (pub.hasMetric != null)
                {
                    if (objInicial_CrossRef.hasMetric != null)
                    {
                        pub.hasMetric.AddRange(objInicial_CrossRef.hasMetric);
                    }
                }
                else { pub.hasMetric = objInicial_CrossRef.hasMetric; }
                List<Publication> bib_CrossRef = obtener_bib_crosRef(objInicial_CrossRef, dois_bibliografia);

                if (bib_CrossRef != new List<Publication>())
                {
                    bibliografia.AddRange(bib_CrossRef);
                }
            }

            //En caos de que halla elementos en la bibliografia pues adjuntamos la bibliografia!
            if (bibliografia != new List<Publication>())
            {
                pub.bibliografia = bibliografia;

            }
            return pub;
        }

        public List<Publication> obtener_bib_crosRef(Publication objInicial_CrossRef, List<string> dois_bibliografia)
        {

            List<Publication> bib_crossREf = new List<Publication>();
            if (objInicial_CrossRef.bibliografia != null)
            {
                foreach (Publication pub_cross in objInicial_CrossRef.bibliografia)
                {
                    if (!dois_bibliografia.Contains(pub_cross.doi))
                    {
                        string doi_bib = pub_cross.doi;
                        dois_bibliografia.Add(doi_bib);
                        Publication pub_semntic_scholar = this.llamada_Semantic_Scholar(doi_bib);
                        Publication pub_crossRef = this.llamada_CrossRef(doi_bib);
                        Publication pub_final_bib = compatacion(pub_crossRef, pub_semntic_scholar);
                        if (pub_final_bib != null)
                        {
                            bib_crossREf.Add(pub_final_bib);
                        }
                    }
                }
                return bib_crossREf;
            }
            return bib_crossREf;
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
                    return null;
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

                    if (pub_1.list_freetextKeyword != null)
                    {
                        pub.list_freetextKeyword = pub_1.list_freetextKeyword;
                        if (pub_2.list_freetextKeyword != null)
                        {
                            pub.list_freetextKeyword.AddRange(pub_2.list_freetextKeyword);
                        }
                    }
                    else
                    {
                        pub.list_freetextKeyword = pub_2.list_freetextKeyword;
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
                            //todo comporbacion de que son o no son iguales.
                            pub.seqOfAuthors.AddRange(pub_2.seqOfAuthors);
                        }
                    }
                    else
                    {
                        pub.seqOfAuthors = pub_2.seqOfAuthors;
                    }
                    if (pub_1.hasKnowledgeArea != null)
                    {
                        pub.hasKnowledgeArea = pub_1.hasKnowledgeArea;
                        if (pub_2.hasKnowledgeArea != null)
                        {
                            pub.hasKnowledgeArea.AddRange(pub_2.hasKnowledgeArea);
                        }
                    }
                    else
                    {
                        pub.hasKnowledgeArea = pub_2.hasKnowledgeArea;
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
                    else
                    {
                        pub.hasMetric = pub_2.hasMetric;
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


    }


}
