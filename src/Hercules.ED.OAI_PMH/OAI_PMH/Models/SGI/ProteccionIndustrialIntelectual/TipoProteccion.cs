namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    /// <summary>
    /// TipoProteccion
    /// </summary>
    public class TipoProteccion : SGI_Base
    {
        /// <summary>
        /// Id.
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// Nombre.
        /// </summary>
        public string nombre { get; set; }
        /// <summary>
        /// Descripcion.
        /// </summary>
        public string descripcion { get; set; }
        /// <summary>
        /// PadreId.
        /// </summary>
        public int? padreId { get; set; }
        /// <summary>
        /// TipoPropiedad.
        /// </summary>
        public string tipoPropiedad { get; set; }   
        /// <summary>
        /// Activo.
        /// </summary>
        public bool? activo { get; set; }
    }
}
