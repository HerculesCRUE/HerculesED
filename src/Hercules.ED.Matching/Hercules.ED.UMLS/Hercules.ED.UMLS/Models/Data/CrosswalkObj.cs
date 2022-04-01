using System.Collections.Generic;

namespace Hercules.ED.UMLS.Models.Data
{
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
}
