using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpenCitationsConnect.ROs.OpenCitations.Models.Inicial
{

 // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
   
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Root
    {
        public List<PublicationInicial> data {get;set;}
       
    }
    
    public class PublicationInicial
    {
        public string oci { get; set; }
        public string citing { get; set; }
        public string cited { get; set; }
        public string creation { get; set; }
        public string timespan { get; set; }
        public string journal_sc { get; set; }
        public string author_sc { get; set; }
    }

    
}