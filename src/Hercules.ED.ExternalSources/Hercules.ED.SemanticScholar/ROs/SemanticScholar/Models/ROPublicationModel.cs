using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SemanticScholarAPI.ROs.SemanticScholar.Models
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
        public List<string> url { get; set; }
        public string pdf { get; set; }
        public List<Knowledge_enriquecidos> topics_enriquecidos { get; set; }
        public List<Knowledge_enriquecidos> freetextKeyword_enriquecidas { get; set; }
        public Person correspondingAuthor { get; set; }
        public List<Person> seqOfAuthors { get; set; }
        public List<KnowledgeAreas> hasKnowledgeAreas { get; set; }
        public string pageEnd { get; set; }
        public string pageStart { get; set; }
        public List<string> IDs { get; set; }
        public string presentedAt { get; set; }
        public Source hasPublicationVenue { get; set; }
        public List<PublicationMetric> hasMetric { get; set; }
        public List<Publication> bibliografia { get; set; }
        public List<Publication> citas { get; set; }
        public string dataOrigin { get; set; }
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
        // public KnowledgeArea hasKnowledgeArea { get; set; }
        //  public JournalMetric hasMetric { get; set; }
        // public string abbreviation { get; set; }
        //public string language { get; set; }
        //  public Organization publisher { get; set; }
        //public Organization correspongingOrganization { get; set; }
        public string type { get; set; }
        public List<string> issn { get; set; }
        public List<string> isbn { get; set; }
        public string name { get; set; }
        public string eissn { get; set; }
        public List<JournalMetric> hasMetric { get; set; }

        //  public string oclcnum { get; set; }
    }


    public class JournalMetric
    {
        public string quartile { get; set; }
        public string ranking { get; set; }
        public string impactFactorName { get; set; }
        public string impactFactor { get; set; }
        public string metricName { get; set; }
    }

    // public class Status
    // {
    //     public string status { get; set; }
    //     public DateTimeValue dateIssued { get; set; }
    // }

    public class Person
    {
        //  public string id_persona {get;set;}

        //public DateTimeValue birthdate { get; set; }
        public Name name { get; set; }
        //public string surname { get; set; }
        public string ORCID { get; set; }
        public List<string> IDs { get; set; }
        public List<string> links { get; set; }
        //public string nick { get; set; }
        public string fuente { get; set; }

    }

    public class Name
    {
        public List<string> given { get; set; }
        //
        public List<string> familia { get; set; }
        public List<string> nombre_completo { get; set; }
    }
    public class Organization
    {
        public string title { get; set; }

    }
    public class Conference
    {
        public string abbreviation { get; set; }
        public DateTimeInterval dateTimeInterval { get; set; }
        public string description { get; set; }
        public List<string> IDs { get; set; }
        public string title { get; set; }
        public string freetextKeyword { get; set; }
        public string locality { get; set; }
        //public ParticipatedBy participatedBy { get; set; }
        public List<KnowledgeAreas> hasKnowledgeArea { get; set; }

    }
    public class KnowledgeAreas
    {
        public List<KnowledgeArea> knowledgeArea { get; set; }
        public string resource { get; set; }
    }

    public class KnowledgeArea
    {
        public string name { get; set; }
        // public string abbreviation { get; set; }
        public string hasCode { get; set; }


    }

    public class DateTimeInterval
    {
        public DateTimeValue end { get; set; }
        public DateTimeValue start { get; set; }
    }




    public class Topics_enriquecidos
    {
        public string pdf_url { get; set; }
        public string rotype { get; set; }
        public List<Knowledge_enriquecidos> topics { get; set; }
    }

    public class Knowledge_enriquecidos
    {
        public string word { get; set; }
        public string porcentaje { get; set; }
    }

    public class palabras_enriquecidas
    {
        public List<Knowledge_enriquecidos> topics { get; set; }
        public string title { get; set; }
        [JsonProperty("abstract")]
        public string abstract_ { get; set; }


    }

    public class enriquecimiento
    {
        public string rotype { get; set; }
        [JsonProperty("pdf_url")]
        public string pdfurl { get; set; }
        public string title { get; set; }

        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
        public string author_name { get; set; }
        public string journal { get; set; }

    }

    public class enriquecimiento_sin_pdf
    {
        public string rotype { get; set; }
        // [JsonProperty("pdf_url")]
        //public string pdfurl {get;set;}
        public string title { get; set; }

        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
        public string author_name { get; set; }
        public string journal { get; set; }

    }

    public class enriquecimiento_palabras
    {
        public string rotype { get; set; }
        // [JsonProperty("pdf_url")]
        //public string pdfurl {get;set;}
        public string title { get; set; }

        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
    }


}
