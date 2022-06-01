namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class InvencionDocumento : SGI_Base
    {
        public int id { get; set; }
        public int invencionId { get; set; }
        public string documentoRef { get; set; }
        public string nombre { get; set; }
    }
}
