﻿using EditorCV.Models.API;
using EditorCV.Models.API.Input;
using EditorCV.Models.API.Response;
using EditorCV.Models.Utils;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace EditorCV.Models
{
    public class AccionesExportacion
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static readonly CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        /// <summary>
        /// Añade el archivo enviado como array de bytes.
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="nombreCV"></param>
        /// <param name="pCVID"></param>
        /// <param name="lang"></param>
        /// <param name="listaId"></param>
        /// <param name="tipoCVNExportacion"></param>
        public void AddFile(ConfigService _Configuracion, string pCVID, string nombreCV, string lang, List<string> listaId, string tipoCVNExportacion)
        {
            Guid guidCortoCVID = mResourceApi.GetShortGuid(pCVID);

            //Añado GeneratedPDFFile sin el link al archivo
            string filePredicateTitle = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/title";
            string filePredicateFecha = "http://w3id.org/roh/generatedPDFFile|http://purl.org/dc/terms/issued";
            string filePredicateEstado = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/status";

            string idEntityAux = $"{mResourceApi.GraphsUrl}items/GeneratedPDFFile_" + guidCortoCVID.ToString() + "_" + Guid.NewGuid();

            string PDFFilePDF = "CV_filePDF" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".pdf";
            string PDFFileFecha = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string PDFFileEstado = "pendiente";

            List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
            TriplesToInclude trTitle = new TriplesToInclude(idEntityAux + "|" + nombreCV, filePredicateTitle);
            listaTriples.Add(trTitle);
            TriplesToInclude trFecha = new TriplesToInclude(idEntityAux + "|" + PDFFileFecha, filePredicateFecha);
            listaTriples.Add(trFecha);
            TriplesToInclude trEstado = new TriplesToInclude(idEntityAux + "|" + PDFFileEstado, filePredicateEstado);
            listaTriples.Add(trEstado);

            var inserted = mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { guidCortoCVID, listaTriples } });

            Thread thread = new Thread(() => AddPDFFile(_Configuracion, pCVID, lang, idEntityAux, PDFFilePDF, guidCortoCVID, filePredicateEstado, listaId, tipoCVNExportacion));
            thread.Start();
        }

        /// <summary>
        /// Adjunto el fichero y modifico los triples de <paramref name="idEntityAux"/> para referenciar el archivo y 
        /// modificar el estado a "procesado". En caso de error durante el proceso cambio el estado a "error".
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="pCVID">Identificador del CV</param>
        /// <param name="lang">Lenguaje del CV</param>
        /// <param name="listaId">listado de identificadores</param>
        /// <param name="idEntityAux">Identificador de la entidad auxiliar a modificar</param>
        /// <param name="PDFFilePDF">nombre del fichero</param>
        /// <param name="guidCortoCVID">GUID corto del CV</param>
        /// <param name="filePredicateEstado">Predicado estado de la entidad</param>
        void AddPDFFile(ConfigService _Configuracion, string pCVID, string lang, string idEntityAux,
            string PDFFilePDF, Guid guidCortoCVID, string filePredicateEstado, List<string> listaId, string tipoCVNExportacion)
        {
            try
            {
                //Petición al exportador
                List<KeyValuePair<string, string>> parametros = new List<KeyValuePair<string, string>>();
                parametros.Add(new KeyValuePair<string, string>("pCVID", pCVID));
                parametros.Add(new KeyValuePair<string, string>("lang", lang));
                parametros.Add(new KeyValuePair<string, string>("tipoCVNExportacion", tipoCVNExportacion));

                string urlExportador = "";
                if (listaId == null)
                {
                    urlExportador = _Configuracion.GetUrlExportador() + "/Exportar";
                }
                else
                {
                    urlExportador = _Configuracion.GetUrlExportador() + "/ExportarLimitado";

                    foreach (string id in listaId)
                    {
                        parametros.Add(new KeyValuePair<string, string>("listaId", id));
                    }
                }


                FormUrlEncodedContent formContent = new FormUrlEncodedContent(parametros);

                //Petición al exportador para conseguir el archivo PDF
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(1, 15, 0);

                HttpResponseMessage response = client.PostAsync($"{urlExportador}", formContent).Result;
                response.EnsureSuccessStatusCode();
                byte[] result = response.Content.ReadAsByteArrayAsync().Result;

                //Inserto el archivo
                string filePredicate = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/filePDF";

                string fileName = idEntityAux + "|" + PDFFilePDF;
                List<byte[]> attachedFile = new List<byte[]>();
                attachedFile.Add(result);

                //Añado el fichero en virtuoso
                mResourceApi.AttachFileToResource(guidCortoCVID, filePredicate, fileName,
                    new List<string>() { PDFFilePDF }, new List<short>() { 0 }, attachedFile);

                //Cambio el estado a "procesado"
                string PDFFileEstado = "procesado";
                Dictionary<Guid, List<TriplesToModify>> triplesModificar = new Dictionary<Guid, List<TriplesToModify>>();
                triplesModificar[mResourceApi.GetShortGuid(pCVID)] = new List<TriplesToModify>()
                {
                    new TriplesToModify(idEntityAux + "|" + PDFFileEstado, idEntityAux + "|pendiente", filePredicateEstado)
                };
                mResourceApi.ModifyPropertiesLoadedResources(triplesModificar);

            }
            catch (Exception e)
            {
                //Cambio el estado a "error"
                string PDFFileEstado = "error";
                Dictionary<Guid, List<TriplesToModify>> triplesModificar = new Dictionary<Guid, List<TriplesToModify>>();
                triplesModificar[mResourceApi.GetShortGuid(pCVID)] = new List<TriplesToModify>()
                {
                    new TriplesToModify(idEntityAux + "|" + PDFFileEstado, idEntityAux + "|pendiente", filePredicateEstado)
                };
                mResourceApi.ModifyPropertiesLoadedResources(triplesModificar);
                mResourceApi.Log.Error("Error: " + e.Message + ". Traza:" + e.StackTrace);
            }
        }

        /// <summary>
        /// Devuelve el listado de ficheros PDF guardados.
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <returns></returns>
        public static List<FilePDF> GetListPDFFile(string pCVId, string baseUrl, int timezoneOffset)
        {
            List<FilePDF> listadoArchivos = new List<FilePDF>();
            string select = "SELECT ?titulo ?fecha ?estado ?fichero";
            string where = $@"WHERE{{
    <{pCVId}> <http://w3id.org/roh/generatedPDFFile> ?pdfFile .
    ?pdfFile <http://w3id.org/roh/title> ?titulo.
    ?pdfFile <http://purl.org/dc/terms/issued> ?fecha.
    ?pdfFile <http://w3id.org/roh/status> ?estado.
    OPTIONAL{{ ?pdfFile <http://w3id.org/roh/filePDF> ?fichero }}
}}order by desc(xsd:long(?fecha))";

            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (!fila.ContainsKey("titulo"))
                {
                    continue;
                }

                FilePDF file = new FilePDF();
                file.titulo = fila["titulo"].value;
                file.fichero = "";
                file.fecha = fila["fecha"].value;
                int dd = int.Parse(file.fecha.Substring(6, 2));
                int MM = int.Parse(file.fecha.Substring(4, 2));
                int yyyy = int.Parse(file.fecha.Substring(0, 4));
                int HH = int.Parse(file.fecha.Substring(8, 2));
                int mm = int.Parse(file.fecha.Substring(10, 2));
                int SS = int.Parse(file.fecha.Substring(12, 2));
                DateTime fecha = new DateTime(yyyy, MM, dd, HH, mm, SS, DateTimeKind.Utc);
                fecha = fecha.AddMinutes(-timezoneOffset);

                file.fecha = fecha.ToString("dd/MM/yyyy HH:mm:ss");
                file.estado = fila["estado"].value;
                if (fila.ContainsKey("fichero"))
                {
                    string uri = baseUrl + "/download-file?doc=" + mResourceApi.GetShortGuid(pCVId) + "&ext=.pdf&archivoAdjuntoSem="
                        + fila["fichero"].value.Split(".").First()
                        + "&proy=" + mCommunityApi.GetCommunityId().ToString().ToLower() + "&dscr=true";
                    file.fichero = uri;
                }

                listadoArchivos.Add(file);
            }

            return listadoArchivos;
        }

        /// <summary>
        /// Devuleve un diccionario con todos los perfiles de exportación del usuario
        /// </summary>
        /// <param name="pCVId">Identificador del curriculum vitae</param>
        /// <returns>Diccionario con los perfiles de exportación</returns>
        public Dictionary<string, List<string>> GetPerfilExportacion(string pCVId)
        {
            Dictionary<string, List<string>> dicIds = new Dictionary<string, List<string>>();
            string select = "SELECT distinct ?titulo ?checkedItems";
            string where = $@"WHERE{{
    <{pCVId}> <http://w3id.org/roh/exportProfile> ?exportProfile .
    ?exportProfile <http://w3id.org/roh/title> ?titulo .
    ?exportProfile <http://w3id.org/roh/checkedItems> ?checkedItems .
}}";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (fila.ContainsKey("titulo") && fila.ContainsKey("checkedItems"))
                {
                    if (dicIds.ContainsKey(fila["titulo"].value))
                    {
                        dicIds[fila["titulo"].value].Add(fila["checkedItems"].value);
                    }
                    else
                    {
                        dicIds.Add(fila["titulo"].value, new List<string>() { fila["checkedItems"].value });
                    }
                }
            }

            return dicIds;
        }

        /// <summary>
        /// Añade un perfil de exportacion al usuario, con los identificadores de los elementos que ha checkeado
        /// </summary>
        /// <param name="pCVId">Identificador del curriculum vitae</param>
        /// <param name="nombrePerfil">Nombre del perfil de exportación</param>
        /// <param name="checks">Listado con los checks</param>
        /// <returns>True si se añade en BBDD</returns>
        public bool AddPerfilExportacion(string pCVId, string nombrePerfil, List<string> checks)
        {
            Dictionary<string, List<string>> dicIds = new Dictionary<string, List<string>>();
            string select = "SELECT distinct ?exportProfile ?titulo ?checkedItems";
            string where = $@"WHERE{{
    <{pCVId}> <http://w3id.org/roh/exportProfile> ?exportProfile .
    ?exportProfile <http://w3id.org/roh/title> ?titulo .
    ?exportProfile <http://w3id.org/roh/checkedItems> ?checkedItems .
}}";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (fila.ContainsKey("titulo") && fila.ContainsKey("checkedItems"))
                {
                    if (dicIds.ContainsKey(fila["titulo"].value))
                    {
                        dicIds[fila["titulo"].value].Add(fila["checkedItems"].value);
                    }
                    else
                    {
                        dicIds.Add(fila["titulo"].value, new List<string>() { fila["checkedItems"].value });
                    }
                }
            }

            //Añadir
            if (!dicIds.ContainsKey(nombrePerfil))
            {
                mResourceApi.ChangeOntoly("curriculumvitae");
                Entity entity = new Entity();
                entity.properties = new List<Entity.Property>();
                entity.rdfType = "http://w3id.org/roh/exportProfile";
                entity.ontology = "curriculumvitae";
                //entity.propTitle = "http://w3id.org/roh/title";
                string idRecurso = Guid.NewGuid().ToString();

                Entity.Property propertyChecks = new Entity.Property();
                propertyChecks.prop = "http://w3id.org/roh/checkedItems";
                propertyChecks.values = new List<string>();
                foreach (string valueCheck in checks)
                {
                    propertyChecks.values.Add(valueCheck);
                }

                Entity.Property propertyTitle = new Entity.Property();
                propertyTitle.prop = "http://w3id.org/roh/title";
                propertyTitle.values = new List<string>() { nombrePerfil };

                entity.properties.Add(propertyChecks);
                entity.properties.Add(propertyTitle);
                IncludeTriples(pCVId, propertyTitle, propertyChecks);
            }
            //Modificar el recurso
            else
            {
                mResourceApi.ChangeOntoly("curriculumvitae");
                Entity entity = new Entity();
                entity.properties = new List<Entity.Property>();

                Entity.Property propertyTitle = new Entity.Property();
                propertyTitle.prop = "http://w3id.org/roh/exportProfile@@@http://w3id.org/roh/title";
                propertyTitle.values = new List<string>() { nombrePerfil };

                Entity.Property propertyChecks = new Entity.Property();
                propertyChecks.prop = "http://w3id.org/roh/exportProfile@@@http://w3id.org/roh/checkedItems";
                propertyChecks.values = checks;

                entity.properties.Add(propertyTitle);
                entity.properties.Add(propertyChecks);

                Entity entityBBDD = GetPerfilExportacion(pCVId, nombrePerfil);

                return UpdateEntityAux(mResourceApi.GetShortGuid(pCVId), new List<string>() { "http://w3id.org/roh/exportProfile" }, new List<string>() { entityBBDD.id }, entityBBDD, entity);
            }
            return false;
        }

        /// <summary>
        /// Elimina un perfil de exportación.
        /// </summary>
        /// <param name="pCVId">Identificador del curriculum vitae</param>
        /// <param name="title">titulo del perfil de exportación</param>
        /// <returns></returns>
        public bool DeletePerfilExportacion(string pCVId, string title)
        {
            string idRecurso = "";
            string select = "SELECT distinct ?exportProfile ?titulo";
            string where = $@"WHERE{{
    <{pCVId}> <http://w3id.org/roh/exportProfile> ?exportProfile .
    ?exportProfile <http://w3id.org/roh/title> '{title}' .
}}";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (fila.Any())
                {
                    idRecurso = fila["exportProfile"].value;
                }
            }

            Dictionary<Guid, List<RemoveTriples>> triplesToRemove = new Dictionary<Guid, List<RemoveTriples>>();
            List<RemoveTriples> listadoTriplesEliminar = new List<RemoveTriples>();
            listadoTriplesEliminar.Add(new RemoveTriples(idRecurso, "http://w3id.org/roh/exportProfile"));

            triplesToRemove.Add(mResourceApi.GetShortGuid(pCVId), listadoTriplesEliminar);
            return mResourceApi.DeletePropertiesLoadedResources(triplesToRemove).First().Value;
        }

        /// <summary>
        /// Dado el ID del curriculum devuelve el GNOSSID de ExportProfile
        /// </summary>
        /// <param name="pId">Id curriculum</param>
        /// <returns>GNOSSID de ExportProfile en caso de existir</returns>
        public static Entity GetPerfilExportacion(string pId, string nombrePerfil)
        {
            try
            {
                Entity entity = new Entity();
                entity.properties = new List<Entity.Property>();
                string selectID = "select ?o ?checkedItems";
                string whereID = $@"where{{
                                    <{pId}> <http://w3id.org/roh/exportProfile> ?o .
                                    ?o <http://w3id.org/roh/title> ?title FILTER(?title='{nombrePerfil}') .
                                    ?o <http://w3id.org/roh/checkedItems> ?checkItem .
                                }}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, "curriculumvitae");
                List<string> checkItems = new List<string>();

                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    checkItems.Add(fila["checkItem"].value);
                }

                throw new Exception("No existe la entidad http://w3id.org/roh/exportProfile");
            }
            catch (NullReferenceException e)
            {
                Console.Error.WriteLine("Errores al cargar mResourceApi " + e.Message);
                throw new NullReferenceException("Errores al cargar mResourceApi");
            }
        }


        /// <summary>
        /// Devuelve todas las pestañas del CV de <paramref name="pCVId"/>
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <returns></returns>
        public ConcurrentDictionary<string, string> GetAllTabs(string pCVId)
        {
            ConcurrentDictionary<string, string> dicIds = new ConcurrentDictionary<string, string>();
            string select = "SELECT *";
            string where = $@"WHERE{{
    <{pCVId}> ?p ?o .
}}";

            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (!fila.ContainsKey("p") || !fila.ContainsKey("o"))
                {
                    continue;
                }
                if (!IsValidTab(fila["p"].value))
                {
                    continue;
                }

                string property = fila["p"].value.Split("/").Last();
                string uri = fila["p"].value.Split(property).First();
                dicIds.TryAdd(FirstLetterUpper(uri, property), fila["o"].value);
            }

            return dicIds;
        }

        /// <summary>
        /// Devuelve true si <paramref name="tab"/> se encuentra en:
        /// "http://w3id.org/roh/personalData",
        /// "http://w3id.org/roh/scientificExperience",
        /// "http://w3id.org/roh/scientificActivity",
        /// "http://w3id.org/roh/teachingExperience",
        /// "http://w3id.org/roh/qualifications",
        /// "http://w3id.org/roh/professionalSituation",
        /// "http://w3id.org/roh/freeTextSummary"
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        private bool IsValidTab(string tab)
        {
            List<string> validTabs = new List<string>()
            {
                "http://w3id.org/roh/personalData",
                "http://w3id.org/roh/scientificExperience",
                "http://w3id.org/roh/scientificActivity",
                "http://w3id.org/roh/teachingExperience",
                "http://w3id.org/roh/qualifications",
                "http://w3id.org/roh/professionalSituation",
                "http://w3id.org/roh/freeTextSummary"
            };
            return validTabs.Contains(tab);
        }

        /// <summary>
        /// Cambia la 1º letra de <paramref name="property"/> a mayuscula y la concatena con <paramref name="uri"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private string FirstLetterUpper(string uri, string property)
        {
            if (property.Length == 0 || property.Length == 1)
            {
                return "";
            }
            string upper = property.Substring(0, 1).ToUpper();
            string substring = property.Substring(1, property.Length - 1);
            return uri + upper + substring;
        }

        /// <summary>
        /// Inserta en BBDD los triples de ExportProfile, indicando el titulo en <paramref name="propertyTitle"/> y los checks en <paramref name="propertyCheck"/>.
        /// </summary>
        /// <param name="pCvID">Identificador del curriculum vitae</param>
        /// <param name="propertyTitle">Property del titulo</param>
        /// <param name="propertyCheck">Property de los checks marcados</param>
        /// <returns>True si se inserta</returns>
        private bool IncludeTriples(string pCvID, Entity.Property propertyTitle, Entity.Property propertyCheck)
        {
            string idNewAux = $"{mResourceApi.GraphsUrl}items/ExportProfile_" + mResourceApi.GetShortGuid(pCvID) + "_" + Guid.NewGuid();

            List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();

            //Titulo
            string valorEntidadTitulo = idNewAux + "|" + propertyTitle.values.First();
            string predicadoEntidadTitulo = "http://w3id.org/roh/exportProfile|" + propertyTitle.prop;
            TriplesToInclude trTitle = new TriplesToInclude(valorEntidadTitulo, predicadoEntidadTitulo);
            listaTriples.Add(trTitle);

            //CheckedItems
            foreach (string value in propertyCheck.values)
            {
                string valorEntidadCheck = idNewAux + "|" + value;
                string predicadoEntidadCheck = "http://w3id.org/roh/exportProfile|" + propertyCheck.prop;
                TriplesToInclude trCheck = new TriplesToInclude(valorEntidadCheck, predicadoEntidadCheck);
                listaTriples.Add(trCheck);
            }

            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>()
            {
                {
                    mResourceApi.GetShortGuid(pCvID), listaTriples
                }
            };

            bool response = mResourceApi.InsertPropertiesLoadedResources(triplesToInclude).First().Value;
            return response;
        }

        /// <summary>
        /// Fusiona dos entidades
        /// </summary>
        /// <param name="pIdMainEntity">Identificador de la entidad a la que pertenece la entidad auxiliar</param>
        /// <param name="pPropertyIDs">Propiedades que apuntan a la auxiliar</param>
        /// <param name="pEntityIDs">Entidades que apuntan a la auxiliar</param>
        /// <param name="pLoadedEntity">Entidad cargada en BBDD</param>
        /// <param name="pUpdatedEntity">Nueva entidad</param>
        /// <returns>true si se ha actualizado correctamente</returns>
        private bool UpdateEntityAux(Guid pIdMainEntity, List<string> pPropertyIDs, List<string> pEntityIDs, Entity pLoadedEntity, Entity pUpdatedEntity)
        {
            bool update = true;
            Dictionary<Guid, List<TriplesToInclude>> triplesInclude = new Dictionary<Guid, List<TriplesToInclude>>() { { pIdMainEntity, new List<TriplesToInclude>() } };
            Dictionary<Guid, List<RemoveTriples>> triplesRemove = new Dictionary<Guid, List<RemoveTriples>>() { { pIdMainEntity, new List<RemoveTriples>() } };
            Dictionary<Guid, List<TriplesToModify>> triplesModify = new Dictionary<Guid, List<TriplesToModify>>() { { pIdMainEntity, new List<TriplesToModify>() } };

            foreach (Entity.Property property in pUpdatedEntity.properties)
            {
                bool remove = property.values == null || property.values.Count == 0 || !property.values.Exists(x => !string.IsNullOrEmpty(x));
                //Recorremos las propiedades de la entidad a actualizar y modificamos la entidad recuperada de BBDD               
                Entity.Property propertyLoadedEntity = null;
                if (pLoadedEntity != null)
                {
                    propertyLoadedEntity = pLoadedEntity.properties.FirstOrDefault(x => x.prop == property.prop);
                }
                if (propertyLoadedEntity != null)
                {
                    if (remove)
                    {
                        foreach (string valor in propertyLoadedEntity.values)
                        {
                            triplesRemove[pIdMainEntity].Add(new RemoveTriples()
                            {
                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                            });
                        }
                    }
                    else
                    {
                        HashSet<string> items = new HashSet<string>();
                        foreach (string valor in propertyLoadedEntity.values)
                        {
                            items.Add(GetEntityOfValue(valor));
                        }
                        foreach (string valor in property.values)
                        {
                            items.Add(GetEntityOfValue(valor));
                        }

                        foreach (string item in items)
                        {
                            List<string> valuesLoadedEntity = propertyLoadedEntity.values.Where(x => GetEntityOfValue(x) == item).ToList();
                            List<string> valuesEntity = property.values.Where(x => GetEntityOfValue(x) == item).ToList();
                            int numLoaded = valuesLoadedEntity.Count;
                            int numNew = valuesEntity.Count;
                            int numIntersect = valuesLoadedEntity.Intersect(valuesEntity).Count();
                            if (numLoaded != numNew || numLoaded != numIntersect)
                            {
                                if (numLoaded == 1 && numNew == 1)
                                {
                                    triplesModify[pIdMainEntity].Add(new TriplesToModify()
                                    {

                                        Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                        NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valuesEntity[0]),
                                        OldValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valuesLoadedEntity[0])
                                    });
                                }
                                else
                                {
                                    //Eliminaciones
                                    foreach (string valor in valuesLoadedEntity.Except(property.values))
                                    {
                                        triplesRemove[pIdMainEntity].Add(new RemoveTriples()
                                        {

                                            Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                            Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                                        });
                                    }
                                    //Inserciones
                                    foreach (string valor in valuesEntity.Except(valuesLoadedEntity))
                                    {
                                        if (!valor.EndsWith("@@@"))
                                        {
                                            triplesInclude[pIdMainEntity].Add(new TriplesToInclude()
                                            {

                                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                                NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (!remove)
                {
                    foreach (string valor in property.values)
                    {
                        if (!valor.EndsWith("@@@"))
                        {
                            triplesInclude[pIdMainEntity].Add(new TriplesToInclude()
                            {

                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                            });
                        }
                    }
                }
                else if (remove)
                {
                    List<Entity.Property> propertiesLoadedEntityRemove = pLoadedEntity.properties.Where(x => x.prop.StartsWith(property.prop)).ToList();
                    foreach (Entity.Property propertyToRemove in propertiesLoadedEntityRemove)
                    {
                        foreach (string valor in propertyToRemove.values)
                        {
                            triplesRemove[pIdMainEntity].Add(new RemoveTriples()
                            {

                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                            });
                        }
                    }
                }
            }
            if (pUpdatedEntity.auxEntityRemove != null)
            {
                foreach (string auxEntityRemove in pUpdatedEntity.auxEntityRemove)
                {
                    if (auxEntityRemove.StartsWith("http"))
                    {
                        foreach (Entity.Property property in pLoadedEntity.properties)
                        {
                            foreach (string valor in property.values)
                            {
                                if (valor.Contains(auxEntityRemove))
                                {
                                    //Elmiminamos de la lista a aliminar los hijos de la entidad a eliminar
                                    triplesRemove[pIdMainEntity].RemoveAll(x => x.Value.Contains(auxEntityRemove));
                                    //Eliminamos la entidad auxiliar
                                    triplesRemove[pIdMainEntity].Add(new RemoveTriples()
                                    {

                                        Predicate = string.Join("|", pPropertyIDs) + "|" + property.prop.Substring(0, property.prop.IndexOf("@@@")),
                                        Value = string.Join("|", pEntityIDs) + "|" + auxEntityRemove
                                    });
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (triplesRemove[pIdMainEntity].Count > 0)
            {
                update = update && mResourceApi.DeletePropertiesLoadedResources(triplesRemove)[pIdMainEntity];
            }
            if (triplesInclude[pIdMainEntity].Count > 0)
            {
                update = update && mResourceApi.InsertPropertiesLoadedResources(triplesInclude)[pIdMainEntity];
            }
            if (triplesModify[pIdMainEntity].Count > 0)
            {
                update = update && mResourceApi.ModifyPropertiesLoadedResources(triplesModify)[pIdMainEntity];
            }
            return update;
        }

        /// <summary>
        /// Transforma la propiedad para su carga en una entiadad auxiliar
        /// </summary>
        /// <param name="pProp">Propiedad</param>
        /// <returns></returns>
        private string GetPropUpdateEntityAux(string pProp)
        {
            while (pProp.Contains("@@@"))
            {
                int indexInitRdfType = pProp.IndexOf("@@@");
                int indexEndRdfType = pProp.IndexOf("|", indexInitRdfType);
                if (indexEndRdfType > indexInitRdfType)
                {
                    pProp = pProp.Substring(0, indexInitRdfType) + pProp.Substring(indexEndRdfType);
                }
                else
                {
                    pProp = pProp.Substring(0, indexInitRdfType);
                }
            }
            return pProp;
        }

        /// <summary>
        /// Transforma el valor de la propiedad para su carga en una entiadad auxiliar
        /// </summary>
        /// <param name="pValue">Valor</param>
        /// <returns></returns>
        private string GetValueUpdateEntityAux(string pValue)
        {
            return pValue.Replace("@@@", "|");
        }

        /// <summary>
        /// Obtiene la entidad del valor
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        private string GetEntityOfValue(string pValue)
        {
            string entityID = "";
            if (pValue.Contains("@@@"))
            {
                entityID = pValue.Substring(0, pValue.IndexOf("@@@"));
            }
            return entityID;
        }

        /// <summary>
        /// Obtiene todos los datos de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns>Entidad completa</returns>
        public Entity GetLoadedEntity(string pId, string pGraph)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            try
            {
                int numLimit = 10000;
                int offset = 0;
                bool cargar = true;
                while (cargar)
                {
                    string selectID = "select * where{ select distinct ?s ?p ?o";
                    string whereID = $"where{{?x <http://gnoss/hasEntidad> <{pId}> . ?x <http://gnoss/hasEntidad> ?s . ?s ?p ?o }}order by desc(?s) desc(?p) desc(?o)}} limit {numLimit} offset {offset}";
                    SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
                    foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                    {
                        if (!listResult.ContainsKey(fila["s"].value))
                        {
                            listResult.Add(fila["s"].value, new List<Dictionary<string, Data>>());
                        }
                        listResult[fila["s"].value].Add(fila);
                    }
                    offset += numLimit;
                    if (resultData.results.bindings.Count < numLimit)
                    {
                        cargar = false;
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            if (listResult.Count > 0 && listResult.ContainsKey(pId))
            {
                Entity entity = new Entity()
                {
                    id = pId,
                    ontology = pGraph,
                    rdfType = listResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value,
                    properties = new List<Entity.Property>()
                };
                GetLoadedEntity(pId, "", "", ref entity, listResult);
                return entity;
            }
            return null;
        }

        /// <summary>
        /// Procesa una entidad auxiliar para crear el ComplexOntologyResource
        /// </summary>
        /// <param name="pProperty">Propiedad que apunta a la entidad auxiliar</param>
        /// <param name="pEntidadAuxiliar">Entidad auxiliar</param>
        /// <returns></returns>
        private OntologyEntity ProcesarEntidadAuxiliar(string pProperty, EntityRdf pEntidadAuxiliar)
        {
            List<string> prefList = new List<string>();
            foreach (string key in UtilityCV.dicPrefix.Keys)
            {
                prefList.Add($"xmlns:{key}=\"{UtilityCV.dicPrefix[key]}\"");
            }

            List<OntologyEntity> entList = new List<OntologyEntity>();
            List<OntologyProperty> propList = new List<OntologyProperty>();

            foreach (string prop in pEntidadAuxiliar.ents.Keys)
            {
                foreach (EntityRdf entity in pEntidadAuxiliar.ents[prop])
                {
                    entList.Add(ProcesarEntidadAuxiliar(prop, entity));
                }
            }

            //Creamos la entidad auxiliar
            foreach (string prop in pEntidadAuxiliar.props.Keys)
            {
                foreach (string value in pEntidadAuxiliar.props[prop])
                {
                    propList.Add(new StringOntologyProperty(prop, value));
                }
            }
            OntologyEntity ontologyEntity = new OntologyEntity(pEntidadAuxiliar.rdfType, pEntidadAuxiliar.rdfType, UtilityCV.AniadirPrefijo(pProperty.Split('|').Last()), propList, entList);
            return ontologyEntity;
        }

        /// <summary>
        /// Carga los datos en el objeto entidad con los datos obtenidos
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pPropAcumulado">Propiedad acumulada</param>
        /// <param name="pObjAcumulado">Objeto acumulado</param>
        /// <param name="pEntity">Entidad</param>
        /// <param name="pListResult">Datos de BBDD</param>
        private void GetLoadedEntity(string pId, string pPropAcumulado, string pObjAcumulado, ref Entity pEntity, Dictionary<string, List<Dictionary<string, Data>>> pListResult)
        {
            foreach (Dictionary<string, Data> prop in pListResult[pId])
            {
                string s = prop["s"].value;
                string p = prop["p"].value;
                string o = prop["o"].value;

                string rdfType = pListResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value;
                if (s == pId && p != "http://www.w3.org/2000/01/rdf-schema#label" && p != "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                {
                    string pPropAcumuladoAux = pPropAcumulado;
                    if (!string.IsNullOrEmpty(pPropAcumulado))
                    {
                        pPropAcumuladoAux += "@@@" + rdfType + "|";
                    }
                    pPropAcumuladoAux += p;
                    string pObjAcumuladoAux = pObjAcumulado;
                    if (!string.IsNullOrEmpty(pObjAcumulado))
                    {
                        pObjAcumuladoAux += "@@@";
                    }
                    pObjAcumuladoAux += o;
                    if (pListResult.ContainsKey(o))
                    {
                        GetLoadedEntity(o, pPropAcumuladoAux, pObjAcumuladoAux, ref pEntity, pListResult);
                    }
                    else
                    {
                        Entity.Property property = pEntity.properties.FirstOrDefault(x => x.prop == pPropAcumuladoAux);
                        if (property == null)
                        {
                            Entity.Property propiedad = new Entity.Property();
                            propiedad.prop = pPropAcumuladoAux;
                            propiedad.values = new List<string>();

                            property = propiedad;
                            pEntity.properties.Add(property);
                        }
                        property.values.Add(pObjAcumuladoAux);
                    }
                }
            }
        }


    }
}
