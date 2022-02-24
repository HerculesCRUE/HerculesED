using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
