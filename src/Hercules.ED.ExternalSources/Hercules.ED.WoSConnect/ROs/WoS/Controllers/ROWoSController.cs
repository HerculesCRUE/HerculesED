using Gnoss.ApiWrapper;
using System.Collections.Generic;

namespace WoSConnect.ROs.WoS.Controllers
{
    public class ROWoSController : ROWoSLogic
    {
        public ROWoSController(string baseUri, string bareer, Dictionary<string, string> ds, ResourceApi pResourceApi) : base(baseUri, bareer, ds, pResourceApi)
        {
            this.baseUri = baseUri;
            this.bareer = bareer;
            this.ds = base.ds;
        }
    }
}
