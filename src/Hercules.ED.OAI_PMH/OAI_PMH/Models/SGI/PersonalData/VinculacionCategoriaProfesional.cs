using System;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class VinculacionCategoriaProfesional : SGI_Base
    {
        public CategoriaProfesional CategoriaProfesional { get; set; }
        public DateTime FechaObtencionCategoria { get; set; }
    }
}
