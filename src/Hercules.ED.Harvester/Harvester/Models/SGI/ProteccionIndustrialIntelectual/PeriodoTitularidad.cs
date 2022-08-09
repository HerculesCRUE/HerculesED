using System;

namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class PeriodoTitularidad
    {
        public int id { get; set; }
        public int invencionId { get; set; }
        public DateTime fechaInicio { get; set; }
        public DateTime fechaFin { get; set; }
    }
}
