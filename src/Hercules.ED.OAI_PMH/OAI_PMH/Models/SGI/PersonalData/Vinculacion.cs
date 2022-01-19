using OAI_PMH.Models.SGI.OrganicStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class Vinculacion : SGI_Base
    {
        public CategoriaProfesional CategoriaProfesionalPDI { get; set; }
        public CategoriaProfesional CategoriaProfesionalPAS { get; set; }
        public DateTime FechaObtencionCategoriaPDI { get; set; }
        public DateTime FechaObtencionCategoriaPAS { get; set; }
        public DateTime FechaFinCategoriaPDI { get; set; }
        public DateTime FechaFinCategoriaPAS { get; set; }
        public Departamento DepartamentoPDI { get; set; }
        public Departamento DepartamentoPAS { get; set; }
        public AreaConocimiento AreaConocimiento { get; set; }
        public string EmpresaRef { get; set; }
    }
}
