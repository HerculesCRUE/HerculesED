using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ScopusConnect.ROs.Scopus.Models.Inicial
{

 // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
      public class Link
    {
        
        [JsonProperty("@href")]
        public string Href { get; set; }

    

        [JsonProperty("@ref")]
        public string Ref { get; set; }

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
   
    public class   PublicacionInicial

    {
        // [JsonProperty("@_fa")]
       // public string Fa { get; set; }
        public List<Link> link { get; set; }

       // [JsonProperty("prism:url")]
       // public string PrismUrl { get; set; } 

        [JsonProperty("dc:identifier")]
        public string DcIdentifier { get; set; }
        //public string eid { get; set; }

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

        //[JsonProperty("prism:coverDisplayDate")] 
        //  public string PrismCoverDisplayDate { get; set; }
          

        [JsonProperty("citedby-count")]
        public string CitedbyCount { get; set; }
      // public List<Affiliation> affiliation { get; set; }

        [JsonProperty("prism:aggregationType")]
        public string PrismAggregationType { get; set; } 
        public string subtype { get; set; }
        public string subtypeDescription { get; set; }

        [JsonProperty("source-id")]
        public string SourceId { get; set; }
       // public string openaccess { get; set; }
  // public bool openaccessFlag { get; set; }

        [JsonProperty("prism:isbn")]
        public List<PrismIsbn> PrismIsbn { get; set; }

        [JsonProperty("prism:doi")]
        public string PrismDoi { get; set; }

      //  [JsonProperty("article-number")]
        //public string ArticleNumber { get; set; }

       // [JsonProperty("prism:issueIdentifier")]
      //  public string PrismIssueIdentifier { get; set; } 

//        [JsonProperty("pubmed-id")]
  //      public string PubmedId { get; set; }
        //public Freetoread freetoread { get; set; }
        //public FreetoreadLabel freetoreadLabel { get; set; }

        [JsonProperty("prism:eIssn")]
        public string PrismEIssn { get; set; } 
    }
 public class PrismIsbn
    {
        [JsonProperty("@_fa")]
        public string Fa { get; set; }

        [JsonProperty("$")]
        public string  id { get; set; }
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
        public List<PublicacionInicial> entry { get; set; }
    }

    public class Root
    {
        [JsonProperty("search-results")]
        public SearchResults SearchResults { get; set; }
    }


    
}