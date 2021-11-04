using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using WoSConnect.ROs.WoS.Models;

namespace WoSConnect.ROs.WoS.Controllers
{
      public class ROWoSController : ROWoSLogic
    {
        public ROWoSController(string baseUri, string bareer) : base(baseUri, bareer)
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
