using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API
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
        public Dictionary<string, List<string>> props = new Dictionary<string, List<string>>();
        /// <summary>
        /// Entidades
        /// </summary>
        public Dictionary<string, List<EntityRdf>> ents = new Dictionary<string, List<EntityRdf>>();

    }
}