using OaiPmhNet;
using OaiPmhNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Models.OAIPMH
{
    public class MetadataFormatRepository : IMetadataFormatRepository
    {
        private readonly Dictionary<string, MetadataFormat> _dictionary;
        
        public MetadataFormatRepository()
        {
            MetadataFormat xml = new("EDMA", "", "", "");
            _dictionary = new Dictionary<string, MetadataFormat>
            {
                { "EDMA", xml }
            };
        }

        public MetadataFormat GetMetadataFormat(string prefix)
        {
            if (_dictionary.TryGetValue(prefix, out MetadataFormat format))
                return format;
            else
                return null;
        }

        public IEnumerable<MetadataFormat> GetMetadataFormats()
        {
            return _dictionary.Select(x => x.Value);
        }
    }
}