using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CrossRefConnect.ROs.CrossRef.Models;
using System;

namespace CrossRefConnect.ROs.CrossRef.Controllers
{
      public class ROCrossRefController : ROCrossRefLogic
    {
        public ROCrossRefController(string baseUri, Dictionary<string, Tuple<string, string, string, string, string, string>> autores_orcid) : base(baseUri,autores_orcid)
        {
            // baseUri = "https://api.elsevier.com/";
            // _bareer = new Guid("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");
            this.baseUri = baseUri;
            this.autores_orcid=autores_orcid;
           // this.bareer = bareer;
            //headers.Add("view","COMPLETE");
            //headers.Add("User-Agent", "http://developer.github.com/v3/#user-agent-required");
            //headers.Add("Accept", "application/vnd.github.mercy-preview+json");
        }
    
    }
     
}
