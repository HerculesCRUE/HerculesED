using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace GitHubAPI.Controllers
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        private string urlBaseEnriquecimiento { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la URL base del API de GitHub que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de GitHub.</returns>
        public string GetUrlBase()
        {
            return "https://api.github.com";
        }

        /// <summary>
        /// Obtiene la URL base del API de Enriquecimiento que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de Enriquecimiento.</returns>
        public string GetUrlBaseEnriquecimiento()
        {
            if (string.IsNullOrEmpty(urlBaseEnriquecimiento))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlBaseEnriquecimiento"))
                {
                    connectionString = environmentVariables["UrlBaseEnriquecimiento"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlBaseEnriquecimiento"];
                }

                urlBaseEnriquecimiento = connectionString;
            }

            return urlBaseEnriquecimiento;
        }
    }
}
