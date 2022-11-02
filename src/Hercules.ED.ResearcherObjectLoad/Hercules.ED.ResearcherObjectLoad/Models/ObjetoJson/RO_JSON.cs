using System.Collections.Generic;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class RO_JSON
    {
        public string ID { get; set; }
        public int? id { get; set; }
        public string titulo { get; set; }
        public string tipo { get; set; }
        public string descripcion { get; set; }
        public string url { get; set; }
        public string licencia { get; set; }
        public List<string> etiquetasEnriquecidas { get; set; }
        public List<string> categoriasEnriquecidas { get; set; }
    }
}
