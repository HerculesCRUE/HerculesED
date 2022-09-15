using System;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class VinculacionCategoriaProfesional : SGI_Base
    {
        public CategoriaProfesional categoriaProfesional { get; set; }
        public DateTime fechaObtencion { get; set; }
    }
}
