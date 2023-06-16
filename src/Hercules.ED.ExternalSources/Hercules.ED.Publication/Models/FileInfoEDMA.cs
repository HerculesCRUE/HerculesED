using System;

namespace PublicationAPI.Models
{
    public class FileInfoEDMA
    {
        public string Name { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public long BytesSize { get; set; }
    }
}
