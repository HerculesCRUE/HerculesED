using Gnoss.ApiWrapper;
using System;
using Hercules.ED.ResearcherObjectLoad.Models;
using Hercules.ED.ResearcherObjectLoad.Config;
using System.IO;
using System.Threading;

namespace Hercules.ED.ResearcherObjectLoad
{
    class Program
    {
        private static string RUTA_OAUTH = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config";
        private static ResourceApi mResourceApi = null;

        private static ResourceApi ResourceApi
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
                        Console.WriteLine($"Contenido OAuth: {File.ReadAllText(RUTA_OAUTH)}");
                        Thread.Sleep(10000);
                    }
                }
                return mResourceApi;
            }
        }

        static void Main(string[] args)
        {
            Carga.mResourceApi = ResourceApi;
            Carga.configuracion = new ConfigService();
            Carga.CargaMain();
        }
    }
}
