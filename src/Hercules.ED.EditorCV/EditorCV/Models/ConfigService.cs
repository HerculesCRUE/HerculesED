using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace EditorCV.Models
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        //Configuración Rabbit para el desnormalizador
        private string RabbitConnectionString { get; set; }
        private string DenormalizerQueueRabbit { get; set; }

        // Credenciales
        private string usernameESBcsp { get; set; }
        private string passwordESBcsp { get; set; }
        private string usernameESBprc { get; set; }
        private string passwordESBprc { get; set; }

        // URLs
        private string urlThematicEnrichment { get; set; }
        private string urlSpecificEnrichment { get; set; }
        private string urlToken { get; set; }
        private string urlProduccionCientifica { get; set; }
        private string urlEnvioProyecto { get; set; }
        private string urlExportador { get; set; }
        private string urlExportadorLimitado { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la cadena de conexión de Rabbit configurada.
        /// </summary>
        /// <returns>Cadena de conexión de Rabbit.</returns>
        public string GetrabbitConnectionString()
        {
            if (string.IsNullOrEmpty(RabbitConnectionString))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string rabbitConnectionString = string.Empty;
                if (environmentVariables.Contains("RabbitMQ"))
                {
                    rabbitConnectionString = environmentVariables["RabbitMQ"] as string;
                }
                else
                {
                    rabbitConnectionString = configuracion.GetConnectionString("RabbitMQ");
                }
                RabbitConnectionString = rabbitConnectionString;
            }
            return RabbitConnectionString;
        }

        /// <summary>
        /// Obtiene la el nombre de la cola Rabbit de desnormalización de configuración.
        /// </summary>
        /// <returns>Nombre de la cola Rabbit.</returns>
        public string GetDenormalizerQueueRabbit()
        {
            if (string.IsNullOrEmpty(DenormalizerQueueRabbit))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue = string.Empty;
                if (environmentVariables.Contains("DenormalizerQueueRabbit"))
                {
                    queue = environmentVariables["DenormalizerQueueRabbit"] as string;
                }
                else
                {
                    queue = configuracion["DenormalizerQueueRabbit"];
                }
                DenormalizerQueueRabbit = queue;
            }
            return DenormalizerQueueRabbit;
        }

        /// <summary>
        /// Obtiene el usuario necesario para la conexión con el ESB.
        /// </summary>
        /// <returns>Usuario.</returns>
        public string GetUsernameEsbCsp()
        {
            if (string.IsNullOrEmpty(usernameESBcsp))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("username_esb_csp"))
                {
                    connectionString = environmentVariables["username_esb_csp"] as string;
                }
                else
                {
                    connectionString = configuracion["username_esb_csp"];
                }

                usernameESBcsp = connectionString;
            }

            return usernameESBcsp;
        }

        /// <summary>
        /// Obtiene el password necesario para la conexión con el ESB.
        /// </summary>
        /// <returns>Password.</returns>
        public string GetPasswordEsbCsp()
        {
            if (string.IsNullOrEmpty(passwordESBcsp))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("password_esb_csp"))
                {
                    connectionString = environmentVariables["password_esb_csp"] as string;
                }
                else
                {
                    connectionString = configuracion["password_esb_csp"];
                }

                passwordESBcsp = connectionString;
            }

            return passwordESBcsp;
        }

        /// <summary>
        /// Obtiene el usuario necesario para la conexión con el ESB.
        /// </summary>
        /// <returns>Usuario.</returns>
        public string GetUsernameEsbPrc()
        {
            if (string.IsNullOrEmpty(usernameESBprc))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("username_esb_prc"))
                {
                    connectionString = environmentVariables["username_esb_prc"] as string;
                }
                else
                {
                    connectionString = configuracion["username_esb_prc"];
                }

                usernameESBprc = connectionString;
            }

            return usernameESBprc;
        }

        /// <summary>
        /// Obtiene el password necesario para la conexión con el ESB.
        /// </summary>
        /// <returns>Password.</returns>
        public string GetPasswordEsbPrc()
        {
            if (string.IsNullOrEmpty(passwordESBprc))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("password_esb_prc"))
                {
                    connectionString = environmentVariables["password_esb_prc"] as string;
                }
                else
                {
                    connectionString = configuracion["password_esb_prc"];
                }

                passwordESBprc = connectionString;
            }

            return passwordESBprc;
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

        public string GetUrlExportador()
        {
            if (string.IsNullOrEmpty(urlExportador))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_exportador"))
                {
                    connectionString = environmentVariables["url_exportador"] as string;
                }
                else
                {
                    connectionString = configuracion["url_exportador"];
                }

                urlExportador = connectionString;
            }

            return urlExportador;
        }
        public string GetUrlExportadorLimitado()
        {
            if (string.IsNullOrEmpty(urlExportadorLimitado))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_exportador_limitado"))
                {
                    connectionString = environmentVariables["url_exportador_limitado"] as string;
                }
                else
                {
                    connectionString = configuracion["url_exportador_limitado"];
                }

                urlExportadorLimitado = connectionString;
            }

            return urlExportadorLimitado;
        }
    }
}
