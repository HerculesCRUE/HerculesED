using EditorCV.Models.Acreditacion;
using Gnoss.ApiWrapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.ResearcherObjectLoad.Models.NotificationOntology;
using Gnoss.ApiWrapper.Model;
using System.IO;

namespace EditorCV.Models
{
    public class AccionesAcreditaciones
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");

        /// <summary>
        /// Envia una petición para conseguir las Acreditaciones del usuario
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="comision"></param>
        /// <param name="tipo_acreditacion"></param>
        /// <param name="categoria_acreditacion"></param>
        /// <param name="investigador"></param>
        public void GetAcreditaciones(ConfigService _Configuracion, string comision, string tipo_acreditacion, [Optional] string categoria_acreditacion, string idInvestigador)
        {
            //Petición al exportador para conseguir el archivo PDF
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(1, 15, 0);
            string urlAcreditaciones = _Configuracion.GetUrlSGISexeniosAcreditaciones() + "/api/orchestrator/schedules/execute";

            string cvOf = Utils.UtilityCV.GetCVFromUser(idInvestigador);
            string person = Utils.UtilityCV.GetPersonFromCV(cvOf);

            //Consigo el identificador del investigador.
            string investigador = Utils.UtilityCV.GetIdInvestigador(cvOf);

            //Si no consigo ningún identificador del usuario.
            if (string.IsNullOrEmpty(investigador))
            {
                throw new Exception("El usuario no se ha encontrado, id: " + idInvestigador);
            }

            ParameterAcreditacion acreditacion = new ParameterAcreditacion(comision, tipo_acreditacion, categoria_acreditacion, investigador);
            string acreditacionJson = JsonConvert.SerializeObject(acreditacion);

            StringContent content = new StringContent(acreditacionJson, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync($"{urlAcreditaciones}", content).Result; 

            if (response.IsSuccessStatusCode)
            {
                Utils.UtilityCV.EnvioNotificacion("NOTIFICACION_ACREDITACION", person, "recuperarAcreditacion");
            }
            else
            {
                Utils.UtilityCV.EnvioNotificacion("NOTIFICACION_ACREDITACION_ERROR", person, "recuperarAcreditacion");
            }
        }

        /// <summary>
        /// Recibe la petición de respuesta
        /// </summary>
        /// <param name="url">URL del documento</param>
        /// <param name="idUsuario">Identificador del usuario</param>
        public void NotifyAcreditaciones(string url, string idUsuario)
        {
            string person = Utils.UtilityCV.GetInvestigadorByID(idUsuario);
            Utils.UtilityCV.EnvioNotificacion(url, person, "notifyAcreditaciones");

            mResourceApi.Log.Info("Acreditación Usuario: " + idUsuario + ", URL: " + url);

        }

    }
}
