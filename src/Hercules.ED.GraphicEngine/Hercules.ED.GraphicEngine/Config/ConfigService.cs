using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace Hercules.ED.GraphicEngine.Config
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la URL base del API de FigShare que ha sido configurada.
        /// </summary>
        /// <returns>URI del API de FigShare.</returns>
        //public string GetUrlBase()
        //{
        //    if (string.IsNullOrEmpty(urlBase))
        //    {
        //        string connectionString = string.Empty;
        //        IDictionary environmentVariables = Environment.GetEnvironmentVariables();
        //        if (environmentVariables.Contains("UrlBase"))
        //        {
        //            connectionString = environmentVariables["UrlBase"] as string;
        //        }
        //        else
        //        {
        //            connectionString = configuracion["UrlBase"];
        //        }

        //        urlBase = connectionString;
        //    }

        //    return urlBase;
        //}
    }
}
