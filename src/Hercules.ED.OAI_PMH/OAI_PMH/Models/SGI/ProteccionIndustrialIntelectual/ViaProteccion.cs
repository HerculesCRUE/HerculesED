namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class ViaProteccion : SGI_Base
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public int tipoPropiedad { get; set; }
        public int mesesPrioridad { get; set; }
        public bool? paisEspecifico { get; set; }
        public bool? extensionInternacional { get; set; }
        public bool? variosPaises { get; set; }
        public bool? activo { get; set; }
    }
}
