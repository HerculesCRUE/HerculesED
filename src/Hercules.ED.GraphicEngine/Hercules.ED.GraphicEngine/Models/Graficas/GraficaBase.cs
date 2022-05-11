﻿using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Hercules.ED.GraphicEngine.Models.Graficas
{
    public class GraficaBase
    {
        public string type { get; set; }
        public Data data { get; set; }
        public Options options { get; set; }
    }

    public class Options
    {
        public Plugin plugins { get; set; }
        public Dictionary<string, EjeY> scales { get; set; }
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

    public class EjeY
    {
        public string position { get; set; }
    }

    public class Data
    {
        public List<string> labels { get; set; }
        public ConcurrentBag<Dataset> datasets { get; set; }  
        public string type { get; set; }
    }

    public class Dataset
    {
        public string label { get; set; }
        public List<float> data { get; set; }
        public List<string> backgroundColor { get; set; }
        public string type { get; set; }
        public string stack { get; set; }
        public float barPercentage { get; set; }
        public string yAxisID { get; set; }
    }
}
