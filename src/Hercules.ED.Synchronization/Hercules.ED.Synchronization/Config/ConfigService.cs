﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.Synchronization.Config
{
    public class ConfigService
    {
        // Archivo de configuración.
        public static IConfigurationRoot configuracion;

        // LogPath
        public string logPath { get; set; }

        // Cron Expression
        public string cronExternalSource { get; set; }

        // RabbitMQ
        public string rabbitConnectionString { get; set; }
        public string queueRabbit { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigService()
        {
            configuracion = new ConfigurationBuilder().AddJsonFile($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/appsettings/appsettings.json").Build();
        }

        /// <summary>
        /// Obtiene la ruta de generación de logs.
        /// </summary>
        /// <returns>Ruta de logs.</returns>
        public string GetLogPath()
        {
            if (string.IsNullOrEmpty(logPath))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("LogPath"))
                {
                    connectionString = environmentVariables["LogPath"] as string;
                }
                else
                {
                    connectionString = configuracion["LogPath"];
                }

                logPath = connectionString;
            }

            return logPath;
        }

        /// <summary>
        /// Obtiene la expresión cron para las fuentes externas.
        /// </summary>
        /// <returns>Expresión cron.</returns>
        public string GetCronExternalSource()
        {
            if (string.IsNullOrEmpty(cronExternalSource))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("cronExternalSource"))
                {
                    connectionString = environmentVariables["cronExternalSource"] as string;
                }
                else
                {
                    connectionString = configuracion["cronExternalSource"];
                }

                cronExternalSource = connectionString;
            }

            return cronExternalSource;
        }

        /// <summary>
        /// Obtiene la cadena de conexión de Rabbit configurada.
        /// </summary>
        /// <returns>Cadena de conexión de Rabbit.</returns>
        public string GetRabbitConnectionString()
        {
            if (string.IsNullOrEmpty(rabbitConnectionString))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("colaFuentesExternas"))
                {
                    rabbitConnectionString = environmentVariables["colaFuentesExternas"] as string;
                }
                else
                {
                    rabbitConnectionString = configuracion["RabbitMQ:colaFuentesExternas"];
                }
            }
            return rabbitConnectionString;
        }

        /// <summary>
        /// Obtiene la el nombre de la cola Rabbit de configuración.
        /// </summary>
        /// <returns>Nombre de la cola Rabbit.</returns>
        public string GetQueueRabbit()
        {
            if (string.IsNullOrEmpty(queueRabbit))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue = "";
                if (environmentVariables.Contains("QueueRabbit"))
                {
                    queue = environmentVariables["QueueRabbit"] as string;
                }
                else
                {
                    queue = configuracion["QueueRabbit"];
                }
                queueRabbit = queue;
            }
            return queueRabbit;
        }
    }
}
