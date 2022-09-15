using System;
using System.Collections.Generic;

namespace OAI_PMH.Models.SGI.GruposInvestigacion
{
    public class Grupo : SGI_Base
    {
        public int? id { get; set; }
        public string nombre { get; set; }
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFin { get;set; }
        public string proyectoSgeRef { get; set; }
        public int? solicitudId { get; set; }
        public string codigo { get; set; }
        public string tipo { get; set; }
        public bool? especialInvestigacion { get; set; }
        public bool? activo { get; set; }
        public List<GrupoEquipo> equipo { get; set; }
        public List<GrupoPalabraClave> palabrasClave { get; set; }
        public List<LineaClasificacion> lineasClasificacion { get; set; }
        public List<LineaInvestigacion> lineasInvestigacion { get; set; }
        public List<string> investigadoresPrincipales { get; set; }
        public List<string> investigadoresPrincipalesMaxParticipacion { get; set; }
    }
}
