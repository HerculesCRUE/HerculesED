using Newtonsoft.Json;
using System.Collections.Generic;

namespace ScopusConnect.ROs.Scopus.Models.Inicial
{
    public class Link
    {
        [JsonProperty("@href")]
        public string Href { get; set; }

        [JsonProperty("@ref")]
        public string Ref { get; set; }
    }

    public class PublicacionInicial
    {
        public List<Link> link { get; set; }

        [JsonProperty("dc:identifier")]
        public string DcIdentifier { get; set; }

        [JsonProperty("dc:title")]
        public string DcTitle { get; set; }

        [JsonProperty("dc:creator")]
        public string DcCreator { get; set; }

        [JsonProperty("prism:publicationName")]
        public string PrismPublicationName { get; set; }

        [JsonProperty("prism:issn")]
        public string PrismIssn { get; set; }

        [JsonProperty("prism:volume")]
        public string PrismVolume { get; set; }

        [JsonProperty("prism:pageRange")]
        public string PrismPageRange { get; set; }

        [JsonProperty("prism:coverDate")]
        public string PrismCoverDate { get; set; }

        [JsonProperty("citedby-count")]
        public string CitedbyCount { get; set; }

        [JsonProperty("prism:aggregationType")]
        public string PrismAggregationType { get; set; }
        public string subtype { get; set; }
        public string subtypeDescription { get; set; }

        [JsonProperty("source-id")]
        public string SourceId { get; set; }
        public string openaccess { get; set; }
        public bool openaccessFlag { get; set; }

        [JsonProperty("prism:isbn")]
        public List<PrismIsbn> PrismIsbn { get; set; }

        [JsonProperty("prism:doi")]
        public string PrismDoi { get; set; }

        [JsonProperty("article-number")]
        public string ArticleNumber { get; set; }


        [JsonProperty("prism:eIssn")]
        public string PrismEIssn { get; set; }
    }

    public class PrismIsbn
    {
        [JsonProperty("@_fa")]
        public string Fa { get; set; }

        [JsonProperty("$")]
        public string id { get; set; }
    }

    public class SearchResults
    {
        public List<PublicacionInicial> entry { get; set; }
    }

    public class Root
    {
        [JsonProperty("search-results")]
        public SearchResults SearchResults { get; set; }
    }
}