using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EditorCV.Models.Acreditacion
{
    public class ParameterAcreditacion
    {
        /// <summary>
        /// Es para indicar una planificación, por si se quiere hacer periódicamente, null si queremos que sea instantáneo
        /// </summary>
        public int? time_schedule { get; set; }
        /// <summary>
        /// ID del proceso a ejecutar, (acreditaciones) será 22
        /// </summary>
        public int? id_process { get; set; }
        /// <summary>
        /// Objeto que tiene los parámetros generales y específicos del proceso
        /// </summary>
        public Process? process { get; set; }

        public ParameterAcreditacion()
        {
            this.time_schedule = null;
            this.id_process = 22;
            this.process = new Process();
        }

        public ParameterAcreditacion(string comision, string tipo_acreditacion, [Optional] string categoria_acreditacion, string investigador)
        {
            this.time_schedule = null;
            this.id_process = 22;
            this.process = new Process(comision, tipo_acreditacion, categoria_acreditacion, investigador);
        }

    }
    public class Process
    {
        /// <summary>
        /// Prioridad del proceso, por defecto 1
        /// </summary>
        public int? priority { get; set; }
        /// <summary>
        /// Robot que queremos que ejecute el proceso, en este caso como nos dará igual que robot lo ejecute, lo dejaremos a null
        /// </summary>
        public string? id_robot { get; set; }
        /// <summary>
        /// Objeto que tendrá los parámetros específicos del proceso
        /// </summary>
        public Parameters? parameters { get; set; }

        public Process()
        {
            this.id_robot = null;
            this.parameters = new Parameters();
            this.priority = 1;
        }
        public Process(string comision, string tipo_acreditacion, [Optional] string categoria_acreditacion, string investigador)
        {
            this.id_robot = null;
            this.parameters = new Parameters(comision, tipo_acreditacion, categoria_acreditacion, investigador);
            this.priority = 1;
        }
    }
    public class Parameters
    {
        /// <summary>
        /// ID de la comisión evaluadora de la acreditación
        /// </summary>
        public string? comision { get; set; }
        /// <summary>
        /// ID de la acreditación
        /// </summary>
        public string? tipo_acreditacion { get; set; }
        /// <summary>
        /// ID de la categoría de acreditación, solo será necesario cuando el id de la comisión tenga el valor "21".
        /// </summary>
        public string? categoria_acreditacion { get; set; }
        /// <summary>
        ///  identificador escogido. Puede ser:
        ///  - PersonaRef
        ///  - Email
        ///  - ORCID
        /// </summary>
        public string? investigador { get; set; }

        public Parameters()
        {
            this.comision = "";
            this.tipo_acreditacion = "";
            this.categoria_acreditacion = null;
            this.investigador = "";
        }

        public Parameters(string comision, string tipo_acreditacion, [Optional] string categoria_acreditacion, string investigador)
        {
            this.comision = comision;
            this.tipo_acreditacion = tipo_acreditacion;
            this.categoria_acreditacion = string.IsNullOrEmpty(categoria_acreditacion) ? null : categoria_acreditacion;
            this.investigador = investigador;
        }
    }
}
