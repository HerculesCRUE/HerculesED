using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.UpdateKeywords.Models
{
    public class Data
    {
        public Result snomedTerm { get; set; }
        public List<ResultRelations> relations { get; set; }
    }

    public class CrosswalkObj
    {
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public int pageCount { get; set; }
        public List<Result> result { get; set; }
    }

    public class SubsetMembership
    {
        public string memberUri { get; set; }
        public string name { get; set; }
        public string uri { get; set; }
    }

    public class ContentViewMembership
    {
        public string memberUri { get; set; }
        public string name { get; set; }
        public string uri { get; set; }
    }

    public class Result
    {
        public string classType { get; set; }
        public string ui { get; set; }
        public bool suppressible { get; set; }
        public bool obsolete { get; set; }
        public string rootSource { get; set; }
        public int atomCount { get; set; }
        public int cVMemberCount { get; set; }
        public string attributes { get; set; }
        public string atoms { get; set; }
        public string descendants { get; set; }
        public string ancestors { get; set; }
        public string parents { get; set; }
        public string children { get; set; }
        public string relations { get; set; }
        public string definitions { get; set; }
        public string concepts { get; set; }
        public string defaultPreferredAtom { get; set; }
        public List<SubsetMembership> subsetMemberships { get; set; }
        public List<ContentViewMembership> contentViewMemberships { get; set; }
        public string name { get; set; }
    }

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
