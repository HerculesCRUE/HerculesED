using EditorCV.Models.Sexenios;
using Gnoss.ApiWrapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace EditorCV.Models
{
    public class AccionesSexenios
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static readonly CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config"); 
        
        public void GetSexenios(ConfigService _Configuracion, string comite, string periodo, string perfil_tecnologico, string subcomite, string idInvestigador)
        {
            try
            {
                //Petición al exportador para conseguir el archivo PDF
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(1, 15, 0);
                string urlSexenios = _Configuracion.GetUrlImportador() + "/api/orchestrator/schedules/execute";

                ParameterSexenio sexenio = new ParameterSexenio(comite, periodo, perfil_tecnologico, subcomite, idInvestigador);
                string sexenioJson = JsonConvert.SerializeObject(sexenio);

                StringContent content = new StringContent(sexenioJson, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync($"{urlSexenios}", content).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {

            }
        }

        public void NotifySexenios(string url, string idUsuario)
        {
            mResourceApi.Log.Info("Usuario: " + idUsuario + ", URL: " + url);
        }
    }
}
