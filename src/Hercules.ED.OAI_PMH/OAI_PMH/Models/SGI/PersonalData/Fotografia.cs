using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.PersonalData
{
    /// <summary>
    /// Fotografía
    /// </summary>
    public class Fotografia : SGI_Base
    {
        /// <summary>
        /// Contenido
        /// </summary>
        public string Contenido { get; set; }
        /// <summary>
        /// MIMEType
        /// </summary>
        public string MIMEType { get; set; }
    }
}
