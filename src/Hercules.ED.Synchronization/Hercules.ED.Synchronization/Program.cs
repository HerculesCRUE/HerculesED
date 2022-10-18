using Gnoss.ApiWrapper;
using Hercules.ED.Synchronization.Config;
using Hercules.ED.Synchronization.Models;

namespace Hercules.ED.Synchronization
{
    class Program
    {
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Argumentos.</param>
        static void Main(string[] args)
        {
            Synchro synchro = new Synchro();
            synchro.mResourceApi = mResourceApi;
            synchro.configuracion = new ConfigService();
            FileLogger.FilePath = synchro.configuracion.GetLogPath();
            synchro.ProcessComplete();
        }

        /// <summary>
        /// Clase FileLogger.
        /// </summary>
        public static class FileLogger
        {
            public static string FilePath;

            /// <summary>
            /// Sobreescribe el método Log para pintar el mensaje de error en un fichero.
            /// </summary>
            /// <param name="messsage"></param>
            public static void Log(string messsage)
            {
                if (!Directory.Exists(FilePath))
                {
                    Directory.CreateDirectory(FilePath);
                }
                using var fileStream = new FileStream($"{FilePath}/log_{CreateTimeStamp()}.txt", FileMode.Append);
                using var writter = new StreamWriter(fileStream);
                writter.WriteLine(messsage);
            }

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
                string timeStamp = $"{time.Year.ToString()}{month}{day}";
                return timeStamp;
            }
        }
    }
}
