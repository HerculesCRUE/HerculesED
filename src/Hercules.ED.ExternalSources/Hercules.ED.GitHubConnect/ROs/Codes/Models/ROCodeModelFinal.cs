using System;
using System.Collections.Generic;

namespace GitHubAPI.ROs.Codes.Models
{
    public class repositorio_roh 
    {
        public string title { get; set; }
        public string description {get;set;}
        public Person correspondingAuthor { get; set; }
        public List<Person> seqOfAuthors { get; set; }
        public List<FileFolderFinal> fileFolder {get; set;}
        public Readmee hasReadme {get; set;}
        public List<TagsFinal> tags { get; set; }

        public List<KnowledgeArea> hasKnowledgeArea { get; set; }
        public vivoLicense hasLicense {get; set;}
        // mirar bien esto! 
        // public repositorio_roh hasPredecessor {get; set;}
        public Status repositoryStatus {get; set;}
        public List<string> language { get; set; } 


        public InfoCommits commit {get; set;}

        //public string Abstract { get; set; }
        public InfoIssues infoIssues {get;set;}
        public List<string> freetextKeyword { get; set; }
        public DateTimeValue dataIssued { get; set; }
        public InfoForks infoforks {get; set;}
    }

    public class InfoForks {
        public int nForks {get;set;}
        public bool isFork {get;set;}

    }
    public class InfoCommits{
        public string nCommit {get; set;}
        public DateTimeValue lastCommit {get;set;}
    }
    public class DateTimeValue
    {
        public string datimeTime { get; set; }
    }
    public class FileFolderFinal
    {
        public string path { get; set; }
        public string name { get; set; }
    }

    public class Status
    {
        public DateTimeInterval dateTimeInterval {get; set;}
        public string typeStatus {get; set;}
    } 


    public class repositoryStatus 
    {
        public List<Status> statusRepository {get;set;}
    }

    public class DateTimeInterval
    {
        public DateTimeValue end { get; set; }
        public DateTimeValue start { get; set; }
    }
    // ¿?
    public class Readmee 
    {
        public List<Url> url { get; set; }
        public string typeOfPublication {get;set;} //no es un atributo de la ontologia!!
        //public List<KnowledgeArea> hasKnowledgeArea { get; set; }
        //public DateTimeValue dataIssued { get; set; }
        // TODO ver si esto es verdad... comentar con el equipo 
        // public Person correspondingAuthor { get; set; }
        //public string eanucc13 { get; set; }
        //public string pageEnd { get; set; }
        //public string doi { get; set; }
        //public List<Person> seqOfAuthors { get; set; }
        //public Organization correspondingOrganization { get; set; }
        //public string pageStart { get; set; }
        //public Status documentStatus { get; set; }
        public string title {get;set;}
        //public List<string> freetextKeyword { get; set; }
        //public string Abstract { get; set; }
        //public string language { get; set; } 

    }

    public class vivoLicense {
        public List<string> title { get; set; }
        public List<KnowledgeArea> hasKnowledgeArea { get; set; }
        public DateTimeValue dataIssued { get; set; }
        public string description { get; set; }
        public DateTimeValue dateIssued { get; set; }
        public Organization hasGoverningAuthority {get; set;}
        public List<Url> url {get; set;}
    }

    public class Organization
    {
        public string title { get; set; }
    }

    public class Person
    {
        //public DateTimeValue birthdate { get; set; }
        public List<String> name { get; set; } 
        public string surname { get; set; }
        public string identifier { get; set; }
        public List<Url> link { get; set; }
        //public string nick { get; set; }  
    }

    public class KnowledgeArea
    {
        public string name { get; set; }
        public string abbreviation { get; set; }
        public string hasCode { get; set; }
    }

    public class Url 
    {
        public string link { get; set; }
        //public string description { get; set; }
    }

    //------------------------- 
    public class TagsFinal
    {
        public string name { get; set; }
        public List<Url> links {get;set;}
    }

    //----------------------------------------------
    public class InfoIssues{
        public int nIssues {get;set;}
        public int nIssuesOpen {get;set;}
        public int nIssuesClosed {get;set;}
        public List<IssueFinal> Issues {get;set;}

        public DateTimeValue lastIssuedOpen {get;set;}
        public DateTimeValue lastIssuedClosed {get;set;}

    }

    public class IssueFinal {
        public string title {get; set;}
        public bool open {get; set;}
        public List<Url> links {get;set;} //html_url
        public DateTimeValue dateIssued {get;set;} //created_at
        public DateTimeValue dateClosed {get; set;} //closed_at
    }
}
