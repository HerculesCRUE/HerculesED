using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.UpdateKeywords.Models
{
    public class Data
    {
        public Result SnomedTerm { get; set; }
        public List<ResultRelations> Relations { get; set; }
    }

    public class CrosswalkObj
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PageCount { get; set; }
        public List<Result> Result { get; set; }
    }

    public class SubsetMembership
    {
        public string MemberUri { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
    }

    public class ContentViewMembership
    {
        public string MemberUri { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
    }

    public class Result
    {
        public string ClassType { get; set; }
        public string Ui { get; set; }
        public bool Suppressible { get; set; }
        public bool Obsolete { get; set; }
        public string RootSource { get; set; }
        public int AtomCount { get; set; }
        public int CVMemberCount { get; set; }
        public string Attributes { get; set; }
        public string Atoms { get; set; }
        public string Descendants { get; set; }
        public string Ancestors { get; set; }
        public string Parents { get; set; }
        public string Children { get; set; }
        public string Relations { get; set; }
        public string Definitions { get; set; }
        public string Concepts { get; set; }
        public string DefaultPreferredAtom { get; set; }
        public List<SubsetMembership> SubsetMemberships { get; set; }
        public List<ContentViewMembership> ContentViewMemberships { get; set; }
        public string Name { get; set; }
    }

    public class RelationsObj
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PageCount { get; set; }
        public List<ResultRelations> Result { get; set; }
    }

    public class ResultRelations
    {
        public string ClassType { get; set; }
        public string Ui { get; set; }
        public bool Suppressible { get; set; }
        public string SourceUi { get; set; }
        public bool Obsolete { get; set; }
        public bool SourceOriginated { get; set; }
        public string RootSource { get; set; }
        public string RelationLabel { get; set; }
        public string AdditionalRelationLabel { get; set; }
        public string GroupId { get; set; }
        public int AttributeCount { get; set; }
        public string RelatedId { get; set; }
        public string RelatedIdName { get; set; }
    }
}
