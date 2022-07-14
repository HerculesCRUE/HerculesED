using Hercules.ED.Synchronization.Config;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.Synchronization.Models
{
    public class Queue
    {
        private ConfigService configService;
        private readonly ConnectionFactory connectionFactory;
        private readonly IConnection connection;

        public Queue(ConfigService pConfigService)
        {
            configService = pConfigService;
            connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(configService.GetRabbitConnectionString())
            };

            connection = connectionFactory.CreateConnection();
        }

        /// <summary>
        /// Inserta en la cola de las fuentes externas.
        /// </summary>
        /// <param name="pOrcid">ORCID del usuario.</param>
        /// <param name="pSyncro">Objeto de la sincronización.</param>
        public void InsertToQueueFuentesExternas(string pOrcid, Queue pRabbitMQService, string pUltimaFechaMod)
        {           
            // Publicaciones.
            List<string> listaDatos = new List<string>() { "investigador", pOrcid, pUltimaFechaMod };
            pRabbitMQService.PublishMessage(listaDatos, configService.GetQueueRabbit());
        }

        /// <summary>
        /// Encola un objeto en Rabbit
        /// </summary>
        /// <param name="message">Objeto a encolar</param>
        /// <param name="queue">Cola</param>
        public void PublishMessage(object message, string queue)
        {
            using (var conn = connectionFactory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: queue,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    var jsonPayload = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(jsonPayload);

                    channel.BasicPublish(exchange: "",
                        routingKey: queue,
                        basicProperties: null,
                        body: body
                    );
                }
            }
        }
    }
}
