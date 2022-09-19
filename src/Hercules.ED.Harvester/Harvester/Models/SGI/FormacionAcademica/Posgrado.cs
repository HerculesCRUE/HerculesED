using OAI_PMH.Models.SGI.OrganicStructure;
using System;

namespace OAI_PMH.Models.SGI.FormacionAcademica
{
    public class Posgrado : SGI_Base
    {
        public string Id { get; set; }
        public string NombreTituloPosgrado { get; set; }
        public DateTime? FechaTitulacion { get; set; }
        public Entidad EntidadTitulacion { get; set; }
        public string CiudadEntidadTitulacion { get; set; }
        public Pais PaisEntidadTitulacion { get; set; }
        public ComunidadAutonoma CcaaRegionEntidadTitulacion { get; set; }
        public TipoFormacionHomologada TipoFormacionHomologada { get; set; }
        public string CalificacionObtenida { get; set; }
        public DateTime? FechaHomologacion { get; set; }
        public bool? TituloHomologado { get; set; }
    }
}
