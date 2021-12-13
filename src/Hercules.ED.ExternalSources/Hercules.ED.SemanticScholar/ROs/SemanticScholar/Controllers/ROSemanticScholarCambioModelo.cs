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
                date.datimeTime = objInicial.year.ToString();
                //todo: esto no es del todo correcto! porque no es una fecha sino un año! 
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
                foreach (Author author in objInicial.authors)
                {
                    Person persona = new Person();
                    int i = this.SemanticScholarLogic.autores_orcid.Count;
                    string orcid = null;
                    string name = null;
                    string familia = null;
                    string completo = null;
                    string idss = null; ;
                    string links = null;
                    if (author.name != null)
                    {
                        List<string> nombres = new List<string>();
                        nombres.Add(author.name);
                        completo=author.name;
                        Name nom = new Name();
                        nom.nombre_completo = nombres;
                        persona.name = nom;
                    }
                    if (author.authorId != null)
                    {
                        List<string> ids = new List<string>();
                        ids.Add("SemanticScholar: " + author.authorId);
                        persona.IDs = ids;
                        idss="SemanticScholar: " + author.authorId;
                    }
                     persona.id_persona=i.ToString();
                    autores.Add(persona);
                                        if(orcid!=null || name!=null ||familia!=null ||completo!=null || idss!=null || links!=null){
                                        Tuple<string,string, string, string, string, string> tupla = new Tuple<string,string, string, string, string, string>(orcid,name,familia,completo,idss,links);
                                        
                                        this.SemanticScholarLogic.autores_orcid[i.ToString()]=tupla;}
                                        
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

            if (objInicial.citationCount != null)
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