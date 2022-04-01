using System.Collections.Generic;

namespace Hercules.ED.UMLS.Models.Data
{
    public class RelationsObj
    {
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public int pageCount { get; set; }
        public List<ResultRelations> result { get; set; }
    }

    public class ResultRelations
    {
        public string classType { get; set; }
        public string ui { get; set; }
        public bool suppressible { get; set; }
        public string sourceUi { get; set; }
        public bool obsolete { get; set; }
        public bool sourceOriginated { get; set; }
        public string rootSource { get; set; }
        public string relationLabel { get; set; }
        public string additionalRelationLabel { get; set; }
        public string groupId { get; set; }
        public int attributeCount { get; set; }
        public string relatedId { get; set; }
        public string relatedIdName { get; set; }
    }
}
