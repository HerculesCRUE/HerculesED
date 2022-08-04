using System.Collections.Generic;

namespace WoSConnect.ROs.WoS.Models
{
    public class Publication
    {
        public List<string> problema { get; set; }
        public string typeOfPublication { get; set; }
        public string title { get; set; }
        public List<FreetextKeywords> freetextKeywords { get; set; }
        public string Abstract { get; set; }
        public string language { get; set; }
        public string doi { get; set; }
        public DateTimeValue dataIssued { get; set; }
        public Person correspondingAuthor { get; set; }
        public List<Person> seqOfAuthors { get; set; }
        public List<KnowledgeAreas> hasKnowledgeAreas { get; set; }
        public string pageEnd { get; set; }
        public string pageStart { get; set; }
        public List<string> IDs { get; set; }
        public Source hasPublicationVenue { get; set; }
        public List<PublicationMetric> hasMetric { get; set; }
        public bool? openAccess { get; set; }
        public string volume { get; set; }
        public Conferencia conferencia { get; set; }
        public string dataOrigin { get; set; }
    }

    public class Conferencia
    {
        public int id { get; set; }
        public string titulo { get; set; }
        public string fechaInicio { get; set; }
        public string fechaFin { get; set; }
        public string pais { get; set; }
        public string ciudad { get; set; }
    }

    public class FreetextKeywords
    {
        public string source { get; set; }
        public List<string> freetextKeyword { get; set; }
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
        public List<string> issn { get; set; }
        public string name { get; set; }
        public string eissn { get; set; }

    }
    public class Person
    {
        public Name name { get; set; }
        public string ORCID { get; set; }
        public string researcherID { get; set; }
        public string fuente { get; set; }
        public int? orden { get; set; }
    }
    public class Name
    {
        public List<string> given { get; set; }
        public List<string> familia { get; set; }
        public List<string> nombre_completo { get; set; }
    }
    public class KnowledgeAreas
    {
        public List<KnowledgeArea> knowledgeArea { get; set; }
        public string resource { get; set; }
    }
    public class KnowledgeArea
    {
        public string name { get; set; }
        public string hasCode { get; set; }
    }
}
