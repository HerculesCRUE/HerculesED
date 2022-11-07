using System;
using System.Collections.Generic;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    public class FreetextKeyword
    {
        public string Source { get; set; }
        public List<string> freetextKeyword { get; set; }
    }

    public class DataIssued
    {
        public DateTime? DatimeTime { get; set; }
    }

    public class TopicsEnriquecido
    {
        public string Word { get; set; }
        public string Porcentaje { get; set; }
    }

    public class FreetextKeywordEnriquecida
    {
        public string Word { get; set; }
        public string Porcentaje { get; set; }
    }

    public class Name
    {
        public List<string> Given { get; set; }
        public List<string> Familia { get; set; }
        public List<string> Nombre_completo { get; set; }
    }

    public class PersonaPub
    {
        public string IdGnoss { get; set; }
        public string ID { get; set; }
        public string Id_persona { get; set; }
        public Name Name { get; set; }
        public string Orcid { get; set; }
        public List<string> IDs { get; set; }
        public List<string> Links { get; set; }
        public string Fuente { get; set; }
        public string ResearcherID { get; set; }
        public string Nick { get; set; }
        public int? Orden { get; set; }
    }

    public class KnowledgeArea
    {
        public string Name { get; set; }
        public string HasCode { get; set; }
    }

    public class HasKnowledgeArea
    {
        public List<KnowledgeArea> KnowledgeArea { get; set; }
        public string Resource { get; set; }
    }

    public class HasMetric
    {
        public string Quartile { get; set; }
        public string Ranking { get; set; }
        public string ImpactFactorName { get; set; }
        public string ImpactFactor { get; set; }
        public string CitationCount { get; set; }
        public string MetricName { get; set; }
    }

    public class HasPublicationVenue
    {
        public string Type { get; set; }
        public List<string> Issn { get; set; }
        public List<string> Isbn { get; set; }
        public string Name { get; set; }
        public string Eissn { get; set; }
        public List<HasMetric> HasMetric { get; set; }
    }
    public class Publication
    {
        public string ID { get; set; }
        public object Problema { get; set; }
        public string TypeOfPublication { get; set; }
        public string Title { get; set; }
        public List<FreetextKeyword> freetextKeywords { get; set; }
        public List<string> EtiquetasUsuario { get; set; }
        public string @abstract { get; set; }
        public string Language { get; set; }
        public string Doi { get; set; }
        public DataIssued DataIssued { get; set; }
        public List<string> Url { get; set; }
        public string Pdf { get; set; }
        public List<TopicsEnriquecido> TopicsEnriquecidos { get; set; }
        public List<FreetextKeywordEnriquecida> FreetextKeywordEnriquecidas { get; set; }
        public List<string> CategoriasUsuario { get; set; }
        public PersonaPub CorrespondingAuthor { get; set; }
        public List<PersonaPub> SeqOfAuthors { get; set; }
        public List<HasKnowledgeArea> HasKnowledgeAreas { get; set; }
        public string PageEnd { get; set; }
        public string PageStart { get; set; }
        public string Volume { get; set; }
        public string ArticleNumber { get; set; }
        public bool? OpenAccess { get; set; }
        public List<string> IDs { get; set; }
        public Conferencia Conferencia { get; set; }
        public HasPublicationVenue HasPublicationVenue { get; set; }
        public List<HasMetric> HasMetric { get; set; }
        public List<Bibliografia> Bibliografia { get; set; }
        public List<Publication> Citas { get; set; }
        public List<string> DataOriginList { get; set; }
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

    public class Bibliografia
    {
        public string Doi { get; set; }
        public string Url { get; set; }
        public int? AnyoPublicacion { get; set; }
        public string Titulo { get; set; }
        public string Revista { get; set; }
        public Dictionary<string, string> Autores { get; set; }
    }
}