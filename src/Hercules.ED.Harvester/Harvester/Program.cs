using Gnoss.ApiWrapper;
using System;

namespace Harvester
{
    public class Program
    {
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\OAuthV3.config");
        private static CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\OAuthV3.config");

        static void Main(string[] args)
        {
            Loader loader = new Loader(mResourceApi);
            loader.LoadMainEntities();
        }
    }
}
