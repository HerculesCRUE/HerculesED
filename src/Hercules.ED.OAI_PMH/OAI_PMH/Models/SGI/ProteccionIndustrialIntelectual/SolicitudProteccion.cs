using System;

namespace OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual
{
    public class SolicitudProteccion : SGI_Base
    {
        public int id { get; set; }
        public int invencionId { get; set; }
        public string titulo { get; set; }
        public DateTime fechaPrioridadSolicitud { get; set; }
        public DateTime fechaFinPriorPresFasNacRec { get; set; }
        public ViaProteccion viaProteccion { get; set; }
        public string PaisProteccionRef { get; set; }
        public string numeroSolicitud { get; set; }
        public string numeroRegistro { get; set; }
        public int estado { get; set; }
        public DateTime fechaPublicacion { get; set; }
        public string numeroPublicacion { get; set; }
        public DateTime fechaConcesion { get; set; }
        public string numeroConcesion { get; set; }
        public DateTime fechaCaducidad { get; set; }
        //public AgentePropiedadRef agentePropiedad { get; set; } //TODO: ???
        public TipoCaducidad tipoCaducidad { get; set; }
        public string comentarios { get; set; }
    }
}
