using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ScopusConnect.ROs.Scopus.Models
{
    public class Coredata
    {
        
        [JsonProperty("ID")]
        public string DcIdentifier { get; set; }
        public string eid { get; set; }

        [JsonProperty("document-count")]
        public string DocumentCount { get; set; }

        [JsonProperty("cited-by-count")]
        public string CitedByCount { get; set; }

        [JsonProperty("citation-count")]
        public string CitationCount { get; set; }
    }

    public class AffiliationCurrent
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@href")]
        public string Href { get; set; }
        public Affiliation affiliation { get; set; }
    }

   public class Affiliation
{

        [JsonProperty("@id")]
        public string Id { get; set; }

     
        [JsonProperty("@affiliation-id")]
        public string AffiliationId { get; set; }

        [JsonProperty("@source")]
        public string Source { get; set; }

        [JsonProperty("ip-doc")]
        public IpDoc IpDoc { get; set; }

        [JsonProperty("@parent")]
        public string Parent { get; set; }
    }


    public class SubjectArea
    {
       
        [JsonProperty("@abbrev")]
        public string Abbrev { get; set; }

        [JsonProperty("@code")]
        public string Code { get; set; }

        [JsonProperty("$")]
        public string  area { get; set; }
    }

    public class SubjectAreas
    {
        [JsonProperty("subject-area")]
        public List<SubjectArea> SubjectArea { get; set; }
    }

    public class DateCreated
    {
        [JsonProperty("@year")]
        public string Year { get; set; }

        [JsonProperty("@month")]
        public string Month { get; set; }

        [JsonProperty("@day")]
        public string Day { get; set; }
    }

    public class PreferredName
    {
        [JsonProperty("@source")]
        public string Source { get; set; }
        public string initials { get; set; }

        [JsonProperty("indexed-name")]
        public string IndexedName { get; set; }
        public string surname { get; set; }

        [JsonProperty("given-name")]
        public string GivenName { get; set; }

        [JsonProperty("$")]
        public string  name { get; set; }
    }

    public class NameVariant
    {
        [JsonProperty("@doc-count")]
        public string DocCount { get; set; }

        [JsonProperty("@source")]
        public string Source { get; set; }
        public string initials { get; set; }

        [JsonProperty("indexed-name")]
        public string IndexedName { get; set; }
        public string surname { get; set; }

        [JsonProperty("given-name")]
        public string GivenName { get; set; }
    }

    public class PublicationRange
    {
        [JsonProperty("@start")]
        public string Start { get; set; }

        [JsonProperty("@end")]
        public string End { get; set; }
    }

    public class Address
    {

        [JsonProperty("address-part")]
        public string AddressPart { get; set; }
        public string city { get; set; }
        public string state { get; set; }

        [JsonProperty("postal-code")]
        public string PostalCode { get; set; }
        public string country { get; set; }
    }

    public class IpDoc
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("@relationship")]
        public string Relationship { get; set; }
        public string afnameid { get; set; }
        public string afdispname { get; set; }

        [JsonProperty("preferred-name")]
        public PreferredName PreferredName { get; set; }

        [JsonProperty("sort-name")]
        public string SortName { get; set; }
        public Address address { get; set; }

        [JsonProperty("org-domain")]
        public string OrgDomain { get; set; }

        [JsonProperty("org-URL")]
        public string OrgURL { get; set; }

        [JsonProperty("parent-preferred-name")]
        public ParentPreferredName ParentPreferredName { get; set; }
    }

    public class ParentPreferredName
    {
       [JsonProperty("@source")]
        public string Source { get; set; }

        [JsonProperty("$")]
        public string name { get; set; }
    }


       public class Author_maite
    {
        [JsonProperty("subject-areas")]
        public SubjectAreas SubjectAreas { get; set; }
        public Coredata coredata { get; set; }


        [JsonProperty("hasPosition")]
        public AffiliationCurrent hasCurrentPosition { get; set; }
        
        
        [JsonProperty("publication-range")]
        public PublicationRange PublicationRange { get; set; }
        public string status { get; set; }       

        [JsonProperty("date-created")]
        public DateCreated DateCreated { get; set; }

        [JsonProperty("preferred-name")]
        public PreferredName PreferredName { get; set; }

        
    }




///// ESTO ES CON LA OTR PETICION DEL ARTICULO 

   public class Author_min
   {
        [JsonProperty("@_fa")]
        public string Fa { get; set; }
         [JsonProperty("@seq")]
        public string Seq { get; set; }
         [JsonProperty("ce:surname")]
        public string CeSurname { get; set; }
        [JsonProperty("ce:indexed-name")]
        public string CeIndexedName { get; set; }
         [JsonProperty("ce:initials")]
        public string CeInitials { get; set; }

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
      //  public Affiliation affiliation { get; set; }
    }

    public class DcCreator
    {
        public List<Author> author { get; set; }
    }

    

    public class Coredata_Publication
    {
        [JsonProperty("prism:url")]
        public string PrismUrl { get; set; }

        [JsonProperty("dc:identifier")]
        public string DcIdentifier { get; set; }

        [JsonProperty("pubmed-id")]
        public string PubmedId { get; set; }
        public string pii { get; set; }

        [JsonProperty("prism:doi")]
        public string PrismDoi { get; set; }

        [JsonProperty("dc:title")]
        public string DcTitle { get; set; }

        [JsonProperty("prism:aggregationType")]
        public string PrismAggregationType { get; set; }
        public string srctype { get; set; }

        [JsonProperty("citedby-count")]
        public string CitedbyCount { get; set; }

        [JsonProperty("prism:publicationName")]
        public string PrismPublicationName { get; set; }

        [JsonProperty("prism:issn")]
        public string PrismIssn { get; set; }

        [JsonProperty("prism:volume")]
        public string PrismVolume { get; set; }

        [JsonProperty("prism:issueIdentifier")]
        public string PrismIssueIdentifier { get; set; }

        [JsonProperty("prism:startingPage")]
        public string PrismStartingPage { get; set; }

        [JsonProperty("prism:endingPage")]
        public string PrismEndingPage { get; set; }

        [JsonProperty("prism:pageRange")]
        public string PrismPageRange { get; set; }

        [JsonProperty("prism:coverDate")]
        public string PrismCoverDate { get; set; }

        [JsonProperty("dc:creator")]
        public DcCreator DcCreator { get; set; }

        [JsonProperty("dc:description")]
        public string DcDescription { get; set; }
        public string intid { get; set; }
      //  public List<Link> link { get; set; }
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
        public string  name{ get; set; }
    }

    public class Authkeywords
    {
        [JsonProperty("author-keyword")]
        public List<AuthorKeyword> AuthorKeyword { get; set; }
    }

    public class Mainterm
    {
        [JsonProperty("@candidate")]
        public string Candidate { get; set; }

        [JsonProperty("@weight")]
        public string Weight { get; set; }

        [JsonProperty("$")]
        public string  name { get; set; }
    }

    public class Idxterms
    {
        public List<Mainterm> mainterm { get; set; }
    }

    public class Copyright
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("$")]
        public string name { get; set; }
    }

    public class Itemid
    {
        [JsonProperty("@idtype")]
        public string Idtype { get; set; }

        [JsonProperty("$")]
        public string  name{ get; set; }
    }

    public class Itemidlist
    {
        [JsonProperty("ce:pii")]
        public string CePii { get; set; }

        [JsonProperty("ce:doi")]
        public string CeDoi { get; set; }
        public List<Itemid> itemid { get; set; }
    }

    public class History
    {
        [JsonProperty("date-created")]
        public DateCreated DateCreated { get; set; }
    }

    public class Dbcollection
    {
        [JsonProperty("$")]
        public string name { get; set; }
    }

    public class ItemInfo
    {
        public Copyright copyright { get; set; }
        public Itemidlist itemidlist { get; set; }
        public History history { get; set; }
        public List<Dbcollection> dbcollection { get; set; }
    }

    public class CitationType
    {
        [JsonProperty("@code")]
        public string Code { get; set; }
    }

    public class CitationLanguage
    {
        [JsonProperty("@xml:lang")]
        public string XmlLang { get; set; }
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

    public class CitationInfo
    {
        [JsonProperty("citation-type")]
        public CitationType CitationType { get; set; }

        [JsonProperty("citation-language")]
        public CitationLanguage CitationLanguage { get; set; }

        [JsonProperty("abstract-language")]
        public AbstractLanguage AbstractLanguage { get; set; }

        [JsonProperty("author-keywords")]
        public AuthorKeywords AuthorKeywords { get; set; }
    }

    public class Organization
    {
        [JsonProperty("$")]
        public string name  { get; set; }
    }

    public class CeEAddress
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("$")]
        public string name { get; set; }
    }

    public class Correspondence
    {
        public Author person { get; set; }

        [JsonProperty("ce:e-address")]
        public CeEAddress CeEAddress { get; set; }
    }

    public class Issn
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("$")]
        public string name { get; set; }
    }

    public class Voliss
    {
        [JsonProperty("@issue")]
        public string Issue { get; set; }

        [JsonProperty("@volume")]
        public string Volume { get; set; }
    }

    public class Pagerange
    {
        [JsonProperty("@first")]
        public string First { get; set; }

        [JsonProperty("@last")]
        public string Last { get; set; }
    }

    public class Volisspag
    {
        public Voliss voliss { get; set; }
        public Pagerange pagerange { get; set; }
    }

    public class Publicationyear
    {
        [JsonProperty("@first")]
        public string First { get; set; }
    }

    public class DateText
    {
        [JsonProperty("@xfab-added")]
        public string XfabAdded { get; set; }

        [JsonProperty("$")]
        public string  name { get; set; }
    }

    public class Publicationdate
    {
        public string year { get; set; }
        public string month { get; set; }
        public string day { get; set; }

        [JsonProperty("date-text")]
        public DateText DateText { get; set; }
    }

    public class Source
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
        public Issn issn { get; set; }
        public string codencode { get; set; }
        public Volisspag volisspag { get; set; }
        public Publicationyear publicationyear { get; set; }
        public Publicationdate publicationdate { get; set; }
    }

    public class Descriptor2
    {
        public Mainterm mainterm { get; set; }
        public object link { get; set; }
    }

    public class Descriptor
    {
        [JsonProperty("@controlled")]
        public string Controlled { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }
        public List<Descriptor> descriptor { get; set; }
    }

    public class Descriptorgroup
    {
        public List<Descriptor> descriptors { get; set; }
    }   

    public class Manufacturer
    {
        [JsonProperty("@country")]
        public string Country { get; set; }

        [JsonProperty("$")]
        public string name { get; set; }
    }

    public class Manufacturers
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public Manufacturer manufacturer { get; set; }
    }

    public class Manufacturergroup
    {
        public Manufacturers manufacturers { get; set; }
    }

    public class Chemical
    {
        [JsonProperty("chemical-name")]
        public string ChemicalName { get; set; }

        [JsonProperty("cas-registry-number")]
        public string CasRegistryNumber { get; set; }
    }

    public class Chemicals
    {
        [JsonProperty("@source")]
        public string Source { get; set; }
        public List<Chemical> chemical { get; set; }
    }

    public class Chemicalgroup
    {
        public Chemicals chemicals { get; set; }
    }

    public class Enhancement
    {
        public Descriptorgroup descriptorgroup { get; set; }
        public Manufacturergroup manufacturergroup { get; set; }
        public Chemicalgroup chemicalgroup { get; set; }
    }

    public class Head
    {
        [JsonProperty("citation-info")]
        public CitationInfo CitationInfo { get; set; }

        [JsonProperty("citation-title")]
        public string CitationTitle { get; set; }
        public Correspondence correspondence { get; set; }
        public string abstracts { get; set; }
        public Source source { get; set; }
        public Enhancement enhancement { get; set; }
    }

    public class RefdItemidlist
    {
        public Itemid itemid { get; set; }
    }

    public class RefAuthors
    {
        //TTOODO
        public List<Author> author { get; set; }
    }

    public class RefPublicationyear
    {
        [JsonProperty("@first")]
        public string First { get; set; }
    }

    public class RefVolisspag
    {
        public Voliss voliss { get; set; }
        public Pagerange pagerange { get; set; }
    }

    public class RefInfo
    {
        [JsonProperty("refd-itemidlist")]
        public RefdItemidlist RefdItemidlist { get; set; }

        [JsonProperty("ref-authors")]
        public RefAuthors RefAuthors { get; set; }

        [JsonProperty("ref-sourcetitle")]
        public string RefSourcetitle { get; set; }

        [JsonProperty("ref-publicationyear")]
        public RefPublicationyear RefPublicationyear { get; set; }

        [JsonProperty("ref-volisspag")]
        public RefVolisspag RefVolisspag { get; set; }

        [JsonProperty("ref-text")]
        public string RefText { get; set; }
    }

    public class Reference
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("ref-info")]
        public RefInfo RefInfo { get; set; }
    }

    public class Bibliography
    {
        [JsonProperty("@refcount")]
        public string Refcount { get; set; }
       public List<Reference> reference { get; set; }
    }

    public class Tail
    {
        public Bibliography bibliography { get; set; }
    }

    public class Bibrecord
    {
        [JsonProperty("item-info")]
        public ItemInfo ItemInfo { get; set; }
       public Head head { get; set; }
        public Tail tail { get; set; }
    }

    public class Item
    {
      // [JsonProperty("ait:process-info")]
      //  public AitProcessInfo AitProcessInfo { get; set; }
      //TODO
      public Bibrecord bibrecord { get; set; }
    }

    public class Publication
    {
       public Coredata_Publication coredata { get; set; }
        //public List<Affiliation> affiliation { get; set; }
     //   public Authors authors { get; set; }
      //  public Language language { get; set; }
       // public Authkeywords authkeywords { get; set; }
        //public Idxterms idxterms { get; set; }

        [JsonProperty("subject-areas")]
         public SubjectAreas SubjectAreas { get; set; }
       // public Item item { get; set; }
    }

    public class Root_Publication
    {
        [JsonProperty("abstracts-retrieval-response")]
        public Publication AbstractsRetrievalResponse { get; set; }
    }

//////////////////////// AHORA hay que unir esta información!! /////////////

//////// Creo que necesitamos 

    


}