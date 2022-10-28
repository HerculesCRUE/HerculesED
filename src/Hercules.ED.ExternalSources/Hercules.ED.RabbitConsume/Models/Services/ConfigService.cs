using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.IO;

namespace Gnoss.Web.ReprocessData.Models.Services
{
    public class ConfigService
    {
        private IConfiguration Configuration { get; set; }
        private string RabbitConnectionString { get; set; }
        private string FuentesExternasQueueRabbit { get; set; }
        private string UrlPublicacion { get; set; }
        private string RutaDirectorioEscritura { get; set; }
        private string UrlZenodo { get; set; }
        private string UrlFigShare { get; set; }
        private string UrlGitHub { get; set; }
        private string LogPath { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration"></param>
        public ConfigService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Contruye el objeto de lectura con la configuración del JSON.
        /// </summary>
        /// <returns></returns>
        public static IConfiguration GetBuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

            return builder.Build();
        }

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
                    connectionString = Configuration["LogPath"];
                }

                LogPath = connectionString;
            }

            return LogPath;
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
                    rabbitConnectionString = Configuration.GetConnectionString("RabbitMQ");
                }

                RabbitConnectionString = rabbitConnectionString;
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
                    queue = Configuration["FuentesExternasQueueRabbit"];
                }

                FuentesExternasQueueRabbit = queue;
            }

            return FuentesExternasQueueRabbit;
        }

        /// <summary>
        /// Obtiene la URL del API de Publicacion que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de Publicacion.</returns>
        public string GetUrlPublicacion()
        {
            if (string.IsNullOrEmpty(UrlPublicacion))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue;

                if (environmentVariables.Contains("UrlPublicacion"))
                {
                    queue = environmentVariables["UrlPublicacion"] as string;
                }
                else
                {
                    queue = Configuration["UrlPublicacion"];
                }

                UrlPublicacion = queue;
            }

            return UrlPublicacion;
        }

        /// <summary>
        /// Obtiene la ruta de escritura de los ficheros.
        /// </summary>
        /// <returns>Ruta de escritura.</returns>
        public string GetRutaDirectorioEscritura()
        {
            if (string.IsNullOrEmpty(RutaDirectorioEscritura))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string connectionString;

                if (environmentVariables.Contains("DirectorioEscritura"))
                {
                    connectionString = environmentVariables["DirectorioEscritura"] as string;
                }
                else
                {
                    connectionString = Configuration["DirectorioEscritura"];
                }

                RutaDirectorioEscritura = connectionString;
            }

            return RutaDirectorioEscritura;
        }

        /// <summary>
        /// Obtiene la URL del API de Zenodo que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de Zenodo.</returns>
        public string GetUrlZenodo()
        {
            if (string.IsNullOrEmpty(UrlZenodo))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue;

                if (environmentVariables.Contains("UrlZenodo"))
                {
                    queue = environmentVariables["UrlZenodo"] as string;
                }
                else
                {
                    queue = Configuration["UrlZenodo"];
                }

                UrlZenodo = queue;
            }

            return UrlZenodo;
        }

        /// <summary>
        /// Obtiene la URL del API de FigShare que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de FigShare.</returns>
        public string GetUrlFigShare()
        {
            if (string.IsNullOrEmpty(UrlFigShare))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue;

                if (environmentVariables.Contains("UrlFigShare"))
                {
                    queue = environmentVariables["UrlFigShare"] as string;
                }
                else
                {
                    queue = Configuration["UrlFigShare"];
                }

                UrlFigShare = queue;
            }

            return UrlFigShare;
        }

        /// <summary>
        /// Obtiene la URL del API de GitHub que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de GitHub.</returns>
        public string GetUrlGitHub()
        {
            if (string.IsNullOrEmpty(UrlGitHub))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue;

                if (environmentVariables.Contains("UrlGitHub"))
                {
                    queue = environmentVariables["UrlGitHub"] as string;
                }
                else
                {
                    queue = Configuration["UrlGitHub"];
                }

                UrlGitHub = queue;
            }

            return UrlGitHub;
        }
    }
}
