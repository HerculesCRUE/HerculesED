using System.Collections.Generic;

namespace EditorCV.Models.Enrichment
{
    public class EnrichmentResponse
    {
        public class EnrichmentResponseItem
        {
            public string word { get; set; }
            public float porcentaje { get; set; }            
        }

        public List<EnrichmentResponseItem> topics { get; set; }
    }

    public class EnrichmentResponseCategory
    {
        public class EnrichmentResponseItem
        {
            public string id { get; set; }
        }

        public List<List<EnrichmentResponseItem>> topics { get; set; }
    }

    public class EnrichmentResponseGlobal
    {        
        public EnrichmentResponse tags { get; set; }
        public EnrichmentResponseCategory categories { get; set; }
    }
}
