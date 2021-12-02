using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using WoSConnect.ROs.WoS.Models;
using System.Data;
using System;
using System.IO;
using System.Text;
using ExcelDataReader;




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
