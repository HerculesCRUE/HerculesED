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
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_processRabbitReady)
                {
                    var scope = _serviceScopeFactory.CreateScope();
                    ConfigService configService = scope.ServiceProvider.GetRequiredService<ConfigService>();
                    ReadRabbitService rabbitMQService = scope.ServiceProvider.GetRequiredService<ReadRabbitService>();
                    //ai.Add("investigador");
                    //ai.Add("0000-0003-0955-976X")
                    //ai.Add(null);
                    //rabbitMQService.ProcessItem(ai);
                    rabbitMQService.ListenToQueue(new ReadRabbitService.ReceivedDelegate(rabbitMQService.ProcessItem), new ReadRabbitService.ShutDownDelegate(OnShutDown), configService.GetQueueRabbit());
                    _processRabbitReady = true;
                }
            }
        }

        private void OnShutDown()
        {
            _processRabbitReady = false;
        }
    }
}
