namespace EditorCV.Models.EnvioDSpace
{
    public class Status
    {
        public string authenticated { get; set; }
        public string okay  { get; set; }

        public Status()
        {
            authenticated = "";
            okay = "";
        }
    }
}
