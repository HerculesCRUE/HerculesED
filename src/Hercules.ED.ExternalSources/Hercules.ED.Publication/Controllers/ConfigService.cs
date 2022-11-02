using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace PublicationAPI.Controllers
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // URLs
        private string urlWos { get; set; }
        private string urlScopus { get; set; }
        private string urlOpenAire { get; set; }
        private string urlCrossRef { get; set; }
        private string urlOpenCitations { get; set; }
        private string urlSemanticScholar { get; set; }
        private string urlZenodo { get; set; }
        private string urlEnriquecimiento { get; set; }
        private string rutaJsonSalida { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la URL del API de enriquecimiento que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de enriquecimiento.</returns>
        public string GetUrlEnriquecimiento()
        {
            if (string.IsNullOrEmpty(urlEnriquecimiento))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlEnriquecimiento"))
                {
                    connectionString = environmentVariables["UrlEnriquecimiento"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlEnriquecimiento"];
                }

                urlEnriquecimiento = connectionString;
            }

            return urlEnriquecimiento;
        }

        /// <summary>
        /// Obtiene la URL del API de WoS que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de WoS.</returns>
        public string GetUrlWos()
        {
            if (string.IsNullOrEmpty(urlWos))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlWos"))
                {
                    connectionString = environmentVariables["UrlWos"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlWos"];
                }

                urlWos = connectionString;
            }

            return urlWos;
        }

        /// <summary>
        /// Obtiene la URL del API de Scopus que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de Scopus.</returns>
        public string GetUrlScopus()
        {
            if (string.IsNullOrEmpty(urlScopus))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlScopus"))
                {
                    connectionString = environmentVariables["UrlScopus"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlScopus"];
                }

                urlScopus = connectionString;
            }

            return urlScopus;
        }

        /// <summary>
        /// Obtiene la URL del API de OpenAire que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de OpenAire.</returns>
        public string GetUrlOpenAire()
        {
            if (string.IsNullOrEmpty(urlOpenAire))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlOpenAire"))
                {
                    connectionString = environmentVariables["UrlOpenAire"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlOpenAire"];
                }

                urlOpenAire = connectionString;
            }

            return urlOpenAire;
        }

        /// <summary>
        /// Obtiene la URL del API de CrossRef que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de CrossRef.</returns>
        public string GetUrlCrossRef()
        {
            if (string.IsNullOrEmpty(urlCrossRef))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlCrossRef"))
                {
                    connectionString = environmentVariables["UrlCrossRef"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlCrossRef"];
                }

                urlCrossRef = connectionString;
            }

            return urlCrossRef;
        }

        /// <summary>
        /// Obtiene la URL del API de OpenCitations que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de OpenCitations.</returns>
        public string GetUrlOpenCitations()
        {
            if (string.IsNullOrEmpty(urlOpenCitations))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlOpenCitations"))
                {
                    connectionString = environmentVariables["UrlOpenCitations"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlOpenCitations"];
                }

                urlOpenCitations = connectionString;
            }

            return urlOpenCitations;
        }

        /// <summary>
        /// Obtiene la URL del API de SemanticScholar que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de SemanticScholar.</returns>
        public string GetUrlSemanticScholar()
        {
            if (string.IsNullOrEmpty(urlSemanticScholar))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlSemanticScholar"))
                {
                    connectionString = environmentVariables["UrlSemanticScholar"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlSemanticScholar"];
                }

                urlSemanticScholar = connectionString;
            }

            return urlSemanticScholar;
        }

        /// <summary>
        /// Obtiene la URL del API de Zenodo que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de Zenodo.</returns>
        public string GetUrlZenodo()
        {
            if (string.IsNullOrEmpty(urlZenodo))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlZenodo"))
                {
                    connectionString = environmentVariables["UrlZenodo"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlZenodo"];
                }

                urlZenodo = connectionString;
            }

            return urlZenodo;
        }

        /// <summary>
        /// Obtiene la ruta del json de salida que ha sido configurada.
        /// </summary>
        /// <returns>Ruta del json de salida.</returns>
        public string GetRutaJsonSalida()
        {
            if (string.IsNullOrEmpty(rutaJsonSalida))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("RutaJsonSalida"))
                {
                    connectionString = environmentVariables["RutaJsonSalida"] as string;
                }
                else
                {
                    connectionString = configuracion["RutaJsonSalida"];
                }

                rutaJsonSalida = connectionString;
            }

            return rutaJsonSalida;
        }
    }
}
