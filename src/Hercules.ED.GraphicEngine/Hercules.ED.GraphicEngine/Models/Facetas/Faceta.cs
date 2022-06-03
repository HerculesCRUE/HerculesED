using System.Collections.Generic;

namespace Hercules.ED.GraphicEngine.Models
{
    public class Faceta
    {
        public string id { get; set; }
        public bool isDate { get; set; }
        public string nombre { get; set; }
        public int numeroItemsFaceta { get; set; }
        public bool ordenAlfaNum { get; set; }
        public bool tesauro { get; set; }
        public List<ItemFaceta> items { get; set; }
    }

    public class ItemFaceta
    {
        public string nombre { get; set; }
        public int numero { get; set; }
        public string filtro { get; set; }
        public string idTesauro { get; set; }
        public List<ItemFaceta> childsTesauro { get; set; }
    }
}
