using Gnoss.ApiWrapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EditorCV.Models
{
    public class AccionesImportacion
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        public void ImportarCV(string pCVID, IFormFile File) { 
            
        }
    }
}
