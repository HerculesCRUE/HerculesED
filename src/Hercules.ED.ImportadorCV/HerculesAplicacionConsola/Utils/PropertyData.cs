using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.Utils
{
    /// <summary>
    /// Clase para la obtención de datos de la BBDD
    /// </summary>
    public class PropertyData
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Grafo en el que buscar los 'hijos' de la propiedad
        /// </summary>
        public string graph { get; set; }
        /// <summary>
        /// Propiedad para ordenar los valores
        /// </summary>
        public string order { get; set; }
        /// <summary>
        /// Propiedades 'hijas' 
        /// </summary>
        public List<PropertyData> childs { get; set; }

    }
}