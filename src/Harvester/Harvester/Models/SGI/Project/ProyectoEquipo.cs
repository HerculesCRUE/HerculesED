using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Project
{
    public class ProyectoEquipo : SGI_Base
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string LastModifiedDate { get; set; }
        public string ProyectoId { get; set; }
        public string PersonaRef { get; set; }
        public RolProyecto RolProyecto { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public int? HorasDedicacion { get; set; }
    }
}
