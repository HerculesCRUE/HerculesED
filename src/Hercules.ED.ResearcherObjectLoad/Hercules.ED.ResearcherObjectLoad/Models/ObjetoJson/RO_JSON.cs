using System.Collections.Generic;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class RO_JSON
    {
        public string ID { get; set; }
        public int? Id { get; set; }
        public string Titulo { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public string Url { get; set; }
        public string Licencia { get; set; }
        public List<string> EtiquetasEnriquecidas { get; set; }
        public List<string> CategoriasEnriquecidas { get; set; }
    }
}
