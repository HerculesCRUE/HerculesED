using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class FreetextKeyword
    {
        public string source { get; set; }
        public List<string> freetextKeyword { get; set; }
    }

    public class DataIssued
    {
        public DateTime? datimeTime { get; set; }
    }

    public class TopicsEnriquecido
    {
        public string word { get; set; }
        public string porcentaje { get; set; }
    }

    public class FreetextKeywordEnriquecida
    {
        public string word { get; set; }
        public string porcentaje { get; set; }
    }

    public class Name
    {
        public List<string> given { get; set; }
        public List<string> familia { get; set; }
        public List<string> nombre_completo { get; set; }
    }

    public class PersonaPub
    {
        public string idGnoss { get; set; }
        public string ID { get; set; }
        public string id_persona { get; set; }
        public Name name { get; set; }
        public string orcid { get; set; }
        public List<string> iDs { get; set; }
        public List<string> links { get; set; }
        public string fuente { get; set; }
        public string researcherID { get; set; }
        public string nick { get; set; }
        public int? orden { get; set; }
    }

    public class KnowledgeArea
    {
        public string name { get; set; }
        public string hasCode { get; set; }
    }

    public class HasKnowledgeArea
    {
        public List<KnowledgeArea> knowledgeArea { get; set; }
        public string resource { get; set; }
    }

    public class HasMetric
    {
        public string quartile { get; set; }
        public string ranking { get; set; }
        public string impactFactorName { get; set; }
        public string impactFactor { get; set; }
        public string citationCount { get; set; }
        public string metricName { get; set; }
    }

    public class HasPublicationVenue
    {
        public string type { get; set; }
        public List<string> issn { get; set; }
        public List<string> isbn { get; set; }
        public string name { get; set; }
        public string eissn { get; set; }
        public List<HasMetric> hasMetric { get; set; }
    }
    public class Publication
    {
        public string ID { get; set; }
        public object problema { get; set; }
        public string typeOfPublication { get; set; }
        public string title { get; set; }
        public List<FreetextKeyword> freetextKeywords { get; set; }
        public List<string> etiquetasUsuario { get; set; }
        public string @abstract { get; set; }
        public string language { get; set; }
        public string doi { get; set; }
        public DataIssued dataIssued { get; set; }
        public List<string> url { get; set; }
        public string pdf { get; set; }
        public List<TopicsEnriquecido> topics_enriquecidos { get; set; }
        public List<FreetextKeywordEnriquecida> freetextKeyword_enriquecidas { get; set; }
        public List<string> categoriasUsuario { get; set; }
        public PersonaPub correspondingAuthor { get; set; }
        public List<PersonaPub> seqOfAuthors { get; set; }
        public List<HasKnowledgeArea> hasKnowledgeAreas { get; set; }
        public string pageEnd { get; set; }
        public string pageStart { get; set; }
        public string volume { get; set; }
        public string articleNumber { get; set; }
        public bool? openAccess { get; set; }
        public List<string> iDs { get; set; }
        public Conferencia conferencia { get; set; }
        public HasPublicationVenue hasPublicationVenue { get; set; }
        public List<HasMetric> hasMetric { get; set; }
        public List<Bibliografia> bibliografia { get; set; }
        public List<Publication> citas { get; set; }
        public List<string> dataOriginList { get; set; }
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

    public class Bibliografia
    {
        public string doi { get; set; }
        public string url { get; set; }
        public int? anyoPublicacion { get; set; }
        public string titulo { get; set; }
        public string revista { get; set; }
        public Dictionary<string, string> autores { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    //public class FreetextKeyword
    //{
    //    public string source { get; set; }
    //    public List<string> freetextKeyword { get; set; }
    //}

    //public class DataIssued
    //{
    //    public DateTime? datimeTime { get; set; }
    //}

    //public class TopicsEnriquecido
    //{
    //    public string word { get; set; }
    //    public string porcentaje { get; set; }
    //}

    //public class FreetextKeywordEnriquecida
    //{
    //    public string word { get; set; }
    //    public string porcentaje { get; set; }
    //}

    //public class Name
    //{
    //    public List<string> given { get; set; }
    //    public List<string> familia { get; set; }
    //    public List<string> nombre_completo { get; set; }
    //}

    //public class CorrespondingAuthor
    //{
    //    public Name name { get; set; }
    //    public string ORCID { get; set; }
    //    public List<string> IDs { get; set; }
    //    public object links { get; set; }
    //}

    //public class SeqOfAuthor
    //{
    //    public Name name { get; set; }
    //    public string ORCID { get; set; }
    //    public List<string> IDs { get; set; }
    //    public List<string> links { get; set; }
    //}

    //public class KnowledgeArea
    //{
    //    public string name { get; set; }
    //    public string hasCode { get; set; }
    //}

    //public class HasKnowledgeArea
    //{
    //    public List<KnowledgeArea> knowledgeArea { get; set; }
    //    public string resource { get; set; }
    //}

    //public class HasPublicationVenue
    //{
    //    public string type { get; set; }
    //    public List<string> issn { get; set; }
    //    public List<string> isbn { get; set; }
    //    public string name { get; set; }
    //    public string eissn { get; set; }
    //}

    //public class HasMetric
    //{
    //    public string citationCount { get; set; }
    //    public string metricName { get; set; }
    //}

    //public class Publication
    //{
    //    public object problema { get; set; }
    //    public string typeOfPublication { get; set; }
    //    public string title { get; set; }
    //    public List<FreetextKeyword> freetextKeywords { get; set; }
    //    public string Abstract { get; set; }
    //    public string language { get; set; }
    //    public string doi { get; set; }
    //    public DataIssued dataIssued { get; set; }
    //    public List<string> url { get; set; }
    //    public object pdf { get; set; }
    //    public List<TopicsEnriquecido> topics_enriquecidos { get; set; }
    //    public List<FreetextKeywordEnriquecida> freetextKeyword_enriquecidas { get; set; }
    //    public CorrespondingAuthor correspondingAuthor { get; set; }
    //    public List<SeqOfAuthor> seqOfAuthors { get; set; }
    //    public List<HasKnowledgeArea> hasKnowledgeAreas { get; set; }
    //    public string pageEnd { get; set; }
    //    public string pageStart { get; set; }
    //    public List<string> IDs { get; set; }
    //    public object presentedAt { get; set; }
    //    public HasPublicationVenue hasPublicationVenue { get; set; }
    //    public List<HasMetric> hasMetric { get; set; }
    //    public List<Publication> bibliografia { get; set; }
    //    public List<Publication> citas { get; set; }
    //}
}