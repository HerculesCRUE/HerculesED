using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class Pais
    {
        public string name { get; set; }
        string alpha_2 { get; set; }
        string alpha_3 { get; set; }
        public string country_code { get; set; }
        string iso_3166_2 { get; set; }
        string region { get; set; }
        string sub_region { get; set; }
        string intermediate_region { get; set; }
        string region_code { get; set; }
        string sub_region_code { get; set; }
        string intermediate_region_code { get; set; }
        Pais()
        {
            name = "";
            alpha_2 = "";
            alpha_3 = "";
            country_code = "";
            iso_3166_2 = "";
            region = "";
            sub_region = "";
            intermediate_region = "";
            region_code = "";
            sub_region_code = "";
            intermediate_region_code = "";
        }
    }
}
