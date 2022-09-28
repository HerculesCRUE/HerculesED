using EditorCV.Models.EnvioDSpace;
using EditorCV.Models.PreimportModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;

namespace EditorCV.Models
{
    public class AccionesEnvioDSpace
    {
        readonly ConfigService _Configuracion;

        /// <summary>
        /// API
        /// </summary>
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static string tokenAuth = "";

        public AccionesEnvioDSpace(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        public void EnvioDSpace(string pIdRecurso)
        {
            string idRecursoDspace = "000000";
            string tituloRecurso = "";
            try
            {
                //Compruebo el estado del servicio
                Status status = GetStatus();

                if (status.okay != "true")
                {
                    throw new Exception("El servicio no está operativo");
                }

                //Compruebo si estoy autenticado
                if (status.authenticated != "true")
                {
                    Authentication();
                }

                //Recupero el identificador de Dspace y el titulo del recurso de la BBDD.
                string select = "SELECT distinct ?doc ?idDspace ?titulo";
                string where = $@"WHERE{{
                                    ?doc a <http://purl.org/ontology/bibo/Document>.
                                    <{pIdRecurso}> ?p ?doc .
                                    OPTIONAL{{?doc <http://w3id.org/roh/idDspace> ?idDspace }}
                                    OPTIONAL{{?doc <http://w3id.org/roh/title> ?titulo }}
                                }}";
                SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string> { "document", "curriculumvitae" });
                if (resultadoQuery.results.bindings.Count != 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> res in resultadoQuery.results.bindings)
                    {
                        if (res.ContainsKey("idDspace"))
                        {
                            idRecursoDspace = res["idDspace"].value.ToString();
                        }
                        if (res.ContainsKey("titulo"))
                        {
                            tituloRecurso = res["titulo"].value.ToString();
                        }
                    }
                }

                if (idRecursoDspace == "000000")
                {
                    //Buscar un recurso por el nombre
                    string urlEstadoFind = _Configuracion.GetUrlDSpace() + "/items/find-by-metadata-field";
                    HttpClient httpClientEstadoFind = new HttpClient();

                    MetadataEntry camp = new MetadataEntry("dc.title", tituloRecurso, "es_ES");

                    HttpResponseMessage responseEstadoFind = httpClientEstadoFind.PostAsJsonAsync($"{urlEstadoFind}", camp).Result;
                    if (responseEstadoFind.Content.ReadAsStringAsync().Result != "[]")
                    {
                        Item[] itemRespuesta = JsonConvert.DeserializeObject<Item[]>(responseEstadoFind.Content.ReadAsStringAsync().Result);
                        idRecursoDspace = itemRespuesta[0].id.ToString();
                    }
                }

                //Compruebo si el ítem está en DSpace y traigo los datos. 
                string urlEstado = _Configuracion.GetUrlDSpace() + "/items/" + idRecursoDspace;
                HttpClient httpClientEstado = new HttpClient();
                HttpResponseMessage responseEstado = httpClientEstado.GetAsync($"{urlEstado}").Result;


                //Inserto o actualizo.
                if (responseEstado.StatusCode == HttpStatusCode.NotFound)
                {
                    MetadataEntry metadataEntry = new MetadataEntry();

                    //En caso de no estar lo inserto
                    HttpClient httpClientInserta = new HttpClient();
                    httpClientInserta.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", tokenAuth);
                    HttpResponseMessage responseInserta = httpClientInserta.PostAsJsonAsync($"{urlEstado}", metadataEntry).Result;
                }
                else if (responseEstado.StatusCode == HttpStatusCode.OK)
                {
                    MetadataEntry[] metadataEntryArray = new MetadataEntry[15];

                    //Si está en la biblioteca actualizo los datos
                    HttpClient httpClientActualiza = new HttpClient();
                    httpClientActualiza.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", tokenAuth);
                    HttpResponseMessage responseActualiza = httpClientActualiza.PutAsJsonAsync($"{urlEstado}", metadataEntryArray).Result;
                }
                else
                {
                    throw new Exception("No se ha recibido un código de estado válido");
                }
            }
            catch (Exception)
            {

            }
        }

        private static List<MetadataEntry> GetListadoValoresItem(Item item)
        {
            List<MetadataEntry> listadoValores = new List<MetadataEntry>();
            return listadoValores;
        }

        private Status GetStatus()
        {
            Status status = new Status();

            try
            {
                string urlStatus = _Configuracion.GetUrlDSpace() + "/status";
                HttpClient httpClientStatus = new HttpClient();
                HttpResponseMessage responseStatus = httpClientStatus.GetAsync($"{urlStatus}").Result;
                if (responseStatus.IsSuccessStatusCode)
                {
                    status = responseStatus.Content.ReadFromJsonAsync<Status>().Result;
                    if (status.okay != "true")
                    {
                        throw new Exception("Status not okay");
                    }
                }
            }
            catch (Exception)
            {
                return status;
            }

            return status;
        }

        private void Authentication()
        {
            try
            {
                string urlLogin = _Configuracion.GetUrlDSpace() + "/login";

                UserDspace user = new UserDspace
                {
                    email = _Configuracion.GetUsernameDspace(),
                    password = _Configuracion.GetPasswordDspace()
                };

                string content = JsonConvert.SerializeObject(user);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpClient httpClientLoguin = new HttpClient();
                var result = httpClientLoguin.PostAsync(urlLogin, byteContent).Result;

                //Asigno el token
                //TODO
                tokenAuth = "";

            }
            catch (Exception)
            {

            }
        }
    }
}
