using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ScopusConnect.ROs.Scopus.Models.Inicial
{

    public class Link
    {
        
        [JsonProperty("@href")]
        public string Href { get; set; }

    

        [JsonProperty("@rel")]
        public string Rel { get; set; }

    }

   /* 
public class Affiliation
    {
        [JsonProperty("@href")]
        public string Href { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }
        public string affilname { get; set; }

        [JsonProperty("@afid")]
        public string Afid { get; set; }

        [JsonProperty("@country")]
        public string Country { get; set; }

        [JsonProperty("@dptid")]
        public string Dptid { get; set; }
        //TODO
        //public List<Organization> organization { get; set; }

        [JsonProperty("address-part")]
        public string AddressPart { get; set; }

        [JsonProperty("city-group")]
        public string CityGroup { get; set; }
    } */
   
    public class Entry
    {
      /*   [JsonProperty("@_fa")]
        public string Fa { get; set; }
        public List<Link> link { get; set; }

        [JsonProperty("prism:url")]
        public string PrismUrl { get; set; } */

        [JsonProperty("dc:identifier")]
        public string DcIdentifier { get; set; }
       /*  public string eid { get; set; }

        [JsonProperty("dc:title")]
        public string DcTitle { get; set; }

        [JsonProperty("dc:creator")]
        public string DcCreator { get; set; } */

       /*  [JsonProperty("prism:publicationName")]
        public string PrismPublicationName { get; set; }

        [JsonProperty("prism:issn")]
        public string PrismIssn { get; set; }

        [JsonProperty("prism:volume")]
        public string PrismVolume { get; set; } */

       /*  [JsonProperty("prism:pageRange")]
        public string PrismPageRange { get; set; }

        [JsonProperty("prism:coverDate")]
        public string PrismCoverDate { get; set; }

        [JsonProperty("prism:coverDisplayDate")] */
       /*  public string PrismCoverDisplayDate { get; set; }

        [JsonProperty("citedby-count")]
        public string CitedbyCount { get; set; }
       // public List<Affiliation> affiliation { get; set; }

        [JsonProperty("prism:aggregationType")]
        public string PrismAggregationType { get; set; } */
        public string subtype { get; set; }
      /*   public string subtypeDescription { get; set; }

        [JsonProperty("source-id")]
        public string SourceId { get; set; }
        public string openaccess { get; set; }
 */       /*  public bool openaccessFlag { get; set; }

        [JsonProperty("prism:isbn")]
        public List<PrismIsbn> PrismIsbn { get; set; }

        [JsonProperty("prism:doi")]
        public string PrismDoi { get; set; }

        [JsonProperty("article-number")]
        public string ArticleNumber { get; set; }

        [JsonProperty("prism:issueIdentifier")]
        public string PrismIssueIdentifier { get; set; } */

      /*   [JsonProperty("pubmed-id")]
        public string PubmedId { get; set; }
        public Freetoread freetoread { get; set; }
        public FreetoreadLabel freetoreadLabel { get; set; }

        [JsonProperty("prism:eIssn")]
        public string PrismEIssn { get; set; } */
    }

    public class SearchResults
    {
       /*  [JsonProperty("opensearch:totalResults")] //commented in phase 1
        public string OpensearchTotalResults { get; set; }

        [JsonProperty("opensearch:startIndex")]
        public string OpensearchStartIndex { get; set; }

        [JsonProperty("opensearch:itemsPerPage")]
        public string OpensearchItemsPerPage { get; set; } */

      /*   [JsonProperty("opensearch:Query")]
        public OpensearchQuery OpensearchQuery { get; set; }
        public List<Link> link { get; set; } */
        public List<Entry> entry { get; set; }
    }

    public class Root
    {
        [JsonProperty("search-results")]
        public SearchResults SearchResults { get; set; }
    }

//----------------------------------------------------------- Peticion articulo!
    public class PreferredName
    {
        [JsonProperty("ce:initials")]
        public string CeInitials { get; set; }

        [JsonProperty("ce:indexed-name")]
        public string CeIndexedName { get; set; }

        [JsonProperty("ce:surname")]
        public string CeSurname { get; set; }

        [JsonProperty("ce:given-name")]
        public string CeGivenName { get; set; }
    }

    public class Author
    {
        [JsonProperty("@_fa")]
        public string Fa { get; set; }

        [JsonProperty("@auid")]
        public string Auid { get; set; }

        [JsonProperty("@seq")]
        public string Seq { get; set; }

        [JsonProperty("ce:initials")]
        public string CeInitials { get; set; }

        [JsonProperty("ce:indexed-name")]
        public string CeIndexedName { get; set; }

        [JsonProperty("ce:surname")]
        public string CeSurname { get; set; }

        [JsonProperty("preferred-name")]
        public PreferredName PreferredName { get; set; }

        [JsonProperty("author-url")]
        public string AuthorUrl { get; set; }
        //public Affiliation affiliation { get; set; }
    }

    public class DcCreator
    {
        public List<Author> author { get; set; }
    }


    public class Coredata
    {
     /*    [JsonProperty("prism:url")] 
        public string PrismUrl { get; set; }

        [JsonProperty("dc:identifier")]
        public string DcIdentifier { get; set; } */

        //[JsonProperty("pubmed-id")]
        //public string PubmedId { get; set; }
        //public string pii { get; set; }

        [JsonProperty("prism:doi")]
        public string PrismDoi { get; set; }

        [JsonProperty("dc:title")]
        public string DcTitle { get; set; }

        //[JsonProperty("prism:aggregationType")]
        //public string PrismAggregationType { get; set; }
        //public string srctype { get; set; }

        [JsonProperty("citedby-count")]
        public string CitedbyCount { get; set; }

        [JsonProperty("prism:publicationName")]
        public string PrismPublicationName { get; set; }

        [JsonProperty("prism:issn")]
        public string PrismIssn { get; set; }

       // [JsonProperty("prism:volume")]
       // public string PrismVolume { get; set; }

        //[JsonProperty("prism:issueIdentifier")]
       // public string PrismIssueIdentifier { get; set; }

        [JsonProperty("prism:startingPage")]
        public string PrismStartingPage { get; set; }

        [JsonProperty("prism:endingPage")]
        public string PrismEndingPage { get; set; }

        //[JsonProperty("prism:pageRange")]
       // public string PrismPageRange { get; set; }

        [JsonProperty("prism:coverDate")]
        public string PrismCoverDate { get; set; }

        [JsonProperty("dc:creator")]
        public DcCreator DcCreator { get; set; }

        [JsonProperty("dc:description")]
        public string DcDescription { get; set; }
        //public string intid { get; set; }
        public List<Link> link { get; set; }
    }

    public class Authors
    {
        public List<Author> author { get; set; }
    }

    public class Language
    {
        [JsonProperty("@xml:lang")]
        public string XmlLang { get; set; }
    }

    public class AuthorKeyword
    {
        [JsonProperty("$")]
        public string  a { get; set; }
    }

    public class Authkeywords
    {
        [JsonProperty("author-keyword")]
        public List<AuthorKeyword> AuthorKeyword { get; set; }
        //TODO Pasa lo mismo que con Itemid, que  a veces es una lista y a veces un AuthorKeyword.... 
    }

  /*   public class Mainterm //commented in phase 1
    {
        [JsonProperty("@candidate")]
        public string Candidate { get; set; }

        [JsonProperty("@weight")]
        public string Weight { get; set; }

        [JsonProperty("$")]
        public string  a { get; set; }
    } */

   /*  public class Idxterms //commented in phase 1
    {
        public List<Mainterm> mainterm { get; set; }
    } */

    public class SubjectArea
    {
        [JsonProperty("@_fa")]
        public string Fa { get; set; }

        [JsonProperty("@abbrev")]
        public string Abbrev { get; set; }

        [JsonProperty("@code")]
        public string Code { get; set; }

        [JsonProperty("$")]
        public string a { get; set; }
    }

    public class SubjectAreas
    {
        [JsonProperty("subject-area")]
        public List<SubjectArea> SubjectArea { get; set; }
    }

 /*    public class AitDateDelivered //commented in phase 1
    {
        [JsonProperty("@day")]
        public string Day { get; set; }

        [JsonProperty("@month")]
        public string Month { get; set; }

        [JsonProperty("@timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("@year")]
        public string Year { get; set; }
    } */

  /*   public class AitDateSort //commented in phase 1
    {
        [JsonProperty("@day")]
        public string Day { get; set; }

        [JsonProperty("@month")]
        public string Month { get; set; }

        [JsonProperty("@year")]
        public string Year { get; set; }
    } */
 
  /*   public class AitStatus //commented in phase 1
    {
        [JsonProperty("@stage")]
        public string Stage { get; set; }

        [JsonProperty("@state")]
        public string State { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }
    } */

/*     public class AitProcessInfo //commented in phase 1
    {
        [JsonProperty("ait:date-delivered")]
        public AitDateDelivered AitDateDelivered { get; set; }

        [JsonProperty("ait:date-sort")]
        public AitDateSort AitDateSort { get; set; }

        [JsonProperty("ait:status")]
        public AitStatus AitStatus { get; set; }
    } */

/*     public class Copyright //commented in phase 1
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("$")]
        public string a { get; set; }
    } */

    public class Itemid 
    {
        [JsonProperty("@idtype")]
        public string Idtype { get; set; }

        [JsonProperty("$")]
        public string  a { get; set; }
    }

  //  public class Itemidlist //commented in phase 1
   // {
        //[JsonProperty("ce:pii")]
        //public string CePii { get; set; }

        //[JsonProperty("ce:doi")]
        //public string CeDoi { get; set; }
        //public List<Itemid> itemid { get; set; }
    //} 

/*     public class DateCreated //commented in phase 1
    {
        [JsonProperty("@day")]
        public string Day { get; set; }

        [JsonProperty("@month")]
        public string Month { get; set; }

        [JsonProperty("@year")]
        public string Year { get; set; }
    } */

/*     public class History //commented in phase 1
    {
        [JsonProperty("date-created")]
        public DateCreated DateCreated { get; set; }
    } */

    public class Dbcollection
    {
        [JsonProperty("$")]
        public string a { get; set; }
    }

    public class ItemInfo   //commented in phase 1
    {
       // public List<Copyright> copyright { get; set; }
        //[JsonProperty("itemidlist")]
        //public Itemidlist itemidlist { get; set; }
        //public History history { get; set; }
        //public List<Dbcollection> dbcollection { get; set; }
    } 

    public class CitationType
    {
        [JsonProperty("@code")]
        public string Code { get; set; }
    }

    public class CitationLanguage
    {
        //[JsonProperty("@xml:lang")]
       // public string XmlLang { get; set; }
    }

    public class AbstractLanguage
    {
        [JsonProperty("@xml:lang")]
        public string XmlLang { get; set; }
    }

    public class AuthorKeywords
    {
        [JsonProperty("author-keyword")]
        public List<AuthorKeyword> AuthorKeyword { get; set; }
    }

 /*    public class CitationInfo
    {
        [JsonProperty("citation-type")]
        public CitationType CitationType { get; set; }

        //[JsonProperty("citation-language")]
       // public CitationLanguage CitationLanguage { get; set; }

        [JsonProperty("abstract-language")]
        public AbstractLanguage AbstractLanguage { get; set; }

        [JsonProperty("author-keywords")]
        public AuthorKeywords AuthorKeywords { get; set; }
    } */

    public class Organization
    {
        [JsonProperty("$")]
        public string a { get; set; }
    }

  /*   public class AuthorGroup //commented in phase 1
    {
        public object author { get; set; }
        public Affiliation affiliation { get; set; }
    } */

    /* public class Person_inicial //commented in phase 1
    {
        [JsonProperty("ce:initials")]
        public string CeInitials { get; set; }

        [JsonProperty("ce:indexed-name")]
        public string CeIndexedName { get; set; }

        [JsonProperty("ce:surname")]
        public string CeSurname { get; set; }
    } */

 /*    public class CeEAddress //commented in phase 1
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("$")]
        public string  a { get; set; }
    } */

  /*   public class Correspondence //commented in phase 1
    {
        public Person_inicial person { get; set; }
        public Affiliation affiliation { get; set; }

        [JsonProperty("ce:e-address")]
        public CeEAddress CeEAddress { get; set; }
    } */

  /*   public class Issn //commented in phase 1
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("$")]
        public string a { get; set; }
    } */

/*     public class Voliss //commented in phase 1
    {
        [JsonProperty("@issue")]
        public string Issue { get; set; }

        [JsonProperty("@volume")]
        public string Volume { get; set; }
    } */

   /*  public class Pagerange //commented in phase 1
    {
        [JsonProperty("@first")]
        public string First { get; set; }

        [JsonProperty("@last")]
        public string Last { get; set; }
    } */

   /*  public class Volisspag //commented in phase 1
    {
        public Voliss voliss { get; set; }
        public Pagerange pagerange { get; set; }
    } */

 /*    public class Publicationyear //commented in phase 1
    {
        [JsonProperty("@first")]
        public string First { get; set; }
    } */

  /*   public class DateText //commented in phase 1
    {
        [JsonProperty("@xfab-added")]
        public string XfabAdded { get; set; }

        [JsonProperty("$")]
        public string a { get; set; }
    } */

   /*  public class Publicationdate  //commented in phase 1
    {
        public string year { get; set; }
        public string month { get; set; }
        public string day { get; set; }

       // [JsonProperty("date-text")]
       // public DateText DateText { get; set; }
    } */

    /* public class Source //commented in phase 1
    {
        [JsonProperty("@country")]
        public string Country { get; set; }

        [JsonProperty("@srcid")]
        public string Srcid { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }
        public string sourcetitle { get; set; }

        [JsonProperty("sourcetitle-abbrev")]
        public string SourcetitleAbbrev { get; set; }
        //public Issn issn { get; set; }
        public string codencode { get; set; }
        public Volisspag volisspag { get; set; }
        public Publicationyear publicationyear { get; set; }
        public Publicationdate publicationdate { get; set; }
    }
 */
 /*    public class Descriptor2  //commented in phase 1
    {
        public Mainterm mainterm { get; set; }
        public object link { get; set; }
    }
 */
    public class Descriptor
    {
        [JsonProperty("@controlled")]
        public string Controlled { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }
        public List<Descriptor> descriptor { get; set; }
    }

 /*    public class Descriptorgroup //commented in phase 1
    {
        public List<Descriptor> descriptors { get; set; }
    } */

/*     public class Classification  //commented in phase 1
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public object classification { get; set; }
    } */

   /*  public class Classificationgroup //commented in phase 1
    {
        public List<Classification> classifications { get; set; }
    } */

/*     public class Manufacturer  //commented in phase 1
    {
        [JsonProperty("@country")]
        public string Country { get; set; }

        [JsonProperty("$")]
        public string a { get; set; }
    } */

/*     public class Manufacturers  //commented in phase 1
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public Manufacturer manufacturer { get; set; }
    } */

/*     public class Manufacturergroup  //commented in phase 1
    {
        public Manufacturers manufacturers { get; set; }
    } */

   /*  public class Chemical ////commented in phase 1
    {
        [JsonProperty("chemical-name")]
        public string ChemicalName { get; set; }

        [JsonProperty("cas-registry-number")]
        public string CasRegistryNumber { get; set; }
    } */

    /* public class Chemicals //commented in phase 1
    {
        [JsonProperty("@source")]
        public string Source { get; set; }
        public List<Chemical> chemical { get; set; }
    } */

    /* public class Chemicalgroup //commented in phase 1
    {
        public Chemicals chemicals { get; set; }
    } */

    //public class Enhancement //commented in phase 1
    //{
      //  public Descriptorgroup descriptorgroup { get; set; }
      //  public Classificationgroup classificationgroup { get; set; }
      //  public Manufacturergroup manufacturergroup { get; set; }
      //  public Chemicalgroup chemicalgroup { get; set; }
    //}

    //public class Head  //commented in phase 1
    //{
    //    [JsonProperty("citation-info")]
    //    public CitationInfo CitationInfo { get; set; }

        //[JsonProperty("citation-title")]
        //public string CitationTitle { get; set; }

        //[JsonProperty("author-group")]
        //public List<AuthorGroup> AuthorGroup { get; set; } //commented in phase 1
        // public Correspondence correspondence { get; set; } //commented in phase 1
       // public string abstracts { get; set; }  //commented in phase 1
       // public Source source { get; set; } //commented in phase 1
       // public Enhancement enhancement { get; set; } //commented in phase 1
   // } 
//---------------------------------------------------------------------------------

 public class RefdItemidlist
    {
        public Object itemid{ get; set;}
        //public List<Itemid> itemid { get; set;}
        /*set{       
            //public dynamic hey;
            try{
                itemid = new List<Itemid>();
                for (int i=0;i<value.Count; i++){
                    itemid.add(value[i]);
                } 
            }catch{
               string itemid = value;
            }
        }
        get{

            return itemid;
        } }    */

    }
    public class RefInfo
    {
        [JsonProperty("refd-itemidlist")]
        public RefdItemidlist RefdItemidlist { get; set; }

    }

    public class Reference
    {
        [JsonProperty("ref-fulltext")]
        public string RefFulltext { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("ref-info")]
        public RefInfo RefInfo { get; set; }
    }

    public class Bibliography
    {
        //[JsonProperty("@refcount")]
       // public string Refcount { get; set; }
        public List<Reference> reference { get; set; }
    }

    public class Tail
    {
        public Bibliography bibliography { get; set; }
    }

//-----------------------------------------------
 
 
   // public class RefAuthors //commented in phase 1
    //{ 
    //    public object author { get; set; }
   // }

  //  public class RefPublicationyear  //commented in phase 1
   // {
    //    [JsonProperty("@first")]
     //   public string First { get; set; }
   // }

   // public class RefVolisspag        //commented in phase 1
    // {
    //    public Voliss voliss { get; set; }
   //     public Pagerange pagerange { get; set; }
   // }




    public class Bibrecord
    {
        //[JsonProperty("item-info")] //commented in phase 1
        // public ItemInfo ItemInfo { get; set; } //commented in phase 1
        //       public Head head { get; set; } //commented in phase 1
        public Tail tail { get; set; }
    }

    public class Item
    {
        //[JsonProperty("ait:process-info")]  commented in phase 1
        //public AitProcessInfo AitProcessInfo { get; set; }  commented in phase 1
        public Bibrecord bibrecord { get; set; }
    }

    public class AbstractsRetrievalResponse
    {
        public Coredata coredata { get; set; }
        //public List<Affiliation> affiliation { get; set; }
        public Authors authors { get; set; }
        public Language language { get; set; }
        public Authkeywords authkeywords { get; set; }
        //public Idxterms idxterms { get; set; } commented in phase 1

        [JsonProperty("subject-areas")]
        public SubjectAreas SubjectAreas { get; set; }
        public Item item { get; set; }
    }

    public class Publication_root
    {
        [JsonProperty("abstracts-retrieval-response")]
        public AbstractsRetrievalResponse AbstractsRetrievalResponse { get; set; }
    }

    
}