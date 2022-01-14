using Newtonsoft.Json;

namespace EditorCV.Models.Enrichment
{
    public class EnrichmentData
    {
        public string rotype { get; set; }
        [JsonProperty("pdf_url")]
        public string pdfurl { get; set; }
        public string title { get; set; }

        [JsonProperty("abstract")]
        public string abstract_ { get; set; }

    }
}
