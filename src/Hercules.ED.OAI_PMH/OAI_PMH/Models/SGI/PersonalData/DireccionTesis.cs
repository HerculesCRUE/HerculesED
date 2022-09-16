using System;

namespace OAI_PMH.Models.SGI.PersonalData
{
    /// <summary>
    /// 
    /// </summary>
    public class DireccionTesis : SGI_Base
    {
        public string Id { get; set; }
        public string PersonaRef { get; set; }
        public string TituloTrabajo { get; set; }
        public DateTime? FechaDefensa { get; set; }
        public string Alumno { get; set; }
        public TipoTrabajoDirigido TipoProyecto { get; set; }
        public string CalificacionObtenida { get; set; }
        public string CoDirectorTesisRef { get; set; }
        public bool? DoctoradoEuropeo { get; set; }
        public DateTime? FechaMencionDoctoradoEuropeo { get; set; }
        public bool? MencionCalidad { get; set; }
        public DateTime? FechaMencionCalidad { get; set; }
        public bool? MencionInternacional { get; set; }
        public bool? MencionIndustrial { get; set; }
    }
}
