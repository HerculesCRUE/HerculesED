using System.Collections.Generic;

namespace WoSConnect.ROs.WoS.Controllers
{
    public class ROWoSController : ROWoSLogic
    {
        public ROWoSController(string baseUri, string bareer, Dictionary<string, string> ds) : base(baseUri, bareer, ds)
        {
            this.baseUri = baseUri;
            this.bareer = bareer;
            this.ds = base.ds;
        }
    }
}
