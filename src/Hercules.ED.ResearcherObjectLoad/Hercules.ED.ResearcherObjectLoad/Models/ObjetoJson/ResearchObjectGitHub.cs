using System.Collections.Generic;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class ResearchObjectGitHub : RO_JSON
    {
        public List<string> ListaAutores { get; set; }
        public string FechaCreacion { get; set; }
        public string FechaActualizacion { get; set; }
        public Dictionary<string, float> Lenguajes { get; set; }
        public int? NumReleases { get; set; }
        public int? NumForks { get; set; }
        public int? NumIssues { get; set; }
        public List<string> Etiquetas { get; set; }
    }
}
