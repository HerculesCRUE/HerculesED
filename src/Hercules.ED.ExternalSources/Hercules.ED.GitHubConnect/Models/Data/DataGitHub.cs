using System.Collections.Generic;

namespace GitHubAPI.Models.Data
{
    /**
    * Modelo encargado de los datos del pricipales del investigador
    */
    public class DataGitHub
    {
        public int? id { get; set; }
        public string tipo { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public string url { get; set; }
        public List<string> listaAutores { get; set; }
        public string fechaCreacion { get;set; }
        public string fechaActualizacion { get; set; }
        public Dictionary<string, float> lenguajes { get; set; }
        public string licencia { get; set; }
        public int? numReleases { get; set; }
        public int? numForks { get; set; }
        public int? numIssues { get; set; }
        public List<string> etiquetas { get; set; }
        public List<string> etiquetasEnriquecidas { get; set; }
        public List<string> categoriasEnriquecidas { get; set; }
    }    
}
