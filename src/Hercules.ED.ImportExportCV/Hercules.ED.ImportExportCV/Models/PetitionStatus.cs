namespace Models
{
    public class PetitionStatus
    {
        public int totalWorks { get; set; }
        public int actualWork { get; set; }
        public string actualWorkTitle { get; set; }
        public int actualSubTotalWorks { get; set; }
        public int actualSubWorks { get; set; }
        public string actualWorkSubtitle { get; set; }

        public PetitionStatus()
        {
            actualWork = 0;
            totalWorks = 0;
            actualWorkTitle = "";
            actualWorkSubtitle = "";
            actualSubWorks = 0;
            actualSubTotalWorks = 0;
        }

        public PetitionStatus(int actualWork, int totalWorks, string actualWorkTitle)
        {
            this.actualWork = actualWork;
            this.totalWorks = totalWorks;
            this.actualWorkTitle = actualWorkTitle;
        }
    }
}
