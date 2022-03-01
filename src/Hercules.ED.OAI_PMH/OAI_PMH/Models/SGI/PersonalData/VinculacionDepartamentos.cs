using OAI_PMH.Models.SGI.OrganicStructure;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class VinculacionDepartamentos : SGI_Base
    {
        public Departamento Departamento { get; set; }
        public string TipoPersonal { get; set; }
    }
}
