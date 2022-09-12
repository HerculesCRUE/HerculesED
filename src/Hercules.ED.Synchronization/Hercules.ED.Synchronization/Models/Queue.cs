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

        /// <summary>
        /// Main queue.
        /// </summary>
        /// <param name="pConfigService">Configuración.</param>
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
        /// <param name="pRabbitMQService">Cola de Rabbit.</param>
        /// <param name="pUltimaFechaMod">Última fecha de modificación.</param>
        /// <param name="pDicIDs">Diccionario con los IDs necesarios de FigShare y GitHub.</param>
        public void InsertToQueueFuentesExternas(string pOrcid, Queue pRabbitMQService, string pUltimaFechaMod, Dictionary<string, string> pDicIDs, string pGnossId)
        {
            // Publicaciones.
            List<string> listaDatos = new List<string>() { "investigador", pOrcid, pUltimaFechaMod, pGnossId };
            pRabbitMQService.PublishMessage(listaDatos, configService.GetQueueRabbit());

            // Zenodo.
            List<string> listaDatosZenodo = new List<string>() { "zenodo", pOrcid };
            pRabbitMQService.PublishMessage(listaDatosZenodo, pUltimaFechaMod);

            // FigShare.
            if (pDicIDs.ContainsKey("usuarioFigshare") && pDicIDs.ContainsKey("tokenFigshare"))
            {
                List<string> listaDatosFigShare = new List<string>() { "figshare", pDicIDs["tokenFigshare"] };
                pRabbitMQService.PublishMessage(listaDatosFigShare, configService.GetQueueRabbit());
            }

            // GitHub.
            if (pDicIDs.ContainsKey("usuarioGitHub") && pDicIDs.ContainsKey("tokenGitHub"))
            {
                List<string> listaDatosGitHub = new List<string>() { "github", pDicIDs["usuarioGitHub"], pDicIDs["tokenGitHub"] };
                pRabbitMQService.PublishMessage(listaDatosGitHub, configService.GetQueueRabbit());
            }
        }

        /// <summary>
        /// Encola un objeto en Rabbit.
        /// </summary>
        /// <param name="message">Objeto a encolar.</param>
        /// <param name="queue">Cola.</param>
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
