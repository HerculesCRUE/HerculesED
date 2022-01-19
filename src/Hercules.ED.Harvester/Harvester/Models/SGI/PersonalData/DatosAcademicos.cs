using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class DatosAcademicos : SGI_Base
    {
        public NivelAcademico NivelAcademico { get; set; }
        public string Fecha { get; set; }
    }
}
