using OAI_PMH.Models.SGI.OrganicStructure;
using System;

namespace OAI_PMH.Models.SGI.ActividadDocente
{
    public class FormacionAcademicaImpartida : SGI_Base
    {
        public string Id { get; set; }
        public string TitulacionUniversitaria { get; set; }
        public string NombreAsignaturaCurso { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public Entidad EntidadRealizacion { get; set; }
        public string CiudadEntidadRealizacion { get; set; }
        public Pais PaisEntidadRealizacion { get; set; }
        public ComunidadAutonoma CcaaRegionEntidadRealizacion { get; set; }
        public TipoDocente TipoDocente { get; set; }
        public double? NumHorasCreditos { get; set; }
        public double? FrecuenciaActividad { get; set; }
        public TipoPrograma TipoPrograma { get; set; }
        public TipoDocencia TipoDocencia { get; set; }
        public string Departamento { get; set; }
        public TipoAsignatura TipoAsignatura { get; set; }
        public string Curso { get; set; }
        public TipoHorasCreditos TipoHorasCreditos { get; set; }
        public string Idioma { get; set; }
        public string Competencias { get; set; }
        public string CategoriaProfesional { get; set; }        
    }
}
