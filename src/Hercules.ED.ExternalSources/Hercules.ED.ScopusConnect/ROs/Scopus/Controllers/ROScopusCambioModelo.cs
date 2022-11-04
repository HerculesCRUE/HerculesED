using System.Collections.Generic;
using ScopusConnect.ROs.Scopus.Models;
using ScopusConnect.ROs.Scopus.Models.Inicial;

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

        public List<Publication> GetListPublications(Root objInicial)
        {
            List<Publication> listaResultados = new();
            if (objInicial != null && objInicial.SearchResults != null && objInicial.SearchResults.entry != null)
            {
                foreach (PublicacionInicial rec in objInicial.SearchResults.entry)
                {
                    Publication publicacion = CambioDeModeloPublicacion(rec);
                    if (publicacion != null)
                    {
                        listaResultados.Add(publicacion);
                    }
                }
            }

            return listaResultados;
        }


        public Publication CambioDeModeloPublicacion(PublicacionInicial objInicial)
        {
            Publication publicacion = new();

            if (objInicial != null)
            {
                publicacion.typeOfPublication = GetType(objInicial);

                if (publicacion.typeOfPublication != null)
                {
                    publicacion.scopusID = GetIDs(objInicial);
                    publicacion.title = GetTitle(objInicial);
                    publicacion.doi = GetDoi(objInicial);
                    publicacion.url = GetLinks(objInicial);
                    publicacion.datimeTime = GetDate(objInicial);
                    publicacion.pageStart = GetPageStart(objInicial);
                    publicacion.pageEnd = GetPageEnd(objInicial);
                    publicacion.volume = GetVolume(objInicial);
                    publicacion.articleNumber = GetArticleNumber(objInicial);
                    publicacion.openAccess = GetOpenAccess(objInicial);
                    publicacion.correspondingAuthor = GetAuthorPrincipal(objInicial);
                    publicacion.hasPublicationVenue = GetJournal(objInicial);
                    publicacion.hasMetric = GetPublicationMetric(objInicial);
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

        public string GetType(PublicacionInicial objInicial)
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

        public string GetIDs(PublicacionInicial objInicial)
        {
            string id = null;
            if (!string.IsNullOrEmpty(objInicial.DcIdentifier))
            {
                id = objInicial.DcIdentifier;
            }
            return id;
        }

        public string GetTitle(PublicacionInicial objInicial)
        {
            if (objInicial.DcTitle != null)
            {
                return objInicial.DcTitle;
            }
            return null;
        }

        public string GetDoi(PublicacionInicial objInicial)
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
        public HashSet<string> GetLinks(PublicacionInicial objInicial)
        {
            HashSet<string> links = new();
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

        public string GetDate(PublicacionInicial objInicial)
        {
            string date = null;

            if (!string.IsNullOrEmpty(objInicial.PrismCoverDate))
            {
                date = objInicial.PrismCoverDate;
            }

            return date;
        }

        public string GetPageStart(PublicacionInicial objInicial)
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

        public string GetPageEnd(PublicacionInicial objInicial)
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

        public string GetVolume(PublicacionInicial objInicial)
        {
            if (objInicial.PrismVolume != null)
            {
                return objInicial.PrismVolume;
            }
            return null;
        }

        public string GetArticleNumber(PublicacionInicial objInicial)
        {
            if (objInicial.ArticleNumber != null)
            {
                return objInicial.ArticleNumber;
            }
            return null;
        }

        public bool? GetOpenAccess(PublicacionInicial objInicial)
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

        public Person GetAuthorPrincipal(PublicacionInicial objInicial)
        {
            if (!string.IsNullOrEmpty(objInicial.DcCreator))
            {
                Person autor = new();
                autor.nick = objInicial.DcCreator;
                autor.fuente = "Scopus";
                return autor;
            }

            return null;
        }
        public Source GetJournal(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPublicationName != null || objInicial.PrismIssn != null)
            {
                Source revista = new();

                // Nombre publicación
                if (!string.IsNullOrEmpty(objInicial.PrismPublicationName))
                {
                    revista.name = objInicial.PrismPublicationName;
                }

                // ISSN
                if (!string.IsNullOrEmpty(objInicial.PrismIssn))
                {
                    string issnFormado = $@"{objInicial.PrismIssn.Substring(0, 4)}-{objInicial.PrismIssn.Substring(4)}";

                    List<string> issn = new();
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

        public List<PublicationMetric> GetPublicationMetric(PublicacionInicial objInicial)
        {
            List<PublicationMetric> metricList = new();
            PublicationMetric metricPublicacion = new();
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