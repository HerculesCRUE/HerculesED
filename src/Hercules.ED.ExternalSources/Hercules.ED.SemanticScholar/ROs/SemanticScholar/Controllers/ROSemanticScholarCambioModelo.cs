using System.Collections.Generic;
using SemanticScholarConnect.ROs.SemanticScholar.Models;
using SemanticScholarConnect.ROs.SemanticScholar.Models.Inicial;

using Newtonsoft.Json.Linq;
using System;
using System.Threading;

using Newtonsoft.Json;


namespace SemanticScholarConnect.ROs.SemanticScholar.Controllers
{
    public class ROSemanticScholarControllerJSON //: //ROScopusLogic
    {
        public ROSemanticScholarLogic SemanticScholarLogic;
        public ROSemanticScholarControllerJSON(ROSemanticScholarLogic SemanticScholarLogic)
        {
            this.SemanticScholarLogic = SemanticScholarLogic;

        }

        // public List<Publication> getListPublicatio(Root objInicial)
        // {
        //     List<Publication> sol = new List<Publication>();
        //     foreach (PublicacionInicial rec in objInicial.Data.Records.records.publicacionInicial)
        //     {
        //         Publication publicacion = cambioDeModeloPublicacion(rec, true);
        //         sol.Add(publicacion);
        //     }
        //     return sol;
        // }


        
        public Publication cambioDeModeloPublicacion(Root objInicial, Boolean publicacion_principal)
        {
            Publication publicacion = new Publication();
            if (objInicial != null)
            {
                publicacion.IDs = getIDs(objInicial);
                publicacion.title = getTitle(objInicial);
                publicacion.Abstract = getAbstract(objInicial);
                //publicacion.language = getLanguage(objInicial);
               // publicacion.doi = getDoi(objInicial);
                publicacion.url = getLinks(objInicial);
                publicacion.dataIssued = getDate(objInicial);
                //publicacion.pageStart = getPageStart(objInicial);
                //publicacion.pageEnd = getPageEnd(objInicial);
                //publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
                //publicacion.freetextKeyword = getFreetextKeyword(objInicial);
                //publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
                publicacion.seqOfAuthors = getAuthors(objInicial);
                publicacion.hasPublicationVenue = getJournal(objInicial);
                publicacion.hasMetric = getPublicationMetric(objInicial);
               
                return publicacion;
            }
            else
            {
                return null;
            }

        }

        public List<string> getIDs(Root objInicial)
        {
            List<string> IDs = new List<string>();
            if(objInicial.paperId!=null){
                IDs.Add("SemanticScholar: "+ objInicial.paperId);
    
            }
            if(objInicial.externalIds!=null){
               if(objInicial.externalIds.ArXiv!=null){
                   IDs.Add("ArXiv: "+objInicial.externalIds.ArXiv);
               }
               if(objInicial.externalIds.MAG!=null){
                   IDs.Add("MAG: "+objInicial.externalIds.MAG);
               }
               if(objInicial.externalIds.PubMedCentral!=null){
                   IDs.Add("PubMedCentral: "+objInicial.externalIds.PubMedCentral);
               }
            }
            return IDs;
        }


        public string getTitle(Root objInicial)
        {
            if(objInicial.title!=null){
                return objInicial.title;
            }
            return null;
        }

        public string getAbstract(Root objInicial)
        {
            if(objInicial.@abstract!=null){
                return objInicial.@abstract;
            }
            return null;
        }

        // public string getLanguage(Root objInicial)
        // {
        //     return null;
        // }
        // public string getDoi(Root objInicial)
        // {
        //     return null;
        // }
        public List<string> getLinks(Root objInicial)
        {
            if(objInicial.url!=null){
                List<string> links =new List<string>();
                links.Add(objInicial.url);
                return links;
            }
            return new List<string>();
        }

        public DateTimeValue getDate(Root objInicial)
        {
            DateTimeValue date = new DateTimeValue();
            date.datimeTime =null;
            if(objInicial.year!=null){
                date.datimeTime=objInicial.year.ToString();
            //todo: esto no es del todo correcto! porque no es una fecha sino un año! 
            }
            return date;
        }        

        // public string getPageStart(Root objInicial)
        // {
        //     return null;
        // }

        // public string getPageEnd(Root objInicial)
        // {
        //     return null;
        // }

        // public List<KnowledgeArea> getKnowledgeAreas(Root objInicial)
        // {
        //     List<KnowledgeArea> result = new List<KnowledgeArea>();
        //     KnowledgeArea area = null; 
        //     result.Add(area);
        //     return result;
        // }

        // public List<string> getFreetextKeyword(Root objInicial)
        // {
        //     return new List<string>();
        // }

        // public Person getAuthorPrincipal(Root objInicial)
        // {
        //     return new Person();
        // }
        public List<Person> getAuthors(Root objInicial)
        {
            if(objInicial.authors!=null){
                List<Person> autores = new List<Person>();
                foreach(Author author in objInicial.authors){
                    Person persona = new Person();
                    if(author.name!=null){
                    List<string> nombres = new List<string>();
                    nombres.Add(author.name);
                    persona.name=nombres;
                    }
                    if(author.authorId!=null){
                        List<string> ids = new List<string>();
                    ids.Add("SemanticScholar: "+ author.authorId);
                        persona.IDs =ids;
                    }
                autores.Add(persona);
                }
                return autores;
            }
            return new List<Person>();
        }

        public Journal getJournal(Root objInicial)
        {
            if(objInicial.venue!=null && objInicial.venue!=""){
                Journal revista = new Journal();
                revista.name= objInicial.venue;
                return revista;
            }
            return new Journal();;
        }

       

        public List<PublicationMetric> getPublicationMetric(Root objInicial)
        {
            List<PublicationMetric> metriscas = new List<PublicationMetric>();
            PublicationMetric metricPublicacion = new PublicationMetric();
            if(objInicial.citationCount!=null){
                metricPublicacion.citationCount=objInicial.citationCount.ToString();
                metricPublicacion.metricName="SemanticScholar";
            }
            metriscas.Add(metricPublicacion);
            return metriscas;
        }

        // public List<Publication> getBiblografia(Root objInicial)
        // {
        //     return new List<Publication>();
        // }

        // public List<Publication> getCitas(Root objInicial)
        // {
        //     return new List<Publication>();
        // }




    }
}