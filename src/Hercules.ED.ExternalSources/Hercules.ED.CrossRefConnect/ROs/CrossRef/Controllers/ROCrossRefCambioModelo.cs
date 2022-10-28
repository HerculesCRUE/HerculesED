using System.Collections.Generic;
using CrossRefConnect.ROs.CrossRef.Models.Inicial;
using CrossRefConnect.ROs.CrossRef.Models;
using System;
using CrossRefAPI.ROs.CrossRef.Models;
using System.Linq;

namespace CrossRefConnect.ROs.CrossRef.Controllers
{
    public class ROCrossRefControllerJSON
    {
        public ROCrossRefLogic CrossRefLogic;
        public ROCrossRefControllerJSON(ROCrossRefLogic CrossRefLogic)
        {
            this.CrossRefLogic = CrossRefLogic;

        }

        /// <summary>
        /// Obtiene los datos necesarios de las referencias.
        /// </summary>
        /// <param name="pPublicacionPrincipal">Publicación principal.</param>
        /// <returns>Listado con los datos necesarios de las referencias.</returns>
        public List<PubReferencias> ObtenerReferencias(PublicacionInicial pPublicacionPrincipal)
        {
            List<PubReferencias> listaReferencias = new List<PubReferencias>();

            if (pPublicacionPrincipal.reference != null && pPublicacionPrincipal.reference.Any())
            {
                foreach (Reference bibliografia in pPublicacionPrincipal.reference)
                {
                    PubReferencias pubRef = new PubReferencias();

                    // DOI
                    if (!string.IsNullOrEmpty(bibliografia.DOI))
                    {
                        if (bibliografia.DOI.Contains("https://doi.org/"))
                        {
                            int indice = bibliografia.DOI.IndexOf("org/");
                            pubRef.doi = bibliografia.DOI.Substring(indice + 4);
                        }
                        else
                        {
                            pubRef.doi = bibliografia.DOI;
                        }
                    }

                    // Año de publicación
                    if (!string.IsNullOrEmpty(bibliografia.year) && Int32.TryParse(bibliografia.year, out var anyo))
                    {
                        pubRef.anyoPublicacion = Int32.Parse(bibliografia.year);
                    }

                    // Título
                    if (!string.IsNullOrEmpty(bibliografia.ArticleTitle))
                    {
                        pubRef.titulo = bibliografia.ArticleTitle;
                    }
                    if (string.IsNullOrEmpty(pubRef.titulo) && !string.IsNullOrEmpty(bibliografia.SeriesTitle))
                    {
                        pubRef.titulo = bibliografia.SeriesTitle;
                    }

                    // Autores
                    if (!string.IsNullOrEmpty(bibliografia.author))
                    {
                        pubRef.autores = new Dictionary<string, string>();
                        pubRef.autores.Add(bibliografia.author, string.Empty);
                    }

                    // Nombre de la revista
                    if (!string.IsNullOrEmpty(bibliografia.JournalTitle))
                    {
                        pubRef.revista = bibliografia.JournalTitle;
                    }

                    // Si no tiene título ni doi, no es una referencia válida...
                    if (string.IsNullOrEmpty(pubRef.doi) && string.IsNullOrEmpty(pubRef.titulo))
                    {
                        continue;
                    }

                    listaReferencias.Add(pubRef);
                }
            }

            if (!listaReferencias.Any())
            {
                return null;
            }
            else
            {
                ROCrossRefLogic CrossRefObject = new ROCrossRefLogic();

                // Obtención de los datos faltantes de las referencias.
                foreach (PubReferencias pubRef in listaReferencias)
                {
                    if (!string.IsNullOrEmpty(pubRef.doi))
                    {
                        Root pubObtenida = CrossRefObject.getEnrichmentPublication(pubRef.doi);

                        if (pubObtenida != null)
                        {
                            PublicacionInicial publicacion = pubObtenida.message;

                            // Año de publicación
                            if (publicacion.created != null && !string.IsNullOrEmpty(publicacion.created.DateTime) && publicacion.created.DateTime.Contains("-"))
                            {
                                string anyo = publicacion.created.DateTime.Split("-")[0];
                                if (Int32.TryParse(anyo, out var anyoPub))
                                {
                                    pubRef.anyoPublicacion = Int32.Parse(anyo);
                                }
                            }

                            // Título
                            if (publicacion.title != null && publicacion.title.Any())
                            {
                                pubRef.titulo = publicacion.title[0];
                            }

                            // Autores
                            if (publicacion.author != null && publicacion.author.Any())
                            {
                                pubRef.autores = new Dictionary<string, string>();
                                foreach (Author autor in publicacion.author)
                                {
                                    string nombre = $@"{autor.given} {autor.family}".Trim();
                                    string orcid = string.Empty;

                                    if (!string.IsNullOrEmpty(autor.ORCID))
                                    {
                                        if (autor.ORCID.Contains("http://orcid.org/"))
                                        {
                                            int indice = autor.ORCID.IndexOf("org/");
                                            orcid = autor.ORCID.Substring(indice + 4);
                                        }
                                        else
                                        {
                                            orcid = autor.ORCID;
                                        }
                                    }

                                    if (!pubRef.autores.ContainsKey(nombre))
                                    {
                                        pubRef.autores.Add(nombre, orcid);
                                    }
                                    else
                                    {
                                        pubRef.autores[nombre] = orcid;
                                    }
                                }

                                // Nombre de la revista
                                if (publicacion.ContainerTitle != null && publicacion.ContainerTitle.Any())
                                {
                                    pubRef.revista = publicacion.ContainerTitle[0];
                                }

                                // Página de inicio
                                if (!string.IsNullOrEmpty(publicacion.page))
                                {
                                    if (publicacion.page.Contains("-"))
                                    {
                                        ComprobarPaginas(pubRef, publicacion.page.Split("-")[0], publicacion.page.Split("-")[1]);
                                    }
                                    else if (publicacion.page.Contains(" "))
                                    {
                                        ComprobarPaginas(pubRef, publicacion.page.Split(" ")[0], publicacion.page.Split(" ")[1]);
                                    }
                                    else if (publicacion.page.Contains("/"))
                                    {
                                        ComprobarPaginas(pubRef, publicacion.page.Split("/")[0], publicacion.page.Split("/")[1]);
                                    }
                                }
                            }
                        }
                    }
                }

                return listaReferencias;
            }
        }

        /// <summary>
        /// Comprueba que el número de páginas sea válido y los asigna.
        /// </summary>
        /// <param name="pPubRef">Publicación a guardar.</param>
        /// <param name="pInicio">Página de inicio.</param>
        /// <param name="pFin">Página de fin.</param>
        public void ComprobarPaginas(PubReferencias pPubRef, string pInicio, string pFin)
        {
            if (Int32.TryParse(pInicio, out var pagInicio))
            {
                pPubRef.paginaInicio = Int32.Parse(pInicio);
            }
            if (Int32.TryParse(pFin, out var pagFin))
            {
                pPubRef.paginaFin = Int32.Parse(pFin);
            }
        }

        public Publication cambioDeModeloPublicacion(PublicacionInicial objInicial, string doi, Boolean publicacion_principal)
        {
            Publication publicacion = new Publication();
            if (objInicial != null)
            {
                publicacion.typeOfPublication = getType(objInicial);
                publicacion.title = getTitle(objInicial);
                publicacion.language = getLanguage(objInicial);
                publicacion.doi = doi;
                publicacion.url = getLinks(objInicial);
                publicacion.dataIssued = getDate(objInicial);
                publicacion.pageStart = getPageStart(objInicial);
                publicacion.pageEnd = getPageEnd(objInicial);
                publicacion.hasKnowledgeAreas = getKnowledgeAreas(objInicial);
                publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
                publicacion.seqOfAuthors = getAuthors(objInicial);
                publicacion.hasPublicationVenue = getJournal(objInicial);
                publicacion.hasMetric = getPublicationMetric(objInicial);
                if (publicacion_principal == true)
                {
                    publicacion.bibliografia = getBiblografia(objInicial);
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

        public string getTitle(PublicacionInicial objInicial)
        {
            if (objInicial.title != null && objInicial.title.Count >= 1)
            {
                return objInicial.title[0];


            }
            return null;
        }

        public string getLanguage(PublicacionInicial objInicial)
        {
            if (objInicial.language != null)
            {

                return objInicial.language;
            }
            return null;
        }

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

        public DateTimeValue getDate(PublicacionInicial objInicial)
        {
            if (objInicial != null)
            {
                if (objInicial.created != null)
                {
                    if (objInicial.created.DateTime != null)
                    {
                        DateTimeValue date = new DateTimeValue();
                        date.datimeTime = objInicial.created.DateTime.Substring(0, 10);

                        return date;
                    }
                    else { return null; }
                }
                else { return null; }
            }
            else { return null; }
        }

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

        public Person getAuthorPrincipal(PublicacionInicial objInicial)
        {

            if (objInicial.author != null)
            {
                Person persona = new Person();
                persona.fuente = "CrossRef";
                foreach (Author autor in objInicial.author)
                {
                    if (autor.sequence == "first")
                    {
                        RellenarAutor(persona, autor);
                        return persona;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Permite mapear el objeto autor.
        /// </summary>
        /// <param name="pPersona">Persona a devolver.</param>
        /// <param name="pAutor">Persona con los datos iniciales.</param>
        public void RellenarAutor(Person pPersona, Author pAutor)
        {
            if (pAutor.ORCID != null)
            {
                if (pAutor.ORCID.Contains("https://orcid.org/") || pAutor.ORCID.Contains("http://orcid.org/"))
                {
                    int indice = pAutor.ORCID.IndexOf("org/");
                    pPersona.ORCID = pAutor.ORCID.Substring(indice + 4);
                }
                else
                {
                    pPersona.ORCID = pAutor.ORCID;
                }
            }

            List<string> name_inicial = new List<string>();
            List<string> apellido = new List<string>();
            if (pAutor.given != null)
            {
                name_inicial.Add(pAutor.given);
            }
            if (pAutor.family != null)
            {
                apellido.Add(pAutor.family);
            }

            Name nombre = new Name();
            if (name_inicial.Any())
            {
                nombre.given = name_inicial;
            }
            if (apellido.Any())
            {
                nombre.familia = apellido;
            }

            pPersona.name = nombre;
        }

        public List<Person> getAuthors(PublicacionInicial objInicial)
        {
            if (objInicial.author != null)
            {
                List<Person> autores = new List<Person>();
                foreach (Author autor in objInicial.author)
                {
                    Person persona = new Person();
                    persona.fuente = "CrossRef";
                    RellenarAutor(persona, autor);
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
                if (objInicial.ContainerTitle != null && objInicial.ContainerTitle.Count >= 1)
                {
                    journal.name = objInicial.ContainerTitle[0];
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
            return null;
        }

    }
}