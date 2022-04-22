using EditorCV.Controllers;
using EditorCV.Models.ValidacionProyectos;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.MA.ServicioExterno.Controllers.Utilidades;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EditorCV.Models
{
    public class AccionesEnvioProyecto
    {
        #region --- Constantes   
        private static string RUTA_OAUTH = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config";
        private static ResourceApi mResourceApi = new ResourceApi(RUTA_OAUTH);
        private static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Models/Utils/prefijos.json";
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));
        #endregion

        /// <summary>
        /// Envia un proyecto a validación.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        /// <param name="pIdProyecto">ID del recurso del proyecto.</param>
        /// <param name="pIdPersona">ID del recurso de la persona.</param>
        public void EnvioProyecto(ConfigService pConfig, string pIdProyecto, string pIdPersona)
        {
            NotificacionProyecto proyecto = CrearProyecto(pIdProyecto, pIdPersona);

            try
            {
                RestClient client = new(pConfig.GetUrlEnvioProyecto());
                client.AddDefaultHeader("Authorization", "Bearer " + GetToken(pConfig));
                var request = new RestRequest(Method.POST);
                request.AddJsonBody(proyecto);
                IRestResponse response = client.Execute(request);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Crea el proyecto notificado para enviar a validar.
        /// </summary>
        /// <param name="pIdProyecto">ID del recurso del proyecto.</param>
        /// <param name="pIdPersona">ID </param>
        /// <returns></returns>
        public NotificacionProyecto CrearProyecto(string pIdProyecto, string pIdPersona)
        {
            // Obtención de datos de Proyecto.
            Dictionary<string, string> dicDatosProyecto = GetDatosProyecto(pIdProyecto);

            NotificacionProyecto notificacion = new NotificacionProyecto();
            notificacion.proyectoCVNId = ""; // TODO: ¿Que dato debería de haber?
            notificacion.solicitanteRef = GetSolicitanteRef(pIdPersona); // Obtención del crisIdentifier de la persona solicitante.
            notificacion.titulo = dicDatosProyecto["titulo"];
            notificacion.fechaInicio = dicDatosProyecto["fechaInicio"];
            notificacion.fechaFin = dicDatosProyecto["fechaFin"];

            return notificacion;
        }

        /// <summary>
        /// Obtiene los datos de los Proyectos a enviar a validación.
        /// </summary>
        /// <param name="pIdProyecto">ID del recurso del proyecto.</param>
        /// <returns>Datos necesarios del proyecto.</returns>
        public Dictionary<string, string> GetDatosProyecto(string pIdProyecto)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT ?titulo ?fechaInicio ?fechaFin ");
            where.Append("WHERE { ");
            where.Append("?s a vivo:Project. ");
            where.Append("?s roh:title ?titulo. ");
            where.Append("OPTIONAL{ ?s vivo:start ?fechaInicio. } ");
            where.Append("OPTIONAL{ ?s vivo:end ?fechaFin. } ");
            where.Append($@"FILTER(?s = <{pIdProyecto}>) ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "project");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("titulo"))
                    {
                        dicResultados["titulo"] = UtilidadesAPI.GetValorFilaSparqlObject(fila, "titulo");
                    }
                    if (fila.ContainsKey("fechaInicio"))
                    {
                        string fecha = "2000-01-01";
                        if(!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fechaInicio")))
                        {
                            fecha = ConstruirFecha(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fechaInicio"));
                        }
                        dicResultados["fechaInicio"] = fecha;
                    }
                    if (fila.ContainsKey("fechaFin"))
                    {
                        string fecha = "3000-01-01";
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fechaFin")))
                        {
                            fecha = ConstruirFecha(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fechaFin"));
                        }
                        dicResultados["fechaFin"] = fecha;
                    }
                }
            }

            return dicResultados;
        }

        /// <summary>
        /// Obtiene el ID de la persona que va a solicitar la autorización.
        /// </summary>
        /// <param name="pIdPersona">ID del recurso de la persona.</param>
        /// <returns>Identificador de la persona.</returns>
        public string GetSolicitanteRef(string pIdPersona)
        {
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?crisIdentifier ");
            where.Append("WHERE { ");
            where.Append("?s a foaf:Person. ");
            where.Append("?s roh:crisIdentifier ?crisIdentifier. ");
            where.Append($@"FILTER(?s = <{pIdPersona}>) ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "person");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return UtilidadesAPI.GetValorFilaSparqlObject(fila, "crisIdentifier");
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Formatea la fecha.
        /// </summary>
        /// <param name="pFechaSparql">Fecha en formato SPARQL.</param>
        /// <returns>Fecha formateada.</returns>
        public string ConstruirFecha(string pFechaSparql)
        {
            string dia = pFechaSparql.Substring(6, 2);
            string mes = pFechaSparql.Substring(4, 2);
            string anyo = pFechaSparql.Substring(0, 4);
            return $@"{anyo}-{mes}-{dia}";
        }

        /// <summary>
        /// Obtención del token.
        /// </summary>
        /// <returns></returns>
        private string GetToken(ConfigService pConfig)
        {
            Uri url = new Uri(pConfig.GetUrlToken());
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("username", pConfig.GetUsernameESB()),
                new KeyValuePair<string, string>("password", pConfig.GetPasswordESB()),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            string result = httpCall(url.ToString(), "POST", content).Result;
            var json = JObject.Parse(result);

            return json["access_token"].ToString();
        }

        /// <summary>
        /// Llamada para la obtención del token.
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pMethod"></param>
        /// <param name="pBody"></param>
        /// <returns></returns>
        protected async Task<string> httpCall(string pUrl, string pMethod, FormUrlEncodedContent pBody)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(pMethod), pUrl))
                {
                    request.Content = pBody;

                    int intentos = 3;
                    while (true)
                    {
                        try
                        {
                            response = await httpClient.SendAsync(request);
                            break;
                        }
                        catch
                        {
                            intentos--;
                            if (intentos == 0)
                            {
                                throw;
                            }
                            else
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
            }
            if (response.Content != null)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
