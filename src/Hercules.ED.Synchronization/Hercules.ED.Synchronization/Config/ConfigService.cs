using Microsoft.Extensions.Configuration;
using System.Collections;

namespace Hercules.ED.Synchronization.Config
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static readonly IConfigurationRoot configuracion = new ConfigurationBuilder().AddJsonFile($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}appsettings{Path.DirectorySeparatorChar}appsettings.json").Build();

        // LogPath.
        public string LogPath { get; set; }

        // Cron Expression.
        public string CronExternalSource { get; set; }

        // RabbitMQ.
        public string RabbitConnectionString { get; set; }
        public string FuentesExternasQueueRabbit { get; set; }

        /// <summary>
        /// Obtiene la ruta de generación de logs.
        /// </summary>
        /// <returns>Ruta de logs.</returns>
        public string GetLogPath()
        {
            if (string.IsNullOrEmpty(LogPath))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string connectionString;

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
        /// Obtiene la expresión cron para las fuentes externas.
        /// </summary>
        /// <returns>Expresión cron.</returns>
        public string GetCronExternalSource()
        {
            if (string.IsNullOrEmpty(CronExternalSource))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string connectionString;

                if (environmentVariables.Contains("cronExternalSource"))
                {
                    connectionString = environmentVariables["cronExternalSource"] as string;
                }
                else
                {
                    connectionString = configuracion["cronExternalSource"];
                }

                CronExternalSource = connectionString;
            }

            return CronExternalSource;
        }

        /// <summary>
        /// Obtiene la cadena de conexión de Rabbit configurada.
        /// </summary>
        /// <returns>Cadena de conexión de Rabbit.</returns>
        public string GetRabbitConnectionString()
        {
            if (string.IsNullOrEmpty(RabbitConnectionString))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string connectionString;

                if (environmentVariables.Contains("RabbitMQ"))
                {
                    connectionString = environmentVariables["RabbitMQ"] as string;
                }
                else
                {
                    connectionString = configuracion.GetConnectionString("RabbitMQ");
                }

                RabbitConnectionString = connectionString;
            }

            return RabbitConnectionString;
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
                string queue;

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
