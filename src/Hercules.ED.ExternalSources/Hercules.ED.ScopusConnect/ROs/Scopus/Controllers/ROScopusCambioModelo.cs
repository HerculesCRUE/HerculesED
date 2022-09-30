using System.Collections.Generic;
using ScopusConnect.ROs.Scopus.Models;
using ScopusConnect.ROs.Scopus.Models.Inicial;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

using Newtonsoft.Json;


namespace ScopusConnect.ROs.Scopus.Controllers
{
    public class ROScopusControllerJSON
    {
        public List<string> advertencia = null;
        public ROScopusLogic ScopusLogic;
        public ROScopusControllerJSON(ROScopusLogic ScopusLogic)
        {
            this.ScopusLogic = ScopusLogic;

        }

        public List<Publication> getListPublications(Root objInicial, string date)
        {
            List<Publication> listaResultados = new List<Publication>();
            if (objInicial != null && objInicial.SearchResults != null && objInicial.SearchResults.entry != null)
            {
                foreach (PublicacionInicial rec in objInicial.SearchResults.entry)
                {
                    Publication publicacion = cambioDeModeloPublicacion(rec, true);
                    if (publicacion != null)
                    {
                        listaResultados.Add(publicacion);
                    }
                }
            }

            return listaResultados;
        }



        public Publication cambioDeModeloPublicacion(PublicacionInicial objInicial, bool publicacion_principal)
        {
            Publication publicacion = new Publication();

            if (objInicial != null)
            {
                publicacion.typeOfPublication = getType(objInicial);

                if (publicacion.typeOfPublication != null)
                {
                    publicacion.scopusID = getIDs(objInicial);
                    publicacion.title = getTitle(objInicial);
                    publicacion.doi = getDoi(objInicial);
                    publicacion.url = getLinks(objInicial);
                    publicacion.datimeTime = getDate(objInicial);
                    publicacion.pageStart = getPageStart(objInicial);
                    publicacion.pageEnd = getPageEnd(objInicial);
                    publicacion.volume = getVolume(objInicial);
                    publicacion.articleNumber = getArticleNumber(objInicial);
                    publicacion.openAccess = getOpenAccess(objInicial);
                    publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
                    publicacion.hasPublicationVenue = getJournal(objInicial);
                    publicacion.hasMetric = getPublicationMetric(objInicial);
                    publicacion.dataOrigin = "Scopus";
                    return publicacion;
                }
                else
                {
                    return null;
                }
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
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }



        }
        public string getIDs(PublicacionInicial objInicial)
        {
            string id = null;
            if (!string.IsNullOrEmpty(objInicial.DcIdentifier))
            {
                id = objInicial.DcIdentifier;
            }
            return id;
        }

        public string getTitle(PublicacionInicial objInicial)
        {
            if (objInicial.DcTitle != null)
            {
                return objInicial.DcTitle;
            }
            return null;
        }

        public string getDoi(PublicacionInicial objInicial)
        {
            if (objInicial.PrismDoi != null)
            {
                if (objInicial.PrismDoi.Contains("https://doi.org/"))
                {
                    int indice = objInicial.PrismDoi.IndexOf("org/");
                    return objInicial.PrismDoi.Substring(indice + 4);
                }
                else
                {
                    return objInicial.PrismDoi;
                }
            }
            return null;
        }
        public HashSet<string> getLinks(PublicacionInicial objInicial)
        {
            HashSet<string> links = new HashSet<string>();
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

        public string getDate(PublicacionInicial objInicial)
        {
            string date = null;

            if (!string.IsNullOrEmpty(objInicial.PrismCoverDate))
            {
                date = objInicial.PrismCoverDate;
            }

            return date;
        }

        public string getPageStart(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPageRange != null)
            {
                if (objInicial.PrismPageRange.Contains("-"))
                {
                    string[] paginas = objInicial.PrismPageRange.Split("-");
                    return paginas[0];
                }
            }
            return null;
        }

        public string getPageEnd(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPageRange != null)
            {
                if (objInicial.PrismPageRange.Contains("-"))
                {
                    string[] paginas = objInicial.PrismPageRange.Split("-");
                    return paginas[1];
                }
            }
            return null;
        }

        public string getVolume(PublicacionInicial objInicial)
        {
            if (objInicial.PrismVolume != null)
            {
                return objInicial.PrismVolume;
            }
            return null;
        }

        public string getArticleNumber(PublicacionInicial objInicial)
        {
            if (objInicial.ArticleNumber != null)
            {
                return objInicial.ArticleNumber;
            }
            return null;
        }

        public bool? getOpenAccess(PublicacionInicial objInicial)
        {
            if (objInicial.openaccess != null)
            {
                switch (objInicial.openaccess)
                {
                    case "1":
                        return true;
                    case "0":
                        return false;
                }
            }
            return null;
        }

        public Person getAuthorPrincipal(PublicacionInicial objInicial)
        {
            if (!string.IsNullOrEmpty(objInicial.DcCreator))
            {
                Person autor = new Person();
                autor.nick = objInicial.DcCreator;
                autor.fuente = "Scopus";
                return autor;
            }

            return null;
        }
        public Source getJournal(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPublicationName != null || objInicial.PrismIssn != null)
            {
                Source revista = new Source();

                // Nombre publicación
                if (!string.IsNullOrEmpty(objInicial.PrismPublicationName))
                {
                    revista.name = objInicial.PrismPublicationName;
                }

                // ISSN
                if (!string.IsNullOrEmpty(objInicial.PrismIssn))
                {
                    string issnFormado = $@"{objInicial.PrismIssn.Substring(0, 4)}-{objInicial.PrismIssn.Substring(4)}";

                    List<string> issn = new List<string>();
                    issn.Add(issnFormado);
                    revista.issn = issn;
                }

                // EISSN
                if (!string.IsNullOrEmpty(objInicial.PrismEIssn))
                {
                    string eissnFormado = $@"{objInicial.PrismEIssn.Substring(0, 4)}-{objInicial.PrismEIssn.Substring(4)}";
                    revista.eissn = eissnFormado;
                }

                // Tipo de la revista
                if (!string.IsNullOrEmpty(objInicial.PrismAggregationType))
                {
                    switch (objInicial.PrismAggregationType)
                    {
                        case "Book":
                            revista.type = "Book";
                            break;
                        case "Journal":
                            revista.type = "Journal";
                            break;
                    }
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
    }
}