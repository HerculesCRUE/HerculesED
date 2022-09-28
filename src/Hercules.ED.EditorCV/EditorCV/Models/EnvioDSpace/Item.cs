namespace EditorCV.Models.EnvioDSpace
{
    public class Item
    {
        public long id { get; set; }
        public string name { get; set; }
        public string handle { get; set; }
        public string type { get; set; }
        public string link { get; set; }
        public string[] expand { get; set; }
        public string lastModified { get; set; }
        public object parentCollection { get; set; }
        public object parentCollectionList { get; set; }
        public object parentCommunityList { get; set; }
        public object bitstreams { get; set; }
        public string archived { get; set; }
        public string withdraw { get; set; }
    }
}
