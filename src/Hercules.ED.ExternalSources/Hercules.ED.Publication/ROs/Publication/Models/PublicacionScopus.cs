using System.Collections.Generic;

namespace PublicationAPI.ROs.Publication.Models
{
    public class PublicacionScopus
    {
        public string typeOfPublication { get; set; }
        public string title { get; set; }
        public string doi { get; set; }
        public string datimeTime { get; set; }
        public HashSet<string> url { get; set; }
        public Person correspondingAuthor { get; set; }
        public string pageEnd { get; set; }
        public string pageStart { get; set; }
        public string volume { get; set; }
        public string scopusID { get; set; }
        public Source hasPublicationVenue { get; set; }
        public List<PublicationMetric> hasMetric { get; set; }
    }
    public class PublicationMetric
    {
        public string citationCount { get; set; }
        public string metricName { get; set; }
    }
    public class DateTimeValue
    {
        public string datimeTime { get; set; }
    }

    public class Source
    {
        public string type { get; set; }
        public string name { get; set; }
        public List<string> issn { get; set; }
        public string eissn { get; set; }
    }
    public class Person
    {
        public string nick { get; set; }
    }
}
