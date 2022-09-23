using System.Collections.Generic;
using OpenAireConnect.ROs.OpenAire.Models;
using OpenAireConnect.ROs.OpenAire.Models.Inicial;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Data;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace OpenAireConnect.ROs.OpenAire.Controllers
{
    public class ROOpenAireControllerJSON
    {
        public ROOpenAireLogic OpenAireLogic;

        public ROOpenAireControllerJSON(ROOpenAireLogic OpenAireLogic)
        {
            this.OpenAireLogic = OpenAireLogic;
        }

        // TODO: Sacarlo a clase Enum.
        // CONSTANTES
        public const string ARTICLE = "Article";
        public const string JOURNAL_ARTICLE = "Journal Article";
        public const string BOOK = "Book";
        public const string BOOK_CHAPTER = "Book Chapter";
        public const string CHAPTER = "Chapter";
        public const string PROCEEDINGS_PAPER = "Proceedings Paper";
        public const string CONFERENCE_PAPER = "Conference Paper";
        public const string JOURNAL = "Journal";

        /// <summary>
        /// Contruye el objeto de la publicación con los datos obtenidos.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación de entrada.</param>
        /// <returns></returns>
        public Publication getPublicacionCita(Result2 pPublicacionIn)
        {
            Publication publicacionFinal = new Publication();
            //publicacionFinal.typeOfPublication = getType(pPublicacionIn);
            //publicacionFinal.IDs = getOpenAireID(pPublicacionIn);
            publicacionFinal.title = getTitle(pPublicacionIn);
            publicacionFinal.Abstract = getAbstract(pPublicacionIn);
            publicacionFinal.doi = getDoi(pPublicacionIn);
            publicacionFinal.dataIssued = getDate(pPublicacionIn);
            //publicacionFinal.hasKnowledgeAreas = getKnowledgeAreas(pPublicacionIn);
            publicacionFinal.freetextKeywords = getFreetextKeyword(pPublicacionIn);
            publicacionFinal.seqOfAuthors = getAuthors(pPublicacionIn);
            publicacionFinal.correspondingAuthor = publicacionFinal.seqOfAuthors[0];
            publicacionFinal.hasPublicationVenue = getJournal(pPublicacionIn);
            publicacionFinal.openAccess = getOpenAccess(pPublicacionIn);
            publicacionFinal.volume = getVolume(pPublicacionIn);
            //publicacionFinal.hasMetric = getPublicationMetric(pPublicacionIn);

            return publicacionFinal;
        }

        public List<Publication> getListPublication(Root objInicial)
        {
            List<Publication> listaResultado = new List<Publication>();
            if (objInicial != null && objInicial.response != null && objInicial.response.results != null && objInicial.response.results.result != null)
            {
                foreach (Result2 rec in objInicial.response.results.result)
                {
                    try
                    {
                        Publication publicacion = cambioDeModeloPublicacion(rec);

                        if (publicacion != null)
                        {
                            listaResultado.Add(publicacion);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            return listaResultado;
        }

        public Publication cambioDeModeloPublicacion(Result2 objInicial)
        {
            Publication publicacion = new Publication();
            publicacion.typeOfPublication = getPublicationType(objInicial);
            publicacion.title = getTitle(objInicial);
            publicacion.Abstract = getAbstract(objInicial);
            publicacion.language = getLanguage(objInicial);
            publicacion.doi = getDoi(objInicial);
            publicacion.dataIssued = getDate(objInicial);
            publicacion.pageStart = getPageStart(objInicial);
            publicacion.pageEnd = getPageEnd(objInicial);
            publicacion.freetextKeywords = getFreetextKeyword(objInicial);
            publicacion.seqOfAuthors = getAuthors(objInicial);
            publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
            publicacion.hasPublicationVenue = getJournal(objInicial);
            publicacion.openAccess = getOpenAccess(objInicial);
            publicacion.volume = getVolume(objInicial);
            if (publicacion.typeOfPublication == CHAPTER)
            {
                publicacion.doi = null;
            }
            publicacion.dataOrigin = "OpenAire";
            return publicacion;
        }

        /// <summary>
        /// Obtiene la infromación del autor princpipal
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el autor princiapl.</param>
        /// <returns>Autor Principal.</returns>
        public Person getAuthorPrincipal(Result2 pPublicacionIn)
        {

            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.creator != null)
            {
                foreach (Creator author_fOriginal in pPublicacionIn.metadata.OafEntity.OafResult.creator)
                {
                    if (author_fOriginal.Rank == "1")
                    {
                        Person author = new Person();
                        author.fuente = "OpenAire";

                        if (author_fOriginal.Orcid != null && !string.IsNullOrEmpty(author_fOriginal.Orcid))
                        {
                            author.ORCID = author_fOriginal.Orcid;
                        }

                        Name nom = new Name();
                        if (author_fOriginal.Text != null && !string.IsNullOrEmpty(author_fOriginal.Text))
                        {
                            // Separar nombre de apellidos.
                            string nombreOriginal = author_fOriginal.Text.Trim();
                            string nombre = string.Empty;
                            string apellidos = string.Empty;

                            if (nombreOriginal.Contains(". "))
                            {
                                nombre = nombreOriginal.Split(". ")[0].Trim() + ".";
                                apellidos = nombreOriginal.Substring(nombreOriginal.IndexOf(". ") + 1).Trim();
                            }
                            else if (nombreOriginal.Contains(" "))
                            {
                                nombre = nombreOriginal.Split(" ")[0].Trim();
                                apellidos = nombreOriginal.Substring(nombreOriginal.IndexOf(" ")).Trim();
                            }

                            nom.nombre_completo = new List<string>() { nombreOriginal };
                            nom.given = new List<string>() { nombre };
                            nom.familia = new List<string>() { apellidos };
                            author.name = nom;
                        }

                        return author;
                    }
                }
            }

            return null;
        }

        /// <summary>.
        /// Obtiene el tipo de publicación.
        /// </summary>
        /// <param name="pPublicacionIn"></param>
        /// <returns></returns>
        public string getPublicationType(Result2 pPublicacionIn)
        {
            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.resourcetype != null)
            {
                switch (pPublicacionIn.metadata.OafEntity.OafResult.resourcetype.Classname.ToLower().Trim())
                {
                    case "publication":
                        return JOURNAL_ARTICLE;
                    case "book":
                        return BOOK;
                    case "conference paper":
                        return CONFERENCE_PAPER;
                    case "part of book or chapter of book":
                        return CHAPTER;
                    default:
                        return JOURNAL_ARTICLE;
                }
            }
            return null;
        }


        /// <summary>
        /// Obtiene el título de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el título.</param>
        /// <returns>Título de la publicación.</returns>
        public string getTitle(Result2 pPublicacionIn)
        {
            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.title != null)
            {
                foreach (Title item in pPublicacionIn.metadata.OafEntity.OafResult.title)
                {
                    if (item.Classid == "main title" && !string.IsNullOrEmpty(item.Text))
                    {
                        return item.Text;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Obtiene la descripción de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener la descripción.</param>
        /// <returns>Descripción.</returns>
        public string getAbstract(Result2 pPublicacionIn)
        {
            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.description != null)
            {
                string descripcion = string.Empty;
                foreach (Descripton item in pPublicacionIn.metadata.OafEntity.OafResult.description)
                {
                    if (!string.IsNullOrEmpty(item.descripton))
                    {
                        descripcion += item.descripton.Trim() + " ";
                    }
                }
                return descripcion.Trim();
            }
            return null;
        }


        /// <summary>
        /// Obtiene el idioma de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el idioma.</param>
        /// <returns>Idioma.</returns>
        public string getLanguage(Result2 pPublicacionIn)
        {
            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.language != null)
            {
                string idioma = pPublicacionIn.metadata.OafEntity.OafResult.language.Classname;

                if (!string.IsNullOrEmpty(idioma) && idioma != "Undetermined")
                {
                    return idioma;
                }
            }

            return null;
        }


        /// <summary>
        /// Obtiene el identificador DOI de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el DOI.</param>
        /// <returns>DOI.</returns>
        public string getDoi(Result2 pPublicacionIn)
        {
            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.pid != null)
            {
                foreach (Title item in pPublicacionIn.metadata.OafEntity.OafResult.pid)
                {
                    if (item.Classid == "doi" && !string.IsNullOrEmpty(item.Text))
                    {
                        return item.Text;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene la fecha de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener la fecha.</param>
        /// <returns>Fecha de la publiación.</returns>
        public DateTimeValue getDate(Result2 pPublicacionIn)
        {

            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.relevantdate != null)
            {
                foreach (Relevantdate item in pPublicacionIn.metadata.OafEntity.OafResult.relevantdate)
                {
                    if (item.Classname == "created")
                    {
                        DateTimeValue date = new DateTimeValue();
                        date.datimeTime = item.date;
                        return date;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene el número de la página de inicio.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el número de la página.</param>
        /// <returns>Número de la página.</returns>
        public string getPageStart(Result2 pPublicacionIn)
        {
            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.journal != null)
            {
                if (pPublicacionIn.metadata.OafEntity.OafResult.journal.Sp != null)
                {
                    return pPublicacionIn.metadata.OafEntity.OafResult.journal.Sp;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene el número de la página de fin.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el número de la página.</param>
        /// <returns>Número de la página.</returns>
        public string getPageEnd(Result2 pPublicacionIn)
        {
            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.journal != null)
            {
                if (pPublicacionIn.metadata.OafEntity.OafResult.journal.Ep != null)
                {
                    return pPublicacionIn.metadata.OafEntity.OafResult.journal.Ep;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene las etiquetas de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener las etiquetas.</param>
        /// <returns>Listado de etiquetas.</returns>
        public List<FreetextKeywords> getFreetextKeyword(Result2 pPublicacionIn)
        {
            FreetextKeywords result = new FreetextKeywords();
            result.source = "OpenAire";

            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.subject != null && pPublicacionIn.metadata.OafEntity.OafResult.subject.Any())
            {
                HashSet<string> lista_keyword = new HashSet<string>();

                foreach (Subject sub in pPublicacionIn.metadata.OafEntity.OafResult.subject)
                {
                    if (sub.Text.Contains(";"))
                    {
                        foreach (string key in sub.Text.Split(";"))
                        {
                            lista_keyword.Add(key.Trim());
                        }
                    }
                    else if (sub.Classid == "keyword" && !string.IsNullOrEmpty(sub.Text))
                    {
                        lista_keyword.Add(sub.Text);
                    }
                }

                if (lista_keyword.Any())
                {
                    result.freetextKeyword = lista_keyword.ToList();
                    return new List<FreetextKeywords>() { result };
                }
            }

            return null;
        }


        /// <summary>
        /// Obtiene los autores de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener autores.</param>
        /// <returns>Lista de personas.</returns>
        public List<Person> getAuthors(Result2 pPublicacionIn)
        {
            List<Person> list_author = new List<Person>();

            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.creator != null)
            {
                foreach (Creator author_fOriginal in pPublicacionIn.metadata.OafEntity.OafResult.creator)
                {
                    Person author = new Person();
                    if (!string.IsNullOrEmpty(author_fOriginal.Rank))
                    {
                        author.orden = Int32.Parse(author_fOriginal.Rank);
                    }
                    author.fuente = "OpenAire";

                    if (author_fOriginal.Orcid != null && !string.IsNullOrEmpty(author_fOriginal.Orcid))
                    {
                        author.ORCID = author_fOriginal.Orcid;
                    }

                    Name nom = new Name();
                    if (author_fOriginal.Text != null && !string.IsNullOrEmpty(author_fOriginal.Text))
                    {
                        // Separar nombre de apellidos.
                        string nombreOriginal = author_fOriginal.Text.Trim();
                        string nombre = string.Empty;
                        string apellidos = string.Empty;

                        if (nombreOriginal.Contains(", "))
                        {
                            nombre = nombreOriginal.Split(", ")[1].Trim();
                            apellidos = nombreOriginal.Substring(0, nombreOriginal.IndexOf(", ")).Trim();
                        }
                        else if (nombreOriginal.Contains(". "))
                        {
                            nombre = nombreOriginal.Split(". ")[0].Trim() + ".";
                            apellidos = nombreOriginal.Substring(nombreOriginal.IndexOf(". ") + 1).Trim();
                        }
                        else if (nombreOriginal.Contains(" "))
                        {
                            nombre = nombreOriginal.Split(" ")[0].Trim();
                            apellidos = nombreOriginal.Substring(nombreOriginal.IndexOf(" ")).Trim();
                        }

                        nom.nombre_completo = new List<string>() { nombreOriginal };
                        nom.given = new List<string>() { nombre };
                        nom.familia = new List<string>() { apellidos };
                        author.name = nom;

                        list_author.Add(author);
                    }
                }
            }

            if (list_author.Any())
            {
                return list_author;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Obtiene los datos de la revista.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener los datos de la revista.</param>
        /// <returns>Revista.</returns>
        public Source getJournal(Result2 pPublicacionIn)
        {

            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.journal != null)
            {
                Source source = new Source();
                source.type = "Journal";

                // Siempre son journal no dan imformacion de los libros. 
                if (pPublicacionIn.metadata.OafEntity.OafResult.journal.Eissn != null)
                {
                    source.eissn = pPublicacionIn.metadata.OafEntity.OafResult.journal.Eissn;
                }

                if (pPublicacionIn.metadata.OafEntity.OafResult.journal.Iss != null)
                {
                    List<string> issns = new List<string>();
                    issns.Add(pPublicacionIn.metadata.OafEntity.OafResult.journal.Iss);
                    source.issn = issns;
                }

                if (pPublicacionIn.metadata.OafEntity.OafResult.journal.Text != null)
                {
                    source.name = pPublicacionIn.metadata.OafEntity.OafResult.journal.Text;
                }

                return source;
            }

            return null;
        }

        public string getVolume(Result2 pPublicacionIn)
        {
            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.journal != null)
            {
                if (pPublicacionIn.metadata.OafEntity.OafResult.journal.Vol != null)
                {
                    return pPublicacionIn.metadata.OafEntity.OafResult.journal.Vol;
                }
            }

            return null;
        }

        public bool? getOpenAccess(Result2 pPublicacionIn)
        {
            if (pPublicacionIn.metadata != null && pPublicacionIn.metadata.OafEntity != null && pPublicacionIn.metadata.OafEntity.OafResult != null && pPublicacionIn.metadata.OafEntity.OafResult.bestaccessright != null)
            {
                if (pPublicacionIn.metadata.OafEntity.OafResult.bestaccessright.Classname == "Open Access")
                {
                    if (pPublicacionIn.metadata.OafEntity.OafResult.bestaccessright.Classid == "OPEN")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return null;
        }
    }
}