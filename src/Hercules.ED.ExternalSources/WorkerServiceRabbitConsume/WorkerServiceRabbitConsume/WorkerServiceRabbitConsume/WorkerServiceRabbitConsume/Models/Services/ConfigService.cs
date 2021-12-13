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
        private string QueueRabbit { get; set; }
        /// <summary>
        /// ConfigService
        /// </summary>
        /// <param name="configuration"></param>
        public ConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// GetBuildConfiguration
        /// </summary>
        /// <returns></returns>
        public static IConfiguration GetBuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

            return builder.Build();
        }

        ///<summary>
        ///Método que obtiene la ConfigUrl configurada
        ///</summary>
        public string GetrabbitConnectionString()
        {
            if (string.IsNullOrEmpty(RabbitConnectionString))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string rabbitConnectionString = "";
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
        public string GetQueueRabbit()
        {
            if (string.IsNullOrEmpty(QueueRabbit))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string queue = "";
                if (environmentVariables.Contains("QueueRabbit"))
                {
                    queue = environmentVariables["QueueRabbit"] as string;
                }
                else
                {
                    queue = _configuration["QueueRabbit"];
                }
                QueueRabbit = queue;
            }
            return QueueRabbit;
        }
    }
}
