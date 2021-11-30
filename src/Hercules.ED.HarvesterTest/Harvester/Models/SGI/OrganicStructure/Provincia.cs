using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.OrganicStructure
{
    public class Provincia : SGI_Base
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string ComunidadAutonomaId { get; set; }
    }
}
