using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EditorCV.Models.API.Input
{
    //Contiene la/las clases necesarias que tiene como entrada el servicio para la carga/edición de una entidad

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
            /// <summary>
            /// Valores multiidioma
            /// </summary>
            public Dictionary<string,string> valuesmultilang { get; set; }
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
        /// <summary>
        /// Lista de entidades auxiliares a eliminar
        /// </summary>
        public List<string> auxEntityRemove { get; set; }
        /// <summary>
        /// Lista de propiedades pra guardar dentro del CV
        /// </summary>
        public List<Property> properties_cv { get; set; }
        public bool IsValidated()
        {
            bool? respuesta = properties.FirstOrDefault(x => x.prop == "http://w3id.org/roh/isValidated")?.values.Contains("true");
            return respuesta.HasValue && respuesta.Value;
        }

    }
}