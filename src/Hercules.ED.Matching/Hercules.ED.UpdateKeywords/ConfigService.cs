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

        // Endpoint
        private string Endpoint { get; set; }

        // Ruta Logs
        private string LogPath { get; set; }

        // ApiKey
        private string ApiKey { get; set; }

        // URLs
        private string UrlTgt { get; set; }
        private string UrlTicket { get; set; }
        private string UrlSnomed { get; set; }
        private string UrlRelaciones { get; set; }

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
            if (string.IsNullOrEmpty(LogPath))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("LogPath"))
                {
                    connectionString = environmentVariables["LogPath"] as string;
                }
                else
                {
                    connectionString = configuracion["LogPath"];
                }

                LogPath = connectionString;
            }

            return LogPath;
        }

        /// <summary>
        /// Obtiene el ApiKey que ha sido configurado.
        /// </summary>
        /// <returns>Apikey.</returns>
        public string GetApiKey()
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("ApiKey"))
                {
                    connectionString = environmentVariables["ApiKey"] as string;
                }
                else
                {
                    connectionString = configuracion["ApiKey"];
                }

                ApiKey = connectionString;
            }

            return ApiKey;
        }

        /// <summary>
        /// Obtiene la URL de la petición al servicio de TGT que ha sido configurada.
        /// </summary>
        /// <returns>URL.</returns>
        public string GetUrlTGT()
        {
            if (string.IsNullOrEmpty(UrlTgt))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlTGT"))
                {
                    connectionString = environmentVariables["UrlTGT"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlTGT"];
                }

                UrlTgt = connectionString;
            }

            return UrlTgt;
        }

        /// <summary>
        /// Obtiene la URL de la petición al servicio de Tickets que ha sido configurada.
        /// </summary>
        /// <returns>URL.</returns>
        public string GetUrlTicket()
        {
            if (string.IsNullOrEmpty(UrlTicket))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlTicket"))
                {
                    connectionString = environmentVariables["UrlTicket"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlTicket"];
                }

                UrlTicket = connectionString;
            }

            return UrlTicket;
        }

        /// <summary>
        /// Obtiene la URL de la petición al servicio de SNOMED que ha sido configurada.
        /// </summary>
        /// <returns>Url.</returns>
        public string GetUrlSNOMED()
        {
            if (string.IsNullOrEmpty(UrlSnomed))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlSNOMED"))
                {
                    connectionString = environmentVariables["UrlSNOMED"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlSNOMED"];
                }

                UrlSnomed = connectionString;
            }

            return UrlSnomed;
        }

        /// <summary>
        /// Obtiene la URL de la petición al servicio de Relaciones que ha sido configurada.
        /// </summary>
        /// <returns>URL.</returns>
        public string GetUrlRelaciones()
        {
            if (string.IsNullOrEmpty(UrlRelaciones))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlRelaciones"))
                {
                    connectionString = environmentVariables["UrlRelaciones"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlRelaciones"];
                }

                UrlRelaciones = connectionString;
            }

            return UrlRelaciones;
        }

        /// <summary>
        /// Obtiene la URL del SPARQL endpoint.
        /// </summary>
        /// <returns>URL.</returns>
        public string GetSparqlEndpoint()
        {
            if (string.IsNullOrEmpty(Endpoint))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("SparqlEndpoint"))
                {
                    connectionString = environmentVariables["SparqlEndpoint"] as string;
                }
                else
                {
                    connectionString = configuracion["SparqlEndpoint"];
                }

                Endpoint = connectionString;
            }

            return Endpoint;
        }
    }
}
