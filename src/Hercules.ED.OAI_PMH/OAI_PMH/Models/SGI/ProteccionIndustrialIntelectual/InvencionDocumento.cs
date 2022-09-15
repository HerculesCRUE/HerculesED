namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    /// <summary>
    /// InvencionDocumento
    /// </summary>
    public class InvencionDocumento : SGI_Base
    {
        /// <summary>
        /// Id.
        /// </summary>
        public int? id { get; set; }
        /// <summary>
        /// InvencionId
        /// </summary>
        public int? invencionId { get; set; }
        /// <summary>
        /// DocumentoRef
        /// </summary>
        public string documentoRef { get; set; }
        /// <summary>
        /// Nombre
        /// </summary>
        public string nombre { get; set; }
    }
}
