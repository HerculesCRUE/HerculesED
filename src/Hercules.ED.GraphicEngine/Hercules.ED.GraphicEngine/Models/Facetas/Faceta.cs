using System.Collections.Generic;

namespace Hercules.ED.GraphicEngine.Models
{
    public class Faceta
    {
        public string nombre { get; set; }
        public List<ItemFaceta> items { get; set; }
    }

    public class ItemFaceta
    {
        public string nombre { get; set; }
        public int numero { get; set; }
        public string filtro { get; set; }
    }
}
