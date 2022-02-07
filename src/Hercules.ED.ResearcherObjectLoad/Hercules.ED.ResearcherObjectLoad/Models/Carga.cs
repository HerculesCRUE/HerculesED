﻿using Gnoss.ApiWrapper;
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
using Hercules.ED.ResearcherObjectLoad.Config;
using System.Threading;
using Hercules.ED.DisambiguationEngine.Models;
using System.Reflection;

namespace Hercules.ED.ResearcherObjectLoad.Models
{
    public class Carga
    {
        public static ResourceApi mResourceApi;
        public static CommunityApi mCommunityApi;
        public static ConfigService configuracion;

        #region --- Constantes
        public const string JOURNAL_ARTICLE = "Journal Article";
        public const string BOOK = "Book";
        public const string CHAPTER = "Chapter";
        public const string CONFERENCE_PAPER = "Conference Paper";
        public const string REVISTA_JOURNAL = "Journal";
        public const string REVISTA_BOOK = "Book";
        public const int MAX_INTENTOS = 3;
        #endregion

        // Diccionarios para almacenar los recursos que se van a desambiguar.
        public static Dictionary<string, ComplexOntologyResource> dicPersonasGnoss = new Dictionary<string, ComplexOntologyResource>();
        public static Dictionary<string, ComplexOntologyResource> dicPublicacionesGnoss = new Dictionary<string, ComplexOntologyResource>();

        public static void CargaMain()
        {
            ProcesarFichero(configuracion.GetRutaDirectorioLectura(), configuracion.GetRutaDirectorioEscritura());
        }

        public static void ProcesarFichero(string pRutaLectura, string pRutaEscritura)
        {
            DirectoryInfo directorio = new DirectoryInfo(pRutaLectura);

            Tuple<Dictionary<string, string>, Dictionary<string, string>> tupla = ObtenerDatosTesauro();
            while (true)
            {
                foreach (var fichero in directorio.GetFiles("*.json"))
                {
                    string jsonString = File.ReadAllText(fichero.FullName);
                    List<Publication> listaPublicaciones = JsonConvert.DeserializeObject<List<Publication>>(jsonString);

                    List<DisambiguableEntity> listaDesambiguar = new List<DisambiguableEntity>();
                    Dictionary<string, Person> dicIdPersona = new Dictionary<string, Person>();

                    Dictionary<string, Publication> dicIdDatosPub = new Dictionary<string, Publication>();
                    Dictionary<string, Document> dicIdPublication = new Dictionary<string, Document>();

                    foreach (Publication publication in listaPublicaciones)
                    {
                        // --- Publicación
                        DisambiguationPublication disambiguationPub = GetDisambiguationPublication(publication);
                        string idPub = disambiguationPub.ID;
                        listaDesambiguar.Add(disambiguationPub);

                        // --- Autor Principal (Siempre corresponde al primer autor de la secuencia de autores.)
                        if (publication.correspondingAuthor != null)
                        {
                            DisambiguationPerson disambiguationPerson = GetDisambiguationPerson(publication.correspondingAuthor);
                            string idPerson = disambiguationPerson.ID;
                            listaDesambiguar.Add(disambiguationPerson);
                            dicIdPersona.Add(idPerson, ContruirPersona(publication.correspondingAuthor));
                        }

                        // --- Autores
                        if (publication.seqOfAuthors != null && publication.seqOfAuthors.Any())
                        {
                            foreach (PersonaPub autor in publication.seqOfAuthors)
                            {
                                DisambiguationPerson disambiguationPerson = GetDisambiguationPerson(autor);
                                string idPerson = disambiguationPerson.ID;
                                listaDesambiguar.Add(disambiguationPerson);
                                dicIdPersona.Add(idPerson, ContruirPersona(autor));
                            }
                        }

                        dicIdDatosPub.Add(idPub, publication);
                        dicIdPublication.Add(idPub, ContruirDocument(publication, tupla.Item1, tupla.Item2));
                    }

                    // Obtención de la lista de equivalencias.
                    Dictionary<string, Dictionary<string, float>> listaEquivalencias = Disambiguation.Disambiguate(listaDesambiguar);

                    // TODO:
                    Dictionary<Person, List<string>> listaPersonasCreadas = new Dictionary<Person, List<string>>();
                    Dictionary<string, List<string>> dicIdsPersonas = new Dictionary<string, List<string>>();
                    Dictionary<Document, List<string>> listaPublicacionesCreadas = new Dictionary<Document, List<string>>();
                    Dictionary<string, List<string>> dicIdsPublicaciones = new Dictionary<string, List<string>>();

                    // Personas
                    foreach (KeyValuePair<string, Dictionary<string, float>> item in listaEquivalencias)
                    {
                        string tipo = item.Key.Split("_")[0];
                        string idA = item.Key.Split("_")[1];

                        HashSet<string> listaIds = new HashSet<string>();

                        foreach (KeyValuePair<string, float> item2 in item.Value)
                        {
                            string idB = item2.Key.Split("_")[1];

                            if (item2.Value >= 0.5) // TODO: Umbral.
                            {
                                listaIds.Add(idB);
                            }
                        }

                        if (tipo == "DisambiguationPerson" && listaIds.ToList().Any())
                        {
                            CrearPersonDesambiguada(idA, listaIds.ToList(), dicIdPersona, listaPersonasCreadas, dicIdsPersonas);
                        }
                    }

                    // Diccionario con TODAS las personas del fichero. (id, objetoPersona)
                    Dictionary<string, List<string>> dicAuxPersonas = new Dictionary<string, List<string>>();
                    Dictionary<string, Person> dicPersonasFinales = new Dictionary<string, Person>();
                    foreach (KeyValuePair<string, Person> item in dicIdPersona)
                    {
                        bool encontrado = false;

                        foreach (KeyValuePair<Person, List<string>> item2 in listaPersonasCreadas)
                        {
                            if (item2.Value.Contains(item.Key))
                            {
                                foreach (string id in item2.Value)
                                {
                                    if (dicPersonasFinales.ContainsKey(id))
                                    {
                                        encontrado = true;
                                        break;
                                    }

                                }
                                if (!encontrado)
                                {
                                    dicPersonasFinales.Add(item.Key, item2.Key);
                                    dicAuxPersonas.Add(item.Key, item2.Value);
                                }
                            }
                        }

                        if (!dicPersonasFinales.ContainsKey(item.Key))
                        {
                            if (!encontrado)
                            {
                                dicPersonasFinales.Add(item.Key, item.Value);
                                dicAuxPersonas.Add(item.Key, new List<string>() { item.Key });
                            }
                        }
                    }

                    // Creación de los ComplexOntologyResources
                    Dictionary<string, string> dicGnossIdPerson = new Dictionary<string, string>();
                    mResourceApi.ChangeOntoly("person");
                    foreach (KeyValuePair<string, Person> item in dicPersonasFinales)
                    {
                        ComplexOntologyResource resourcePersona = item.Value.ToGnossApiResource(mResourceApi, null);
                        dicGnossIdPerson.Add(item.Key, resourcePersona.GnossId);
                    }

                    // Publicaciones
                    //foreach (KeyValuePair<string, Dictionary<string, float>> item in listaEquivalencias)
                    //{
                    //    string tipo = item.Key.Split("_")[0];
                    //    string idA = item.Key.Split("_")[1];

                    //    HashSet<string> listaIds = new HashSet<string>();

                    //    foreach (KeyValuePair<string, float> item2 in item.Value)
                    //    {
                    //        if (item2.Value >= 0.5) // TODO: Umbral.
                    //        {
                    //            string idB = item2.Key.Split("_")[1];
                    //            listaIds.Add(idB);
                    //        }
                    //    }

                    //    if (tipo == "DisambiguationPublication" && listaIds.ToList().Any())
                    //    {
                    //        CrearPublicationDesambiguada(idA, listaIds.ToList(), dicIdDatosPub, listaPublicacionesCreadas, dicIdsPublicaciones, tupla.Item1, tupla.Item2);
                    //    }
                    //}

                    //// Diccionario con TODAS las publicaciones del fichero. (id, objetoDocument)
                    //Dictionary<string, Document> dicPublicacionesFinales = new Dictionary<string, Document>();
                    //foreach (KeyValuePair<string, Document> item in dicIdPublication)
                    //{
                    //    bool encontrado = false;

                    //    foreach (KeyValuePair<Document, List<string>> item2 in listaPublicacionesCreadas)
                    //    {
                    //        if (item2.Value.Contains(item.Key))
                    //        {
                    //            if (!dicPublicacionesFinales.ContainsKey(item.Key))
                    //            {
                    //                foreach (string id in item2.Value)
                    //                {
                    //                    if (dicPublicacionesFinales.ContainsKey(id))
                    //                    {
                    //                        encontrado = true;
                    //                        break;
                    //                    }

                    //                }

                    //                if (!encontrado)
                    //                {
                    //                    dicPublicacionesFinales.Add(item.Key, item2.Key);
                    //                }
                    //            }

                    //            break;
                    //        }
                    //    }

                    //    if (!encontrado)
                    //    {
                    //        if (!dicPublicacionesFinales.ContainsKey(item.Key))
                    //        {
                    //            Publication pubAux = dicIdDatosPub[item.Key];

                    //            // Autor de correspondencia
                    //            item.Value.IdsRoh_correspondingAuthor = new List<string>();
                    //            string idCorrespondingAuthor = pubAux.correspondingAuthor.ID;
                    //            foreach (KeyValuePair<string, List<string>> itemAux in dicAuxPersonas)
                    //            {
                    //                if (itemAux.Value.Contains(pubAux.correspondingAuthor.ID))
                    //                {
                    //                    idCorrespondingAuthor = itemAux.Key;
                    //                    break;
                    //                }
                    //            }
                    //            item.Value.IdsRoh_correspondingAuthor.Add(dicGnossIdPerson[idCorrespondingAuthor]);

                    //            // Autores
                    //            item.Value.IdsRoh_publicAuthorList = new List<string>();
                    //            foreach (PersonaPub personaAux in pubAux.seqOfAuthors)
                    //            {
                    //                string idPerson = personaAux.ID;
                    //                foreach (KeyValuePair<string, List<string>> itemAux in dicAuxPersonas)
                    //                {
                    //                    if (itemAux.Value.Contains(personaAux.ID))
                    //                    {
                    //                        idPerson = itemAux.Key;
                    //                        break;
                    //                    }
                    //                }
                    //                item.Value.IdsRoh_publicAuthorList.Add(dicGnossIdPerson[idPerson]);
                    //            }

                    //            dicPublicacionesFinales.Add(item.Key, item.Value);
                    //        }
                    //    }
                    //}

                    //// Creación de los ComplexOntologyResources
                    //Dictionary<string, string> dicGnossIdPub = new Dictionary<string, string>();
                    //mResourceApi.ChangeOntoly("document");
                    //foreach (KeyValuePair<string, Document> item in dicPublicacionesFinales)
                    //{
                    //    ComplexOntologyResource resourcePub = item.Value.ToGnossApiResource(mResourceApi, null);
                    //    dicGnossIdPub.Add(item.Key, resourcePub.GnossId);
                    //    dicPublicacionesGnoss.Add(item.Key, resourcePub);
                    //}

                    return;

                    //foreach (Publication publicacion in listaPublicaciones)
                    //{
                    //    ProcesarPublicacion(publicacion, tupla.Item1, tupla.Item2);
                    //    Console.WriteLine($@"{DateTime.Now} ------------------------------ Publicación leída.");
                    //}

                    // Hace una copia del fichero y elimina el original.
                    CrearZip(pRutaEscritura, fichero.Name, jsonString);
                    File.Delete(fichero.FullName);
                }

                Thread.Sleep(5000);
            }
        }

        private static void CrearPersonDesambiguada(string idPersona, List<string> pListaIds, Dictionary<string, Person> pDicIdPersona, Dictionary<Person, List<string>> pListaPersonasCreadas, Dictionary<string, List<string>> pDicIdsPersonasCreadas)
        {
            bool encontrado = false;
            string idEncontrado = string.Empty;
            foreach (KeyValuePair<string, List<string>> item in pDicIdsPersonasCreadas)
            {
                if (item.Value.Contains(idPersona))
                {
                    idEncontrado = item.Key;
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado)
            {
                Person personaA = pDicIdPersona[idPersona];
                foreach (string idSimilar in pListaIds)
                {
                    Person personaB = pDicIdPersona[idSimilar];

                    if (string.IsNullOrEmpty(personaA.Roh_ORCID) && !string.IsNullOrEmpty(personaB.Roh_ORCID))
                    {
                        personaA.Roh_ORCID = personaB.Roh_ORCID;
                    }

                    if (string.IsNullOrEmpty(personaA.Foaf_firstName) && !string.IsNullOrEmpty(personaB.Foaf_firstName))
                    {
                        personaA.Foaf_firstName = personaB.Foaf_firstName;
                    }

                    if (string.IsNullOrEmpty(personaA.Foaf_lastName) && !string.IsNullOrEmpty(personaB.Foaf_lastName))
                    {
                        personaA.Foaf_lastName = personaB.Foaf_lastName;
                    }

                    if (string.IsNullOrEmpty(personaA.Foaf_name) && !string.IsNullOrEmpty(personaB.Foaf_name))
                    {
                        personaA.Foaf_name = personaB.Foaf_name;
                    }

                    if (string.IsNullOrEmpty(personaA.Roh_semanticScholarId) && !string.IsNullOrEmpty(personaB.Roh_semanticScholarId))
                    {
                        personaA.Roh_semanticScholarId = personaB.Roh_semanticScholarId;
                    }
                }

                List<string> listaTotalIds = pListaIds;
                listaTotalIds.Add(idPersona);
                pDicIdsPersonasCreadas.Add(idPersona, listaTotalIds);
                pListaPersonasCreadas.Add(personaA, listaTotalIds);
            }
            else
            {
                foreach (KeyValuePair<Person, List<string>> item in pListaPersonasCreadas)
                {
                    if (item.Value.Contains(idEncontrado))
                    {
                        foreach (string idSimilar in pListaIds)
                        {
                            if (idSimilar != idEncontrado)
                            {
                                Person personaB = pDicIdPersona[idSimilar];

                                if (string.IsNullOrEmpty(item.Key.Roh_ORCID) && !string.IsNullOrEmpty(personaB.Roh_ORCID))
                                {
                                    item.Key.Roh_ORCID = personaB.Roh_ORCID;
                                }

                                if (string.IsNullOrEmpty(item.Key.Foaf_firstName) && !string.IsNullOrEmpty(personaB.Foaf_firstName))
                                {
                                    item.Key.Foaf_firstName = personaB.Foaf_firstName;
                                }

                                if (string.IsNullOrEmpty(item.Key.Foaf_lastName) && !string.IsNullOrEmpty(personaB.Foaf_lastName))
                                {
                                    item.Key.Foaf_lastName = personaB.Foaf_lastName;
                                }

                                if (string.IsNullOrEmpty(item.Key.Foaf_name) && !string.IsNullOrEmpty(personaB.Foaf_name))
                                {
                                    item.Key.Foaf_name = personaB.Foaf_name;
                                }

                                if (string.IsNullOrEmpty(item.Key.Roh_semanticScholarId) && !string.IsNullOrEmpty(personaB.Roh_semanticScholarId))
                                {
                                    item.Key.Roh_semanticScholarId = personaB.Roh_semanticScholarId;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void CrearPublicationDesambiguada(string idPublicacion, List<string> pListaIds, Dictionary<string, Publication> pDicIdPublicacion, Dictionary<Document, List<string>> pListaPublicacionesCreadas, Dictionary<string, List<string>> pDicIdsPublicacionesCreadas, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            bool encontrado = false;
            string idEncontrado = string.Empty;
            foreach (KeyValuePair<string, List<string>> item in pDicIdsPublicacionesCreadas)
            {
                if (item.Value.Contains(idPublicacion))
                {
                    idEncontrado = item.Key;
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado)
            {
                Publication documentoA = pDicIdPublicacion[idPublicacion];
                Document documentoCreado = new Document();

                foreach (string idSimilar in pListaIds)
                {
                    if (idSimilar != idEncontrado)
                    {
                        Publication documentoB = pDicIdPublicacion[idSimilar];
                        documentoCreado = ContruirDocument(documentoA, pDicAreasBroader, pDicAreasNombre, pPublicacionB: documentoB);
                    }
                }

                List<string> listaTotalIds = pListaIds;
                listaTotalIds.Add(idPublicacion);
                pDicIdsPublicacionesCreadas.Add(idPublicacion, listaTotalIds);
                pListaPublicacionesCreadas.Add(documentoCreado, listaTotalIds);
            }
            else
            {
                Publication documentoA = pDicIdPublicacion[idPublicacion];
                Document documentoCreado = new Document();
                foreach (KeyValuePair<Document, List<string>> item in pListaPublicacionesCreadas)
                {
                    if (item.Value.Contains(idEncontrado))
                    {
                        foreach (string idSimilar in pListaIds)
                        {
                            if (idSimilar != idEncontrado)
                            {
                                Publication documentoB = pDicIdPublicacion[idSimilar];
                                documentoCreado = ContruirDocument(documentoA, pDicAreasBroader, pDicAreasNombre, pPublicacionB: documentoB);
                            }
                        }
                    }
                }
            }
        }

        public static Dictionary<string, Person> dicIdPersonas = new Dictionary<string, Person>();

        public static Document ContruirDocument(Publication pPublicacion, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre, Publication pPublicacionB = null)
        {
            Document document = new Document();

            // Estado de validación (IsValidated)
            document.Roh_isValidated = true;

            // ID DOI (Doi)
            if (!string.IsNullOrEmpty(pPublicacion.doi))
            {
                document.Bibo_doi = pPublicacion.doi;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.doi) && string.IsNullOrEmpty(document.Bibo_doi))
                {
                    document.Bibo_doi = pPublicacionB.doi;
                }
            }

            // Identificadores
            HashSet<string> listaIds = new HashSet<string>();
            if (pPublicacion.iDs != null && pPublicacion.iDs.Any())
            {
                foreach (string id in pPublicacion.iDs)
                {
                    listaIds.Add(id);
                }
            }
            if (listaIds.Any())
            {
                document.Bibo_identifier = new List<fDocument>();

                foreach (string id in listaIds)
                {
                    if (id.Contains(":"))
                    {
                        if (id.ToLower().Contains("wos"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "WoS";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("semanticscholar"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "SemanticScholar";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("mag"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "MAG";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("pubmedcentral"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "PubMedCentral";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("scopus_id"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "Scopus";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("arxiv"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "ArXiv";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("medline"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "Medline";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }
                    }
                }
            }

            // Título (Title)
            if (!string.IsNullOrEmpty(pPublicacion.title))
            {
                document.Roh_title = pPublicacion.title;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.title) && string.IsNullOrEmpty(document.Roh_title))
                {
                    document.Roh_title = pPublicacionB.title;
                }
            }

            // Descripción (Abstract)
            if (!string.IsNullOrEmpty(pPublicacion.@abstract))
            {
                string descpLimpia = pPublicacion.@abstract;
                if (pPublicacion.@abstract.StartsWith("[") && pPublicacion.@abstract.EndsWith("]"))
                {
                    descpLimpia = pPublicacion.@abstract.Remove(0, 1);
                    descpLimpia = descpLimpia.Remove(descpLimpia.Length - 1, 1);
                }
                document.Bibo_abstract = descpLimpia.Trim();

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.@abstract) && string.IsNullOrEmpty(document.Bibo_abstract))
                {
                    string descpLimpiaB = pPublicacionB.@abstract;
                    if (pPublicacionB.@abstract.StartsWith("[") && pPublicacionB.@abstract.EndsWith("]"))
                    {
                        descpLimpiaB = pPublicacionB.@abstract.Remove(0, 1);
                        descpLimpiaB = descpLimpiaB.Remove(descpLimpiaB.Length - 1, 1);
                    }
                    document.Bibo_abstract = descpLimpiaB.Trim();
                }
            }

            // Tipo de publicación (TypeOfPublication)
            if (pPublicacion.typeOfPublication != null)
            {
                if (pPublicacion.typeOfPublication == JOURNAL_ARTICLE)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_020";
                }
                else if (pPublicacion.typeOfPublication == BOOK)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_032";
                }
                else if (pPublicacion.typeOfPublication == CHAPTER)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_004";
                }
                else if (pPublicacion.typeOfPublication == CONFERENCE_PAPER)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD2";
                }
                else
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_OTHERS";
                    document.Roh_typeOthers = "Otros";
                }
            }

            // Etiquetas Externas (ExternalFreeTextKeywords)
            HashSet<string> listaSinRepetirEtiquetas = new HashSet<string>();
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
            }
            document.Roh_externalKeywords = listaSinRepetirEtiquetas.ToList();

            // Etiquetas Enriquecidas (EnrichedFreeTextKeywords)
            HashSet<string> etiquetasEnriquecidas = new HashSet<string>();
            if (pPublicacion.freetextKeyword_enriquecidas != null && pPublicacion.freetextKeyword_enriquecidas.Count > 0)
            {
                foreach (FreetextKeywordEnriquecida tag in pPublicacion.freetextKeyword_enriquecidas)
                {
                    if (!listaSinRepetirEtiquetas.Contains(tag.word.ToLower()))
                    {
                        etiquetasEnriquecidas.Add(tag.word);
                        listaSinRepetirEtiquetas.Add(tag.word.ToLower());
                    }
                }
            }
            document.Roh_enrichedKeywords = etiquetasEnriquecidas.ToList();

            // Fecha de publicación (Issued)
            if (pPublicacion.dataIssued != null && pPublicacion.dataIssued.datimeTime != null)
            {
                document.Dct_issued = pPublicacion.dataIssued.datimeTime;
            }

            // URL (Url)
            HashSet<string> urlSinRepetir = new HashSet<string>();
            if (pPublicacion.url != null && pPublicacion.url.Count() > 0)
            {
                foreach (string url in pPublicacion.url)
                {
                    urlSinRepetir.Add(url);
                }
            }
            document.Vcard_url = urlSinRepetir.ToList();

            // Página de inicio (PageStart)
            if (!string.IsNullOrEmpty(pPublicacion.pageStart) && Int32.TryParse(pPublicacion.pageEnd, out int n1))
            {
                document.Bibo_pageStart = Int32.Parse(pPublicacion.pageStart);
            }

            // Página de fin (PageEnd)
            if (!string.IsNullOrEmpty(pPublicacion.pageEnd) && Int32.TryParse(pPublicacion.pageEnd, out int n3))
            {
                document.Bibo_pageEnd = Int32.Parse(pPublicacion.pageEnd);
            }

            // Áreas de conocimiento externas (ExternalKnowledgeArea)                
            HashSet<string> listaIDs = new HashSet<string>();
            if (pPublicacion.hasKnowledgeAreas != null && pPublicacion.hasKnowledgeAreas.Count > 0)
            {
                document.Roh_externalKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                foreach (HasKnowledgeArea knowledgearea in pPublicacion.hasKnowledgeAreas)
                {
                    if (knowledgearea.resource.ToLower() == "hércules")
                    {
                        foreach (KnowledgeArea area in knowledgearea.knowledgeArea)
                        {
                            if (pDicAreasNombre.ContainsKey(area.name.ToLower()))
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
                                        document.Roh_externalKnowledgeArea.Add(categoria);
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
                document.Roh_enrichedKnowledgeArea = new List<DocumentOntology.CategoryPath>();
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
                                document.Roh_enrichedKnowledgeArea.Add(categoria);
                            }
                        }
                        listaIDs.Add(idHijoAux);
                    }
                }
            }

            // Métricas (HasMetric)
            if (pPublicacion.hasMetric != null && pPublicacion.hasMetric.Count > 0)
            {
                HashSet<string> listaMetricas = new HashSet<string>();
                foreach (HasMetric itemMetric in pPublicacion.hasMetric)
                {
                    if (itemMetric.metricName.ToLower() == "wos")
                    {
                        document.Roh_wos = Int32.Parse(itemMetric.citationCount);
                    }
                    else
                    {
                        if (!listaMetricas.Contains(itemMetric.metricName.ToLower()))
                        {
                            PublicationMetric publicationMetric = new PublicationMetric();
                            publicationMetric.Roh_metricName = itemMetric.metricName;
                            publicationMetric.Roh_citationCount = Int32.Parse(itemMetric.citationCount);
                            document.Roh_hasMetric = new List<PublicationMetric>() { publicationMetric };
                            listaMetricas.Add(itemMetric.metricName.ToLower());
                        }
                    }
                }
            }

            // Revista (HasPublicationVenue)
            if (pPublicacion.hasPublicationVenue != null && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name) && (pPublicacion.hasPublicationVenue.type == "Journal" || string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.type)))
            {
                // Comprobar si la revista existe o no.
                string idRevista = string.Empty;

                // Comprobar ISSN
                if (string.IsNullOrEmpty(idRevista) && pPublicacion.hasPublicationVenue.issn != null && pPublicacion.hasPublicationVenue.issn.Count > 0)
                {
                    foreach (string issn in pPublicacion.hasPublicationVenue.issn)
                    {
                        idRevista = ComprobarRevistaISSN(issn);
                        if (!string.IsNullOrEmpty(idRevista))
                        {
                            break;
                        }
                    }
                }

                // Comprobar EISSN
                if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.eissn))
                {
                    idRevista = ComprobarRevistaEISSN(pPublicacion.hasPublicationVenue.eissn);
                }

                // Comprobar Título
                if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name))
                {
                    idRevista = ComprobarRevistaTitulo(pPublicacion.hasPublicationVenue.name);
                }

                // Apunte a la revista.
                if (!string.IsNullOrEmpty(idRevista))
                {
                    document.IdVivo_hasPublicationVenue = idRevista;
                }

                if (pPublicacion.hasPublicationVenue.hasMetric != null && pPublicacion.hasPublicationVenue.hasMetric.Count > 0)
                {
                    foreach (HasMetric metrica in pPublicacion.hasPublicationVenue.hasMetric)
                    {
                        if (float.TryParse(metrica.impactFactor.ToString(), out float numFloat1))
                        {
                            document.Roh_impactIndexInYear = float.Parse(metrica.impactFactor);
                        }
                        if (!string.IsNullOrEmpty(metrica.quartile))
                        {
                            switch (metrica.quartile)
                            {
                                case "1":
                                    document.Roh_quartile = 1;
                                    break;
                                case "Q1":
                                    document.Roh_quartile = 1;
                                    break;
                                case "2":
                                    document.Roh_quartile = 2;
                                    break;
                                case "Q2":
                                    document.Roh_quartile = 2;
                                    break;
                                case "3":
                                    document.Roh_quartile = 3;
                                    break;
                                case "Q3":
                                    document.Roh_quartile = 3;
                                    break;
                                case "4":
                                    document.Roh_quartile = 4;
                                    break;
                                case "Q4":
                                    document.Roh_quartile = 4;
                                    break;
                            }
                        }
                    }
                }
            }

            return document;
        }

        private static void ProcesarPublicacion(Publication pPublicacion, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            string idDocumento = "";
            //ComprobarPublicacion(pPublicacion);

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

            // Estado de validación (IsValidated)
            documentoCargar.Roh_isValidated = true;

            // Diccionario para guardar los ORCID - ID_RECURSO de las personas de la publicación.
            Dictionary<string, string> dicOrcidRecurso = new Dictionary<string, string>();

            // Si no tiene título, no se carga la publicación.
            if (!string.IsNullOrEmpty(pPublicacion.title) && pPublicacion.correspondingAuthor != null && pPublicacion.correspondingAuthor.name != null)
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

                            if (id.ToLower().Contains("scopus_id"))
                            {
                                fDocument fDocumento = new fDocument();
                                fDocumento.Foaf_topic = "Scopus";
                                fDocumento.Dc_title = id.Split(":")[1].Trim();
                                documentoCargar.Bibo_identifier.Add(fDocumento);
                            }

                            if (id.ToLower().Contains("arxiv"))
                            {
                                fDocument fDocumento = new fDocument();
                                fDocumento.Foaf_topic = "ArXiv";
                                fDocumento.Dc_title = id.Split(":")[1].Trim();
                                documentoCargar.Bibo_identifier.Add(fDocumento);
                            }

                            if (id.ToLower().Contains("medline"))
                            {
                                fDocument fDocumento = new fDocument();
                                fDocumento.Foaf_topic = "Medline";
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
                    string descpLimpia = pPublicacion.@abstract;
                    if (pPublicacion.@abstract.StartsWith("[") && pPublicacion.@abstract.EndsWith("]"))
                    {
                        descpLimpia = pPublicacion.@abstract.Remove(0, 1);
                        descpLimpia = descpLimpia.Remove(descpLimpia.Length - 1, 1);
                    }
                    documentoCargar.Bibo_abstract = descpLimpia.Trim();
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
                    HashSet<string> urlSinRepetir = new HashSet<string>();
                    documentoCargar.Vcard_url = new List<string>();
                    foreach (string url in pPublicacion.url)
                    {
                        urlSinRepetir.Add(url);
                    }
                    foreach (string url in urlSinRepetir)
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
                    // Autor (Person)
                    documentoCargar.IdsRoh_correspondingAuthor = new List<string>();
                    Person autor = ContruirPersona(pPublicacion.correspondingAuthor);
                    mResourceApi.ChangeOntoly("person");
                    ComplexOntologyResource resource = autor.ToGnossApiResource(mResourceApi, null);
                    //dicPersonasGnoss.Add(autor, resource);
                    documentoCargar.IdsRoh_correspondingAuthor.Add(resource.GnossId);

                    // Comprobar si existe la persona.
                    //string idPersona = ComprobarPersona(pPublicacion.correspondingAuthor);

                    //if (string.IsNullOrEmpty(idPersona))
                    //{
                    //    Person persona = ConstruirPersona(pPublicacion.correspondingAuthor.name.nombre_completo, pPublicacion.correspondingAuthor.name.given, pPublicacion.correspondingAuthor.name.familia);

                    //    // ID ORCID (Orcid)
                    //    string orcidLimpio = pPublicacion.correspondingAuthor.orcid;
                    //    if (!string.IsNullOrEmpty(pPublicacion.correspondingAuthor.orcid))
                    //    {
                    //        orcidLimpio = LimpiarORCID(pPublicacion.correspondingAuthor.orcid);

                    //        if (dicOrcidRecurso.ContainsKey(orcidLimpio))
                    //        {
                    //            idPersona = dicOrcidRecurso[orcidLimpio];
                    //        }
                    //    }

                    //    // Identificadores
                    //    if (pPublicacion.correspondingAuthor.iDs != null && pPublicacion.correspondingAuthor.iDs.Count > 0)
                    //    {
                    //        foreach (string id in pPublicacion.correspondingAuthor.iDs)
                    //        {
                    //            if (id.Contains(":"))
                    //            {
                    //                if (id.ToLower().Contains("semanticscholar"))
                    //                {
                    //                    persona.Roh_semanticScholarId = id.Split(":")[1].Trim();
                    //                }
                    //            }
                    //        }
                    //    }

                    //    // TODO: Hacer comprobación a parte del nombre... (ASIO)
                    //    //if (string.IsNullOrEmpty(idPersona) && !string.IsNullOrEmpty(persona.Foaf_name))
                    //    //{
                    //    //    idPersona = ComprobarPersonaNombre(persona.Foaf_name);
                    //    //}

                    //    // Carga
                    //    if (string.IsNullOrEmpty(idPersona) && !string.IsNullOrEmpty(persona.Foaf_name))
                    //    {
                    //        mResourceApi.ChangeOntoly("person");
                    //        ComplexOntologyResource resourcePersona = persona.ToGnossApiResource(mResourceApi, null);
                    //        int numIntentos = 0;
                    //        while (!resourcePersona.Uploaded)
                    //        {
                    //            numIntentos++;

                    //            if (numIntentos > MAX_INTENTOS)
                    //            {
                    //                break;
                    //            }

                    //            //mResourceApi.LoadComplexSemanticResource(resourcePersona, false, true);
                    //        }
                    //        idPersona = resourcePersona.GnossId;
                    //        mResourceApi.ChangeOntoly("document");
                    //    }

                    //    if (!string.IsNullOrEmpty(orcidLimpio) && !dicOrcidRecurso.ContainsKey(orcidLimpio))
                    //    {
                    //        // Guardo la persona con su identificador.
                    //        dicOrcidRecurso.Add(orcidLimpio, idPersona);
                    //    }
                    //}
                    //else if (!string.IsNullOrEmpty(LimpiarORCID(pPublicacion.correspondingAuthor.orcid)) && !string.IsNullOrEmpty(LimpiarORCID(pPublicacion.correspondingAuthor.orcid)) && !dicOrcidRecurso.ContainsKey(LimpiarORCID(pPublicacion.correspondingAuthor.orcid)))
                    //{
                    //    // Guardo la persona con su identificador.
                    //    dicOrcidRecurso.Add(LimpiarORCID(pPublicacion.correspondingAuthor.orcid), idPersona);
                    //}

                    //if (!string.IsNullOrEmpty(idPersona))
                    //{
                    //    documentoCargar.IdsRoh_correspondingAuthor.Add(idPersona);
                    //}
                }

                // Lista de autores (AuthorList)
                if (pPublicacion.seqOfAuthors != null)
                {
                    documentoCargar.Bibo_authorList = new List<BFO_0000023>();

                    // Orden de los autores.
                    int orden = 1;

                    foreach (PersonaPub itemAutor in pPublicacion.seqOfAuthors)
                    {
                        BFO_0000023 relacionPersona = new BFO_0000023();

                        Person autor = ContruirPersona(itemAutor);
                        mResourceApi.ChangeOntoly("person");
                        ComplexOntologyResource resource = autor.ToGnossApiResource(mResourceApi, null);
                        //dicPersonasGnoss.Add(autor, resource);
                        relacionPersona.IdRdf_member = resource.GnossId;

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


                        // Comprobar si existe la persona con el ORCID.
                        //string idPersona = ComprobarPersona(itemAutor);

                        //if (string.IsNullOrEmpty(idPersona) && itemAutor.name != null)
                        //{
                        //    Person persona = ConstruirPersona(itemAutor.name.nombre_completo, itemAutor.name.given, itemAutor.name.familia);

                        //    // ID ORCID (Orcid)
                        //    string orcidLimpio = itemAutor.orcid;
                        //    if (!string.IsNullOrEmpty(itemAutor.orcid))
                        //    {
                        //        orcidLimpio = LimpiarORCID(itemAutor.orcid);

                        //        if (dicOrcidRecurso.ContainsKey(orcidLimpio))
                        //        {
                        //            idPersona = dicOrcidRecurso[orcidLimpio];
                        //        }
                        //    }

                        //    // Identificadores
                        //    if (itemAutor.iDs != null && itemAutor.iDs.Count > 0)
                        //    {
                        //        foreach (string id in itemAutor.iDs)
                        //        {
                        //            if (id.Contains(":"))
                        //            {
                        //                if (id.ToLower().Contains("semanticscholar"))
                        //                {
                        //                    persona.Roh_semanticScholarId = id.Split(":")[1].Trim();
                        //                }
                        //            }
                        //        }
                        //    }

                        //    // TODO: Hacer comprobación a parte del nombre... (ASIO)
                        //    //if (string.IsNullOrEmpty(idPersona) && !string.IsNullOrEmpty(persona.Foaf_name))
                        //    //{
                        //    //    idPersona = ComprobarPersonaNombre(persona.Foaf_name);
                        //    //}

                        //    // Carga
                        //    if (string.IsNullOrEmpty(idPersona) && !string.IsNullOrEmpty(persona.Foaf_name))
                        //    {
                        //        mResourceApi.ChangeOntoly("person");
                        //        ComplexOntologyResource resourcePersona = persona.ToGnossApiResource(mResourceApi, null);
                        //        int numIntentos = 0;
                        //        while (!resourcePersona.Uploaded)
                        //        {
                        //            numIntentos++;

                        //            if (numIntentos > MAX_INTENTOS)
                        //            {
                        //                break;
                        //            }
                        //            //mResourceApi.LoadComplexSemanticResource(resourcePersona, false, true);
                        //        }
                        //        idPersona = resourcePersona.GnossId;
                        //        mResourceApi.ChangeOntoly("document");
                        //    }

                        //    if (!string.IsNullOrEmpty(orcidLimpio) && !dicOrcidRecurso.ContainsKey(orcidLimpio) && !string.IsNullOrEmpty(persona.Foaf_name))
                        //    {
                        //        dicOrcidRecurso.Add(orcidLimpio, idPersona);
                        //    }
                        //}
                        //else if (!string.IsNullOrEmpty(LimpiarORCID(itemAutor.orcid)) && !dicOrcidRecurso.ContainsKey(LimpiarORCID(itemAutor.orcid)))
                        //{
                        //    dicOrcidRecurso.Add(LimpiarORCID(itemAutor.orcid), idPersona);
                        //}

                        //// Agrego la relación con BFO_0000023.
                        //if (!string.IsNullOrEmpty(idPersona))
                        //{
                        //    relacionPersona.IdRdf_member = idPersona;

                        //    // Firma (Nick)
                        //    if (itemAutor.name != null)
                        //    {
                        //        if (itemAutor.name.nombre_completo != null && itemAutor.name.nombre_completo.Count > 0)
                        //        {
                        //            relacionPersona.Foaf_nick = itemAutor.name.nombre_completo[0];
                        //        }
                        //        else if (itemAutor.name.given != null && itemAutor.name.given.Count > 0 && itemAutor.name.familia != null && itemAutor.name.familia.Count > 0 && !string.IsNullOrEmpty(itemAutor.name.given[0]) && !string.IsNullOrEmpty(itemAutor.name.familia[0]))
                        //        {
                        //            relacionPersona.Foaf_nick = itemAutor.name.given[0] + " " + itemAutor.name.familia[0];
                        //        }
                        //    }

                        //    // Orden (Comment)
                        //    relacionPersona.Rdf_comment = orden;

                        //    documentoCargar.Bibo_authorList.Add(relacionPersona);
                        //    orden++;
                        //}
                    }
                }

                // Áreas de conocimiento externas (ExternalKnowledgeArea)                
                HashSet<string> listaIDs = new HashSet<string>();
                if (pPublicacion.hasKnowledgeAreas != null && pPublicacion.hasKnowledgeAreas.Count > 0)
                {
                    documentoCargar.Roh_externalKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                    foreach (HasKnowledgeArea knowledgearea in pPublicacion.hasKnowledgeAreas)
                    {
                        if (knowledgearea.resource.ToLower() == "hércules")
                        {
                            foreach (KnowledgeArea area in knowledgearea.knowledgeArea)
                            {
                                if (pDicAreasNombre.ContainsKey(area.name.ToLower()))
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
                    HashSet<string> listaMetricas = new HashSet<string>();
                    foreach (HasMetric itemMetric in pPublicacion.hasMetric)
                    {
                        if (itemMetric.metricName.ToLower() == "wos")
                        {
                            documentoCargar.Roh_wos = Int32.Parse(itemMetric.citationCount);
                        }
                        else
                        {
                            if (!listaMetricas.Contains(itemMetric.metricName.ToLower()))
                            {
                                PublicationMetric publicationMetric = new PublicationMetric();
                                publicationMetric.Roh_metricName = itemMetric.metricName;
                                publicationMetric.Roh_citationCount = Int32.Parse(itemMetric.citationCount);
                                documentoCargar.Roh_hasMetric = new List<PublicationMetric>() { publicationMetric };
                                listaMetricas.Add(itemMetric.metricName.ToLower());
                            }
                        }
                    }
                }

                // Revista (HasPublicationVenue)
                if (pPublicacion.hasPublicationVenue != null && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name) && (pPublicacion.hasPublicationVenue.type == "Journal" || string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.type)))
                {
                    // Comprobar si la revista existe o no.
                    string idRevista = string.Empty;

                    // Comprobar ISSN
                    if (string.IsNullOrEmpty(idRevista) && pPublicacion.hasPublicationVenue.issn != null && pPublicacion.hasPublicationVenue.issn.Count > 0)
                    {
                        foreach (string issn in pPublicacion.hasPublicationVenue.issn)
                        {
                            idRevista = ComprobarRevistaISSN(issn);
                            if (!string.IsNullOrEmpty(idRevista))
                            {
                                break;
                            }
                        }
                    }

                    // Comprobar EISSN
                    if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.eissn))
                    {
                        idRevista = ComprobarRevistaEISSN(pPublicacion.hasPublicationVenue.eissn);
                    }

                    // Comprobar ISBN
                    //if (string.IsNullOrEmpty(idRevista) && pPublicacion.hasPublicationVenue.isbn != null && pPublicacion.hasPublicationVenue.isbn.Count > 0)
                    //{
                    //    foreach (string isbn in pPublicacion.hasPublicationVenue.isbn)
                    //    {
                    //        idRevista = ComprobarRevistaISBN(isbn);
                    //        if (!string.IsNullOrEmpty(idRevista))
                    //        {
                    //            break;
                    //        }
                    //    }
                    //}

                    // Comprobar Título
                    if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name))
                    {
                        idRevista = ComprobarRevistaTitulo(pPublicacion.hasPublicationVenue.name);
                    }

                    // Apunte a la revista.
                    if (!string.IsNullOrEmpty(idRevista))
                    {
                        documentoCargar.IdVivo_hasPublicationVenue = idRevista;
                    }

                    if (pPublicacion.hasPublicationVenue.hasMetric != null && pPublicacion.hasPublicationVenue.hasMetric.Count > 0)
                    {
                        foreach (HasMetric metrica in pPublicacion.hasPublicationVenue.hasMetric)
                        {
                            if (float.TryParse(metrica.impactFactor.ToString(), out float numFloat1))
                            {
                                documentoCargar.Roh_impactIndexInYear = float.Parse(metrica.impactFactor);
                            }
                            if (!string.IsNullOrEmpty(metrica.quartile))
                            {
                                switch (metrica.quartile)
                                {
                                    case "1":
                                        documentoCargar.Roh_quartile = 1;
                                        break;
                                    case "Q1":
                                        documentoCargar.Roh_quartile = 1;
                                        break;
                                    case "2":
                                        documentoCargar.Roh_quartile = 2;
                                        break;
                                    case "Q2":
                                        documentoCargar.Roh_quartile = 2;
                                        break;
                                    case "3":
                                        documentoCargar.Roh_quartile = 3;
                                        break;
                                    case "Q3":
                                        documentoCargar.Roh_quartile = 3;
                                        break;
                                    case "4":
                                        documentoCargar.Roh_quartile = 4;
                                        break;
                                    case "Q4":
                                        documentoCargar.Roh_quartile = 4;
                                        break;
                                }
                            }
                        }
                    }
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
                        string idDocumentoAux = "";
                        //ComprobarPublicacion(pub);

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

                // Generar ID de la publicación principal.
                ComplexOntologyResource resourceDocumentoPrimaria = new ComplexOntologyResource();
                string idDocumentoPrincipal = string.Empty;
                if (pPubPrimaria && string.IsNullOrEmpty(pIdDocumento))
                {
                    Guid gnossIdPrimaria = mResourceApi.GetShortGuid(idDocumentoActual);
                    Guid articleIdPrimaria = new Guid(idDocumentoActual.Split('_')[2]);
                    resourceDocumentoPrimaria = documentoCargar.ToGnossApiResource(mResourceApi, null, gnossIdPrimaria, articleIdPrimaria);
                    idDocumentoPrincipal = $"http://gnoss.com/items/Document_{Guid.NewGuid().ToString().ToLower()}_{Guid.NewGuid().ToString().ToLower()}";
                }
                else if (pPubPrimaria && !string.IsNullOrEmpty(pIdDocumento))
                {
                    Guid gnossIdPrimaria = mResourceApi.GetShortGuid(pIdDocumento);
                    Guid articleIdPrimaria = new Guid(pIdDocumento.Split('_')[2]);
                    resourceDocumentoPrimaria = documentoCargar.ToGnossApiResource(mResourceApi, null, gnossIdPrimaria, articleIdPrimaria);
                    idDocumentoPrincipal = $"http://gnoss.com/items/Document_{Guid.NewGuid().ToString().ToLower()}_{Guid.NewGuid().ToString().ToLower()}";
                }

                if (pPublicacion.citas != null)
                {
                    foreach (Publication item in pPublicacion.citas)
                    {
                        // Comprobar si el documento existe o no.
                        string idDocumentoAux = "";
                        //ComprobarPublicacion(item);

                        if (string.IsNullOrEmpty(pIdDocumento))
                        {
                            pIdDocumento = idDocumentoPrincipal;
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

                // Carga de la publicación.
                mResourceApi.ChangeOntoly("document");
                //dicPublicacionesGnoss.Add(documentoCargar, resourceDocumentoPrimaria);

                if (pPubPrimaria) // Si es una publicación primaria
                {
                    if (pTuplaDatosRecuperados != null) // Si hay datos obtenidos de la recuperación...
                    {
                        int numIntentos = 0;
                        while (!resourceDocumentoPrimaria.Modified)
                        {
                            numIntentos++;

                            if (numIntentos > MAX_INTENTOS)
                            {
                                break;
                            }
                            //mResourceApi.ModifyComplexOntologyResource(resourceDocumentoPrimaria, false, true);
                        }
                    }
                    else
                    {
                        int numIntentos = 0;
                        while (!resourceDocumentoPrimaria.Uploaded)
                        {
                            numIntentos++;

                            if (numIntentos > MAX_INTENTOS)
                            {
                                break;
                            }

                            //mResourceApi.LoadComplexSemanticResource(resourceDocumentoPrimaria, false, true);
                        }
                    }
                }
                else if (pPubSecundariaBiblio) // Si es una publicación secundaria de bibliografía
                {
                    if (string.IsNullOrEmpty(idDocumentoActual)) // Si viene vacío...
                    {
                        Guid gnossIdSecundario = mResourceApi.GetShortGuid(idDocumentoActual);
                        Guid articleIdSecundario = new Guid(idDocumentoActual.Split('_')[2]);
                        ComplexOntologyResource resourceDocumento = documentoCargar.ToGnossApiResource(mResourceApi, null, gnossIdSecundario, articleIdSecundario);
                        //dicPublicacionesGnoss.Add(documentoCargar, resourceDocumento);
                        int numIntentos = 0;
                        while (!resourceDocumento.Uploaded)
                        {
                            numIntentos++;

                            if (numIntentos > MAX_INTENTOS)
                            {
                                break;
                            }

                            //mResourceApi.LoadComplexSemanticResource(resourceDocumento, false, true);
                        }
                    }
                }
                else if (pPubSecundariaCita) // Si es una publicación secundaria de cita
                {
                    if (!string.IsNullOrEmpty(pIdDocumento)) // Si NO viene vacío...
                    {
                        //Dictionary<Guid, List<TriplesToInclude>> triples = new Dictionary<Guid, List<TriplesToInclude>>();
                        //triples.Add(mResourceApi.GetShortGuid(pIdDocumento), new List<TriplesToInclude>() {
                        //    new TriplesToInclude(){
                        //            Predicate = "http://purl.org/ontology/bibo/cites",
                        //            NewValue = pIdPadre
                        //        }
                        //    });
                        //Dictionary<Guid, bool> resultado = mResourceApi.InsertPropertiesLoadedResources(triples);
                        //bool comprobacion = resultado.ContainsKey(mResourceApi.GetShortGuid(pIdDocumento));
                    }
                    else
                    {
                        //Guid gnossId = mResourceApi.GetShortGuid(idDocumentoActual);
                        //Guid articleId = new Guid(idDocumentoActual.Split('_')[2]);
                        //ComplexOntologyResource resourceDocumento = documentoCargar.ToGnossApiResource(mResourceApi, null, gnossId, articleId);
                        //string idCreado = string.Empty;
                        //int numIntentos = 0;
                        //while (!resourceDocumento.Uploaded)
                        //{
                        //    numIntentos++;

                        //    if (numIntentos > MAX_INTENTOS)
                        //    {
                        //        break;
                        //    }

                        //    idCreado = mResourceApi.LoadComplexSemanticResource(resourceDocumento, false, true);
                        //}
                        //Dictionary<Guid, List<TriplesToInclude>> triples = new Dictionary<Guid, List<TriplesToInclude>>();
                        //triples.Add(mResourceApi.GetShortGuid(idCreado), new List<TriplesToInclude>() {
                        //    new TriplesToInclude(){
                        //            Predicate = "http://purl.org/ontology/bibo/cites",
                        //            NewValue = pIdPadre
                        //        }
                        //    });
                        //Dictionary<Guid, bool> resultado = mResourceApi.InsertPropertiesLoadedResources(triples);
                        //bool comprobacion = resultado.ContainsKey(mResourceApi.GetShortGuid(idDocumentoActual));
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
        /// Consulta en SPARQL si existe el documento con el DOI.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns>ID del recurso.</returns>
        public static string ObtenerDoiPublicacion(string pId)
        {
            // Consulta sparql.
            string select = "SELECT ?doi";
            string where = $@"WHERE {{
                                <{pId}> <http://purl.org/ontology/bibo/doi> ?doi
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["doi"].value;
                }
            }

            return string.Empty;
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
        /// Comprueba en SPARQL si existe la revista con el ISSN.
        /// </summary>
        /// <param name="pEissn">ISSN a consultar.</param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarRevistaEISSN(string pEissn)
        {
            // Consulta sparql.
            string select = "SELECT ?revista ";
            string where = $@"WHERE {{
                                ?revista a <http://w3id.org/roh/MainDocument>. 
                                ?revista <http://purl.org/ontology/bibo/eissn> ?eissn. 
                                FILTER(?eissn = '{pEissn}')
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
                                FILTER(?titulo = '{pTitulo.Replace("'", "\\'")}')
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
        public static string ComprobarPublicacionDoi(string pDOI)
        {
            if (string.IsNullOrEmpty(pDOI))
            {
                return string.Empty;
            }

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

        public static string ComprobarPublicacionesIdFuente(string pId, string pNombreFuente)
        {
            if (string.IsNullOrEmpty(pId))
            {
                return string.Empty;
            }

            // Consulta sparql.
            string select = "SELECT ?documento ";
            string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                ?documento <http://purl.org/ontology/bibo/identifier> ?identificador.
                                ?identificador <http://purl.org/dc/elements/1.1/title> ?id.
                                ?identificador <http://xmlns.com/foaf/0.1/topic> ?nombreFuente.
                                FILTER(?id = '{pId}')
                                FILTER(?nombreFuente = '{pNombreFuente}')
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

            try
            {
                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        return fila["documento"].value;
                    }
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe la persona.
        /// </summary>
        /// <param name="pORCID">ID ORCID.</param>
        /// <returns></returns>
        public static string ComprobarPersonaOrcid(string pORCID)
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
        /// <param name="pSemanticScholarId">ID Semantic Scholar.</param>
        /// <returns></returns>
        public static string ComprobarPersonaSemanticScholarId(string pSemanticScholarId)
        {
            // Consulta sparql.
            string select = "SELECT ?person";
            string where = $@"WHERE {{
                                ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                ?person <http://w3id.org/roh/semanticScholarId> ?semanticScholarId. 
                                FILTER(?semanticScholarId = '{pSemanticScholarId}')
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
        /// <param name="pNombre"></param>
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
        public static Person ConstruirPersonaNO(List<string> pNombreCompleto, List<string> pNombre, List<string> pApellidos)
        {
            Person person = new Person();
            if (pNombre != null && pNombre.Count > 0 && !string.IsNullOrEmpty(pNombre[0]) && pApellidos != null && pApellidos.Count > 0 && !string.IsNullOrEmpty(pApellidos[0]))
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
        /// Contruye el objeto persona a cargar.
        /// </summary>
        /// <param name="pNombreCompleto">Nombre completo (nombre + apellidos)</param>
        /// <param name="pNombre">Nombre de la persona.</param>
        /// <param name="pApellidos">Apellidos de la persona.</param>
        /// <returns>Objeto persona con los datos.</returns>
        public static Person ContruirPersona(PersonaPub pPersona)
        {
            Person person = new Person();

            if (pPersona.name.given != null && pPersona.name.given.Any() && pPersona.name.given[0] != null && pPersona.name.familia != null && pPersona.name.familia.Any() && pPersona.name.familia[0] != null)
            {
                person.Foaf_name = pPersona.name.given[0].Trim() + " " + pPersona.name.familia[0].Trim();
                person.Foaf_firstName = pPersona.name.given[0].Trim();
                person.Foaf_lastName = pPersona.name.familia[0].Trim();
            }
            else if (pPersona.name.nombre_completo != null && pPersona.name.nombre_completo.Any())
            {
                person.Foaf_name = pPersona.name.nombre_completo[0].Trim();
                if (person.Foaf_name.Contains(","))
                {
                    person.Foaf_firstName = pPersona.name.nombre_completo[0].Split(',')[1].Trim();
                    person.Foaf_lastName = pPersona.name.nombre_completo[0].Split(',')[0].Trim();
                }
                else if (person.Foaf_name.Contains(" "))
                {
                    person.Foaf_firstName = pPersona.name.nombre_completo[0].Trim().Split(' ')[0].Trim();
                    person.Foaf_lastName = pPersona.name.nombre_completo[0].Trim().Substring(pPersona.name.nombre_completo[0].Trim().IndexOf(' ')).Trim();
                }
                else
                {
                    person.Foaf_firstName = pPersona.name.nombre_completo[0].Trim();
                }
            }

            if (!string.IsNullOrEmpty(pPersona.orcid))
            {
                person.Roh_ORCID = pPersona.orcid;
            }

            if (pPersona.iDs != null && pPersona.iDs.Any())
            {
                foreach (string item in pPersona.iDs)
                {
                    if (item.ToLower().Contains("semanticscholar"))
                    {
                        person.Roh_semanticScholarId = item.Split(":")[1].Trim();
                    }
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

        /// <summary>
        /// Obtiene el ID indicado de una publicación dada.
        /// </summary>
        /// <param name="pPublicacion">Publicación a obtener el ID.</param>
        /// <param name="pId">ID a obtener.</param>
        /// <returns>ID obtenido.</returns>
        private static string GetPublicationId(Publication pPublicacion, string pId)
        {
            if (pPublicacion.iDs != null && pPublicacion.iDs.Any())
            {
                foreach (string id in pPublicacion.iDs)
                {
                    if (id.ToLower().Contains(pId))
                    {
                        return id.Split(":")[1].Trim();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Obtiene el objeto para desambiguar publicaciones.
        /// </summary>
        /// <param name="pPublicacion">Publicación a convertir.</param>
        /// <returns>Objeto para desambiguar.</returns>
        private static DisambiguationPublication GetDisambiguationPublication(Publication pPublicacion)
        {
            pPublicacion.ID = Guid.NewGuid().ToString();

            // Obtención de IDs.
            string idWos = GetPublicationId(pPublicacion, "wos");
            string idSemanticScholar = GetPublicationId(pPublicacion, "semanticscholar");
            string idMag = GetPublicationId(pPublicacion, "mag");
            string idScopus = GetPublicationId(pPublicacion, "scopus_id");

            DisambiguationPublication pub = new DisambiguationPublication()
            {
                ID = pPublicacion.ID,
                doi = pPublicacion.doi,
                title = pPublicacion.title
            };

            return pub;
        }

        /// <summary>
        /// Obtiene el ID indicado de una persona dada.
        /// </summary>
        /// <param name="pPersona">Persona a obtener el ID.</param>
        /// <param name="pId">ID a obtener.</param>
        /// <returns>ID obtenido.</returns>
        private static string GetPersonId(PersonaPub pPersona, string pId)
        {
            if (pPersona.iDs != null && pPersona.iDs.Any())
            {
                foreach (string id in pPersona.iDs)
                {
                    if (id.ToLower().Contains(pId))
                    {
                        return id.Split(":")[1].Trim();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Obtiene el objeto para desambiguar personas.
        /// </summary>
        /// <param name="pPersona">Persona a convertir.</param>
        /// <returns>Objeto para desambiguar.</returns>
        private static DisambiguationPerson GetDisambiguationPerson(PersonaPub pPersona)
        {
            pPersona.ID = Guid.NewGuid().ToString();

            // Obtención de IDs.
            string idWos = GetPersonId(pPersona, "wos");
            string idSemanticScholar = GetPersonId(pPersona, "semanticscholar");
            string idMag = GetPersonId(pPersona, "mag");
            string idScopus = GetPersonId(pPersona, "scopus_id");

            string nombreCompleto = string.Empty;
            if (pPersona.name != null && pPersona.name.nombre_completo != null && pPersona.name.nombre_completo.Any())
            {
                nombreCompleto = pPersona.name.nombre_completo[0];
            }

            DisambiguationPerson persona = new DisambiguationPerson()
            {
                ID = pPersona.ID,
                orcid = pPersona.orcid,
                completeName = nombreCompleto
            };

            return persona;
        }

        /// <summary>
        /// Comprueba las publicaciones para que no hayan duplicados.
        /// </summary>
        /// <param name="pPublicacion">Publicación a comprobar.</param>
        /// <returns>ID del recurso cargado.</returns>
        private static string ComprobarPublicacion(Publication pPublicacion)
        {
            // ID del recurso.
            string idDocumento = string.Empty;

            // Comprueba si existe la publicación mediante el DOI.
            idDocumento = ComprobarPublicacionDoi(pPublicacion.doi);

            // Comprueba si existe la publicación mediante los otros IDs.
            if (pPublicacion.iDs != null && pPublicacion.iDs.Count > 0)
            {
                foreach (string id in pPublicacion.iDs)
                {
                    if (id.Contains(":"))
                    {
                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("wos"))
                        {
                            // Comprueba si existe la publicación mediante el WOS ID.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "WoS");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("semanticscholar"))
                        {
                            // Comprueba si existe la publicación mediante el SemanticScholar ID.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "SemanticScholar");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("mag"))
                        {
                            // Comprueba si existe la publicación mediante el MAG ID.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "MAG");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("pubmedcentral"))
                        {
                            // Comprueba si existe la publicación mediante el DOI.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "PubMedCentral");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("scopus_id"))
                        {
                            // Comprueba si existe la publicación mediante el Scopus ID.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "Scopus");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("arxiv"))
                        {
                            // Comprueba si existe la publicación mediante el ID de arXiv.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "ArXiv");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("arxiv"))
                        {
                            // Comprueba si existe la publicación mediante el ID de arXiv.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "Medline");
                        }
                    }
                }
            }

            // TODO: Comprueba si está la publicación cargada. (ASIO) (De momento, solo compruebo el titulo)
            if (string.IsNullOrEmpty(pPublicacion.title))
            {
                idDocumento = ComprobarPublicacionTitulo(pPublicacion.title);
            }

            return idDocumento;
        }

        private static string ComprobarPersona(PersonaPub pPersona)
        {
            string idPersona = string.Empty;

            // Comprobar si existe la persona en SPARQL con el ORCID.
            idPersona = ComprobarPersonaOrcid(LimpiarORCID(pPersona.orcid));

            // Comprobar si existe la persona en SPARQL con el SemanticScholar ID.
            if (string.IsNullOrEmpty(idPersona) && pPersona.iDs != null && pPersona.iDs.Count > 0)
            {
                foreach (string id in pPersona.iDs)
                {
                    if (id.Contains(":"))
                    {
                        if (id.ToLower().Contains("semanticscholar"))
                        {
                            idPersona = ComprobarPersonaSemanticScholarId(id.Split(":")[1].Trim());
                        }
                    }
                }
            }

            return idPersona;
        }
    }
}
