using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Config
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // Rutas
        private string rutaDirectorioLectura { get; set; }
        private string rutaDirectorioEscritura { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/appsettings/appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la ruta de lectura de los ficheros.
        /// </summary>
        /// <returns>Ruta de lectura.</returns>
        public string GetRutaDirectorioLectura()
        {
            if (string.IsNullOrEmpty(rutaDirectorioLectura))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("DirectorioLectura"))
                {
                    connectionString = environmentVariables["DirectorioLectura"] as string;
                }
                else
                {
                    connectionString = configuracion["DirectorioLectura"];
                }

                rutaDirectorioLectura = connectionString;
            }

            return rutaDirectorioLectura;
        }

        /// <summary>
        /// Obtiene la ruta de escritura de los ficheros.
        /// </summary>
        /// <returns>Ruta de escritura.</returns>
        public string GetRutaDirectorioEscritura()
        {
            if (string.IsNullOrEmpty(rutaDirectorioEscritura))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("DirectorioEscritura"))
                {
                    connectionString = environmentVariables["DirectorioEscritura"] as string;
                }
                else
                {
                    connectionString = configuracion["DirectorioEscritura"];
                }

                rutaDirectorioEscritura = connectionString;
            }

            return rutaDirectorioEscritura;
        }
    }
}