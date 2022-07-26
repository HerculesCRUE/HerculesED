using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.ImportExportCV.Models.FuentesExternas
{
    public class Conferencia
    {
        public int id { get; set; }
        public string titulo { get; set; }
        public string fechaInicio { get; set; }
        public string fechaFin { get; set; }
        public string pais { get; set; }
        public string ciudad { get; set; }
    }
}
