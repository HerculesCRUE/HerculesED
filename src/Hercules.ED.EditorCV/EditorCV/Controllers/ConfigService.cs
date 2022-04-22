using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace EditorCV.Controllers
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // Credenciales
        private string usernameESB { get; set; }
        private string passwordESB { get; set; }

        // URLs
        private string urlThematicEnrichment { get; set; }
        private string urlSpecificEnrichment { get; set; }
        private string urlToken { get; set; }
        private string urlProduccionCientifica { get; set; }
        private string urlEnvioProyecto { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene el usuario necesario para la conexión con el ESB.
        /// </summary>
        /// <returns>Usuario.</returns>
        public string GetUsernameESB()
        {
            if (string.IsNullOrEmpty(usernameESB))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("username_esb"))
                {
                    connectionString = environmentVariables["username_esb"] as string;
                }
                else
                {
                    connectionString = configuracion["username_esb"];
                }

                usernameESB = connectionString;
            }

            return usernameESB;
        }

        /// <summary>
        /// Obtiene el password necesario para la conexión con el ESB.
        /// </summary>
        /// <returns>Password.</returns>
        public string GetPasswordESB()
        {
            if (string.IsNullOrEmpty(passwordESB))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("password_esb"))
                {
                    connectionString = environmentVariables["password_esb"] as string;
                }
                else
                {
                    connectionString = configuracion["password_esb"];
                }

                passwordESB = connectionString;
            }

            return passwordESB;
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

        /// <summary>
        /// Obtiene la URL del API de obtención de tokens.
        /// </summary>
        /// <returns>URI del API de tokens.</returns>
        public string GetUrlToken()
        {
            if (string.IsNullOrEmpty(urlToken))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_token"))
                {
                    connectionString = environmentVariables["url_token"] as string;
                }
                else
                {
                    connectionString = configuracion["url_token"];
                }

                urlToken = connectionString;
            }

            return urlToken;
        }

        /// <summary>
        /// Obtiene la URL del API del envío a producción científica.
        /// </summary>
        /// <returns>URI del API de envío a producción científica.</returns>
        public string GetUrlProduccionCientifica()
        {
            if (string.IsNullOrEmpty(urlProduccionCientifica))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_produccion_cientifica"))
                {
                    connectionString = environmentVariables["url_produccion_cientifica"] as string;
                }
                else
                {
                    connectionString = configuracion["url_produccion_cientifica"];
                }

                urlProduccionCientifica = connectionString;
            }

            return urlProduccionCientifica;
        }

        /// <summary>
        /// Obtiene la URL del API del envío a validar proyecto.
        /// </summary>
        /// <returns>URI del API de envío a producción científica.</returns>
        public string GetUrlEnvioProyecto()
        {
            if (string.IsNullOrEmpty(urlEnvioProyecto))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_validar_proyectos"))
                {
                    connectionString = environmentVariables["url_validar_proyectos"] as string;
                }
                else
                {
                    connectionString = configuracion["url_validar_proyectos"];
                }

                urlEnvioProyecto = connectionString;
            }

            return urlEnvioProyecto;
        }
    }
}
