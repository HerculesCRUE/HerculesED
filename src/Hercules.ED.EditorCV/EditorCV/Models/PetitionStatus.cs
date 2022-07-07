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

        public PetitionStatus()
        {
            actualWork = 0;
            totalWorks = 0;
            subActualWork = 0;
            subTotalWorks = 0;
            actualWorkTitle = "";
        }
        
        public PetitionStatus(int actualWork, int totalWorks, string actualWorkTitle)
        {
            this.actualWork = actualWork;
            this.totalWorks = totalWorks;
            this.actualWorkTitle = actualWorkTitle;
            this.subActualWork = 0;
            this.subTotalWorks = 0;
        }
    }
}
