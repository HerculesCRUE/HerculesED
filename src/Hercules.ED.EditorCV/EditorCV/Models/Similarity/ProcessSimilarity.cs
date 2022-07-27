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
            noduplicado2
        }
        public string principal { get; set; }
        public Dictionary<string, ProcessSimilarityAction> secundarios { get; set; }
    }
}
