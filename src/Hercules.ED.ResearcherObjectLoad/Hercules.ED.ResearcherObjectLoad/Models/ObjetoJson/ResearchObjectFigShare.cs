using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class ResearchObjectFigShare : RO_JSON
    {             
        public string urlPdf { get; set; }
        public string fechaPublicacion { get; set; }
        public string doi { get; set; }
        public List<Person_JSON> autores { get; set; }
        public List<string> etiquetas { get; set; }
    }

}
