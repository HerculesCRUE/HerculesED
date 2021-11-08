using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SemanticScholarConnect.ROs.SemanticScholar.Models.Inicial
{

 // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class ExternalIds
    {
        public string MAG { get; set; }
        public string DOI { get; set; }
        public List<string> DBLP { get; set; }
        public string ArXiv {get;set;}
        public string PubMedCentral {get;set;} 
    }

    public class Author
    {
        public string authorId { get; set; }
        public ExternalIds externalIds { get; set; }
        public string name { get; set; }
    }

    public class Root
    {
        public string paperId { get; set; }
        public ExternalIds externalIds { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string @abstract { get; set; }
        public string venue { get; set; }
        public int year { get; set; }
        public int referenceCount { get; set; }
        public int citationCount { get; set; }
        public List<Author> authors { get; set; }
    }


    
}