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

namespace EditorCV.Models
{
    public class AccionesAcreditaciones
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static readonly CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        /// <summary>
        /// Envia una petición para conseguir las Acreditaciones del usuario
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="comision"></param>
        /// <param name="tipo_acreditacion"></param>
        /// <param name="categoria_acreditacion"></param>
        /// <param name="investigador"></param>
        public void GetAcreditaciones(ConfigService _Configuracion, string comision, string tipo_acreditacion, [Optional] string categoria_acreditacion, string investigador)
        {
            //Petición al exportador para conseguir el archivo PDF
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(1, 15, 0);
            string urlAcreditaciones = _Configuracion.GetUrlSGI() + "/api/orchestrator/schedules/execute";

            string[] personaCrisIdentifier = ObtenerPersonaYCrisIdentifier(investigador);
            string persona = personaCrisIdentifier[0];
            string crisIdentifier = personaCrisIdentifier[1];

            ParameterAcreditacion acreditacion = new ParameterAcreditacion(comision, tipo_acreditacion, categoria_acreditacion, crisIdentifier);
            string acreditacionJson = JsonConvert.SerializeObject(acreditacion);

            StringContent content = new StringContent(acreditacionJson, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync($"{urlAcreditaciones}", content).Result;
            if (response.IsSuccessStatusCode)
            {
                EnvioNotificacion("NOTIFICACION_ACREDITACIONES", persona);
            }
            else
            {
                EnvioNotificacion("NOTIFICACION_ACREDITACIONES_ERROR", persona);
            }
            response.EnsureSuccessStatusCode();
        }
        /// <summary>
        /// Método que obtiene un recurso Person y el crisIdentifier a partir de una id de una persona
        /// </summary>
        /// <param name="idInvestigador"></param>
        /// <returns></returns>
        public string[] ObtenerPersonaYCrisIdentifier(string idInvestigador)
        {
            string crisIdentifier = "";
            string persona = "";
            string select = "SELECT ?persona ?crisIdentifier";
            string where = @$"WHERE {{
                ?persona a <http://xmlns.com/foaf/0.1/Person>.
                ?persona <http://w3id.org/roh/gnossUser> <http://gnoss/{idInvestigador.ToUpper()}>.
                ?persona <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
            }}";
            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                if (resultadoQuery.results.bindings.First().ContainsKey("crisIdentifier"))
                {
                    crisIdentifier = resultadoQuery.results.bindings.First()["crisIdentifier"].value;
                }
                if (resultadoQuery.results.bindings.First().ContainsKey("persona"))
                {
                    persona = resultadoQuery.results.bindings.First()["persona"].value;
                }
            }

            return new string[] { persona, crisIdentifier };
        }

        /// <summary>
        /// Método que envía una notificación de acreditaciones específica
        /// </summary>
        /// <param name="title"></param>
        /// <param name="owner"></param>
        public void EnvioNotificacion(string title, string owner)
        {
            Notification notificacion = new Notification();
            notificacion.Roh_text = title;
            notificacion.IdRoh_owner = owner;
            notificacion.Dct_issued = DateTime.UtcNow;
            notificacion.Roh_type = "recuperarAcreditaciones";
            mResourceApi.ChangeOntoly("notification");
            ComplexOntologyResource recursoCargar = notificacion.ToGnossApiResource(mResourceApi);
            int numIntentos = 0;
            while (!recursoCargar.Uploaded)
            {
                numIntentos++;

                if (numIntentos > 5)
                {
                    break;
                }
                mResourceApi.LoadComplexSemanticResource(recursoCargar);
            }
        }

        /// <summary>
        /// Recibe la petición de respuesta
        /// </summary>
        /// <param name="url">URL del documento</param>
        /// <param name="idUsuario">Identificador del usuario</param>
        public void NotifyAcreditaciones(string url, string idUsuario)
        {
            mResourceApi.Log.Info("Acreditación Usuario: " + idUsuario + ", URL: " + url);

        }

    }
}
