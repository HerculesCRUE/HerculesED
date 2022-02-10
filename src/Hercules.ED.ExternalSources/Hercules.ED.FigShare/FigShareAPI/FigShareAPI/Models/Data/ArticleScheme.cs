using System;

namespace FigShareAPI.Models.Data
{
    public class TimelineScheme
    {
        public string posted { get; set; }
        public string submission { get; set; }
        public string revision { get; set; }
        public string firstOnline { get; set; }
        public string publisherPublication { get; set; }
        public string publisherAcceptance { get; set; }
    }

    public class ArticleScheme
    {
        public int id { get; set; }
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
        public TimelineScheme timeline { get; set; }
        public string thumb { get; set; }
        public int? defined_type { get; set; }
        public string defined_type_name { get; set; }
    }
}
