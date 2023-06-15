using Gnoss.Web.ReprocessData.Models.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace Hercules.ED.RabbitConsume
{
    public class Program
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        protected Program() { }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// CreateHostBuilder.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(typeof(ConfigService));
                    services.AddScoped(typeof(ReadRabbitService));
                    services.AddHostedService<Worker>();
                });

    }
}
