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
        private static string dirLogIdentifier { get; set; }
        private static string dirLogCargas { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReadConfig()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}/Config/appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la ruta dónde se van a almacenar los IDs obtenidos por el OAI-PMH.
        /// </summary>
        /// <returns>Ruta.</returns>
        public string GetLogIdentifier()
        {
            if (string.IsNullOrEmpty(dirLogIdentifier))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("DirLogIdentifier"))
                {
                    connectionString = environmentVariables["DirLogIdentifier"] as string;
                }
                else
                {
                    connectionString = configuracion["DirLogIdentifier"];
                }

                dirLogIdentifier = connectionString;
            }

            return dirLogIdentifier;
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
    }
}
