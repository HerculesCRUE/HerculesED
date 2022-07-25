using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.ImportExportCV.Models.FuentesExternas
{
    public class ObjEnriquecimientoSinPdf
    {
        public string rotype { get; set; }
        public string title { get; set; }
        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
    }
}
