using Newtonsoft.Json;

namespace Hercules.ED.ImportExportCV.Models.FuentesExternas
{
    public class ObjEnriquecimientoConPdf
    {
        public string rotype { get; set; }
        [JsonProperty("pdf_url")]
        public string pdfurl { get; set; }
        public string title { get; set; }
        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
    }
}
