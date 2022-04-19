using System.Collections.Generic;

namespace OAI_PMH.Models.SGI.Project
{
    public class NotificacionProyectoExternoCVN : SGI_Base
    {
        public int Id { get; set; }
        public int ProyectoId { get; set; }
        public string ProyectoCVNId { get; set; }
        public string SolicitanteRef { get; set; }
        public int AutorizacionId { get; set; }
        public string Titulo { get; set; }  
        public string CodExterno { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string AmbitoGeografico { get; set; }
        public string ResponsableRef { get; set; }
        public string EntidadParticipacionRef { get; set; }
        public string NombrePrograma { get; set; }
        public float ImporteTotal { get; set; }
        public float PorcentajeSubvencion { get; set; }
        public string DocumentoRef { get; set; }
        public string UrlAcreditativa { get; set; }
        public List<EntidadFinanciadora> EntidadesFinanciadoras { get; set; }
    }
}
