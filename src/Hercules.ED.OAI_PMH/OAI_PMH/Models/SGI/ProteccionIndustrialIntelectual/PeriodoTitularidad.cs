using System;

namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    /// <summary>
    /// PeriodoTitularidad
    /// </summary>
    public class PeriodoTitularidad
    {
        /// <summary>
        /// Id.
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// InvencionId.
        /// </summary>
        public int invencionId { get; set; }
        /// <summary>
        /// FechaInicio
        /// </summary>
        public DateTime? fechaInicio { get; set; }
        /// <summary>
        /// FechaFin
        /// </summary>
        public DateTime? fechaFin { get; set; }
    }
}
