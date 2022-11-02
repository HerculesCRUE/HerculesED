using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace Hercules.ED.ResearcherObjectLoad.Config
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // Rutas
        private string rutaDirectorioLectura { get; set; }
        private string rutaDirectorioEscritura { get; set; }

        //Configuración Rabbit para el desnormalizador
        private string RabbitConnectionString { get; set; }
        private string DenormalizerQueueRabbit { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/appsettings/appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la ruta de lectura de los ficheros.
        /// </summary>
        /// <returns>Ruta de lectura.</returns>
        public string GetRutaDirectorioLectura()
        {
            if (string.IsNullOrEmpty(rutaDirectorioLectura))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("DirectorioLectura"))
                {
                    connectionString = environmentVariables["DirectorioLectura"] as string;
                }
                else
                {
                    connectionString = configuracion["DirectorioLectura"];
                }

                rutaDirectorioLectura = connectionString;
            }

            return rutaDirectorioLectura;
        }

        /// <summary>
        /// Obtiene la ruta de escritura de los ficheros.
        /// </summary>
        /// <returns>Ruta de escritura.</returns>
        public string GetRutaDirectorioEscritura()
        {
            if (string.IsNullOrEmpty(rutaDirectorioEscritura))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("DirectorioEscritura"))
                {
                    connectionString = environmentVariables["DirectorioEscritura"] as string;
                }
                else
                {
                    connectionString = configuracion["DirectorioEscritura"];
                }

                rutaDirectorioEscritura = connectionString;
            }

            return rutaDirectorioEscritura;
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
    }
}