using System.Collections.Generic;

namespace Hercules.ED.DisambiguationEngine.Models
{
    public abstract class DisambiguableEntity
    {
        public abstract List<DisambiguationData> GetDisambiguationData();
        public string ID { get; set; }
        public HashSet<string> distincts { get; set; }
        public bool block { get; set; }
    }
}
