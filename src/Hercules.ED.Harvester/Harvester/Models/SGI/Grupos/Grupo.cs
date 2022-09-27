using System;
using System.Collections.Generic;

namespace OAI_PMH.Models.SGI.Grupos
{
    public class Grupo : SGI_Base
    {
        public long? id { get; set; }
        public string nombre { get; set; }
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFin { get;set; }
        public string proyectoSgeRef { get; set; }
        public long? solicitudId { get; set; }
        public string codigo { get; set; }
        public string tipo { get; set; }
        public bool? especialInvestigacion { get; set; }
        public bool? activo { get; set; }
        public List<GrupoEquipo> equipo { get; set; }
        public List<string> areasTematicas { get; set; }
    }
}
