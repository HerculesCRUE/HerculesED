using System.Collections.Generic;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class ResearchObjectZenodo : RO_JSON
    {
        public List<string> UrlData { get; set; }
        public string FechaPublicacion { get; set; }
        public string Doi { get; set; }
        public List<Person_JSON> Autores { get; set; }
    }
}
