using System.Collections.Generic;
using WoSConnect.ROs.WoS.Models;
using WoSConnect.ROs.WoS.Models.Inicial;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

using Newtonsoft.Json;


namespace WoSConnect.ROs.WoS.Controllers
{
    public class ROWoSControllerJSON //: //ROScopusLogic
    {
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
                                Console.Write("Cantidad de items devueltos:\n");
                                Console.Write(objInicial.Data.Records.records.REC.Count);
                                

                                foreach (PublicacionInicial rec in objInicial.Data.Records.records.REC)
                                {

                                    //Console.Write("hola");
                                    //try
                                    //{
                                    //    PublicacionInicial hey = JsonConvert.DeserializeObject<PublicacionInicial>(rec.ToString());
                                    Publication publicacion = cambioDeModeloPublicacion(rec, true);
                                    if (publicacion != null)
                                    {
                                        sol.Add(publicacion);
                                    }
                                    //}
                                    //catch
                                    // {
                                    //    Console.Write("Extepción\n");
                                    //TODO aquellos que no siguen el modelo averiguar porque y ver como generalos, de momento se llaman PRocessing la clase... 
                                    //    continue;
                                    // }

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
                    //publicacion.url = getLinks(objInicial);
                    publicacion.dataIssued = getDate(objInicial);
                    publicacion.pageStart = getPageStart(objInicial);
                    publicacion.pageEnd = getPageEnd(objInicial);

                    //publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
                    publicacion.freetextKeyword = getFreetextKeyword(objInicial);
                    //publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
                    publicacion.seqOfAuthors = getAuthors(objInicial);
                    publicacion.correspondingAuthor = publicacion.seqOfAuthors[0];
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
                            try
                            {
                                JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.static_data.summary.doctypes.doctype.ToString());
                                List<string> types = new List<string>();
                                for(int i=0; i<hey.Count;i++){
                                   
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
                                if(types.Count>1){
                                    //todo: problemas!!!
                                    return "problema_a_solucionar";
 
                                }else if (types.Count==0){
                                    return null;
                                }else{return types[0];}

                            }
                            catch
                            {
                                //string hey = JsonConvert.DeserializeObject<string>(objInicial.static_data.summary.doctypes.doctype.ToString());
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
                                try
                                {
                                    AbstractText hey = JsonConvert.DeserializeObject<AbstractText>(objInicial.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text.ToString());
                                    if (hey.p != null)
                                    {
                                        return hey.p;
                                    }
                                }
                                catch
                                {
                                    AbstractText_list hey = JsonConvert.DeserializeObject<AbstractText_list>(objInicial.static_data.fullrecord_metadata.abstracts.@abstract.abstract_text.ToString());
                                    foreach (string var in hey.p)
                                    {
                                        return var;
                                        //TODO aqui si es una lista de abstract solo estoy devolviendo el primero! 
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
                            try
                            {
                                JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString());
                                foreach (JContainer var in hey)
                                {
                                    Identifier identifier = JsonConvert.DeserializeObject<Identifier>(var.ToString());

                                    if (identifier.type == "doi")
                                    {
                                        return identifier.value;
                                    }
                                }
                            }
                            catch
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
        // public List<string> getLinks(PublicacionInicial objInicial)
        // {
        //     return new List<string>();
        // }

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

        // public List<KnowledgeArea> getKnowledgeAreas(PublicacionInicial objInicial)
        // {

        //     return new List<KnowledgeArea>();
        // }

        public List<string> getFreetextKeyword(PublicacionInicial objInicial)
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
                            return hey.keyword;
                        }
                        catch
                        {
                            List<string> sol = new List<string>();

                            Keywords_2 hey = JsonConvert.DeserializeObject<Keywords_2>(objInicial.static_data.fullrecord_metadata.keywords.ToString());
                            sol.Add(hey.keyword);
                            return sol;

                        }
                    }
                }
            }
            return null;
        }

        // public Person getAuthorPrincipal(PublicacionInicial objInicial)
        // {
        //     Person persona = new Person();
        //     return null;
        // }
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

                                    try
                                    {
                                        Name_2 ee = JsonConvert.DeserializeObject<Name_2>(var.ToString());
                                        if (ee.orcid_id != null)
                                        {
                                            persona.ORCID = ee.orcid_id;
                                        }
                                        List<string> nombres = new List<string>();
                                        if (ee.display_name != null)
                                        {
                                            if (!nombres.Contains(ee.display_name))
                                            {
                                                nombres.Add(ee.display_name);
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
                                            if (!nombres.Contains(ee.full_name))
                                            {
                                                nombres.Add(ee.full_name);
                                            }
                                        }
                                        if (ee.last_name != null)
                                        {
                                            if (!nombres.Contains(ee.last_name))
                                            {
                                                nombres.Add(ee.last_name);
                                            }
                                        }
                                        if (nombres.Count != 0)
                                        {
                                            persona.name = nombres;
                                        }
                                        result.Add(persona);

                                    }
                                    catch
                                    {
                                        Name_1 ee = JsonConvert.DeserializeObject<Name_1>(var.ToString());

                                        List<string> nombres = new List<string>();
                                        if (ee.display_name != null)
                                        {
                                            if (!nombres.Contains(ee.display_name))
                                            {
                                                nombres.Add(ee.display_name);
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
                                            if (!nombres.Contains(ee.full_name))
                                            {
                                                nombres.Add(ee.full_name);
                                            }
                                        }
                                        if (ee.last_name != null)
                                        {
                                            if (!nombres.Contains(ee.last_name))
                                            {
                                                nombres.Add(ee.last_name);
                                            }
                                        }
                                        if (nombres.Count != 0)
                                        {
                                            persona.name = nombres;
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
                                    if (ee.display_name != null)
                                    {
                                        if (!nombres.Contains(ee.display_name))
                                        {
                                            nombres.Add(ee.display_name);
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
                                        if (!nombres.Contains(ee.full_name))
                                        {
                                            nombres.Add(ee.full_name);
                                        }
                                    }
                                    if (ee.last_name != null)
                                    {
                                        if (!nombres.Contains(ee.last_name))
                                        {
                                            nombres.Add(ee.last_name);
                                        }
                                    }
                                    if (nombres.Count != 0)
                                    {
                                        persona.name = nombres;
                                    }
                                    result.Add(persona);

                                    return result;
                                }
                                catch
                                {
                                    Name_1 ee = JsonConvert.DeserializeObject<Name_1>(objInicial.static_data.summary.names.name.ToString());
                                    Person persona = new Person();

                                    List<string> nombres = new List<string>();
                                    if (ee.display_name != null)
                                    {
                                        if (!nombres.Contains(ee.display_name))
                                        {
                                            nombres.Add(ee.display_name);
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
                                        if (!nombres.Contains(ee.full_name))
                                        {
                                            nombres.Add(ee.full_name);
                                        }
                                    }
                                    if (ee.last_name != null)
                                    {
                                        if (!nombres.Contains(ee.last_name))
                                        {
                                            nombres.Add(ee.last_name);
                                        }
                                    }
                                    if (nombres.Count != 0)
                                    {
                                        persona.name = nombres;
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

        public Journal getJournal(PublicacionInicial objInicial)
        {
            Journal revista = new Journal();
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
            if (objInicial.dynamic_data != null)
            {
                if (objInicial.dynamic_data.cluster_related != null)
                {
                    if (objInicial.dynamic_data.cluster_related.identifiers != null)
                    {
                        if (objInicial.dynamic_data.cluster_related.identifiers.identifier != null)
                        {
                            try
                            {
                                JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString());
                                foreach (JContainer var in hey)
                                {
                                    Identifier identifier = JsonConvert.DeserializeObject<Identifier>(var.ToString());

                                    if (identifier.type == "issn")
                                    {
                                        revista.issn = identifier.value;
                                    }
                                    if (identifier.type == "eissn")
                                    {
                                        revista.eissn = identifier.value;
                                    }
                                }
                            }
                            catch
                            {
                                Identifier identifier = JsonConvert.DeserializeObject<Identifier>(objInicial.dynamic_data.cluster_related.identifiers.identifier.ToString());
                                if (identifier.type == "issn")
                                {
                                    revista.issn = identifier.value;
                                }
                                if (identifier.type == "eissn")
                                {
                                    revista.eissn = identifier.value;
                                }
                            }
                        }


                    }
                }
            }
            if (revista != new Journal())
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

        // public List<Publication> getBiblografia(PublicacionInicial objInicial)
        // {
        //     return new List<Publication>();
        // }

        // public List<Publication> getCitas(PublicacionInicial objInicial)
        // {
        //     return new List<Publication>();
        // }



    }
}