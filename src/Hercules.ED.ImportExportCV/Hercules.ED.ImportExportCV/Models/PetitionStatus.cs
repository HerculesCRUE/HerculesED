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
        public string actualWorkTitle { get; set; }
        public string actualWorkSubtitle { get; set; }

        public PetitionStatus()
        {
            actualWork = 0;
            totalWorks = 0;
            actualWorkTitle = "";
            actualWorkSubtitle = "";
        }
        
        public PetitionStatus(int actualWork, int totalWorks, string actualWorkTitle)
        {
            this.actualWork = actualWork;
            this.totalWorks = totalWorks;
            this.actualWorkTitle = actualWorkTitle;
        }

        public PetitionStatus(int actualWork, int totalWorks, string actualWorkTitle, string actualWorkSubtitle)
        {
            this.actualWork = actualWork;
            this.totalWorks = totalWorks;
            this.actualWorkTitle = actualWorkTitle;
            this.actualWorkSubtitle = actualWorkSubtitle;
        }
    }
}
