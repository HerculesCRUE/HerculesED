using OAI_PMH.Models.SGI.OrganicStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class Vinculacion : SGI_Base
    {       
        public VinculacionCategoriaProfesional VinculacionCategoriaProfesional { get; set; }
        public Departamento Departamento { get; set; }
        public Centro centro { get; set; }
        public AreaConocimiento AreaConocimiento { get; set; }
        public string EmpresaRef { get; set; }
        public bool? PersonalPropio { get; set; }
        public string EntidadPropiaRef { get; set; }
    }
}
