using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Project
{
    public class ProyectoEntidadConvocante : SGI_Base
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string LastModifiedDate { get; set; }
        public string ProyectoId { get; set; }
        public string EntidadRef { get; set; }
        public Programa ProgramaConvocatoria { get; set; }
        public Programa Programa { get; set; }
    }
}
