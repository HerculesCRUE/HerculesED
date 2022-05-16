namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class TipoProteccion : SGI_Base
    {
        public int id { get; set; }
        public string tipoPropiedad { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public int padreId { get; set; }
        public bool? activo { get; set; }
    }
}
