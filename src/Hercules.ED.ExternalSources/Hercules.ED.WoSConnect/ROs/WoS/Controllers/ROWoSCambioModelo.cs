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

namespace WoSConnect.ROs.WoS.Controllers
{
    public class ROWoSControllerJSON
    {
        public List<string> advertencia = null;
        public ROWoSLogic WoSLogic;

        public ROWoSControllerJSON(ROWoSLogic WoSLogic)
        {
            this.WoSLogic = WoSLogic;
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
        public Publication getPublicacionCita(PublicacionInicial pPublicacionIn)
        {
            Publication publicacionFinal = new Publication();
            publicacionFinal.typeOfPublication = getType(pPublicacionIn);
            publicacionFinal.IDs = getWosID(pPublicacionIn);
            publicacionFinal.title = getTitle(pPublicacionIn);
            publicacionFinal.Abstract = getAbstract(pPublicacionIn);
            publicacionFinal.doi = getDoi(pPublicacionIn);
            publicacionFinal.dataIssued = getDate(pPublicacionIn);
            publicacionFinal.hasKnowledgeAreas = getKnowledgeAreas(pPublicacionIn);
            publicacionFinal.freetextKeywords = getFreetextKeyword(pPublicacionIn);
            publicacionFinal.seqOfAuthors = getAuthors(pPublicacionIn);
            publicacionFinal.correspondingAuthor = publicacionFinal.seqOfAuthors[0];
            publicacionFinal.hasPublicationVenue = getJournal(pPublicacionIn);
            publicacionFinal.hasMetric = getPublicationMetric(pPublicacionIn);

            return publicacionFinal;
        }


        public List<Publication> getListPublicatio(Root objInicial)
        {
            List<Publication> sol = new List<Publication>();
            if (objInicial != null)
            {
                if (objInicial.Data != null)
                {
                    if (objInicial.Data.Records != null)
                    {
                        if (objInicial.Data.Records.records != null)
                        {
                            if (objInicial.Data.Records.records.REC != null)
                            {

                                foreach (PublicacionInicial rec in objInicial.Data.Records.records.REC)
                                {
                                    try
                                    {
                                        if(rec.UID== "WOS:000224385500052")
                                        {

                                        }
                                        Publication publicacion = cambioDeModeloPublicacion(rec, true);

                                        if (publicacion != null)
                                        {
                                            if (this.advertencia != null)
                                            {
                                                publicacion.problema = this.advertencia;
                                                this.advertencia = null;
                                            }
                                            sol.Add(publicacion);
                                        }
                                    }catch(Exception e)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }

            return sol;
        }

        public Publication cambioDeModeloPublicacion(PublicacionInicial objInicial, Boolean publicacion_principal)
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
            publicacion.hasKnowledgeAreas = getKnowledgeAreas(objInicial);
            publicacion.freetextKeywords = getFreetextKeyword(objInicial);
            publicacion.seqOfAuthors = getAuthors(objInicial);
            publicacion.correspondingAuthor = publicacion.seqOfAuthors[0];
            publicacion.hasPublicationVenue = getJournal(objInicial);
            publicacion.hasMetric = getPublicationMetric(objInicial);
            if(publicacion.typeOfPublication==CHAPTER)
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
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.titles != null && pPublicacionIn.static_data.summary.titles.title != null && pPublicacionIn.static_data.summary.titles.title.Any())
            {
                return pPublicacionIn.static_data.summary.titles.title.FirstOrDefault(x => x.type == "item").content;
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
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.fullrecord_metadata != null && pPublicacionIn.static_data.fullrecord_metadata.abstracts != null && pPublicacionIn.static_data.fullrecord_metadata.abstracts.@abstract != null && pPublicacionIn.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text != null && pPublicacionIn.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text.p != null && pPublicacionIn.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text.p.Any())
            {
                string descripcion = string.Empty;

                foreach (string item in pPublicacionIn.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text.p)
                {
                    descripcion += item.Trim() + " ";
                }

                return descripcion.Trim();
            }

            return null;
        }

        /// <summary>
        /// Obtiene el idioma de la publicación.
        /// </summary>
        /// <param name="objInicial">Publicación a obtener el idioma.</param>
        /// <returns>Idioma.</returns>
        public string getLanguage(PublicacionInicial pPublicacionIn)
        {
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.fullrecord_metadata != null && pPublicacionIn.static_data.fullrecord_metadata.languages != null && pPublicacionIn.static_data.fullrecord_metadata.languages.language != null && !string.IsNullOrEmpty(pPublicacionIn.static_data.fullrecord_metadata.languages.language.content))
            {
                return pPublicacionIn.static_data.fullrecord_metadata.languages.language.content.Trim();
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
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data != null && !string.IsNullOrEmpty(pPublicacionIn.static_data.summary.pub_info.sortdate))
            {
                DateTimeValue fecha = new DateTimeValue();
                fecha.datimeTime = pPublicacionIn.static_data.summary.pub_info.sortdate;
                return fecha;
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
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.pub_info != null && pPublicacionIn.static_data.summary.pub_info.page != null && pPublicacionIn.static_data.summary.pub_info.page.begin != null)
            {
                return pPublicacionIn.static_data.summary.pub_info.page.begin.ToString();
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
            if (pPublicacionIn.static_data != null && pPublicacionIn.static_data.summary != null && pPublicacionIn.static_data.summary.pub_info != null && pPublicacionIn.static_data.summary.pub_info.page != null && pPublicacionIn.static_data.summary.pub_info.page.end != null)
            {
                return pPublicacionIn.static_data.summary.pub_info.page.end.ToString();
            }

            return null;
        }

        /// <summary>
        /// Obtiene las areas de conocimiento de la taxonomía unificada.
        /// </summary>
        /// <param name="pPublicacionIn">Publicación a obtener las areas.</param>
        /// <returns>Areas.</returns>
        public List<KnowledgeAreas> getKnowledgeAreas(PublicacionInicial pPublicacionIn)
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
                listaAreas.Add(area);

                // TODO: Comprobar funcionamiento correcto.
                KnowledgeAreas taxonomia = recuperar_taxonomia(area);
                if (taxonomia != null)
                {
                    listaAreas.Add(taxonomia);
                }

                return listaAreas;
            }

            return null;
        }

        public KnowledgeAreas recuperar_taxonomia(KnowledgeAreas areas_wos)
        {
            KnowledgeAreas taxonomia_hercules = new KnowledgeAreas();
            List<KnowledgeArea> listado = new List<KnowledgeArea>();
            taxonomia_hercules.resource = "Hércules";

            List<KnowledgeArea> wos = areas_wos.knowledgeArea;
            foreach (KnowledgeArea area_wos_obtenida in wos)
            {
                KnowledgeArea area_taxonomia = new KnowledgeArea();
                try
                {
                    //en Scopus tambien habra una extepcion de este tipo. 
                    if (area_wos_obtenida.name == "Physics, Fluids & Plasmas")
                    {
                        area_taxonomia.name = "Fluid Dynamics";
                        listado.Add(area_taxonomia);
                        KnowledgeArea area_taxonomia_2 = new KnowledgeArea();
                        area_taxonomia_2.name = "Plasma Physics";
                        listado.Add(area_taxonomia_2);
                    }
                    else
                    {
                        area_taxonomia.name = this.WoSLogic.ds[area_wos_obtenida.name];

                        listado.Add(area_taxonomia);
                    }
                }
                catch
                {
                    Console.Write("No se encuentra enla taxonomia el siguiente area: " + area_wos_obtenida.name);
                    Console.Write("\n");
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
                foreach (Models.Inicial.Name item in pPublicacionIn.static_data.summary.names.name)
                {
                    if (!item.display_name.Contains(" ") || !item.full_name.Contains(" ") || item.display_name.ToLower().Contains("ieee") || item.full_name.ToLower().Contains("ieee") || (string.IsNullOrEmpty(item.first_name) && string.IsNullOrEmpty(item.last_name) && string.IsNullOrEmpty(item.full_name) && string.IsNullOrEmpty(item.orcid_id)))
                    {
                        continue;
                    }

                    Person person = new Person();
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

                    // ORCID
                    if (!string.IsNullOrEmpty(item.orcid_id))
                    {
                        person.ORCID = item.orcid_id;
                    }
                    else if (string.IsNullOrEmpty(item.orcid_id) && !string.IsNullOrEmpty(item.r_id) && pPublicacionIn.static_data.contributors != null && pPublicacionIn.static_data.contributors.contributor != null && pPublicacionIn.static_data.contributors.contributor.Any())
                    {
                        foreach(Contributor itemContributor in pPublicacionIn.static_data.contributors.contributor)
                        {
                            if(itemContributor.name.r_id == item.r_id)
                            {
                                person.ORCID = itemContributor.name.orcid_id;
                            }
                        }
                    }

                    if (person.name.nombre_completo != null || !string.IsNullOrEmpty(person.ORCID))
                    {
                        listaPersonas.Add(person);
                    }
                }

                return listaPersonas;
            }

            return null;
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
                        revista.name = title.content;
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
                    default: // TODO: ¿Por defecto son todas Journal?
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
            if (pPublicacionIn.dynamic_data != null && pPublicacionIn.dynamic_data.citation_related != null & pPublicacionIn.dynamic_data.citation_related.tc_list != null && pPublicacionIn.dynamic_data.citation_related.tc_list.silo_tc != null && pPublicacionIn.dynamic_data.citation_related.tc_list.silo_tc.coll_id == "WOS")
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
    }
}