using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.GraphicEngine.Models.Graficas
{
    public class GraficaBarras : GraficaBase
    {
        public string type { get; set; }
        public Options options { get; set; }
        public DataBarras data { get; set; }
        public override byte[] GenerateCSV()
        {
            return Encoding.ASCII.GetBytes("Hola mundo");
        }
    }
    public class DataBarras
    {
        public List<string> labels { get; set; }
        public ConcurrentBag<DatasetBarras> datasets { get; set; }
        public string type { get; set; }
    }
    public class DatasetBarras
    {
        public string label { get; set; }
        public List<float> data { get; set; }
        public List<string> backgroundColor { get; set; }
        public string type { get; set; }
        public string stack { get; set; }
        public float barPercentage { get; set; }
        public float maxBarThickness { get; set; }
        public string yAxisID { get; set; }
        public int order { get; set; }
    }
}
