using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace PublicationAPI.Controllers
{
    public class ConfigService
    {
        // Archivo de configuración.
        private static readonly IConfigurationRoot configuracion = new ConfigurationBuilder().AddJsonFile($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();

        // URLs
        private string UrlWos { get; set; }
        private string UrlScopus { get; set; }
        private string UrlOpenAire { get; set; }
        private string UrlCrossRef { get; set; }
        private string UrlOpenCitations { get; set; }
        private string UrlSemanticScholar { get; set; }
        private string UrlZenodo { get; set; }
        private string UrlEnriquecimiento { get; set; }
        private string RutaJsonSalida { get; set; }

        private string RutaDirectorioLectura { get; set; }
        private string RutaDirectorioEscritura { get; set; }

        /// <summary>
        /// Obtiene la URL del API de enriquecimiento que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de enriquecimiento.</returns>
        public string GetUrlEnriquecimiento()
        {
            if (string.IsNullOrEmpty(UrlEnriquecimiento))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlEnriquecimiento"))
                {
                    connectionString = environmentVariables["UrlEnriquecimiento"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlEnriquecimiento"];
                }

                UrlEnriquecimiento = connectionString;
            }

            return UrlEnriquecimiento;
        }

        /// <summary>
        /// Obtiene la URL del API de WoS que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de WoS.</returns>
        public string GetUrlWos()
        {
            if (string.IsNullOrEmpty(UrlWos))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlWos"))
                {
                    connectionString = environmentVariables["UrlWos"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlWos"];
                }

                UrlWos = connectionString;
            }

            return UrlWos;
        }

        /// <summary>
        /// Obtiene la URL del API de Scopus que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de Scopus.</returns>
        public string GetUrlScopus()
        {
            if (string.IsNullOrEmpty(UrlScopus))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlScopus"))
                {
                    connectionString = environmentVariables["UrlScopus"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlScopus"];
                }

                UrlScopus = connectionString;
            }

            return UrlScopus;
        }

        /// <summary>
        /// Obtiene la URL del API de OpenAire que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de OpenAire.</returns>
        public string GetUrlOpenAire()
        {
            if (string.IsNullOrEmpty(UrlOpenAire))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlOpenAire"))
                {
                    connectionString = environmentVariables["UrlOpenAire"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlOpenAire"];
                }

                UrlOpenAire = connectionString;
            }

            return UrlOpenAire;
        }

        /// <summary>
        /// Obtiene la URL del API de CrossRef que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de CrossRef.</returns>
        public string GetUrlCrossRef()
        {
            if (string.IsNullOrEmpty(UrlCrossRef))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlCrossRef"))
                {
                    connectionString = environmentVariables["UrlCrossRef"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlCrossRef"];
                }

                UrlCrossRef = connectionString;
            }

            return UrlCrossRef;
        }

        /// <summary>
        /// Obtiene la URL del API de OpenCitations que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de OpenCitations.</returns>
        public string GetUrlOpenCitations()
        {
            if (string.IsNullOrEmpty(UrlOpenCitations))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlOpenCitations"))
                {
                    connectionString = environmentVariables["UrlOpenCitations"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlOpenCitations"];
                }

                UrlOpenCitations = connectionString;
            }

            return UrlOpenCitations;
        }

        /// <summary>
        /// Obtiene la URL del API de SemanticScholar que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de SemanticScholar.</returns>
        public string GetUrlSemanticScholar()
        {
            if (string.IsNullOrEmpty(UrlSemanticScholar))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlSemanticScholar"))
                {
                    connectionString = environmentVariables["UrlSemanticScholar"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlSemanticScholar"];
                }

                UrlSemanticScholar = connectionString;
            }

            return UrlSemanticScholar;
        }

        /// <summary>
        /// Obtiene la URL del API de Zenodo que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de Zenodo.</returns>
        public string GetUrlZenodo()
        {
            if (string.IsNullOrEmpty(UrlZenodo))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlZenodo"))
                {
                    connectionString = environmentVariables["UrlZenodo"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlZenodo"];
                }

                UrlZenodo = connectionString;
            }

            return UrlZenodo;
        }

        /// <summary>
        /// Obtiene la ruta del json de salida que ha sido configurada.
        /// </summary>
        /// <returns>Ruta del json de salida.</returns>
        public string GetRutaJsonSalida()
        {
            if (string.IsNullOrEmpty(RutaJsonSalida))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("RutaJsonSalida"))
                {
                    connectionString = environmentVariables["RutaJsonSalida"] as string;
                }
                else
                {
                    connectionString = configuracion["RutaJsonSalida"];
                }

                RutaJsonSalida = connectionString;
            }

            return RutaJsonSalida;
        }

        /// <summary>
        /// Obtiene la ruta de lectura de los ficheros.
        /// </summary>
        /// <returns>Ruta de lectura.</returns>
        public string GetRutaDirectorioLectura()
        {
            if (string.IsNullOrEmpty(RutaDirectorioLectura))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();

                if (environmentVariables.Contains("DirectorioLectura"))
                {
                    connectionString = environmentVariables["DirectorioLectura"] as string;
                }
                else
                {
                    connectionString = configuracion["DirectorioLectura"];
                }

                RutaDirectorioLectura = connectionString;
            }

            return RutaDirectorioLectura;
        }

        /// <summary>
        /// Obtiene la ruta de escritura de los ficheros.
        /// </summary>
        /// <returns>Ruta de escritura.</returns>
        public string GetRutaDirectorioEscritura()
        {
            if (string.IsNullOrEmpty(RutaDirectorioEscritura))
            {
                string connectionString;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();

                if (environmentVariables.Contains("DirectorioEscritura"))
                {
                    connectionString = environmentVariables["DirectorioEscritura"] as string;
                }
                else
                {
                    connectionString = configuracion["DirectorioEscritura"];
                }

                RutaDirectorioEscritura = connectionString;
            }

            return RutaDirectorioEscritura;
        }
    }
}
