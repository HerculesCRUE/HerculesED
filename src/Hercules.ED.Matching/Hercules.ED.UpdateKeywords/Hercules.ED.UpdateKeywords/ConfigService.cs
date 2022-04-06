using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.UpdateKeywords
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // Ruta Logs
        private string logPath { get; set; }

        // ApiKey
        private string apiKey { get; set; }

        // URLs
        private string urlTgt { get; set; }
        private string urlTicket { get; set; }
        private string urlSnomed { get; set; }
        private string urlRelaciones { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la ruta de los logs que ha sido configurado.
        /// </summary>
        /// <returns>Ruta.</returns>
        public string GetLogPath()
        {
            if (string.IsNullOrEmpty(logPath))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("LogPath"))
                {
                    connectionString = environmentVariables["LogPath"] as string;
                }
                else
                {
                    connectionString = configuracion["LogPath"];
                }

                logPath = connectionString;
            }

            return logPath;
        }

        /// <summary>
        /// Obtiene el ApiKey que ha sido configurado.
        /// </summary>
        /// <returns>Apikey.</returns>
        public string GetApiKey()
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("ApiKey"))
                {
                    connectionString = environmentVariables["ApiKey"] as string;
                }
                else
                {
                    connectionString = configuracion["ApiKey"];
                }

                apiKey = connectionString;
            }

            return apiKey;
        }

        /// <summary>
        /// Obtiene la URL de la petición al servicio de TGT que ha sido configurada.
        /// </summary>
        /// <returns>URL.</returns>
        public string GetUrlTGT()
        {
            if (string.IsNullOrEmpty(urlTgt))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlTGT"))
                {
                    connectionString = environmentVariables["UrlTGT"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlTGT"];
                }

                urlTgt = connectionString;
            }

            return urlTgt;
        }

        /// <summary>
        /// Obtiene la URL de la petición al servicio de Tickets que ha sido configurada.
        /// </summary>
        /// <returns>URL.</returns>
        public string GetUrlTicket()
        {
            if (string.IsNullOrEmpty(urlTicket))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlTicket"))
                {
                    connectionString = environmentVariables["UrlTicket"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlTicket"];
                }

                urlTicket = connectionString;
            }

            return urlTicket;
        }

        /// <summary>
        /// Obtiene la URL de la petición al servicio de SNOMED que ha sido configurada.
        /// </summary>
        /// <returns>Url.</returns>
        public string GetUrlSNOMED()
        {
            if (string.IsNullOrEmpty(urlSnomed))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlSNOMED"))
                {
                    connectionString = environmentVariables["UrlSNOMED"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlSNOMED"];
                }

                urlSnomed = connectionString;
            }

            return urlSnomed;
        }

        /// <summary>
        /// Obtiene la URL de la petición al servicio de Relaciones que ha sido configurada.
        /// </summary>
        /// <returns>URL.</returns>
        public string GetUrlRelaciones()
        {
            if (string.IsNullOrEmpty(urlRelaciones))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlRelaciones"))
                {
                    connectionString = environmentVariables["UrlRelaciones"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlRelaciones"];
                }

                urlRelaciones = connectionString;
            }

            return urlRelaciones;
        }
    }
}
