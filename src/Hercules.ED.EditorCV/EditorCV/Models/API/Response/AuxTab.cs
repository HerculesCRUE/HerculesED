using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EditorCV.Models.API.Response
{
    public abstract class AuxTab
    {
        /// <summary>
        /// Título del tab
        /// </summary>
        public string title { get; set; }
        public string rdftype { get; set; }
        public string entityid { get; set; }
    }
}
