using System.Collections.Generic;
using Newtonsoft.Json;
using PublicationAPI.ROs.Publication.Models;

namespace PublicationConnect.ROs.Publications.Models
{
    public class Publication
    {
        public string TypeOfPublication { get; set; }
        public string Title { get; set; }
        public List<FreetextKeywords> FreetextKeywords { get; set; }
        public string Abstract { get; set; }
        public string Language { get; set; }
        public string Doi { get; set; }
        public DateTimeValue DataIssued { get; set; }
        public HashSet<string> Url { get; set; }
        public string Pdf { get; set; }
        public List<KnowledgeEnriquecidos> Topics_enriquecidos { get; set; }
        public List<KnowledgeEnriquecidos> FreetextKeyword_enriquecidas { get; set; }
        public Person CorrespondingAuthor { get; set; }
        public List<Person> SeqOfAuthors { get; set; }
        public List<KnowledgeAreas> HasKnowledgeAreas { get; set; }
        public string PageEnd { get; set; }
        public string PageStart { get; set; }
        public string Volume { get; set; }
        public string ArticleNumber { get; set; }
        public bool? OpenAccess { get; set; }
        public List<string> IDs { get; set; }
        public string PresentedAt { get; set; }
        public Source HasPublicationVenue { get; set; }
        public List<PublicationMetric> HasMetric { get; set; }
        public List<PubReferencias> Bibliografia { get; set; }
        public HashSet<string> DataOriginList { get; set; }
        public Conferencia Conferencia { get; set; }
        public string DataOrigin { get; set; }
    }
    public class FreetextKeywords
    {
        public string Source { get; set; }
        public List<string> FreetextKeyword { get; set; }
    }

    public class Conferencia
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string Pais { get; set; }
        public string Ciudad { get; set; }
    }

    public class PublicationMetric
    {
        public string CitationCount { get; set; }
        public string MetricName { get; set; }
    }

    public class DateTimeValue
    {
        public string DatimeTime { get; set; }
    }

    public class Source
    {
        public string Type { get; set; }
        public List<string> Issn { get; set; }
        public List<string> Isbn { get; set; }
        public string Name { get; set; }
        public string Eissn { get; set; }
    }

    public class Person
    {
        public Name Name { get; set; }
        public string ORCID { get; set; }
        public List<string> IDs { get; set; }
        public List<string> Links { get; set; }
        public string Fuente { get; set; }
        public string ResearcherID { get; set; }
        public string Nick { get; set; }
        public int? Orden { get; set; }
    }

    public class Name
    {
        public List<string> Given { get; set; }
        public List<string> Familia { get; set; }
        public List<string> Nombre_completo { get; set; }
    }

    public class KnowledgeAreas
    {
        public List<KnowledgeArea> KnowledgeArea { get; set; }
        public string Resource { get; set; }
    }

    public class KnowledgeArea
    {
        public string Name { get; set; }
        public string HasCode { get; set; }
    }

    public class ObjEnriquecimientoSinPdf
    {
        public ObjEnriquecimientoSinPdf(string rotype, string title, string abstract_)
        {
            this.rotype = rotype;
            this.title = title;
            this.abstract_ = abstract_;
        }

        public string rotype { get; set; }

        public string title { get; set; }

        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
    }

    public class ObjEnriquecimientoConPdf
    {
        public string rotype { get; set; }

        [JsonProperty("pdf_url")]
        public string pdfurl { get; set; }

        public string title { get; set; }

        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
    }

    public class TopicsEnriquecidos
    {
        public string Pdf_url { get; set; }
        public string Rotype { get; set; }
        public List<KnowledgeEnriquecidos> Topics { get; set; }
    }

    public class KnowledgeEnriquecidos
    {
        public string Word { get; set; }
        public string Porcentaje { get; set; }
    }
}
