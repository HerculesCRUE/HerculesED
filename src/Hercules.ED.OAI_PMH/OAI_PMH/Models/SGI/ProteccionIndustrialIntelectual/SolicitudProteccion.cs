using System;

namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class SolicitudProteccion : SGI_Base
    {
        public int id { get; set; }
        public Invencion invencion { get; set; }
        public string titulo { get; set; }
        public DateTime? fechaPrioridadSolicitud { get; set; }
        public DateTime? fechaFinPriorPresFasNacRec { get; set; }
        public ViaProteccion viaProteccion { get; set; }
        public string paisProteccionRef { get; set; }
        public string numeroSolicitud { get; set; }
        public string numeroRegistro { get; set; }
        public string estado { get; set; }
        public DateTime? fechaPublicacion { get; set; }
        public string numeroPublicacion { get; set; }
        public DateTime? fechaConcesion { get; set; }
        public string numeroConcesion { get; set; }
        public DateTime? fechaCaducidad { get; set; }
        public string agentePropiedadRef { get; set; }
        public string tipoCaducidad { get; set; }
        public string comentarios { get; set; }
        public bool? activo { get; set; }
    }
}
