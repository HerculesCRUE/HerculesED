using System.Collections.Generic;

namespace ZenodoAPI.Models.Data
{
    public class OntologyRO
    {
        public int? id { get; set; }
        public string titulo { get; set; }
        public string tipo { get; set; }
        public string descripcion { get; set; }
        public string url { get; set; }
        public List<string> urlData { get; set; }
        public string fechaPublicacion { get; set; }
        public string doi { get; set; }
        public List<string> etiquetas { get; set; }
        public List<string> etiquetasEnriquecidas { get; set; }
        public List<string> categoriasEnriquecidas { get; set; }
        public List<Person> autores { get; set; }
        public string licencia { get; set; }
    }

    public class Person
    {
        public int? id { get; set; }
        public string orcid { get; set; }
        public string nombreCompleto { get; set; }
    }
}