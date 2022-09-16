namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    /// <summary>
    /// Titular
    /// </summary>
    public class Titular
    {
        /// <summary>
        /// Identificador.
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Periodo de titularidad.
        /// </summary>
        public int periodoTitularidadId { get; set; }
        /// <summary>
        /// RefTitular.
        /// </summary>
        public string titularRef { get; set; }
        /// <summary>
        /// Participación.
        /// </summary>
        public float participacion { get; set; }
    }
}
