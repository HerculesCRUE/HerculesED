using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Gnoss.Web.ReprocessData.Models;
using Newtonsoft.Json;
using System.Threading;
using static WorkerServiceRabbitConsume.Program;

namespace Gnoss.Web.ReprocessData.Models.Services
{
    public class ReadRabbitService
    {



        /// <summary>
        /// ReceivedDelegate
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public delegate bool ReceivedDelegate(string s);
        /// <summary>
        /// ShutDownDelegate
        /// </summary>
        public delegate void ShutDownDelegate();
        private ConfigService _configService;
        private readonly ConnectionFactory connectionFactory;
        private readonly IConnection connection;

        public ReadRabbitService(ConfigService configService)
        {
            _configService = configService;
            connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_configService.GetrabbitConnectionString())
            };

            connection = connectionFactory.CreateConnection();
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

        /// <summary>
        /// ListenToQueue
        /// </summary>
        /// <param name="receivedFunction"></param>
        /// <param name="shutdownFunction"></param>
        public void ListenToQueue(ReceivedDelegate receivedFunction, ShutDownDelegate shutdownFunction, string queue)
        {
            IModel channel = connection.CreateModel();
            channel.BasicQos(0, 1, false);

            channel.QueueDeclare(queue: queue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);

            eventingBasicConsumer.Received += (sender, basicDeliveryEventArgs) =>
            {
                try
                {
                    IBasicProperties basicProperties = basicDeliveryEventArgs.BasicProperties;
                    string body = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body.ToArray());

                    if (receivedFunction(body))
                    {
                        channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        channel.BasicNack(basicDeliveryEventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception)
                {
                    channel.BasicNack(basicDeliveryEventArgs.DeliveryTag, false, true);
                    throw;
                }
            };

            eventingBasicConsumer.Shutdown += (sender, shutdownEventArgs) =>
            {
                shutdownFunction();
            };

            channel.BasicConsume(queue, false, eventingBasicConsumer);
        }

        public string dir_fichero = @"FileDatosOut/";
        protected Dictionary<string, string> headers = new Dictionary<string, string>();

        /// <summary>
        /// A Http calls function
        /// </summary>
        /// <param name="url">the http call url</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        protected async Task<string> httpCall(string url, string method = "GET", Dictionary<string, string> headers = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(method), url))
                {
                    //request.Headers.TryAddWithoutValidation("X-ApiKey", bareer);
                    //request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");

                    if (headers != null && headers.Count > 0)
                    {
                        // if (headers.ContainsKey("Authorization"))
                        // {
                        //     request.Headers.TryAddWithoutValidation("Authorization", headers["Authorization"]);
                        // }
                        foreach (var item in headers)
                        {
                            request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    try
                    {
                        response = await httpClient.SendAsync(request);
                    }
                    catch (System.Exception)
                    {
                        throw new Exception("Error in the http call");
                    }
                }
            }
            if (response.Content != null)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "";
            }

        }

        // message = orcid 
        //fecha. 
        
        /// <summary>
        /// Permite mandar a procesar los datos a una cola Rabbit.
        /// {"investigador"; [ORCID]; "2021-11-01"} -> Obtiene todos los datos relacionados con ese autor desde una fecha indicada. 
        /// {"publicación"; [DOI]} -> Actualización de citas de un documento en concreto en las diversas fuentes que puedas encontrarlo.
        /// </summary>
        /// <param name="pMessage">Datos en formato string de un json.</param>
        /// <returns></returns>
        public bool ProcessItem(string pMessage)
        {
            List<string> message = JsonConvert.DeserializeObject<List<string>>(pMessage);
            if (message.Count() == 3 & message[0] == "investigador")
            {
                if (message[2] != null)
                {
                    Uri url = new Uri(string.Format(_configService.GetUrlPublicacion() + "Publication/GetROs?orcid={0}&date={1}", message[1], message[2]));

                    try
                    {
                        string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                        FileLogger.Log("Publicación obtenida." + _configService.GetRutaDirectorioEscritura());
                        if (!Directory.Exists(_configService.GetRutaDirectorioEscritura()))
                        {
                            Directory.CreateDirectory(_configService.GetRutaDirectorioEscritura());
                            FileLogger.Log("Directorio creado: " + _configService.GetRutaDirectorioEscritura());
                        }

                        File.WriteAllText(_configService.GetRutaDirectorioEscritura() + "inv_" + DateTime.Now.ToString().Replace('/', '-').Replace(':', '_') + ".json", info_publication);
                        FileLogger.Log("JSON creado.");
                    }
                    catch (Exception e)
                    {
                        FileLogger.Log(DateTime.Now + " - " + e);
                    }                    
                    return true;
                }
                else
                {
                    Uri url = new Uri(string.Format(_configService.GetUrlPublicacion() + "Publication/GetROs?orcid={0}&date=1500-01-01", message[1]));
                    string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    //List<Publication> objInicial = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
                    //escribirlo en un fichero! 
                    File.WriteAllText(dir_fichero + "inv_" + DateTime.Now + ".json", info_publication);

                    return true;
                }
            }
            else if (message.Count() == 2 & message[0] == "publicación")
            {
                //todo! 
                return true;
            }

            // Si llegamos aqui lo que tenemos es la entrada de la cola de rabbit no esta en el formato correcto! 
            return true;
        }
    }
}

