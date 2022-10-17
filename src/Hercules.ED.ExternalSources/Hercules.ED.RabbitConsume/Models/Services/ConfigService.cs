using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gnoss.Web.ReprocessData.Models.Services
{
    public class ConfigService
    {
        private IConfiguration _configuration { get; set; }
        private string RabbitConnectionString { get; set; }
        private string FuentesExternasQueueRabbit { get; set; }
        private string urlPublicacion { get; set; }
        private string rutaDirectorioEscritura { get; set; }
        private string urlZenodo { get; set; }
        private string urlFigShare { get; set; }
        private string urlGitHub { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration"></param>
        public ConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
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
                    rabbitConnectionString = _configuration.GetConnectionString("RabbitMQ");
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
                string queue = string.Empty;
                if (environmentVariables.Contains("FuentesExternasQueueRabbit"))
                {
                    queue = environmentVariables["FuentesExternasQueueRabbit"] as string;
                }
                else
                {
                    queue = _configuration["FuentesExternasQueueRabbit"];
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
            if (string.IsNullOrEmpty(urlPublicacion))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue = string.Empty;
                if (environmentVariables.Contains("UrlPublicacion"))
                {
                    queue = environmentVariables["UrlPublicacion"] as string;
                }
                else
                {
                    queue = _configuration["UrlPublicacion"];
                }
                urlPublicacion = queue;
            }
            return urlPublicacion;
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
                    connectionString = _configuration["DirectorioEscritura"];
                }

                rutaDirectorioEscritura = connectionString;
            }

            return rutaDirectorioEscritura;
        }

        /// <summary>
        /// Obtiene la URL del API de Zenodo que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de Zenodo.</returns>
        public string GetUrlZenodo()
        {
            if (string.IsNullOrEmpty(urlZenodo))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue = string.Empty;
                if (environmentVariables.Contains("UrlZenodo"))
                {
                    queue = environmentVariables["UrlZenodo"] as string;
                }
                else
                {
                    queue = _configuration["UrlZenodo"];
                }
                urlZenodo = queue;
            }
            return urlZenodo;
        }

        /// <summary>
        /// Obtiene la URL del API de FigShare que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de FigShare.</returns>
        public string GetUrlFigShare()
        {
            if (string.IsNullOrEmpty(urlFigShare))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue = string.Empty;
                if (environmentVariables.Contains("UrlFigShare"))
                {
                    queue = environmentVariables["UrlFigShare"] as string;
                }
                else
                {
                    queue = _configuration["UrlFigShare"];
                }
                urlFigShare = queue;
            }
            return urlFigShare;
        }

        /// <summary>
        /// Obtiene la URL del API de GitHub que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de GitHub.</returns>
        public string GetUrlGitHub()
        {
            if (string.IsNullOrEmpty(urlGitHub))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue = string.Empty;
                if (environmentVariables.Contains("UrlGitHub"))
                {
                    queue = environmentVariables["UrlGitHub"] as string;
                }
                else
                {
                    queue = _configuration["UrlGitHub"];
                }
                urlGitHub = queue;
            }
            return urlGitHub;
        }
    }
}
