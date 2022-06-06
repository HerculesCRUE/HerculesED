using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Hercules.ED.GraphicEngine.Models.Graficas
{
    public class GraficaBase
    {

    }

    public class Options
    {
        public Animation animation { get; set; }
        public Plugin plugins { get; set; }
        public Dictionary<string, Eje> scales { get; set; }
        public string indexAxis { get; set; }
    }

    public class Animation
    {
        public int duration { get; set; }
    }

    public class Plugin
    {
        public Title title { get; set; }
    }

    public class Title
    {
        public bool display { get; set; }
        public string text { get; set; }
    }

    public class Eje
    {
        public string position { get; set; }
        public Title title { get; set; }
    }
}
