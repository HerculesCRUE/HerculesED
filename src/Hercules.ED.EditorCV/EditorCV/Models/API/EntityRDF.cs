using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EditorCV.Models.API
{
    /// <summary>
    /// Clases usadas para el guardado de una ontología
    /// </summary>
    public class EntityRdf
    {
        /// <summary>
        /// Identificador
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// rdf:type
        /// </summary>
        public string rdfType { get; set; }
        /// <summary>
        /// Propiedades
        /// </summary>
        public Dictionary<string, List<string>> props { get; set; }
        /// <summary>
        /// Entidades
        /// </summary>
        public Dictionary<string, List<EntityRdf>> ents { get; set; }

    }
}