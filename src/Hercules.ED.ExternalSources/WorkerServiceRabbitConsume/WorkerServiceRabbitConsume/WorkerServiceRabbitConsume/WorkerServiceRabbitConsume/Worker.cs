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
            ListenToQueue();
        }

        private void ListenToQueue()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                ConfigService configService = scope.ServiceProvider.GetRequiredService<ConfigService>();
                ReadRabbitService rabbitMQService = scope.ServiceProvider.GetRequiredService<ReadRabbitService>();
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
