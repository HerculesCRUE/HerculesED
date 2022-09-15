using System;
using System.Collections.Generic;

namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    /// <summary>
    /// Invencion (PII)
    /// </summary>
    public class Invencion : SGI_Base
    {
        /// <summary>
        /// Id.
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Título.
        /// </summary>
        public string titulo { get; set; }
        /// <summary>
        /// FechaComunicacion.
        /// </summary>
        public DateTime? fechaComunicacion { get; set; }
        /// <summary>
        /// Descripción.
        /// </summary>
        public string descripcion { get; set; }
        /// <summary>
        /// TipoProtecciónId.
        /// </summary>
        public int tipoProteccionId { get; set; }
        /// <summary>
        /// ProyectoRef.
        /// </summary>
        public string proyectoRef { get; set; }
        /// <summary>
        /// Comentarios.
        /// </summary>
        public string comentarios { get; set; }
        /// <summary>
        /// SectoresAplicacion.
        /// </summary>
        public List<SectorAplicacion> sectoresAplicacion { get; set; }
        /// <summary>
        /// InvencionDocumentos.
        /// </summary>
        public List<InvencionDocumento> invencionDocumentos { get; set; }
        /// <summary>
        /// Gastos.
        /// </summary>
        public List<InvencionGastos> gastos { get; set; }
        /// <summary>
        /// PalabrasClave.
        /// </summary>
        public List<PalabraClave> palabrasClave { get; set; }
        /// <summary>
        /// Inventores.
        /// </summary>
        public List<Inventor> inventores { get; set; }
    }
}
