using System;
using System.Collections.Generic;

namespace ZenodoAPI.ROs.Zenodo.Models
{
    public class Bucket
    {
        public int doc_count { get; set; }
        public string key { get; set; }
        public Subtype subtype { get; set; }
    }

    public class AccessRight
    {
        public List<Bucket> buckets { get; set; }
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
    }

    public class FileType
    {
        public List<Bucket> buckets { get; set; }
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
    }

    public class Keywords
    {
        public List<Bucket> buckets { get; set; }
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
    }

    public class Subtype
    {
        public List<object> buckets { get; set; }
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
    }

    public class Type
    {
        public List<Bucket> buckets { get; set; }
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
    }

    public class Aggregations
    {
        public AccessRight access_right { get; set; }
        public FileType file_type { get; set; }
        public Keywords keywords { get; set; }
        public Type type { get; set; }
    }

    public class Links
    {
        public string self { get; set; }
        public string badge { get; set; }
        public string bucket { get; set; }
        public string conceptbadge { get; set; }
        public string conceptdoi { get; set; }
        public string doi { get; set; }
        public string html { get; set; }
        public string latest { get; set; }
        public string latest_html { get; set; }
        public string prev { get; set; }
    }

    public class File
    {
        public string bucket { get; set; }
        public string checksum { get; set; }
        public string key { get; set; }
        public Links links { get; set; }
        public int size { get; set; }
        public string type { get; set; }
    }

    public class Community
    {
        public string id { get; set; }
    }

    public class Creator
    {
        public string affiliation { get; set; }
        public string name { get; set; }
        public string orcid { get; set; }
    }

    public class License
    {
        public string id { get; set; }
    }

    public class RelatedIdentifier
    {
        public string identifier { get; set; }
        public string relation { get; set; }
        public string scheme { get; set; }
    }

    public class LastChild
    {
        public string pid_type { get; set; }
        public string pid_value { get; set; }
    }

    public class Parent
    {
        public string pid_type { get; set; }
        public string pid_value { get; set; }
    }

    public class Version
    {
        public int count { get; set; }
        public int index { get; set; }
        public bool is_last { get; set; }
        public LastChild last_child { get; set; }
        public Parent parent { get; set; }
    }

    public class Relations
    {
        public List<Version> version { get; set; }
    }

    public class ResourceType
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Contributor
    {
        public string affiliation { get; set; }
        public string name { get; set; }
        public string orcid { get; set; }
        public string type { get; set; }
    }

    public class Metadata
    {
        public string access_right { get; set; }
        public string access_right_category { get; set; }
        public List<Community> communities { get; set; }
        public List<Creator> creators { get; set; }
        public string description { get; set; }
        public string doi { get; set; }
        public List<string> keywords { get; set; }
        public string language { get; set; }
        public License license { get; set; }
        public string notes { get; set; }
        public string publication_date { get; set; }
        public List<RelatedIdentifier> related_identifiers { get; set; }
        public Relations relations { get; set; }
        public ResourceType resource_type { get; set; }
        public string title { get; set; }
        public string version { get; set; }
        public List<Contributor> contributors { get; set; }
    }

    public class Stats
    {
        public double downloads { get; set; }
        public double unique_downloads { get; set; }
        public double unique_views { get; set; }
        public double version_downloads { get; set; }
        public double version_unique_downloads { get; set; }
        public double version_unique_views { get; set; }
        public double version_views { get; set; }
        public double version_volume { get; set; }
        public double views { get; set; }
        public double volume { get; set; }
    }

    public class Hit
    {
        public string conceptdoi { get; set; }
        public string conceptrecid { get; set; }
        public DateTime created { get; set; }
        public string doi { get; set; }
        public List<File> files { get; set; }
        public int id { get; set; }
        public Links links { get; set; }
        public Metadata metadata { get; set; }
        public List<int> owners { get; set; }
        public int revision { get; set; }
        public Stats stats { get; set; }
        public DateTime updated { get; set; }
        public List<Hit> hits { get; set; }
        public int total { get; set; }
    }

    public class Hits
    {
        public List<Hit> hits { get; set; }
        public int total { get; set; }
    }

    public class ResearchObject
    {
        public Aggregations aggregations { get; set; }
        public Hits hits { get; set; }
        public Links links { get; set; }
    }
}
