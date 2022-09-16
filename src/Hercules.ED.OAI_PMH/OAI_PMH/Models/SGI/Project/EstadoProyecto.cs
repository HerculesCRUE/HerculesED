using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Project
{
    public class EstadoProyecto : SGI_Base
    {
        public object CreatedBy { get; set; }
        public object CreationDate { get; set; }
        public object LastModifiedBy { get; set; }
        public object LastModifiedDate { get; set; }
        public int Id { get; set; }
        public int ProyectoId { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaEstado { get; set; }
        public string Comentario { get; set; }
    }
}