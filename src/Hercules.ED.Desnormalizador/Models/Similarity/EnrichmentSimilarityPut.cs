using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesnormalizadorHercules.Models
{
    public class EnrichmentSimilarityPut
    {
        public string ro_id { get; set; }
        public string ro_type { get; set; }
        public string text { get; set; }
        public List<string> authors { get; set; }
        public List<List<object>> thematic_descriptors { get; set; }
        public List<List<object>> specific_descriptors { get; set; }  
    }

}
