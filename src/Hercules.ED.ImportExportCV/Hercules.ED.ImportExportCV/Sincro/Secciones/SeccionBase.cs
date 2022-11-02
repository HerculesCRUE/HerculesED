using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Hercules.ED.ImportExportCV.Controllers;
using Hercules.ED.ImportExportCV.Models;
using Models;
using Models.NotificationOntology;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones
{
    public abstract class SeccionBase
    {
        protected static readonly ResourceApi mResourceApi = new($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        protected cvnRootResultBean mCvn { get; set; }
        protected string mCvID { get; set; }
        protected string mPersonID { get; set; }

        readonly ConfigService mConfiguracion;


        protected SeccionBase(cvnRootResultBean cvn, string cvID, ConfigService configuracion)
        {
            mCvn = cvn;
            mCvID = cvID;
            mConfiguracion = configuracion;
        }
        protected SeccionBase(cvnRootResultBean cvn, string cvID, string personID, ConfigService configuracion)
        {
            mCvn = cvn;
            mCvID = cvID;
            mPersonID = personID;
            mConfiguracion = configuracion;
        }

        /// <summary>
        /// Obtiene todos los datos de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns>Entidad completa</returns>
        public Entity GetLoadedEntity(string pId, string pGraph)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new();
            try
            {
                int numLimit = 10000;
                int offset = 0;
                while (true)
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
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
            }

            if (listResult.Count > 0 && listResult.ContainsKey(pId))
            {
                Entity entity = new()
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
                            property = new Entity.Property(pPropAcumuladoAux, new List<string>());
                            pEntity.properties.Add(property);
                        }
                        property.values.Add(pObjAcumuladoAux);
                    }
                }
            }
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
        protected bool UpdateEntityAux(Guid pIdMainEntity, List<string> pPropertyIDs, List<string> pEntityIDs, Entity pLoadedEntity, Entity pUpdatedEntity)
        {
            bool update = true;
            Dictionary<Guid, List<TriplesToInclude>> triplesInclude = new() { { pIdMainEntity, new List<TriplesToInclude>() } };
            Dictionary<Guid, List<RemoveTriples>> triplesRemove = new() { { pIdMainEntity, new List<RemoveTriples>() } };
            Dictionary<Guid, List<TriplesToModify>> triplesModify = new() { { pIdMainEntity, new List<TriplesToModify>() } };

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
                        HashSet<string> items = new();
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
                else if (pLoadedEntity != null)
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
                    if (auxEntityRemove.StartsWith("http") && pLoadedEntity != null)
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

                                        Predicate = string.Concat(string.Join("|", pPropertyIDs), "|", property.prop.AsSpan(0, property.prop.IndexOf("@@@"))),
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
                update = mResourceApi.DeletePropertiesLoadedResources(triplesRemove)[pIdMainEntity];
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
        /// Añade o modifica las propiedades de las entidades en BBDD comparandolas con las leidas del XML.
        /// </summary>
        private void AniadirModificar(List<Entity> listadoAux, Dictionary<string, DisambiguableEntity> entidadesXML,
            Dictionary<string, string> equivalencias, string propTitle, string graph, string rdfType, string rdfTypePrefix,
            List<string> propiedadesItem, string RdfTypeTab, [Optional] string pPropertyCV, [Optional] string pRdfTypeCV, [Optional] List<string> listadoIdBBDD)
        {
            HashSet<string> itemsNuevosOModificados = new();
            for (int i = 0; i < listadoAux.Count; i++)
            {
                Entity entityXML = listadoAux[i];
                string idBBDD;

                if (listadoIdBBDD != null && listadoIdBBDD.Count > 0)
                {
                    idBBDD = listadoIdBBDD.ElementAt(i).Split("@@@").First();
                    if (string.IsNullOrEmpty(idBBDD))
                    {
                        idBBDD = listadoIdBBDD.ElementAt(i).Split("@@@").Last();
                    }
                    //Duplicar
                    if (listadoIdBBDD.ElementAt(i).Split("@@@").Last().Equals("du"))
                    {
                        //Añadir
                        entityXML.propTitle = propTitle;
                        entityXML.ontology = graph;
                        entityXML.rdfType = rdfType;
                        idBBDD = CreateListEntityAux(mCvID, RdfTypeTab, rdfTypePrefix, propiedadesItem, entityXML);
                        listadoAux.RemoveAt(i);
                        listadoIdBBDD.RemoveAt(i);
                        i--;
                    }
                    //Fusionar
                    else if (listadoIdBBDD.ElementAt(i).Split("@@@").Last().Equals("fu") && !string.IsNullOrEmpty(idBBDD))
                    {
                        ModificarExistentes(idBBDD, graph, propTitle, entityXML);
                        listadoAux.RemoveAt(i);
                        listadoIdBBDD.RemoveAt(i);
                        i--;
                    }
                    //Sobrescribir
                    else if (listadoIdBBDD.ElementAt(i).Split("@@@").Last().Equals("so") && !string.IsNullOrEmpty(idBBDD))
                    {
                        SobrescribirExistentes(idBBDD, graph, propTitle, entityXML);
                        listadoAux.RemoveAt(i);
                        listadoIdBBDD.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    string idXML = entidadesXML.Keys.ToList()[i];

                    if (string.IsNullOrEmpty(equivalencias[idXML]))
                    {
                        //Añadir
                        entityXML.propTitle = propTitle;
                        entityXML.ontology = graph;
                        entityXML.rdfType = rdfType;
                        idBBDD = CreateListEntityAux(mCvID, RdfTypeTab, rdfTypePrefix, propiedadesItem, entityXML);
                    }
                    else
                    {
                        //Modificar
                        ModificarExistentes(equivalencias, idXML, graph, propTitle, entityXML);
                        //Añadimos la referencia a BBDD para usarla en caso de añadir elementos en el CV.
                        idBBDD = equivalencias[idXML];
                    }
                }

                if (!string.IsNullOrEmpty(idBBDD))
                {
                    itemsNuevosOModificados.Add(idBBDD);
                    if (!string.IsNullOrEmpty(pPropertyCV) && !string.IsNullOrEmpty(pRdfTypeCV))
                    {
                        valoresPropertiesCV(entityXML, propiedadesItem, pPropertyCV, pRdfTypeCV, idBBDD, RdfTypeTab);
                    }
                }
            }

            //Insertamos en la cola del desnormalizador            
            if (itemsNuevosOModificados.Count > 0)
            {
                RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new(mConfiguracion);
                if (rdfType == "http://vivoweb.org/ontology/core#Project")
                {
                    rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.project, itemsNuevosOModificados));
                }
                if (rdfType == "http://xmlns.com/foaf/0.1/Group")
                {
                    rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.group, itemsNuevosOModificados));
                }
            }
        }

        /// <summary>
        /// Devuelve un listado con los datos de las subsecciones o añade los datos en BBDD.
        /// </summary>
        /// <param name="preimportar"></param>
        /// <param name="listadoAux"></param>
        /// <param name="entidadesXML"></param>
        /// <param name="equivalencias"></param>
        /// <param name="propTitle"></param>
        /// <param name="graph"></param>
        /// <param name="rdfType"></param>
        /// <param name="rdfTypePrefix"></param>
        /// <param name="propiedadesItem"></param>
        /// <returns></returns>
        protected List<SubseccionItem> CheckPreimportar(bool preimportar, List<Entity> listadoAux, Dictionary<string, DisambiguableEntity> entidadesXML,
            Dictionary<string, string> equivalencias, string propTitle, string graph, string rdfType, string rdfTypePrefix,
            List<string> propiedadesItem, string RdfTypeTab, List<bool> listadoBloqueados,
            [Optional] string pPropertyCV, [Optional] string pRdfTypeCV, [Optional] bool propertiesCV, [Optional] List<string> listadoIdBBDD,
            [Optional] PetitionStatus petitionStatus)
        {
            if (preimportar)
            {
                if (listadoBloqueados.Count != equivalencias.Count)
                {
                    throw new ArgumentException("El listado de items bloqueados y el listado de equivalencias no concuerdan");
                }

                if (petitionStatus != null)
                {
                    petitionStatus.actualSubWorks = 1;
                    petitionStatus.actualSubTotalWorks = equivalencias.Count;
                }

                if (propertiesCV)
                {
                    List<SubseccionItem> listaAux = new();
                    for (int i = 0; i < equivalencias.Count; i++)
                    {
                        if (petitionStatus != null)
                        {
                            petitionStatus.actualSubWorks++;
                        }
                        listaAux.Add(new SubseccionItem(i, equivalencias.Values.ElementAt(i), listadoAux.ElementAt(i).properties, listadoAux.ElementAt(i).properties_cv, listadoBloqueados.ElementAt(i)));
                    }
                    return listaAux;
                }
                else
                {
                    List<SubseccionItem> listaAux = new();
                    for (int i = 0; i < equivalencias.Count; i++)
                    {
                        if (petitionStatus != null)
                        {
                            petitionStatus.actualSubWorks++;
                        }
                        listaAux.Add(new SubseccionItem(i, equivalencias.Values.ElementAt(i), listadoAux.ElementAt(i).properties, listadoBloqueados.ElementAt(i)));
                    }
                    return listaAux;
                }
            }
            else
            {
                //4º Añadimos o modificamos las entidades
                AniadirModificar(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, pPropertyCV, pRdfTypeCV, listadoIdBBDD);
                return null;
            }
        }

        /// <summary>
        /// Añade o modifica las publicaciones y los autores pertenecientes a las mismas.
        /// </summary>
        /// <param name="listadoAux"></param>
        /// <param name="equivalencias"></param>
        /// <param name="propTitle"></param>
        /// <param name="graph"></param>
        /// <param name="rdfType"></param>
        /// <param name="rdfTypePrefix"></param>
        /// <param name="propiedadesItem"></param>
        /// <param name="RdfTypeTab"></param>
        /// <param name="pPropertyCV"></param>
        /// <param name="pRdfTypeCV"></param>
        protected void AniadirModificarPublicaciones(List<Entity> listadoAux, Dictionary<string, HashSet<string>> equivalencias, string propTitle,
            string graph, string rdfType, string rdfTypePrefix, List<string> propiedadesItem, string RdfTypeTab,
            [Optional] string pPropertyCV, [Optional] string pRdfTypeCV, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Diccionario para almacenar las notificaciones
            ConcurrentBag<Notification> notificaciones = new();

            //Listados para añadir las personas y documentos a desnormalizar
            HashSet<string> personasDesnormalizar = new();
            HashSet<string> documentosDesnormalizar = new();

            //Obtengo los datos de la persona para comprobar que existe en los documentos que cargamos
            Dictionary<string, string> idPersonaNick = UtilitySecciones.ObtenerIdPersona(mResourceApi, mCvID);
            string idPersona = idPersonaNick.ElementAt(0).Key;
            string nickPersona = idPersonaNick.ElementAt(0).Value;

            if (petitionStatus != null)
            {
                petitionStatus.actualWorkSubtitle = "PREPARANDO_PERSONAS_PUBLICACIONES";
                petitionStatus.actualSubWorks = 1;
                petitionStatus.actualSubTotalWorks = 1;
            }

            //1º cargar todas las personas que no estén cargadas
            List<ComplexOntologyResource> personasCargar = new();
            HashSet<string> personasAniadidas = new();
            Dictionary<string, string> identificadoresPersonasAniadidas = new();
            mResourceApi.ChangeOntoly("person");
            for (int i = 0; i < listadoAux.Count; i++)
            {
                Entity entityXML = listadoAux[i];
                foreach (Persona persona in entityXML.autores)
                {
                    string idTemporal = equivalencias.First(x => x.Value.Select(x => x.Split('|')[1]).Contains(persona.ID)).Key;
                    if (Guid.TryParse(idTemporal, out Guid aux) && personasAniadidas.Add(idTemporal))
                    {
                        Entity entidadPersona = new();
                        entidadPersona.rdfType = "http://xmlns.com/foaf/0.1/Person";
                        entidadPersona.ontology = "person";
                        entidadPersona.propTitle = "http://xmlns.com/foaf/0.1/name";
                        entidadPersona.properties = new List<Entity.Property>()
                            {
                                new Entity.Property()
                                {
                                    prop = "http://xmlns.com/foaf/0.1/name",
                                    values = new List<string>() { persona.nombreCompleto?.Trim() }
                                },
                                new Entity.Property()
                                {
                                    prop = "http://xmlns.com/foaf/0.1/firstName",
                                    values = new List<string>() { persona.nombre?.Trim() }
                                },
                                new Entity.Property()
                                {
                                    prop = "http://xmlns.com/foaf/0.1/lastName",
                                    values = new List<string>() { (persona.primerApellido?.Trim()+" "+ persona.segundoApellido?.Trim()).Trim() }
                                }
                            };
                        entidadPersona.id = mResourceApi.GraphsUrl + "items/Person_" + Guid.NewGuid().ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                        identificadoresPersonasAniadidas[idTemporal] = entidadPersona.id;

                        ComplexOntologyResource resource = ToGnossApiResource(entidadPersona);
                        personasCargar.Add(resource);

                    }
                }
            }

            //2º modificar todas las personas en los docs para que apunte a la entidad correspondiente
            string BFO_comment = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://www.w3.org/1999/02/22-rdf-syntax-ns#comment";
            string BFO_member = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://www.w3.org/1999/02/22-rdf-syntax-ns#member";
            string BFO_nick = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/nick";

            //Añado las propiedades de los autores a los documentos para su carga
            for (int i = 0; i < listadoAux.Count; i++)
            {
                Entity entityXML = listadoAux[i];

                Entity.Property propertyBFO_comment = new()
                {
                    prop = BFO_comment
                };
                Entity.Property propertyBFO_member = new()
                {
                    prop = BFO_member
                };
                Entity.Property propertyBFO_nick = new()
                {
                    prop = BFO_nick
                };

                entityXML.properties.Add(propertyBFO_comment);
                entityXML.properties.Add(propertyBFO_member);
                entityXML.properties.Add(propertyBFO_nick);

                //Añado los autores del documento
                int contador = 1;
                foreach (Persona persona in entityXML.autores)
                {
                    string idPersonaBBDD = equivalencias.First(x => x.Value.Select(x => x.Split('|').Last()).Contains(persona.ID)).Key;
                    if (Guid.TryParse(idPersonaBBDD, out Guid aux))
                    {
                        idPersonaBBDD = identificadoresPersonasAniadidas[idPersonaBBDD];
                    }

                    string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                    UtilitySecciones.CheckProperty(propertyBFO_comment, entityXML, UtilitySecciones.StringGNOSSID(entityPartAux, contador.ToString()), BFO_comment);
                    UtilitySecciones.CheckProperty(propertyBFO_member, entityXML, UtilitySecciones.StringGNOSSID(entityPartAux, idPersonaBBDD), BFO_member);
                    UtilitySecciones.CheckProperty(propertyBFO_nick, entityXML, UtilitySecciones.StringGNOSSID(entityPartAux, persona.firma), BFO_nick);
                    contador++;
                }

                //Si el documento no contiene al autor del CV lo añado
                if (!entityXML.properties.Where(x => x.prop.Equals(BFO_member)).SelectMany(x => x.values).Any(x => x.Split("@@@")[1].Equals(idPersona)))
                {
                    string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                    UtilitySecciones.CheckProperty(propertyBFO_comment, entityXML, UtilitySecciones.StringGNOSSID(entityPartAux, contador.ToString()), BFO_comment);
                    UtilitySecciones.CheckProperty(propertyBFO_member, entityXML, UtilitySecciones.StringGNOSSID(entityPartAux, idPersona), BFO_member);
                    UtilitySecciones.CheckProperty(propertyBFO_nick, entityXML, UtilitySecciones.StringGNOSSID(entityPartAux, nickPersona), BFO_nick);
                }
            }

            //Compruebo las equivalencias entre los documentos
            Dictionary<string, string> equivalenciasDocumentos = new();
            for (int i = 0; i < listadoAux.Count; i++)
            {
                Entity entityXML = listadoAux[i];
                string idXML = entityXML.id;
                equivalenciasDocumentos[idXML] = "";
                string idDesambiguacion = equivalencias.First(x => x.Value.Select(x => x.Split('|')[1]).Contains(idXML)).Key;
                if (!Guid.TryParse(idDesambiguacion, out Guid aux))
                {
                    equivalenciasDocumentos[idXML] = idDesambiguacion;
                }
            }


            if (petitionStatus != null)
            {
                petitionStatus.actualWorkSubtitle = "IMPORTANDO_PUBLICACIONES";
                petitionStatus.actualSubWorks = 1;
                petitionStatus.actualSubTotalWorks = listadoAux.Count;
            }

            //Añado las personas que se usan en los documentos, si se añade o puede modificar el documento.
            HashSet<string> personasUsadas = new();
            for (int i = 0; i < listadoAux.Count; i++)
            {
                if (petitionStatus != null)
                {
                    petitionStatus.actualSubWorks++;
                }

                Entity entityXML = listadoAux[i];

                string idXML = entityXML.id;
                string idBBDD = "";
                bool modificado = false;
                //Si el listadoIdBBDD no es nulo es que viene de PostImportar
                if (listadoIdBBDD != null)
                {
                    string opcion = listadoIdBBDD.ElementAt(i).Split("@@@").Last();
                    if (opcion.Equals("du"))
                    {
                        //Añadir
                        entityXML.id = "";
                        entityXML.propTitle = propTitle;
                        entityXML.ontology = graph;
                        entityXML.rdfType = rdfType;
                        idBBDD = CreateListEntityAux(mCvID, RdfTypeTab, rdfTypePrefix, propiedadesItem, entityXML);
                        if (!string.IsNullOrEmpty(idBBDD))
                        {
                            modificado = true;
                            //Notificar
                            if (entityXML.autores != null)
                            {
                                foreach (Persona autor in entityXML.autores)
                                {
                                    //No notifico a quien suben el documento
                                    if (equivalencias.ContainsKey(idPersona))
                                    {
                                        string idPersonaAux = equivalencias[idPersona].First();
                                        if (autor.ID == idPersonaAux.Split("|").Last())
                                        {
                                            continue;
                                        }
                                    }
                                    //Compruebo que la persona propietaria de la notificación está en BBDD
                                    if (!equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key.Contains('_'))
                                    {
                                        continue;
                                    }

                                    Notification notificacion = new();
                                    notificacion.IdRoh_trigger = idPersona;
                                    notificacion.Roh_tabPropertyCV = "http://w3id.org/roh/scientificActivity";
                                    notificacion.Roh_entity = idBBDD;
                                    notificacion.IdRoh_owner = equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key;
                                    notificacion.Dct_issued = DateTime.Now;
                                    notificacion.Roh_type = "create";
                                    notificacion.CvnCode = UtilityCV.IdentificadorFECYT(entityXML.properties
                                        .Where(x => x.prop.Equals("http://w3id.org/roh/scientificActivityDocument")).SelectMany(x => x.values).FirstOrDefault());

                                    notificaciones.Add(notificacion);
                                }
                            }
                        }

                        //Elimino el objeto tratado
                        listadoAux.RemoveAt(i);
                        listadoIdBBDD.RemoveAt(i);
                        i--;
                    }
                    if (opcion.Equals("fu"))
                    {
                        //Fusionar
                        idBBDD = listadoIdBBDD.ElementAt(i).Split("@@@").First();
                        modificado = ModificarExistentes(idBBDD, graph, propTitle, entityXML);

                        //Notificar
                        if (entityXML.autores != null)
                        {
                            foreach (Persona autor in entityXML.autores)
                            {
                                //No notifico a quien suben el documento
                                if (equivalencias.ContainsKey(idPersona))
                                {
                                    string idPersonaAux = equivalencias[idPersona].First();
                                    if (autor.ID == idPersonaAux.Split("|").Last())
                                    {
                                        continue;
                                    }
                                }
                                //Compruebo que la persona propietaria de la notificación está en BBDD
                                if (!equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key.Contains('_'))
                                {
                                    continue;
                                }

                                Notification notificacion = new();
                                notificacion.IdRoh_trigger = idPersona;
                                notificacion.Roh_tabPropertyCV = "http://w3id.org/roh/scientificActivity";
                                notificacion.Roh_entity = idBBDD;
                                notificacion.IdRoh_owner = equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key;
                                notificacion.Dct_issued = DateTime.Now;
                                notificacion.Roh_type = "edit";
                                notificacion.CvnCode = UtilityCV.IdentificadorFECYT(entityXML.properties
                                    .Where(x => x.prop.Equals("http://w3id.org/roh/scientificActivityDocument")).SelectMany(x => x.values).FirstOrDefault());

                                notificaciones.Add(notificacion);
                            }
                        }

                        //Elimino el objeto tratado
                        listadoAux.RemoveAt(i);
                        listadoIdBBDD.RemoveAt(i);
                        i--;
                    }
                    if (opcion.Equals("so"))
                    {
                        //Sobrescribir
                        idBBDD = listadoIdBBDD.ElementAt(i).Split("@@@").First();
                        modificado = SobrescribirExistentes(idBBDD, graph, propTitle, entityXML);

                        //Notificar
                        if (entityXML.autores != null)
                        {
                            foreach (Persona autor in entityXML.autores)
                            {
                                //No notifico a quien suben el documento
                                if (equivalencias.ContainsKey(idPersona))
                                {
                                    string idPersonaAux = equivalencias[idPersona].First();
                                    if (autor.ID == idPersonaAux.Split("|").Last())
                                    {
                                        continue;
                                    }
                                }
                                //Compruebo que la persona propietaria de la notificación está en BBDD
                                if (!equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key.Contains('_'))
                                {
                                    continue;
                                }

                                Notification notificacion = new();
                                notificacion.IdRoh_trigger = idPersona;
                                notificacion.Roh_tabPropertyCV = "http://w3id.org/roh/scientificActivity";
                                notificacion.Roh_entity = idBBDD;
                                notificacion.IdRoh_owner = equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key;
                                notificacion.Dct_issued = DateTime.Now;
                                notificacion.Roh_type = "edit";
                                notificacion.CvnCode = UtilityCV.IdentificadorFECYT(entityXML.properties
                                    .Where(x => x.prop.Equals("http://w3id.org/roh/scientificActivityDocument")).SelectMany(x => x.values).FirstOrDefault());

                                notificaciones.Add(notificacion);
                            }
                        }

                        //Elimino el objeto tratado
                        listadoAux.RemoveAt(i);
                        listadoIdBBDD.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(equivalenciasDocumentos[idXML]))
                    {
                        //Añadir
                        entityXML.id = "";
                        entityXML.propTitle = propTitle;
                        entityXML.ontology = graph;
                        entityXML.rdfType = rdfType;
                        idBBDD = CreateListEntityAux(mCvID, RdfTypeTab, rdfTypePrefix, propiedadesItem, entityXML);
                        if (!string.IsNullOrEmpty(idBBDD))
                        {
                            modificado = true;
                            //Notificar
                            foreach (Persona autor in entityXML.autores)
                            {
                                //No notifico a quien suben el documento
                                if (autor.personid == idPersona)
                                {
                                    continue;
                                }
                                //Compruebo que la persona propietaria de la notificación está en BBDD
                                if (!equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key.Contains('_'))
                                {
                                    continue;
                                }

                                Notification notificacion = new();
                                notificacion.IdRoh_trigger = idPersona;
                                notificacion.Roh_tabPropertyCV = "http://w3id.org/roh/scientificActivity";
                                notificacion.Roh_entity = idBBDD;
                                notificacion.IdRoh_owner = equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key;
                                notificacion.Dct_issued = DateTime.Now;
                                notificacion.Roh_type = "create";
                                notificacion.CvnCode = UtilityCV.IdentificadorFECYT(entityXML.properties
                                    .Where(x => x.prop.Equals("http://w3id.org/roh/scientificActivityDocument")).SelectMany(x => x.values).FirstOrDefault());

                                notificaciones.Add(notificacion);
                            }
                        }
                    }
                    else
                    {
                        //Modificar
                        modificado = ModificarExistentes(equivalenciasDocumentos, idXML, graph, propTitle, entityXML);

                        //Añadimos la referencia a BBDD para usarla en caso de añadir elementos en el CV.
                        idBBDD = equivalenciasDocumentos[idXML];

                        //Notificar
                        foreach (Persona autor in entityXML.autores)
                        {
                            //No notifico a quien suben el documento
                            if (autor.personid == idPersona)
                            {
                                continue;
                            }
                            //Compruebo que la persona propietaria de la notificación está en BBDD
                            if (!equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key.Contains('_'))
                            {
                                continue;
                            }

                            Notification notificacion = new();
                            notificacion.IdRoh_trigger = idPersona;
                            notificacion.Roh_tabPropertyCV = "http://w3id.org/roh/scientificActivity";
                            notificacion.Roh_entity = idBBDD;
                            notificacion.IdRoh_owner = equivalencias.FirstOrDefault(x => x.Value.Any(y => y.Split('|')[1].Equals(autor.ID))).Key;
                            notificacion.Dct_issued = DateTime.Now;
                            notificacion.Roh_type = "edit";
                            notificacion.CvnCode = UtilityCV.IdentificadorFECYT(entityXML.properties
                                .Where(x => x.prop.Equals("http://w3id.org/roh/scientificActivityDocument")).SelectMany(x => x.values).FirstOrDefault());

                            notificaciones.Add(notificacion);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(idBBDD))
                {
                    //Si el documento se ha cargado nuevo o se ha podido modificar añado los autores.
                    if (modificado)
                    {
                        personasUsadas.UnionWith(entityXML.properties
                                .Where(x => x.prop == BFO_member)
                                .SelectMany(x => x.values)
                                .Select(x => x.Split("@@@")[1]));
                    }

                    if (!string.IsNullOrEmpty(pPropertyCV) && !string.IsNullOrEmpty(pRdfTypeCV))
                    {
                        valoresPropertiesCV(entityXML, propiedadesItem, pPropertyCV, pRdfTypeCV, idBBDD, RdfTypeTab);
                    }

                    documentosDesnormalizar.Add(idBBDD);
                }
            }

            //Elimino las personas repetidas.
            personasCargar.RemoveAll(x => !personasUsadas.Contains(x.GnossId));

            //Cargo las personas
            ConcurrentBag<string> personasDesnormalizarAux = new();
            Parallel.ForEach(personasCargar, new ParallelOptions { MaxDegreeOfParallelism = 5 }, personaCargar =>
            {
                int numIntentos = 0;
                while (!personaCargar.Uploaded)
                {
                    numIntentos++;
                    if (numIntentos > 5)
                    {
                        break;
                    }
                    string id = mResourceApi.LoadComplexSemanticResource(personaCargar);
                    if (personaCargar.Uploaded)
                    {
                        personasDesnormalizarAux.Add(id);
                    }
                }
            });
            personasDesnormalizar.UnionWith(personasDesnormalizarAux);

            //Insertamos en la cola del desnormalizador            
            if (personasDesnormalizar.Count > 0)
            {
                RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new(mConfiguracion);
                rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.person, personasDesnormalizar));
            }
            if (documentosDesnormalizar.Count > 0)
            {
                RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new(mConfiguracion);
                rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.document, documentosDesnormalizar));
            }


            //Enviamos las notificaciones
            List<Notification> notificacionesCargar = notificaciones.ToList();
            notificacionesCargar.RemoveAll(x => x.IdRoh_owner == idPersona);

            if (petitionStatus != null)
            {
                petitionStatus.actualWorkSubtitle = "IMPORTANDO_NOTIFICACIONES_PUBLICACIONES";
                petitionStatus.actualSubWorks = 1;
                petitionStatus.actualSubTotalWorks = notificacionesCargar.Count;
            }

            mResourceApi.ChangeOntoly("notification");
            Parallel.ForEach(notificacionesCargar, new ParallelOptions { MaxDegreeOfParallelism = 6 }, notificacion =>
            {
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

                if (petitionStatus != null)
                {
                    petitionStatus.actualSubWorks++;
                }
            });
        }

        /// <summary>
        /// Añade los valores de las entidades con propiedades especificas del CV.
        /// </summary>
        /// <param name="entityXML"></param>
        /// <param name="propiedadesItem"></param>
        /// <param name="pPropertyCV"></param>
        /// <param name="pRdfTypeCV"></param>
        /// <param name="pIdEntidadBBDD"></param>
        /// <param name="RdfTypeTab"></param>
        private void valoresPropertiesCV(Entity entityXML, List<string> propiedadesItem,
            string pPropertyCV, string pRdfTypeCV, string pIdEntidadBBDD, string RdfTypeTab)
        {
            string rdfTypePrefix;
            string entityCVID;

            //Si es nueva tendra valor sino traera una cadena vacia

            if (entityXML.properties_cv != null
                && entityXML.properties_cv.Count > 0
                && propiedadesItem.Count > 2)
            {

                //Obtenemos la auxiliar en la que cargar la entidad
                SparqlObject tab = mResourceApi.VirtuosoQuery("select *", "where{<" + mCvID + "> ?s ?o. ?o a <" + RdfTypeTab + "> }", "curriculumvitae");
                string idTab = tab.results.bindings[0]["o"].value;

                //Query para obtener entityID
                string select = "select distinct ?related ?relatedCV ";
                string where = $@"where {{
                                        ?s <{propiedadesItem[0]}> ?category . 
                                        ?category <{propiedadesItem[propiedadesItem.Count - 2]}> ?related . 
                                        OPTIONAL{{?related <{pPropertyCV}> ?relatedCV.}}
                                        ?related <http://vivoweb.org/ontology/core#relatedBy> ?item
                                        FILTER(?s=<{mCvID}>)
                                        FILTER(?item=<{pIdEntidadBBDD}>)
                                    }}";
                SparqlObject entityIDCV = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                if (entityIDCV.results.bindings.Count == 0)
                {
                    return;
                }
                string idEntity = entityIDCV.results.bindings[0]["related"].value;

                //Si no es una nueva entidad añado la referencia de la clase intermedia y de claseCV
                string idEntityCV = "";
                if (entityIDCV.results.bindings[0].ContainsKey("relatedCV"))
                {
                    idEntityCV = entityIDCV.results.bindings[0]["relatedCV"].value;
                }

                //Si es nulo, genero una nueva referencia de claseCV
                if (string.IsNullOrEmpty(idEntityCV))
                {
                    rdfTypePrefix = UtilityCV.AniadirPrefijo(pRdfTypeCV);
                    rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                    entityCVID = $"{mResourceApi.GraphsUrl}items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(mCvID) + "_" + Guid.NewGuid();
                }
                else
                {
                    entityCVID = idEntityCV;
                }

                List<string> propertyIDs = new(propiedadesItem);
                propertyIDs.RemoveAt(propertyIDs.Count - 1);
                propertyIDs.Add(pPropertyCV);
                List<string> entityIDs = new()
                    {
                        idTab,
                        idEntity,
                        entityCVID
                    };

                Entity entityToLoad = new();
                entityToLoad.id = entityCVID;
                entityToLoad.ontology = "curriculumvitae";
                entityToLoad.properties = entityXML.properties_cv;
                entityToLoad.rdfType = pRdfTypeCV;
                Entity entityBBDD = GetLoadedEntity(entityCVID, "curriculumvitae");
                UpdateEntityAux(mResourceApi.GetShortGuid(mCvID), propertyIDs, entityIDs, entityBBDD, entityToLoad);
            }
        }

        /// <summary>
        /// Edita las propiedades si la entidad en BBDD no esta validada ni bloqueada.
        /// </summary>
        /// <param name="equivalencias"></param>
        /// <param name="idXML"></param>
        /// <param name="graph"></param>
        /// <param name="propTitle"></param>
        /// <param name="entityXML"></param>
        /// <returns>True si no esta bloqueado</returns>
        protected bool ModificarExistentes(Dictionary<string, string> equivalencias, string idXML, string graph, string propTitle, Entity entityXML)
        {
            //Entidad a modificar
            Entity entityBBDD = GetLoadedEntity(equivalencias[idXML], graph);
            //Modificamos si no está bloqueada
            if (entityBBDD != null && !entityBBDD.properties.Any(x => x.prop.Equals("http://w3id.org/roh/crisIdentifier"))
                && !entityBBDD.properties.Any(x => x.prop.Equals("http://w3id.org/roh/isValidated") && x.values.Contains("true"))
                && !entityBBDD.properties.Any(x => x.prop.Equals("http://w3id.org/roh/validationStatusPRC")))
            {
                entityBBDD.propTitle = propTitle;
                bool hasChange = MergeLoadedEntity(entityBBDD, entityXML);
                if (hasChange)
                {
                    ComplexOntologyResource resource = ToGnossApiResource(entityBBDD);
                    mResourceApi.ModifyComplexOntologyResource(resource, false, true);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Edita las propiedades si la entidad en BBDD no esta validada ni bloqueada.
        /// </summary>
        /// <param name="entidadBBDD"></param>
        /// <param name="graph"></param>
        /// <param name="propTitle"></param>
        /// <param name="entityXML"></param>
        /// <returns>True si no esta bloqueado</returns>
        protected bool ModificarExistentes(string entidadBBDD, string graph, string propTitle, Entity entityXML)
        {
            //Entidad a modificar
            Entity entityBBDD = GetLoadedEntity(entidadBBDD, graph);
            //Modificamos si no está bloqueada
            if (entityBBDD != null && !entityBBDD.properties.Any(x => x.prop.Equals("http://w3id.org/roh/crisIdentifier"))
                && !entityBBDD.properties.Any(x => x.prop.Equals("http://w3id.org/roh/isValidated") && x.values.Contains("true"))
                && !entityBBDD.properties.Any(x => x.prop.Equals("http://w3id.org/roh/validationStatusPRC")))
            {
                entityBBDD.propTitle = propTitle;
                bool hasChange = MergeLoadedEntity(entityBBDD, entityXML);
                if (hasChange)
                {
                    ComplexOntologyResource resource = ToGnossApiResource(entityBBDD);
                    mResourceApi.ModifyComplexOntologyResource(resource, false, true);
                }
                return true;
            }
            return false;
        }

        protected bool SobrescribirExistentes(string entidadBBDD, string graph, string propTitle, Entity entityXML)
        {
            //Entidad a modificar
            Entity entityBBDD = GetLoadedEntity(entidadBBDD, graph);
            //Modificamos si no está bloqueada
            if (entityBBDD != null && !entityBBDD.properties.Any(x => x.prop.Equals("http://w3id.org/roh/crisIdentifier"))
                && !entityBBDD.properties.Any(x => x.prop.Equals("http://w3id.org/roh/isValidated") && x.values.Contains("true"))
                && !entityBBDD.properties.Any(x => x.prop.Equals("http://w3id.org/roh/validationStatusPRC")))
            {
                entityBBDD.propTitle = propTitle;
                bool hasChange = OverwriteLoadedEntity(entityBBDD, entityXML);
                if (hasChange)
                {
                    ComplexOntologyResource resource = ToGnossApiResource(entityXML);
                    mResourceApi.ModifyComplexOntologyResource(resource, false, true);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Devuelve el listado de las <paramref name="entidadesBBDD"/> y si estan bloqueado su modificación.
        /// </summary>
        /// <param name="entidadesBBDD"></param>
        /// <param name="pGrafo"></param>
        /// <returns></returns>
        protected Dictionary<string, bool> ComprobarBloqueados(List<string> entidadesBBDD, string pGrafo)
        {
            if (entidadesBBDD == null || string.IsNullOrEmpty(pGrafo)) { return new Dictionary<string, bool>(); }

            Dictionary<string, bool> resultados = entidadesBBDD.ToDictionary(x => x, x => false);

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(entidadesBBDD, Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = "select distinct ?s";
                string where = $@"where {{
                                ?s a ?o.
                                {{?s <http://w3id.org/roh/crisIdentifier> ?cris}}
                                UNION
                                {{?s <http://w3id.org/roh/isValidated> ""true""}}
                                UNION
                                {{?s <http://w3id.org/roh/validationStatusPRC> ""pendiente""}}
                                UNION
                                {{?s <http://w3id.org/roh/validationStatusPRC> ""validado""}}

                                FILTER(?s in (<{string.Join(">,<", lista)}>))
                             }}";
                SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, pGrafo);

                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    resultados[fila["s"].value] = true;
                }
            }

            return resultados;
        }


        /// <summary>
        /// Inserta triples en <paramref name="pCvID"/> 
        /// </summary>
        /// <param name="pCvID">pCvID</param>
        /// <param name="pRdfTypeTab">pRdfTypeTab</param>
        /// <param name="rdfTypePrefix">rdfTypePrefix</param>
        /// <param name="pPropertyIDs">pPropertyIDs</param>
        /// <param name="pUpdatedEntity">pUpdatedEntity</param>
        /// <returns></returns>
        public string CreateListEntityAux(string pCvID, string pRdfTypeTab, string rdfTypePrefix, List<string> pPropertyIDs, Entity pUpdatedEntity)
        {
            if (!string.IsNullOrEmpty(pUpdatedEntity.ontology))
            {
                mResourceApi.ChangeOntoly(pUpdatedEntity.ontology);
            }
            //Creamos el recurso que no pertenece al CV
            ComplexOntologyResource resource = ToGnossApiResource(pUpdatedEntity);
            string result = "";
            int numIntentos = 0;
            while (!resource.Uploaded)
            {
                numIntentos++;
                if (numIntentos > 5)
                {
                    break;
                }
                result = mResourceApi.LoadComplexSemanticResource(resource);
            }
            if (resource.Uploaded)
            {
                //Obtenemos la auxiliar en la que cargar la entidad
                SparqlObject tab = mResourceApi.VirtuosoQuery("select *", "where{<" + pCvID + "> ?s ?o. ?o a <" + pRdfTypeTab + "> }", "curriculumvitae");
                string idTab = tab.results.bindings[0]["o"].value;

                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = $"{mResourceApi.GraphsUrl}items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(pCvID) + "_" + Guid.NewGuid();

                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = idTab + "|" + idNewAux;
                string valorEntityAux = pPropertyIDs[0] + "|" + pPropertyIDs[1];

                //Privacidad, por defecto falso
                string valorPrivacidad = idEntityAux + "|false";
                string predicadoPrivacidad = valorEntityAux + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(valorPrivacidad, predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string valorEntidad = idEntityAux + "|" + result;
                string predicadoEntidad = valorEntityAux + "|" + pPropertyIDs[2];
                TriplesToInclude tr1 = new(valorEntidad, predicadoEntidad);
                listaTriples.Add(tr1);

                Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new()
                {
                    {
                        mResourceApi.GetShortGuid(pCvID), listaTriples
                    }
                };

                mResourceApi.InsertPropertiesLoadedResources(triplesToInclude);
                return result;
            }
            else
            {
                return null;
            }
        }

        protected bool OverwriteLoadedEntity(Entity pLoadedEntity, Entity pUpdatedEntity)
        {
            pUpdatedEntity.autores = pLoadedEntity.autores;
            pUpdatedEntity.auxEntityRemove = pLoadedEntity.auxEntityRemove;
            pUpdatedEntity.id = pLoadedEntity.id;
            pUpdatedEntity.ontology = pLoadedEntity.ontology;
            pUpdatedEntity.propDescription = pLoadedEntity.propDescription;
            pUpdatedEntity.propTitle = pLoadedEntity.propTitle;
            pUpdatedEntity.rdfType = pLoadedEntity.rdfType;

            return true;
        }

        /// <summary>
        /// Fusiona dos entidades
        /// </summary>
        /// <param name="pLoadedEntity">Entidad cargada en BBDD</param>
        /// <param name="pUpdatedEntity">Nueva entidad</param>
        /// <returns>Devuelve true si ha detetado cambios</returns>
        protected bool MergeLoadedEntity(Entity pLoadedEntity, Entity pUpdatedEntity)
        {
            bool change = false;

            foreach (Entity.Property property in pUpdatedEntity.properties)
            {
                if (property.values != null)
                {
                    property.values.RemoveAll(x => x != null && x == "@@@");
                }

                bool remove = property.values == null || property.values.Count == 0 || !property.values.Exists(x => !string.IsNullOrEmpty(x));
                //Recorremos las propiedades de la entidad a actualizar y modificamos la entidad recuperada de BBDD
                Entity.Property propertyLoadedEntity = pLoadedEntity.properties.FirstOrDefault(x => x.prop == property.prop);
                if (propertyLoadedEntity != null)
                {
                    if (remove)
                    {
                        change = true;
                        pLoadedEntity.properties.Remove(propertyLoadedEntity);
                    }
                    else
                    {
                        int numLoaded = propertyLoadedEntity.values.Count;
                        int numNew = property.values.Count;
                        int numIntersect = propertyLoadedEntity.values.Intersect(property.values).Count();
                        if (numLoaded != numNew || numLoaded != numIntersect)
                        {
                            change = true;
                        }
                        propertyLoadedEntity.values = property.values;

                    }
                }
                else if (!remove)
                {
                    change = true;
                    pLoadedEntity.properties.Add(property);
                }
                else
                {
                    List<Entity.Property> propertiesLoadedEntityRemove = pLoadedEntity.properties.Where(x => x.prop == property.prop || x.prop.StartsWith(property.prop + "|") || x.prop.StartsWith(property.prop + "@@@")).ToList();
                    foreach (Entity.Property propertyToRemove in propertiesLoadedEntityRemove)
                    {
                        pLoadedEntity.properties.Remove(propertyToRemove);
                        change = true;
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
                            List<string> eliminar = new();
                            foreach (string value in property.values)
                            {
                                if (value.Contains(auxEntityRemove))
                                {
                                    eliminar.Add(value);
                                }
                            }
                            if (property.values.RemoveAll(x => eliminar.Contains(x)) > 0)
                            {
                                change = true;
                            }
                        }
                    }
                }
            }
            return change;
        }


        /// <summary>
        /// Transforma la propiedad para su carga en una entiadad auxiliar
        /// </summary>
        /// <param name="pProp">Propiedad</param>
        /// <returns></returns>
        private static string GetPropUpdateEntityAux(string pProp)
        {
            while (pProp.Contains("@@@"))
            {
                int indexInitRdfType = pProp.IndexOf("@@@");
                int indexEndRdfType = pProp.IndexOf("|", indexInitRdfType);
                if (indexEndRdfType > indexInitRdfType)
                {
                    pProp = string.Concat(pProp.AsSpan(0, indexInitRdfType), pProp.AsSpan(indexEndRdfType));
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
        private static string GetValueUpdateEntityAux(string pValue)
        {
            return pValue.Replace("@@@", "|");
        }


        /// <summary>
        /// Obtiene la entidad del valor
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        private static string GetEntityOfValue(string pValue)
        {
            string entityID = "";
            if (pValue.Contains("@@@"))
            {
                entityID = pValue.Substring(0, pValue.IndexOf("@@@"));
            }
            return entityID;
        }

        public static Tuple<string, string, string> GetIdentificadoresItemPresentation(string pId, List<string> pPropiedades, List<string> rdfTypeItem)
        {
            if (pPropiedades.Count != 3)
            {
                return null;
            }

            try
            {
                string item1 = "";
                string item2 = "";
                string item3 = "";

                string selectID = "select ?item1 ?item2 ?item3";
                string whereID = $@"where{{
                                    <{pId}> <{pPropiedades[0]}> ?item1 .
                                    OPTIONAL{{
                                        ?item1 <{pPropiedades[1]}> ?item2.
                                        OPTIONAL{{
                                            ?item2 <{pPropiedades[2]}> ?item3
                                        }}
                                    }}
                                }}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, "curriculumvitae");
                if (resultData == null || resultData.results == null || resultData.results.bindings == null || !resultData.results.bindings.Any())
                {
                    return null;
                }

                Dictionary<string, Data> fila = resultData.results.bindings.First();
                if (fila.ContainsKey("item1"))
                {
                    item1 = fila["item1"].value;
                }

                if (fila.ContainsKey("item2"))
                {
                    item2 = fila["item2"].value;
                }
                else
                {
                    item2 = mResourceApi.GraphsUrl + "items/" + rdfTypeItem[0].Split("/").Last() + "_" + mResourceApi.GetShortGuid(item1).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                }

                if (fila.ContainsKey("item3"))
                {
                    item3 = fila["item3"].value;
                }
                else
                {
                    item3 = mResourceApi.GraphsUrl + "items/" + rdfTypeItem[1].Split("/").Last() + "_" + mResourceApi.GetShortGuid(item1).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                }

                return new Tuple<string, string, string>(item1, item2, item3);
            }
            catch (NullReferenceException e)
            {
                Console.Error.WriteLine("Errores al cargar mResourceApi " + e.Message);
                return null;
            }
        }


        /// <summary>
        /// Crea un ComplexOntologyResource con los datos de una entidad para su carga
        /// </summary>
        /// <param name="pEntity">Datos de una entidad</param>
        /// <returns>ComplexOntologyResource</returns>
        protected ComplexOntologyResource ToGnossApiResource(Entity pEntity)
        {
            //Preparamos los datos de la entidad principal y las auxiliares
            List<EntityRdf> entities = new();
            EntityRdf entidadPrincipal = new();
            entities.Add(entidadPrincipal);
            entidadPrincipal.id = pEntity.id;
            entidadPrincipal.rdfType = pEntity.rdfType;
            entidadPrincipal.props = new Dictionary<string, List<string>>();
            entidadPrincipal.ents = new Dictionary<string, List<EntityRdf>>();
            foreach (Entity.Property property in pEntity.properties.OrderBy(x => x.prop.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries).Length))
            {
                if (property.prop != "null" && property.prop != null && property.values.Count > 0)
                {
                    string[] propArray = property.prop.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string value in property.values)
                    {
                        string valueAux = value;
                        if (value == null)
                        {
                            valueAux = "";
                        }
                        string[] valueArray = valueAux.Split(new string[] { "@@@" }, StringSplitOptions.None);
                        if (propArray.Length != valueArray.Length)
                        {
                            throw new ArgumentException("El tamaño de las propiedades no coincide con el de los valores");
                        }
                        if (propArray.Length == 1)
                        {
                            //Es la entidad principal
                            string propPrefix = UtilityCV.AniadirPrefijo(propArray.Last());
                            if (!entidadPrincipal.props.ContainsKey(propPrefix))
                            {
                                entidadPrincipal.props.Add(propPrefix, new List<string>());
                            }
                            entidadPrincipal.props[propPrefix].Add(valueAux);
                        }
                        else
                        {
                            //Es una entidad auxiliar
                            string rdfType = propArray.Last().Split('|').First();
                            string id = valueArray[valueArray.Length - 2];
                            EntityRdf entidadActual = entities.FirstOrDefault(x => x.id == id);
                            if (entidadActual == null)
                            {
                                entidadActual = new EntityRdf()
                                {
                                    props = new Dictionary<string, List<string>>(),
                                    ents = new Dictionary<string, List<EntityRdf>>(),
                                    rdfType = rdfType,
                                    id = id
                                };
                                entities.Add(entidadActual);
                            }

                            string propPrefix = UtilityCV.AniadirPrefijo(propArray.Last().Split('|').Last());
                            if (!entidadActual.props.ContainsKey(propPrefix))
                            {
                                entidadActual.props.Add(propPrefix, new List<string>());
                            }
                            entidadActual.props[propPrefix].Add(valueArray.Last());

                            //Padre
                            EntityRdf entidadPadre = null;
                            if (valueArray.Length == 2)
                            {
                                entidadPadre = entidadPrincipal;
                            }
                            else
                            {
                                entidadPadre = entities.First(x => x.id == valueArray[valueArray.Length - 3]);
                            }

                            if (!entidadPadre.ents.ContainsKey(propArray[propArray.Length - 2]))
                            {
                                entidadPadre.ents.Add(propArray[propArray.Length - 2], new List<EntityRdf>());
                            }
                            if (!entidadPadre.ents[propArray[propArray.Length - 2]].Exists(x => x.id == entidadActual.id))
                            {
                                entidadPadre.ents[propArray[propArray.Length - 2]].Add(entidadActual);
                            }

                        }
                    }
                }
            }
            ComplexOntologyResource resource = new()
            {
                Ontology = ProcesarEntidadPrincipal(entidadPrincipal)
            };
            Entity.Property propTitle = pEntity.properties.FirstOrDefault(x => x.prop == pEntity.propTitle);
            Entity.Property propDescription = pEntity.properties.FirstOrDefault(x => x.prop == pEntity.propDescription);
            if (propTitle != null && propTitle.values.Count > 0)
            {
                resource.Title = propTitle.values.First();
                if (resource.Title == null)
                {
                    resource.Title = "";
                }
            }
            if (propDescription != null && propDescription.values.Count > 0)
            {
                resource.Description = propDescription.values.First();
                if (resource.Description == null)
                {
                    resource.Description = "";
                }
            }
            return resource;
        }

        /// <summary>
        /// Procesa una entidad principal para crear el ComplexOntologyResource
        /// </summary>
        /// <param name="pEntidadPrincipal">Entidad principal</param>
        /// <returns></returns>
        private Ontology ProcesarEntidadPrincipal(EntityRdf pEntidadPrincipal)
        {
            List<string> prefList = new();
            foreach (string key in UtilityCV.GetDicPrefix().Keys)
            {
                prefList.Add($"xmlns:{key}=\"{UtilityCV.GetDicPrefix()[key]}\"");
            }

            List<OntologyEntity> entList = new();
            List<OntologyProperty> propList = new();

            foreach (string prop in pEntidadPrincipal.ents.Keys)
            {
                foreach (EntityRdf entity in pEntidadPrincipal.ents[prop])
                {
                    entList.Add(ProcesarEntidadAuxiliar(prop, entity));
                }
            }

            //Creamos la entidad principal
            foreach (string prop in pEntidadPrincipal.props.Keys)
            {
                foreach (string value in pEntidadPrincipal.props[prop])
                {
                    propList.Add(new StringOntologyProperty(prop, value));
                }
            }
            Ontology ontology;
            if (string.IsNullOrEmpty(pEntidadPrincipal.id))
            {
                ontology = new Ontology(mResourceApi.GraphsUrl, mResourceApi.OntologyUrl, pEntidadPrincipal.rdfType, pEntidadPrincipal.rdfType, prefList, propList, entList);
            }
            else
            {
                string[] idSplit = pEntidadPrincipal.id.Split('_');
                Guid idRecurso = new(idSplit[idSplit.Length - 2]);
                Guid idArticulo = new(idSplit[idSplit.Length - 1]);
                ontology = new Ontology(mResourceApi.GraphsUrl, mResourceApi.OntologyUrl, pEntidadPrincipal.rdfType, pEntidadPrincipal.rdfType, prefList, propList, entList, idRecurso, idArticulo);
            }

            return ontology;
        }

        /// <summary>
        /// Procesa una entidad auxiliar para crear el ComplexOntologyResource
        /// </summary>
        /// <param name="pProperty">Propiedad que apunta a la entidad auxiliar</param>
        /// <param name="pEntidadAuxiliar">Entidad auxiliar</param>
        /// <returns></returns>
        private OntologyEntity ProcesarEntidadAuxiliar(string pProperty, EntityRdf pEntidadAuxiliar)
        {
            List<string> prefList = new ();
            foreach (string key in UtilityCV.GetDicPrefix().Keys)
            {
                prefList.Add($"xmlns:{key}=\"{UtilityCV.GetDicPrefix()[key]}\"");
            }

            List<OntologyEntity> entList = new();
            List<OntologyProperty> propList = new();

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
            OntologyEntity ontologyEntity = new(pEntidadAuxiliar.rdfType, pEntidadAuxiliar.rdfType, UtilityCV.AniadirPrefijo(pProperty.Split('|').Last()), propList, entList);
            return ontologyEntity;
        }
    }
}
