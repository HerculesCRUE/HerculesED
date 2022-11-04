using System.Collections.Generic;

namespace Hercules.ED.ImportExportCV.Models.FuentesExternas
{
    public class Person
    {
        public Name name { get; set; }
        public string ORCID { get; set; }
        public List<string> IDs { get; set; }
        public List<string> links { get; set; }
        public string fuente { get; set; }
        public string researcherID { get; set; }
        public string nick { get; set; }
    }
}
