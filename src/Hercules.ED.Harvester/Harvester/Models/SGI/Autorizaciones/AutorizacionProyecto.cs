using OAI_PMH.Models.SGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvester.Models.SGI.Autorizaciones
{
    public class AutorizacionProyecto : SGI_Base
    {
        public int id { get; set; }
        public string solicitanteRef { get; set; }
        public string titulo { get; set; }
        public string entidadRef { get; set; }
        public string responsableRef { get; set; }
        public string datosResponsable { get; set; }
    }
}
