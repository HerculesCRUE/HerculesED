using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.GraphicEngine.Models.Graficas
{
    public class GraficaBarrasY : GraficaBase
    {
        public string type { get; set; }
        public Options options { get; set; }
        public DataBarrasY data { get; set; }
    }
    public class DataBarrasY
    {
        public List<string> labels { get; set; }
        public ConcurrentBag<DatasetBarrasY> datasets { get; set; }
        public string type { get; set; }
    }
    public class DatasetBarrasY
    {
        public string label { get; set; }
        public List<float> data { get; set; }
        public List<string> backgroundColor { get; set; }
        public string type { get; set; }
        public string stack { get; set; }
        public float barPercentage { get; set; }
        public float maxBarThickness { get; set; }
        public string xAxisID { get; set; }
        public int order { get; set; }
    }
}
