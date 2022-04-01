using Hercules.ED.UMLS.Controllers;
using System;
using System.IO;

namespace Hercules.ED.UMLS.Middlewares
{
    public class FileLogger
    {
        // Configuración.
        readonly ConfigService _Configuracion;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        public FileLogger(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Sobreescribe el método Log para pintar el mensaje de error en un fichero.
        /// </summary>
        /// <param name="pMensaje">Mensaje personalizado a mostrar.</param>
        /// <param name="pExcepcion">Mensaje de la excepción.</param>
        public void Log(string pMensaje, string pExcepcion)
        {
            string nombre = "UMLS";
            string fecha = DateTime.Now.ToString().Split(" ")[0].Replace("/", "-");
            string ruta = $@"{_Configuracion.GetLogPath()}{nombre}_{fecha}.log";

            if (!File.Exists(ruta))
            {
                using (FileStream fs = File.Create(ruta)) { }               
            }

            File.AppendAllText(ruta, $@"{DateTime.Now} ---------- {pMensaje}{Environment.NewLine}{pExcepcion}{Environment.NewLine}={Environment.NewLine}");
        }
    }
}
