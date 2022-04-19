using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class ResearchObjectGitHub:RO_JSON
    {                
        public List<string> listaAutores { get; set; }
        public string fechaCreacion { get; set; }
        public string fechaActualizacion { get; set; }
        public Dictionary<string, float> lenguajes { get; set; }
        public int? numReleases { get; set; }
        public int? numForks { get; set; }
        public int? numIssues { get; set; }
        public List<string> etiquetas { get; set; }
    }
}
