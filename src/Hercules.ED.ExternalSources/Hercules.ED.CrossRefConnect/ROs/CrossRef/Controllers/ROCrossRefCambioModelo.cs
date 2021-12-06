using System.Collections.Generic;
//using CrossRefConnect.ROs.CrossRef.Models;
using CrossRefConnect.ROs.CrossRef.Models.Inicial;
using CrossRefConnect.ROs.CrossRef.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

using Newtonsoft.Json;


namespace CrossRefConnect.ROs.CrossRef.Controllers
{
    public class ROCrossRefControllerJSON //: ROScopusLogic
    {
        public ROCrossRefLogic CrossRefLogic;
        public ROCrossRefControllerJSON(ROCrossRefLogic CrossRefLogic)
        {
            this.CrossRefLogic = CrossRefLogic;

        }

        public Publication cambioDeModeloPublicacion(PublicacionInicial objInicial, string doi, Boolean publicacion_principal)
        {
            Publication publicacion = new Publication();
            if (objInicial != null)
            {
                publicacion.typeOfPublication = getType(objInicial);
                //publicacion.IDs = getIDs(objInicial);
                publicacion.title = getTitle(objInicial);
                //publicacion.Abstract = getAbstract(objInicial);
                publicacion.language = getLanguage(objInicial);
                publicacion.doi = doi;
                publicacion.url = getLinks(objInicial);
                //publicacion.dataIssued = getDate(objInicial);
                publicacion.pageStart = getPageStart(objInicial);
                publicacion.pageEnd = getPageEnd(objInicial);
                publicacion.hasKnowledgeAreas = getKnowledgeAreas(objInicial);
                //publicacion.freetextKeyword = getFreetextKeyword(objInicial);
                publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
                publicacion.seqOfAuthors = getAuthors(objInicial);
                publicacion.hasPublicationVenue = getJournal(objInicial);
                publicacion.hasMetric = getPublicationMetric(objInicial);
                if (publicacion_principal == true)
                {
                    //TODO: tengo que ver como hacer esto!
                    publicacion.bibliografia = getBiblografia(objInicial);
                    //publicacion.citas = getCitas(objInicial);
                }
                return publicacion;
            }
            else
            {
                return null;
            }

        }
        public string getType(PublicacionInicial objInicial)
        {
            if (objInicial.type != null)
            {
                if (objInicial.type == "journal-article")
                {
                    return "Journal Article";
                }
                if (objInicial.type == "proceedings-article")
                {
                    return "Conference Paper";
                }
                if (objInicial.type == "book-chapter")
                {
                    return "Chapter";
                }
                if (objInicial.type == "book")
                {
                    return "Book";
                }

            }
            return null;
        }

        // public List<String> getIDs(PublicacionInicial objInicial)
        // {
        //     return null;
        // }


        public string getTitle(PublicacionInicial objInicial)
        {
            if (objInicial.title != null & objInicial.title.Count >= 1)
            {
                return objInicial.title[0];
                //TODO: esto puede no estar bien! porque no se tiene que ser el primero... 


            }
            return null;
        }

        // public string getAbstract(PublicacionInicial objInicial)
        // {
        //     return null;
        // }

        public string getLanguage(PublicacionInicial objInicial)
        {
            if (objInicial.language != null)
            {

                return objInicial.language;
            }
            return null;
        }
        // public string getDoi(PublicacionInicial objInicial)
        // {
        //     return null;
        // }
        public List<string> getLinks(PublicacionInicial objInicial)
        {
            if (objInicial.URL != null)
            {
                List<string> links = new List<string>();
                links.Add(objInicial.URL);
                return links;
            }
            return null;
        }

        // public DateTimeValue getDate(PublicacionInicial objInicial)
        // {
        //     DateTimeValue date = new DateTimeValue();
        //     date.datimeTime = null;
        //     return date;
        // }

        public string getPageStart(PublicacionInicial objInicial)
        {
            if (objInicial.page != null)
            {
                if (objInicial.page.Contains("-"))
                {

                    string[] paguinas = objInicial.page.Split("-");
                    return paguinas[0];
                }
            }
            return null;
        }

        public string getPageEnd(PublicacionInicial objInicial)
        {
            if (objInicial.page != null)
            {
                if (objInicial.page.Contains("-"))
                {
                    string[] paguinas = objInicial.page.Split("-");
                    return paguinas[1];
                }
            }
            return null;
        }

        public List<KnowledgeAreas> getKnowledgeAreas(PublicacionInicial objInicial)
        {
            List<KnowledgeAreas> result = new List<KnowledgeAreas>();
            KnowledgeAreas areas = new KnowledgeAreas();
            List<KnowledgeArea> ultima_list = new List<KnowledgeArea>();
            if (objInicial.subject != null)
            {
                foreach (string name in objInicial.subject)
                {
                    KnowledgeArea a = new KnowledgeArea();
                    a.name = name;
                    ultima_list.Add(a);

                }
                if (ultima_list != new List<KnowledgeArea>())
                {
                    areas.resource = "CrossRef";
                    areas.knowledgeArea = ultima_list;
                    result.Add(areas);
                    return result;
                }
            }
            return null;
        }

        // public List<string> getFreetextKeyword(PublicacionInicial objInicial)
        // {
        //     return new List<string>();
        // }

        public Person getAuthorPrincipal(PublicacionInicial objInicial)
        {

            if (objInicial.author != null)
            {
                Person persona = new Person();

                foreach (Author autor in objInicial.author)
                {
                    if (autor.sequence == "first")
                    {

                        persona.ORCID = autor.ORCID;

                        List<string> name_inicial = new List<string>();
                        List<string> apellido = new List<string>();

                        if (autor.given != null)
                        {
                            name_inicial.Add(autor.given);
                        }
                        if (autor.family != null)
                        {
                            apellido.Add(autor.family);
                        }
                        if (name_inicial != null || apellido != null)
                        {
                            Name nombre = new Name();
                            if (name_inicial != new List<string>())
                            {
                                nombre.given = name_inicial;
                            }
                            if (apellido != new List<string>())
                            {
                                nombre.familia = apellido;
                            }
                            persona.name = nombre;
                        }

                        return persona;
                    }
                }
            }
            return null;
        }
        public List<Person> getAuthors(PublicacionInicial objInicial)
        {
            if (objInicial.author != null)
            {
                List<Person> autores = new List<Person>();
                foreach (Author autor in objInicial.author)
                {
                    Person persona = new Person();
                    if (autor.ORCID != null)
                    {
                        if (autor.ORCID.Contains("https://orcid.org/"))
                        {
                            int indice = autor.ORCID.IndexOf("org/");
                            persona.ORCID = autor.ORCID.Substring(indice + 4);
                        }
                        else
                        {
                            persona.ORCID = autor.ORCID;
                        }
                    }
                    //persona.ORCID = autor.ORCID;

                    List<string> name_inicial = new List<string>();
                    List<string> apellido = new List<string>();

                    if (autor.given != null)
                    {
                        name_inicial.Add(autor.given);
                    }
                    if (autor.family != null)
                    {
                        apellido.Add(autor.family);
                    }
                    if (name_inicial != null || apellido != null)
                    {
                        Name nombre = new Name();
                        if (name_inicial != new List<string>())
                        {
                            nombre.given = name_inicial;
                        }
                        if (apellido != new List<string>())
                        {
                            nombre.familia = apellido;
                        }
                        persona.name = nombre;
                    }
                    autores.Add(persona);

                }
                return autores;
            }
            return null;
        }

        public Source getJournal(PublicacionInicial objInicial)
        {
            if (objInicial.ISSN != null || objInicial.ContainerTitle != null)
            {
                Source journal = new Source();
                if (objInicial.ISSN != null)
                {
                    journal.issn = objInicial.ISSN;
                }
                if (objInicial.ContainerTitle != null & objInicial.ContainerTitle.Count >= 1)
                {
                    journal.name = objInicial.ContainerTitle[0];
                    //TODO: esto puede no estar bien! porque no se tiene que ser el primero... 
                }
                if (objInicial.ISBN != null)
                {
                    journal.isbn = objInicial.ISBN;

                }
                if (objInicial.type == "book" || objInicial.type == "book-chapter")
                {
                    journal.type = "Book";
                }
                if (objInicial.type == "journal-article")
                {
                    journal.type = "Journal";
                }


                if (journal != new Source())
                {
                    return journal;
                }
                else { return null; }
            }
            return null;
        }



        public List<PublicationMetric> getPublicationMetric(PublicacionInicial objInicial)
        {

            if (objInicial.IsReferencedByCount != null)
            {
                List<PublicationMetric> metricList = new List<PublicationMetric>();
                PublicationMetric metricPublicacion = new PublicationMetric();
                metricPublicacion.citationCount = objInicial.IsReferencedByCount.ToString();
                metricPublicacion.metricName = "CrossRef";
                metricList.Add(metricPublicacion);
                return metricList;
                //TODO esto tampoco se si esta muy bien porque lo ha sacado de Scopus entonces aqui que debo poner? 
            }
            return null;
        }

        public List<Publication> getBiblografia(PublicacionInicial objInicial)
        {

            if (objInicial.reference != null)
            {
                List<Publication> bibiografia = new List<Publication>();
                foreach (Reference bib in objInicial.reference)
                {
                    Publication pub = new Publication();
                    if (bib.DOI != null)
                    {
                        if (bib.DOI.Contains("https://doi.org/"))
                        {
                            int indice = bib.DOI.IndexOf("org/");
                            pub.doi = bib.DOI.Substring(indice + 4);

                        }
                        else
                        {
                            pub.doi = bib.DOI;
                        }
                    }
                    if (bib.ArticleTitle != null)
                    {
                        pub.title = bib.ArticleTitle;
                    }
                    if (bib.JournalTitle != null)
                    {
                        Source revista = new Source();
                        revista.name = bib.JournalTitle;
                        pub.hasPublicationVenue = revista;
                    }
                    if (pub.doi != null || pub.hasPublicationVenue != null || pub.title != null)
                    {
                        bibiografia.Add(pub);
                    }
                }
                return bibiografia;

            }
            //todo solo he dejado aquellos que estan estructurados... 
            return null;
        }

        // public List<Publication> getCitas(PublicacionInicial objInicial)
        // {
        //     return null;
        // }
    }
}