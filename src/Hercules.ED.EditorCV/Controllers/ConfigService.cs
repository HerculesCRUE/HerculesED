using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace EditorCV.Controllers
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // URLs
        private string urlThematicEnrichment { get; set; }
        private string urlSpecificEnrichment { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la URL del API del enriquecimiento de tópicos temáticos que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de enriquecimiento.</returns>
        public string GetUrlThematicEnrichment()
        {
            if (string.IsNullOrEmpty(urlThematicEnrichment))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_enriquecimiento_thematic"))
                {
                    connectionString = environmentVariables["url_enriquecimiento_thematic"] as string;
                }
                else
                {
                    connectionString = configuracion["url_enriquecimiento_thematic"];
                }

                urlThematicEnrichment = connectionString;
            }

            return urlThematicEnrichment;
        }

        /// <summary>
        /// Obtiene la URL del API del enriquecimiento de tópicos específicos que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de enriquecimiento.</returns>
        public string GetUrlSpecificEnrichment()
        {
            if (string.IsNullOrEmpty(urlSpecificEnrichment))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_enriquecimiento_specific"))
                {
                    connectionString = environmentVariables["url_enriquecimiento_specific"] as string;
                }
                else
                {
                    connectionString = configuracion["url_enriquecimiento_specific"];
                }

                urlSpecificEnrichment = connectionString;
            }

            return urlSpecificEnrichment;
        }
    }
}
