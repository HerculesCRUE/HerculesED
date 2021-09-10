using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ScopusConnect.ROs.Scopus.Models;

namespace ScopusConnect.ROs.Scopus.Controllers
{
      public class ROScopusController : ROScopusLogic
    {
        public ROScopusController(string baseUri, string bareer) : base(baseUri, bareer)
        {
            // baseUri = "https://api.elsevier.com/";
            // _bareer = new Guid("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");
            this.baseUri = baseUri;
            this.bareer = bareer;
            //headers.Add("view","COMPLETE");
            //headers.Add("User-Agent", "http://developer.github.com/v3/#user-agent-required");
            //headers.Add("Accept", "application/vnd.github.mercy-preview+json");
        }
    
    }
     
}
