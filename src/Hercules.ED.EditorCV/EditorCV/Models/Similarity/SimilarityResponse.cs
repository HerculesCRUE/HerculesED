using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EditorCV.Models.Similarity
{
    public class SimilarityResponse
    {
        public string idSection { get; set; }
        public string rdfTypeTab { get; set; }
        public HashSet<string> items { get; set; }
    }
}
