using RabbitMQ.Client;
using System;
using System.Text;
using Newtonsoft.Json;
using Hercules.ED.ResearcherObjectLoad.Config;

namespace Hercules.ED.ResearcherObjectLoad.Models
{
    public class RabbitServiceWriterDenormalizer
    {
        private readonly ConfigService _configService;
        private readonly ConnectionFactory connectionFactory;


        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="configService"></param>
        public RabbitServiceWriterDenormalizer(ConfigService configService)
        {
            _configService = configService;
            connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_configService.GetrabbitConnectionString())
            };
        }


        /// <summary>
        /// Encola un objeto en Rabbit.
        /// </summary>
        /// <param name="message">Objeto a encolar</param>
        /// <param name="queue">Cola</param>
        public void PublishMessage(DenormalizerItemQueue item)
        {
            using (var conn = connectionFactory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: _configService.GetDenormalizerQueueRabbit(),
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    var jsonPayload = JsonConvert.SerializeObject(item);
                    var body = Encoding.UTF8.GetBytes(jsonPayload);

                    channel.BasicPublish(exchange: string.Empty,
                        routingKey: _configService.GetDenormalizerQueueRabbit(),
                        basicProperties: null,
                        body: body
                    );
                }
            }
        }
    }
}

