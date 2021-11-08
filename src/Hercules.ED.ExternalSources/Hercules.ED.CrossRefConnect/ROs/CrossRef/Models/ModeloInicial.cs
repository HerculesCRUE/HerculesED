using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CrossRefConnect.ROs.CrossRef.Models.Inicial
{
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Indexed
    {
        [JsonProperty("date-parts")]
        public List<List<int>> DateParts { get; set; }

        [JsonProperty("date-time")]
        public DateTime DateTime { get; set; }
        public long timestamp { get; set; }
    }

    public class Start
    {
        [JsonProperty("date-parts")]
        public List<List<int>> DateParts { get; set; }

        [JsonProperty("date-time")]
        public DateTime DateTime { get; set; }
        public long timestamp { get; set; }
    }

    public class License
    {
        public Start start { get; set; }

        [JsonProperty("content-version")]
        public string ContentVersion { get; set; }

        [JsonProperty("delay-in-days")]
        public int DelayInDays { get; set; }
        public string URL { get; set; }
    }

    public class Funder
    {
        public string name { get; set; }
    }

    public class ContentDomain
    {
        public List<string> domain { get; set; }

        [JsonProperty("crossmark-restriction")]
        public bool CrossmarkRestriction { get; set; }
    }

    public class PublishedPrint
    {
        [JsonProperty("date-parts")]
        public List<List<int>> DateParts { get; set; }
    }

    public class Created
    {
        [JsonProperty("date-parts")]
        public List<List<int>> DateParts { get; set; }

        [JsonProperty("date-time")]
        public DateTime DateTime { get; set; }
        public long timestamp { get; set; }
    }

    public class Author
    {
        public string given { get; set; }
        public string family { get; set; }
        public string sequence { get; set; }
        public List<object> affiliation { get; set; }
        public string ORCID { get; set; }

        [JsonProperty("authenticated-orcid")]
        public bool? AuthenticatedOrcid { get; set; }
    }

    public class Reference
    {
        public string key { get; set; }
        public string unstructured { get; set; }
        public string issue { get; set; }

        [JsonProperty("doi-asserted-by")]
        public string DoiAssertedBy { get; set; }

        [JsonProperty("first-page")]
        public string FirstPage { get; set; }
        public string DOI { get; set; }

        [JsonProperty("article-title")]
        public string ArticleTitle { get; set; }
        public string volume { get; set; }
        public string author { get; set; }
        public string year { get; set; }

        [JsonProperty("journal-title")]
        public string JournalTitle { get; set; }

        [JsonProperty("series-title")]
        public string SeriesTitle { get; set; }
    }

    public class Link
    {
        public string URL { get; set; }

        [JsonProperty("content-type")]
        public string ContentType { get; set; }

        [JsonProperty("content-version")]
        public string ContentVersion { get; set; }

        [JsonProperty("intended-application")]
        public string IntendedApplication { get; set; }
    }

    public class Deposited
    {
        [JsonProperty("date-parts")]
        public List<List<int>> DateParts { get; set; }

        [JsonProperty("date-time")]
        public DateTime DateTime { get; set; }
        public long timestamp { get; set; }
    }

    public class Issued
    {
        [JsonProperty("date-parts")]
        public List<List<int>> DateParts { get; set; }
    }

    public class Relation
    {
    }

    public class IssnType
    {
        public string value { get; set; }
        public string type { get; set; }
    }

    public class Published
    {
        [JsonProperty("date-parts")]
        public List<List<int>> DateParts { get; set; }
    }

    public class Assertion
    {
        public string value { get; set; }
        public string name { get; set; }
        public string label { get; set; }
    }

    public class PublicacionInicial
    {
        public Indexed indexed { get; set; }

        [JsonProperty("reference-count")]
        public int ReferenceCount { get; set; }
        public string publisher { get; set; }
        public List<License> license { get; set; }
        public List<Funder> funder { get; set; }

        [JsonProperty("content-domain")]
        public ContentDomain ContentDomain { get; set; }

        [JsonProperty("short-container-title")]
        public List<string> ShortContainerTitle { get; set; }

        [JsonProperty("published-print")]
        public PublishedPrint PublishedPrint { get; set; }
        public string DOI { get; set; }
        public string type { get; set; }
        public Created created { get; set; }
        public string page { get; set; }

        [JsonProperty("update-policy")]
        public string UpdatePolicy { get; set; }
        public string source { get; set; }

        [JsonProperty("is-referenced-by-count")]
        public int IsReferencedByCount { get; set; }
        public List<string> title { get; set; }
        public string prefix { get; set; }
        public string volume { get; set; }
        public List<Author> author { get; set; }
        public string member { get; set; }
        public List<Reference> reference { get; set; }

        [JsonProperty("container-title")]
        public List<string> ContainerTitle { get; set; }

        [JsonProperty("original-title")]
        public List<object> OriginalTitle { get; set; }
        public string language { get; set; }
        public List<Link> link { get; set; }
        public Deposited deposited { get; set; }
        public int score { get; set; }
        public List<object> subtitle { get; set; }

        [JsonProperty("short-title")]
        public List<object> ShortTitle { get; set; }
        public Issued issued { get; set; }

        [JsonProperty("references-count")]
        public int ReferencesCount { get; set; }

        [JsonProperty("alternative-id")]
        public List<string> AlternativeId { get; set; }
        public string URL { get; set; }
        public Relation relation { get; set; }
        public List<string> ISSN { get; set; }

        [JsonProperty("issn-type")]
        public List<IssnType> IssnType { get; set; }
        public List<string> subject { get; set; }
        public Published published { get; set; }
        public List<Assertion> assertion { get; set; }
    }

    public class Root
    {
        public string status { get; set; }

        [JsonProperty("message-type")]
        public string MessageType { get; set; }

        [JsonProperty("message-version")]
        public string MessageVersion { get; set; }
        public PublicacionInicial message { get; set; }
    }



    
}