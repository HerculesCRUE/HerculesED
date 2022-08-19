using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace Hercules.ED.ImportExportCV.Controllers
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // URLs
        private string Usuario_PDF { get; set; }
        private string PSS_PDF { get; set; }
        private string Version { get; set; }
        private string UrlEnriquecimiento { get; set; }
        private string UrlServicioExterno { get; set; }

        //Configuración Rabbit para el desnormalizador
        private string RabbitConnectionString { get; set; }
        private string DenormalizerQueueRabbit { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        public string GetVersion()
        {
            if (string.IsNullOrEmpty(Version))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("Version"))
                {
                    connectionString = environmentVariables["Version"] as string;
                }
                else
                {
                    connectionString = configuracion["Version"];
                }

                Version = connectionString;
            }

            return Version;
        }

        /// <summary>
        /// Obtiene el usuario del conversor de PDF que ha sido configurado.
        /// </summary>
        /// <returns>Usuario del conversor de PDF.</returns>
        public string GetUsuarioPDF()
        {
            if (string.IsNullOrEmpty(Usuario_PDF))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("Usuario_PDF"))
                {
                    connectionString = environmentVariables["Usuario_PDF"] as string;
                }
                else
                {
                    connectionString = configuracion["Usuario_PDF"];
                }

                Usuario_PDF = connectionString;
            }

            return Usuario_PDF;
        }

        /// <summary>
        /// Obtiene la contraseña del conversor de PDF que ha sido configurado.
        /// </summary>
        /// <returns>Usuario del conversor de PDF.</returns>
        public string GetContraseñaPDF()
        {
            if (string.IsNullOrEmpty(PSS_PDF))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("PSS_PDF"))
                {
                    connectionString = environmentVariables["PSS_PDF"] as string;
                }
                else
                {
                    connectionString = configuracion["PSS_PDF"];
                }

                PSS_PDF = connectionString;
            }

            return PSS_PDF;
        }

        public string GetUrlEnriquecimiento() {
            if (string.IsNullOrEmpty(UrlEnriquecimiento))
            {
                string connectionString = string.Empty;
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
        
        public string GetUrlServicioExterno() {
            if (string.IsNullOrEmpty(UrlServicioExterno))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlServicioExterno"))
                {
                    connectionString = environmentVariables["UrlServicioExterno"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlServicioExterno"];
                }

                UrlServicioExterno = connectionString;
            }

            return UrlServicioExterno;
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
