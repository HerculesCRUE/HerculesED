using System.Collections.Generic;

namespace Hercules.ED.GraphicEngine.Models
{
    public class ConfigModel
    {
        public Dictionary<string, string> nombre { get; set; }
        public string filtro { get; set; }
        public string identificador { get; set; }
        public List<Grafica> graficas { get; set; }
        public List<FacetaConf> facetas { get; set; }
    }

    public class FacetaConf
    {
        public Dictionary<string, string> nombre { get; set; }
        public string filtro { get; set; }
        public int numeroItemsFaceta { get; set; }
    }

    public class Grafica
    {
        public string identificador { get; set; }
        public Dictionary<string, string> nombre { get; set; }
        public EnumGraficas tipoGrafica { get; set; }
        public Config config { get; set; }
    }

    public class Config
    {
        public bool orientacionVertical { get; set; }
        public string ejeX { get; set; }
        public bool orderDesc { get; set; }
        public string color { get; set; }
        public bool rellenarEjeX { get; set; }
        public List<EjeYConf> yAxisPrint { get; set; }
        public List<EjeXConf> xAxisPrint { get; set; }
        public List<Dimension> dimensiones { get; set; }
    }

    public class EjeYConf
    {
        public string yAxisID { get; set; }
        public string posicion { get; set; }
    }
    public class EjeXConf
    {
        public string xAxisID { get; set; }
        public string posicion { get; set; }
    }

    public class Dimension
    {
        public Dictionary<string, string> nombre { get; set; }
        public string filtro { get; set; }
        public string color { get; set; }
        public string tipoDimension { get; set; }
        public string calculo { get; set; }
        public string stack { get; set; }
        public float anchura { get; set; }
        public string yAxisID { get; set; }
        public string xAxisID { get; set; }
        public int orden { get; set; }
    }
}
