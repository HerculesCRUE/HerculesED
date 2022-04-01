using System.Collections.Generic;

namespace Hercules.ED.UMLS.Models.Data
{
    public class Data
    {
        public Result snomedTerm  { get; set; }
        public List<ResultRelations> relations { get; set; }
    }
}
