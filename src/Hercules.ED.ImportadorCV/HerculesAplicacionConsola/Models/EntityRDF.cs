using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    /// <summary>
    /// Clases usadas para el guardado de una ontología
    /// </summary>
    public class EntityRdf
    {
        /// <summary>
        /// Identificador
        /// </summary>
        public string id;
        /// <summary>
        /// rdf:type
        /// </summary>
        public string rdfType;
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