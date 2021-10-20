using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API
{
    public class ItemsLoad
    {
        public List<LoadProp> items { get; set; }
    }
    public class LoadProp
    {
        public string id { get; set; }
        public string about { get; set; }
        public string route { get; set; }
        public string routeComplete { get; set; }
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
            string propComplete = "";
            foreach (string routeIn in route.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (routeIn.StartsWith("http"))
                {
                    propComplete += routeIn+"@@@";
                }
            }
            if (propComplete.Length >= 3)
            {
                propComplete = propComplete.Substring(0, propComplete.Length - 3);
            }
            return propComplete;
        }
    }
}