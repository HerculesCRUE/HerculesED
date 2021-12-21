using Gnoss.ApiWrapper;
using System;
using Hercules.ED.ResearcherObjectLoad.Models;
using Microsoft.Extensions.Configuration;
using Hercules.ED.ResearcherObjectLoad.Config;

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
    }
}
