using System;
using System.Collections.Generic;

namespace PublicationConnect.ROs.Publications.Models
{
// public class Url 
// {
//     public string link { get; set; }
//    // public string description { get; set; }
// }
public class Publication 
{
    public string typeOfPublication { get; set; } //no es un atributo de la ontologia!!
    public string title { get; set; }
    public List<string> freetextKeyword { get; set; }
    public string Abstract { get; set; }
    public string language { get; set; } 
    public string doi { get; set; }
    public string identifier {get;set;}
    public DateTimeValue dataIssued { get; set; }

    public List<string> url { get; set; }
    public Person correspondingAuthor { get; set; }

    public List<Person> seqOfAuthors { get; set; }
    public Organization correspondingOrganization { get; set; }
    public List<KnowledgeArea> hasKnowledgeArea { get; set; }
    public string pageEnd { get; set; }

    public string pageStart { get; set; }
    public Status documentStatus { get; set; }
    public string eanucc13 { get; set; }
    public List<string> IDs {get;set;}

    public Journal hasPublicationVenue { get; set; }
    public List<PublicationMetric> hasMetric { get; set; }
    // TODO preguntar equipo!!1
    public List<Publication> bibliografia { get; set; }
    public List<Publication> citas {get;set;}
}
   
public class ConferencePaper : Publication
{
    public Conference presentedAt { get; set; }  
    
}
public class WorkshopPaper : ConferencePaper
{
}
  public class JournalArticle : Publication
{
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

  public class Journal 
  {
   // public KnowledgeArea hasKnowledgeArea { get; set; }
      public JournalMetric hasMetric { get; set; }
      public string abbreviation { get; set; }
      //public string language { get; set; }
      public Organization publisher { get; set; }
      //public Organization correspongingOrganization { get; set; }
      public string issn { get; set; }
      public string name { get; set; }
      public string eissn { get; set; }
      public string oclcnum { get; set; }
  }

public class JournalMetric
{
    public string quartile { get; set; }
    public string ranking { get; set; }
    public string impactFactorName { get; set; }
    public float impactFactor { get; set; }
    public string metricName { get; set; }
}
public class Status
{
    public string status { get; set; }
    public DateTimeValue dateIssued { get; set; }
}

public class Person
{
    //public DateTimeValue birthdate { get; set; }
    public List<String> name { get; set; } 

    public string surname { get; set; }
    public string ORCID {get;set;}

    public List<string> identifier { get; set; }
    public List<string> link { get; set; }
    //public string nick { get; set; }

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
    public string identifier { get; set; }
    public string title { get; set; }
    public string freetextKeyword { get; set; }
    public string locality { get; set; }
    //public ParticipatedBy participatedBy { get; set; }
    public KnowledgeArea hasKnowledgeArea { get; set; }

}
public class KnowledgeArea
{
    public string name { get; set; }
    public string abbreviation { get; set; }
    public string hasCode { get; set; }
}
public class DateTimeInterval
{
    public DateTimeValue end { get; set; }
    public DateTimeValue start { get; set; }
}


}
