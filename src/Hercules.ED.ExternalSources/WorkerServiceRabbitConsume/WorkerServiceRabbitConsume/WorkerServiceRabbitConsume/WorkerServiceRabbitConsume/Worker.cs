using Gnoss.Web.ReprocessData.Models.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerServiceRabbitConsume
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private bool _processRabbitReady = false;
        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (!_processRabbitReady)
            //    {

            //    }
            //    Thread.Sleep(5000);
            //}
            ListenToQueue();
        }

        private void ListenToQueue()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                ConfigService configService = scope.ServiceProvider.GetRequiredService<ConfigService>();
                ReadRabbitService rabbitMQService = scope.ServiceProvider.GetRequiredService<ReadRabbitService>();

                // Prueba 
                //List<string> listaDatos = new List<string>() { "investigador" , "0000-0002-5525-1259", "2021-12-01"};
                //rabbitMQService.PublishMessage(listaDatos, configService.GetQueueRabbit());

                rabbitMQService.ListenToQueue(new ReadRabbitService.ReceivedDelegate(rabbitMQService.ProcessItem), new ReadRabbitService.ShutDownDelegate(OnShutDown), configService.GetQueueRabbit());
                _processRabbitReady = true;
            }
        }

        private void OnShutDown()
        {
            _processRabbitReady = false;
            Thread.Sleep(5000);
            ListenToQueue();
        }
    }
}
