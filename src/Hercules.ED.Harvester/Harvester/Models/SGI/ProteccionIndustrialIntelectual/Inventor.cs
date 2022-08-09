namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class Inventor
    {
        public int id { get; set; }
        public string invencionId { get; set; }
        public string inventorRef { get; set; }
        public float participacion { get; set; }
        public bool? repartoUniversidad { get; set; }
        public bool? activo { get; set; }
    }
}
