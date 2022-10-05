namespace EditorCV.Models.EnvioDSpace
{
    public class Metadata
    {
        public string key { get; set; }
        public string value { get; set; }

        public Metadata(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public Metadata()
        {
            this.key = "";
            this.value = "";
        }
    }
}
