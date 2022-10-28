using Gnoss.ApiWrapper;
using Hercules.ED.Synchronization.Config;
using Hercules.ED.Synchronization.Models;
using Newtonsoft.Json;

namespace Hercules.ED.Synchronization
{
    class Program
    {
        private static readonly ResourceApi mResourceApi = new($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");
        private static readonly string RUTA_PREFIJOS = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}prefijos.json";

        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Argumentos.</param>
        static void Main()
        {
            Synchro synchro = new();
            synchro.mConfiguracion = new ConfigService();
            synchro.mResourceApi = mResourceApi;
            synchro.mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));
            synchro.ProcessComplete();
        }

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
                using var fileStream = new FileStream($"{pPath}{Path.DirectorySeparatorChar}Synchronization_{CreateTimeStamp()}.log", FileMode.Append);

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
