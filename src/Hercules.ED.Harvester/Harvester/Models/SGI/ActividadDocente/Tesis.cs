using OAI_PMH.Models.SGI.OrganicStructure;
using System;

namespace OAI_PMH.Models.SGI.ActividadDocente
{
    public class Tesis : SGI_Base
    {
        public string Id { get; set; }
        public string TituloTrabajo { get; set; }
        public DateTime? FechaDefensa { get; set; }
        public string Alumno { get; set; }
        public Entidad EntidadRealizacion { get; set; }
        public string CiudadEntidadRealizacion { get; set; }
        public Pais PaisEntidadRealizacion { get; set; }
        public ComunidadAutonoma CcaaRegionEntidadRealizacion { get; set; }
        public TipoTrabajoDirigido TipoProyecto { get; set; }
        public string CalificacionObtenida { get; set; }
        public Director CoDirectorTesis { get; set; }
        public bool? DoctoradoEuropeo { get; set; }
        public DateTime? FechaMencionDoctoradoEuropeo { get; set; }
        public bool? MencionCalidad { get; set; }
        public DateTime? FechaMencionCalidad { get; set; }
    }
}
