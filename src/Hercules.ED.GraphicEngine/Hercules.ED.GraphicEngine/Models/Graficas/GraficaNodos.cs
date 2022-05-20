using System.Collections.Generic;

namespace Hercules.ED.GraphicEngine.Models.Graficas
{
    public class GraficaNodos : GraficaBase
    {
        public List<DataItemRelacion> listaItems { get; set; }
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
