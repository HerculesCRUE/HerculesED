using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Hercules.ED.GraphicEngine.Models.Graficas
{
    public class GraficaNodos : GraficaBase
    {
        public string title { get; set; }
        public string container { get; set; }
        public Layout layout { get; set; }
        public List<Style> style { get; set; }
        public List<DataItemRelacion> elements { get; set; }
        public bool userZoomingEnabled { get; set; }
        public bool zoomingEnabled { get; set; }
        public float minZoom { get; set; }
        public float maxZoom { get; set; }

        public override byte[] GenerateCSV()
        {
            StringBuilder csv = new StringBuilder("");
            List<string> nombres = new List<string>();
            List<double?> valores = new List<double?>();
            foreach (DataItemRelacion item in elements)
            {
                if (item.data.group == "nodes")
                {
                    nombres.Add(item.data.name.Substring(0,item.data.name.Length-1 - item.data.name.LastIndexOf('(')));
                    valores.Add(item.data.score);
                }
            }

            csv.AppendLine("\"" + String.Join("\";\"", elements[0].data.score).Replace("\"", "\"\"") + "\"");
            csv.AppendLine("\"" + String.Join("\";\"", elements[0].data.score).Replace("\"", "\"\"") + "\"");
            return Encoding.ASCII.GetBytes(csv.ToString());
        }
    }

    public class Layout
    {
        public string name { get; set; }
        public int idealEdgeLength { get; set; }
        public int nodeOverlap { get; set; }
        public int refresh { get; set; }
        public bool fit { get; set; }
        public int padding { get; set; }
        public bool randomize { get; set; }
        public int componentSpacing { get; set; }
        public int nodeRepulsion { get; set; }
        public int edgeElasticity { get; set; }
        public int nestingFactor { get; set; }
        public int gravity { get; set; }
        public int numIter { get; set; }
        public int initialTemp { get; set; }
        public float coolingFactor { get; set; }
        public float minTemp { get; set; }
    }

    public class Style
    {
        public string selector { get; set; }
        public LayoutStyle style { get; set; }
    }

    public class LayoutStyle
    {
        public string width { get; set; }
        public string content { get; set; }
        [JsonPropertyName("font-size")]
        public string font_size { get; set; }
        [JsonPropertyName("font-family")]
        public string font_family { get; set; }
        [JsonPropertyName("background-color")]
        public string background_color { get; set; }
        [JsonPropertyName("overlay-padding")]
        public string overlay_padding { get; set; }
        [JsonPropertyName("z-index")]
        public string z_index { get; set; }
        public string height { get; set; }
        [JsonPropertyName("text-outline-width")]
        public string text_outline_width { get; set; }
        [JsonPropertyName("curve-style")]
        public string curve_style { get; set; }
        [JsonPropertyName("haystack-radius")]
        public string haystack_radius { get; set; }
        public string opacity { get; set; }
        [JsonPropertyName("line-color")]
        public string line_color { get; set; }
    }

    public class DataItemRelacion
    {
        public Data data { get; set; }
        public bool? selectable { get; set; }
        public bool? grabbable { get; set; }

        public DataItemRelacion(Data pData, bool? pSelectable, bool? pGrabbable)
        {
            this.data = pData;
            this.selectable = pSelectable;
            this.grabbable = pGrabbable;
        }
    }

    public class Data
    {
        public enum Type
        {
            none,
            icon_member,
            icon_ip,
            icon_area,
            icon_group,
            relation_document,
            relation_project
        }
        public string id { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public string target { get; set; }
        public double? weight { get; set; }
        public string group { get; set; }
        public string type { get; set; }
        public double? score { get; set; }

        public Data(string pId, string pName, string pSource, string pTarget, double? pWeight, string pGroup, Type pType)
        {
            this.id = pId;
            this.name = pName;
            this.source = pSource;
            this.target = pTarget;
            this.weight = pWeight;
            this.group = pGroup;
            this.type = pType.ToString();
        }
    }

    public class DataQueryRelaciones
    {
        public string nombreRelacion { get; set; }
        public List<Datos> idRelacionados { get; set; }
    }
    public class Datos
    {
        public string idRelacionado { get; set; }
        public int numVeces { get; set; }        
    }
}
