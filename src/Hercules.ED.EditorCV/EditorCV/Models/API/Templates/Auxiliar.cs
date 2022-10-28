using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EditorCV.Models.API.Templates
{  
    /// <summary>
    /// Clase genérica para la configuración de las propiedades
    /// </summary>
    public class PropertyDataTemplate
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Grafo en el que buscar los 'hijos'
        /// </summary>
        public string graph { get; set; }
        /// <summary>
        /// 'Hijos'
        /// </summary>
        public PropertyDataTemplate child { get; set; }

        /// <summary>
        /// Orden
        /// </summary>
        public string order { get; set; }

        /// <summary>
        /// Texto auxiliar para el titulo
        /// </summary>
        public Dictionary<string,string> auxTitle { get; set; }

        public Utils.PropertyData GenerarPropertyData(string pGraph)
        {
            Utils.PropertyData propertyData = new Utils.PropertyData()
            {
                property = this.property,
                order = order,
                childs = new List<Utils.PropertyData>()
            };
            if (this.child != null)
            {
                string graphAux = this.graph;
                if (string.IsNullOrEmpty(graphAux))
                {
                    graphAux = pGraph;
                }
                propertyData.graph = graphAux;
                Utils.UtilityCV.GenerarPropertyData(this, ref propertyData, graphAux);
                Utils.UtilityCV.CleanPropertyData(ref propertyData);
            }
            return propertyData;
        }

        public string GetRoute()
        {
            string route = property;
            if (!string.IsNullOrEmpty(graph))
            {
                route += "||" + graph;
            }
            if (child != null)
            {
                route += "||" + child.GetRoute();
            }
            while (route.EndsWith("||"))
            {
                route = route.Substring(0, route.Length - 2);

            }
            return route;
        }
    }
}


