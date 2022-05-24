using System.Collections.Generic;

namespace Hercules.ED.GraphicEngine.Models.Paginas
{
    public class Pagina
    {
        public string id { get; set; }
        public string nombre { get; set; }        
        public List<ConfigPagina> listaConfigGraficas { get; set; }
        public List<string> listaIdsFacetas { get; set; }
    }
    public class ConfigPagina
    {
        public string id { get; set; }
        public int anchura { get; set; }
    }
}
