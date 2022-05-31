namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class InvencionGastos : SGI_Base
    {
        public int id { get; set; }
        public int invencionId { get; set; }
        public string gastoRef { get; set; }
        public string estado { get; set; }
        public float importePendienteDeducir { get; set; }
        public int solicitudProteccion { get; set; }
    }
}
