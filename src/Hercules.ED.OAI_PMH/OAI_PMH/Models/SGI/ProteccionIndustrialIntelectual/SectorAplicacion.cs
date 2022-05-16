namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class SectorAplicacion
    {
        public int id { get; set; }
        public int invencionId { get; set; }
        public SectorAplicacion sectorAplicacion { get; set; }
    }
}
