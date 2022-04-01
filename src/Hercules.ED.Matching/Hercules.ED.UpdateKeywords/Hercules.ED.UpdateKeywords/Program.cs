using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using System;
using System.Collections.Generic;

namespace Hercules.ED.UpdateKeywords
{
    public class Program
    {
        private static string RUTA_OAUTH = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config";
        private static ResourceApi mResourceApi = new ResourceApi(RUTA_OAUTH);
        private static CommunityApi mCommunityApi = new CommunityApi(RUTA_OAUTH);
       

        static void Main(string[] args)
        {
            UtilKeywords utilKeywords = new UtilKeywords(mResourceApi, mCommunityApi);
            List<string> listaIds = utilKeywords.GetDocument();
            Dictionary<string, string> dicResultados = utilKeywords.SelectDataMesh("anti-bacterial");
        }
    }
}
