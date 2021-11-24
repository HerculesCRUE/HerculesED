using OAI_PMH.Models.SGI.OrganicStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Organization
{
    public class DatosContacto : SGI_Base
    {
        public Pais PaisContacto { get; set; }
        public ComunidadAutonoma ComAutonomaContacto { get; set; }
        public Provincia ProvinciaContacto { get; set; }
        public string CiudadContacto { get; set; }
        public string CodigoPostal { get; set; }
        public TipoVia TipoVia { get; set; }
        public string NombreVia { get; set; }
        public string Numero { get; set; }
        public string Ampliacion { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Telefono { get; set; }
        public string DireccionWeb { get; set; }
    }
}
