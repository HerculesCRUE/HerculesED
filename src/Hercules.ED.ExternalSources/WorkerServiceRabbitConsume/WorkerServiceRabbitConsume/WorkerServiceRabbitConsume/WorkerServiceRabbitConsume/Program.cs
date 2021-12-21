using Gnoss.Web.ReprocessData.Models.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorkerServiceRabbitConsume.Middlewares;

namespace WorkerServiceRabbitConsume
{
    public class Program
    {
        private string _LogPath;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(typeof(ConfigService));
                    services.AddScoped(typeof(ReadRabbitService));
                    services.AddScoped(typeof(ErrorHandlingMiddleware));
                    services.AddHostedService<Worker>();
                });
    }
}
