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
        public ROScopusLogic scopusLogic;
        public ROScopusControllerJSON(ROScopusLogic scopusLogic)
        {
            this.scopusLogic = scopusLogic;

        }

        public List<Publication> getListPublicatio(string stringInicial, string Date)
        {
            //Console.Write(stringInicial);
            Root objInicial = JsonConvert.DeserializeObject<Root>(stringInicial);

            List<Publication> sol = new List<Publication>();
            //---modificacion en otro repo!  ---------------------------------------------------------
            List<Entry> lista_item = objInicial.SearchResults.entry;

            for (int i = 0; i < lista_item.Count; i++)
            {
                Publication a = new Publication();
                Entry entidad = lista_item[i];
                //Console.Write("---------------------------");

                //Console.Write(entidad.DcIdentifier);
                //Console.Write("---------------------------");
                string[] id_code = entidad.DcIdentifier.Split(':');
                string fechaPublicacion = entidad.PrismCoverDate;
                if (DateTime.Parse(fechaPublicacion) > DateTime.Parse(Date))
                {
                    //Console.Write(entidad.PrismCoverDate);
                    //Console.Write("---");
                    //Console.Write(entidad.PrismCoverDisplayDate);

                    string id = id_code[1];
                    string informacion; //= this.scopusLogic.getStringPublication(id);
                    Publication_root info_publicacion_root; //= getPublication(informacion);
                                                            //------------------------------------------------------------------------
                                                            //Console.Write(id);
                                                            //Console.Write("-------");
                                                            // if (info_publicacion_root != null)
                                                            //{
                    if (entidad.subtype == "cp")
                    {
                        ConferencePaper conferencePaper = new ConferencePaper();
                        conferencePaper.typeOfPublication = "ConferencePaper";
                        conferencePaper.doi = getDoi_llamada_inicial(entidad);
                        conferencePaper.title = getTitle_llamada_inicial(entidad);
                        conferencePaper.dataIssued = getDate_llamada_inicial(entidad);
                        conferencePaper.hasMetric = getPublicationMetric_llamada_inicial(entidad);
                        List<String> paginas = getPageRage(entidad);
                        conferencePaper.pageStart = paginas[0];
                        conferencePaper.pageEnd = paginas[1];
                        conferencePaper.hasPublicationVenue = getJournal_inicial(entidad);


                        //conferencePaper.bibliografia = getBibliografia(info_publicacion_root);
                        Console.Write(conferencePaper.doi);
                        Console.Write("hhh");
                        Console.Write("-----------");
                        informacion = this.scopusLogic.getStringPublication(id);
                        info_publicacion_root = getPublication(informacion);
                        if (info_publicacion_root != null)
                        {
                            ConferencePaper conferencePaper_completo = getConferencePaper(info_publicacion_root, conferencePaper);
                            sol.Add(conferencePaper_completo);
                        }
                    }
                    else if (entidad.subtype == "ar")
                    {
                        JournalArticle conferencePaper = new JournalArticle();
                        conferencePaper.typeOfPublication = "JournalArticle";
                        conferencePaper.doi = getDoi_llamada_inicial(entidad);
                        conferencePaper.title = getTitle_llamada_inicial(entidad);
                        conferencePaper.dataIssued = getDate_llamada_inicial(entidad);
                        conferencePaper.hasMetric = getPublicationMetric_llamada_inicial(entidad);
                        List<string> paginas = getPageRage(entidad);
                        conferencePaper.pageStart = paginas[0];
                        conferencePaper.pageEnd = paginas[1];
                        conferencePaper.hasPublicationVenue = getJournal_inicial(entidad);

                         Console.Write(conferencePaper.doi);
                                                 Console.Write("hhh");

                        Console.Write("-----------");
                        //conferencePaper.bibliografia = getBibliografia(info_publicacion_root);
                        // Completamos los datos con la segunda llamada. 
                        informacion = this.scopusLogic.getStringPublication(id);
                        info_publicacion_root = getPublication(informacion);
                        if (info_publicacion_root != null)
                        {

                            JournalArticle JournalArticle_completa = getJournalArticle(info_publicacion_root, conferencePaper);
                            sol.Add(JournalArticle_completa);
                        }

                    }
                    else
                    {
                        Publication publicacion = new Publication();
                        publicacion.typeOfPublication = "AcademicArticle"; // TODO no tengo claro si aqui seria Article o Academic Article 
                        publicacion.doi = getDoi_llamada_inicial(entidad);
                        publicacion.title = getTitle_llamada_inicial(entidad);
                        publicacion.dataIssued = getDate_llamada_inicial(entidad);
                        publicacion.hasMetric = getPublicationMetric_llamada_inicial(entidad);
                        publicacion.hasPublicationVenue = getJournal_inicial(entidad);
                        //Console.Write(entidad.PrismPageRange);
                        List<string> paginas = getPageRage(entidad);

                        publicacion.pageStart = paginas[0];
                        publicacion.pageEnd = paginas[1];
                        Console.Write(publicacion.doi);
                        Console.Write("hhh");
                        Console.Write("-----------");
                        //Console.Write(publicacion.doi);
                        //publicacion.bibliografia = getBibliografia(info_publicacion_root);
                        informacion = this.scopusLogic.getStringPublication(id);
                        info_publicacion_root = getPublication(informacion);
                        if (info_publicacion_root != null)
                        {

                            // Completamos los metadatos de la publicacion con la segunda llamada. 
                            Publication publication_completa = Publication_inicial(info_publicacion_root, publicacion);
                            sol.Add(publication_completa);
                        }


                    }
                }
            }
            return sol;
        }

        public List<Publication> getBibliografia(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                if (objInicial.AbstractsRetrievalResponse.item.bibrecord.tail != null)
                {
                    List<Publication> bibliografia = new List<Publication>();
                    if (objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference != null)
                    {
                        for (int j = 0; j < objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference.Count; j++)
                        {
                            if (objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference[j].RefInfo.RefdItemidlist != null)
                            {
                                string scopus_id = null;
                                try
                                {
                                    Itemid hey = JsonConvert.DeserializeObject<Itemid>(objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference[j].RefInfo.RefdItemidlist.itemid.ToString());
                                    scopus_id = hey.a;
                                }
                                catch
                                {
                                    JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference[j].RefInfo.RefdItemidlist.itemid.ToString());
                                    foreach (JContainer var in hey)
                                    {
                                        Itemid ee = JsonConvert.DeserializeObject<Itemid>(var.ToString());
                                        if (ee.Idtype == "SGR" ^ ee.Idtype == "SCOPUS")
                                        {
                                            scopus_id = ee.a;
                                        }
                                    }
                                }
                                Publication_root obj_inicial = null;
                                if (scopus_id != null)
                                {
                                    string publicacion_ref = this.scopusLogic.getStringPublication(scopus_id);
                                    obj_inicial = getPublication(publicacion_ref);
                                }
                                if (obj_inicial != null)
                                {
                                    Publication publication_def = getGenericPublication(obj_inicial);
                                    bibliografia.Add(publication_def);
                                }
                            }
                        }
                        return bibliografia;
                    }
                    else { return null; }
                }
                else { return null; }
            }
            else { return null; }
        }

        public Publication_root getPublication(string stringPublication)
        {
            Publication_root info_publicacion = new Publication_root();
            try
            {
                //Console.Write(stringPublication);
                //string path = @"C:\Users\mpuer\Desktop\MyTest.txt";
                //System.IO.File.WriteAllText(path, stringPublication);
                info_publicacion = JsonConvert.DeserializeObject<Publication_root>(stringPublication);
            }
            catch
            {
                string path = @"C:\Users\mpuer\Desktop\MyTest.txt";
                System.IO.File.WriteAllText(path, stringPublication);
                //Todo: comentar si quieren hacer algo aqui con esta excepción. 
                //info_publicacion = null;
                Console.Write("Error de deserializacion del articulo.");
            }
            return info_publicacion;
        }

        //-----------------------------------------
        private string getAbstract(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                string abstractt = objInicial.AbstractsRetrievalResponse.coredata.DcDescription;
                return abstractt;
            }
            else
            {
                string abstractt = null;
                return abstractt;
            }
        }
        //extraccion de pageEnd en el modelo inicial! 
        private string getPageEnd(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                string pageEnd = objInicial.AbstractsRetrievalResponse.coredata.PrismEndingPage;
                return pageEnd;
            }
            else
            {
                string pageEnd = null;
                return pageEnd;
            }
        }
        // extraccion del string de page star en el modelo inicial.
        public string getPageStart(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                string pageStart = objInicial.AbstractsRetrievalResponse.coredata.PrismStartingPage;
                return pageStart;
            }
            else
            {
                string pageStart = null;
                return pageStart;
            }
        }
        public List<String> getPageRage(Entry objInicial)
        {
            List<String> paguinas = new List<string>();
            if (objInicial != null & objInicial.PrismPageRange != null)
            {
                string coverPage = objInicial.PrismPageRange;
                //Console.Write(coverPage);
                //Console.Write("------------------------");
                if (coverPage.Contains("-"))
                {
                    string[] pages = coverPage.Split("-");
                    paguinas.Add(pages[0]);
                    paguinas.Add(pages[1]);
                    return paguinas;
                }
                else
                {

                    paguinas.Add(null);
                    paguinas.Add(null);
                    return paguinas;
                }
            }
            else
            {

                paguinas.Add(null);
                paguinas.Add(null);
                return paguinas;
            }
        }


        public string getTitle_llamada_inicial(Entry objInicial)
        {
            if (objInicial != null & objInicial.DcTitle != null)
            {
                string title = objInicial.DcTitle;
                return title;
            }
            else
            {
                string title = null;
                return title;
            }
        }
        public string getTitle(Publication_root objInicial)
        {
            if (objInicial != null & objInicial.AbstractsRetrievalResponse != null)
            {
                string title = objInicial.AbstractsRetrievalResponse.coredata.DcTitle;
                return title;
            }
            else
            {
                string title = null;
                return title;
            }
        }

        public List<Url> getLinks(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                List<Url> url = new List<Url>();
                if (objInicial.AbstractsRetrievalResponse.coredata.link != null)
                {
                    for (int i = 0; i < objInicial.AbstractsRetrievalResponse.coredata.link.Count; i++)
                    {
                        Url link = new Url();
                        if (objInicial.AbstractsRetrievalResponse.coredata.link[i].Rel == "scopus")
                        {
                            link.link = objInicial.AbstractsRetrievalResponse.coredata.link[i].Href;
                            url.Add(link);
                        }
                    }
                    return url;
                }
                else
                {
                    List<Url> url_null = null;
                    return url_null;
                }
            }
            else
            {
                List<Url> url_null = null;
                return url_null;
            }
        }
        public string getDoi_llamada_inicial(Entry objInicial)
        {
            if (objInicial != null)
            {
                string doi = objInicial.PrismDoi;
                return doi;
            }
            else
            {
                string doi = null;
                return doi;
            }
        }


        public string getDoi(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                string doi = objInicial.AbstractsRetrievalResponse.coredata.PrismDoi;
                return doi;
            }
            else
            {
                string doi = null;
                return doi;
            }
        }

        public List<KnowledgeArea> getKnowledgeAreas(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                List<KnowledgeArea> knowledgeAreas = new List<KnowledgeArea>();
                if (objInicial.AbstractsRetrievalResponse.SubjectAreas != null)
                {
                    for (int i = 0; i < objInicial.AbstractsRetrievalResponse.SubjectAreas.SubjectArea.Count; i++)
                    {
                        KnowledgeArea area = new KnowledgeArea();
                        area.abbreviation = objInicial.AbstractsRetrievalResponse.SubjectAreas.SubjectArea[i].Abbrev;
                        area.hasCode = objInicial.AbstractsRetrievalResponse.SubjectAreas.SubjectArea[i].Code;
                        area.name = objInicial.AbstractsRetrievalResponse.SubjectAreas.SubjectArea[i].a;
                        knowledgeAreas.Add(area);
                        //TODO ver si esto esta bien insertado en la ontologia porque lo dudo!!! 
                    }
                    return knowledgeAreas;
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
        public Person getAuthorPrincipal(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                if (objInicial.AbstractsRetrievalResponse.coredata.DcCreator != null)
                {
                    Person authorPrincipal = getPerson(objInicial.AbstractsRetrievalResponse.coredata.DcCreator.author[0]);
                    return authorPrincipal;
                }
                else { return null; }
            }
            else
            {
                return null;
            }
        }
        public List<Person> getAuthors(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                List<Person> authores = new List<Person>();
                if (objInicial.AbstractsRetrievalResponse.authors != null)
                {
                    for (int i = 0; i < objInicial.AbstractsRetrievalResponse.authors.author.Count; i++)
                    {
                        Person persona = getPerson(objInicial.AbstractsRetrievalResponse.authors.author[i]);
                        authores.Add(persona);
                    }
                    return authores;
                }
                else { return null; }
            }
            else
            {
                return null;
            }
        }
        public Journal getJournal(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                Journal journal = new Journal();
                journal.name = objInicial.AbstractsRetrievalResponse.coredata.PrismPublicationName;
                journal.issn = objInicial.AbstractsRetrievalResponse.coredata.PrismIssn;
                // TODO mas info sobre la revista
                return journal;
            }
            else
            {
                return null;
            }
        }

        public Journal getJournal_inicial(Entry objInicial)
        {
            if (objInicial != null)
            {
                Journal journal = new Journal();
                journal.name = objInicial.PrismPublicationName;
                journal.issn = objInicial.PrismIssn;
                // TODO mas info sobre la revista
                return journal;
            }
            else
            {
                return null;
            }
        }
        public DateTimeValue getDate(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                DateTimeValue date = new DateTimeValue();
                date.datimeTime = objInicial.AbstractsRetrievalResponse.coredata.PrismCoverDate;
                return date;
            }
            else
            {
                return null;
            }
        }
        public DateTimeValue getDate_llamada_inicial(Entry objInicial)
        {
            if (objInicial != null)
            {
                DateTimeValue date = new DateTimeValue();
                date.datimeTime = objInicial.PrismCoverDate;
                return date;
            }
            else
            {
                return null;
            }
        }
        public string getLanguage(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                if (objInicial.AbstractsRetrievalResponse.language != null)
                {
                    return objInicial.AbstractsRetrievalResponse.language.XmlLang;
                }
                else { return null; }
            }
            else { return null; }
        }
        public List<String> getFreetextKeyword(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                List<String> freetextKeyword = new List<string>();
                if (objInicial.AbstractsRetrievalResponse.authkeywords != null)
                {
                    try
                    {
                        AuthorKeyword palabraClave = JsonConvert.DeserializeObject<AuthorKeyword>(objInicial.AbstractsRetrievalResponse.authkeywords.AuthorKeyword.ToString());
                        freetextKeyword.Add(palabraClave.a);
                    }
                    catch
                    {
                        JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.AbstractsRetrievalResponse.authkeywords.AuthorKeyword.ToString());
                        foreach (JContainer var in hey)
                        {
                            AuthorKeyword palabraClave = JsonConvert.DeserializeObject<AuthorKeyword>(var.ToString());
                            freetextKeyword.Add(palabraClave.a);
                        }
                    }
                    return freetextKeyword;
                }
                else { return null; }
            }
            else { return null; }
        }
        public PublicationMetric getPublicationMetric(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                PublicationMetric publicationMetric = new PublicationMetric();
                publicationMetric.citationCount = objInicial.AbstractsRetrievalResponse.coredata.CitedbyCount;
                publicationMetric.metricName = "Scopus";
                return publicationMetric;
            }
            else
            {
                PublicationMetric publicationMetric = null;
                return publicationMetric;
            }
        }
        public PublicationMetric getPublicationMetric_llamada_inicial(Entry objInicial)
        {
            if (objInicial != null)
            {
                PublicationMetric publicationMetric = new PublicationMetric();
                publicationMetric.citationCount = objInicial.CitedbyCount;
                publicationMetric.metricName = "Scopus";
                return publicationMetric;
            }
            else
            {
                PublicationMetric publicationMetric = null;
                return publicationMetric;
            }
        }

        public Person getPerson(Author info_person)
        {
            //scopus id ----------------------------------------
            Person author = new Person();
            if (info_person.Auid != "")
            {
                author.identifier = "Scopus-id: " + info_person.Auid;

                // link---------------------------
                List<Url> listAuthorUrl = new List<Url>();
                Url linkScopus = new Url();
                linkScopus.link = "https://www.scopus.com/authid/detail.uri?authorId=" + info_person.Auid;
                //linkScopus.description = "Scopus profile";
                listAuthorUrl.Add(linkScopus);
                author.link = listAuthorUrl;
            }
            else { author.link = null; }

            // names-------------
            List<string> names = new List<string>();
            if (info_person.CeIndexedName != null)
            {
                names.Add(info_person.CeIndexedName);
            }
            if (info_person.PreferredName != null)
            {
                author.surname = info_person.PreferredName.CeSurname;
                names.Add(info_person.PreferredName.CeGivenName);
                names.Add(info_person.PreferredName.CeInitials);
            }
            author.name = names;

            return author;

        }

        /// no se deberian necesitar de modificar en otros modelos!!!!!!

        public Publication Publication_inicial(Publication_root objInicial, Publication publicacion)
        {
            //title--------------------------------------------------------------------
            //links-----------------------------------------------------------------------------
            publicacion.url = getLinks(objInicial);
            //doi-----------------------------------------------------------------------------
            //publicacion.doi = getDoi(objInicial);
            //knowledgeAreas--------------------------------------------------------------------
            publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
            //corresponding author-----------------------------------------------------------
            publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
            // lisOfAuthors-------------------------------------------------------------------
            publicacion.seqOfAuthors = getAuthors(objInicial);
            //journal------------------------------------------------------
            publicacion.hasPublicationVenue = getJournal(objInicial);
            //date--------------------------------------------------------
            //language --------------------------------------------------------
            publicacion.language = getLanguage(objInicial);
            // freetextKeyword ------------------------------------------------------
            publicacion.freetextKeyword = getFreetextKeyword(objInicial);
            //abstract------------------------------------------- 
            publicacion.Abstract = getAbstract(objInicial);
            if (publicacion.pageStart == null)
            {
                publicacion.pageStart = getPageStart(objInicial);
                publicacion.pageEnd = getPageEnd(objInicial);
            }
            // publication metric --------------------------------
            return publicacion;
        }
        public Publication getGenericPublication(Publication_root objInicial)
        {
            Publication publicacion = new Publication();
            //title--------------------------------------------------------------------
            publicacion.title = getTitle(objInicial);
            //links-----------------------------------------------------------------------------
            publicacion.url = getLinks(objInicial);
            //doi-----------------------------------------------------------------------------
            publicacion.doi = getDoi(objInicial);
            //knowledgeAreas--------------------------------------------------------------------
            publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
            //corresponding author-----------------------------------------------------------
            publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
            // lisOfAuthors-------------------------------------------------------------------
            publicacion.seqOfAuthors = getAuthors(objInicial);
            //journal------------------------------------------------------
            publicacion.hasPublicationVenue = getJournal(objInicial);
            //date--------------------------------------------------------
            publicacion.dataIssued = getDate(objInicial);
            //language --------------------------------------------------------
            publicacion.language = getLanguage(objInicial);
            // freetextKeyword ------------------------------------------------------
            publicacion.freetextKeyword = getFreetextKeyword(objInicial);
            //abstract------------------------------------------- 
            publicacion.Abstract = getAbstract(objInicial);
            //page start and page end ------------------------------------------
            publicacion.pageStart = getPageStart(objInicial);
            publicacion.pageEnd = getPageEnd(objInicial);
            // publication metric --------------------------------
            publicacion.hasMetric = getPublicationMetric(objInicial);
            return publicacion;
        }



        public JournalArticle getJournalArticle(Publication_root objInicial, JournalArticle publicacion)
        {
            //JournalArticle publicacion = new JournalArticle();
            //title--------------------------------------------------------------------
            //publicacion.title = getTitle(objInicial);
            //links-----------------------------------------------------------------------------
            publicacion.url = getLinks(objInicial);
            //doi-----------------------------------------------------------------------------
            //publicacion.doi = getDoi(objInicial);
            //knowledgeAreas--------------------------------------------------------------------
            publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
            //corresponding author-----------------------------------------------------------
            publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
            // lisOfAuthors-------------------------------------------------------------------
            publicacion.seqOfAuthors = getAuthors(objInicial);
            //journal------------------------------------------------------
            publicacion.hasPublicationVenue = getJournal(objInicial);
            //date--------------------------------------------------------
            //language --------------------------------------------------------
            publicacion.language = getLanguage(objInicial);
            // freetextKeyword ------------------------------------------------------
            publicacion.freetextKeyword = getFreetextKeyword(objInicial);
            //abstract------------------------------------------- 
            publicacion.Abstract = getAbstract(objInicial);
            //page start and page end ------------------------------------------
            if (publicacion.pageStart == null)
            {
                publicacion.pageStart = getPageStart(objInicial);
                publicacion.pageEnd = getPageEnd(objInicial);
            }
            // publication metric --------------------------------
            publicacion.hasMetric = getPublicationMetric(objInicial);

            return publicacion;

        }

        public ConferencePaper getConferencePaper(Publication_root objInicial, ConferencePaper publicacion)
        {

            //ConferencePaper publicacion = new ConferencePaper();
            //title--------------------------------------------------------------------
            //publicacion.title = getTitle(objInicial);
            //links-----------------------------------------------------------------------------
            publicacion.url = getLinks(objInicial);
            //doi-----------------------------------------------------------------------------
            //publicacion.doi = getDoi(objInicial);
            //knowledgeAreas--------------------------------------------------------------------
            publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
            //corresponding author-----------------------------------------------------------
            publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
            // lisOfAuthors-------------------------------------------------------------------
            publicacion.seqOfAuthors = getAuthors(objInicial);
            //journal------------------------------------------------------
            publicacion.hasPublicationVenue = getJournal(objInicial);
            //date--------------------------------------------------------
            //language --------------------------------------------------------
            publicacion.language = getLanguage(objInicial);
            // freetextKeyword ------------------------------------------------------
            publicacion.freetextKeyword = getFreetextKeyword(objInicial);
            //abstract------------------------------------------- 
            publicacion.Abstract = getAbstract(objInicial);
            //page start and page end ------------------------------------------
            if (publicacion.pageStart == null)
            {
                publicacion.pageStart = getPageStart(objInicial);
                publicacion.pageEnd = getPageEnd(objInicial);
            }
            // publication metric --------------------------------
            return publicacion;
        }
    }

}