using Gnoss.ApiWrapper;
using System;
using Hercules.ED.ResearcherObjectLoad.Models;
using Microsoft.Extensions.Configuration;
using Hercules.ED.ResearcherObjectLoad.Config;
using System.IO;
using System.Threading;

namespace Hercules.ED.ResearcherObjectLoad
{
    class Program
    {
        private static string RUTA_OAUTH = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config";
        private static ResourceApi mResourceApi = null;
        private static CommunityApi mCommunityApi = null;

        private static ResourceApi resourceApi
        {
            get
            {
                while (mResourceApi == null)
                {
                    try
                    {
                        mResourceApi = new ResourceApi(RUTA_OAUTH);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("No se ha podido iniciar ResourceApi");
                        Console.WriteLine($"Contenido OAuth: {System.IO.File.ReadAllText(RUTA_OAUTH)}");
                        Thread.Sleep(10000);
                    }
                }
                return mResourceApi;
            }
        }

        private static CommunityApi communityApi
        {
            get
            {
                while (mCommunityApi == null)
                {
                    try
                    {
                        mCommunityApi = new CommunityApi(RUTA_OAUTH);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("No se ha podido iniciar CommunityApi");
                        Console.WriteLine($"Contenido OAuth: {System.IO.File.ReadAllText(RUTA_OAUTH)}");
                        Thread.Sleep(10000);
                    }
                }
                return mCommunityApi;
            }
        }

        static void Main(string[] args)
        {
            Carga.mResourceApi = resourceApi;
            Carga.mCommunityApi = communityApi;
            Carga.configuracion = new ConfigService();
            Carga.CargaMain();
        }

        /// <summary>
        /// Clase FileLogger.
        /// </summary>
        public static class FileLogger
        {
            private const string FilePath = $@"/app/logs/log.txt"; // --- TODO: Sacarlo a archivo de configuración.

            /// <summary>
            /// Sobreescribe el método Log para pintar el mensaje de error en un fichero.
            /// </summary>
            /// <param name="messsage"></param>
            public static void Log(string messsage)
            {
                using var fileStream = new FileStream(FilePath, FileMode.Append);
                using var writter = new StreamWriter(fileStream);
                writter.WriteLine(messsage);
            }
        }
    }
}
