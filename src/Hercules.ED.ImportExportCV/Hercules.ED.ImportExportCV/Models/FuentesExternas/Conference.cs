using System.Collections.Generic;

namespace Hercules.ED.ImportExportCV.Models.FuentesExternas
{
    public class Conference
    {
        public string abbreviation { get; set; }
        public DateTimeInterval dateTimeInterval { get; set; }
        public string description { get; set; }
        public List<string> IDs { get; set; }
        public string title { get; set; }
        public string freetextKeyword { get; set; }
        public string locality { get; set; }
        public List<KnowledgeAreas> hasKnowledgeArea { get; set; }
    }
}
