using OAI_PMH.Models.SGI.OrganicStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class DatosContacto : SGI_Base
    {
        public Pais PaisContacto { get; set; }
        public ComunidadAutonoma ComAutonomaContacto { get; set; }
        public Provincia ProvinciaContacto { get; set; }
        public string CiudadContacto { get; set; }
        public string DireccionContacto { get; set; }
        public string CodigoPostalContacto { get; set; }
        public List<Email> Emails { get; set; }
        public List<string> Telefonos { get; set; }
        public List<string> Moviles { get; set; }
    }
}
