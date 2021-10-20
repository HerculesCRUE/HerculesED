using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API
{
    /// <summary>
    /// Datos de una entidad para realizar su carga/modificación
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public class Property
        {
            /// <summary>
            /// URL
            /// </summary>
            public string prop { get; set; }
            /// <summary>
            /// Valores
            /// </summary>
            public List<string> values { get; set; }
        }
        /// <summary>
        /// Propiedad usada para el título
        /// </summary>
        public string propTitle { get; set; }
        /// <summary>
        /// Propiedad usada para la descripción
        /// </summary>
        public string propDescription { get; set; }
        /// <summary>
        /// Lista de propiedades
        /// </summary>
        public List<Property> properties { get; set; }
        /// <summary>
        /// Ontología
        /// </summary>
        public string ontology { get; set; }
        /// <summary>
        /// rdf:type
        /// </summary>
        public string rdfType { get; set; }
        /// <summary>
        /// Identificador
        /// </summary>
        public string id { get; set; }

        public List<string> auxEntityRemove { get; set; }
    }
}