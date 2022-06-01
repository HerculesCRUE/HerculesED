using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Project
{
    public class Proyecto : SGI_Base
    {
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string LastModifiedDate { get; set; }
        public string Id { get; set; }
        public string ConvocatoriaId { get; set; }
        public string SolicitudId { get; set; }
        public EstadoProyecto Estado { get; set; }
        public string Titulo { get; set; }
        public string Acronimo { get; set; }
        public string CodigoExterno { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string FechaFinDefinitiva { get; set; }
        public string UnidadGestionRef { get; set; }
        public ModeloEjecucion ModeloEjecucion { get; set; }
        public TipoFinalidad Finalidad { get; set; }
        public string ConvocatoriaExterna { get; set; }
        public AmbitoGeografico AmbitoGeografico { get; set; }
        public bool? Confidencial { get; set; }
        public string ClasificacionCVN { get; set; }
        public bool? Coordinado { get; set; }
        public bool? Colaborativo { get; set; }
        public bool? CoordinadorExterno { get; set; }
        public bool? Timesheet { get; set; }
        public bool? PermitePaquetesTrabajo { get; set; }
        public bool? CosteHora { get; set; }
        public string TipoHorasAnuales { get; set; }
        public ProyectoIVA Iva { get; set; }
        public string CausaExencion { get; set; }
        public string Observaciones { get; set; }
        public bool? Anualidades { get; set; }
        public double? ImportePresupuesto { get; set; }
        public double? ImporteConcedido { get; set; }
        public double? ImportePresupuestoSocios { get; set; }
        public double? ImporteConcedidoSocios { get; set; }
        public double? TotalImportePresupuesto { get; set; }
        public double? TotalImporteConcedido { get; set; }
        public bool? Activo { get; set; }
        public ContextoProyecto Contexto { get; set; }
        public List<ProyectoEquipo> Equipo { get; set; }
        public List<ProyectoEntidadGestora> EntidadesGestoras { get; set; }
        public List<ProyectoEntidadConvocante> EntidadesConvocantes { get; set; }
        public List<ProyectoEntidadFinanciadora> EntidadesFinanciadoras { get; set; }
        public List<ProyectoAnualidadResumen> ResumenAnualidades { get; set; }
        public ProyectoPresupuestoTotales PresupuestosTotales { get; set; }
        public List<ProyectoClasificacion> ProyectoClasificacion { get; set; }
        public List<NotificacionProyectoExternoCVN> NotificacionProyectoExternoCVN { get; set; }
        public List<ProyectoAreaConocimiento> AreasConocimiento { get; set; }
        public List<PalabraClave> PalabrasClaves { get; set; }
    }
}
