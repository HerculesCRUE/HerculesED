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
                    services.AddHostedService<Worker>();
                });

        public static class FileLogger
        {
            private const string FilePath = "/app/logs/log.txt"; // --- TODO
            public static void Log(string messsage)
            {
                using var fileStream = new FileStream(FilePath, FileMode.Append);
                using var writter = new StreamWriter(fileStream);
                writter.WriteLine(messsage);
            }
        }
    }
}
