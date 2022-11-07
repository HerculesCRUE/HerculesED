using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.IO;

namespace Hercules.ED.ResearcherObjectLoad.Config
{
    public class ConfigService
    {
        // Archivo de configuración.
        private static readonly IConfigurationRoot configuracion = new ConfigurationBuilder().AddJsonFile($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}appsettings{Path.DirectorySeparatorChar}appsettings.json").Build();

        // Rutas.
        private string RutaDirectorioLectura { get; set; }
        private string RutaDirectorioEscritura { get; set; }

        // Ruta Logs
        private string LogPath { get; set; }

        // Configuración Rabbit para el desnormalizador.
        private string RabbitConnectionString { get; set; }
        private string DenormalizerQueueRabbit { get; set; }

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

        /// <summary>
        /// Obtiene la cadena de conexión de Rabbit configurada.
        /// </summary>
        /// <returns>Cadena de conexión de Rabbit.</returns>
        public string GetrabbitConnectionString()
        {
            if (string.IsNullOrEmpty(RabbitConnectionString))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string rabbitConnectionString;

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
        /// Obtiene la el nombre de la cola Rabbit de desnormalización de configuración.
        /// </summary>
        /// <returns>Nombre de la cola Rabbit.</returns>
        public string GetDenormalizerQueueRabbit()
        {
            if (string.IsNullOrEmpty(DenormalizerQueueRabbit))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue;

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
    }
}