using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.ImportExportCV.Models.FuentesExternas
{
    public class Bibliografia
    {
        public string doi { get; set; }
        public string url { get; set; }
        public int? anyoPublicacion { get; set; }
        public string titulo { get; set; }
        public string revista { get; set; }
        public Dictionary<string, string> autores { get; set; }
    }
}
