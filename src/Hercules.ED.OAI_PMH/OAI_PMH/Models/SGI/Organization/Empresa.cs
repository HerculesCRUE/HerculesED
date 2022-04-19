using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Organization
{
    public class Empresa : SGI_Base
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public TipoIdentificador TipoIdentificador { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string RazonSocial { get; set; }                
        public bool? DatosEconomicos { get; set; }
        public string PadreId { get; set; }
        public DatosContacto DatosContacto { get; set; }
    }
}
