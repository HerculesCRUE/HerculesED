using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Project
{
    public class RolProyecto : SGI_Base
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string LastModifiedDate { get; set; }
        public string Abreviatura { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool RolPrincipal { get; set; }
        public string Orden { get; set; }
        public string Equipo { get; set; }
        public bool Activo { get; set; }
    }
}