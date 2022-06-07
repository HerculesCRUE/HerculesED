using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.GraphicEngine.Models.Graficas
{
    public class GraficaCircular : GraficaBase
    {
        public string type { get; set; }
        public Options options { get; set; }
        public DataCircular data { get; set; }
        public override byte[] GenerateCSV()
        {
            return Encoding.ASCII.GetBytes("Hola mundo");
        }
    }
    public class DataCircular
    {
        public List<string> labels { get; set; }
        public ConcurrentBag<DatasetCircular> datasets { get; set; }
    }
    public class DatasetCircular
    {
        public string label { get; set; }
        public List<float> data { get; set; }
        public List<string> backgroundColor { get; set; }
        public int hoverOffset { get; set; }
    }
}
