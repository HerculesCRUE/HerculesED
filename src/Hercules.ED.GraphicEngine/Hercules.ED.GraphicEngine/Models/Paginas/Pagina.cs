using System.Collections.Generic;

namespace Hercules.ED.GraphicEngine.Models.Paginas
{
    public class Pagina
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public List<string> listaIdsGraficas { get; set; }
        public List<string> listaIdsFacetas { get; set; }
    }
}
