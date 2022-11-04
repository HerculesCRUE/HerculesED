using Gnoss.Web.ReprocessData.Models.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Hercules.ED.RabbitConsume
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="serviceScopeFactory"></param>
        public Worker(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Tarea asincrona.
        /// </summary>
        /// <param name="stoppingToken">Cancellation Token.</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ListenToQueue();
        }

        /// <summary>
        /// Lectura de una cola Rabbit.
        /// </summary>
        private void ListenToQueue()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                ConfigService configService = scope.ServiceProvider.GetRequiredService<ConfigService>();
                ReadRabbitService rabbitMQService = scope.ServiceProvider.GetRequiredService<ReadRabbitService>();
                rabbitMQService.ListenToQueue(new ReadRabbitService.ReceivedDelegate(rabbitMQService.ProcessItem), new ReadRabbitService.ShutDownDelegate(OnShutDown), configService.GetFuentesExternasQueueRabbit());
            }
        }

        /// <summary>
        /// Permite lanzar la escucha después de leer. 
        /// Contiene un sleep de 30 segundos.
        /// </summary>
        private void OnShutDown()
        {
            Thread.Sleep(30000);
            ListenToQueue();
        }
    }
}
