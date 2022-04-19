namespace OAI_PMH.Models.SGI.Project
{
    public class ProyectoClasificacion : SGI_Base
    {
        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string LastModifiedDate { get; set; }
        public int ProyectoId { get; set; }
        public string ClasificacionRef { get; set; }
    }
}
