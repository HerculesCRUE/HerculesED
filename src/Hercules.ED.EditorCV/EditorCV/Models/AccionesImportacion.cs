using Gnoss.ApiWrapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EditorCV.Models
{
    public class AccionesImportacion
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        public HttpResponseMessage ImportarCV(ConfigService _Configuracion, string pCVID, IFormFile File)
        {
            try
            {
                //Petición al exportador
                MultipartFormDataContent multipartFormData = new MultipartFormDataContent();
                multipartFormData.Add(new StringContent(pCVID), "pCVID");

                var ms = new MemoryStream();
                File.CopyTo(ms);
                byte[] filebytes = ms.ToArray();
                multipartFormData.Add(new ByteArrayContent(filebytes), "File", File.FileName);

                string urlPreImportador = "";
                urlPreImportador = _Configuracion.GetUrlImportador() + "/Preimportar";

                //Petición al exportador para conseguir el archivo PDF
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(1, 15, 0);

                HttpResponseMessage response = client.PostAsync($"{urlPreImportador}", multipartFormData).Result;
                response.EnsureSuccessStatusCode();
                if (response.StatusCode != System.Net.HttpStatusCode.OK) 
                {
                    throw new Exception(response.StatusCode.ToString() + " " + response.Content);
                }

                return response;
            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
                return new HttpResponseMessage();
            }
        }
    }
}
