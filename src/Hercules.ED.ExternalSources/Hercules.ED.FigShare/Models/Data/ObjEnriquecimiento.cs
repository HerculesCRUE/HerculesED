using Newtonsoft.Json;
using System.Collections.Generic;

namespace FigShareAPI.Models.Data
{
    public class ObjEnriquecimientoSinPdf
    {
        public string rotype { get; set; }

        public string title { get; set; }

        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
    }

    public class ObjEnriquecimientoConPdf
    {
        public string rotype { get; set; }

        [JsonProperty("pdf_url")]
        public string pdfurl { get; set; }

        public string title { get; set; }

        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
    }

    public class Topics_enriquecidos
    {
        public string pdf_url { get; set; }
        public string rotype { get; set; }
        public List<Knowledge_enriquecidos> topics { get; set; }
    }

    public class Knowledge_enriquecidos
    {
        public string word { get; set; }
        public string porcentaje { get; set; }
    }
}
