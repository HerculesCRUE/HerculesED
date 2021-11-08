using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OpenCitationsConnect.ROs.OpenCitations.Models;

namespace OpenCitationsConnect.ROs.OpenCitations.Controllers
{
      public class ROOpenCitationsController : ROOpenCitationsLogic
    {
        public ROOpenCitationsController(string baseUri) : base(baseUri)
        {
            // baseUri = "https://api.elsevier.com/";
            // _bareer = new Guid("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");
            this.baseUri = baseUri;
           // this.bareer = bareer;
            //headers.Add("view","COMPLETE");
            //headers.Add("User-Agent", "http://developer.github.com/v3/#user-agent-required");
            //headers.Add("Accept", "application/vnd.github.mercy-preview+json");
        }
    
    }
     
}
