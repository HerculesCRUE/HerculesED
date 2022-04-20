using EditorCV.Controllers;
using EditorCV.Models.EnvioPRC;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Hercules.MA.ServicioExterno.Controllers.Utilidades;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EditorCV.Models
{
    public class AccionesEnvioPRC
    {
        #region --- Constantes   
        private static string RUTA_OAUTH = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config";
        private static ResourceApi mResourceApi = new ResourceApi(RUTA_OAUTH);
        private static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Models/Utils/prefijos.json";
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));
        #endregion

        private static Dictionary<string, string> dicPropiedades = new Dictionary<string, string>();

        /// <summary>
        /// Permite enviar a Producción Científica los datos necesarios para la validación.
        /// </summary>
        /// <param name="pId">ID del documento.</param>
        public void EnvioPRC(ConfigService pConfig, string pIdDocumento, string pIdProyecto)
        {
            // Rellena el diccionario de propiedades.
            if (!dicPropiedades.Any())
            {
                RellenarDiccionario();
            }

            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            ProduccionCientifica PRC = new ProduccionCientifica();

            // Identificador.
            PRC.idRef = pIdDocumento;
            PRC.estado = "PENDIENTE";

            #region --- Tipo del Documento.
            // Consulta sparql (Tipo del documento).
            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?tipoDocumento ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("?s roh:scientificActivityDocument ?tipoDocumento. ");
            where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    string tipo = UtilidadesAPI.GetValorFilaSparqlObject(fila, "tipoDocumento");

                    switch (tipo)
                    {
                        case "http://gnoss.com/items/scientificactivitydocument_SAD1":
                            PRC.epigrafeCVN = "060.010.010.000";
                            break;
                        case "http://gnoss.com/items/scientificactivitydocument_SAD2":
                            PRC.epigrafeCVN = "060.010.020.000";
                            break;
                        case "http://gnoss.com/items/scientificactivitydocument_SAD3":
                            PRC.epigrafeCVN = "060.010.030.000";
                            break;
                        case "http://gnoss.com/items/scientificactivitydocument_SAD4":
                            PRC.epigrafeCVN = "060.010.040.000";
                            break;
                    }
                }
            }
            #endregion

            #region --- Autores.
            // Consulta sparql (Obtención de datos de la persona).
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?crisIdentifier ?orcid ?orden ?nombre ?apellidos ?firma ");
            where.Append("FROM <http://gnoss.com/person.owl> ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("OPTIONAL{ ");
            where.Append("?s bibo:authorList ?listaAutores. ");
            where.Append("?listaAutores rdf:member ?persona. ");
            where.Append("?listaAutores rdf:comment ?orden. ");
            where.Append("?persona foaf:firstName ?nombre. ");
            where.Append("?persona foaf:lastName ?apellidos. ");
            where.Append("OPTIONAL{?persona roh:crisIdentifier ?crisIdentifier. } ");
            where.Append("OPTIONAL{?persona roh:ORCID ?orcid. } ");
            where.Append("OPTIONAL{?persona foaf:nick ?firma. } ");
            where.Append("} ");
            where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
            where.Append("} ORDER BY ?orden ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (PRC.autores == null)
                    {
                        PRC.autores = new List<Autor>();
                    }

                    Autor autor = new Autor();
                    autor.personaRef = UtilidadesAPI.GetValorFilaSparqlObject(fila, "crisIdentifier");
                    autor.firma = UtilidadesAPI.GetValorFilaSparqlObject(fila, "firma");
                    autor.nombre = UtilidadesAPI.GetValorFilaSparqlObject(fila, "nombre");
                    autor.apellidos = UtilidadesAPI.GetValorFilaSparqlObject(fila, "apellidos");
                    autor.orden = int.Parse(UtilidadesAPI.GetValorFilaSparqlObject(fila, "orden"));
                    autor.orcidId = UtilidadesAPI.GetValorFilaSparqlObject(fila, "orcid");
                    autor.ip = false; // No tenemos IP en los documentos.
                    PRC.autores.Add(autor);
                }
            }
            #endregion

            #region --- Inserción y obtención del Proyecto asociado.
            // Comprobar si está el triple.
            string idProyectoAux = string.Empty;
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?proyecto ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("OPTIONAL{?s roh:projectAux ?proyecto. } ");
            where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    idProyectoAux = UtilidadesAPI.GetValorFilaSparqlObject(fila, "proyecto");
                }
            }

            mResourceApi.ChangeOntoly("document");
            Guid guid = mResourceApi.GetShortGuid(pIdDocumento);

            if (string.IsNullOrEmpty(idProyectoAux))
            {
                // Inserción.
                Insercion(guid, "http://w3id.org/roh/projectAux", pIdProyecto);
            }
            else
            {
                // Modificación.
                Modificacion(guid, "http://w3id.org/roh/projectAux", pIdProyecto, idProyectoAux);
            }

            // Consulta sparql (Obtención del ID del proyecto).
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?crisIdentifier ");
            where.Append("WHERE { ");
            where.Append("?s a vivo:Project. ");
            where.Append("OPTIONAL{?s roh:crisIdentifier ?crisIdentifier. } ");
            where.Append($@"FILTER(?s = <{pIdProyecto}>) ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "project");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    string crisIdentifier = UtilidadesAPI.GetValorFilaSparqlObject(fila, "crisIdentifier");
                    if (!string.IsNullOrEmpty(crisIdentifier))
                    {
                        //PRC.proyectos = new List<float>() { float.Parse(140012+"") };
                        PRC.proyectos = new List<float>() { float.Parse(crisIdentifier) };
                    }
                }
            }
            #endregion

            #region --- Obtención de datos del Documento.
            // TODO: ¿Qué campos enviamos? Problema con PagInicio y PagFin.
            PRC.campos = new List<CampoProduccionCientifica>();

            // Consulta sparql (Obtención de datos del proyecto).
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT * ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("?s roh:title ?title. ");
            where.Append("OPTIONAL{?s dc:type ?type. } ");
            where.Append("OPTIONAL{?s dc:typeOthers ?typeOthers. } ");
            where.Append("OPTIONAL{?s roh:contributionGrade ?contributionGrade. } ");
            where.Append("OPTIONAL{?s roh:supportType ?supportType. } ");
            where.Append("OPTIONAL{?s bibo:volume ?volume. } ");
            where.Append("OPTIONAL{?s bibo:issue ?issue. } ");
            where.Append("OPTIONAL{?s bibo:pageStart ?pageStart. } ");
            where.Append("OPTIONAL{?s bibo:pageEnd ?pageEnd. } ");
            where.Append("OPTIONAL{?s vcard:hasCountryName ?hasCountryName. } ");
            where.Append("OPTIONAL{?s vcard:hasRegion ?hasRegion. } ");
            where.Append("OPTIONAL{?s dct:issued ?issued. } ");
            where.Append("OPTIONAL{?s vcard:url ?url. } ");
            where.Append("OPTIONAL{?s roh:isbn ?isbn. } ");
            where.Append("OPTIONAL{?s roh:legalDeposit ?legalDeposit. } ");
            where.Append("OPTIONAL{?s vcard:locality ?locality. } ");
            where.Append("OPTIONAL{?s roh:collection ?collection. } ");
            where.Append("OPTIONAL{?s roh:relevantResults ?relevantResults. } ");
            where.Append("OPTIONAL{?s roh:relevantPublication ?relevantPublication. } ");
            where.Append("OPTIONAL{?s roh:reviewsNumber ?reviewsNumber. } ");
            where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    foreach (string item in fila.Keys)
                    {
                        if (dicPropiedades.ContainsKey(item))
                        {
                            if (PRC.campos.Any(x => x.codigoCVN == dicPropiedades[item]))
                            {
                                if (item == "issued")
                                {
                                    string dia = fila[item].value.Substring(6, 2);
                                    string mes = fila[item].value.Substring(4, 2);
                                    string anyo = fila[item].value.Substring(0, 4);
                                    string fecha = $@"{anyo}-{mes}-{dia}";
                                    PRC.campos.First(x => x.codigoCVN == dicPropiedades[item]).valores.Add(fecha);
                                }
                                else
                                {
                                    PRC.campos.First(x => x.codigoCVN == dicPropiedades[item]).valores.Add(fila[item].value);
                                }
                            }
                            else
                            {
                                CampoProduccionCientifica campo = new CampoProduccionCientifica();
                                campo.codigoCVN = dicPropiedades[item];
                                if (item == "issued")
                                {
                                    string dia = fila[item].value.Substring(6, 2);
                                    string mes = fila[item].value.Substring(4, 2);
                                    string anyo = fila[item].value.Substring(0, 4);
                                    string fecha = $@"{anyo}-{mes}-{dia}";
                                    campo.valores = new List<string>() { fecha };
                                }
                                else
                                {
                                    campo.valores = new List<string>() { fila[item].value };
                                }
                                PRC.campos.Add(campo);
                            }
                        }
                    }
                }
            }
            #endregion

            #region --- Envío a SGI.
            try
            {
                RestClient client = new(pConfig.GetUrlProduccionCientifica());
                client.AddDefaultHeader("Authorization", "Bearer " + GetToken(pConfig));
                var request = new RestRequest(Method.POST);
                request.AddJsonBody(PRC);
                IRestResponse response = client.Execute(request);
            }
            catch (Exception)
            {
                throw;
            }
            #endregion

            #region -- Cambio del estado del envío.
            // Comprobar si está el triple del estado.
            string valorEnviado = string.Empty;
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?enviado ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("OPTIONAL{?s roh:validationStatusPRC ?enviado. } ");
            where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    valorEnviado = UtilidadesAPI.GetValorFilaSparqlObject(fila, "enviado");
                }
            }

            mResourceApi.ChangeOntoly("document");
            guid = mResourceApi.GetShortGuid(pIdDocumento);

            if (string.IsNullOrEmpty(valorEnviado))
            {
                // Inserción.
                Insercion(guid, "http://w3id.org/roh/validationStatusPRC", "PENDIENTE");
            }
            else
            {
                // Modificación.
                Modificacion(guid, "http://w3id.org/roh/validationStatusPRC", "PENDIENTE", valorEnviado);
            }
            #endregion
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
        /// Mapea el código CVN con la propiedad usada en SPARQL.
        /// </summary>
        private void RellenarDiccionario()
        {
            dicPropiedades.Add("title", "060.010.010.030");
            dicPropiedades.Add("type", "060.010.010.010");
            dicPropiedades.Add("typeOthers", "060.010.010.020");
            dicPropiedades.Add("contributionGrade", "060.010.010.060");
            dicPropiedades.Add("supportType", "060.010.010.070");
            dicPropiedades.Add("volume", "060.010.010.080");
            dicPropiedades.Add("issue", "060.010.010.080");
            dicPropiedades.Add("pageStart", "060.010.010.090");
            dicPropiedades.Add("pageEnd", "060.010.010.090");
            dicPropiedades.Add("hasCountryName", "060.010.010.110");
            dicPropiedades.Add("hasRegion", "060.010.010.120");
            dicPropiedades.Add("issued", "060.010.010.140");
            dicPropiedades.Add("url", "060.010.010.150");
            dicPropiedades.Add("isbn", "060.010.010.160");
            dicPropiedades.Add("legalDeposit", "060.010.010.170");
            dicPropiedades.Add("locality", "060.010.010.220");
            dicPropiedades.Add("collection", "060.010.010.270");
            dicPropiedades.Add("relevantResults", "060.010.010.290");
            dicPropiedades.Add("relevantPublication", "060.010.010.300");
            dicPropiedades.Add("reviewsNumber", "060.010.010.340");
        }

        /// <summary>
        /// Obtención del token.
        /// </summary>
        /// <returns></returns>
        private string GetToken(ConfigService pConfig)
        {
            // TODO: Sacar a archivo de configuración.
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
