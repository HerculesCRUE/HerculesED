using System.Collections.Generic;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class ResearchObjectFigShare : RO_JSON
    {
        public string UrlPdf { get; set; }
        public string FechaPublicacion { get; set; }
        public string Doi { get; set; }
        public List<Person_JSON> Autores { get; set; }
        public List<string> Etiquetas { get; set; }
    }
}
