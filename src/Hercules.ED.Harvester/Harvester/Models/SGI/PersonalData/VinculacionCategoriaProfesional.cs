using System;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class VinculacionCategoriaProfesional : SGI_Base
    {
        public CategoriaProfesional CategoriaProfesional { get; set; }
        public DateTime? FechaObtencion { get; set; }
        public DateTime? FechaFin { get; set; }
        public string TipoPersonal { get; set; }
    }
}
