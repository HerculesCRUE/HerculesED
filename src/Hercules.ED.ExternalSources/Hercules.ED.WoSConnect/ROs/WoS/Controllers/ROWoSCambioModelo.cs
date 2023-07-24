using System.Collections.Generic;
using WoSConnect.ROs.WoS.Models;
using WoSConnect.ROs.WoS.Models.Inicial;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Data;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WoSConnect.ROs.WoS.Controllers
{
    public class ROWoSControllerJSON
    {
        public List<string> advertencia = null;
        public ROWoSLogic WoSLogic;
        private ResourceApi mResourceApi;

        public ROWoSControllerJSON(ROWoSLogic WoSLogic, ResourceApi pResourceApi)
        {
            this.WoSLogic = WoSLogic;
            mResourceApi = pResourceApi;
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
        /// <param name="pResourceApi"></param>
        /// <param name="pTuplaTesauro"></param>
        /// <returns></returns>
        public Publication getPublicacionCita(PublicacionInicial pPublicacionIn, Dictionary<string, string> pTuplaTesauro, ResourceApi pResourceApi)
        {
            Publication publicacionFinal = new Publication();
            publicacionFinal.typeOfPublication = getType(pPublicacionIn);
            publicacionFinal.IDs = getWosID(pPublicacionIn);
            publicacionFinal.title = getTitle(pPublicacionIn);
            publicacionFinal.Abstract = getAbstract(pPublicacionIn);
            publicacionFinal.doi = getDoi(pPublicacionIn);
            publicacionFinal.dataIssued = getDate(pPublicacionIn);
            publicacionFinal.hasKnowledgeAreas = getKnowledgeAreas(pPublicacionIn, pTuplaTesauro, pResourceApi);
            publicacionFinal.freetextKeywords = getFreetextKeyword(pPublicacionIn);
            publicacionFinal.seqOfAuthors = getAuthors(pPublicacionIn);
            publicacionFinal.correspondingAuthor = publicacionFinal.seqOfAuthors[0];
            publicacionFinal.hasPublicationVenue = getJournal(pPublicacionIn);
            publicacionFinal.hasMetric = getPublicationMetric(pPublicacionIn);
            publicacionFinal.openAccess = getOpenAccess(pPublicacionIn);
            publicacionFinal.volume = getVolume(pPublicacionIn);

            return publicacionFinal;
        }


        public List<Publication> getListPublication(Root objInicial, Dictionary<string, string> pTuplaTesauro, ResourceApi pResourceApi)
        {
            List<Publication> listaResultados = new List<Publication>();
            if (objInicial != null && objInicial.Data != null && objInicial.Data.Records != null
                && objInicial.Data.Records.records != null && objInicial.Data.Records.records.REC != null)
            {
                foreach (PublicacionInicial rec in objInicial.Data.Records.records.REC)
                {
                    try
                    {
                        Publication publicacion = cambioDeModeloPublicacion(rec, true, pTuplaTesauro, pResourceApi);

                        if (publicacion != null)
                        {
                            if (this.advertencia != null)
                            {
                                publicacion.problema = this.advertencia;
                                this.advertencia = null;
                            }

                            listaResultados.Add(publicacion);
                        }
                    }
                    catch (Exception ex)
                    {
                        mResourceApi.Log.Error($@"[ERROR] {DateTime.Now} {ex.Message} {ex.StackTrace}");
                    }
                }
            }

            return listaResultados;
        }

        public Publication cambioDeModeloPublicacion(PublicacionInicial objInicial, bool publicacion_principal, Dictionary<string, string> pTuplaTesauro, ResourceApi pResourceApi)
        {
            Publication publicacion = new Publication();
            publicacion.typeOfPublication = getType(objInicial);
            publicacion.IDs = getWosID(objInicial);
            publicacion.title = getTitle(objInicial);
            publicacion.Abstract = getAbstract(objInicial);
            publicacion.language = getLanguage(objInicial);
            publicacion.doi = getDoi(objInicial);
            publicacion.dataIssued = getDate(objInicial);
            publicacion.pageStart = getPageStart(objInicial);
            publicacion.pageEnd = getPageEnd(objInicial);
            publicacion.hasKnowledgeAreas = getKnowledgeAreas(objInicial, pTuplaTesauro, pResourceApi);
            publicacion.freetextKeywords = getFreetextKeyword(objInicial);
            publicacion.seqOfAuthors = getAuthors(objInicial);
            if (publicacion.seqOfAuthors.Count > 0)
            {
                publicacion.correspondingAuthor = publicacion.seqOfAuthors[0];
            }
            publicacion.hasPublicationVenue = getJournal(objInicial);
            publicacion.hasMetric = getPublicationMetric(objInicial);
            publicacion.openAccess = getOpenAccess(objInicial);
            publicacion.volume = getVolume(objInicial);
            publicacion.dataOrigin = "WoS";
            publicacion.conferencia = getConferencia(objInicial);
            if (publicacion.typeOfPublication == CHAPTER)
            {
                publicacion.doi = null;
            }
            return publicacion;
        }

        /// <summary>
        /// Obtiene el tipo de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el tipo.</param>
        /// <returns>Tipo de la publicación.</returns>
        public string getType(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.doctypes != null && pPublicacionIn.static_data.summary.doctypes.doctype != null)
            {
                if (pPublicacionIn.static_data.summary.doctypes.doctype.Contains(BOOK_CHAPTER))
                {
                    return CHAPTER;
                }
                else if (pPublicacionIn.static_data.summary.doctypes.doctype.Contains(BOOK))
                {
                    return BOOK;
                }
                else if (pPublicacionIn.static_data.summary.doctypes.doctype.Contains(PROCEEDINGS_PAPER))
                {
                    return CONFERENCE_PAPER;
                }
                else if (pPublicacionIn.static_data.summary.doctypes.doctype.Contains(ARTICLE))
                {
                    return JOURNAL_ARTICLE;
                }
                else
                {
                    return JOURNAL_ARTICLE;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene el identificador de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el WoS ID.</param>
        /// <returns></returns>
        public List<string> getWosID(PublicacionInicial pPublicacionIn)
        {
            if (!string.IsNullOrEmpty(pPublicacionIn.UID))
            {
                return new List<string>() { pPublicacionIn.UID };
            }

            return null;
        }

        /// <summary>
        /// Obtiene el título de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el título.</param>
        /// <returns>Título de la publicación.</returns>
        public string getTitle(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn != null && pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.titles != null && pPublicacionIn.static_data.summary.titles.title != null && pPublicacionIn.static_data.summary.titles.title.Any())
            {
                Title title = pPublicacionIn.static_data.summary.titles.title.FirstOrDefault(x => x.type == "item");
                if (title != null && title.content.Count > 0)
                {
                    return title.content[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene la descripción de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener la descripción.</param>
        /// <returns>Descripción.</returns>
        public string getAbstract(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.fullrecord_metadata != null && pPublicacionIn.static_data.fullrecord_metadata.abstracts != null && pPublicacionIn.static_data.fullrecord_metadata.abstracts.@abstract != null && pPublicacionIn.static_data.fullrecord_metadata.abstracts.@abstract.Count > 0)
            {
                string descripcion = string.Empty;
                foreach (Abstract abstractin in pPublicacionIn.static_data.fullrecord_metadata.abstracts.@abstract)
                {
                    if(abstractin.abstract_text!=null && abstractin.abstract_text.p != null && abstractin.abstract_text.p.Count>0)
                    {
                        foreach (string item in abstractin.abstract_text.p)
                        {
                            descripcion += item.Trim() + " ";
                        }
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
        public string getLanguage(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.fullrecord_metadata != null && pPublicacionIn.static_data.fullrecord_metadata.languages != null && pPublicacionIn.static_data.fullrecord_metadata.languages.language != null && pPublicacionIn.static_data.fullrecord_metadata.languages.language.Count > 0)
            {
                return pPublicacionIn.static_data.fullrecord_metadata.languages.language.First().content.Trim();
            }

            return null;
        }

        /// <summary>
        /// Obtiene el identificador DOI de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el DOI.</param>
        /// <returns>DOI.</returns>
        public string getDoi(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.dynamic_data != null && pPublicacionIn.dynamic_data.cluster_related != null && pPublicacionIn.dynamic_data.cluster_related.identifiers != null && pPublicacionIn.dynamic_data.cluster_related.identifiers.identifier != null && pPublicacionIn.dynamic_data.cluster_related.identifiers.identifier.Any())
            {
                foreach (Identificadores item in pPublicacionIn.dynamic_data.cluster_related.identifiers.identifier)
                {
                    if (item.type.Contains("doi"))
                    {
                        return item.value;
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
        public DateTimeValue getDate(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.pub_info != null && !string.IsNullOrEmpty(pPublicacionIn.static_data.summary.pub_info.sortdate))
            {
                DateTimeValue fecha = new DateTimeValue();
                fecha.datimeTime = pPublicacionIn.static_data.summary.pub_info.sortdate;
                return fecha;
            }

            return null;
        }

        public Conferencia getConferencia(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.conferences != null && pPublicacionIn.static_data.summary.conferences.conference[0] != null)
            {
                Conferencia conferencia = new Conferencia();

                // ID.
                conferencia.id = pPublicacionIn.static_data.summary.conferences.conference[0].conf_id;

                // Título.
                if (pPublicacionIn.static_data.summary.conferences.conference[0].conf_titles != null && pPublicacionIn.static_data.summary.conferences.conference[0].conf_titles.count == 1 && !string.IsNullOrEmpty(pPublicacionIn.static_data.summary.conferences.conference[0].conf_titles.conf_title))
                {
                    conferencia.titulo = pPublicacionIn.static_data.summary.conferences.conference[0].conf_titles.conf_title;
                }

                // Fechas
                if (pPublicacionIn.static_data.summary.conferences.conference[0].conf_dates != null && pPublicacionIn.static_data.summary.conferences.conference[0].conf_dates.conf_date != null)
                {
                    try
                    {
                        int yearInicio = int.Parse(pPublicacionIn.static_data.summary.conferences.conference[0].conf_dates.conf_date.conf_start.ToString().Substring(0, 4));
                        int monthInicio = int.Parse(pPublicacionIn.static_data.summary.conferences.conference[0].conf_dates.conf_date.conf_start.ToString().Substring(4, 2));
                        int dayInicio = int.Parse(pPublicacionIn.static_data.summary.conferences.conference[0].conf_dates.conf_date.conf_start.ToString().Substring(6, 2));
                        conferencia.fechaInicio = $@"{yearInicio}-{monthInicio}-{dayInicio}";
                    }
                    catch
                    {
                        // Fecha inválida.
                    }

                    try
                    {
                        int yearFin = int.Parse(pPublicacionIn.static_data.summary.conferences.conference[0].conf_dates.conf_date.conf_start.ToString().Substring(0, 4));
                        int monthFin = int.Parse(pPublicacionIn.static_data.summary.conferences.conference[0].conf_dates.conf_date.conf_start.ToString().Substring(4, 2));
                        int dayFin = int.Parse(pPublicacionIn.static_data.summary.conferences.conference[0].conf_dates.conf_date.conf_start.ToString().Substring(6, 2));
                        conferencia.fechaFin = $@"{yearFin}-{monthFin}-{dayFin}";
                    }
                    catch
                    {
                        // Fecha inválida.
                    }
                }

                // Pais y ciudad
                if (pPublicacionIn.static_data.summary.conferences.conference[0].conf_locations != null && pPublicacionIn.static_data.summary.conferences.conference[0].conf_locations.conf_location != null && pPublicacionIn.static_data.summary.conferences.conference[0].conf_locations.count == 1)
                {
                    if (!string.IsNullOrEmpty(pPublicacionIn.static_data.summary.conferences.conference[0].conf_locations.conf_location.conf_state))
                    {
                        conferencia.pais = pPublicacionIn.static_data.summary.conferences.conference[0].conf_locations.conf_location.conf_state;
                    }

                    if (!string.IsNullOrEmpty(pPublicacionIn.static_data.summary.conferences.conference[0].conf_locations.conf_location.conf_city))
                    {
                        conferencia.ciudad = pPublicacionIn.static_data.summary.conferences.conference[0].conf_locations.conf_location.conf_city;
                    }
                }

                return conferencia;
            }

            return null;
        }

        /// <summary>
        /// Obtiene el número de la página de inicio.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el número de la página.</param>
        /// <returns>Número de la página.</returns>
        public string getPageStart(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.pub_info != null && pPublicacionIn.static_data.summary.pub_info.page != null)
            {
                if (pPublicacionIn.static_data.summary.pub_info.page.begin != null && pPublicacionIn.static_data.summary.pub_info.page.begin.ToString() != "0")
                {
                    return pPublicacionIn.static_data.summary.pub_info.page.begin.ToString();
                }
                else if (pPublicacionIn.static_data.summary.pub_info.page.page_count != 0)
                {
                    if (pPublicacionIn.static_data.summary.pub_info.page.end == null || !int.TryParse(pPublicacionIn.static_data.summary.pub_info.page.end.ToString(), out int endPage))
                    {
                        return "1";
                    }
                    return endPage - pPublicacionIn.static_data.summary.pub_info.page.page_count > 0 ? (endPage - pPublicacionIn.static_data.summary.pub_info.page.page_count).ToString() : "1";
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene el número de la página de fin.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener el número de la página.</param>
        /// <returns>Número de la página.</returns>
        public string getPageEnd(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.pub_info != null && pPublicacionIn.static_data.summary.pub_info.page != null)
            {
                if (pPublicacionIn.static_data.summary.pub_info.page.end != null && pPublicacionIn.static_data.summary.pub_info.page.end.ToString() != "0")
                {
                    return pPublicacionIn.static_data.summary.pub_info.page.end.ToString();
                }
                else if (pPublicacionIn.static_data.summary.pub_info.page.page_count != 0)
                {
                    return pPublicacionIn.static_data.summary.pub_info.page.page_count.ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// Obtiene las areas de conocimiento de la taxonomía unificada.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener las areas.</param>
        /// <param name="pTuplaTesauro"></param>
        /// <param name="pResourceApi"></param>
        /// <returns>Areas.</returns>
        public List<KnowledgeAreas> getKnowledgeAreas(PublicacionInicial pPublicacionIn, Dictionary<string, string> pTuplaTesauro, ResourceApi pResourceApi)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.fullrecord_metadata != null && pPublicacionIn.static_data.fullrecord_metadata.category_info != null && pPublicacionIn.static_data.fullrecord_metadata.category_info.subjects != null && pPublicacionIn.static_data.fullrecord_metadata.category_info.subjects.subject != null && pPublicacionIn.static_data.fullrecord_metadata.category_info.subjects.subject.Any())
            {
                List<KnowledgeAreas> listaAreas = new List<KnowledgeAreas>();
                KnowledgeAreas area = new KnowledgeAreas();
                area.resource = "WoS";
                area.knowledgeArea = new List<KnowledgeArea>();
                foreach (Subject item in pPublicacionIn.static_data.fullrecord_metadata.category_info.subjects.subject)
                {
                    if (item.ascatype == "traditional")
                    {
                        KnowledgeArea areaCompleta = new KnowledgeArea();
                        areaCompleta.name = item.content;
                        if (!string.IsNullOrEmpty(item.code))
                        {
                            areaCompleta.hasCode = item.code;
                        }
                        area.knowledgeArea.Add(areaCompleta);
                    }
                }
                //listaAreas.Add(area);

                // Mapeo de la categoría con la taxonomía unificada.
                KnowledgeAreas taxonomia = recuperar_taxonomia(area, pTuplaTesauro, pResourceApi);
                if (taxonomia != null)
                {
                    listaAreas.Add(taxonomia);
                }

                return listaAreas;
            }

            return null;
        }

        private static Tuple<string, string> ObtenerEquivalencias(string pNombre, ResourceApi pResourceApi)
        {
            string select = @"SELECT DISTINCT ?id ?concept ?nombre FROM <http://gnoss.com/referencesource.owl> ";
            string where = @$"WHERE {{
                        ?concept <http://purl.org/dc/elements/1.1/identifier> ?id.
                        ?concept <http://w3id.org/roh/sourceDescriptor> ?descriptor.
                        ?concept <http://www.w3.org/2008/05/skos#prefLabel> ?nombre.
                        ?descriptor <http://w3id.org/roh/impactSourceCategory> ?nombreAux.
                        FILTER (lcase(str(?nombreAux)) = '{pNombre}').
                        ?descriptor <http://w3id.org/roh/impactSource> <http://gnoss.com/items/referencesource_000>. 
                }}";
            SparqlObject resultadoQuery = pResourceApi.VirtuosoQuery(select, where, "taxonomy");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count == 1)
            {
                Dictionary<string, SparqlObject.Data> fila = resultadoQuery.results.bindings.First();
                return new Tuple<string, string>(fila["id"].value, fila["nombre"].value);
            }
            return null;
        }

        public KnowledgeAreas recuperar_taxonomia(KnowledgeAreas areas_wos, Dictionary<string, string> pTuplaTesauro, ResourceApi pResourceApi)
        {
            KnowledgeAreas taxonomia_hercules = new KnowledgeAreas();
            List<KnowledgeArea> listado = new List<KnowledgeArea>();
            taxonomia_hercules.resource = "Hércules";

            List<KnowledgeArea> wos = areas_wos.knowledgeArea;
            foreach (KnowledgeArea area_wos_obtenida in wos)
            {
                try
                {
                    Tuple<string, string> equivalencia = ObtenerEquivalencias(area_wos_obtenida.name.ToLower(), pResourceApi);
                    if (equivalencia != null)
                    {
                        KnowledgeArea area_taxonomia = new KnowledgeArea();
                        area_taxonomia.hasCode = equivalencia.Item1;
                        area_taxonomia.name = equivalencia.Item2;
                        listado.Add(area_taxonomia);
                    }
                }
                catch (Exception)
                {
                    mResourceApi.Log.Error($@"[ERROR] {DateTime.Now} No se encuentra en la taxonomia el siguiente area: {area_wos_obtenida.name}");
                }
            }
            if (listado.Count > 0)
            {
                taxonomia_hercules.knowledgeArea = listado;
                return taxonomia_hercules;
            }
            else { return null; }
        }

        /// <summary>
        /// Obtiene las etiquetas de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener las etiquetas.</param>
        /// <returns>Listado de etiquetas.</returns>
        public List<FreetextKeywords> getFreetextKeyword(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.fullrecord_metadata != null && pPublicacionIn.static_data.fullrecord_metadata.keywords != null && pPublicacionIn.static_data.fullrecord_metadata.keywords.keyword != null && pPublicacionIn.static_data.fullrecord_metadata.keywords.keyword.Any())
            {
                List<FreetextKeywords> listaEtiquetas = new List<FreetextKeywords>();
                FreetextKeywords etiquetas = new FreetextKeywords();
                etiquetas.source = "WoS";
                etiquetas.freetextKeyword = new List<string>();
                foreach (string item in pPublicacionIn.static_data.fullrecord_metadata.keywords.keyword)
                {
                    if (!etiquetas.freetextKeyword.Contains(item))
                    {
                        etiquetas.freetextKeyword.Add(item);
                    }
                }
                listaEtiquetas.Add(etiquetas);

                return listaEtiquetas;
            }

            return null;
        }

        /// <summary>
        /// Obtiene los autores de la publicación.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener autores.</param>
        /// <returns>Lista de personas.</returns>
        public List<Person> getAuthors(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.names != null && pPublicacionIn.static_data.summary.names.name != null && pPublicacionIn.static_data.summary.names.name.Any())
            {
                List<Person> listaPersonas = new List<Person>();
                Dictionary<Models.Inicial.Name, string> authorsORCID = ObtenerORCIDsAuthors(pPublicacionIn);
                foreach (Models.Inicial.Name item in pPublicacionIn.static_data.summary.names.name)
                {
                    if (!item.display_name.Contains(" ") || !item.full_name.Contains(" ") || item.display_name.ToLower().Contains("ieee") || item.full_name.ToLower().Contains("ieee") || (string.IsNullOrEmpty(item.first_name) && string.IsNullOrEmpty(item.last_name) && string.IsNullOrEmpty(item.full_name) && string.IsNullOrEmpty(item.orcid_id)))
                    {
                        continue;
                    }
                    if (item.role != "author")
                    {
                        continue;
                    }
                    Person person = new Person();
                    if (item.seq_no != null)
                    {
                        person.orden = item.seq_no;
                    }
                    person.fuente = "WoS";
                    person.name = new Models.Name();
                    person.name.given = new List<string>() { item.first_name };
                    person.name.familia = new List<string>() { item.last_name };
                    string nombreCompleto = $@"{item.first_name} {item.last_name}".Trim();
                    if (!string.IsNullOrEmpty(nombreCompleto))
                    {
                        person.name.nombre_completo = new List<string>() { nombreCompleto };
                    }
                    else
                    {
                        person.name.given = new List<string>() { item.full_name.Split(", ")[1] };
                        person.name.familia = new List<string>() { item.full_name.Split(", ")[0] };
                        person.name.nombre_completo = new List<string>() { $@"{item.full_name.Split(", ")[1]} {item.full_name.Split(", ")[0]}".Trim() };
                    }

                    if (authorsORCID.ContainsKey(item) && !string.IsNullOrEmpty(authorsORCID[item]))
                    {
                        person.ORCID = authorsORCID[item];
                    }

                    if (person.name.nombre_completo != null || !string.IsNullOrEmpty(person.ORCID))
                    {
                        string orcid = person.ORCID;
                        bool encontrado = false;

                        foreach (Person persona in listaPersonas)
                        {
                            if (!string.IsNullOrEmpty(orcid) && orcid == persona.ORCID)
                            {
                                encontrado = true;
                                break;
                            }
                        }
                        if (!encontrado)
                        {
                            listaPersonas.Add(person);
                        }
                    }
                }
                return listaPersonas;
            }

            return null;
        }

        /// <summary>
        /// Obtiene el ORCID del contributor asociado al nombre
        /// </summary>
        /// <param name="doc">Publicación</param>
        /// <param name="fullName">Nombre del autor</param>
        /// <returns>ORCID</returns>
        private static Dictionary<Models.Inicial.Name, string> ObtenerORCIDsAuthors(PublicacionInicial doc)
        {
            Dictionary<Models.Inicial.Name, string> authorsORCID = new Dictionary<Models.Inicial.Name, string>();

            Dictionary<Models.Inicial.Name, Dictionary<Models.Inicial.Contributor, float>> authorsContributors = new Dictionary<Models.Inicial.Name, Dictionary<Contributor, float>>();
            foreach (Models.Inicial.Name item in doc.static_data.summary.names.name)
            {
                if (!item.display_name.Contains(" ") || !item.full_name.Contains(" ") || item.display_name.ToLower().Contains("ieee") || item.full_name.ToLower().Contains("ieee") || (string.IsNullOrEmpty(item.first_name) && string.IsNullOrEmpty(item.last_name) && string.IsNullOrEmpty(item.full_name) && string.IsNullOrEmpty(item.orcid_id)))
                {
                    continue;
                }
                if (item.role != "author")
                {
                    continue;
                }

                string nombreCompletoAutor = $@"{item.first_name} {item.last_name}".Trim();
                if (string.IsNullOrEmpty(nombreCompletoAutor))
                {
                    nombreCompletoAutor = $@"{item.full_name.Split(", ")[1]} {item.full_name.Split(", ")[0]}".Trim();
                }
                List<string> namesAuthor = OrdenacionesNombre(nombreCompletoAutor);
                if (doc.static_data.contributors != null && doc.static_data.contributors.contributor != null && doc.static_data.contributors.contributor.Any())
                {
                    foreach (Contributor itemContributor in doc.static_data.contributors.contributor)
                    {
                        if (itemContributor.name != null && !string.IsNullOrEmpty(itemContributor.name.full_name))
                        {
                            List<string> namesContributor = OrdenacionesNombre(itemContributor.name.full_name);
                            float igualdad = CompararNombres(namesAuthor, namesContributor);
                            if (igualdad > 0)
                            {
                                if (!authorsContributors.ContainsKey(item))
                                {
                                    authorsContributors[item] = new Dictionary<Contributor, float>();
                                }
                                authorsContributors[item][itemContributor] = igualdad;
                            }
                        }
                    }
                }
            }

            //Eliminamos en los autors todos los contributors duplicados y después cargamos en cada author sólo los que tengan el mayor valor (si sólo hay uno)
            Dictionary<Contributor, int> contributorCount = new Dictionary<Contributor, int>();
            foreach (Models.Inicial.Name author in authorsContributors.Keys)
            {
                foreach (Contributor contributor in authorsContributors[author].Keys)
                {
                    if (!contributorCount.ContainsKey(contributor))
                    {
                        contributorCount[contributor] = 1;
                    }
                    else
                    {
                        contributorCount[contributor]++;
                    }
                }
            }
            foreach (var contributor in contributorCount.Where(x => x.Value > 1))
            {
                foreach (Models.Inicial.Name author in authorsContributors.Keys.ToList())
                {
                    authorsContributors[author].Remove(contributor.Key);
                    if (authorsContributors[author].Count == 0)
                    {
                        authorsContributors.Remove(author);
                    }
                }
            }
            foreach (Models.Inicial.Name author in authorsContributors.Keys)
            {
                float max = authorsContributors[author].Select(x => x.Value).Max();
                List<Models.Inicial.Contributor> contributorsMax = authorsContributors[author].Where(x => x.Value == max).Select(x => x.Key).ToList();
                if (contributorsMax.Count == 1 && !string.IsNullOrEmpty(contributorsMax[0].name.orcid_id))
                {
                    authorsORCID[author] = contributorsMax[0].name.orcid_id;
                }
            }
            return authorsORCID;
        }

        /// <summary>
        /// Genera nombres con diferentes ordenaciones
        /// </summary>
        /// <param name="nombre">Nombre de entrada</param>
        /// <returns>Nombres</returns>
        private static List<string> OrdenacionesNombre(string nombre)
        {
            List<string> ordenaciones = new List<string>();
            ordenaciones.Add(nombre);
            if (nombre.Contains(","))
            {
                string primeraparte = nombre.Substring(nombre.IndexOf(",") + 1).Trim();
                string segundaparte = nombre.Substring(0, nombre.IndexOf(",")).Trim();
                string nombre2 = primeraparte + " " + segundaparte;
                ordenaciones.Add(nombre2);
            }
            return ordenaciones;
        }

        /// <summary>
        /// Obtiene un ScoreE al comparar nombres
        /// </summary>
        /// <param name="nombresA">Nombres A</param>
        /// <param name="nombresB">Nombres B</param>
        /// <returns>Score</returns>
        private static float CompararNombres(List<string> nombresA, List<string> nombresB)
        {
            float resultado = 0;
            foreach (string nombreA in nombresA)
            {
                foreach (string nombreB in nombresB)
                {
                    float temp = 0;

                    Regex reg = new Regex("[^a-zA-Z0-9]+");

                    string nombreAAux = nombreA.ToLower();
                    nombreAAux = new string(
                        nombreAAux.Normalize(NormalizationForm.FormD)
                        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                        .ToArray()
                    ).Normalize(NormalizationForm.FormC);

                    string nombreBAux = nombreB.ToLower();
                    nombreBAux = new string(
                        nombreBAux.Normalize(NormalizationForm.FormD)
                        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                        .ToArray()
                    ).Normalize(NormalizationForm.FormC);

                    string[] nombreASplit = nombreAAux.Split(new string[] { " ", "-", ".", "," }, StringSplitOptions.RemoveEmptyEntries);
                    string[] nombreBSplit = nombreBAux.Split(new string[] { " ", "-", ".", "," }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> nombreCorto;
                    List<string> nombreLargo;
                    if (nombreASplit.Count() > nombreBSplit.Count())
                    {
                        nombreCorto = nombreBSplit.ToList();
                        nombreLargo = nombreASplit.ToList();
                    }
                    else
                    {
                        nombreLargo = nombreBSplit.ToList();
                        nombreCorto = nombreASplit.ToList();
                    }
                    bool ok = true;
                    bool omitidaInicial = false;
                    int index = 0;
                    foreach (string palabraNombreCorto in nombreCorto)
                    {
                        bool encontrado = false;
                        List<string> nombreLargoAux = nombreLargo.GetRange(index, nombreLargo.Count - index);
                        int sum = 0;
                        foreach (string palabraNombreLargo in nombreLargoAux)
                        {
                            sum++;
                            if (palabraNombreCorto.Length == 1 || palabraNombreLargo.Length == 1)
                            {
                                if (palabraNombreCorto[0] == palabraNombreLargo[0])
                                {
                                    encontrado = true;
                                    index += sum;
                                    temp += 0.5f;
                                    break;
                                }
                                else if (!omitidaInicial && palabraNombreCorto.Length == 1)
                                {
                                    omitidaInicial = true;
                                    encontrado = true;
                                    break;
                                }
                            }
                            else
                            {
                                int distancia = LevenshteinDistance(palabraNombreCorto, palabraNombreLargo, out float aux);
                                if (distancia <= 1)
                                {
                                    encontrado = true;
                                    index += sum;
                                    if (distancia == 0)
                                    {
                                        temp += 1f;
                                    }
                                    else
                                    {
                                        temp += 0.5f;
                                    }
                                    break;
                                }
                            }
                        }
                        ok = ok && encontrado;
                    }
                    if (ok)
                    {
                        float nombreCortoLength = nombreCorto.Count;
                        if ((nombreCortoLength == 1 && temp >= 1) ||
                            (nombreCortoLength == 2 && temp >= 1.5f) ||
                            (nombreCortoLength > 2 && temp + 1 >= nombreCortoLength))
                        {
                            resultado = Math.Max(resultado, temp);
                        }
                    }
                }
            }
            return resultado;
        }

        /// <summary>
        /// Computa la distancia de Levenshtein entre dos textos (https://es.wikipedia.org/wiki/Distancia_de_Levenshtein).
        /// </summary>
        /// <param name="str1">Cadena de texto 1</param>
        /// <param name="str2">Cadena de texto 2</param>
        /// <param name="similarity">Similaridad de los textos (entre 0 y 1)</param>
        /// <returns>Distancia de Levenshtein</returns>
        public static int LevenshteinDistance(string str1, string str2, out float similarity)
        {
            similarity = 0;

            // d es una tabla con m+1 renglones y n+1 columnas
            int costo = 0;
            int m = str1.Length;
            int n = str2.Length;
            int[,] d = new int[m + 1, n + 1];

            // Verifica que exista algo que comparar
            if (n == 0) return m;
            if (m == 0) return n;

            // Llena la primera columna y la primera fila.
            for (int i = 0; i <= m; d[i, 0] = i++) ;
            for (int j = 0; j <= n; d[0, j] = j++) ;


            /// recorre la matriz llenando cada unos de los pesos.
            /// i columnas, j renglones
            for (int i = 1; i <= m; i++)
            {
                // recorre para j
                for (int j = 1; j <= n; j++)
                {
                    /// si son iguales en posiciones equidistantes el peso es 0
                    /// de lo contrario el peso suma a uno.
                    costo = str1[i - 1] == str2[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1,  //Eliminacion
                                  d[i, j - 1] + 1),                             //Insercion 
                                  d[i - 1, j - 1] + costo);                     //Sustitucion
                }
            }

            /// Calculamos el porcentaje de cambios en la palabra.
            similarity = 1 - d[m, n] / Math.Max(str1.Length, (float)str2.Length);
            return d[m, n];
        }

        /// <summary>
        /// Obtiene los datos de la revista.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener los datos de la revista.</param>
        /// <returns>Revista.</returns>
        public Source getJournal(PublicacionInicial pPublicacionIn)
        {
            Source revista = new Source();

            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.titles != null && pPublicacionIn.static_data.summary.titles.title != null)
            {
                foreach (Title title in pPublicacionIn.static_data.summary.titles.title)
                {
                    if (title.type == "source")
                    {
                        if( title.content.Count > 0)
                        {
                            revista.name = title.content[0];
                        }
                    }
                }
            }

            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.pub_info != null && !string.IsNullOrEmpty(pPublicacionIn.static_data.summary.pub_info.pubtype))
            {
                switch (pPublicacionIn.static_data.summary.pub_info.pubtype)
                {
                    case JOURNAL:
                        revista.type = JOURNAL;
                        break;
                    case BOOK:
                        revista.type = BOOK;
                        break;
                    default:
                        revista.type = JOURNAL;
                        break;
                }
            }

            if (pPublicacionIn.dynamic_data != null && pPublicacionIn.dynamic_data.cluster_related != null && pPublicacionIn.dynamic_data.cluster_related.identifiers != null && pPublicacionIn.dynamic_data.cluster_related.identifiers.identifier != null && pPublicacionIn.dynamic_data.cluster_related.identifiers.identifier.Any())
            {
                foreach (Identificadores item in pPublicacionIn.dynamic_data.cluster_related.identifiers.identifier)
                {
                    if (item.type == "issn")
                    {
                        revista.issn = new List<string>() { item.value };
                    }

                    if (item.type == "eissn")
                    {
                        revista.eissn = item.value;
                    }
                }
            }

            if (!string.IsNullOrEmpty(revista.name) || (revista.issn != null && revista.issn.Any()) || !string.IsNullOrEmpty(revista.eissn))
            {
                return revista;
            }

            return null;
        }

        /// <summary>
        /// Obtiene la lista de las métricas.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener la métrica.</param>
        /// <returns>Métrica.</returns>
        public List<PublicationMetric> getPublicationMetric(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.dynamic_data != null && pPublicacionIn.dynamic_data.citation_related != null && pPublicacionIn.dynamic_data.citation_related.tc_list != null && pPublicacionIn.dynamic_data.citation_related.tc_list.silo_tc != null && pPublicacionIn.dynamic_data.citation_related.tc_list.silo_tc.coll_id == "WOS")
            {
                List<PublicationMetric> listaMetricas = new List<PublicationMetric>();
                PublicationMetric metrica = new PublicationMetric();
                metrica.metricName = "WoS";
                metrica.citationCount = pPublicacionIn.dynamic_data.citation_related.tc_list.silo_tc.local_count.ToString();
                listaMetricas.Add(metrica);

                return listaMetricas;
            }

            return null;
        }

        /// <summary>
        /// Obtiene si es Open Access o no.
        /// </summary>
        /// <param name="pPublicacionIn"></param>
        /// <returns></returns>
        public bool? getOpenAccess(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.pub_info != null && !string.IsNullOrEmpty(pPublicacionIn.static_data.summary.pub_info.journal_oas_gold))
            {
                if (pPublicacionIn.static_data.summary.pub_info.journal_oas_gold == "S")
                {
                    return true;
                }

                if (pPublicacionIn.static_data.summary.pub_info.journal_oas_gold == "N")
                {
                    return false;
                }
            }

            return null;
        }

        public string getVolume(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.pub_info != null && !string.IsNullOrEmpty(pPublicacionIn.static_data.summary.pub_info.vol))
            {
                return pPublicacionIn.static_data.summary.pub_info.vol;
            }

            return null;
        }
    }
}