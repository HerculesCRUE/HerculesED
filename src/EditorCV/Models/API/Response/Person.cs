using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EditorCV.Models.API.Response
{
    public class Person
    {
        public HashSet<string> signatures { get; set; }
        public string personid { get; set; }
        public string orcid { get; set; }
        public string name { get; set; }
        public string department { get; set; }
        public string url { get; set; }
        public float score { get; set; }
        public float scoreMin { get; set; }
        public float scoreMax { get; set; }
    }
}
