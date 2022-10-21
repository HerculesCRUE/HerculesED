using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.LoadCV.Config
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        private string RutaCarpeta { get; set; }
        private string UrlImportadorExportador { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la url del servicio importador/exportador
        /// </summary>
        /// <returns>Devuelve la url del importador/exportador</returns>
        public string GetUrlImportadorExportador()
        {
            if (string.IsNullOrEmpty(UrlImportadorExportador))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_importador_exportador"))
                {
                    connectionString = environmentVariables["url_importador_exportador"] as string;
                }
                else
                {
                    connectionString = configuracion["url_importador_exportador"];
                }

                UrlImportadorExportador = connectionString;
            }

            return UrlImportadorExportador;
        }

        /// <summary>
        /// Obtiene la ruta del la carpeta de lectura de archivos
        /// </summary>
        /// <returns>Devuelve la ruta de la carpeta de archivos</returns>
        public string GetRutaCarpeta()
        {
            if (string.IsNullOrEmpty(RutaCarpeta))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("RutaCarpeta"))
                {
                    connectionString = environmentVariables["RutaCarpeta"] as string;
                }
                else
                {
                    connectionString = configuracion["RutaCarpeta"];
                }

                RutaCarpeta = connectionString;
            }

            return RutaCarpeta;
        }

    }
}
