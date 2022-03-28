using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using OpenAireConnect.ROs.OpenAire.Controllers;

namespace OpenAireConnect.ROs.OpenAire.Models.Inicial
{

    public class Header
    {
        //public string query { get; set; }
        //public string locale { get; set; }
        public Total size { get; set; }
        public Total page { get; set; }
        public Total total { get; set; }

        [JsonProperty("-xmlns:xsi")]
        public string XmlnsXsi { get; set; }

        [JsonProperty("dri:objIdentifier")]
        public object DriObjIdentifier { get; set; }

        [JsonProperty("dri:dateOfCollection")]
        public object DriDateOfCollection { get; set; }

        [JsonProperty("dri:dateOfTransformation")]
        public object DriDateOfTransformation { get; set; }
    }

    public class Bestaccessright
    {
        [JsonProperty("-classid")]
        public string Classid { get; set; }

        [JsonProperty("-classname")]
        public string Classname { get; set; }

        [JsonProperty("-schemeid")]
        public string Schemeid { get; set; }

        [JsonProperty("-schemename")]
        public string Schemename { get; set; }
    }

    public class Creator
    {
        [JsonProperty("@rank")]
        public string Rank { get; set; }

        [JsonProperty("@URL")]
        public string URL { get; set; }

        [JsonProperty("@orcid")]
        public string Orcid { get; set; }

        [JsonProperty("$")]
        public string Text { get; set; }

        [JsonProperty("@orcid_pending")]
        public string OrcidPending { get; set; }

        [JsonProperty("@name")]
        public string Name { get; set; }

        [JsonProperty("@surname")]
        public string Surname { get; set; }
    }

    public class Subject
    {
        [JsonProperty("@classid")]
        public string Classid { get; set; }

        [JsonProperty("@classname")]
        public string Classname { get; set; }

        [JsonProperty("$")]
        public string Text { get; set; }
    }

    public class Total
    {

        [JsonProperty("$")]
        public string Text { get; set; }
    }
    public class Language
    {
        [JsonProperty("@classid")]
        public string Classid { get; set; }

        [JsonProperty("@classname")]
        public string Classname { get; set; }



    }

    public class Resulttype
    {
        [JsonProperty("-classid")]
        public string Classid { get; set; }

        [JsonProperty("-classname")]
        public string Classname { get; set; }

        [JsonProperty("-schemeid")]
        public string Schemeid { get; set; }

        [JsonProperty("-schemename")]
        public string Schemename { get; set; }
    }

    public class Resourcetype
    {
        [JsonProperty("@classid")]
        public string Classid { get; set; }

        [JsonProperty("@classname")]
        public string Classname { get; set; }

        [JsonProperty("@schemeid")]
        public string Schemeid { get; set; }

        [JsonProperty("@schemename")]
        public string Schemename { get; set; }
    }

    public class Provenanceaction
    {
        [JsonProperty("-classid")]
        public string Classid { get; set; }

        [JsonProperty("-classname")]
        public string Classname { get; set; }

        [JsonProperty("-schemeid")]
        public string Schemeid { get; set; }

        [JsonProperty("-schemename")]
        public string Schemename { get; set; }
    }

    public class Rels
    {
        public object rel { get; set; }
    }

    public class Title
    {
        [JsonProperty("@classid")]
        public string Classid { get; set; }

        [JsonProperty("@classname")]
        public string Classname { get; set; }

        [JsonProperty("-schemeid")]
        public string Schemeid { get; set; }

        [JsonProperty("-schemename")]
        public string Schemename { get; set; }

        [JsonProperty("-inferred")]
        public string Inferred { get; set; }

        [JsonProperty("-provenanceaction")]
        public string Provenanceaction { get; set; }

        [JsonProperty("-trust")]
        public string Trust { get; set; }

        [JsonProperty("$")]
        public string Text { get; set; }
    }

    public class Result2
    {
        public Metadata metadata { get; set; }
    }

    public class Children
    {
        public List<Result2> result { get; set; }
        public object instance { get; set; }
    }

    public class Country
    {
        [JsonProperty("-classid")]
        public string Classid { get; set; }

        [JsonProperty("-classname")]
        public string Classname { get; set; }

        [JsonProperty("-schemeid")]
        public string Schemeid { get; set; }

        [JsonProperty("-schemename")]
        public string Schemename { get; set; }
    }

    public class OafResult
    {
        [JsonConverter(typeof(SingleOrArrayConverter<Title>))]
        public List<Title> pid { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<Title>))]
        public List<Title> title { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<Creator>))]
        public List<Creator> creator { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<Descripton>))]
        public List<Descripton> description { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<Subject>))]
        public List<Subject> subject { get; set; }

        public Language language { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<Relevantdate>))]
        public List<Relevantdate> relevantdate { get; set; }

        public Resourcetype resourcetype { get; set; }

        public Journal journal { get; set; }
    }

    public class Relevantdate
    {
        [JsonProperty("@classid")]
        public string Classid { get; set; }

        [JsonProperty("@classname")]
        public string Classname { get; set; }

        [JsonProperty("@schemeid")]
        public string Schemeid { get; set; }

        [JsonProperty("@schemename")]
        public string Schemename { get; set; }

        [JsonProperty("$")]
        public string date { get; set; }
    }

    public class Dateofacceptance
    {
        [JsonProperty("$")]
        public string date { get; set; }
    }
    public class Descripton
    {
        [JsonProperty("$")]
        public string descripton { get; set; }
    }

    public class Id
    {
        [JsonProperty("@value")]
        public string Value { get; set; }

        [JsonProperty("-type")]
        public string Type { get; set; }

        [JsonProperty("-trustLevel")]
        public string TrustLevel { get; set; }
    }

    public class Citation
    {
        [JsonProperty("-position")]
        public string Position { get; set; }
        public string rawText { get; set; }
        public Id id { get; set; }
    }

    public class Citations
    {
        public List<Citation> citation { get; set; }
    }

    public class ExtraInfo
    {
        [JsonProperty("-name")]
        public string Name { get; set; }

        [JsonProperty("-typology")]
        public string Typology { get; set; }

        [JsonProperty("-provenance")]
        public string Provenance { get; set; }

        [JsonProperty("-trust")]
        public string Trust { get; set; }
        public Citations citations { get; set; }
    }
    public class Journal
    {
        [JsonProperty("@eissn")]
        public string Eissn { get; set; }

        [JsonProperty("@ep")]
        public string Ep { get; set; }

        [JsonProperty("@iss")]
        public string Iss { get; set; }

        [JsonProperty("@sp")]
        public string Sp { get; set; }

        [JsonProperty("@vol")]
        public string Vol { get; set; }

        [JsonProperty("$")]
        public string Text { get; set; }
    }

    public class OafEntity
    {
        [JsonProperty("oaf:result")]
        public OafResult OafResult { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("oaf:entity")]
        public OafEntity OafEntity { get; set; }
    }

    public class Results
    {
        public List<Result2> result { get; set; }
    }



    public class Response
    {
        public Header header { get; set; }
        public Results results { get; set; }
    }

    public class Root
    {
        public Response response { get; set; }
    }

}