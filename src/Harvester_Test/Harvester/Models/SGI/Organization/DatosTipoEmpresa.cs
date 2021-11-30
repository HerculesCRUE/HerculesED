using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Organization
{
    public class DatosTipoEmpresa : SGI_Base
    {
        public TipoTercero Id { get; set; }
        public TipoEmpresa Nombre { get; set; }
        public TipoEmpresaContabilidad TipoEmpresaContabilidad { get; set; }
        public TipoTerceroReinoUnido TipoTerceroReinoUnido { get; set; }
    }
}
