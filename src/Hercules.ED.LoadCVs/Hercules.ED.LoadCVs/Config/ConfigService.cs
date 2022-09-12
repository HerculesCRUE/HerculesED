using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.LoadCVs.Config
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        private string rutaDatos { get; set; }

        private string rutaImportador { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la ruta de lectura de los ficheros.
        /// </summary>
        /// <returns>Ruta de lectura.</returns>
        public string GetRutaDatos()
        {
            if (string.IsNullOrEmpty(rutaDatos))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("DirectorioOrcid"))
                {
                    connectionString = environmentVariables["DirectorioOrcid"] as string;
                }
                else
                {
                    connectionString = configuracion["DirectorioOrcid"];
                }

                rutaDatos = connectionString;
            }

            return rutaDatos;
        }

        /// <summary>
        /// Obtiene la ruta del servicio de Imporador.
        /// </summary>
        /// <returns>Ruta de lectura.</returns>
        public string GetRutaImportador()
        {
            if (string.IsNullOrEmpty(rutaImportador))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("ServicioImportador"))
                {
                    connectionString = environmentVariables["ServicioImportador"] as string;
                }
                else
                {
                    connectionString = configuracion["ServicioImportador"];
                }

                rutaImportador = connectionString;
            }

            return rutaImportador;
        }
    }
}
