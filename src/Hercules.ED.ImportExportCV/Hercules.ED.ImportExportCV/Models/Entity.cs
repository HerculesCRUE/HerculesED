using Hercules.ED.ImportExportCV.Models;
using System.Collections.Generic;

namespace Models
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
            /// Constructor sin parametros
            /// </summary>
            public Property() => (prop, values) = (null, new List<string>());

            /// <summary>
            /// Constructor para que en caso de no pasarle valor, la propiedad también sea nula.
            /// </summary>
            /// <param name="propiedad"></param>
            public Property(string propiedad) => (prop, values) = (propiedad, new List<string>() { null });

            /// <summary>
            /// Constructor para que dada la propiedad y un listado estos se asignen.
            /// </summary>
            /// <param name="propiedad"></param>
            /// <param name="lista"></param>
            public Property(string propiedad, List<string> lista) => (prop, values) = (propiedad, lista);

            /// <summary>
            /// Constructor para que dada la propiedad y un valor este se asigne dentro de un listado.
            /// </summary>
            /// <param name="propiedad"></param>
            /// <param name="valor"></param>
            public Property(string propiedad, string valor) => (prop, values) = (propiedad, new List<string>() { valor });
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
        /// Lista de autores
        /// </summary>
        public List<Persona> autores { get; set; }
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
    }
}