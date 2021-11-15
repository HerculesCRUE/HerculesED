using System.Collections.Generic;
using OpenCitationsConnect.ROs.OpenCitations.Models;
using OpenCitationsConnect.ROs.OpenCitations.Models.Inicial;

using Newtonsoft.Json.Linq;
using System;
using System.Threading;

using Newtonsoft.Json;


namespace OpenCitationsConnect.ROs.OpenCitations.Controllers
{
    public class ROOpenCitationsControllerJSON //: //ROScopusLogic
    {
        public ROOpenCitationsLogic WoSLogic;
        public ROOpenCitationsControllerJSON(ROOpenCitationsLogic WoSLogic)
        {
            this.WoSLogic = WoSLogic;

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



        // public Publication cambioDeModeloPublicacion(PublicacionInicial objInicial, Boolean publicacion_principal)
        // {
        //     Publication publicacion = new Publication();
        //     if (objInicial != null)
        //     {
        //         publicacion.IDs = getIDs(objInicial);
        //         publicacion.title = getTitle(objInicial);
        //         publicacion.Abstract = getAbstract(objInicial);
        //         publicacion.language = getLanguage(objInicial);
        //         publicacion.doi = getDoi(objInicial);
        //         publicacion.url = getLinks(objInicial);
        //         publicacion.dataIssued = getDate(objInicial);
        //         publicacion.pageStart = getPageStart(objInicial);
        //         publicacion.pageEnd = getPageEnd(objInicial);
        //         publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
        //         publicacion.freetextKeyword = getFreetextKeyword(objInicial);
        //         publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
        //         publicacion.seqOfAuthors = getAuthors(objInicial);
        //         publicacion.hasPublicationVenue = getJournal(objInicial);
        //         publicacion.hasMetric = getPublicationMetric(objInicial);
        //         if(publicacion_principal==true){
        //         publicacion.bibliografia = getBiblografia(objInicial);
        //         publicacion.citas = getCitas(objInicial);
        //         }
        //         return publicacion;
        //     }
        //     else
        //     {
        //         return null;
        //     }

        // }

        // public List<String> getIDs(PublicacionInicial objInicial)
        // {
        //     return null;
        // }


        // public string getTitle(PublicacionInicial objInicial)
        // {
        //     return null;
        // }

        // public string getAbstract(PublicacionInicial objInicial)
        // {
        //     return null;
        // }

        // public string getLanguage(PublicacionInicial objInicial)
        // {
        //     return null;
        // }
        // public string getDoi(PublicacionInicial objInicial)
        // {
        //     return null;
        // }
        // public List<string> getLinks(PublicacionInicial objInicial)
        // {
        //     return new List<string>();
        // }

        // public DateTimeValue getDate(PublicacionInicial objInicial)
        // {
        //     DateTimeValue date = new DateTimeValue();
        //     date.datimeTime =null;
        //     return date;
        // }        

        // public string getPageStart(PublicacionInicial objInicial)
        // {
        //     return null;
        // }

        // public string getPageEnd(PublicacionInicial objInicial)
        // {
        //     return null;
        // }

        // public List<KnowledgeArea> getKnowledgeAreas(PublicacionInicial objInicial)
        // {
        //     List<KnowledgeArea> result = new List<KnowledgeArea>();
        //     KnowledgeArea area = null; 
        //     result.Add(area);
        //     return result;
        // }

        // public List<string> getFreetextKeyword(PublicacionInicial objInicial)
        // {
        //     return new List<string>();
        // }

        // public Person getAuthorPrincipal(PublicacionInicial objInicial)
        // {
        //     return new Person();
        // }
        // public List<Person> getAuthors(PublicacionInicial objInicial)
        // {
        //     return new List<Person>();
        // }

        // public Journal getJournal(PublicacionInicial objInicial)
        // {
        //     return new Journal();;
        // }



        // public PublicationMetric getPublicationMetric(PublicacionInicial objInicial)
        // {
        //     PublicationMetric metricPublicacion = new PublicationMetric();
        //     return metricPublicacion;
        // }

        public List<Publication> getBiblografia(Root objInicial)
        {
            List<Publication> sol = new List<Publication>();
            if (objInicial.data != null)
            {
                foreach (PublicationInicial referencia in objInicial.data)
                {
                    if (referencia.cited != null)
                    {
                        Publication pub = new Publication();
                        string[] datos = referencia.cited.Split("> ");
                        pub.doi = datos[1];
                        sol.Add(pub);
                    }
                }
                if (sol.Count==0)
                {
                    return null;
                }
                else { return sol; }
            }
            return null;

        }

        public List<Publication> getCitas(Root objInicial)
        {
            List<Publication> sol = new List<Publication>();
            if (objInicial.data != null)
            {
                foreach (PublicationInicial referencia in objInicial.data)
                {
                    if (referencia.cited != null)
                    {
                        Publication pub = new Publication();

                        string[] datos = referencia.cited.Split("> ");
                        pub.doi = datos[1];
                        sol.Add(pub);
                    }
                }
                if (sol.Count==0)
                {
                    return null;
                }
                else { return sol; }
            }
            return null;
        }




    }
}