using OAI_PMH.Models.SGI.OrganicStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class Vinculacion : SGI_Base
    {
        public List<VinculacionCategoriaProfesional> VinculacionesCategoriaProfesionales { get; set; }
        public List<VinculacionDepartamentos> VinculacionesDepartamentos { get; set; }
        public Centro centro { get; set; }
        public AreaConocimiento AreaConocimiento { get; set; }
        public string EmpresaRef { get; set; }
        public bool? PersonalPropio { get; set; }
        public string EntidadPropiaRef { get; set; }
        // TODO:
        public CategoriaProfesional CategoriaProfesional { get; set; }
        public DateTime? FechaObtencionCategoria { get; set; }
        public Departamento Departamento { get; set; }
    }
}
