using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class PetitionStatus
    {
        public int totalWorks { get; set; }
        public int actualWork { get; set; }
        public int subTotalWorks { get; set; }
        public int subActualWork { get; set; }
        public string actualWorkTitle { get; set; }
        public string actualWorkSubtitle { get; set; }
        public int actualSubWorks { get; set; }
        public int actualSubTotalWorks { get; set; }

        public PetitionStatus(int actualWork, int totalWorks, string actualWorkTitle)
        {
            this.actualWork = actualWork;
            this.totalWorks = totalWorks;
            this.actualWorkTitle = actualWorkTitle;
            this.actualWorkSubtitle = "";
            this.subActualWork = 0;
            this.subTotalWorks = 0;
            this.actualSubWorks = 0;
            this.actualSubTotalWorks = 0;
        }
    }
}
