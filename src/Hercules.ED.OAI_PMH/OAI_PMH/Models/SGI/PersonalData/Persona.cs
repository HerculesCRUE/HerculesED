using OAI_PMH.Models.SGI.ActividadDocente;
using OAI_PMH.Models.SGI.FormacionAcademica;
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
        public DatosAcademicos DatosAcademicos { get; set; }
        public Colectivo Colectivo { get; set; }
        public Fotografia Fotografia { get; set; }
        public Sexenio Sexenios { get; set; }        
        public List<Posgrado> Posgrado { get; set; }
        public List<Ciclos> Ciclos { get; set; }
        public List<Doctorados> Doctorados { get; set; }
        public List<FormacionEspecializada> FormacionEspecializada { get; set; }
        public List<Tesis> Tesis { get; set; }
        public List<SeminariosCursos> SeminariosCursos { get; set; }
        public List<FormacionAcademicaImpartida> FormacionAcademicaImpartida { get; set; }
    }
}
