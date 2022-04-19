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
        public string property;
        /// <summary>
        /// Grafo en el que buscar los 'hijos'
        /// </summary>
        public string graph;
        /// <summary>
        /// 'Hijos'
        /// </summary>
        public PropertyDataTemplate child;

        /// <summary>
        /// Orden
        /// </summary>
        public string order;

        public Utils.PropertyData GenerarPropertyData(string pGraph)
        {
            Utils.PropertyData property = new Utils.PropertyData()
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
                property.graph = graphAux;
                Utils.UtilityCV.GenerarPropertyData(this, ref property, graphAux);
                Utils.UtilityCV.CleanPropertyData(ref property);
            }
            return property;
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


