using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class Persona : SGI_Base
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public Sexo Sexo { get; set; }
        public string NumeroDocumento { get; set; }
        public TipoDocumento TipoDocumento { get; set; }
        public string EmpresaRef { get; set; }
        public bool? PersonalPropio { get; set; }
        public string EntidadPropiaRef { get; set; }
        public List<Email> Emails { get; set; }
        public bool? Activo { get; set; }
        public DatosPersonales DatosPersonales { get; set; }
        public DatosContacto DatosContacto { get; set; }
        public Vinculacion Vinculacion { get; set; }
        public CategoriaProfesional CategoriaProfesional { get; set; }
        public DatosAcademicos DatosAcademicos { get; set; }
        public Fotografia Fotografia { get; set; }
        public Sexenio Sexenios { get; set; }
    }
}
