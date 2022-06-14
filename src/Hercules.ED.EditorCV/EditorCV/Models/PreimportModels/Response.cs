using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EditorCV.Models.PreimportModels
{
    public class Response
    {
        private ConcurrentDictionary<int, API.Response.Tab> respuesta { get; set; }
        private string cvn_xml { get; set; }

        public Response(ConcurrentDictionary<int, API.Response.Tab> respuesta, string cvn_xml)
        {
            this.respuesta = respuesta;
            this.cvn_xml = cvn_xml;
        }
    }
}
