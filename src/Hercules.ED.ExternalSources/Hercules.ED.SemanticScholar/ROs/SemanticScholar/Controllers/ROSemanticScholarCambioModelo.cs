using System;
using System.Collections.Generic;
using System.Linq;
using SemanticScholarAPI.ROs.SemanticScholar.Models;
using SemanticScholarAPI.ROs.SemanticScholar.Models;
using SemanticScholarAPI.ROs.SemanticScholar.Models.Inicial;

namespace SemanticScholarAPI.ROs.SemanticScholar.Controllers
{
    public class ROSemanticScholarControllerJSON
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

        public List<PubReferencias> getReferences(SemanticScholarObj pData)
        {
            List<PubReferencias> listaReferencias = new List<PubReferencias>();

            if (pData.references != null && pData.references.Any())
            {
                foreach (Reference pubRef in pData.references)
                {
                    PubReferencias referencia = new PubReferencias();

                    // DOI
                    if (!string.IsNullOrEmpty(pubRef.doi))
                    {
                        referencia.doi = pubRef.doi;
                    }

                    // URL
                    if (!string.IsNullOrEmpty(pubRef.url))
                    {
                        referencia.url = pubRef.url;
                    }

                    // A�o de la publicaci�n
                    if (pubRef.year != null)
                    {
                        referencia.anyoPublicacion = pubRef.year;
                    }

                    // T�tulo
                    if (!string.IsNullOrEmpty(pubRef.title))
                    {
                        referencia.titulo = pubRef.title;
                    }

                    // Revista
                    if (!string.IsNullOrEmpty(pubRef.venue))
                    {
                        referencia.revista = pubRef.venue;
                    }

                    // Autores
                    if (pubRef.authors != null && pubRef.authors.Any())
                    {
                        referencia.autores = new Dictionary<string, string>();
                        foreach (SemanticScholarAPI.ROs.SemanticScholar.Models.Author autor in pubRef.authors)
                        {
                            if (!referencia.autores.ContainsKey(autor.name))
                            {
                                referencia.autores.Add(autor.name, autor.authorId);
                            }
                            else
                            {
                                referencia.autores[autor.name] = autor.authorId;
                            }
                        }
                    }

                    // Si no tiene t�tulo ni doi, no es una referencia v�lida...
                    if (string.IsNullOrEmpty(referencia.doi) && string.IsNullOrEmpty(referencia.titulo))
                    {
                        continue;
                    }

                    listaReferencias.Add(referencia);
                }
            }

            if (!listaReferencias.Any())
            {
                return null;
            }
            else
            {
                return listaReferencias;
            }
        }

        public Publication cambioDeModeloPublicacion(Root objInicial)
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
                publicacion.dataOrigin = "SemanticScholar";

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
            if (objInicial.paperId != null)
            {
                IDs.Add("SemanticScholar: " + objInicial.paperId);

            }
            if (objInicial.externalIds != null)
            {
                if (objInicial.externalIds.ArXiv != null)
                {
                    IDs.Add("ArXiv: " + objInicial.externalIds.ArXiv);
                }
                if (objInicial.externalIds.MAG != null)
                {
                    IDs.Add("MAG: " + objInicial.externalIds.MAG);
                }
                if (objInicial.externalIds.PubMedCentral != null)
                {
                    IDs.Add("PubMedCentral: " + objInicial.externalIds.PubMedCentral);
                }
            }
            if (IDs.Count == 0)
            {
                return null;
            }
            else
            {
                return IDs;
            }
        }


        public string getTitle(Root objInicial)
        {
            if (objInicial.title != null)
            {
                return objInicial.title;
            }
            return null;
        }

        public string getAbstract(Root objInicial)
        {
            if (objInicial.@abstract != null)
            {
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
            if (objInicial.url != null)
            {
                List<string> links = new List<string>();
                links.Add(objInicial.url);
                return links;
            }
            return null;
        }

        public DateTimeValue getDate(Root objInicial)
        {

            if (objInicial.year != null)
            {
                DateTimeValue date = new DateTimeValue();
                date.datimeTime = null;
                date.datimeTime = objInicial.year;
            }
            return null;
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
            if (objInicial.authors != null)
            {
                List<Person> autores = new List<Person>();
                foreach (SemanticScholarAPI.ROs.SemanticScholar.Models.Inicial.Author author in objInicial.authors)
                {
                    Person persona = new Person();
                    persona.fuente = "SemanticScholar";

                    if (author.name != null)
                    {
                        List<string> nombres = new List<string>();
                        nombres.Add(author.name);
                        Name nom = new Name();
                        nom.nombre_completo = nombres;
                        persona.name = nom;
                    }
                    if (author.authorId != null)
                    {
                        List<string> ids = new List<string>();
                        ids.Add("SemanticScholar: " + author.authorId);
                        persona.IDs = ids;
                    }

                    autores.Add(persona);
                }
                return autores;
            }
            return null;
        }

        public Source getJournal(Root objInicial)
        {
            if (objInicial.venue != null && objInicial.venue != "")
            {
                Source revista = new Source();
                revista.name = objInicial.venue;
                return revista;
            }
            return null;
        }

        public List<PublicationMetric> getPublicationMetric(Root objInicial)
        {

            if (objInicial.citationCount != 0)
            {
                List<PublicationMetric> metriscas = new List<PublicationMetric>();
                PublicationMetric metricPublicacion = new PublicationMetric();
                metricPublicacion.citationCount = objInicial.citationCount.ToString();
                metricPublicacion.metricName = "SemanticScholar";
                metriscas.Add(metricPublicacion);
                return metriscas;
            }
            return null;
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