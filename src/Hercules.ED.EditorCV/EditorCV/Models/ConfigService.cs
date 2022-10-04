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
        private string usernameDspace { get; set; }
        private string passwordDspace { get; set; }

        // URLs
        private string urlEnrichment { get; set; }
        private string urlSGI { get; set; }
        private string urlDSpace { get; set; }
        private string collectionDSpace { get; set; }
        private string urlImportadorExportador { get; set; }
        private int? maxMonthsValidationDocument { get; set; }
        private int? maxMonthsValidationProjectsDocument { get; set; }

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
        public string GetUrlEnrichment()
        {
            if (string.IsNullOrEmpty(urlEnrichment))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_enriquecimiento"))
                {
                    connectionString = environmentVariables["url_enriquecimiento"] as string;
                }
                else
                {
                    connectionString = configuracion["url_enriquecimiento"];
                }

                urlEnrichment = connectionString;
            }

            return urlEnrichment;
        }

        public string GetUsernameDspace()
        {
            if (string.IsNullOrEmpty(usernameDspace))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("username_dspace"))
                {
                    connectionString = environmentVariables["username_dspace"] as string;
                }
                else
                {
                    connectionString = configuracion["username_dspace"];
                }

                usernameDspace = connectionString;
            }

            return usernameDspace;
        }

        public string GetPasswordDspace()
        {
            if (string.IsNullOrEmpty(passwordDspace))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("password_dspace"))
                {
                    connectionString = environmentVariables["password_dspace"] as string;
                }
                else
                {
                    connectionString = configuracion["password_dspace"];
                }

                passwordDspace = connectionString;
            }

            return passwordDspace;
        }

        /// <summary>
        /// Obtiene la URL del API del enriquecimiento de tópicos temáticos que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de enriquecimiento.</returns>
        public string GetUrlThematicEnrichment()
        {
            return GetUrlEnrichment() + "/thematic";
        }

        /// <summary>
        /// Obtiene la URL del API del enriquecimiento de tópicos específicos que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de enriquecimiento.</returns>
        public string GetUrlSpecificEnrichment()
        {
            return GetUrlEnrichment() + "/specific";
        }

        /// <summary>
        /// Obtiene la URL del API de obtención de tokens.
        /// </summary>
        /// <returns>URI del API de tokens.</returns>
        public string GetUrlSGI()
        {
            if (string.IsNullOrEmpty(urlSGI))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_sgi"))
                {
                    connectionString = environmentVariables["url_sgi"] as string;
                }
                else
                {
                    connectionString = configuracion["url_sgi"];
                }

                urlSGI = connectionString;
            }

            return urlSGI;
        }

        /// <summary>
        /// Obtiene la URL del API de obtención de tokens.
        /// </summary>
        /// <returns>URI del API de tokens.</returns>
        public string GetUrlToken()
        {
            return GetUrlSGI() + "/auth/realms/sgi/protocol/openid-connect/token";
        }

        /// <summary>
        /// Obtiene la URL del API del envío a producción científica.
        /// </summary>
        /// <returns>URI del API de envío a producción científica.</returns>
        public string GetUrlProduccionCientifica()
        {
            return GetUrlSGI() + "/api/sgiprc/producciones-cientificas";
        }

        /// <summary>
        /// Obtiene la URL del API del envío a validar proyecto.
        /// </summary>
        /// <returns>URI del API de envío a producción científica.</returns>
        public string GetUrlEnvioProyecto()
        {
            return GetUrlSGI() + "/api/sgicsp/notificacionesproyectosexternoscvn";
        }

        /// <summary>
        /// Obtiene la url del servicio importador/exportador
        /// </summary>
        /// <returns></returns>
        public string GetUrlimportadorExportador()
        {
            if (string.IsNullOrEmpty(urlImportadorExportador))
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

                urlImportadorExportador = connectionString;
            }

            return urlImportadorExportador;
        }

        /// <summary>
        /// Obtiene los meses máximos para el envío a validación de documentos
        /// </summary>
        /// <returns></returns>
        public int GetMaxMonthsValidationDocument()
        {
            if (!maxMonthsValidationDocument.HasValue)
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("max_months_validation_document"))
                {
                    connectionString = environmentVariables["max_months_validation_document"] as string;
                }
                else
                {
                    connectionString = configuracion["max_months_validation_document"];
                }
                maxMonthsValidationDocument = int.Parse(connectionString);
            }
            return maxMonthsValidationDocument.Value;
        }

        /// <summary>
        /// Obtiene los meses máximos de los proyectos para el envío a validación de documentos
        /// </summary>
        /// <returns></returns>
        public int GetMaxMonthsValidationProjectsDocument()
        {
            if (!maxMonthsValidationProjectsDocument.HasValue)
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("max_months_validation_projects_document"))
                {
                    connectionString = environmentVariables["max_months_validation_projects_document"] as string;
                }
                else
                {
                    connectionString = configuracion["max_months_validation_projects_document"];
                }
                maxMonthsValidationProjectsDocument = int.Parse(connectionString);
            }
            return maxMonthsValidationProjectsDocument.Value;
        }

        /// <summary>
        /// Obtiene la URL del controlador del exportador
        /// </summary>
        /// <returns></returns>
        public string GetUrlExportador()
        {
            return GetUrlimportadorExportador() + "/ExportadorCV";
        }

        /// <summary>
        /// Obtiene la URL del controlador del importador
        /// </summary>
        /// <returns></returns>
        public string GetUrlImportador()
        {
            return GetUrlimportadorExportador() + "/ImportadorCV";
        }

        public string GetUrlDSpace()
        {
            if (string.IsNullOrEmpty(urlDSpace))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("url_dspace"))
                {
                    connectionString = environmentVariables["url_dspace"] as string;
                }
                else
                {
                    connectionString = configuracion["url_dspace"];
                }

                urlDSpace = connectionString;
            }

            return urlDSpace;
        }
        
        public string GetCollectionDSpace()
        {
            if (string.IsNullOrEmpty(collectionDSpace))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("collection_dspace"))
                {
                    connectionString = environmentVariables["collection_dspace"] as string;
                }
                else
                {
                    connectionString = configuracion["collection_dspace"];
                }

                collectionDSpace = connectionString;
            }

            return collectionDSpace;
        }
    }
}
