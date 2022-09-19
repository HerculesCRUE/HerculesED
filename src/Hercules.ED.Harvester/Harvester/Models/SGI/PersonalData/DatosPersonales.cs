using OAI_PMH.Models.SGI.OrganicStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class DatosPersonales : SGI_Base
    {
        public DateTime? FechaNacimiento { get; set; }
        public Pais PaisNacimiento { get; set; }
        public ComunidadAutonoma ComAutonomaNacimiento { get; set; }
        public string CiudadNacimiento { get; set; }
    }
}
