using Gnoss.ApiWrapper;
using Hercules.ED.Synchronization.Config;
using Hercules.ED.Synchronization.Models;
using Newtonsoft.Json;

namespace Hercules.ED.Synchronization
{
    class Program
    {
        private static ResourceApi mResourceApi = new ($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");
        private static string RUTA_PREFIJOS = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}prefijos.json";

        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Argumentos.</param>
        static void Main(string[] args)
        {
            Synchro synchro = new ();
            synchro.mResourceApi = mResourceApi;
            synchro.mConfiguracion = new ConfigService();
            synchro.mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));

            FileLogger.FilePath = synchro.mConfiguracion.GetLogPath();
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
            /// <param name="pMesssage">Mensaje de error a mostrar.</param>
            public static void Log(string pMesssage)
            {
                // Si no existe el directorio, se crea.
                if (!Directory.Exists(FilePath))
                {
                    Directory.CreateDirectory(FilePath);
                }

                // Modo del fichero.
                using var fileStream = new FileStream($"{FilePath}{Path.DirectorySeparatorChar}Synchronization_{CreateTimeStamp()}.log", FileMode.Append);

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
