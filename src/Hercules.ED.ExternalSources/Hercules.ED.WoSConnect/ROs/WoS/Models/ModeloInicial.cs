using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using WoSConnect.ROs.WoS.Controllers;

namespace WoSConnect.ROs.WoS.Models.Inicial
{
    public class Page
    {
        public int page_count { get; set; }
        public object end { get; set; }
        public object begin { get; set; }
        public string content { get; set; }
    }

    public class PubInfo
    {
        public object coverdate { get; set; }
        public string vol { get; set; }
        public string journal_oas_gold { get; set; }
        public int pubyear { get; set; }
        public string sortdate { get; set; }
        public string has_abstract { get; set; }
        public string pubmonth { get; set; }
        public string pubtype { get; set; }
        public Page page { get; set; }
        public string has_citation_context { get; set; }
        public string supplement { get; set; }
        public string special_issue { get; set; }
        public object issue { get; set; }
    }

    public class PreferredName
    {
        public string full_name { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
    }

    public class Name
    {
        public int? seq_no { get; set; }
        public string orcid_id { get; set; }
        public string role { get; set; }
        public string full_name { get; set; }
        public string reprint { get; set; }
        public object addr_no { get; set; }
        public string last_name { get; set; }
        public string display_name { get; set; }
        public string wos_standard { get; set; }
        public string r_id { get; set; }
        public int daisng_id { get; set; }
        public string first_name { get; set; }
        public bool? claim_status { get; set; }
        public PreferredName preferred_name { get; set; }
        public string suffix { get; set; }
        public string unified_name { get; set; }
        [JsonProperty("data-item-ids")]
        public DataItemIds data_item_ids { get; set; }
    }

    public class DataItemIds
    {
        [JsonConverter(typeof(SingleOrArrayConverter<DataItemId>))]
        [JsonProperty("data-item-id")]
        public List<DataItemId> DataItemId { get; set; }
    }

    public class DataItemId
    {
        public string type { get; set; }
        public string content { get; set; }
        [JsonProperty("id-type")]
        public string IdType { get; set; }
    }

    public class Names
    {
        public int count { get; set; }
        [JsonConverter(typeof(SingleOrArrayConverter<Name>))]
        public List<Name> name { get; set; }
    }

    public class Doctypes
    {
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string> doctype { get; set; }
        public int count { get; set; }
    }

    public class Title
    {
        public string type { get; set; }
        public string content { get; set; }
        public string translated { get; set; }
    }

    public class Titles
    {
        public int count { get; set; }
        public List<Title> title { get; set; }
    }

    public class Summary
    {
        public PubInfo pub_info { get; set; }
        public Names names { get; set; }
        public Doctypes doctypes { get; set; }
        public Titles titles { get; set; }        
        public Conferences conferences { get; set; }
    }
    public class ConfDate
    {
        public int conf_start { get; set; }
        public int conf_end { get; set; }
        public string content { get; set; }
    }

    public class ConfDates
    {
        public ConfDate conf_date { get; set; }
        public int count { get; set; }
    }

    public class Conference
    {
        public ConfDates conf_dates { get; set; }
        public int conf_id { get; set; }
        public ConfInfos conf_infos { get; set; }
        public ConfLocations conf_locations { get; set; }
        public ConfTitles conf_titles { get; set; }
    }

    public class Conferences
    {
        [JsonConverter(typeof(SingleOrArrayConverter<Conference>))]
        public List<Conference> conference { get; set; }
        public int count { get; set; }
    }

    public class ConfInfos
    {
        public int count { get; set; }
        public string conf_info { get; set; }
    }

    public class ConfLocation
    {
        public string conf_state { get; set; }
        public string conf_city { get; set; }
    }

    public class ConfLocations
    {
        public int count { get; set; }
        public ConfLocation conf_location { get; set; }
    }

    public class ConfTitles
    {
        public int count { get; set; }
        public string conf_title { get; set; }
    }
    public class Sponsors
    {
        public List<string> sponsor { get; set; }
        public int count { get; set; }
    }

    public class Subheadings
    {
        public int count { get; set; }
        public object subheading { get; set; }
    }

    public class Subject
    {
        public string ascatype { get; set; }
        public string code { get; set; }
        public string content { get; set; }
    }

    public class Subjects
    {
        public List<Subject> subject { get; set; }
        public int count { get; set; }
    }

    public class Headings
    {
        public object heading { get; set; }
        public int count { get; set; }
    }

    public class CategoryInfo
    {
        public Subheadings subheadings { get; set; }
        public Subjects subjects { get; set; }
        public Headings headings { get; set; }
    }

    public class Language
    {
        public string type { get; set; }
        public string content { get; set; }
    }

    public class Languages
    {
        public int count { get; set; }
        public Language language { get; set; }
    }

    public class Keywords
    {
        public int count { get; set; }
        [JsonConverter(typeof(SingleValueArrayConverter<string>))]
        public List<string> keyword { get; set; }
    }

    public class SingleValueArrayConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal = new Object();
            if (reader.TokenType == JsonToken.StartObject)
            {
                T instance = (T)serializer.Deserialize(reader, typeof(T));
                retVal = new List<T>() { instance };
            }else if (reader.TokenType == JsonToken.String)
            {
                T instance = (T)serializer.Deserialize(reader, typeof(T));
                retVal = new List<T>() { instance };
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                retVal = serializer.Deserialize(reader, objectType);
            }
            return retVal;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }

    public class AbstractText
    {
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string> p { get; set; }
        public int count { get; set; }
    }

    public class Abstract
    {
        public AbstractText abstract_text { get; set; }
    }

    public class Abstracts
    {
        public int count { get; set; }
        public Abstract @abstract { get; set; }
    }

    public class FullrecordMetadata
    {
        public CategoryInfo category_info { get; set; }
        public Languages languages { get; set; }
        public Keywords keywords { get; set; }
        public Abstracts abstracts { get; set; }
    }

    public class StaticData
    {
        public Summary summary { get; set; }
        public FullrecordMetadata fullrecord_metadata { get; set; }

        public Contributors contributors { get; set; }
    }

    public class NameContributor
    {
        public int? seq_no { get; set; }
        public string orcid_id { get; set; }
        public string role { get; set; }
        public string full_name { get; set; }
        public string last_name { get; set; }
        public string display_name { get; set; }
        public string r_id { get; set; }
        public string first_name { get; set; }
    }

    public class Contributor
    {
        public NameContributor name { get; set; }
    }

    public class Contributors
    {
        [JsonConverter(typeof(SingleOrArrayConverter<Contributor>))]
        public List<Contributor> contributor { get; set; }
        public int count { get; set; }
    }
    public class SiloTc
    {
        public string coll_id { get; set; }
        public int local_count { get; set; }
    }

    public class TcList
    {
        public SiloTc silo_tc { get; set; }
    }

    public class CitationRelated
    {
        public TcList tc_list { get; set; }
    }

    public class Identifiers
    {
        [JsonConverter(typeof(SingleOrArrayConverter<Identificadores>))]
        public List<Identificadores> identifier { get; set; }
    }

    public class Identificadores
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class ClusterRelated
    {
        public Identifiers identifiers { get; set; }
    }

    public class DynamicData
    {
        public CitationRelated citation_related { get; set; }
        public ClusterRelated cluster_related { get; set; }
    }

    public class Records2
    {
        public List<PublicacionInicial> REC { get; set; }
    }

    public class Records
    {
        public Records2 records { get; set; }
    }

    public class Data
    {
        public Records Records { get; set; }
    }

    public class Root
    {
        public Data Data { get; set; }
    }

    public class PublicacionInicial
    {
        public string UID { get; set; }
        public StaticData static_data { get; set; }
        public DynamicData dynamic_data { get; set; }
    }
}