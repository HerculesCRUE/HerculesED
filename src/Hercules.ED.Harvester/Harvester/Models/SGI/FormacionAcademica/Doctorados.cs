using OAI_PMH.Models.SGI.OrganicStructure;
using System;

namespace OAI_PMH.Models.SGI.FormacionAcademica
{
    public class Doctorados : SGI_Base
    {
        public string Id { get; set; }
        public string ProgramaDoctorado { get; set; }
        public DateTime? FechaTitulacion { get; set; }
        public Entidad EntidadTitulacion { get; set; }
        public string CiudadEntidadTitulacion { get; set; }
        public Pais PaisEntidadTitulacion { get; set; }
        public ComunidadAutonoma CcaaRegionEntidadTitulacion { get; set; }
        public DateTime? FechaTitulacionDEA { get; set; }
        public Entidad EntidadTitulacionDEA { get; set; }
        public string TituloTesis { get; set; }
        public string CalificacionObtenida { get; set; }
        public string DirectorTesis { get; set; }
        public string CoDirectorTesis { get; set; }
        public bool? DoctoradoEuropeo { get; set; }
        public DateTime? FechaMencionDoctoradoEuropeo { get; set; }
        public bool? MencionCalidad { get; set; }
        public bool? PremioExtraordinarioDoctor { get; set; }
        public DateTime? FechaPremioExtraordinarioDoctor { get; set; }
        public bool? TituloHomologado { get; set; }
        public DateTime? FechaHomologacion { get; set; }
    }
}
