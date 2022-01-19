using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Project
{
    public class ProyectoIVA : SGI_Base
    {
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string LastModifiedDate { get; set; }
        public string Id { get; set; }
        public string ProyectoId { get; set; }
        public double? Iva { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
    }
}