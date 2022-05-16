using System;
using System.Collections.Generic;

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
        public List<SectorAplicacion> sectoresAplicacion { get; set; }
        public List<AreaConocimiento> areasConocimiento { get; set; }
        public List<PalabraClave> palabrasClave { get; set; }
        public List<Inventor> inventores { get; set; }
        public List<PeriodoTitularidad> periodosTitularidad { get; set; }
        public List<SolicitudProteccion> solicitudesProteccion { get; set; }
        public List<Titular> titulares { get; set; }
     }
}
