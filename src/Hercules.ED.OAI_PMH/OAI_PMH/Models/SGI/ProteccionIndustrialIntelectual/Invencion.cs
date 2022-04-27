using System;

namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class Invencion : SGI_Base
    {
        public int id { get; set; }
        public string titulo { get; set; }
        public DateTime fechaComunicacion { get; set; }
        public string descripcion { get; set; }
        public TipoProteccion tipoProteccion { get; set; }
        public string proyectoRef { get; set; }
        public string comentarios { get; set; }
    }
}
