using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.UpdateKeywords.Models
{
    public class DataConcept
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }

        public Dictionary<string, string> Broader { get; set; }
        public Dictionary<string, string> Qualifiers { get; set; }
        public Dictionary<string, string> RelatedTo { get; set; }
        public Dictionary<string, string> CloseMatch { get; set; }
        public Dictionary<string, string> ExactMatch { get; set; }
    }
}
