using EditorCV.Models.Sexenios;
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

namespace EditorCV.Models
{
    public class AccionesSexenios
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        /// <summary>
        /// Envia una petición para conseguir los Sexenios del usuario
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="comite"></param>
        /// <param name="periodo"></param>
        /// <param name="perfil_tecnologico"></param>
        /// <param name="subcomite"></param>
        /// <param name="idInvestigador"></param>
        public void GetSexenios(ConfigService _Configuracion, string comite, string periodo, [Optional] string perfil_tecnologico, [Optional] string subcomite, string idInvestigador)
        {
            try
            {
                //Petición al exportador para conseguir el archivo PDF
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(1, 15, 0);
                string urlSexenios = _Configuracion.GetUrlSGISexeniosAcreditaciones() + "/api/orchestrator/schedules/execute";

                string cvOf = Utils.UtilityCV.GetCVFromUser(idInvestigador);
                string person = Utils.UtilityCV.GetPersonFromCV(cvOf);

                //Consigo el identificador del investigador.
                string investigador = Utils.UtilityCV.GetIdInvestigador(cvOf);

                //Si no consigo ningún identificador del usuario.
                if (string.IsNullOrEmpty(investigador))
                {
                    Utils.UtilityCV.EnvioNotificacion("NOTIFICACION_SEXENIOS_ERROR", person, "recuperarSexenios");
                    throw new Exception("El usuario no se ha encontrado, id: " + idInvestigador);
                }

                ParameterSexenio sexenio = new ParameterSexenio(comite, periodo, perfil_tecnologico, subcomite, investigador);
                string sexenioJson = JsonConvert.SerializeObject(sexenio);

                StringContent content = new StringContent(sexenioJson, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync($"{urlSexenios}", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    Utils.UtilityCV.EnvioNotificacion("NOTIFICACION_SEXENIOS", person, "recuperarSexenios");
                }
                else
                {
                    Utils.UtilityCV.EnvioNotificacion("NOTIFICACION_SEXENIOS_ERROR", person, "recuperarSexenios");
                }

            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Recibe la petición de respuesta
        /// </summary>
        /// <param name="url">URL del documento</param>
        /// <param name="idUsuario">Identificador del usuario</param>
        public void NotifySexenios(string url, string idUsuario)
        {
            mResourceApi.Log.Info("Usuario: " + idUsuario + ", URL: " + url);
        }

    }
}
