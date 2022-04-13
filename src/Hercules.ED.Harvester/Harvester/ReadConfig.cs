using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvester
{
    public class ReadConfig
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // Rutas
        private static string dirLogCargas { get; set; }
        private static string lastUpdateDateFile { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReadConfig()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}/Config/appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la ruta dónde se van a almacenar los IDs cargados.
        /// </summary>
        /// <returns>Ruta.</returns>
        public string GetLogCargas()
        {
            if (string.IsNullOrEmpty(dirLogCargas))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("DirLogCarga"))
                {
                    connectionString = environmentVariables["DirLogCarga"] as string;
                }
                else
                {
                    connectionString = configuracion["DirLogCarga"];
                }

                dirLogCargas = connectionString;
            }

            return dirLogCargas;
        }

        /// <summary>
        /// Obtiene la ruta del fichero de la última fecha de modificación.
        /// </summary>
        /// <returns>Ruta.</returns>
        public string GetLastUpdateDate()
        {
            if (string.IsNullOrEmpty(lastUpdateDateFile))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("LastUpdateDateFile"))
                {
                    connectionString = environmentVariables["LastUpdateDateFile"] as string;
                }
                else
                {
                    connectionString = configuracion["LastUpdateDateFile"];
                }

                lastUpdateDateFile = connectionString;
            }

            return lastUpdateDateFile;
        }
    }
}
