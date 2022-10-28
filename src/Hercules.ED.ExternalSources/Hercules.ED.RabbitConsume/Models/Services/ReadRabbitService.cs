using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using static Hercules.ED.RabbitConsume.Program;

namespace Gnoss.Web.ReprocessData.Models.Services
{
    public class ReadRabbitService
    {
        private readonly ConfigService _configService;
        private readonly ConnectionFactory connectionFactory;
        private readonly IConnection connection;
        private readonly Dictionary<string, string> headers = new();

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
            _configService.GetLogPath();
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

            EventingBasicConsumer eventingBasicConsumer = new(channel);

            eventingBasicConsumer.Received += (sender, basicDeliveryEventArgs) =>
            {
                try
                {
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
        protected static async Task<string> HttpCall(string url, string method = "GET", Dictionary<string, string> headers = null)
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
        /// Obtiene el ítem de la cola de Rabbit.
        /// </summary>
        /// <param name="pMessage">Ítem en formato string de una lista.</param>
        /// <returns>True o False si se procesa o no.</returns>
        public bool ProcessItem(string pMessage)
        {
            // Listado con los datos.
            List<string> message = JsonConvert.DeserializeObject<List<string>>(pMessage);

            if (message != null && message.Count == 4 && message[0] == "doi" && !string.IsNullOrEmpty(message[1]) && !string.IsNullOrEmpty(message[2]))
            {
                try
                {
                    // Creación de la URL.
                    Uri urlDoi = new(string.Format(_configService.GetUrlPublicacion() + "Publication/GetRoPublication?pDoi={0}&pNombreCompletoAutor={1}", message[1], message[3]));

                    // Obtención de datos con la petición.
                    string resultDoi = HttpCall(urlDoi.ToString(), "GET", headers).Result;

                    // Creación del directorio si no existe.
                    ComprobarDirectorio();

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    string id = message[2].Substring(message[2].LastIndexOf('/') + 1);
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{id}___{fecha.ToString().Replace(' ', '_').Replace('/', '-').Replace(':', '-')}.json", resultDoi);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[2], fecha);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                }
            }
            else if (message != null && message.Count == 4 && message[0] == "investigador" && !string.IsNullOrEmpty(message[1]) && !string.IsNullOrEmpty(message[2]) && !string.IsNullOrEmpty(message[3]))
            {
                try
                {
                    // Creación de la URL.
                    Uri urlInv = new(string.Format(_configService.GetUrlPublicacion() + "Publication/GetROs?orcid={0}&date={1}", message[1], message[2]));

                    // Obtención de datos con la petición.
                    string resultInv = HttpCall(urlInv.ToString(), "GET", headers).Result;

                    // Creación del directorio si no existe.
                    ComprobarDirectorio();

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    string id = message[3].Substring(message[3].LastIndexOf('/') + 1);
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{id}___{fecha.ToString().Replace(' ', '_').Replace('/', '-').Replace(':', '-')}.json", resultInv);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[3], fecha);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                }
            }
            else if (message != null && message.Count == 2 && message[0] == "publicación")
            {
                try
                {
                    // Creación de la URL.
                    Uri urlPub = new(string.Format(_configService.GetUrlPublicacion() + "Publication/GetRoPublication?pDoi={0}", message[1]));

                    // Obtención de datos con la petición.
                    string resultPub = HttpCall(urlPub.ToString(), "GET", headers).Result;

                    // Creación del directorio si no existe.
                    ComprobarDirectorio();

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{message[1]}___{fecha.ToString().Replace('/', '-').Replace(' ', '_').Replace(':', '-')}.json", resultPub);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[1], fecha);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                }
            }
            else if (message != null && message.Count == 2 && message[0] == "zenodo")
            {
                try
                {
                    // Creación de la URL.
                    Uri urlZenodo = new(string.Format(_configService.GetUrlZenodo() + "Zenodo/GetOntologyData?pOrcid={0}", message[1]));

                    // Obtención de datos con la petición.
                    string resultZenodo = HttpCall(urlZenodo.ToString(), "GET", headers).Result;

                    // Creación del directorio si no existe.
                    ComprobarDirectorio();

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{message[0]}___{message[1]}___{fecha.ToString().Replace(' ', '_').Replace('/', '-').Replace(':', '-')}.json", resultZenodo);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[1], fecha);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                }
            }
            else if (message != null && message.Count == 2 && message[0] == "figshare")
            {
                try
                {
                    // Creación de la URL.
                    Uri urlFigShare = new(string.Format(_configService.GetUrlFigShare() + "FigShare/GetROs?pToken={0}", message[1]));

                    // Obtención de datos con la petición.
                    string resultFigShare = HttpCall(urlFigShare.ToString(), "GET", headers).Result;

                    // Creación del directorio si no existe.
                    ComprobarDirectorio();

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{message[0]}___{message[1]}___{fecha.ToString().Replace(' ', '_').Replace('/', '-').Replace(':', '-')}.json", resultFigShare);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[2], fecha);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                }
            }
            else if (message != null && message.Count == 3 && message[0] == "github")
            {
                try
                {
                    // Creación de la URL.
                    Uri urlGitHub = new(string.Format(_configService.GetUrlFigShare() + "github/GetData?pUser={0}&pToken={1}", message[2], message[1]));

                    // Obtención de datos con la petición.
                    string resultGitHub = HttpCall(urlGitHub.ToString(), "GET", headers).Result;

                    // Creación del directorio si no existe.
                    ComprobarDirectorio();

                    // Guardado de la información en formato JSON.
                    DateTime fecha = DateTime.Now;
                    File.WriteAllText($@"{_configService.GetRutaDirectorioEscritura()}{message[0]}___{message[2]}___{fecha.ToString().Replace(' ', '_').Replace('/', '-').Replace(':', '-')}.json", resultGitHub);
                    Hercules.ED.RabbitConsume.Models.Services.DataPerson.ModifyDate(message[2], fecha);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Fallo de conexión al leer la cola. Se vuelve a encolar de nuevo.
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                    return false;
                }
                catch (Exception e)
                {
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.Message}");
                    FileLogger.Log(_configService.GetLogPath(), $@"[ERROR] {DateTime.Now} - {e.StackTrace}");
                }
            }

            return true;
        }
    
        /// <summary>
        /// Comprobar directorio de escritura.
        /// </summary>
        public void ComprobarDirectorio()
        {
            if (!Directory.Exists(_configService.GetRutaDirectorioEscritura()))
            {
                Directory.CreateDirectory(_configService.GetRutaDirectorioEscritura());
            }
        }
    }
}

