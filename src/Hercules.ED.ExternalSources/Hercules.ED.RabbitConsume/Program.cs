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

        /// <summary>
        /// Clase FileLogger.
        /// </summary>
        public static class FileLogger
        {
            /// <summary>
            /// Sobreescribe el método Log para pintar el mensaje de error en un fichero.
            /// </summary>
            /// <param name="pPath">Ruta.</param>
            /// <param name="pMesssage">Mensaje de error a mostrar.</param>
            public static void Log(string pPath, string pMesssage)
            {
                // Si no existe el directorio, se crea.
                if (!Directory.Exists(pPath))
                {
                    Directory.CreateDirectory(pPath);
                }

                // Modo del fichero.
                using var fileStream = new FileStream($"{pPath}{Path.DirectorySeparatorChar}RabbitConsume_{CreateTimeStamp()}.log", FileMode.Append);

                // Writer.
                using var writter = new StreamWriter(fileStream);
                writter.WriteLine(pMesssage + Environment.NewLine);
            }

            /// <summary>
            /// Crear formato de año.
            /// </summary>
            /// <returns></returns>
            private static string CreateTimeStamp()
            {
                DateTime time = DateTime.Now;

                string month = time.Month.ToString();
                if (month.Length == 1)
                {
                    month = $"0{month}";
                }

                string day = time.Day.ToString();
                if (day.Length == 1)
                {
                    day = $"0{day}";
                }

                return $"{time.Year}-{month}-{day}";
            }
        }
    }
}
