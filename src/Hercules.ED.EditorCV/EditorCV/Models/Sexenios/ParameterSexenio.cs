using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCV.Models.Sexenios
{
    public class ParameterSexenio
    {
        /// <summary>
        /// Es para indicar una planificación, por si se quiere hacer periódicamente, null si queremos que sea instantáneo
        /// </summary>
        public int? time_schedule { get; set; }
        /// <summary>
        /// ID del proceso a ejecutar, (sexenios) será 18
        /// </summary>
        public int? id_process { get; set; }
        /// <summary>
        /// Objeto que tiene los parámetros generales y específicos del proceso
        /// </summary>
        public Process process { get; set; }

        public ParameterSexenio()
        {
            time_schedule = null;
            id_process = 18;
            process = new Process();
        }

        public ParameterSexenio(string comite, string periodo, string perfil_tecnologico, string subcomite, string idInvestigador)
        {
            time_schedule = null;
            id_process = 18;
            process = new Process(comite, periodo, perfil_tecnologico, subcomite, idInvestigador);
        }

        public ParameterSexenio(string comite, List<string> periodo, string perfil_tecnologico, string subcomite, string idInvestigador)
        {
            time_schedule = null;
            id_process = 18;
            process = new Process(comite, periodo, perfil_tecnologico, subcomite,idInvestigador);
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
        public string id_robot { get; set; }
        /// <summary>
        /// Objeto que tendrá los parámetros específicos del proceso
        /// </summary>
        public Parameters parameters { get; set; }

        public Process()
        {
            id_robot = null;
            parameters = new Parameters();
            priority = 1;
        }
        public Process(string comite, string periodo, string perfil_tecnologico, string subcomite, string idInvestigador)
        {
            id_robot = null;
            parameters = new Parameters(comite, periodo, perfil_tecnologico, subcomite, idInvestigador);
            priority = 1;
        }
        public Process(string comite, List<string> periodo, string perfil_tecnologico, string subcomite, string idInvestigador)
        {
            id_robot = null;
            string periodoConcat = string.Join(",", periodo);
            parameters = new Parameters(comite, periodoConcat, perfil_tecnologico, subcomite, idInvestigador);
            priority = 1;
        }

    }

    public class Parameters
    {
        /// <summary>
        /// ID del comité evaluador del sexenio.
        /// </summary>
        public string comite { get; set; }
        /// <summary>
        /// Define el período de solicitud del sexenio.
        /// Rango de años es secuencial "2011-2016"
        /// Rango de años contiene años que no son secuenciales "2011,2012,2014-2016"
        /// Lista de años separandolos por comas "2011,2012,2013,2014,2015,2016"
        /// </summary>
        public string periodo { get; set; }
        /// <summary>
        /// Necesario solo para el comité con identificador "8". True si el investigador tiene un perfil tecnológico o False si no lo tiene.
        /// </summary>
        public string perfil_tecnologico { get; set; }
        /// <summary>
        /// ID del subcomité, solo será necesario cuando el id del comité sea "9".
        /// </summary>
        public string subcomite { get; set; }
        /// <summary>
        /// identificador escogido. Puede ser:
        /// - PersonaRef
        /// - Email
        /// - ORCID
        /// </summary>
        public string investigador { get; set; }

        public Parameters()
        {
            comite = "";
            periodo = "";
            perfil_tecnologico = "";
            subcomite = "";
        }

        public Parameters(string comite, string periodo, string perfil_tecnologico, string subcomite, string idInvestigador)
        {
            this.comite = comite;
            this.periodo = periodo;
            this.perfil_tecnologico = perfil_tecnologico;
            this.subcomite = subcomite;
            this.investigador = idInvestigador;
        }
    }
}
