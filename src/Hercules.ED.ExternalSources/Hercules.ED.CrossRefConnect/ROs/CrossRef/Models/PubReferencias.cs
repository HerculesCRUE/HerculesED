using System.Collections.Generic;

namespace CrossRefAPI.ROs.CrossRef.Models
{
    public class PubReferencias
    {
        public string doi { get; set; }
        public int? anyoPublicacion { get; set; }
        public string titulo { get; set; }
        public Dictionary<string, string> autores { get; set; }
        public string revista { get; set; }
        public int? paginaInicio { get; set; }
        public int? paginaFin { get; set; }
    }
}
