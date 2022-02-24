using System.Collections.Generic;

namespace SemanticScholarAPI.ROs.SemanticScholar.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ExternalIds
    {
        public string ArXiv { get; set; }
        public string DBLP { get; set; }
        public string PubMedCentral { get; set; }
        public int? ORCID { get; set; }
    }

    public class Author
    {
        public string authorId { get; set; }
        public ExternalIds externalIds { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public List<string> aliases { get; set; }
        public List<string> affiliations { get; set; }
        public string homepage { get; set; }
        public int? paperCount { get; set; }
        public int? citationCount { get; set; }
        public int? hIndex { get; set; }
    }

    public class Citation
    {
        public string paperId { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string venue { get; set; }
        public int? year { get; set; }
        public List<Author> authors { get; set; }
    }

    public class Reference
    {
        public string doi { get; set; }
        public string paperId { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string venue { get; set; }
        public int? year { get; set; }
        public List<Author> authors { get; set; }
    }

    public class Embedding
    {
        public string model { get; set; }
        public List<double> vector { get; set; }
    }

    public class Tldr
    {
        public string model { get; set; }
        public string text { get; set; }
    }

    public class SemanticScholarObj
    {
        public string paperId { get; set; }
        public ExternalIds externalIds { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string @abstract { get; set; }
        public string venue { get; set; }
        public int? year { get; set; }
        public int? referenceCount { get; set; }
        public int? citationCount { get; set; }
        public int? influentialCitationCount { get; set; }
        public bool isOpenAccess { get; set; }
        public List<string> fieldsOfStudy { get; set; }
        public List<Author> authors { get; set; }
        public List<Citation> citations { get; set; }
        public List<Reference> references { get; set; }
        public Embedding embedding { get; set; }
        public Tldr tldr { get; set; }
    }
}
