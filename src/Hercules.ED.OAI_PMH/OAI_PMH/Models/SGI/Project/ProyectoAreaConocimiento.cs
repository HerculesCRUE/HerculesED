using System;

namespace OAI_PMH.Models.SGI.Project
{
    public class ProyectoAreaConocimiento : SGI_Base
    {
        public int id { get; set; }
        public int proyectoId { get; set; }
        public string areaConocimientoRef { get; set; }
        public string createdBy { get; set; }
        public DateTime? creationDate { get; set; }
        public string lastModifiedBy { get; set; }
        public DateTime? lastModifiedDate { get; set; }
    }
}
