using System.Collections.Generic;

namespace EditorCV.Models.ValidacionProyectos
{
    public class NotificacionProyecto
    {
        public string proyectoCVNId { get; set; }
        public string solicitanteRef { get; set; }
        public int autorizacionId { get; set; }
        public string titulo { get; set; }
        public string codExterno { get; set; }
        public string fechaInicio { get; set; }
        public string fechaFin { get; set; }
        public string ambitoGeografico { get; set; }
        public string gradoContribucion { get; set; }
        public string responsableRef { get; set; }
        public string datosResponsable { get; set; }
        public string entidadParticipacionRef { get; set; }
        public string datosEntidadParticipacion { get; set; }
        public string nombrePrograma { get; set; }
        public float importeTotal { get; set; }
        public float porcentajeSubvencion { get; set; }
        public string documentoRef { get; set; }
        public string urlAcreditativa { get; set; }
        public List<EntidadFinanciadora> entidadesFinanciadoras { get; set; }
    }
}
