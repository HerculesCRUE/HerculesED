using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.ImportExportCV.Models.FuentesExternas
{
    public class Topics_enriquecidos
    {
        public string pdf_url { get; set; }
        public string rotype { get; set; }
        public List<Knowledge_enriquecidos> topics { get; set; }
    }
}
