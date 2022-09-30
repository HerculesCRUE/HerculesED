using EditorCV.Controllers;
using EditorCV.Models.ValidacionProyectos;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Hercules.MA.ServicioExterno.Controllers.Utilidades;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;
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
        /// <param name="pIdRecurso">ID del recurso que apunta al proyecto.</param>
        /// <param name="pIdPersona">ID del recurso de la persona.</param>
        /// <param name="pIdAutorizacion">ID del recurso de la autorización.</param>
        public void EnvioProyecto(ConfigService pConfig, string pIdRecurso, string pIdPersona, string pIdAutorizacion)
        {
            string pIdProyecto = "";
            string selectProyecto = "select distinct ?proyecto";
            string whereProyecto = $@"where{{
    <{pIdRecurso}> <http://vivoweb.org/ontology/core#relatedBy> ?proyecto .
}}";
            SparqlObject query = mResourceApi.VirtuosoQuery(selectProyecto, whereProyecto, "curriculumvitae");
            if (query.results.bindings.Count != 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> res in query.results.bindings)
                {
                    pIdProyecto = res["proyecto"].value;
                }
            }
            if (string.IsNullOrEmpty(pIdProyecto))
            {
                return;
            }

            NotificacionProyecto proyecto = CrearProyecto(pIdProyecto, pIdPersona, pIdAutorizacion);
            try
            {
                RestClient client = new(pConfig.GetUrlEnvioProyecto());
                client.AddDefaultHeader("Authorization", "Bearer " + GetTokenCSP(pConfig));
                var request = new RestRequest(Method.POST);
                request.AddJsonBody(proyecto);
                IRestResponse response = client.Execute(request);
                if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw;
            }

            CambioEstadoEnvio(pIdProyecto);
        }

        /// <summary>
        /// Cambia el estado del proyecto <paramref name="pIdProyecto"/> a enviado o pendiente.
        /// </summary>
        /// <param name="pIdProyecto">Identificador del proyecto</param>
        public void CambioEstadoEnvio(string pIdProyecto)
        {
            // Comprobar si está el triple del estado.
            string valorEnviado = string.Empty;
            string select = "";
            string where = "";

            select += mPrefijos;
            select += "SELECT DISTINCT ?enviado ";
            where = @$"WHERE {{ 
                        ?s a vivo:Project .
                        OPTIONAL{{?s roh:validationStatusProject ?enviado. }}
                        FILTER(?s = <{pIdProyecto}>)
                    }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "project");
            foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
            {
                valorEnviado = UtilidadesAPI.GetValorFilaSparqlObject(fila, "enviado");
            }

            mResourceApi.ChangeOntoly("document");
            Guid guid = mResourceApi.GetShortGuid(pIdProyecto);

            if (string.IsNullOrEmpty(valorEnviado))
            {
                // Inserción.
                Insercion(guid, "http://w3id.org/roh/validationStatusProject", "pendiente");
            }
            else
            {
                // Modificación.
                Modificacion(guid, "http://w3id.org/roh/validationStatusProject", "pendiente", valorEnviado);
            }
        }

        /// <summary>
        /// Crea el proyecto notificado para enviar a validar.
        /// </summary>
        /// <param name="pIdProyecto">ID del recurso del proyecto.</param>
        /// <param name="pIdPersona">ID del recurso de la persona.</param>
        /// <param name="pIdAutorizacion">ID del recurso de la autorizacion.</param>
        /// <returns></returns>
        public NotificacionProyecto CrearProyecto(string pIdProyecto, string pIdPersona, string pIdAutorizacion)
        {
            // Obtención de datos de Proyecto.
            Dictionary<string, string> dicDatosProyecto = GetDatosProyecto(pIdProyecto);

            NotificacionProyecto notificacion = new NotificacionProyecto();
            notificacion.proyectoCVNId = pIdProyecto;
            //notificacion.proyectoCVNId = mResourceApi.GetShortGuid(pIdProyecto).ToString();
            notificacion.autorizacionId = GetAutorizacion(pIdAutorizacion); // Obtención del crisIdentifier de la autorización.
            notificacion.solicitanteRef = GetSolicitanteRef(pIdPersona); // Obtención del crisIdentifier de la persona solicitante.
            notificacion.titulo = dicDatosProyecto["titulo"];
            notificacion.fechaInicio = dicDatosProyecto["fechaInicio"];
            notificacion.fechaFin = dicDatosProyecto["fechaFin"];

            return notificacion;
        }

        /// <summary>
        /// Inserta un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        private void Insercion(Guid pGuid, string pPropiedad, string pValorNuevo)
        {
            Dictionary<Guid, List<TriplesToInclude>> dicInsercion = new Dictionary<Guid, List<TriplesToInclude>>();
            List<TriplesToInclude> listaTriplesInsercion = new List<TriplesToInclude>();
            TriplesToInclude triple = new TriplesToInclude();
            triple.Predicate = pPropiedad;
            triple.NewValue = pValorNuevo;
            listaTriplesInsercion.Add(triple);
            dicInsercion.Add(pGuid, listaTriplesInsercion);
            mResourceApi.InsertPropertiesLoadedResources(dicInsercion);
        }

        /// <summary>
        /// Modifica un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        /// <param name="pValorAntiguo"></param>
        private void Modificacion(Guid pGuid, string pPropiedad, string pValorNuevo, string pValorAntiguo)
        {
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            TriplesToModify triple = new TriplesToModify();
            triple.Predicate = pPropiedad;
            triple.NewValue = pValorNuevo;
            triple.OldValue = pValorAntiguo;
            listaTriplesModificacion.Add(triple);
            dicModificacion.Add(pGuid, listaTriplesModificacion);
            mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
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
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fechaInicio")))
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
        /// Obtiene el ID de la autorización.
        /// </summary>
        /// <param name="pIdAutorizacion">ID del recurso de la autorización.</param>
        /// <returns>Identificador de la autorización.</returns>
        public int GetAutorizacion(string pIdAutorizacion)
        {
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?crisIdentifier ");
            where.Append("WHERE { ");
            where.Append("?s a roh:ProjectAuthorization. ");
            where.Append("?s roh:crisIdentifier ?crisIdentifier. ");
            where.Append($@"FILTER(?s = <{pIdAutorizacion}>) ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "projectauthorization");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return int.Parse(fila["crisIdentifier"].value.Split("|").Last());
                }
            }

            return 0;
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
            return $@"{anyo}-{mes}-{dia}T00:00:00Z";
        }

        /// <summary>
        /// Obtención del token.
        /// </summary>
        /// <returns></returns>
        private string GetTokenCSP(ConfigService pConfig)
        {
            Uri url = new Uri(pConfig.GetUrlToken());
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("username", pConfig.GetUsernameEsbCsp()),
                new KeyValuePair<string, string>("password", pConfig.GetPasswordEsbCsp()),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            string result = httpCall(url.ToString(), "POST", content).Result;
            var json = JObject.Parse(result);

            return json["access_token"].ToString();
        }

        /// <summary>
        /// Obtención del token.
        /// </summary>
        /// <returns></returns>
        private string GetTokenPRC(ConfigService pConfig)
        {
            Uri url = new Uri(pConfig.GetUrlToken());
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("username", pConfig.GetUsernameEsbPrc()),
                new KeyValuePair<string, string>("password", pConfig.GetPasswordEsbPrc()),
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
