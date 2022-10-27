using EditorCV.Models.EnvioDSpace;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace EditorCV.Models
{
    public class AccionesEnvioDSpace
    {
        readonly ConfigService _Configuracion;

        /// <summary>
        /// API
        /// </summary>
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");
        private static string tokenAuth = "";

        public AccionesEnvioDSpace(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        public void EnvioDSpace(string pIdRecurso, IFormFile file)
        {
            Publication publication = new Publication("000000", "");
            string idPublication = "";

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
                //Si no me he conseguido autenticar, devuelvo una excepción
                status = GetStatus();
                if(status.authenticated != "true")
                {
                    throw new Exception("No se ha conseguido autenticar");
                }


                //Recupero los datos del recurso
                GetDatosPublicacion(pIdRecurso, publication, ref idPublication);

                //Autores del documento
                GetAutoresPublicacion(pIdRecurso, publication);

                if (publication.idRecursoDspace == "000000")
                {
                    //Buscar un recurso por el nombre
                    string urlEstadoFind = _Configuracion.GetUrlDSpace() + "/items/find-by-metadata-field";
                    HttpClient httpClientEstadoFind = new HttpClient();

                    Metadata camp = new Metadata("dc.title", publication.tituloRecurso);
                    try
                    {
                        HttpResponseMessage responseEstadoFind = httpClientEstadoFind.PostAsJsonAsync($"{urlEstadoFind}", camp).Result;
                        if (responseEstadoFind.Content.ReadAsStringAsync().Result != "[]")
                        {
                            Item[] itemRespuesta = JsonConvert.DeserializeObject<Item[]>(responseEstadoFind.Content.ReadAsStringAsync().Result);
                            publication.idRecursoDspace = itemRespuesta[0].id.ToString();
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                //Compruebo si el ítem está en DSpace y traigo los datos. 
                string urlEstado = _Configuracion.GetUrlDSpace() + "/items/" + publication.idRecursoDspace;
                HttpClient httpClientEstado = new HttpClient();
                httpClientEstado.DefaultRequestHeaders.Add("rest-dspace-token", tokenAuth);
                HttpResponseMessage responseEstado = httpClientEstado.GetAsync($"{urlEstado}").Result;


                //Inserto(404, recurso no encontrado por ID).
                if (responseEstado.StatusCode == HttpStatusCode.NotFound)
                {
                    //Añade los metadatos a DSpace
                    DSpaceResponse dSpace = new();
                    dSpace = InsertaMetadatosDspace(publication);
                    if (dSpace.id.Equals("000000"))
                    {
                        throw new Exception("Error al insertar datos");
                    }
                    //Inserta el triple con el Identificador de DSpace 
                    AniadirIdDspace(dSpace.id, idPublication);

                    //Añado el archivo al recurso
                    if (file != null)
                    {
                        AniadirBitstreamDspace(publication, file);
                    }
                }
                //Actualizo(200, recurso encontrado en DSpace)
                else if (responseEstado.StatusCode == HttpStatusCode.OK)
                {
                    ActualizaDspace(publication); 

                    //Añado el archivo al recurso
                    if (file != null)
                    {
                        AniadirBitstreamDspace(publication, file);
                    }
                }
                else
                {
                    throw new Exception("No se ha recibido un código de estado válido");
                }
            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Inserta los metadatos del recurso en DSpace
        /// </summary>
        /// <param name="publication"></param>
        /// <returns></returns>
        private DSpaceResponse InsertaMetadatosDspace(Publication publication)
        {
            //Colección con handle
            string urlEstado = _Configuracion.GetUrlDSpace() + "/collections/" + _Configuracion.GetCollectionDSpace() + "/items";
            MetadataSend metadata = GetListadoValoresItem(publication);

            try
            {
                HttpClient httpClientInserta = new HttpClient();
                httpClientInserta.DefaultRequestHeaders.Add("rest-dspace-token", tokenAuth);
                HttpResponseMessage responseInserta = httpClientInserta.PostAsJsonAsync($"{urlEstado}", metadata.rootObject).Result;
                if (responseInserta.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Error en la inserción");
                }

                DSpaceResponse dSpace = JsonConvert.DeserializeObject<DSpaceResponse>(responseInserta.Content.ReadAsStringAsync().Result.ToString());
                return dSpace;
            }
            catch (Exception)
            {
                return new DSpaceResponse() { id = "000000" };
            }
        }

        /// <summary>
        /// Actualiza los metadatos del recurso en DSpace
        /// </summary>
        /// <param name="publication"></param>
        /// <returns></returns>
        private DSpaceResponse ActualizaDspace(Publication publication)
        {
            MetadataSend metadata = GetListadoValoresItem(publication);

            string urlEstado = _Configuracion.GetUrlDSpace() + "/items/" + publication.idRecursoDspace + "/metadata";
            try
            {
                //Si está en la biblioteca actualizo los datos
                HttpClient httpClientActualiza = new HttpClient();
                httpClientActualiza.DefaultRequestHeaders.Add("rest-dspace-token", tokenAuth);
                HttpResponseMessage responseActualiza = httpClientActualiza.PutAsJsonAsync($"{urlEstado}", metadata.rootObject.metadata).Result;
                if (responseActualiza.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Error en la actualización");
                }

                DSpaceResponse dSpace = JsonConvert.DeserializeObject<DSpaceResponse>(responseActualiza.Content.ReadAsStringAsync().Result.ToString());
                return dSpace;
            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
                return new DSpaceResponse() { id = "000000" };
            }
        }

        /// <summary>
        /// Añade el erchivo <paramref name="file"/> al ítem de DSpace.
        /// </summary>
        /// <param name="publication"></param>
        /// <param name="file"></param>
        private void AniadirBitstreamDspace(Publication publication, IFormFile file)
        {
            MetadataSend metadata = GetListadoValoresItem(publication);

            string urlEstado = _Configuracion.GetUrlDSpace() + "/items/" + publication.idRecursoDspace + "/bitstreams?name=" + file.FileName;
            try
            {
                MultipartFormDataContent multipartFormData = new MultipartFormDataContent();
                var ms = new MemoryStream();
                file.CopyTo(ms);
                byte[] filebytes = ms.ToArray();
                multipartFormData.Add(new ByteArrayContent(filebytes), "File", file.FileName);


                //Si está en la biblioteca actualizo los datos
                HttpClient httpClientBitstream = new HttpClient();
                httpClientBitstream.DefaultRequestHeaders.Add("rest-dspace-token", tokenAuth);
                HttpResponseMessage responseBitstream = httpClientBitstream.PostAsync($"{urlEstado}", multipartFormData).Result;
                if (responseBitstream.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Error en la inserción del archivo");
                }
            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza los datos de una publicación
        /// </summary>
        /// <param name="pIdRecurso"></param>
        /// <param name="publication"></param>
        /// <param name="idPublication"></param>
        private void GetDatosPublicacion(string pIdRecurso, Publication publication, ref string idPublication)
        {
            string select = "SELECT distinct ?doc ?idDspace ?titulo ?anioPublicacion ?issn ?isbn ?handle ?descripcion ?pagIni ?pagFin ?editorial ?isOpenAccess ?idTipo";
            string where = $@"WHERE{{
                                    ?doc a <http://purl.org/ontology/bibo/Document>.
                                    <{pIdRecurso}> ?p ?doc .
                                    OPTIONAL{{?doc <http://w3id.org/roh/idDspace> ?idDspace }}
                                    OPTIONAL{{?doc <http://w3id.org/roh/title> ?titulo }}

                                    OPTIONAL{{?doc <http://w3id.org/roh/year> ?anioPublicacion}}
                                    OPTIONAL{{?doc <http://purl.org/ontology/bibo/issn> ?issn}}
                                    OPTIONAL{{?doc <http://w3id.org/roh/isbn> ?isbn}}
                                    OPTIONAL{{?doc <http://purl.org/ontology/bibo/handle> ?handle}}
                                    OPTIONAL{{?doc <http://purl.org/ontology/bibo/abstract> ?descripcion}}
                                    OPTIONAL{{?doc <http://purl.org/ontology/bibo/pageStart> ?pagIni}}
                                    OPTIONAL{{?doc <http://purl.org/ontology/bibo/pageEnd> ?pagFin}}
                                    OPTIONAL{{?doc <http://purl.org/ontology/bibo/publisher> ?editorial}}
                                    OPTIONAL{{?doc <http://w3id.org/roh/openAccess> ?isOpenAccess}}
                                    OPTIONAL{{?doc <http://purl.org/dc/elements/1.1/type> ?tipo . ?tipo <http://purl.org/dc/elements/1.1/identifier> ?idTipo}}
                                }}";
            SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string> { "curriculumvitae", "document", "publicationtype" });
            if (resultadoQuery.results.bindings.Count != 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> res in resultadoQuery.results.bindings)
                {
                    if (res.ContainsKey("doc") && !string.IsNullOrEmpty(res["doc"].value))
                    {
                        idPublication = res["doc"].value;
                    }

                    if (res.ContainsKey("idDspace") && !string.IsNullOrEmpty(res["idDspace"].value))
                    {
                        publication.idRecursoDspace = res["idDspace"].value.ToString();
                    }
                    if (res.ContainsKey("titulo") && !string.IsNullOrEmpty(res["titulo"].value))
                    {
                        publication.tituloRecurso = res["titulo"].value.ToString();
                    }

                    if (res.ContainsKey("anioPublicacion") && !string.IsNullOrEmpty(res["anioPublicacion"].value))
                    {
                        publication.anioPublicacion = res["anioPublicacion"].value.ToString();
                    }
                    if (res.ContainsKey("issn") && !string.IsNullOrEmpty(res["issn"].value))
                    {
                        publication.issn = res["issn"].value.ToString();
                    }
                    if (res.ContainsKey("isbn") && !string.IsNullOrEmpty(res["isbn"].value))
                    {
                        publication.isbn = res["isbn"].value.ToString();
                    }
                    if (res.ContainsKey("handle") && !string.IsNullOrEmpty(res["handle"].value))
                    {
                        publication.handle = res["handle"].value.ToString();
                    }
                    if (res.ContainsKey("descripcion") && !string.IsNullOrEmpty(res["descripcion"].value))
                    {
                        publication.descripcion = res["descripcion"].value.ToString();
                    }
                    if (res.ContainsKey("pagIni") && !string.IsNullOrEmpty(res["pagIni"].value))
                    {
                        publication.pagIni = res["pagIni"].value.ToString();
                    }
                    if (res.ContainsKey("pagFin") && !string.IsNullOrEmpty(res["pagFin"].value))
                    {
                        publication.pagFin = res["pagFin"].value.ToString();
                    }
                    if (res.ContainsKey("editorial") && !string.IsNullOrEmpty(res["editorial"].value))
                    {
                        publication.editorial = res["editorial"].value.ToString();
                    }
                    if (res.ContainsKey("isOpenAccess") && !string.IsNullOrEmpty(res["isOpenAccess"].value))
                    {
                        publication.isOpenAccess = res["isOpenAccess"].value.ToString();
                    }
                    if (res.ContainsKey("idTipo") && !string.IsNullOrEmpty(res["idTipo"].value))
                    {
                        publication.tipo = res["idTipo"].value.ToString();
                    }
                }
            }
        }

        private void GetAutoresPublicacion(string pIdRecurso, Publication publication)
        {
            string selectAutores = "select distinct ?nombrePersona";
            string whereAutores = $@"WHERE{{
                                    ?doc a <http://purl.org/ontology/bibo/Document>.
                                    <{pIdRecurso}> ?p ?doc .
                                    OPTIONAL{{
                                        ?doc <http://purl.org/ontology/bibo/authorList> ?autores .
                                        ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona . 
                                        ?persona <http://xmlns.com/foaf/0.1/name> ?nombrePersona .
                                    }}
                                }}";
            SparqlObject resultadoAutores = mResourceApi.VirtuosoQueryMultipleGraph(selectAutores, whereAutores, new List<string> { "person", "document", "curriculumvitae" });
            if (resultadoAutores.results.bindings.Count != 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> res in resultadoAutores.results.bindings)
                {
                    if (res.ContainsKey("nombrePersona") && !string.IsNullOrEmpty(res["nombrePersona"].value))
                    {
                        publication.autores.Add(res["nombrePersona"].value);
                    }
                }
            }
        }

        /// <summary>
        /// Añade el triple del identificador de DSpace al recurso <paramref name="pIdRecurso"/>
        /// </summary>
        /// <param name="IdDSpace">Identificador de DSpace</param>
        /// <param name="pIdRecurso">Identificador largo del recuso</param>
        private void AniadirIdDspace(string IdDSpace, string pIdRecurso)
        {
            //No hago nada si el identificador del recurso es nulo.
            if (string.IsNullOrEmpty(pIdRecurso))
            {
                return;
            }
            //No hago nada si el identificador de DSpace es el valor por defecto.
            if (IdDSpace.Equals("000000"))
            {
                return;
            }

            mResourceApi.ChangeOntoly("document");
            Dictionary<Guid, List<TriplesToInclude>> dicInsercion = new Dictionary<Guid, List<TriplesToInclude>>();
            List<TriplesToInclude> listaTriplesInsercion = new List<TriplesToInclude>();

            //Triple de identificador de DSpace
            TriplesToInclude triple = new TriplesToInclude();
            triple.Predicate = "http://w3id.org/roh/idDspace";
            triple.NewValue = IdDSpace;
            listaTriplesInsercion.Add(triple);

            dicInsercion.Add(mResourceApi.GetShortGuid(pIdRecurso), listaTriplesInsercion);
            mResourceApi.InsertPropertiesLoadedResources(dicInsercion);
        }

        private static MetadataSend GetListadoValoresItem(Publication publication)
        {
            List<Metadata> listadoValores = new List<Metadata>();
            //Titulo
            if (!string.IsNullOrEmpty(publication.tituloRecurso))
            {
                Metadata metadataEntryTitle = new Metadata("dc.title", publication.tituloRecurso);
                listadoValores.Add(metadataEntryTitle);
            }

            //Tipo de documento
            if (!string.IsNullOrEmpty(publication.tipo))
            {
                Metadata metadataEntryType = new Metadata("dc.type", tipoType(publication.tipo));
                listadoValores.Add(metadataEntryType);
            }

            //Autores
            if (publication.autores.Any())
            {
                foreach (string autor in publication.autores)
                {
                    Metadata metadataEntryAuthor = new Metadata("dc.contributor.author", autor);
                    listadoValores.Add(metadataEntryAuthor);
                }
            }

            //Año de publicación
            if (!string.IsNullOrEmpty(publication.anioPublicacion))
            {
                Metadata metadataEntryDateCreated = new Metadata("dc.date.created", publication.anioPublicacion);
                listadoValores.Add(metadataEntryDateCreated);
            }

            //Fecha defensa/creación
            //if (!string.IsNullOrEmpty(publication.)) { 
            //MetadataEntry metadataEntryDateIssued = new MetadataEntry("dc.date.issued", "2022");
            //listadoValores.Add(metadataEntryDateIssued);
            //}

            //Descripción
            if (!string.IsNullOrEmpty(publication.descripcion))
            {
                Metadata metadataEntryDescription = new Metadata("dc.description", publication.descripcion);
                listadoValores.Add(metadataEntryDescription);
            }

            //Resumen
            //if (!string.IsNullOrEmpty(publication.)) { 
            //MetadataEntry metadataEntryDescriptionAbstract = new MetadataEntry("dc.description.abstract", );
            //listadoValores.Add(metadataEntryDescriptionAbstract);
            //}

            //ISBN
            if (!string.IsNullOrEmpty(publication.isbn))
            {
                Metadata metadataEntryISBN = new Metadata("dc.identifier.isbn", publication.isbn);
                listadoValores.Add(metadataEntryISBN);
            }

            //ISSN
            if (!string.IsNullOrEmpty(publication.issn))
            {
                Metadata metadataEntryISSN = new Metadata("dc.identifier.issn", publication.issn);
                listadoValores.Add(metadataEntryISSN);
            }

            //Handle
            if (!string.IsNullOrEmpty(publication.handle))
            {
                Metadata metadataEntryIdentifierUri = new Metadata("dc.identifier.uri", publication.handle);
                listadoValores.Add(metadataEntryIdentifierUri);
            }

            //Idioma
            //if (!string.IsNullOrEmpty(publication.)) { 
            //MetadataEntry metadataEntryLanguage = new MetadataEntry("dc.language", "");
            //listadoValores.Add(metadataEntryLanguage);
            //}

            //Editorial
            if (!string.IsNullOrEmpty(publication.editorial))
            {
                Metadata metadataEntryPublisher = new Metadata("dc.publisher", publication.editorial);
                listadoValores.Add(metadataEntryPublisher);
            }

            if (!string.IsNullOrEmpty(publication.isOpenAccess))
            {
                string isOpenAccess = "";
                if (publication.isOpenAccess.Equals("true"))
                {
                    isOpenAccess = "info:eu-repo/semantics/openAccess";
                }
                if (!string.IsNullOrEmpty(isOpenAccess))
                {
                    Metadata metadataEntryOpenAccess = new Metadata("", isOpenAccess);
                    listadoValores.Add(metadataEntryOpenAccess);
                }
            }

            MetadataSend metadataSend = new MetadataSend(listadoValores);
            return metadataSend;
        }

        /// <summary>
        /// Sustituye el valor numerico del tipo, por su equivalente de tipo de pueblocacion en openaire
        /// </summary>
        /// <param name="idTipo"></param>
        /// <returns></returns>
        private static string tipoType(string idTipo)
        {
            switch (idTipo)
            {
                //Capitulo de libro
                case "004":
                    return "info:eu-repo/semantics/bookPart";

                //Informe científico-técnico
                case "018":
                    return "info:eu-repo/semantics/report";

                //Artículo científico
                case "020":
                    return "info:eu-repo/semantics/article";

                //Libro o monografía científica
                case "032":
                    return "info:eu-repo/semantics/book";

                //Artículo de enciclopedia
                case "202":
                    return "info:eu-repo/semantics/article";

                //Artículo de divulgación
                case "203":
                    return "info:eu-repo/semantics/article";

                //Traducción
                case "204":
                    return "info:eu-repo/semantics/other";

                //Reseña
                case "205":
                    return "info:eu-repo/semantics/review";

                //Revisión bibliográfica
                case "206":
                    return "info:eu-repo/semantics/other";

                //Libro de divulgación
                case "207":
                    return "info:eu-repo/semantics/book";

                //Diccionario científico
                case "209":
                    return "info:eu-repo/semantics/other";

                //Otros
                case "OTHERS":
                    return "info:eu-repo/semantics/other";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Devuelve el estado del servicio
        /// </summary>
        /// <returns></returns>
        private Status GetStatus()
        {
            Status status = new Status();

            try
            {
                string urlStatus = _Configuracion.GetUrlDSpace() + "/status";
                HttpClient httpClientStatus = new HttpClient();

                if (!string.IsNullOrEmpty(tokenAuth))
                {
                    httpClientStatus.DefaultRequestHeaders.Add("rest-dspace-token", tokenAuth);
                }

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

        /// <summary>
        /// Autentica al usuario con las credenciales de configuración en DSpace y asigna el token a tokenAuth
        /// </summary>
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
                tokenAuth = result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception)
            {

            }
        }
    }
}
