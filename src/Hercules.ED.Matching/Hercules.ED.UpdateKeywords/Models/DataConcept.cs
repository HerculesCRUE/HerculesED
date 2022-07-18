using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.UpdateKeywords.Models
{
    public class DataConcept
    {
        public string title { get; set; }
        public string url { get; set; }
        public string type { get; set; }

        public Dictionary<string, string> broader { get; set; }
        public Dictionary<string, string> qualifiers { get; set; }
        public Dictionary<string, string> relatedTo { get; set; }
        public Dictionary<string, string> closeMatch { get; set; }
        public Dictionary<string, string> exactMatch { get; set; }
    }
}
