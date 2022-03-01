using Microsoft.Extensions.Configuration;
using System;
using System.Collections;

namespace OAI_PMH.Controllers
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // URLs
        private string ConfigUrl { get; set; }
        private string UrlBaseToken { get; set; }
        private string UrlBaseProyecto { get; set; }
        private string UrlBasePersona { get; set; }
        private string UrlBaseOrganizacion { get; set; }
        private string UrlBaseEstructuraOrganica { get; set; }
        private string UrlBaseFormacionAcademica { get; set; }
        private string UrlBaseActividadDocente { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la URL base del API de obtención del OAI-PMH que ha sido configurada.
        /// </summary>
        /// <returns></returns>
        public string GetConfigUrl()
        {
            if (string.IsNullOrEmpty(ConfigUrl))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("ConfigUrl"))
                {
                    connectionString = environmentVariables["ConfigUrl"] as string;
                }
                else
                {
                    connectionString = configuracion["ConfigUrl"];
                }

                ConfigUrl = connectionString;
            }

            return ConfigUrl;
        }

        /// <summary>
        /// Obtiene la URL base del API de obtención de Actividad Docente que ha sido configurada.
        /// </summary>
        /// <returns></returns>
        public string GetUrlBaseToken()
        {
            if (string.IsNullOrEmpty(UrlBaseToken))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlBaseToken"))
                {
                    connectionString = environmentVariables["UrlBaseToken"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlBaseToken"];
                }

                UrlBaseToken = connectionString;
            }

            return UrlBaseToken;
        }

        /// <summary>
        /// Obtiene la URL base del API de obtención de Proyectos que ha sido configurada.
        /// </summary>
        /// <returns></returns>
        public string GetUrlBaseProyecto()
        {
            if (string.IsNullOrEmpty(UrlBaseProyecto))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlBaseProyecto"))
                {
                    connectionString = environmentVariables["UrlBaseProyecto"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlBaseProyecto"];
                }

                UrlBaseProyecto = connectionString;
            }

            return UrlBaseProyecto;
        }

        /// <summary>
        /// Obtiene la URL base del API de obtención de Personas que ha sido configurada.
        /// </summary>
        /// <returns></returns>
        public string GetUrlBasePersona()
        {
            if (string.IsNullOrEmpty(UrlBasePersona))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlBasePersona"))
                {
                    connectionString = environmentVariables["UrlBasePersona"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlBasePersona"];
                }

                UrlBasePersona = connectionString;
            }

            return UrlBasePersona;
        }

        /// <summary>
        /// Obtiene la URL base del API de obtención de Organizaciones que ha sido configurada.
        /// </summary>
        /// <returns></returns>
        public string GetUrlBaseOrganizacion()
        {
            if (string.IsNullOrEmpty(UrlBaseOrganizacion))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlBaseOrganizacion"))
                {
                    connectionString = environmentVariables["UrlBaseOrganizacion"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlBaseOrganizacion"];
                }

                UrlBaseOrganizacion = connectionString;
            }

            return UrlBaseOrganizacion;
        }

        /// <summary>
        /// Obtiene la URL base del API de obtención de Estructura Organica que ha sido configurada.
        /// </summary>
        /// <returns></returns>
        public string GetUrlBaseEstructuraOrganica()
        {
            if (string.IsNullOrEmpty(UrlBaseEstructuraOrganica))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlBaseEstructuraOrganica"))
                {
                    connectionString = environmentVariables["UrlBaseEstructuraOrganica"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlBaseEstructuraOrganica"];
                }

                UrlBaseEstructuraOrganica = connectionString;
            }

            return UrlBaseEstructuraOrganica;
        }

        /// <summary>
        /// Obtiene la URL base del API de obtención de Formacion Academica que ha sido configurada.
        /// </summary>
        /// <returns></returns>
        public string GetUrlBaseFormacionAcademica()
        {
            if (string.IsNullOrEmpty(UrlBaseFormacionAcademica))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlBaseFormacionAcademica"))
                {
                    connectionString = environmentVariables["UrlBaseFormacionAcademica"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlBaseFormacionAcademica"];
                }

                UrlBaseFormacionAcademica = connectionString;
            }

            return UrlBaseFormacionAcademica;
        }

        /// <summary>
        /// Obtiene la URL base del API de obtención de Actividad Docente que ha sido configurada.
        /// </summary>
        /// <returns></returns>
        public string GetUrlBaseActividadDocente()
        {
            if (string.IsNullOrEmpty(UrlBaseActividadDocente))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlBaseActividadDocente"))
                {
                    connectionString = environmentVariables["UrlBaseActividadDocente"] as string;
                }
                else
                {
                    connectionString = configuracion["UrlBaseActividadDocente"];
                }

                UrlBaseActividadDocente = connectionString;
            }

            return UrlBaseActividadDocente;
        }
    }
}
