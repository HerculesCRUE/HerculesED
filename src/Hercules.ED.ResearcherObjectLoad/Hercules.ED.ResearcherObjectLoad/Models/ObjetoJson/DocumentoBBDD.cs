using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class DocumentoBBDD
    {
        public string urlPdf { get; set; }
        public HashSet<string> etiquetas { get; set; }
        public HashSet<string> categorias { get; set; }
    }
}
