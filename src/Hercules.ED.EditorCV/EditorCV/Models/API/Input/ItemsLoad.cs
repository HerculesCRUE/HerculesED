using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorCV.Models.API.Input
{
    //Contiene la/las clases necesarias que tiene como entrada el servicio para la obtención de propiedades

    /// <summary>
    /// Clase con una lista de items y sus propiedades
    /// </summary>
    public class ItemsLoad
    {
        /// <summary>
        /// lista de items y sus propiedades
        /// </summary>
        public List<LoadProp> items { get; set; }
    }
    public class LoadProp
    {
        /// <summary>
        /// Identificador del item
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Identificador del item
        /// </summary>
        public string about { get; set; }
        /// <summary>
        /// Ruta de la propiedad
        /// </summary>
        public string route { get; set; }
        /// <summary>
        /// Ruta de la propiedad
        /// </summary>
        public string routeComplete { get; set; }
        /// <summary>
        /// Valores
        /// </summary>
        public List<string> values { get; set; }

        public KeyValuePair<string, Utils.PropertyData> GenerarPropertyData()
        {
            string graph = "";
            Utils.PropertyData propertyData = new Utils.PropertyData();
            Utils.PropertyData propertyDataActual = propertyData;
            Utils.PropertyData propertyDataAnterior = propertyData;
            int num = 0;
            int total = route.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).Length;
            foreach (string routeIn in route.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries))
            {
                num++;
                if (!routeIn.StartsWith("http"))
                {
                    if (string.IsNullOrEmpty(graph))
                    {
                        graph = routeIn;
                    }
                    else
                    {
                        propertyDataAnterior.graph = routeIn;
                    }
                }
                else
                {
                    propertyDataActual.property = routeIn;
                    if (total == num)
                    {
                        break;
                    }
                    propertyDataActual.childs = new List<Utils.PropertyData>() { new Utils.PropertyData() };
                    propertyDataAnterior = propertyDataActual;
                    propertyDataActual = propertyDataActual.childs[0];
                }
            }
            return new KeyValuePair<string, Utils.PropertyData>(graph, propertyData);
        }

        public string GetPropComplete()
        {
            StringBuilder stringBuilder = new StringBuilder();            
            foreach (string routeIn in route.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.StartsWith("http")))
            {
                stringBuilder.Append(routeIn + "@@@");
            }
            string propComplete = stringBuilder.ToString();
            if (propComplete.Length >= 3)
            {
                propComplete = propComplete.Substring(0, propComplete.Length - 3);
            }
            return propComplete;
        }
    }
}