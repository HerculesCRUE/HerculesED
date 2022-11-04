using Newtonsoft.Json;

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
