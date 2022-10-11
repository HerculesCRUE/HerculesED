using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EditorCV.Models.API.Response
{
    public class Person : DisambiguableEntity
    {
        private static DisambiguationDataConfig configName = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.algoritmoNombres,
            score = 1f
        };
        public HashSet<string> signatures { get; set; }
        public string personid { get; set; }
        public string orcid { get; set; }
        public string name { get; set; }
        public string department { get; set; }
        public string url { get; set; }
        public float score { get; set; }
        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>();

            data.Add(new DisambiguationData()
            {
                property = "name",
                config = configName,
                value = name
            });

            return data;
        }
    }
}
