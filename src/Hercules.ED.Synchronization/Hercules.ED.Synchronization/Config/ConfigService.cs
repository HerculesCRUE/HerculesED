using Microsoft.Extensions.Configuration;
using System.Collections;

namespace Hercules.ED.Synchronization.Config
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // LogPath.
        public string logPath { get; set; }

        // Cron Expression.
        public string cronExternalSource { get; set; }

        // RabbitMQ.
        public string rabbitConnectionString { get; set; }
        public string FuentesExternasQueueRabbit { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}appsettings{Path.DirectorySeparatorChar}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la ruta de generación de logs.
        /// </summary>
        /// <returns>Ruta de logs.</returns>
        public string GetLogPath()
        {
            if (string.IsNullOrEmpty(logPath))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string connectionString = string.Empty;

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
        /// Obtiene la expresión cron para las fuentes externas.
        /// </summary>
        /// <returns>Expresión cron.</returns>
        public string GetCronExternalSource()
        {
            if (string.IsNullOrEmpty(cronExternalSource))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string connectionString = string.Empty;

                if (environmentVariables.Contains("cronExternalSource"))
                {
                    connectionString = environmentVariables["cronExternalSource"] as string;
                }
                else
                {
                    connectionString = configuracion["cronExternalSource"];
                }

                cronExternalSource = connectionString;
            }

            return cronExternalSource;
        }

        /// <summary>
        /// Obtiene la cadena de conexión de Rabbit configurada.
        /// </summary>
        /// <returns>Cadena de conexión de Rabbit.</returns>
        public string GetRabbitConnectionString()
        {
            if (string.IsNullOrEmpty(rabbitConnectionString))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string connectionString = string.Empty;

                if (environmentVariables.Contains("RabbitMQ"))
                {
                    connectionString = environmentVariables["RabbitMQ"] as string;
                }
                else
                {
                    connectionString = configuracion.GetConnectionString("RabbitMQ");
                }

                rabbitConnectionString = connectionString;
            }

            return rabbitConnectionString;
        }

        /// <summary>
        /// Obtiene la el nombre de la cola Rabbit de configuración.
        /// </summary>
        /// <returns>Nombre de la cola Rabbit.</returns>
        public string GetFuentesExternasQueueRabbit()
        {
            if (string.IsNullOrEmpty(FuentesExternasQueueRabbit))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue = string.Empty;

                if (environmentVariables.Contains("FuentesExternasQueueRabbit"))
                {
                    queue = environmentVariables["FuentesExternasQueueRabbit"] as string;
                }
                else
                {
                    queue = configuracion["FuentesExternasQueueRabbit"];
                }

                FuentesExternasQueueRabbit = queue;
            }

            return FuentesExternasQueueRabbit;
        }
    }
}
