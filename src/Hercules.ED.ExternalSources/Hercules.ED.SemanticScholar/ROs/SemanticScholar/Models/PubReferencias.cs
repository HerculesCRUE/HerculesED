using System.Collections.Generic;

namespace SemanticScholarAPI.ROs.SemanticScholar.Models
{
    public class PubReferencias
    {
        public string doi { get; set; }
        public string url { get; set; }
        public int? anyoPublicacion { get; set; }
        public string titulo { get; set; }        
        public string revista { get; set; }
        public Dictionary<string, string> autores { get; set; }
    }
}
