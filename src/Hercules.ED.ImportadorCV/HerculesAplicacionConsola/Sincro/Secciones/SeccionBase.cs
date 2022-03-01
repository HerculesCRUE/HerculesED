using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using GuardadoCV.Models.Utils;
using HerculesAplicacionConsola.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace HerculesAplicacionConsola.Sincro.Secciones
{
    public abstract class SeccionBase
    {
        protected static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configOAuth\OAuthV3.config");
        protected cvnRootResultBean mCvn { get; set; }
        protected string mCvID { get; set; }
        public SeccionBase(cvnRootResultBean cvn, string cvID)
        {
            mCvn = cvn;
            mCvID = cvID;
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
                    if (resultData.results.bindings.Count() < numLimit)
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
                            property = new Entity.Property()
                            {
                                prop = pPropAcumuladoAux,
                                values = new List<string>()
                            };
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
            Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToInclude>> triplesInclude = new Dictionary<Guid, List<TriplesToInclude>>() { { pIdMainEntity, new List<TriplesToInclude>() } };
            Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>> triplesRemove = new Dictionary<Guid, List<RemoveTriples>>() { { pIdMainEntity, new List<RemoveTriples>() } };
            Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToModify>> triplesModify = new Dictionary<Guid, List<TriplesToModify>>() { { pIdMainEntity, new List<TriplesToModify>() } };

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
                                Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(pIdMainEntity, valor, property.prop)
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
                                        NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(pIdMainEntity, valuesEntity[0], property.prop),
                                        OldValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(pIdMainEntity, valuesLoadedEntity[0], property.prop)
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
                                            Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(pIdMainEntity, valor, property.prop)
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
                                                NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(pIdMainEntity, valor, property.prop)
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
                                NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(pIdMainEntity, valor, property.prop)
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
                                Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(pIdMainEntity, valor, property.prop)
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
        /// Edita las propiedades si la entidad en BBDD no esta validada ni bloqueada.
        /// </summary>
        /// <param name="equivalencias"></param>
        /// <param name="idXML"></param>
        /// <param name="graph"></param>
        /// <param name="propTitle"></param>
        /// <param name="entityXML"></param>
        protected void ModificarExistentes(Dictionary<string, string> equivalencias, string idXML, string graph, string propTitle, Entity entityXML)
        {
            //Entidad a modificar
            Entity entityBBDD = GetLoadedEntity(equivalencias[idXML], graph);

            //Modificamos si no está bloqueada
            if (!entityBBDD.properties.Where(x => x.prop.Equals("http://w3id.org/roh/crisIdentifier")).Any()
                && !entityBBDD.properties.Where(x => x.prop.Equals("http://w3id.org/roh/isValidated") && x.values.Contains("true")).Any())
            {
                entityBBDD.propTitle = propTitle;
                bool hasChange = MergeLoadedEntity(entityBBDD, entityXML);
                if (hasChange)
                {
                    ComplexOntologyResource resource = ToGnossApiResource(entityBBDD);
                    mResourceApi.ModifyComplexOntologyResource(resource, false, true);
                }
            }
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
            string result = mResourceApi.LoadComplexSemanticResource(resource, false, true);

            //Obtenemos la auxiliar en la que cargar la entidad
            SparqlObject tab = mResourceApi.VirtuosoQuery("select *", "where{<" + pCvID + "> ?s ?o. ?o a <" + pRdfTypeTab + "> }", "curriculumvitae");
            string idTab = tab.results.bindings[0]["o"].value;

            rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
            string idNewAux = $"{mResourceApi.GraphsUrl}items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(pCvID) + "_" + Guid.NewGuid();

            List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
            string idEntityAux = idTab + "|" + idNewAux;
            string valorEntityAux = pPropertyIDs[0] + "|" + pPropertyIDs[1];

            //Privacidad, por defecto falso
            string valorPrivacidad = idEntityAux + "|false";
            string predicadoPrivacidad = valorEntityAux + "|http://w3id.org/roh/isPublic";
            TriplesToInclude tr2 = new TriplesToInclude(valorPrivacidad, predicadoPrivacidad);
            listaTriples.Add(tr2);

            //Entidad
            string valorEntidad = idEntityAux + "|" + result;
            string predicadoEntidad = valorEntityAux + "|" + pPropertyIDs[2];
            TriplesToInclude tr1 = new TriplesToInclude(valorEntidad, predicadoEntidad);
            listaTriples.Add(tr1);

            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>()
            {
                {
                    mResourceApi.GetShortGuid(pCvID), listaTriples
                }
            };

            Dictionary<Guid, bool> respuesta = mResourceApi.InsertPropertiesLoadedResources(triplesToInclude);
            return idNewAux;
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
                else if (remove)
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
                            List<string> eliminar = new List<string>();
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
        /// <param name="pMainEntityID">Identificador de la entiad principal</param>
        /// <param name="pValue">Valor</param>
        /// <param name="pProp">Propiedad</param>
        /// <returns></returns>
        private string GetValueUpdateEntityAux(Guid pMainEntityID, string pValue, string pProp)
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

        public static Tuple<string, string, string> GetIdentificadoresItemPresentation(string pId, List<string> pPropiedades)
        {
            if (pPropiedades.Count != 3) { return null; }
            try
            {
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
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    string item1 = "";
                    string item2 = "";
                    string item3 = "";
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
                        item2 = mResourceApi.GraphsUrl+ "items/GeneralQualityIndicator_"+mResourceApi.GetShortGuid(item1).ToString().ToLower() +"_"+Guid.NewGuid().ToString().ToLower();
                    }
                    if (fila.ContainsKey("item3"))
                    {
                        item3 = fila["item3"].value;
                    }
                    else
                    {
                        item3 = mResourceApi.GraphsUrl + "items/GeneralQualityIndicatorCV_"+mResourceApi.GetShortGuid(item1).ToString().ToLower() +"_"+Guid.NewGuid().ToString().ToLower();
                    }
                    return new Tuple<string, string, string>(item1, item2, item3);
                }
                throw new Exception($"No existe la propiedad {pPropiedades[0]}");
            }
            catch (NullReferenceException e)
            {
                Console.Error.WriteLine("Errores al cargar mResourceApi " + e.Message);
                throw new NullReferenceException("Errores al cargar mResourceApi");
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
            List<EntityRdf> entities = new List<EntityRdf>();
            EntityRdf entidadPrincipal = new EntityRdf();
            entities.Add(entidadPrincipal);
            entidadPrincipal.id = pEntity.id;
            entidadPrincipal.rdfType = pEntity.rdfType;
            entidadPrincipal.props = new Dictionary<string, List<string>>();
            entidadPrincipal.ents = new Dictionary<string, List<EntityRdf>>();
            List<EntityRdf> listaEntidadesAuxiliares = new List<EntityRdf>();
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
                            throw new Exception("El tamaño de las propiedades no coincide con el de los valores");
                        }
                        if (propArray.Count() == 1)
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
            ComplexOntologyResource resource = new ComplexOntologyResource()
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
            List<string> prefList = new List<string>();
            foreach (string key in UtilityCV.dicPrefix.Keys)
            {
                prefList.Add($"xmlns:{key}=\"{UtilityCV.dicPrefix[key]}\"");
            }

            List<OntologyEntity> entList = new List<OntologyEntity>();
            List<OntologyProperty> propList = new List<OntologyProperty>();

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
                Guid idRecurso = new Guid(idSplit[idSplit.Length - 2]);
                Guid idArticulo = new Guid(idSplit[idSplit.Length - 1]);
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
    }
}
