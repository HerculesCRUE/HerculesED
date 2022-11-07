using System.Collections.Generic;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class DocumentoBBDD
    {
        public string urlPdf { get; set; }
        public HashSet<string> etiquetas { get; set; }
        public HashSet<string> categorias { get; set; }
    }
}
