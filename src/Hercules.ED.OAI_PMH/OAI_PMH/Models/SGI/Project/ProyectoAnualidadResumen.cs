using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Project
{
    public class ProyectoAnualidadResumen : SGI_Base
    {
        public string Id { get; set; }
        public string Anio { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public double TotalGastosPresupuesto { get; set; }
        public double TotalGastosConcedido { get; set; }
        public double TotalIngresos { get; set; }
        public bool Presupuestar { get; set; }
        public bool EnviadoSGE { get; set; }
    }
}
