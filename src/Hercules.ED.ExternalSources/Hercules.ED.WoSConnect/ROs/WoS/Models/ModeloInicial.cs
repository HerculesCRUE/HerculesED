using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WoSConnect.ROs.WoS.Models.Inicial
{

 // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Page
    {
        public int page_count { get; set; }
        public Object end { get; set; }
        public int? begin { get; set; }
        public string content { get; set; }
    }

    public class PubInfo
    {
        public string coverdate { get; set; }
        public Object vol { get; set; } //deberia ser un int 
        public string has_citation_context { get; set; }
        public Object pubyear { get; set; } //Deberia ser un int 
        public Object issue { get; set; } // deberia ser un int 
        public string sortdate { get; set; }
        public string has_abstract { get; set; }
        public string pubmonth { get; set; }
        public string pubtype { get; set; }
        public Page page { get; set; }
        public string special_issue { get; set; }
    }

    public class DataItemIds
    {
        [JsonProperty("data-item-id")]
        public object DataItemId { get; set; }
    }

    public class PreferredName
    {
        public string full_name { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
    }

    public class Name_1
    {
        public int seq_no { get; set; }
        public string role { get; set; }
     //   public bool claim_status { get; set; }
    //    public string reprint { get; set; }
        public string last_name { get; set; }
        public string display_name { get; set; }
    //    public int daisng_id { get; set; }
        public string full_name { get; set; }
    //    public int addr_no { get; set; }

     //   [JsonProperty("data-item-ids")]
       // public DataItemIds DataItemIds { get; set; }
        public string wos_standard { get; set; }
    //    public string r_id { get; set; }
        public string first_name { get; set; }
 //       public PreferredName preferred_name { get; set; }
    //    public string orcid_id { get; set; }
    //    public string unified_name { get; set; }
    }

 public class Name_2
    {
        public int seq_no { get; set; }
        public string role { get; set; }
     //   public bool claim_status { get; set; }
    //    public string reprint { get; set; }
        public string last_name { get; set; }
        public string display_name { get; set; }
    //    public int daisng_id { get; set; }
        public string full_name { get; set; }
    //    public int addr_no { get; set; }

     //   [JsonProperty("data-item-ids")]
       // public DataItemIds DataItemIds { get; set; }
        public string wos_standard { get; set; }
    //    public string r_id { get; set; }
        public string first_name { get; set; }
 //       public PreferredName preferred_name { get; set; }
        public string orcid_id { get; set; }
    //    public string unified_name { get; set; }
    }
    public class Names
    {
        public int count { get; set; }
       // public Object name {get;set;}
        public Object name { get; set; }
    }


    public class Doctypes
    {
        public Object doctype { get; set; }
        public int count { get; set; }
    }

    public class AddressSpec
    {
        public string city { get; set; }
        public int addr_no { get; set; }
        public string full_address { get; set; }
        public Zip zip { get; set; }
        public string country { get; set; }
        public string street { get; set; }
        public Organizations organizations { get; set; }
        public Suborganizations suborganizations { get; set; }
    }

    public class Publisher
    {
        public Names names { get; set; }
        public AddressSpec address_spec { get; set; }
    }

    public class Publishers
    {
        public Publisher publisher { get; set; }
    }

    public class WUID
    {
        public string coll_id { get; set; }
    }

    public class EWUID
    {
        public WUID WUID { get; set; }
        public object edition { get; set; }
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

    public class ConfDate
    {
        public int? conf_start { get; set; }
        public int? conf_end { get; set; }
        public string content { get; set; }
    }

    public class ConfDates
    {
        public ConfDate conf_date { get; set; }
        public int count { get; set; }
    }

    public class Sponsors
    {
        //public List<string> sponsor { get; set; } //todo da error 
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
        public string conf_host { get; set; }
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

    public class Conference
    {
        public ConfDates conf_dates { get; set; }
        public Object conf_id { get; set; } //deberia ser un int 
        public Sponsors sponsors { get; set; }
        public ConfInfos conf_infos { get; set; }
        public ConfLocations conf_locations { get; set; }
        public ConfTitles conf_titles { get; set; }
    }

    public class Conferences
    {
        public Object conference { get; set; } // deberia ser de tipo Conference pero  aveces sera una lista de ese objeto 
        public Object count { get; set; } //deberia ser un int 
    }

    public class Summary
    {
        public PubInfo pub_info { get; set; }
        public Names names { get; set; }
        public Doctypes doctypes { get; set; }
        public Publishers publishers { get; set; }
        public EWUID EWUID { get; set; }
        public Titles titles { get; set; }
        public Conferences conferences { get; set; }
    }

    public class Ids
    {
        public string avail { get; set; }
        public string content { get; set; }
    }

    public class BibPagecount
    {
        public string type { get; set; }
        public int content { get; set; }
    }

    public class KeywordsPlus
    {
        public int count { get; set; }
        public Object keyword { get; set; }
    }

    public class BookNotes
    {
        public object book_note { get; set; }
        public int count { get; set; }
    }

    public class Item
    {
        [JsonProperty("xsi:type")]
        public string XsiType { get; set; }
        public string coll_id { get; set; }
        public Ids ids { get; set; }

        [JsonProperty("xmlns:xsi")]
        public string XmlnsXsi { get; set; }
        public BibPagecount bib_pagecount { get; set; }
        public KeywordsPlus keywords_plus { get; set; }
        public string bib_id { get; set; }
        public int? book_pages { get; set; }
        public BookNotes book_notes { get; set; }
        public object book_desc { get; set; }
    }

    public class Addresses
    {
        public int count { get; set; }
        public Object address_name { get; set; }
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

    public class NormalizedLanguages
    {
        public int count { get; set; }
        public Language language { get; set; }
    }

    public class Languages
    {
        public int count { get; set; }
        public Language language { get; set; }
    }

    public class Keywords
    {
        public int count { get; set; }
        public List<string> keyword { get; set; } //list<String> or string. 
    }
      public class Keywords_2
    {
        public int count { get; set; }
        public string keyword { get; set; } //list<String> or string. 
    }


    public class Refs
    {
        public int count { get; set; }
    }

    public class Zip
    {
        public string location { get; set; }
        public int content { get; set; }
    }

    public class Organization
    {
        public string pref { get; set; }
        public string content { get; set; }
    }

    public class Organizations
    {
        public List<Organization> organization { get; set; }
        public int count { get; set; }
    }

    public class Suborganizations
    {
        public int count { get; set; }
        public object suborganization { get; set; }
    }

    public class AddressName
    {
        public Names names { get; set; }
        public AddressSpec address_spec { get; set; }
    }

    public class ReprintAddresses
    {
        public int count { get; set; }
       // public AddressName address_name { get; set; }
       public Object address_name {get;set;} // este objeto es el tipo AddressName pero algunos solo tienen un nombre y otros una lista d enombres
    }

    public class AbstractText
    {
        public string p { get; set; }
        public int count { get; set; }
    }

      public class AbstractText_list
    {
        public List<string> p { get; set; }
        public int count { get; set; }
    }


    public class Abstract
    {
        public Object abstract_text { get; set; }
    }

    public class Abstracts
    {
        public int count { get; set; }
        public Abstract @abstract { get; set; }
    }

    public class Grants
    {
        public int count { get; set; }
        public object grant { get; set; }
    }

    public class FundText
    {
        public string p { get; set; }
    }

    public class FundAck
    {
        public Grants grants { get; set; }
        public FundText fund_text { get; set; }
    }

    public class NormalizedDoctypes
    {
        public object doctype { get; set; }
        public int count { get; set; }
    }

    public class FullrecordMetadata
    {
        public Addresses addresses { get; set; }
        public CategoryInfo category_info { get; set; }
        public NormalizedLanguages normalized_languages { get; set; }
        public Languages languages { get; set; }
        public Object keywords { get; set; }
        public Refs refs { get; set; }
        public ReprintAddresses reprint_addresses { get; set; }
        public Abstracts abstracts { get; set; }
       // public FundAck fund_ack { get; set; }
        public NormalizedDoctypes normalized_doctypes { get; set; }
    }

    public class Contributor
    {
        public Object name { get; set; }
    }

    public class Contributors
    {
        public List<Contributor> contributor { get; set; }
        
        public int count { get; set; }
    }

    public class StaticData
    {
        public Summary summary { get; set; }
        public Item item { get; set; }
        public FullrecordMetadata fullrecord_metadata { get; set; }
       // public Contributors contributors { get; set; }
       public Object contributors { get; set; } //TODO cuidado que este objet es de tipo Controbutors
    }

    public class Dates
    {
        public DateTime date_modified { get; set; }
        public DateTime date_created { get; set; }
    }

    public class SiloTc
    {
        public string coll_id { get; set; }
        public int? local_count { get; set; }
    }

    public class TcList
    {
        public SiloTc silo_tc { get; set; }
    }

    public class CitationRelated
    {
        public TcList tc_list { get; set; }
    }

    public class Identifier
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Identifiers
    {
        public Object identifier { get; set; }
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

    public class PublicacionInicial
    {
        public string UID { get; set; }
        public StaticData static_data { get; set; }
        public Dates dates { get; set; }
        public string r_id_disclaimer { get; set; }
        public DynamicData dynamic_data { get; set; }
    }


      public class Book
    {
        public string UID { get; set; }
        public StaticData static_data { get; set; }
        public Dates dates { get; set; }
        public string r_id_disclaimer { get; set; }
        public DynamicData dynamic_data { get; set; }
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

    // public class QueryResult
    // {
    //     public int QueryID { get; set; }
    //     public int RecordsSearched { get; set; }
    //     public int RecordsFound { get; set; }
    // }

    public class Root
    {
        public Data Data { get; set; }
    //    public QueryResult QueryResult { get; set; }
    }



    
}