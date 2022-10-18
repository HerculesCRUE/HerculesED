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
            return "https://api.openaire.eu";
        }

    }
}
