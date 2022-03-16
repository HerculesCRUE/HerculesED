using System.Collections.Generic;

namespace Hercules.ED.DisambiguationEngine.Models
{
    public enum DisambiguationDataConfigType
    {
        equalsIdentifiers,
        equalsTitle,
        equalsItem,
        equalsItemList,
        //equalsIgnoreCaseItem,
        algoritmoNombres
    }

    public class DisambiguationDataConfig
    {
        public DisambiguationDataConfigType type { get; set; }
        public float score { get; set; }
        public float scoreMinus { get; set; }
    }

    public class DisambiguationData
    {
        public DisambiguationDataConfig config { get; set; }
        public string property { get; set; }
        public string value { get; set; }
        public HashSet<string> values { get; set; }
    }
}
