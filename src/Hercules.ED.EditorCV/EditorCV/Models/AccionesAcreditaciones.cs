using EditorCV.Models.Acreditacion;
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
    public class AccionesAcreditaciones
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static readonly CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        public void GetAcreditaciones(ConfigService _Configuracion, string comision, string tipo_acreditacion, string categoria_acreditacion, string investigador)
        {
            //Petición al exportador para conseguir el archivo PDF
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(1, 15, 0);
            string urlAcreditaciones = _Configuracion.GetUrlImportador() + "/api/orchestrator/schedules/execute";

            ParameterAcreditacion acreditacion = new ParameterAcreditacion(comision, tipo_acreditacion, categoria_acreditacion, investigador);
            string acreditacionJson = JsonConvert.SerializeObject(acreditacion);

            StringContent content = new StringContent(acreditacionJson, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync($"{urlAcreditaciones}", content).Result;
            response.EnsureSuccessStatusCode();
        }

        public void NotifyAcreditaciones(string url, string idUsuario)
        {
            mResourceApi.Log.Info("Acreditación Usuario: " + idUsuario + ", URL: " + url);

        }

    }
}
