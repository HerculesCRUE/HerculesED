using System;
using System.Collections.Generic;

namespace FigShareAPI.Models.Data
{
    public class File
    {
        public int? id { get; set; }
        public string name { get; set; }
        public int? size { get; set; }
        public bool? is_link_only { get; set; }
        public string download_url { get; set; }
        public string supplied_md5 { get; set; }
        public string computed_md5 { get; set; }
    }

    public class Author
    {
        public int? id { get; set; }
        public string full_name { get; set; }
        public bool? is_active { get; set; }
        public string url_name { get; set; }
        public string orcid_id { get; set; }
    }

    public class CustomField
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class EmbargoOption
    {
        public int? id { get; set; }
        public string type { get; set; }
        public string ip_name { get; set; }
    }

    public class Category
    {
        public int? parent_id { get; set; }
        public int? id { get; set; }
        public string title { get; set; }
        public string path { get; set; }
        public string source_id { get; set; }
        public int? taxonomy_id { get; set; }
    }

    public class License
    {
        public int? value { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Timeline
    {
        public string posted { get; set; }
        public string submission { get; set; }
        public string revision { get; set; }
        public string firstOnline { get; set; }
        public string publisherPublication { get; set; }
        public string publisherAcceptance { get; set; }
    }

    public class FundingList
    {
        public int? id { get; set; }
        public string title { get; set; }
        public string grant_code { get; set; }
        public string funder_name { get; set; }
        public int? is_user_defined { get; set; }
    }

    public class Article
    {
        public string figshare_url { get; set; }
        public string resource_title { get; set; }
        public string resource_doi { get; set; }
        public List<File> files { get; set; }
        public List<Author> authors { get; set; }
        public List<CustomField> custom_fields { get; set; }
        public List<EmbargoOption> embargo_options { get; set; }
        public string citation { get; set; }
        public string confidential_reason { get; set; }
        public string embargo_type { get; set; }
        public bool? is_confidential { get; set; }
        public int? size { get; set; }
        public string funding { get; set; }
        public List<FundingList> funding_list { get; set; }
        public List<string> tags { get; set; }
        public int? version { get; set; }
        public bool? is_active { get; set; }
        public bool? is_metadata_record { get; set; }
        public string metadata_reason { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public bool? is_embargoed { get; set; }
        public DateTime? embargo_date { get; set; }
        public bool? is_public { get; set; }
        public DateTime? modified_date { get; set; }
        public DateTime? created_date { get; set; }
        public bool? has_linked_file { get; set; }
        public List<Category> categories { get; set; }
        public License license { get; set; }
        public string embargo_title { get; set; }
        public string embargo_reason { get; set; }
        public List<string> references { get; set; }
        public int? id { get; set; }
        public string title { get; set; }
        public string doi { get; set; }
        public string handle { get; set; }
        public int? group_id { get; set; }
        public string url { get; set; }
        public string url_public_html { get; set; }
        public string url_public_api { get; set; }
        public string url_private_html { get; set; }
        public string url_private_api { get; set; }
        public DateTime? published_date { get; set; }
        public Timeline timeline { get; set; }
        public string thumb { get; set; }
        public int? defined_type { get; set; }
        public string defined_type_name { get; set; }
    }
}
