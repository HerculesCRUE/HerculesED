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

        public string dir_fichero = @"";
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

        //{"investigador";[ORCID];"1/11/2021"} -> obtener todo lo de un autor desde la fecha inducada. 
        //{"publicación"; [DOI]} ->  proceso de actualizacion de citas de un documento en concreto en las diversas fuentes que puedas encontrarlo. 

        public bool ProcessItem(List<string> message)
        {
            if (message.Count() == 3 & message[0] == "investigador")
            {
                if (message[2] != null)
                {
                    Uri url = new Uri(string.Format("http://localhost:4999/Publication/GetROs?orcid={0}&date={1}", message[1], message[2]));
                    Console.Write(url);
                    string info_publication = httpCall(url.ToString(), "GET",headers).Result;
                    //List<Publication> objInicial = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
                    
                    File.WriteAllText(dir_fichero, info_publication);
                    //escribirlo en un fichero! 
                    return true;
                }
                else
                {
                    Uri url = new Uri(string.Format("http://localhost:4999/Publication/GetROs?orcid={0}&date=1500-01-01", message[1]));
                    string info_publication = httpCall(url.ToString(), "GET",headers).Result;
                    //List<Publication> objInicial = JsonConvert.DeserializeObject<List<Publication>>(info_publication);
                    //escribirlo en un fichero! 
                    File.WriteAllText(dir_fichero, info_publication);

                    return true;
                }
            }
            else if (message.Count() == 2 & message[0] == "publicación")
            {
                //todo! 
                return true;
            }
            // Si llegamos aqui lo que tenemos es la entrada de la cola de rabbit no esta en el formato correcto! 
            return false;

        }
    
    }
}

