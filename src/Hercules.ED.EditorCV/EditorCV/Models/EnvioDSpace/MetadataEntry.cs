namespace EditorCV.Models.EnvioDSpace
{
    public class MetadataEntry
    {
        public string key { get; set; }
        public string value { get; set; }
        public string language { get; set; }

        public MetadataEntry(string key, string value, string language)
        {
            this.key = key;
            this.value = value;
            this.language = language;
        }

        public MetadataEntry(string key, string value)
        {
            this.key = key;
            this.value = value;
            this.language = null;
        }

        public MetadataEntry()
        {
            this.key = "";
            this.value = "";
            this.language = null;
        }
    }
}
