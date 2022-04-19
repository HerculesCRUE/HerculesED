using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Organization
{
    public class DatosTipoEmpresa : SGI_Base
    {
        public TipoTercero TipoTercero { get; set; }
        public TipoEmpresa TipoEmpresa { get; set; }
        public TipoEmpresaContabilidad TipoEmpresaContabilidad { get; set; }
        public TipoTerceroReinoUnido TipoTerceroReinoUnido { get; set; }
    }
}
