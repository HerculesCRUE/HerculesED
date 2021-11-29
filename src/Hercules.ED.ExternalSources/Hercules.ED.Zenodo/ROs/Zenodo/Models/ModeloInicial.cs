using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ZenodoConnect.ROs.Zenodo.Models.Inicial
{// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Links
    {
        public string download { get; set; }
        public string self { get; set; }
        public string badge { get; set; }
        public string bucket { get; set; }
        public string doi { get; set; }
        public string html { get; set; }
        public string latest { get; set; }
        public string latest_html { get; set; }
    }

    public class File
    {
        public string checksum { get; set; }
        public string filename { get; set; }
        public int filesize { get; set; }
        public string id { get; set; }
        public Links links { get; set; }
    }

    public class Creator
    {
        public string affiliation { get; set; }
        public string name { get; set; }
    }

    public class PrereserveDoi
    {
        public string doi { get; set; }
        public int recid { get; set; }
    }

    public class Metadata
    {
        public string access_right { get; set; }
        public List<Creator> creators { get; set; }
        public string description { get; set; }
        public string doi { get; set; }
        public string imprint_publisher { get; set; }
        public string journal_issue { get; set; }
        public string journal_pages { get; set; }
        public string journal_title { get; set; }
        public string journal_volume { get; set; }
        public List<string> keywords { get; set; }
        public string license { get; set; }
        public PrereserveDoi prereserve_doi { get; set; }
        public string publication_date { get; set; }
        public string publication_type { get; set; }
        public string title { get; set; }
        public string upload_type { get; set; }
    }

    public class Datum
    {
        public string conceptrecid { get; set; }
        public DateTime created { get; set; }
        public string doi { get; set; }
        public string doi_url { get; set; }
        public List<File> files { get; set; }
        public int id { get; set; }
        public Links links { get; set; }
        public Metadata metadata { get; set; }
        public DateTime modified { get; set; }
        public int owner { get; set; }
        public int record_id { get; set; }
        public string state { get; set; }
        public bool submitted { get; set; }
        public string title { get; set; }
    }

    public class Root_2
    {
        public List<Datum> data { get; set; }
    }
    
}