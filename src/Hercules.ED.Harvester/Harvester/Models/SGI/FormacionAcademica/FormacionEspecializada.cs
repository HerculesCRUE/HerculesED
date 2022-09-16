using OAI_PMH.Models.SGI.OrganicStructure;
using System;

namespace OAI_PMH.Models.SGI.FormacionAcademica
{
    public class FormacionEspecializada : SGI_Base
    {
        public string Id { get; set; }
        public string NombreTitulo { get; set; }
        public DateTime? FechaTitulacion { get; set; }
        public int? DuracionTitulacion { get; set; }
        public Entidad EntidadTitulacion { get; set; }
        public string CiudadEntidadTitulacion { get; set; }
        public Pais PaisEntidadTitulacion { get; set; }
        public ComunidadAutonoma CcaaRegionEntidadTitulacion { get; set; }
        public TipoFormacion TipoFormacion { get; set; }
        public string Objetivos { get; set; }
        public string ResponsableFormacion { get; set; }
    }
}
