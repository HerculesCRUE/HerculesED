using Gnoss.ApiWrapper;
using System;
using Hercules.ED.ResearcherObjectLoad.Models;
using Microsoft.Extensions.Configuration;
using Hercules.ED.ResearcherObjectLoad.Config;
using System.IO;

namespace Hercules.ED.ResearcherObjectLoad
{
    class Program
    {
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configOAuth\OAuthV3.config");
        private static CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configOAuth\OAuthV3.config");
       
        static void Main(string[] args)
        {
            Carga.mResourceApi = mResourceApi;
            Carga.mCommunityApi = mCommunityApi;
            Carga.configuracion = new ConfigService();
            Carga.CargaMain();
        }

        /// <summary>
        /// Clase FileLogger.
        /// </summary>
        public static class FileLogger
        {
            private const string FilePath = "/app/logs/log.txt"; // --- TODO: Sacarlo a archivo de configuración.

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
