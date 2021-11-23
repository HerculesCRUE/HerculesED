using System;
using System.Collections.Generic;

namespace PublicationConnect.ROs.Publications.Models
{

public class Publication 
{
    public List<string> problema {get;set;}
    public string typeOfPublication { get; set; } //no es un atributo de la ontologia!!
    public string title { get; set; }
    public List<FreetextKeywords>  freetextKeywords { get; set; }
    public string Abstract { get; set; }
    public string language { get; set; } 
    public string doi { get; set; }
   // public string identifier {get;set;}
    public DateTimeValue dataIssued { get; set; }

    public List<string> url { get; set; }
    public Person correspondingAuthor { get; set; }

    public List<Person> seqOfAuthors { get; set; }
  //  public Organization correspondingOrganization { get; set; }
    public List<KnowledgeAreas> hasKnowledgeAreas { get; set; }
    public string pageEnd { get; set; }

    public string pageStart { get; set; }
    //public Status documentStatus { get; set; }
    //public string eanucc13 { get; set; }
    public List<string> IDs {get;set;}
     public string presentedAt { get; set; }  
    //todo no creo que esto en nuestra ontologia sea un string y no esta contemplado de mommento rellenarlo! 

    public Source hasPublicationVenue { get; set; }
    public List<PublicationMetric> hasMetric { get; set; }
    public List<Publication> bibliografia { get; set; }
    public List<Publication> citas {get;set;}
    

}
public class FreetextKeywords {
    public string source {get;set;}
    public List<string> freetextKeyword {get;set;}
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
      public string type {get;set;}
      public List<string> issn { get; set; }
      public List<string> isbn {get;set;}
      public string name { get; set; }
      public string eissn { get; set; }
    //  public string oclcnum { get; set; }
  }


public class JournalMetric
{
    public string quartile { get; set; }
    public string ranking { get; set; }
    public string impactFactorName { get; set; }
    public float impactFactor { get; set; }
    public string metricName { get; set; }
}

// public class Status
// {
//     public string status { get; set; }
//     public DateTimeValue dateIssued { get; set; }
// }

public class Person
{
    //public DateTimeValue birthdate { get; set; }
    public List<string> name { get; set; } 
    //public string surname { get; set; }
    public string ORCID {get;set;}
    public List<string> IDs { get; set; }
    public List<string> links { get; set; }
    //public string nick { get; set; }

}
public class Organization
{
    public string title { get; set; }

}
public class Conference
{
    //todo! esto mirar! 
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
    public List<KnowledgeArea> knowledgeArea {get;set;}
    public string resource {get;set;}
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
}
