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
        protected string bareer;
        //ROScopusControllerJSON info = new ROScopusControllerJSON();
        protected string baseUri { get; set; }


        // protected List<Publication> publications = new List<Publication>();
        protected List<string> dois_principales = new List<string>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROPublicationLogic(string baseUri)
        {

            this.baseUri = "http://localhost:5000/WoS/GetROs?orcid={0}";

            //this.bareer = bareer;

        }

        // TODO: Esto no se si abra que cambiarlo o no.... 
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
        /// <returns></returns>
        public List<Publication> getPublications(string name)
        {
            //Declaro el Resultado
            List<Publication> resultado = new List<Publication>();

            //consulta a WoS 
            List<Publication> objInicial_woS = llamada_WoS(name);

            foreach (Publication pub in objInicial_woS)
            {
                string doi = pub.doi;
                dois_principales.Add(doi);
                Publication pub_completa = Obtener_publicacion_dependiendo_grafo(pub,true);
                if(pub_completa!=null){
                resultado.Add(pub_completa);}
            }
            //llamada Scopus para completar publicaciones. 
            List<Publication> objInicial_Scopus = llamada_Scopus(name);
            foreach (Publication pub_scopus in objInicial_Scopus)
            {
                if (!dois_principales.Contains(pub_scopus.doi))
                {
                    dois_principales.Add(pub_scopus.doi);
                    Publication pub_completa= Obtener_publicacion_dependiendo_grafo(pub_scopus,true);
                    if(pub_completa!=null){
                        resultado.Add(pub_completa);
                    }
                }
            }
            return resultado;

        }

        //en esta funcion lo que hacemos es dependiedo de si esta o no y de como este en el graof de concomiento 
        //completar los datos que tenemos de la publicacion. 
        //el booleano de principal es true si la publicacion que queremos meter en el grafo es principal 
        public Publication Obtener_publicacion_dependiendo_grafo(Publication pub, Boolean principal, string fuente = "Open Citations")
        {
            if (false)
            {
                //Esta en el grafo! 
                //todo: esta cjomporbacion de si esta en el grafo 
                if (false)
                {
                    //esta como tipo publicacion principal -> no hacemos nada! 
                    return null;
                    //todo en realidad no queremos devolver null solo que no noshaxe falta conectar mas porque ya lo tenemos 
                    //todo esta comporbacion de si esta ene lgrafo como publicacion principal! 
                }
                else
                {
                    // esto es si esta como tipo publicacion de bibliografia o cita 
                    //este como bibliografia y nosotros queremos como principal:
                    if (principal)
                    {
                        Publication pub_completa = obtener_bib_citas(pub);
                        return pub_completa;
                    }
                    else
                    {
                        //todo en realidad no queremos devolver null solo que no noshaxe falta conectar mas porque ya lo tenemos 
                        //este caso esta como bibliografia y queremos meter como bibliograi
                        return null; 
                    }

                }
            }
            else
            {
                if(principal){
                Publication pub_completa = obtener_bib_citas(pub);
                return pub_completa;
                }else{
                    //si no es principal (no ha sido obtenida por WoS )y no esta en el grafo debemos llamar a funciones para completar esta funcion 
                    if(fuente=="Open Citations"){
                    Publication objInicial_SemanticScholar = llamada_Semantic_Scholar(pub.doi);
                    return objInicial_SemanticScholar;
                    }
                    if(fuente=="CrossRef"){
                        //TODO
                        return pub;
                    }
                    if(fuente=="SemanticScholar"){
                        return pub;
                        //todo comprobar esto! 
                    }
                    //en caso de que pase algo raro devolbemos lo que tenemos
                    return pub;
                   
                }
            }
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
                    
                    //todo: comprobar si ya es     ta en el grafo o no. Este es el caso en el que no esta en el grafo! Directamente al 3.2
                    //llamada Semantic Scholar 
                    Publication objInicial_SemanticScholar = llamada_Semantic_Scholar(doi_cita);
                    Publication pub_completa = Obtener_publicacion_dependiendo_grafo(objInicial_SemanticScholar,false,"SemanticScholar");
                    if (pub_completa != null)
                    {
                        citas.Add(objInicial_SemanticScholar);
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

                    //todo: comprobar si ya esta en el grafo o no. Este es el caso en el que no esta en el grafo! Directamente al 3.2

                    //llamada Semantic Scholar 
                    Publication objInicial_SemanticScholar = llamada_Semantic_Scholar(doi_bib);
                    Publication pub_completa = Obtener_publicacion_dependiendo_grafo(objInicial_SemanticScholar,false,"SemanticScholar");
                     if (pub_completa != null){
                        bibliografia.Add(objInicial_SemanticScholar);
                    }

                }

            }
            //obteneemos toda la bibliografia de CrossREF
            Publication objInicial_CrossRef = llamada_CrossRef(doi);
            if(objInicial_CrossRef!=null){
            List<Publication> bib_CrossRef = obtener_bib_crosRef(objInicial_CrossRef, dois_bibliografia);
            if (bib_CrossRef != new List<Publication>())
            {
                bibliografia.AddRange(bib_CrossRef);
            }}

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
                        //todo: complementar con SemanticScholar! 
                        bib_crossREf.Add(pub_cross);

                    }
                }
                return bib_crossREf;
            }
            return bib_crossREf;
        }

        public Publication llamada_Semantic_Scholar(string doi)
        {
            //todo: ¿igual una extepcion para ver cuando devuelve null?

            Uri url = new Uri(string.Format("http://localhost:5004/SemanticScholar/GetROs?doi={0}", doi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Publication objInicial_SemanticScholar = JsonConvert.DeserializeObject<Publication>(info_publication);
            return objInicial_SemanticScholar;
        }

        public Publication llamada_open_citations(string doi)
        {
            //todo: igual es mejor que esto devuelva una lista de doi para la bibliografia y otra para las citas
            Uri url = new Uri(string.Format("http://localhost:5003/OpenCitations/GetROs?doi={0}", doi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            Publication objInicial_OpenCitatons = JsonConvert.DeserializeObject<Publication>(info_publication);
            return objInicial_OpenCitatons;
        }

        public Publication llamada_CrossRef(string doi)
        {
            //todo: igual es mejor que esto devuelva una lista de doi para la bibliografia y otra para las citas
            Uri url = new Uri(string.Format("http://localhost:5002/CrossRef/GetROs?doi={0}", doi));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            if(info_publication==null){
                return null;
            }
            Publication objInicial_CrossRef = JsonConvert.DeserializeObject<Publication>(info_publication);
            return objInicial_CrossRef;
        }
        public List<Publication> llamada_Scopus(string orcid)
        {
            Uri url = new Uri(string.Format("http://localhost:5001/Scopus/GetROs?orcid={0}", orcid));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            List<Publication> objInicial_Scopus = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            return objInicial_Scopus;
        }
        public List<Publication> llamada_WoS(string orcid)
        {
            Uri url = new Uri(string.Format("http://localhost:5000/WoS/GetROs?orcid={0}", orcid));
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;
            List<Publication> objInicial_woS = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
            return objInicial_woS;
        }


    }


}
