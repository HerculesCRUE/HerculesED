using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EditorCV.Models.Similarity
{
    public class ProcessSimilarity
    {
        public enum ProcessSimilarityAction
        {
            fusionar=0,
            eliminar=1,
            noduplicado=2
        }
        public string idCV { get; set; }
        public string idSection { get; set; }
        public string rdfTypeTab { get; set; }
        public string principal { get; set; }
        public Dictionary<string, ProcessSimilarityAction> secundarios { get; set; }
    }
}
