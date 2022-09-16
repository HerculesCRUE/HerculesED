using OAI_PMH.Models.SGI.OrganicStructure;
using System;

namespace OAI_PMH.Models.SGI.ActividadDocente
{
    public class SeminariosCursos : SGI_Base
    {
        public string Id { get; set; }
        public string NombreEvento { get; set; }
        //public TipoEvento TipoEvento { get; set; }
        public DateTime? FechaTitulacion { get; set; }
        public Entidad EntidadOrganizacionEvento { get; set; }
        public TipoEntidadOrganizadora TipoEntidad { get; set; }
        public string CiudadEntidadOrganizacionEvento { get; set; }
        public string ObjetivosCurso { get; set; }
        public string Idioma { get; set; }
        public string ISBN { get; set; }
        public string ISSN { get; set; }
        public bool? AutorCorrespondencia { get; set; }
        public TipoIdentificador IdentificadoresPublicacion { get; set; }
        public string PerfilDestinatarios { get; set; }
        public TipoParticipacion TipoParticipacion { get; set; }
    }
}
