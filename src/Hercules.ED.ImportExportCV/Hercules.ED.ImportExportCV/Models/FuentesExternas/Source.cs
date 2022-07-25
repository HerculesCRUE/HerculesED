using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.ImportExportCV.Models.FuentesExternas
{
    public class Source
    {
        public string type { get; set; }
        public List<string> issn { get; set; }
        public List<string> isbn { get; set; }
        public string name { get; set; }
        public string eissn { get; set; }
        public List<JournalMetric> hasMetric { get; set; }
    }
}
