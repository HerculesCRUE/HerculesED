using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAireAPI.Controllers
{    
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // URLs
        private string urlOpenAire { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la URL del API de OpenAire que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de OpenAire.</returns>
        public string GetUrlOpenAire()
        {
            if (string.IsNullOrEmpty(urlOpenAire))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlOpenAire"))
                {
                    connectionString = environmentVariables["UrlOpenAire"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlOpenAire"];
                }

                urlOpenAire = connectionString;
            }

            return urlOpenAire;
        }

    }
}
