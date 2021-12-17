using System.Collections.Generic;
using WoSConnect.ROs.WoS.Models;
using WoSConnect.ROs.WoS.Models.Inicial;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Data;
using Newtonsoft.Json;
using System.IO;
//using Excel = Microsoft.Office.Interop.Excel;


namespace WoSConnect.ROs.WoS.Controllers
{
    public class ROWoSControllerJSON //: //ROScopusLogic
    {
        public List<string> advertencia = null;
        public ROWoSLogic WoSLogic;
        public ROWoSControllerJSON(ROWoSLogic WoSLogic)
        {
            this.WoSLogic = WoSLogic;

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

            if (objInicial != null)
            {
                publicacion.typeOfPublication = getType(objInicial);
                if (publicacion.typeOfPublication != null)
                {
                    publicacion.IDs = getIDs(objInicial);
                    publicacion.title = getTitle(objInicial);
                    publicacion.Abstract = getAbstract(objInicial);
                    publicacion.language = getLanguage(objInicial);
                    publicacion.doi = getDoi(objInicial);
                    if (publicacion.typeOfPublication == "Journal Article" & publicacion.doi == null)
                    {
                        //edventencia! 
                        string ad_text = "No hay doi, sin embargo es un Journal Article.";
                        if (this.advertencia == null)
                        {
                            List<string> ad_list = new List<string>();
                            ad_list.Add(ad_text);
                            this.advertencia = ad_list;
                        }
                        else
                        {
                            this.advertencia.Add(ad_text);
                        }
                    }
                    //publicacion.url = getLinks(objInicial);
                    publicacion.dataIssued = getDate(objInicial);
                    if (publicacion.dataIssued == null)
                    {
                        //edventencia! 
                        string ad_text = "No hay fecha de publicacion.";
                        if (this.advertencia == null)
                        {
                            List<string> ad_list = new List<string>();
                            ad_list.Add(ad_text);
                            this.advertencia = ad_list;
                        }
                        else
                        {
                            this.advertencia.Add(ad_text);
                        }
                    }
                    publicacion.pageStart = getPageStart(objInicial);
                    publicacion.pageEnd = getPageEnd(objInicial);
                    //todo! falta el siitio de la conferencia! 
                    publicacion.hasKnowledgeAreas = getKnowledgeAreas(objInicial);
                    publicacion.freetextKeywords = getFreetextKeyword(objInicial);
                    //publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
                    publicacion.seqOfAuthors = getAuthors(objInicial);
                    if (publicacion.seqOfAuthors == null)
                    {
                        //edventencia! 
                        string ad_text = "No hay conjunto de autores.";
                        if (this.advertencia == null)
                        {
                            List<string> ad_list = new List<string>();
                            ad_list.Add(ad_text);
                            this.advertencia = ad_list;
                        }
                        else
                        {
                            this.advertencia.Add(ad_text);
                        }
                    }
                    publicacion.correspondingAuthor = publicacion.seqOfAuthors[0];
                    if (publicacion.correspondingAuthor == null)
                    {
                        //edventencia! 
                        string ad_text = "No hay autor principal.";
                        if (this.advertencia == null)
                        {
                            List<string> ad_list = new List<string>();
                            ad_list.Add(ad_text);
                            this.advertencia = ad_list;
                        }
                        else
                        {
                            this.advertencia.Add(ad_text);
                        }
                    }
                    publicacion.hasPublicationVenue = getJournal(objInicial);
                    publicacion.hasMetric = getPublicationMetric(objInicial);

                    return publicacion;
                }
                else { return null; }
            }
            else
            {
                return null;
            }

        }

        public string getType(PublicacionInicial objInicial)
        {

            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.summary != null)
                {
                    if (objInicial.static_data.summary.doctypes != null)
                    {
                        if (objInicial.static_data.summary.doctypes.doctype != null)
                        {
                            bool esLista = false;
                            try
                            {
                                if (objInicial.static_data.summary.doctypes.doctype.ToString().Trim().StartsWith("[") && objInicial.static_data.summary.doctypes.doctype.ToString().Trim().EndsWith("]"))
                                {
                                    JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.static_data.summary.doctypes.doctype.ToString());
                                    esLista = true;
                                    List<string> types = new List<string>();
                                    for (int i = 0; i < hey.Count; i++)
                                    {
                                        //return "problema_a_solucionar";
                                        string typeWoS = hey[i].ToString();
                                        if (typeWoS == "Article")
                                        {
                                            types.Add("Journal Article");
                                        }
                                        if (typeWoS == "Book")
                                        {
                                            types.Add("Book");
                                        }
                                        if (typeWoS == "Book Chapter")
                                        {
                                            types.Add("Chapter");
                                        }
                                        if (typeWoS == "Proceedings Paper")
                                        {
                                            types.Add("Conference Paper");
                                        }
                                    }
                                    if (types.Count > 1)
                                    {
                                        //obtener las etiquetas juntas para definirlas
                                        string types_merge = "";
                                        foreach (string type in types)
                                        {
                                            types_merge = types_merge + type + ";";
                                        }
                                        //definir el problema! 
                                        if (this.advertencia == null)
                                        {
                                            List<string> ad = new List<string>();
                                            ad.Add("Problema con el tipo de articulo. Los diferentes tipos obtenidos son los siguientes: " + types_merge);
                                            this.advertencia = ad;
                                        }
                                        else
                                        {
                                            this.advertencia.Add("Problema con el tipo de articulo. Los diferentes tipos obtenidos son los siguientes: " + types_merge);
                                        }
                                        return types_merge;

                                    }
                                    else if (types.Count == 0)
                                    {
                                        return null;
                                    }
                                    else { return types[0]; }
                                }
                            }
                            catch
                            {


                            }
                            if (!esLista)
                            {
                                string typeWoS = objInicial.static_data.summary.doctypes.doctype.ToString();
                                if (typeWoS == "Article")
                                {
                                    return "Journal Article";
                                }
                                else if (typeWoS == "Book")
                                {
                                    return "Book";
                                }
                                else if (typeWoS == "Book Chapter")
                                {
                                    return "Chapter";
                                }
                                else if (typeWoS == "Proceedings Paper")
                                {
                                    return "Conference Paper";
                                }
                                else { return null; }
                            }

                        }
                    }
                }
            }
            return null;

        }
        public List<string> getIDs(PublicacionInicial objInicial)
        {
            if (objInicial.UID != null)
            {
                List<string> ids = new List<string>();
                ids.Add(objInicial.UID);
                return ids;
            }
            return null;
        }

        public string getTitle(PublicacionInicial objInicial)
        {
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.summary != null)
                {
                    if (objInicial.static_data.summary.titles != null)
                    {
                        if (objInicial.static_data.summary.titles.title != null)
                        {
                            foreach (Title title in objInicial.static_data.summary.titles.title)
                            {
                                if (title.type == "item")
                                {
                                    return title.content;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public string getAbstract(PublicacionInicial objInicial)
        {
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.fullrecord_metadata != null)
                {
                    if (objInicial.static_data.fullrecord_metadata.abstracts != null)
                    {
                        if (objInicial.static_data.fullrecord_metadata.abstracts.@abstract != null)
                        {
                            if (objInicial.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text != null)
                            {
                                bool esLista = false;
                                try
                                {
                                    if (objInicial.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text.ToString().Trim().StartsWith("[") && objInicial.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text.ToString().Trim().EndsWith("]"))
                                    {
                                        AbstractText hey = JsonConvert.DeserializeObject<AbstractText>(objInicial.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text.ToString());
                                        esLista = true;
                                        if (hey.p != null)
                                        {
                                            return hey.p;
                                        }
                                    }
                                }
                                catch
                                {

                                }

                                if (!esLista)
                                {
                                    try
                                    {
                                        AbstractText_list hey = JsonConvert.DeserializeObject<AbstractText_list>(objInicial.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text.ToString());

                                        string abstract_2 = "";
                                        string advertencia = "Hay un problema con el abstract: se han encontrado varios, son los siguientes:\n ";
                                        for (int i = 0; i < hey.p.Count; i++)
                                        {
                                            if (i == 0)
                                            {
                                                abstract_2 = hey.p[i];
                                            }
                                            advertencia = advertencia + " Abstract: " + hey.p[i] + " \n.";
                                        }
                                        if (this.advertencia == null)
                                        {
                                            List<string> ad = new List<string>();
                                            ad.Add(advertencia);
                                            this.advertencia = ad;
                                        }
                                        else
                                        {
                                            this.advertencia.Add(advertencia);
                                        }
                                        return abstract_2;
                                    }
                                    catch (Exception)
                                    {
                                        return "";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public string getLanguage(PublicacionInicial objInicial)
        {
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.fullrecord_metadata != null)
                {
                    if (objInicial.static_data.fullrecord_metadata.languages != null)
                    {
                        if (objInicial.static_data.fullrecord_metadata.languages.language != null)
                        {
                            if (objInicial.static_data.fullrecord_metadata.languages.language.content != null)
                            {
                                return objInicial.static_data.fullrecord_metadata.languages.language.content;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public string getDoi(PublicacionInicial objInicial)
        {
            if (objInicial.dynamic_data != null)
            {
                if (objInicial.dynamic_data.cluster_related != null)
                {
                    if (objInicial.dynamic_data.cluster_related.identifiers != null)
                    {
                        if (objInicial.dynamic_data.cluster_related.identifiers.identifier != null)
                        {
                            bool esLista = false;
                            try
                            {
                                if (objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString().Trim().StartsWith("[") && objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString().Trim().EndsWith("]"))
                                {
                                    JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString());
                                    esLista = true;
                                    foreach (JContainer var in hey)
                                    {
                                        Identifier identifier = JsonConvert.DeserializeObject<Identifier>(var.ToString());

                                        if (identifier.type == "doi")
                                        {
                                            if (identifier.value.Contains("https://doi.org/"))
                                            {
                                                int indice = identifier.value.IndexOf("org/");
                                                return identifier.value.Substring(indice + 4);

                                            }
                                            else
                                            {
                                                return identifier.value;
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {

                            }
                            if (!esLista)
                            {
                                Identifier identifier = JsonConvert.DeserializeObject<Identifier>(objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString());
                                if (identifier.type == "doi")
                                {
                                    return identifier.value;
                                }
                            }
                        }


                    }
                }
            }
            return null;
        }
       

        public DateTimeValue getDate(PublicacionInicial objInicial)
        {
            DateTimeValue date = new DateTimeValue();
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.summary != null)
                {
                    if (objInicial.static_data.summary.pub_info != null)
                    {
                        if (objInicial.static_data.summary.pub_info.sortdate != null)
                        {
                            date.datimeTime = objInicial.static_data.summary.pub_info.sortdate;
                            return date;
                        }
                    }
                }
            }
            return null;
        }

        public string getPageStart(PublicacionInicial objInicial)
        {
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.summary != null)
                {
                    if (objInicial.static_data.summary.pub_info != null)
                    {
                        if (objInicial.static_data.summary.pub_info.page != null)
                        {
                            if (objInicial.static_data.summary.pub_info.page.begin != null)
                            {
                                return objInicial.static_data.summary.pub_info.page.begin.ToString();
                            }
                        }
                    }
                }
            }
            return null;
        }

        public string getPageEnd(PublicacionInicial objInicial)
        {
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.summary != null)
                {
                    if (objInicial.static_data.summary.pub_info != null)
                    {
                        if (objInicial.static_data.summary.pub_info.page != null)
                        {
                            if (objInicial.static_data.summary.pub_info.page.end != null)
                            {
                                return objInicial.static_data.summary.pub_info.page.end.ToString();
                            }
                        }
                    }
                }
            }
            return null;
        }

        public List<KnowledgeAreas> getKnowledgeAreas(PublicacionInicial objInicial)
        {

            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.fullrecord_metadata != null)
                {
                    if (objInicial.static_data.fullrecord_metadata.category_info != null)
                    {
                        if (objInicial.static_data.fullrecord_metadata.category_info.subjects != null)
                        {
                            if (objInicial.static_data.fullrecord_metadata.category_info.subjects.subject != null)
                            {
                                List<KnowledgeArea> list = new List<KnowledgeArea>();
                                List<KnowledgeAreas> result = new List<KnowledgeAreas>();
                                KnowledgeAreas info_woS = new KnowledgeAreas();
                                List<string> names_areas = new List<string>();
                                List<bool> booleans_code_areas = new List<bool>();
                                foreach (Subject sub in objInicial.static_data.fullrecord_metadata.category_info.subjects.subject)
                                {

                                    if (sub.content != null & sub.ascatype == "traditional")
                                    {
                                        KnowledgeArea area = new KnowledgeArea();
                                        if (names_areas.Count == 0 || !names_areas.Contains(sub.content))
                                        {
                                            area.name = sub.content;
                                            names_areas.Add(sub.content);
                                            if (sub.code != null)
                                            {

                                                area.hasCode = sub.code;
                                                booleans_code_areas.Add(true);
                                            }
                                            else { booleans_code_areas.Add(false); }
                                            if (area != null)
                                            {
                                                list.Add(area);
                                            }
                                        }
                                        else if (sub.code != null)
                                        {
                                            for (int i = 0; i < list.Count; i++)
                                            {
                                                if (list[i].name == sub.content)
                                                {
                                                    list[i].hasCode = sub.code;
                                                }
                                            }
                                        }


                                    }

                                }
                                if (list != null)
                                {
                                    info_woS.resource = "WoS";
                                    info_woS.knowledgeArea = list;
                                    result.Add(info_woS);
                                    KnowledgeAreas taxonomia = recuperar_taxonomia(info_woS);
                                    if (taxonomia != null)
                                    {
                                        result.Add(taxonomia);
                                    }
                                    return result;
                                }
                            }
                        }
                    }
                }
            }
            return new List<KnowledgeAreas>();
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
                    Console.Write(area_wos_obtenida.name);
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

        public List<FreetextKeywords> getFreetextKeyword(PublicacionInicial objInicial)
        {
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.fullrecord_metadata != null)
                {
                    if (objInicial.static_data.fullrecord_metadata.keywords != null)
                    {
                        try
                        {
                            Keywords hey = JsonConvert.DeserializeObject<Keywords>(objInicial.static_data.fullrecord_metadata.keywords.ToString());
                            FreetextKeywords list = new FreetextKeywords();
                            list.freetextKeyword = hey.keyword;
                            list.source = "WoS";
                            List<FreetextKeywords> sol_list = new List<FreetextKeywords>();
                            sol_list.Add(list);
                            return sol_list;
                        }
                        catch
                        {
                            //en este caso solo hay un palabra y hay que meterlo en una lista para eso! 
                            List<string> sol = new List<string>();
                            Keywords_2 hey = JsonConvert.DeserializeObject<Keywords_2>(objInicial.static_data.fullrecord_metadata.keywords.ToString());
                            sol.Add(hey.keyword);
                            FreetextKeywords list = new FreetextKeywords();
                            list.freetextKeyword = sol;
                            list.source = "WoS";
                            List<FreetextKeywords> sol_list = new List<FreetextKeywords>();
                            sol_list.Add(list);
                            return sol_list;

                        }
                    }
                }
            }
            return null;
        }

        
        public List<Person> getAuthors(PublicacionInicial objInicial)
        {
            List<Person> result = new List<Person>();
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.summary != null)
                {
                    if (objInicial.static_data.summary.names != null)
                    {
                        if (objInicial.static_data.summary.names.name != null)
                        {
                            try
                            {
                                JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.static_data.summary.names.name.ToString());
                                foreach (JContainer var in hey)
                                {
                                    Person persona = new Person();
                                    string orcid = null;
                                    string name = null;
                                    string familia = null;
                                    string completo = null;
                                    string ids = null; ;
                                    string links = null;

                                    try
                                    {
                                        Name_2 ee = JsonConvert.DeserializeObject<Name_2>(var.ToString());
                                        if (ee.orcid_id != null)
                                        {
                                            if (ee.orcid_id.Contains("https://orcid.org/") || ee.orcid_id.Contains("http://orcid.org/"))
                                            {
                                                int indice = ee.orcid_id.IndexOf("org/");
                                                persona.ORCID = ee.orcid_id.Substring(indice + 4);
                                                orcid = ee.orcid_id.Substring(indice + 4);
                                            }
                                            else
                                            {
                                                persona.ORCID = ee.orcid_id;
                                                orcid = ee.orcid_id;
                                            }
                                        }
                                        List<string> nombres = new List<string>();
                                        List<string> apellidos = new List<string>();
                                        List<string> nombres_completo = new List<string>();

                                        if (ee.display_name != null)
                                        {
                                            if (!nombres_completo.Contains(ee.display_name) && !ee.full_name.Contains("IEEE"))
                                            {
                                                nombres_completo.Add(ee.display_name);
                                                completo = ee.display_name;
                                            }
                                        }
                                        if (ee.first_name != null)
                                        {
                                            if (!nombres.Contains(ee.first_name))
                                            {
                                                nombres.Add(ee.first_name);
                                                name = ee.first_name;
                                            }
                                        }
                                        if (ee.full_name != null)
                                        {
                                            if (!nombres_completo.Contains(ee.full_name) && !ee.full_name.Contains("IEEE"))
                                            {
                                                nombres_completo.Add(ee.full_name);
                                                if (completo != null)
                                                {
                                                    completo = completo + "*" + ee.first_name;
                                                }
                                            }
                                        }
                                        if (ee.last_name != null)
                                        {
                                            if (!apellidos.Contains(ee.last_name))
                                            {
                                                apellidos.Add(ee.last_name);
                                                familia = ee.last_name;
                                            }
                                        }
                                        if (nombres.Count > 0 || apellidos.Count > 0 || nombres_completo.Count > 0)
                                        {
                                            Name nombre = new Name();
                                            if (apellidos.Count > 0)
                                            {
                                                nombre.familia = apellidos;
                                            }
                                            if (nombres.Count > 0)
                                            {
                                                nombre.given = nombres;
                                            }
                                            if (nombres_completo.Count > 0)
                                            {
                                                nombre.nombre_completo = nombres_completo;
                                            }
                                            persona.name = nombre;

                                        }
                                        result.Add(persona);
                                    }
                                    catch
                                    {
                                        Name_1 ee = JsonConvert.DeserializeObject<Name_1>(var.ToString());

                                        List<string> nombres = new List<string>();
                                        List<string> apellidos = new List<string>();
                                        List<string> nombres_completo = new List<string>();

                                        if (ee.display_name != null)
                                        {
                                            if (!nombres_completo.Contains(ee.display_name) && !ee.full_name.Contains("IEEE"))
                                            {
                                                nombres_completo.Add(ee.display_name);
                                                completo = ee.display_name;
                                            }
                                        }
                                        if (ee.first_name != null)
                                        {
                                            if (!nombres.Contains(ee.first_name))
                                            {
                                                nombres.Add(ee.first_name);
                                                name = ee.first_name;
                                            }
                                        }
                                        if (ee.full_name != null)
                                        {
                                            if (!nombres_completo.Contains(ee.full_name) && !ee.full_name.Contains("IEEE"))
                                            {

                                                nombres_completo.Add(ee.full_name);
                                                if (completo != null)
                                                {
                                                    completo = completo + "*" + ee.first_name;
                                                }
                                            }
                                        }
                                        if (ee.last_name != null)
                                        {
                                            if (!apellidos.Contains(ee.last_name))
                                            {
                                                apellidos.Add(ee.last_name);
                                                familia = ee.last_name;
                                            }
                                        }
                                        if (nombres.Count > 0 || apellidos.Count > 0 || nombres_completo.Count > 0)
                                        {
                                            Name nombre = new Name();
                                            if (apellidos.Count > 0)
                                            {
                                                nombre.familia = apellidos;
                                            }
                                            if (nombres.Count > 0)
                                            {
                                                nombre.given = nombres;
                                            }
                                            if (nombres_completo.Count > 0)
                                            {
                                                nombre.nombre_completo = nombres_completo;
                                            }
                                            persona.name = nombre;

                                        }
                                        result.Add(persona);
                                    }


                                }
                                return result;
                            }
                            catch
                            {
                                try
                                {
                                    Name_2 ee = JsonConvert.DeserializeObject<Name_2>(objInicial.static_data.summary.names.name.ToString());
                                    Person persona = new Person();
                                    if (ee.orcid_id != null)
                                    {
                                        persona.ORCID = ee.orcid_id;
                                    }

                                    List<string> nombres = new List<string>();
                                    List<string> apellidos = new List<string>();
                                    List<string> nombres_completo = new List<string>();

                                    if (ee.display_name != null)
                                    {
                                        if (!nombres_completo.Contains(ee.display_name))
                                        {
                                            nombres_completo.Add(ee.display_name);
                                        }
                                    }
                                    if (ee.first_name != null)
                                    {
                                        if (!nombres.Contains(ee.first_name))
                                        {
                                            nombres.Add(ee.first_name);
                                        }
                                    }
                                    if (ee.full_name != null)
                                    {
                                        if (!nombres_completo.Contains(ee.full_name))
                                        {
                                            nombres_completo.Add(ee.full_name);
                                        }
                                    }
                                    if (ee.last_name != null)
                                    {
                                        if (!apellidos.Contains(ee.last_name))
                                        {
                                            apellidos.Add(ee.last_name);
                                        }
                                    }
                                    if (nombres.Count > 0 || apellidos.Count > 0 || nombres_completo.Count > 0)
                                    {
                                        Name nombre = new Name();
                                        if (apellidos.Count > 0)
                                        {
                                            nombre.familia = apellidos;
                                        }
                                        if (nombres.Count > 0)
                                        {
                                            nombre.given = nombres;
                                        }
                                        if (nombres_completo.Count > 0)
                                        {
                                            nombre.nombre_completo = nombres_completo;
                                        }
                                        persona.name = nombre;

                                    }
                                    result.Add(persona);

                                    return result;
                                }
                                catch
                                {
                                    Name_1 ee = JsonConvert.DeserializeObject<Name_1>(objInicial.static_data.summary.names.name.ToString());
                                    Person persona = new Person();


                                    List<string> nombres = new List<string>();
                                    List<string> apellidos = new List<string>();
                                    List<string> nombres_completo = new List<string>();

                                    if (ee.display_name != null)
                                    {
                                        if (!nombres_completo.Contains(ee.display_name))
                                        {
                                            nombres_completo.Add(ee.display_name);
                                        }
                                    }
                                    if (ee.first_name != null)
                                    {
                                        if (!nombres.Contains(ee.first_name))
                                        {
                                            nombres.Add(ee.first_name);
                                        }
                                    }
                                    if (ee.full_name != null)
                                    {
                                        if (!nombres_completo.Contains(ee.full_name))
                                        {
                                            nombres_completo.Add(ee.full_name);
                                        }
                                    }
                                    if (ee.last_name != null)
                                    {
                                        if (!apellidos.Contains(ee.last_name))
                                        {
                                            apellidos.Add(ee.last_name);
                                        }
                                    }
                                    if (nombres.Count > 0 || apellidos.Count > 0 || nombres_completo.Count > 0)
                                    {
                                        Name nombre = new Name();
                                        if (apellidos.Count > 0)
                                        {
                                            nombre.familia = apellidos;
                                        }
                                        if (nombres.Count > 0)
                                        {
                                            nombre.given = nombres;
                                        }
                                        if (nombres_completo.Count > 0)
                                        {
                                            nombre.nombre_completo = nombres_completo;
                                        }
                                        persona.name = nombre;

                                    }
                                    result.Add(persona);
                                    return result;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public Source getJournal(PublicacionInicial objInicial)
        {
            Source revista = new Source();
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.summary != null)
                {
                    if (objInicial.static_data.summary.titles != null)
                    {
                        if (objInicial.static_data.summary.titles.title != null)
                        {
                            foreach (Title title in objInicial.static_data.summary.titles.title)
                            {
                                if (title.type == "source")
                                {
                                    revista.name = title.content;
                                }
                            }
                        }
                    }
                }
            }
            if (objInicial.static_data != null)
            {
                if (objInicial.static_data.summary != null)
                {
                    if (objInicial.static_data.summary.pub_info != null)
                    {
                        if (objInicial.static_data.summary.pub_info.pubtype != null)
                        {
                            if (objInicial.static_data.summary.pub_info.pubtype == "Journal")
                            {
                                revista.type = "Journal";
                            }
                            else if (objInicial.static_data.summary.pub_info.pubtype == "Book")
                            {
                                revista.type = "Book";
                            }
                            else
                            {
                                //recogida de errores! 
                                string ad = "No se ha identidicado el tipo de recurso en el que esta publicado";
                                if (this.advertencia != null)
                                {
                                    this.advertencia.Add(ad);
                                }
                                else
                                {
                                    List<string> advertencias = new List<string>();
                                    advertencias.Add(ad);
                                    this.advertencia = advertencias;
                                }
                            }
                        }
                    }
                }
            }
            if (objInicial.dynamic_data != null)
            {
                if (objInicial.dynamic_data.cluster_related != null)
                {
                    if (objInicial.dynamic_data.cluster_related.identifiers != null)
                    {
                        if (objInicial.dynamic_data.cluster_related.identifiers.identifier != null)
                        {
                            bool esLista = false;
                            try
                            {
                                if (objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString().Trim().StartsWith("[") && objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString().EndsWith("]"))
                                {
                                    JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString());
                                    esLista = true;
                                    foreach (JContainer var in hey)
                                    {
                                        Identifier identifier = JsonConvert.DeserializeObject<Identifier>(var.ToString());

                                        if (identifier.type == "issn")
                                        {
                                            List<string> issn = new List<string>();
                                            issn.Add(identifier.value);
                                            revista.issn = issn;
                                        }
                                        if (identifier.type == "eissn")
                                        {
                                            revista.eissn = identifier.value;
                                        }
                                        if (identifier.type == "isbn")
                                        {
                                            List<string> isbn = new List<string>();
                                            isbn.Add(identifier.value);
                                            revista.issn = isbn;
                                            revista.isbn = isbn;
                                        }
                                    }
                                }
                            }
                            catch
                            {

                            }

                            if (!esLista)
                            {
                                Identifier identifier = JsonConvert.DeserializeObject<Identifier>(objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString());
                                if (identifier.type == "issn")
                                {
                                    List<string> issn = new List<string>();
                                    issn.Add(identifier.value);
                                    revista.issn = issn;
                                }
                                if (identifier.type == "eissn")
                                {
                                    revista.eissn = identifier.value;
                                }

                                if (identifier.type == "isbn")
                                {
                                    List<string> isbn = new List<string>();
                                    isbn.Add(identifier.value);
                                    revista.issn = isbn;
                                    revista.isbn = isbn;
                                }
                            }
                        }
                    }
                }
            }
            if (revista != new Source())
            {
                return revista;
            }
            else { return null; }
        }

        public List<PublicationMetric> getPublicationMetric(PublicacionInicial objInicial)
        {
            List<PublicationMetric> metricList = new List<PublicationMetric>();
            PublicationMetric metricPublicacion = new PublicationMetric();
            metricPublicacion.metricName = "WoS";
            if (objInicial.dynamic_data != null)
            {
                if (objInicial.dynamic_data.citation_related != null)
                {
                    if (objInicial.dynamic_data.citation_related.tc_list != null)
                    {
                        if (objInicial.dynamic_data.citation_related.tc_list.silo_tc != null)
                        {
                            if (objInicial.dynamic_data.citation_related.tc_list.silo_tc.local_count != null)
                            {
                                metricPublicacion.citationCount = objInicial.dynamic_data.citation_related.tc_list.silo_tc.local_count.ToString();
                                metricList.Add(metricPublicacion);
                                return metricList;

                            }
                        }
                    }
                }
            }

            return null;
        }


    }
}