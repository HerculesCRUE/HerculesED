using System.Collections.Generic;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class ResearchObjectZenodo : RO_JSON
    {
        public List<string> urlData { get; set; }
        public string fechaPublicacion { get; set; }
        public string doi { get; set; }
        public List<Person_JSON> autores { get; set; }
    }

}
