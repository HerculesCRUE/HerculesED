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
using static Hercules.ED.RabbitConsume.Program;

namespace Gnoss.Web.ReprocessData.Models.Services
{
    public class ReadRabbitService
    {
        private ConfigService _configService;
        private readonly ConnectionFactory connectionFactory;
        private readonly IConnection connection;
        private Dictionary<string, string> headers = new Dictionary<string, string>();

        /// <summary>
        /// ReceivedDelegate.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public delegate bool ReceivedDelegate(string s);

        /// <summary>
        /// ShutDownDelegate.
        /// </summary>
        public delegate void ShutDownDelegate();

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="configService"></param>
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
        /// Encola un objeto en Rabbit.
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

                    channel.BasicPublish(exchange: string.Empty,
                        routingKey: queue,
                        basicProperties: null,
                        body: body
                    );
                }
            }
        }

        /// <summary>
        /// ListenToQueue.
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

        /// <summary>
        /// A Http calls function.
        /// </summary>
        /// <param name="url">The http call URL.</param>
        /// <param name="method">Crud method for the call.</param>
        /// <returns></returns>
        protected async Task<string> httpCall(string url, string method = "GET", Dictionary<string, string> headers = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromHours(24);
                using (var request = new HttpRequestMessage(new HttpMethod(method), url))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");

                    if (headers != null && headers.Count > 0)
                    {
                        foreach (var item in headers)
                        {
                            request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    response = await httpClient.SendAsync(request);
                }
            }

            if (response.Content != null)
            {
                return await response.Content.ReadAsStringAsync();
            }

            else
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Permite mandar a procesar los datos a una cola Rabbit.
        /// {"investigador"; [ORCID]; "2021-11-01"} -> Obtiene todos los datos relacionados con ese autor desde una fecha indicada. 
        /// {"publicación"; [DOI]} -> Actualización de citas de un documento en concreto en las diversas fuentes que puedas encontrarlo.
        /// </summary>
        /// <param name="pMessage">Datos en formato string de un json.</param>
        /// <returns></returns>
        public bool ProcessItem(string pMessage)
        {
            // Listado con los datos.
            List<string> message = JsonConvert.DeserializeObject<List<string>>(pMessage);

            if (message != null && message.Count() == 4 && message[0] == "investigador" && !string.IsNullOrEmpty(message[1]) && !string.IsNullOrEmpty(message[2]) && !string.IsNullOrEmpty(message[3]))
            {
                try
                {
                    // Creación de la URL.
                    Uri url = new Uri(string.Format(_configService.GetUrlPublicacion() + "Publication/GetROs?orcid={0}&date={1}", message[1], message[2]));
                    FileLogger.Log($@"Haciendo petición a {url}");

                    // Obtención de datos con la petición.
                    string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    FileLogger.Log($@"{DateTime.Now} - Publicación obtenida.");

                    // Creación del directorio si no existe.
                    if (!Directory.Exists(_configService.GetRutaDirectorioEscritura()))
                    {
                        Directory.CreateDirectory(_configService.GetRutaDirectorioEscritura());
                        FileLogger.Log($@"{DateTime.Now} - Directorio creado: {_configService.GetRutaDirectorioEscritura()}");
                    }

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    string id = message[3].Substring(message[3].LastIndexOf('/') + 1);
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{id}___{fecha.ToString().Replace(' ', '_').Replace('/', '-').Replace(':', '-')}.json", info_publication);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[3], fecha);
                    FileLogger.Log($@"{fecha} - fichero JSON creado.");
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                }
            }
            else if (message.Count() == 2 & message[0] == "publicación")
            {
                try
                {
                    // Creación de la URL.
                    Uri url = new Uri(string.Format(_configService.GetUrlPublicacion() + "Publication/GetRoPublication?pDoi={0}", message[1]));

                    // Obtención de datos con la petición.
                    string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    FileLogger.Log($@"{DateTime.Now} - Publicación obtenida.");

                    // Creación del directorio si no existe.
                    if (!Directory.Exists(_configService.GetRutaDirectorioEscritura()))
                    {
                        Directory.CreateDirectory(_configService.GetRutaDirectorioEscritura());
                        FileLogger.Log($@"{DateTime.Now} - Directorio creado: {_configService.GetRutaDirectorioEscritura()}");
                    }

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{message[1]}___{fecha.ToString().Replace('/', '-').Replace(' ', '_').Replace(':', '-')}.json", info_publication);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[1], fecha);
                    FileLogger.Log($@"{fecha} - fichero JSON creado.");
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                }
            }
            else if (message.Count() == 2 & message[0] == "zenodo")
            {
                try
                {
                    // Creación de la URL.
                    Uri url = new Uri(string.Format(_configService.GetUrlZenodo() + "Zenodo/GetOntologyData?pOrcid={0}", message[1]));

                    // Obtención de datos con la petición.
                    string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    FileLogger.Log($@"{DateTime.Now} - RO Zenodo obtenido.");

                    // Creación del directorio si no existe.
                    if (!Directory.Exists(_configService.GetRutaDirectorioEscritura()))
                    {
                        Directory.CreateDirectory(_configService.GetRutaDirectorioEscritura());
                        FileLogger.Log($@"{DateTime.Now} - Directorio creado: {_configService.GetRutaDirectorioEscritura()}");
                    }

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{message[0]}___{message[1]}___{fecha.ToString().Replace(' ', '_').Replace('/', '-').Replace(':', '-')}.json", info_publication);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[1], fecha);
                    FileLogger.Log($@"{fecha} - fichero JSON creado.");
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                }
            }
            else if (message.Count() == 2 & message[0] == "figshare")
            {
                try
                {
                    // Creación de la URL.
                    Uri url = new Uri(string.Format(_configService.GetUrlFigShare() + "FigShare/GetROs?pToken={0}", message[1]));

                    // Obtención de datos con la petición.
                    string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    FileLogger.Log($@"{DateTime.Now} - RO FigShare obtenido.");

                    // Creación del directorio si no existe.
                    if (!Directory.Exists(_configService.GetRutaDirectorioEscritura()))
                    {
                        Directory.CreateDirectory(_configService.GetRutaDirectorioEscritura());
                        FileLogger.Log($@"{DateTime.Now} - Directorio creado: {_configService.GetRutaDirectorioEscritura()}");
                    }

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{message[0]}___{message[1]}___{fecha.ToString().Replace(' ', '_').Replace('/', '-').Replace(':', '-')}.json", info_publication);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[2], fecha);
                    FileLogger.Log($@"{fecha} - fichero JSON creado.");
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                }
            }
            else if (message.Count() == 3 & message[0] == "github")
            {
                try
                {
                    // Creación de la URL.
                    Uri url = new Uri(string.Format(_configService.GetUrlFigShare() + "github/GetData?pUser={0}&pToken={1}", message[2], message[1]));

                    // Obtención de datos con la petición.
                    string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                    FileLogger.Log($@"{DateTime.Now} - RO GitHub obtenido.");

                    // Creación del directorio si no existe.
                    if (!Directory.Exists(_configService.GetRutaDirectorioEscritura()))
                    {
                        Directory.CreateDirectory(_configService.GetRutaDirectorioEscritura());
                        FileLogger.Log($@"{DateTime.Now} - Directorio creado: {_configService.GetRutaDirectorioEscritura()}");
                    }

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{message[0]}___{message[2]}___{fecha.ToString().Replace(' ', '_').Replace('/', '-').Replace(':', '-')}.json", info_publication);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[2], fecha);
                    FileLogger.Log($@"{fecha} - fichero JSON creado.");
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log($@"{DateTime.Now} - {e}");
                }
            }

            return true;
        }
    }
}

