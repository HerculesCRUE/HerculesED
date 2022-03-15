using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.ImportadorWebCV.Controllers
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // URLs
        private string Usuario_PDF { get; set; }
        private string PSS_PDF { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
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
    }
}
