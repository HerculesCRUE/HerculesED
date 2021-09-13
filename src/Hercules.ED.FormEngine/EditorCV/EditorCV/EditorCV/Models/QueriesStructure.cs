using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace EditorCV.Models
{

    public class QueryStructure
    {
        public Dictionary<string, Query> queries { get; set; }
    }

    public class Query
    {
        public List<string> Select { get; set; }
        public List<List<string>> Where { get; set; }
        public List<List<string>> Filter { get; set; }
    }

}
