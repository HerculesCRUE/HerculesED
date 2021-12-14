using Gnoss.ApiWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using Gnoss.ApiWrapper.ApiModel;
using DocumentOntology;
using PersonOntology;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;

namespace Hercules.ED.ResearcherObjectLoad.Models
{
    public class Carga
    {
        public static ResourceApi mResourceApi;
        public static CommunityApi mCommunityApi;
        public static IConfigurationRoot configuracion;

        #region --- Constantes
        public static string JOURNAL_ARTICLE = "Journal Article";
        public static string BOOK = "Book";
        public static string CHAPTER = "Chapter";
        public static string CONFERENCE_PAPER = "Conference Paper";
        public static string REVISTA_JOURNAL = "Journal";
        public static string REVISTA_BOOK = "Book";
        #endregion

        public static void CargaMain()
        {
            ProcesarFichero(configuracion["DirectorioLectura"], configuracion["DirectorioEscritura"]);
        }

        public static void ProcesarFichero(string pRutaLectura, string pRutaEscritura)
        {
            DirectoryInfo directorio = new DirectoryInfo(pRutaLectura);

            Tuple<Dictionary<string, string>, Dictionary<string, string>> tupla = ObtenerDatosTesauro();

            foreach (var fichero in directorio.GetFiles("*.json"))
            {
                string jsonString = File.ReadAllText(fichero.FullName);
                List<Publication> listaPublicaciones = JsonConvert.DeserializeObject<List<Publication>>(jsonString);

                foreach (Publication publicacion in listaPublicaciones)
                {
                    ProcesarPublicacion(publicacion, tupla.Item1, tupla.Item2);
                    Console.WriteLine($@"{DateTime.Now} Publicación leída.");
                }

                // Hace una copia del fichero y elimina el original.
                CrearZip(pRutaEscritura, fichero.Name, jsonString);
                File.Delete(fichero.FullName);
            }
        }

        private static void ProcesarPublicacion(Publication pPublicacion, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            // Comprueba si está la publicación cargada.
            string idDocumento = ComprobarPublicacion(pPublicacion.doi);
            //TODO recuperar cosas ASIO

            if (string.IsNullOrEmpty(idDocumento))
            {
                CargarDocumento(pPublicacion, pDicAreasBroader, pDicAreasNombre, idDocumento, true, false, false);
            }
            else
            {
                ModificarDocumento(pPublicacion, idDocumento, pDicAreasBroader, pDicAreasNombre);
            }
        }

        /// <summary>
        /// Permite cargar un documento que no esté ya cargado.
        /// </summary>
        /// <param name="pPublicacion">Publicación con los datos a cargar.</param>
        /// <param name="pDicAreasBroader">Diccionario con los hijos.</param>
        /// <param name="pDicAreasNombre">Diccionario con las áreas temáticas.</param>
        /// <param name="pIdDocumento">ID del recurso.</param>
        /// <param name="pPubPrimaria">True si es una publicación primaria.</param>
        /// <param name="pPubSecundariaBiblio">True si es una publicación secundaria de bibliografía.</param>
        /// <param name="pPubSecundariaCita">True si es una publicación secundaria de citas.</param>
        /// <param name="pIdPadre">ID del recurso padre.</param>
        /// <param name="pIdCargarCita">ID de la cita a cargar.</param>
        /// <param name="pTuplaDatosRecuperados">Tupla con los datos recuperados.</param>
        /// <returns></returns>
        public static string CargarDocumento(Publication pPublicacion, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre, string pIdDocumento, bool pPubPrimaria, bool pPubSecundariaBiblio, bool pPubSecundariaCita, string pIdPadre = null, string pIdCargarCita = null, Tuple<string, List<string>, List<string>, Dictionary<string, List<string>>, Dictionary<string, List<string>>, string, List<string>, Tuple<string>> pTuplaDatosRecuperados = null)
        {
            // Creación del objeto con los datos.
            Document documentoCargar = new Document();

            // Diccionario para guardar los ORCID - ID_RECURSO de las personas de la publicación.
            Dictionary<string, string> dicOrcidRecurso = new Dictionary<string, string>();

            // Si no tiene título, no se carga la publicación.
            if (!string.IsNullOrEmpty(pPublicacion.title))
            {
                // ID DOI (Doi)
                if (!string.IsNullOrEmpty(pPublicacion.doi))
                {
                    documentoCargar.Bibo_doi = pPublicacion.doi;
                }

                // Identificadores
                if (pPublicacion.iDs != null && pPublicacion.iDs.Count > 0)
                {
                    documentoCargar.Bibo_identifier = new List<fDocument>();

                    foreach (string id in pPublicacion.iDs)
                    {
                        if (id.Contains(":"))
                        {
                            if (id.ToLower().Contains("wos"))
                            {
                                fDocument fDocumento = new fDocument();
                                fDocumento.Foaf_topic = "WoS";
                                fDocumento.Dc_title = id.Split(":")[1].Trim();
                                documentoCargar.Bibo_identifier.Add(fDocumento);
                            }

                            if (id.ToLower().Contains("semanticscholar"))
                            {
                                fDocument fDocumento = new fDocument();
                                fDocumento.Foaf_topic = "SemanticScholar";
                                fDocumento.Dc_title = id.Split(":")[1].Trim();
                                documentoCargar.Bibo_identifier.Add(fDocumento);
                            }

                            if (id.ToLower().Contains("mag"))
                            {
                                fDocument fDocumento = new fDocument();
                                fDocumento.Foaf_topic = "MAG";
                                fDocumento.Dc_title = id.Split(":")[1].Trim();
                                documentoCargar.Bibo_identifier.Add(fDocumento);
                            }

                            if (id.ToLower().Contains("pubmedcentral"))
                            {
                                fDocument fDocumento = new fDocument();
                                fDocumento.Foaf_topic = "PubMedCentral";
                                fDocumento.Dc_title = id.Split(":")[1].Trim();
                                documentoCargar.Bibo_identifier.Add(fDocumento);
                            }
                        }
                    }
                }

                // Título (Title)
                documentoCargar.Roh_title = pPublicacion.title;

                // Descripción (Abstract)
                if (!string.IsNullOrEmpty(pPublicacion.@abstract))
                {
                    documentoCargar.Bibo_abstract = pPublicacion.@abstract;
                }

                // Tipo de publicación (TypeOfPublication)
                if (pPublicacion.typeOfPublication != null)
                {
                    if (pPublicacion.typeOfPublication == JOURNAL_ARTICLE)
                    {
                        documentoCargar.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                        documentoCargar.IdDc_type = "http://gnoss.com/items/publicationtype_020";
                    }
                    else if (pPublicacion.typeOfPublication == BOOK)
                    {
                        documentoCargar.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                        documentoCargar.IdDc_type = "http://gnoss.com/items/publicationtype_032";
                    }
                    else if (pPublicacion.typeOfPublication == CHAPTER)
                    {
                        documentoCargar.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                        documentoCargar.IdDc_type = "http://gnoss.com/items/publicationtype_004";
                    }
                    else if (pPublicacion.typeOfPublication == CONFERENCE_PAPER)
                    {
                        documentoCargar.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD2";
                    }
                    else
                    {
                        documentoCargar.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                        documentoCargar.IdDc_type = "http://gnoss.com/items/publicationtype_OTHERS";
                        documentoCargar.Roh_typeOthers = "Otros";
                    }
                }

                // Lista sin repetición para las etiquetas.
                HashSet<string> listaSinRepetirEtiquetas = new HashSet<string>();

                // Etiquetas Externas (ExternalFreeTextKeywords)                
                if (pPublicacion.freetextKeywords != null && pPublicacion.freetextKeywords.Count > 0)
                {
                    HashSet<string> etiquetasExternas = new HashSet<string>();
                    foreach (FreetextKeyword etiquetas in pPublicacion.freetextKeywords)
                    {
                        foreach (string tag in etiquetas.freetextKeyword)
                        {
                            if (!listaSinRepetirEtiquetas.Contains(tag.ToLower()))
                            {
                                etiquetasExternas.Add(tag);
                                listaSinRepetirEtiquetas.Add(tag.ToLower());
                            }
                        }
                    }
                    documentoCargar.Roh_externalKeywords = etiquetasExternas.ToList();
                }

                // Etiquetas Enriquecidas (EnrichedFreeTextKeywords)                
                if (pPublicacion.freetextKeyword_enriquecidas != null && pPublicacion.freetextKeyword_enriquecidas.Count > 0)
                {
                    HashSet<string> etiquetasEnriquecidas = new HashSet<string>();
                    foreach (FreetextKeywordEnriquecida tag in pPublicacion.freetextKeyword_enriquecidas)
                    {
                        if (!listaSinRepetirEtiquetas.Contains(tag.word.ToLower()))
                        {
                            etiquetasEnriquecidas.Add(tag.word);
                            listaSinRepetirEtiquetas.Add(tag.word.ToLower());
                        }
                    }
                    documentoCargar.Roh_enrichedKeywords = etiquetasEnriquecidas.ToList();
                }

                // Etiquetas del usuario (UserKeywords)
                if (pTuplaDatosRecuperados != null)
                {
                    documentoCargar.Roh_userKeywords = pTuplaDatosRecuperados.Item2;
                }

                // Etiquetas propuestas al usuario (SuggestedKeywords)
                if (pTuplaDatosRecuperados != null)
                {
                    documentoCargar.Roh_suggestedKeywords = pTuplaDatosRecuperados.Item3;
                }

                // Fecha de publicación (Issued)
                if (pPublicacion.dataIssued != null && pPublicacion.dataIssued.datimeTime != null)
                {
                    documentoCargar.Dct_issued = pPublicacion.dataIssued.datimeTime;
                }

                // URL (Url)
                if (pPublicacion.url != null && pPublicacion.url.Count() > 0)
                {
                    documentoCargar.Vcard_url = new List<string>();
                    foreach (string url in pPublicacion.url)
                    {
                        documentoCargar.Vcard_url.Add(url);
                    }
                }

                // Página de inicio (PageStart)
                if (!string.IsNullOrEmpty(pPublicacion.pageStart) && Int32.TryParse(pPublicacion.pageEnd, out int n1))
                {
                    documentoCargar.Bibo_pageStart = Int32.Parse(pPublicacion.pageStart);
                }

                // Página de fin (PageEnd)
                if (!string.IsNullOrEmpty(pPublicacion.pageEnd) && Int32.TryParse(pPublicacion.pageEnd, out int n2))
                {
                    documentoCargar.Bibo_pageEnd = Int32.Parse(pPublicacion.pageEnd);
                }

                // Autor de correspondencia (CorrespondingAuthor)                
                if (pPublicacion.correspondingAuthor != null)
                {
                    documentoCargar.IdsRoh_correspondingAuthor = new List<string>();
                    string idPersona = ComprobarPersona(LimpiarORCID(pPublicacion.correspondingAuthor.orcid));
                    if (string.IsNullOrEmpty(idPersona))
                    {
                        Person persona = ConstruirPersona(pPublicacion.correspondingAuthor.name.nombre_completo, pPublicacion.correspondingAuthor.name.given, pPublicacion.correspondingAuthor.name.familia);

                        // ID ORCID (Orcid)
                        string orcidLimpio = pPublicacion.correspondingAuthor.orcid;
                        if (!string.IsNullOrEmpty(pPublicacion.correspondingAuthor.orcid))
                        {
                            orcidLimpio = LimpiarORCID(pPublicacion.correspondingAuthor.orcid);

                            if (dicOrcidRecurso.ContainsKey(orcidLimpio))
                            {
                                idPersona = dicOrcidRecurso[orcidLimpio];
                            }
                        }

                        // Identificadores
                        if (pPublicacion.correspondingAuthor.iDs != null && pPublicacion.correspondingAuthor.iDs.Count > 0)
                        {
                            foreach (string id in pPublicacion.correspondingAuthor.iDs)
                            {
                                if (id.Contains(":"))
                                {
                                    if (id.ToLower().Contains("semanticscholar"))
                                    {
                                        persona.Roh_semanticScholarId = id.Split(":")[1].Trim();
                                    }
                                }
                            }
                        }

                        // TODO: Hacer comprobación a parte del nombre...
                        if (string.IsNullOrEmpty(idPersona) && !string.IsNullOrEmpty(persona.Foaf_name))
                        {
                            idPersona = ComprobarPersonaNombre(persona.Foaf_name);
                        }

                        // Carga
                        if (string.IsNullOrEmpty(idPersona) && !string.IsNullOrEmpty(persona.Foaf_name))
                        {
                            mResourceApi.ChangeOntoly("person");
                            ComplexOntologyResource resourcePersona = persona.ToGnossApiResource(mResourceApi, null);
                            //mResourceApi.LoadComplexSemanticResource(resourcePersona, false, true);
                            idPersona = resourcePersona.GnossId;
                        }

                        if (!string.IsNullOrEmpty(orcidLimpio) && !dicOrcidRecurso.ContainsKey(orcidLimpio))
                        {
                            // Guardo la persona con su identificador.
                            dicOrcidRecurso.Add(orcidLimpio, idPersona);
                        }
                    }
                    else if (!string.IsNullOrEmpty(LimpiarORCID(pPublicacion.correspondingAuthor.orcid)) && !dicOrcidRecurso.ContainsKey(LimpiarORCID(pPublicacion.correspondingAuthor.orcid)))
                    {
                        // Guardo la persona con su identificador.
                        dicOrcidRecurso.Add(LimpiarORCID(pPublicacion.correspondingAuthor.orcid), idPersona);
                    }

                    if (!string.IsNullOrEmpty(idPersona))
                    {
                        documentoCargar.IdsRoh_correspondingAuthor.Add(idPersona);
                    }
                }

                // Lista de autores (AuthorList)
                if (pPublicacion.seqOfAuthors != null)
                {
                    documentoCargar.Bibo_authorList = new List<BFO_0000023>();

                    // Orden de los autores.
                    int orden = 1;

                    foreach (SeqOfAuthor itemAutor in pPublicacion.seqOfAuthors)
                    {
                        BFO_0000023 relacionPersona = new BFO_0000023();
                        string idPersona = ComprobarPersona(LimpiarORCID(itemAutor.orcid));

                        if (string.IsNullOrEmpty(idPersona))
                        {
                            Person persona = ConstruirPersona(itemAutor.name.nombre_completo, itemAutor.name.given, itemAutor.name.familia);

                            // ID ORCID (Orcid)
                            string orcidLimpio = itemAutor.orcid;
                            if (!string.IsNullOrEmpty(itemAutor.orcid))
                            {
                                orcidLimpio = LimpiarORCID(itemAutor.orcid);

                                if (dicOrcidRecurso.ContainsKey(orcidLimpio))
                                {
                                    idPersona = dicOrcidRecurso[orcidLimpio];
                                }
                            }

                            // Identificadores
                            if (itemAutor.iDs != null && itemAutor.iDs.Count > 0)
                            {
                                foreach (string id in itemAutor.iDs)
                                {
                                    if (id.Contains(":"))
                                    {
                                        if (id.ToLower().Contains("semanticscholar"))
                                        {
                                            persona.Roh_semanticScholarId = id.Split(":")[1].Trim();
                                        }
                                    }
                                }
                            }

                            // TODO: Hacer comprobación a parte del nombre...
                            if (string.IsNullOrEmpty(idPersona) && !string.IsNullOrEmpty(persona.Foaf_name))
                            {
                                idPersona = ComprobarPersonaNombre(persona.Foaf_name);
                            }

                            // Carga
                            if (string.IsNullOrEmpty(idPersona) && !string.IsNullOrEmpty(persona.Foaf_name))
                            {
                                mResourceApi.ChangeOntoly("person");
                                ComplexOntologyResource resourcePersona = persona.ToGnossApiResource(mResourceApi, null);
                                //mResourceApi.LoadComplexSemanticResource(resourcePersona, false, true);
                                idPersona = resourcePersona.GnossId;
                            }

                            if (!string.IsNullOrEmpty(orcidLimpio) && !dicOrcidRecurso.ContainsKey(orcidLimpio) && !string.IsNullOrEmpty(persona.Foaf_name))
                            {
                                dicOrcidRecurso.Add(orcidLimpio, idPersona);
                            }
                        }
                        else if (!string.IsNullOrEmpty(LimpiarORCID(itemAutor.orcid)) && !dicOrcidRecurso.ContainsKey(LimpiarORCID(itemAutor.orcid)))
                        {
                            dicOrcidRecurso.Add(LimpiarORCID(itemAutor.orcid), idPersona);
                        }

                        // Agrego la relación con BFO_0000023.
                        if (!string.IsNullOrEmpty(idPersona))
                        {
                            relacionPersona.IdRdf_member = idPersona;

                            // Firma (Nick)
                            if (itemAutor.name != null)
                            {
                                if (itemAutor.name.nombre_completo != null && itemAutor.name.nombre_completo.Count > 0)
                                {
                                    relacionPersona.Foaf_nick = itemAutor.name.nombre_completo[0];
                                }
                                else if (itemAutor.name.given != null && itemAutor.name.given.Count > 0 && itemAutor.name.familia != null && itemAutor.name.familia.Count > 0 && !string.IsNullOrEmpty(itemAutor.name.given[0]) && !string.IsNullOrEmpty(itemAutor.name.familia[0]))
                                {
                                    relacionPersona.Foaf_nick = itemAutor.name.given[0] + " " + itemAutor.name.familia[0];
                                }
                            }

                            // Orden (Comment)
                            relacionPersona.Rdf_comment = orden;

                            documentoCargar.Bibo_authorList.Add(relacionPersona);
                            orden++;
                        }
                    }
                }

                // Áreas de conocimiento externas (ExternalKnowledgeArea)                
                HashSet<string> listaIDs = new HashSet<string>();
                if (pPublicacion.hasKnowledgeAreas != null && pPublicacion.hasKnowledgeAreas.Count > 0)
                {
                    documentoCargar.Roh_externalKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                    foreach (HasKnowledgeArea knowledgearea in pPublicacion.hasKnowledgeAreas)
                    {
                        if (knowledgearea.resource.ToLower() == "wos")
                        {
                            foreach (KnowledgeArea area in knowledgearea.knowledgeArea)
                            {
                                if (pDicAreasNombre.ContainsKey(area.name.ToLower()) && !string.IsNullOrEmpty(area.hasCode))
                                {
                                    DocumentOntology.CategoryPath categoria = new DocumentOntology.CategoryPath();
                                    categoria.IdsRoh_categoryNode = new List<string>();
                                    categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.name.ToLower()]);
                                    string idHijo = pDicAreasNombre[area.name.ToLower()];
                                    string idHijoAux = idHijo;
                                    if (!listaIDs.Contains(idHijo))
                                    {
                                        while (!idHijo.EndsWith(".0.0.0"))
                                        {
                                            categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                            idHijo = pDicAreasBroader[idHijo];
                                        }
                                        if (categoria.IdsRoh_categoryNode.Count > 0)
                                        {
                                            documentoCargar.Roh_externalKnowledgeArea.Add(categoria);
                                        }
                                    }
                                    listaIDs.Add(idHijoAux);
                                }
                            }
                        }
                    }
                }

                // Áreas de conocimiento enriquecidas (EnrichedKnowledgeArea)
                if (pPublicacion.topics_enriquecidos != null && pPublicacion.topics_enriquecidos.Count > 0)
                {
                    documentoCargar.Roh_enrichedKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                    foreach (TopicsEnriquecido area in pPublicacion.topics_enriquecidos)
                    {
                        if (pDicAreasNombre.ContainsKey(area.word.ToLower()))
                        {
                            DocumentOntology.CategoryPath categoria = new DocumentOntology.CategoryPath();
                            categoria.IdsRoh_categoryNode = new List<string>();
                            categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.word.ToLower()]);
                            string idHijo = pDicAreasNombre[area.word.ToLower()];
                            string idHijoAux = idHijo;
                            if (!listaIDs.Contains(idHijo))
                            {
                                while (!idHijo.EndsWith(".0.0.0"))
                                {
                                    categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                    idHijo = pDicAreasBroader[idHijo];
                                }
                                if (categoria.IdsRoh_categoryNode.Count > 0)
                                {
                                    documentoCargar.Roh_enrichedKnowledgeArea.Add(categoria);
                                }
                            }
                            listaIDs.Add(idHijoAux);
                        }
                    }
                }

                // Áreas de conocimiento del usuario (UserKnowledgeArea) 
                if (pTuplaDatosRecuperados != null)
                {
                    documentoCargar.Roh_userKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                    foreach (string aux in pTuplaDatosRecuperados.Item4.Keys)
                    {
                        DocumentOntology.CategoryPath categoria = new DocumentOntology.CategoryPath();
                        categoria.IdsRoh_categoryNode = new List<string>();
                        foreach (string node in pTuplaDatosRecuperados.Item4[aux])
                        {
                            categoria.IdsRoh_categoryNode.Add(node);
                        }
                        documentoCargar.Roh_userKnowledgeArea.Add(categoria);
                    }
                }

                // Áreas de conocimiento sugeridas (SuggestedKnowledgeArea) 
                if (pTuplaDatosRecuperados != null)
                {
                    documentoCargar.Roh_userKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                    foreach (string aux in pTuplaDatosRecuperados.Item5.Keys)
                    {
                        DocumentOntology.CategoryPath categoria = new DocumentOntology.CategoryPath();
                        categoria.IdsRoh_categoryNode = new List<string>();
                        foreach (string node in pTuplaDatosRecuperados.Item5[aux])
                        {
                            categoria.IdsRoh_categoryNode.Add(node);
                        }
                        documentoCargar.Roh_suggestedKnowledgeArea.Add(categoria);
                    }
                }

                // Métricas (HasMetric)
                if (pPublicacion.hasMetric != null && pPublicacion.hasMetric.Count > 0)
                {
                    foreach (HasMetric itemMetric in pPublicacion.hasMetric)
                    {
                        if (itemMetric.metricName.ToLower() == "wos")
                        {
                            documentoCargar.Roh_wos = Int32.Parse(itemMetric.citationCount);
                        }
                        else
                        {
                            PublicationMetric publicationMetric = new PublicationMetric();
                            publicationMetric.Roh_metricName = itemMetric.metricName;
                            publicationMetric.Roh_citationCount = Int32.Parse(itemMetric.citationCount);
                            documentoCargar.Roh_hasMetric = new List<PublicationMetric>() { publicationMetric };
                        }
                    }
                }

                // Revista (HasPublicationVenue)
                if (pPublicacion.hasPublicationVenue != null && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name))
                {
                    // Comprobar si la revista existe o no.
                    string idRevista = string.Empty;

                    #region --- Comprobar ISSN  
                    string issnObtenido = string.Empty;
                    if (pPublicacion.hasPublicationVenue.issn != null)
                    {
                        // Comparo los elementos de la lista, y me quedo con el que mayor número tenga.
                        if (pPublicacion.hasPublicationVenue.issn.Count() > 1)
                        {
                            issnObtenido = pPublicacion.hasPublicationVenue.issn.Max();
                        }
                        else
                        {
                            issnObtenido = pPublicacion.hasPublicationVenue.issn[0];
                        }

                        idRevista = ComprobarRevistaISSN(issnObtenido);
                    }
                    #endregion

                    #region --- Comprobar ISBN
                    string isbnObtenido = string.Empty;
                    if (pPublicacion.hasPublicationVenue.isbn != null)
                    {
                        // Comparo los elementos de la lista, y me quedo con el que mayor número tenga.
                        if (pPublicacion.hasPublicationVenue.isbn.Count() > 1)
                        {
                            int num = 0;
                            foreach (string itemIsbn in pPublicacion.hasPublicationVenue.issn)
                            {
                                if (Int32.TryParse(itemIsbn, out int n3) && Int32.Parse(itemIsbn) > num)
                                {
                                    num = Int32.Parse(itemIsbn);
                                    isbnObtenido = itemIsbn;
                                }
                                else
                                {
                                    isbnObtenido = itemIsbn;
                                }
                            }
                        }
                        else
                        {
                            isbnObtenido = pPublicacion.hasPublicationVenue.isbn[0];
                        }

                        if (string.IsNullOrEmpty(idRevista))
                        {
                            idRevista = ComprobarRevistaISBN(isbnObtenido);
                        }
                    }
                    #endregion

                    #region --- Comprobar Titulo
                    if (string.IsNullOrEmpty(idRevista) && pPublicacion.hasPublicationVenue.name != null)
                    {
                        idRevista = ComprobarRevistaTitulo(pPublicacion.hasPublicationVenue.name);
                    }
                    #endregion

                    bool revistaNueva = true;
                    if (!string.IsNullOrEmpty(idRevista))
                    {
                        revistaNueva = false;
                    }

                    // Creación del contenedor.
                    MaindocumentOntology.MainDocument revistaCargar = new MaindocumentOntology.MainDocument();
                    revistaCargar.Roh_title = pPublicacion.hasPublicationVenue.name;
                    revistaCargar.Bibo_issn = issnObtenido;
                    revistaCargar.Bibo_isbn = isbnObtenido;
                    if (!string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.eissn))
                    {
                        revistaCargar.Bibo_eissn = pPublicacion.hasPublicationVenue.eissn;
                    }
                    if (!string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.type))
                    {
                        if (pPublicacion.hasPublicationVenue.type == REVISTA_JOURNAL)
                        {
                            revistaCargar.IdRoh_format = "http://gnoss.com/items/documentformat_057";
                        }
                        else if (pPublicacion.hasPublicationVenue.type == REVISTA_BOOK)
                        {
                            revistaCargar.IdRoh_format = "http://gnoss.com/items/documentformat_032";
                        }
                    }
                    if (pPublicacion.hasPublicationVenue.hasMetric != null && pPublicacion.hasPublicationVenue.hasMetric.Count > 0)
                    {
                        revistaCargar.Roh_impactIndex = new List<MaindocumentOntology.ImpactIndex>();
                        foreach (HasMetric metrica in pPublicacion.hasPublicationVenue.hasMetric)
                        {
                            MaindocumentOntology.ImpactIndex impacto = new MaindocumentOntology.ImpactIndex();
                            if (!string.IsNullOrEmpty(metrica.quartile) && metrica.quartile.ToLower() == "q1")
                            {
                                impacto.Roh_journalTop25 = true;
                            }
                            if (!string.IsNullOrEmpty(metrica.ranking) && metrica.ranking.Contains("/"))
                            {
                                impacto.Roh_publicationPosition = Int32.Parse(metrica.ranking.Split('/')[0]);
                            }
                            if (!string.IsNullOrEmpty(metrica.impactFactorName))
                            {
                                impacto.Roh_impactSourceOther = metrica.impactFactorName;
                            }
                            impacto.Roh_impactIndexInYear = (float)metrica.impactFactor;
                            revistaCargar.Roh_impactIndex.Add(impacto);
                        }
                    }

                    // Carga de la revista.
                    mResourceApi.ChangeOntoly("maindocument");
                    if (revistaNueva)
                    {
                        ComplexOntologyResource resourceRevista = revistaCargar.ToGnossApiResource(mResourceApi, null);
                        //mResourceApi.LoadComplexSemanticResource(resourceRevista, false, true);
                        idRevista = resourceRevista.GnossId;
                    }
                    else
                    {
                        Guid gnossId = mResourceApi.GetShortGuid(idRevista);
                        Guid articleId = new Guid(idRevista.Split('_')[2]);
                        ComplexOntologyResource resourceRevista = revistaCargar.ToGnossApiResource(mResourceApi, null, gnossId, articleId);
                        //mResourceApi.ModifyComplexOntologyResource(resourceRevista, false, true);
                        idRevista = resourceRevista.GnossId;
                    }
                    documentoCargar.IdVivo_hasPublicationVenue = idRevista;
                }

                // Bibliografia (Cites)
                documentoCargar.IdsBibo_cites = new List<string>();
                if (pPublicacion.bibliografia != null)
                {
                    foreach (Publication pub in pPublicacion.bibliografia)
                    {
                        if (string.IsNullOrEmpty(pub.title))
                        {
                            continue;
                        }

                        // Comprobar si el documento existe o no.
                        string idDocumentoAux = ComprobarPublicacion(pub.doi);
                        if (string.IsNullOrEmpty(idDocumentoAux))
                        {
                            idDocumentoAux = ComprobarPublicacionTitulo(pub.title);
                        }

                        string idDocumentoCargado = CargarDocumento(pub, pDicAreasBroader, pDicAreasNombre, idDocumentoAux, false, true, false);
                        documentoCargar.IdsBibo_cites.Add(idDocumentoCargado);
                    }
                }

                // Citas (Cites)
                string idDocumentoActual = pIdDocumento;
                if (string.IsNullOrEmpty(idDocumentoActual))
                {
                    idDocumentoActual = $"http://gnoss.com/items/Document_{Guid.NewGuid().ToString().ToLower()}_{Guid.NewGuid().ToString().ToLower()}";
                }

                if (pPublicacion.citas != null)
                {
                    foreach (Publication item in pPublicacion.citas)
                    {
                        // Comprobar si el documento existe o no.
                        string idDocumentoAux = ComprobarPublicacion(item.doi);
                        if (string.IsNullOrEmpty(idDocumentoAux))
                        {
                            idDocumentoAux = ComprobarPublicacionTitulo(item.title);
                        }
                        CargarDocumento(item, pDicAreasBroader, pDicAreasNombre, idDocumentoAux, false, false, true, pIdDocumento, idDocumentoActual);
                    }
                }

                // Identificador interno (CrisIdentifier)
                if (pTuplaDatosRecuperados != null)
                {
                    documentoCargar.Roh_crisIdentifier = pTuplaDatosRecuperados.Item1;
                }
                else
                {
                    documentoCargar.Roh_crisIdentifier = ObtenerCrisIdentifierPublicacion(idDocumentoActual);
                }

                // Proyecto asignado (Project)
                if (pTuplaDatosRecuperados != null)
                {
                    documentoCargar.IdRoh_project = pTuplaDatosRecuperados.Item6;
                }

                // Grupos (isProducedBy)
                if (pTuplaDatosRecuperados != null)
                {
                    documentoCargar.IdsRoh_isProducedBy = pTuplaDatosRecuperados.Item7;
                }

                // Estado de validación (AssessmentStatus)
                if (pTuplaDatosRecuperados != null)
                {
                    documentoCargar.Roh_assessmentStatus = pTuplaDatosRecuperados.Rest.Item1;
                }

                // Estado de validación (IsValidated)
                if (pPubPrimaria)
                {
                    documentoCargar.Roh_isValidated = true;
                }

                // Carga de la publicación.
                mResourceApi.ChangeOntoly("document");
                if (pPubPrimaria) // Si es una publicación primaria
                {
                    if (pTuplaDatosRecuperados != null) // Si hay datos obtenidos de la recuperación...
                    {
                        Guid gnossIdPrimaria = mResourceApi.GetShortGuid(idDocumentoActual);
                        Guid articleIdPrimaria = new Guid(idDocumentoActual.Split('_')[2]);
                        ComplexOntologyResource resourceDocumentoPrimaria = documentoCargar.ToGnossApiResource(mResourceApi, null, gnossIdPrimaria, articleIdPrimaria);
                        //mResourceApi.ModifyComplexOntologyResource(resourceDocumentoPrimaria, false, true);
                    }
                    else
                    {
                        Guid gnossIdPrimaria = mResourceApi.GetShortGuid(idDocumentoActual);
                        Guid articleIdPrimaria = new Guid(idDocumentoActual.Split('_')[2]);
                        ComplexOntologyResource resourceDocumentoPrimaria = documentoCargar.ToGnossApiResource(mResourceApi, null, gnossIdPrimaria, articleIdPrimaria);
                        //mResourceApi.LoadComplexSemanticResource(resourceDocumentoPrimaria, false, true);
                    }
                }
                else if (pPubSecundariaBiblio) // Si es una publicación secundaria de bibliografía
                {
                    if (string.IsNullOrEmpty(idDocumentoActual)) // Si viene vacío...
                    {
                        Guid gnossIdSecundario = mResourceApi.GetShortGuid(idDocumentoActual);
                        Guid articleIdSecundario = new Guid(idDocumentoActual.Split('_')[2]);
                        ComplexOntologyResource resourceDocumento = documentoCargar.ToGnossApiResource(mResourceApi, null, gnossIdSecundario, articleIdSecundario);
                        //mResourceApi.LoadComplexSemanticResource(resourceDocumento, false, true);
                    }
                }
                else if (pPubSecundariaCita) // Si es una publicación secundaria de cita
                {
                    if (!string.IsNullOrEmpty(idDocumentoActual)) // Si NO viene vacío...
                    {
                        Dictionary<Guid, List<TriplesToInclude>> triples = new Dictionary<Guid, List<TriplesToInclude>>();
                        triples.Add(mResourceApi.GetShortGuid(idDocumentoActual), new List<TriplesToInclude>() {
                            new TriplesToInclude(){
                                    Predicate = "http://purl.org/ontology/bibo/cites",
                                    NewValue = pIdPadre
                                }
                            });
                        //mResourceApi.InsertPropertiesLoadedResources(triples);
                    }
                    else
                    {
                        Guid gnossId = mResourceApi.GetShortGuid(idDocumentoActual);
                        Guid articleId = new Guid(idDocumentoActual.Split('_')[2]);
                        ComplexOntologyResource resourceDocumento = documentoCargar.ToGnossApiResource(mResourceApi, null, gnossId, articleId);
                        //string idCreado = mResourceApi.LoadComplexSemanticResource(resourceDocumento, false, true);

                        //Dictionary<Guid, List<TriplesToInclude>> triples = new Dictionary<Guid, List<TriplesToInclude>>();
                        //triples.Add(mResourceApi.GetShortGuid(idCreado), new List<TriplesToInclude>() {
                        //    new TriplesToInclude(){
                        //            Predicate = "http://purl.org/ontology/bibo/cites",
                        //            NewValue = pIdPadre
                        //        }
                        //    });
                        //mResourceApi.InsertPropertiesLoadedResources(triples);
                    }
                }

                return idDocumentoActual;
            }

            return string.Empty;
        }

        /// <summary>
        /// Permite borrar y cargar un documento existente con datos actualizados.
        /// </summary>
        /// <param name="pPublicacion">Publicación con los datos a cargar.</param>
        /// <param name="pIdDocumento">ID del recurso.</param>
        /// <param name="pDicAreasBroader">Diccionario con los hijos.</param>
        /// <param name="pDicAreasNombre">Diccionario con las áreas temáticas.</param>        
        /// <returns></returns>
        public static void ModificarDocumento(Publication pPublicacion, string pIdDocumento, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            // Recuperación del Crisidentifier
            string crisIdentifier = ObtenerCrisIdentifierPublicacion(pIdDocumento);

            // Recuperación de las UserKeywords
            List<string> userKeywords = ObtenerUserKeywordsPublicacion(pIdDocumento);

            // Recuperación de las SuggestedKeywords
            List<string> suggestedKeywords = ObtenerSuggestedKeywordsPublicacion(pIdDocumento);

            // Recuperación de las UserKnowledgeArea
            Dictionary<string, List<string>> userKnowledgeArea = ObtenerUserKnowledgeAreaPublicacion(pIdDocumento);

            // Recuperación de las SuggestedKnowledgeArea
            Dictionary<string, List<string>> suggestedKnowledgeArea = ObtenerSuggestedKnowledgeAreaPublicacion(pIdDocumento);

            // Recuperación del Project
            string project = ObtenerProjectPublicacion(pIdDocumento);

            // Recuperación del isProducedBy 
            List<string> isProducedBy = ObtenerIsProducedByPublicacion(pIdDocumento);

            // Recuperación del AssessmentStatus
            Tuple<string> assessmentStatus = new Tuple<string>(ObtenerAssessmentStatusPublicacion(pIdDocumento));

            // Creación de la tupla con los datos almacenados.
            Tuple<string, List<string>, List<string>, Dictionary<string, List<string>>, Dictionary<string, List<string>>, string, List<string>, Tuple<string>> tuplaDatos = new(
                    crisIdentifier,
                    userKeywords,
                    suggestedKeywords,
                    userKnowledgeArea,
                    suggestedKnowledgeArea,
                    project,
                    isProducedBy,
                    assessmentStatus
                );

            CargarDocumento(pPublicacion, pDicAreasBroader, pDicAreasNombre, pIdDocumento, true, false, false, pTuplaDatosRecuperados: tuplaDatos);
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con el crisidentifier.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns>ID del recurso.</returns>
        public static string ObtenerCrisIdentifierPublicacion(string pId)
        {
            // Consulta sparql.
            string select = "SELECT ?crisIdentifier";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/crisIdentifier> ?crisIdentifier
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["crisIdentifier"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con las userkeywords.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns>ID del recurso.</returns>
        public static List<string> ObtenerUserKeywordsPublicacion(string pId)
        {
            List<string> listaEtiquetas = new List<string>();

            // Consulta sparql.
            string select = "SELECT ?userKeywords";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/userKeywords> ?userKeywords
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    listaEtiquetas.Add(fila["userKeywords"].value);
                }
            }

            return listaEtiquetas;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con las suggestedkeywords.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns>ID del recurso.</returns>
        public static List<string> ObtenerSuggestedKeywordsPublicacion(string pId)
        {
            List<string> listaEtiquetas = new List<string>();

            // Consulta sparql.
            string select = "SELECT ?suggestedKeywords";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/suggestedKeywords> ?suggestedKeywords
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    listaEtiquetas.Add(fila["suggestedKeywords"].value);
                }
            }

            return listaEtiquetas;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con las userknowledgearea.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> ObtenerUserKnowledgeAreaPublicacion(string pId)
        {
            Dictionary<string, List<string>> listaCategorias = new Dictionary<string, List<string>>();

            string select = "SELECT * ";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/userKnowledgeArea> ?userAreas.
                                ?userAreas <http://w3id.org/roh/categoryNode> ?nodo.
                            }}";
            SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");


            foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            {
                string userAreas = fila["userAreas"].value;
                string nodo = fila["nodo"].value;
                if (!listaCategorias.ContainsKey(userAreas))
                {
                    listaCategorias.Add(userAreas, new List<string>());
                }
                listaCategorias[userAreas].Add(nodo);
            }

            return listaCategorias;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con las suggestedknowledgearea.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> ObtenerSuggestedKnowledgeAreaPublicacion(string pId)
        {
            Dictionary<string, List<string>> listaCategorias = new Dictionary<string, List<string>>();

            string select = "SELECT * ";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/suggestedKnowledgeArea> ?userAreas.
                                ?userAreas <http://w3id.org/roh/categoryNode> ?nodo.
                            }}";
            SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

            foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            {
                string userAreas = fila["userAreas"].value;
                string nodo = fila["nodo"].value;
                if (!listaCategorias.ContainsKey(userAreas))
                {
                    listaCategorias.Add(userAreas, new List<string>());
                }
                listaCategorias[userAreas].Add(nodo);
            }

            return listaCategorias;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con el proyecto asignado.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static string ObtenerProjectPublicacion(string pId)
        {
            // Consulta sparql.
            string select = "SELECT ?project";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/project> ?project
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["project"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con los grupos.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static List<string> ObtenerIsProducedByPublicacion(string pId)
        {
            List<string> listaGrupos = new List<string>();

            // Consulta sparql.
            string select = "SELECT ?grupos";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/isProducedBy> ?grupos
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    listaGrupos.Add(fila["grupos"].value);
                }
            }

            return listaGrupos;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con el estado.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static string ObtenerAssessmentStatusPublicacion(string pId)
        {
            // Consulta sparql.
            string select = "SELECT ?estado";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/assessmentStatus> ?estado
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["estado"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Comprueba en SPARQL si existe la revista con el ISSN.
        /// </summary>
        /// <param name="pISSN">ISSN a consultar.</param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarRevistaISSN(string pISSN)
        {
            // Consulta sparql.
            string select = "SELECT ?revista ";
            string where = $@"WHERE {{
                                ?revista a <http://w3id.org/roh/MainDocument>. 
                                ?revista <http://purl.org/ontology/bibo/issn> ?issn. 
                                FILTER(?issn = '{pISSN}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "maindocument");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["revista"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Comprueba en SPARQL si existe la revista con el ISBN.
        /// </summary>
        /// <param name="pISBN">ISBN a consultar.</param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarRevistaISBN(string pISBN)
        {
            // Consulta sparql.
            string select = "SELECT ?revista ";
            string where = $@"WHERE {{
                                ?revista a <http://w3id.org/roh/MainDocument>. 
                                ?revista <http://purl.org/ontology/bibo/isbn> ?isbn. 
                                FILTER(?isbn = '{pISBN}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "maindocument");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["revista"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Comprueba en SPARQL si existe la revista con el título.
        /// </summary>
        /// <param name="pTitulo">Título a consultar.</param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarRevistaTitulo(string pTitulo)
        {
            // Consulta sparql.
            string select = "SELECT ?revista ";
            string where = $@"WHERE {{
                                ?revista a <http://w3id.org/roh/MainDocument>. 
                                ?revista <http://w3id.org/roh/title> ?titulo. 
                                FILTER(?titulo = '{pTitulo}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "maindocument");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["revista"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe una publicación.
        /// </summary>
        /// <param name="pDOI"></param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarPublicacion(string pDOI)
        {
            // Consulta sparql.
            string select = "SELECT ?documento ";
            string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                ?documento <http://purl.org/ontology/bibo/doi> ?doi. 
                                FILTER(?doi = '{pDOI}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["documento"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe un documento.
        /// </summary>
        /// <param name="pTitulo"></param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarPublicacionTitulo(string pTitulo)
        {
            // Consulta sparql.
            string select = "SELECT ?documento ";
            string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                ?documento  <http://w3id.org/roh/title> ?titulo. 
                                FILTER(?titulo = '{pTitulo.Replace("'", "\\'")}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["documento"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe la persona.
        /// </summary>
        /// <param name="pORCID"></param>
        /// <returns></returns>
        public static string ComprobarPersona(string pORCID)
        {
            // Consulta sparql.
            string select = "SELECT ?person";
            string where = $@"WHERE {{
                                ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                ?person <http://w3id.org/roh/ORCID> ?orcid. 
                                FILTER(?orcid = '{pORCID}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["person"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe la persona.
        /// </summary>
        /// <param name="pORCID"></param>
        /// <returns></returns>
        public static string ComprobarPersonaNombre(string pNombre)
        {
            // Consulta sparql.
            string select = "SELECT ?person";
            string where = $@"WHERE {{
                                ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                ?person <http://xmlns.com/foaf/0.1/name> ?nombre. 
                                FILTER(?nombre = '{pNombre.Replace("'", "\\'")}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["person"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Contruye el objeto persona a cargar.
        /// </summary>
        /// <param name="pNombreCompleto">Nombre completo (nombre + apellidos)</param>
        /// <param name="pNombre">Nombre de la persona.</param>
        /// <param name="pApellidos">Apellidos de la persona.</param>
        /// <returns>Objeto persona con los datos.</returns>
        public static Person ConstruirPersona(List<string> pNombreCompleto, List<string> pNombre, List<string> pApellidos)
        {
            Person person = new Person();
            if (pNombre != null && pNombre.Count > 0 && pApellidos != null && pApellidos.Count > 0)
            {
                person.Foaf_name = pNombre[0].Trim() + " " + pApellidos[0].Trim();
                person.Foaf_firstName = pNombre[0].Trim();
                person.Foaf_lastName = pApellidos[0].Trim();
            }
            else if (pNombreCompleto != null && pNombreCompleto.Count > 0)
            {
                person.Foaf_name = pNombreCompleto[0].Trim();
                if (person.Foaf_name.Contains(","))
                {
                    person.Foaf_firstName = pNombreCompleto[0].Split(',')[1].Trim();
                    person.Foaf_lastName = pNombreCompleto[0].Split(',')[0].Trim();
                }
                else if (person.Foaf_name.Contains(" "))
                {
                    person.Foaf_firstName = pNombreCompleto[0].Trim().Split(' ')[0].Trim();
                    person.Foaf_lastName = pNombreCompleto[0].Trim().Substring(pNombreCompleto[0].Trim().IndexOf(' ')).Trim();
                }
                else
                {
                    person.Foaf_firstName = pNombreCompleto[0].Trim();
                }
            }

            return person;
        }

        /// <summary>
        /// Limpia el ORCID de la persona.
        /// </summary>
        /// <param name="pORCID">Código ORCID a limpiar.</param>
        /// <returns>ORCID limpio.</returns>
        public static string LimpiarORCID(string pORCID)
        {
            try
            {
                Uri uri = new Uri(pORCID);
                return uri.AbsolutePath.Substring(1);
            }
            catch (Exception e)
            {

            }

            return pORCID;
        }

        /// <summary>
        /// Permite crear un archivo Zip de un único fichero.
        /// </summary>
        /// <param name="pRutaEscritura">Ruta dónde se va a guardar el archivo zip.</param>
        /// <param name="pNombreFichero">Nombre del fichero.</param>
        /// <param name="pData">Datos a guardar.</param>
        private static void CrearZip(string pRutaEscritura, string pNombreFichero, string pData)
        {
            using (FileStream zipToOpen = new FileStream($@"{pRutaEscritura}\{pNombreFichero.Split('.')[0]}.zip", FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry readmeEntry = archive.CreateEntry(pNombreFichero);
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.Write(pData);
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene los datos del tesauro.
        /// </summary>
        /// <returns>Tupla con los dos diccionarios.</returns>
        private static Tuple<Dictionary<string, string>, Dictionary<string, string>> ObtenerDatosTesauro()
        {
            Dictionary<string, string> dicAreasBroader = new Dictionary<string, string>();
            Dictionary<string, string> dicAreasNombre = new Dictionary<string, string>();

            string select = @"SELECT DISTINCT * ";
            string where = @$"WHERE {{
                ?concept a <http://www.w3.org/2008/05/skos#Concept>.
                ?concept <http://www.w3.org/2008/05/skos#prefLabel> ?nombre.
                ?concept <http://purl.org/dc/elements/1.1/source> 'researcharea'
                OPTIONAL{{?concept <http://www.w3.org/2008/05/skos#broader> ?broader}}
                }}";
            SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "taxonomy");

            foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            {
                string concept = fila["concept"].value;
                string nombre = fila["nombre"].value;
                string broader = "";
                if (fila.ContainsKey("broader"))
                {
                    broader = fila["broader"].value;
                }
                dicAreasBroader.Add(concept, broader);
                if (!dicAreasNombre.ContainsKey(nombre.ToLower()))
                {
                    dicAreasNombre.Add(nombre.ToLower(), concept);
                }
            }

            return new Tuple<Dictionary<string, string>, Dictionary<string, string>>(dicAreasBroader, dicAreasNombre);
        }
    }
}
