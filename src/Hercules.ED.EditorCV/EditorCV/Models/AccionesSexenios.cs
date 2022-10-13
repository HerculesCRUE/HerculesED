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

namespace EditorCV.Models
{
    public class AccionesSexenios
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static readonly CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

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
                string urlSexenios = _Configuracion.GetUrlSGI() + "/api/orchestrator/schedules/execute";

                string investigador = ObtenerIdInvestigador(idInvestigador);
                ParameterSexenio sexenio = new ParameterSexenio(comite, periodo, perfil_tecnologico, subcomite, investigador);
                string sexenioJson = JsonConvert.SerializeObject(sexenio);

                StringContent content = new StringContent(sexenioJson, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync($"{urlSexenios}", content).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {

            }
        }
        public string ObtenerIdInvestigador(string idInvestigador)
        {
            string crisIdentifier = "";
            string select = "SELECT ?crisIdentifier";
            string where = @$"WHERE {{


                            }}";
            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {

            }

            return crisIdentifier;
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
