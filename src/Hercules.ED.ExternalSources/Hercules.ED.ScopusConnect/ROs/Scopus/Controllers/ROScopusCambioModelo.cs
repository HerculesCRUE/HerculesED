using System.Collections.Generic;
using ScopusConnect.ROs.Scopus.Models;
using ScopusConnect.ROs.Scopus.Models.Inicial;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

using Newtonsoft.Json;


namespace ScopusConnect.ROs.Scopus.Controllers
{
    public class ROScopusControllerJSON //: //ROScopusLogic
    {
        public ROScopusLogic ScopusLogic;
        public ROScopusControllerJSON(ROScopusLogic WoSLogic)
        {
            this.ScopusLogic = ScopusLogic;

        }

        public List<Publication> getListPublicatio(Root objInicial, string date)
        {
            List<Publication> sol = new List<Publication>();
            if (objInicial != null)
            {
                if (objInicial.SearchResults != null)
                {
                    if (objInicial.SearchResults.entry != null)
                    {
            
                        foreach (PublicacionInicial rec in objInicial.SearchResults.entry)
                        {
                            if (DateTime.Parse(rec.PrismCoverDate) > DateTime.Parse(date))
                            {
                                
                                Publication publicacion = cambioDeModeloPublicacion(rec, true);
                                if(publicacion!=null){
                                sol.Add(publicacion);
                                }
                                
                            }
                        }
                    }
                }
            }

            return sol;
        }



        public Publication cambioDeModeloPublicacion(PublicacionInicial objInicial, Boolean publicacion_principal)
        {
            Publication publicacion = new Publication();

            if (objInicial != null)
            {
                publicacion.typeOfPublication = getType(objInicial);
                if (publicacion.typeOfPublication != null)
                {
                    publicacion.IDs = getIDs(objInicial);
                    publicacion.title = getTitle(objInicial);
                    //publicacion.Abstract = getAbstract(objInicial);
                    //publicacion.language = getLanguage(objInicial);
                    publicacion.doi = getDoi(objInicial);
                    publicacion.url = getLinks(objInicial);
                    publicacion.dataIssued = getDate(objInicial);
                    publicacion.pageStart = getPageStart(objInicial);
                    publicacion.pageEnd = getPageEnd(objInicial);
                    ///publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
                    //publicacion.freetextKeyword = getFreetextKeyword(objInicial);
                    publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
                    //publicacion.seqOfAuthors = getAuthors(objInicial);

                    publicacion.hasPublicationVenue = getJournal(objInicial);
                    publicacion.hasMetric = getPublicationMetric(objInicial);
                    return publicacion;
                }
                else { return null; }
            }
            else
            {

                return null;
            }

        }


        public string getType(PublicacionInicial objInicial)
        {

            if (objInicial.subtypeDescription != null)
            {
                string type = objInicial.subtypeDescription;
                if (type == "Article")
                {
                    return "Journal Article";
                }
                else if (type == "Book")
                {
                    return "Book";
                }
                else if (type == "Book Chapter")
                {
                    return "Chapter";
                }
                else if (type == "Conference Paper")
                {
                    return "Conference Paper";
                }
                else { return null; }
            }
            else { return null; }



        }
        public List<string> getIDs(PublicacionInicial objInicial)
        {
            List<string> ids = new List<string>();
            if (objInicial.DcIdentifier != null)
            {
                ids.Add(objInicial.DcIdentifier);
            }
            return ids;
        }

        public string getTitle(PublicacionInicial objInicial)
        {
            if (objInicial.DcTitle != null)
            {
                return objInicial.DcTitle;
            }
            return null;
        }

        // public string getAbstract(PublicacionInicial objInicial)
        // {
        //     return null;
        // }

        // public string getLanguage(PublicacionInicial objInicial)
        // {
        //     return null;
        // }
        public string getDoi(PublicacionInicial objInicial)
        {
            if (objInicial.PrismDoi != null)
            {
                return objInicial.PrismDoi;
            }
            return null;
        }
        public List<string> getLinks(PublicacionInicial objInicial)
        {
            List<string> links = new List<string>();
            if (objInicial.link != null)
            {
                foreach (Link link in objInicial.link)
                {
                    if (link.Ref == "scopus")
                    {
                        links.Add(link.Href);
                    }
                }
            }
            return links;
        }

        public DateTimeValue getDate(PublicacionInicial objInicial)
        {
            DateTimeValue date = new DateTimeValue();

            date.datimeTime = null;
            if (objInicial.PrismCoverDate != null)
            {
                date.datimeTime = objInicial.PrismCoverDate;
            }
            return date;
        }

        public string getPageStart(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPageRange != null)
            {
                if (objInicial.PrismCoverDate.Contains("-"))
                {
                    string[] paginas = objInicial.PrismCoverDate.Split("-");
                    return paginas[0];
                }
            }
            return null;
        }

        public string getPageEnd(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPageRange != null)
            {
                if (objInicial.PrismCoverDate.Contains("-"))
                {
                    string[] paginas = objInicial.PrismCoverDate.Split("-");
                    return paginas[1];
                }
            }
            return null;
        }

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

        public Person getAuthorPrincipal(PublicacionInicial objInicial)
        {
            Person autor = new Person();
            if (objInicial.DcCreator != null)
            {
                List<string> names = new List<string>();
                names.Add(objInicial.DcCreator);
                autor.name = names;
                return autor;
            }
            return null;
        }
        // public List<Person> getAuthors(PublicacionInicial objInicial)
        // {
        //     return new List<Person>();
        // }

        public Journal getJournal(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPublicationName != null || objInicial.PrismIssn != null)
            {
                Journal revista = new Journal();
                if (objInicial.PrismPublicationName != null)
                {
                    revista.name = objInicial.PrismPublicationName;
                }
                if (objInicial.PrismIssn != null)
                {
                    revista.issn = objInicial.PrismIssn;
                }
                return revista;
            }
            return null;
        }



        public List<PublicationMetric> getPublicationMetric(PublicacionInicial objInicial)
        {
            List<PublicationMetric> metricList = new List<PublicationMetric>();
            PublicationMetric metricPublicacion = new PublicationMetric();
            if (objInicial.CitedbyCount != null)
            {
                metricPublicacion.citationCount = objInicial.CitedbyCount;
                metricPublicacion.metricName = "Scopus";
                metricList.Add(metricPublicacion);
                return metricList;
            }

            return null;
        }

        // public List<Publication> getBiblografia(PublicacionInicial objInicial)
        // {
        //     return new List<Publication>();
        // }

        // public List<Publication> getCitas(PublicacionInicial objInicial)
        // {
        //     return new List<Publication>();
        // }




    }
}