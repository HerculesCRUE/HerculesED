using EditorCV.Models.API.Response;
using EditorCV.Models.ORCID;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using GuardadoCV.Models.API;
using GuardadoCV.Models.API.Input;
using GuardadoCV.Models.API.Response;
using GuardadoCV.Models.API.Templates;
using GuardadoCV.Models.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace GuardadoCV.Models
{
    /// <summary>
    /// Clase utilizada para las acciones de modificación de datos en el CV
    /// </summary>
    public class AccionesGuardado
    {
        /// <summary>
        /// API
        /// </summary>
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configOAuth/OAuthV3.config");

        /// <summary>
        /// Cambia la privacidad de un item
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntity">Identificador de la entidad</param>
        /// <param name="pIsPublic">TRUE si es público</param>
        /// <returns></returns>
        public JsonResult ChangePrivacityItem(string pIdSection, string pRdfTypeTab, string pEntity, bool pIsPublic)
        {
            TabSectionListItem presentationMini = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation.listItemsPresentation.listItem;

            mResourceApi.ChangeOntoly("curriculumvitae");
            KeyValuePair<List<string>, List<string>> subjectAndProperties = GetSubjectsAndPropertiesFromAuxCV(pEntity, UtilityCV.PropertyIspublic);
            List<string> properties = subjectAndProperties.Value;
            List<string> entities = subjectAndProperties.Key;

            //Obtenemos el valor actual de la propiedad
            string valorActual = "";
            SparqlObject resultDataProperty = mResourceApi.VirtuosoQuery("select *", "where{<" + pEntity + "> <" + UtilityCV.PropertyIspublic + "> ?o. }", "curriculumvitae");
            if (resultDataProperty.results.bindings.Count > 0)
            {
                valorActual = resultDataProperty.results.bindings[0]["o"].value;
            }

            if (string.IsNullOrEmpty(valorActual))
            {
                //Insertamos la propiedad
                Dictionary<Guid, List<TriplesToInclude>> dicTriplesAInsertar = new Dictionary<Guid, List<TriplesToInclude>>();
                string valor = string.Join("|", entities) + "|" + pIsPublic.ToString().ToLower();
                string propiedad = string.Join("|", properties);
                List<TriplesToInclude> listaTriples = new List<TriplesToInclude>() { new TriplesToInclude(valor, propiedad) };
                dicTriplesAInsertar.Add(mResourceApi.GetShortGuid(pEntity), listaTriples);
                Dictionary<Guid, bool> dicCorrecto = mResourceApi.InsertPropertiesLoadedResources(dicTriplesAInsertar);
                return new JsonResult() { ok = dicCorrecto[mResourceApi.GetShortGuid(pEntity)] };
            }
            else
            {
                //Modificamos la propiedad
                Dictionary<Guid, List<TriplesToModify>> dicModify = new Dictionary<Guid, List<TriplesToModify>>();
                TriplesToModify tripe = new TriplesToModify() { Predicate = string.Join("|", properties), NewValue = string.Join("|", entities) + "|" + pIsPublic.ToString().ToLower(), OldValue = string.Join("|", entities) + "|" + valorActual };
                dicModify.Add(mResourceApi.GetShortGuid(pEntity), new List<TriplesToModify>() { tripe });
                Dictionary<Guid, bool> dicCorrecto = mResourceApi.ModifyPropertiesLoadedResources(dicModify);
                return new JsonResult() { ok = dicCorrecto[mResourceApi.GetShortGuid(pEntity)] }; ;
            }
        }

        /// <summary>
        /// Elimina un item de un listado
        /// </summary>
        /// <param name="pEntity">Entidad a eliminar</param>
        /// <returns></returns>
        public JsonResult RemoveItem(string pEntity)
        {
            mResourceApi.ChangeOntoly("curriculumvitae");
            KeyValuePair<List<string>, List<string>> subjectAndProperties = GetSubjectsAndPropertiesFromAuxCV(pEntity, "");
            List<string> properties = subjectAndProperties.Value;
            List<string> entities = subjectAndProperties.Key;

            List<RemoveTriples> lista = new List<RemoveTriples>();
            Dictionary<Guid, List<RemoveTriples>> dicEliminar = new Dictionary<Guid, List<RemoveTriples>>();
            RemoveTriples rt = new RemoveTriples() { Predicate = string.Join("|", properties), Value = string.Join("|", entities) };
            lista.Add(rt);
            dicEliminar.Add(mResourceApi.GetShortGuid(pEntity), lista);
            Dictionary<Guid, bool> dicCorrecto = mResourceApi.DeletePropertiesLoadedResources(dicEliminar);
            return new JsonResult() { ok = dicCorrecto[mResourceApi.GetShortGuid(pEntity)] };
        }

        /// <summary>
        /// Crea/actualiza una entidad
        /// </summary>
        /// <param name="pEntity">Datos de la entidad a crear/actualizar</param>
        /// <param name="pCvID">Identificador del CV</param>
        /// <param name="pSectionID">Identifiador de la sección (para la edición de un item de un listado)</param>
        /// <param name="pRdfTypeTab">rdf:type de la pestaña (para la edición de un item de un listado)</param>
        /// <returns></returns>
        public JsonResult ActualizarEntidad(Entity pEntity, string pCvID, string pSectionID, string pRdfTypeTab)
        {
            if (pRdfTypeTab == "http://w3id.org/roh/PersonalData")
            {
                GuardadoCV.Models.API.Templates.Tab template = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab);

                //Modificamos
                Entity loadedEntity = GetLoadedEntity(pEntity.id, "curriculumvitae");
                bool updated = UpdateEntityAux(new List<string>() { "http://w3id.org/roh/personalData" }, new List<string>() { pEntity.id }, loadedEntity, pEntity);

                if (updated)
                {
                    return new JsonResult() { ok = true, id = pEntity.id };
                }
                else
                {
                    return new JsonResult() { ok = false };
                }

            }
            else if (!string.IsNullOrEmpty(pSectionID) && !string.IsNullOrEmpty(pRdfTypeTab))
            {
                //Item de CV
                GuardadoCV.Models.API.Templates.Tab template = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab);
                GuardadoCV.Models.API.Templates.TabSection templateSection = template.sections.First(x => x.property == pSectionID);
                mResourceApi.ChangeOntoly(templateSection.presentation.listItemsPresentation.listItemEdit.graph);
                pEntity.ontology = templateSection.presentation.listItemsPresentation.listItemEdit.graph;
                if (string.IsNullOrEmpty(pEntity.id))
                {
                    //Creamos
                    pEntity.propTitle = templateSection.presentation.listItemsPresentation.listItemEdit.proptitle;
                    pEntity.propDescription = templateSection.presentation.listItemsPresentation.listItemEdit.propdescription;
                    ProcesCompossedProperties(templateSection.presentation.listItemsPresentation.listItemEdit, pEntity);
                    ProcesLoadPropertyValues(templateSection.presentation.listItemsPresentation.listItemEdit, pEntity);
                    ComplexOntologyResource resource = ToGnossApiResource(pEntity);
                    string result = mResourceApi.LoadComplexSemanticResource(resource, false, true);

                    //En el caso de añadir en un listado añadir la entidad al listado
                    if (resource.Uploaded && !string.IsNullOrEmpty(templateSection.property))
                    {
                        //Obtenemos la auxiliar en la que cargar la entidad
                        SparqlObject tab = mResourceApi.VirtuosoQuery("select *", "where{<" + pCvID + "> ?s ?o. ?o a <" + pRdfTypeTab + "> }", "curriculumvitae");
                        string idTab = tab.results.bindings[0]["o"].value;
                        string rdfTypePrefix = UtilityCV.AniadirPrefijo(templateSection.rdftype);
                        rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                        string idNewAux = "http://gnoss.com/items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(pCvID) + "_" + Guid.NewGuid();

                        List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
                        string idEntityAux = idTab + "|" + idNewAux;

                        //Privacidad, por defecto falso                    
                        string predicadoPrivacidad = template.property + "|" + templateSection.property + "|" + UtilityCV.PropertyIspublic;
                        TriplesToInclude tr2 = new TriplesToInclude(idEntityAux + "|false", predicadoPrivacidad);
                        listaTriples.Add(tr2);

                        //Entidad
                        string predicadoEntidad = template.property + "|" + templateSection.property + "|" + templateSection.presentation.listItemsPresentation.property;
                        TriplesToInclude tr1 = new TriplesToInclude(idEntityAux + "|" + result, predicadoEntidad);
                        listaTriples.Add(tr1);

                        Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>()
                        {
                            {
                                mResourceApi.GetShortGuid(pCvID), listaTriples
                            }
                        };
                        Dictionary<Guid, bool> respuesta = mResourceApi.InsertPropertiesLoadedResources(triplesToInclude);
                        return new JsonResult() { ok = respuesta[mResourceApi.GetShortGuid(pCvID)], id = idNewAux };
                    }
                    return new JsonResult() { ok = resource.Uploaded, id = result };
                }
                else
                {
                    //Modificamos
                    Entity loadedEntity = GetLoadedEntity(pEntity.id, pEntity.ontology);
                    loadedEntity.propTitle = templateSection.presentation.listItemsPresentation.listItemEdit.proptitle;
                    loadedEntity.propDescription = templateSection.presentation.listItemsPresentation.listItemEdit.propdescription;
                    ProcesCompossedProperties(templateSection.presentation.listItemsPresentation.listItemEdit, loadedEntity);
                    ProcesLoadPropertyValues(templateSection.presentation.listItemsPresentation.listItemEdit, loadedEntity);

                    bool hasChange = MergeLoadedEntity(loadedEntity, pEntity);

                    if (hasChange)
                    {
                        ComplexOntologyResource resource = ToGnossApiResource(loadedEntity);
                        mResourceApi.ModifyComplexOntologyResource(resource, false, true);
                        return new JsonResult() { ok = resource.Modified, id = pEntity.id };
                    }
                    else
                    {
                        return new JsonResult() { ok = true, id = pEntity.id };
                    }
                }
            }
            else
            {
                //Entidad
                ItemEdit itemEdit = UtilityCV.EntityTemplates.First(x => x.rdftype == pEntity.rdfType);
                mResourceApi.ChangeOntoly(itemEdit.graph);
                pEntity.ontology = itemEdit.graph;
                if (string.IsNullOrEmpty(pEntity.id) || !pEntity.id.StartsWith("http"))
                {
                    //Creamos
                    pEntity.id = "";
                    pEntity.propTitle = itemEdit.proptitle;
                    pEntity.propDescription = itemEdit.propdescription;
                    ProcesCompossedProperties(itemEdit, pEntity);
                    ProcesLoadPropertyValues(itemEdit, pEntity);
                    ComplexOntologyResource resource = ToGnossApiResource(pEntity);
                    string result = mResourceApi.LoadComplexSemanticResource(resource, false, true);
                    return new JsonResult() { ok = resource.Uploaded, id = result };
                }
                else
                {
                    //Modificamos
                    Entity loadedEntity = GetLoadedEntity(pEntity.id, pEntity.ontology);
                    loadedEntity.propTitle = itemEdit.proptitle;
                    loadedEntity.propDescription = itemEdit.propdescription;
                    bool hasChange = MergeLoadedEntity(loadedEntity, pEntity);
                    hasChange = ProcesCompossedProperties(itemEdit, loadedEntity) || ProcesLoadPropertyValues(itemEdit, loadedEntity) || hasChange;
                    if (hasChange)
                    {
                        ComplexOntologyResource resource = ToGnossApiResource(loadedEntity);
                        mResourceApi.ModifyComplexOntologyResource(resource, false, true);
                        return new JsonResult() { ok = resource.Modified, id = pEntity.id };
                    }
                    else
                    {
                        return new JsonResult() { ok = true, id = pEntity.id };
                    }
                }
            }
        }

        public object ValidateORCID(string pORCID)
        {
            pORCID = pORCID.Replace("https://orcid.org/", "");
            //1º Buscamos en las personas cargadas
            SparqlObject resultData = mResourceApi.VirtuosoQuery("select ?s", "where{?s <http://w3id.org/roh/ORCID> '" + pORCID + "'. ?s a <http://xmlns.com/foaf/0.1/Person>}order by asc(?s)", "person");
            string idPerson = "";
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                idPerson = fila["s"].value;
            }
            if (string.IsNullOrEmpty(idPerson))
            {
                //2º Si no existe recuperamos la persona de ORCID, la creamos y la devolvemos
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
                    string jsonRespuestaOrcidPerson = webClient.DownloadString("https://pub.orcid.org/v3.0/" + pORCID + "/person");
                    webClient.Dispose();
                    ORCIDPerson person = JsonConvert.DeserializeObject<ORCIDPerson>(jsonRespuestaOrcidPerson);

                    if (person != null)
                    {
                        Entity entity = new Entity();
                        entity.rdfType = "http://xmlns.com/foaf/0.1/Person";
                        entity.propTitle = "http://xmlns.com/foaf/0.1/name";
                        entity.properties = new List<Entity.Property>()
                    {
                        new Entity.Property()
                        {
                            prop = "http://xmlns.com/foaf/0.1/name",
                            values = new List<string>() { person.name.given_names.value.Trim() + " "+ person.name.family_name.value.Trim() }
                        },
                        new Entity.Property()
                        {
                            prop = "http://xmlns.com/foaf/0.1/firstName",
                            values = new List<string>() { person.name.given_names.value.Trim() }
                        },
                        new Entity.Property()
                        {
                            prop = "http://xmlns.com/foaf/0.1/lastName",
                            values = new List<string>() { person.name.family_name.value.Trim() }
                        },
                        new Entity.Property()
                        {
                            prop = "http://w3id.org/roh/ORCID",
                            values = new List<string>() {pORCID }
                        }
                    };
                        //TODO privacidad
                        mResourceApi.ChangeOntoly("person");
                        ComplexOntologyResource resource = ToGnossApiResource(entity);
                        string result = mResourceApi.LoadComplexSemanticResource(resource, false, true);
                        if (resource.Uploaded)
                        {
                            idPerson = result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO texto                
                }
            }
            if (!string.IsNullOrEmpty(idPerson))
            {
                return GetPerson(idPerson);
            }
            return new JsonResult() { ok = false, id = "", error = "El código introducido no es válio" };
        }

        public Object CreatePerson(string pName, string pSurname)
        {
            Entity entity = new Entity();
            entity.rdfType = "http://xmlns.com/foaf/0.1/Person";
            entity.propTitle = "http://xmlns.com/foaf/0.1/name";
            entity.properties = new List<Entity.Property>()
                    {
                        new Entity.Property()
                        {
                            prop = "http://xmlns.com/foaf/0.1/name",
                            values = new List<string>() { pName + " "+ pSurname }
                        },
                        new Entity.Property()
                        {
                            prop = "http://xmlns.com/foaf/0.1/firstName",
                            values = new List<string>() { pName }
                        },
                        new Entity.Property()
                        {
                            prop = "http://xmlns.com/foaf/0.1/lastName",
                            values = new List<string>() { pSurname }
                        }
                    };
            //TODO privacidad
            mResourceApi.ChangeOntoly("person");
            ComplexOntologyResource resource = ToGnossApiResource(entity);
            string result = mResourceApi.LoadComplexSemanticResource(resource, false, true);
            if (resource.Uploaded)
            {
                return GetPerson(result);
            }
            //TODO mensaje
            return new JsonResult() { ok = false, id = "", error = "Se ha producido un error al crear la persona" };
        }

        private Person GetPerson(string pIdPerson)
        {
            string select = $@"select distinct ?ORCID ?name ?departamento from <{mResourceApi.GraphsUrl}department.owl>";
            string where = $@"where
                                {{
                                    ?personID <http://xmlns.com/foaf/0.1/name> ?name.
                                    OPTIONAL{{
                                        ?personID <http://w3id.org/roh/ORCID> ?ORCID
                                    }}
                                    OPTIONAL{{
                                        ?personID <http://vivoweb.org/ontology/core#departmentOrSchool> ?depID.
                                        ?depID <http://purl.org/dc/elements/1.1/title> ?departamento.
                                    }}
                                    FILTER(?personID=<{pIdPerson}>)
                                }}";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "person");
            foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
            {
                Person persona = new Person();
                persona.name = fila["name"].value;
                persona.personid = pIdPerson;
                if (fila.ContainsKey("ORCID"))
                {
                    persona.orcid = fila["ORCID"].value;
                }
                if (fila.ContainsKey("departamento"))
                {
                    persona.department = fila["departamento"].value;
                }
                return persona;
            }
            return null;
        }

        /// <summary>
        /// Procesa las entidades configuradas como compuestas (por ejemplo el nombre completo de una persona 'nombre'+' '+ 'apellidos')
        /// </summary>
        /// <param name="pItemEdit">Configuración de item de edición</param>
        /// <param name="pLoadedEntity">Datos de la entidad</param>
        /// <returns></returns>
        private bool ProcesCompossedProperties(ItemEdit pItemEdit, Entity pLoadedEntity)
        {
            bool changes = false;
            foreach (ItemEditSection section in pItemEdit.sections)
            {
                foreach (ItemEditSectionRow sectionRow in section.rows)
                {
                    changes = changes || ProcesCompossedPropertieRow(sectionRow, pLoadedEntity, "");
                }
            }
            return changes;
        }

        /// <summary>
        /// Procesa las entidades configuradas como compuestas (por ejemplo el nombre completo de una persona 'nombre'+' '+ 'apellidos')
        /// </summary>
        /// <param name="pItemEdit">Configuración de item de edición</param>
        /// <param name="pLoadedEntity">Datos de la entidad</param>
        /// <returns></returns>
        private bool ProcesLoadPropertyValues(ItemEdit pItemEdit, Entity pLoadedEntity)
        {
            bool changes = false;
            if (pItemEdit.loadPropertyValues != null && pItemEdit.loadPropertyValues.Count > 0)
            {
                foreach (LoadPropertyValues propertyValue in pItemEdit.loadPropertyValues)
                {
                    Entity.Property prop = new Entity.Property()
                    {
                        prop = propertyValue.property,
                        values = propertyValue.values
                    };
                    Entity.Property propLoad = pLoadedEntity.properties.FirstOrDefault(x => x.prop == prop.prop);
                    if (propLoad == null)
                    {
                        pLoadedEntity.properties.Add(prop);
                        changes = true;
                    }
                    else if (propLoad.values.Union(prop.values).Count() != prop.values.Count)
                    {
                        propLoad.values = prop.values;
                    }
                }
            }
            return changes;
        }



        /// <summary>
        /// Procesa las entidades configuradas como compuestas (por ejemplo el nombre completo de una persona 'nombre'+' '+ 'apellidos') (recursivo)
        /// </summary>
        /// <param name="pItemEditSectionRow">Configuración de item de edición</param>
        /// <param name="pLoadedEntity">Datos de la entidad</param>
        /// <param name="pAcumulado">Propiedad acumulada</param>
        /// <returns></returns>
        private bool ProcesCompossedPropertieRow(ItemEditSectionRow pItemEditSectionRow, Entity pLoadedEntity, string pAcumulado)
        {
            bool changes = false;
            foreach (ItemEditSectionRowProperty sectionRowProperty in pItemEditSectionRow.properties)
            {
                if (sectionRowProperty.auxEntityData != null)
                {
                    foreach (ItemEditSectionRow sectionRow in sectionRowProperty.auxEntityData.rows)
                    {
                        changes = changes || ProcesCompossedPropertieRow(sectionRow, pLoadedEntity, pAcumulado + "||" + sectionRowProperty.property);
                    }
                }
                if (!string.IsNullOrEmpty(sectionRowProperty.compossed))
                {
                    string propCompossed = pAcumulado + "||" + sectionRowProperty.property;
                    string[] propCompossedSplit = sectionRowProperty.compossed.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    string value = "";
                    foreach (string item in propCompossedSplit)
                    {
                        value += " " + pLoadedEntity.properties.FirstOrDefault(x => x.prop == item)?.values?.First();
                        value = value.Trim();
                    }
                    if (!string.IsNullOrEmpty(value))
                    {
                        Entity.Property prop = new Entity.Property()
                        {
                            prop = sectionRowProperty.property,
                            values = new List<string>() { value }
                        };
                        pLoadedEntity.properties.Add(prop);
                    }
                }
            }
            return changes;
        }

        /// <summary>
        /// Obtiene sujetos y propiedades de las anteriores entidades de una entidad dentro de un CV
        /// </summary>
        /// <param name="pEntity">Entidad desde la que partimos</param>
        /// <param name="pProperty">Propiedad desde la que partimos</param>
        /// <returns></returns>
        private KeyValuePair<List<string>, List<string>> GetSubjectsAndPropertiesFromAuxCV(string pEntity, string pProperty)
        {
            List<string> properties = new List<string>();
            List<string> entities = new List<string>() { pEntity };
            if (!string.IsNullOrEmpty(pProperty))
            {
                properties.Add(pProperty);
            }
            bool continuar = true;
            while (continuar)
            {
                continuar = false;
                SparqlObject resultData = mResourceApi.VirtuosoQuery("select *", "where{?s ?p <" + pEntity + ">. }", "curriculumvitae");
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    if (fila["p"].value != "http://gnoss/hasEntidad")
                    {
                        continuar = true;
                        properties.Add(fila["p"].value);
                        entities.Add(fila["s"].value);
                        pEntity = fila["s"].value;
                    }
                }
            }
            properties.Reverse();
            entities.Reverse();
            entities.RemoveAt(0);
            return new KeyValuePair<List<string>, List<string>>(entities, properties);
        }

        /// <summary>
        /// Fusiona dos entidades
        /// </summary>
        /// <param name="pLoadedEntity">Entidad cargada en BBDD</param>
        /// <param name="pUpdatedEntity">Nueva entidad</param>
        /// <returns>Devuelve true si ha detetado cambios</returns>
        private bool MergeLoadedEntity(Entity pLoadedEntity, Entity pUpdatedEntity)
        {
            bool change = false;

            foreach (Entity.Property property in pUpdatedEntity.properties)
            {
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
                    List<Entity.Property> propertiesLoadedEntityRemove = pLoadedEntity.properties.Where(x => x.prop.StartsWith(property.prop)).ToList();
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
        /// Fusiona dos entidades
        /// </summary>
        /// <param name="pPropertyIDs">Propiedades que apuntan a la auxiliar separadas por '|'</param>
        /// <param name="pEntityIDs">Entidades que apuntan a la auxiliar separadas por '|'</param>
        /// <param name="pLoadedEntity">Entidad cargada en BBDD</param>
        /// <param name="pUpdatedEntity">Nueva entidad</param>
        /// <returns>Devuelve true si se ha actualizado correctamente</returns>
        private bool UpdateEntityAux(List<string> pPropertyIDs, List<string> pEntityIDs, Entity pLoadedEntity, Entity pUpdatedEntity)
        {
            bool update = true;
            Guid guid = mResourceApi.GetShortGuid(pLoadedEntity.id);
            Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToInclude>> triplesInclude = new Dictionary<Guid, List<TriplesToInclude>>() { { guid, new List<TriplesToInclude>() } };
            Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>> triplesRemove = new Dictionary<Guid, List<RemoveTriples>>() { { guid, new List<RemoveTriples>() } };
            Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToModify>> triplesModify = new Dictionary<Guid, List<TriplesToModify>>() { { guid, new List<TriplesToModify>() } };

            foreach (Entity.Property property in pUpdatedEntity.properties)
            {
                bool remove = property.values == null || property.values.Count == 0 || !property.values.Exists(x => !string.IsNullOrEmpty(x));
                //Recorremos las propiedades de la entidad a actualizar y modificamos la entidad recuperada de BBDD               
                Entity.Property propertyLoadedEntity = pLoadedEntity.properties.FirstOrDefault(x => x.prop == property.prop);
                if (propertyLoadedEntity != null)
                {
                    if (remove)
                    {
                        foreach (string valor in propertyLoadedEntity.values)
                        {
                            triplesRemove[guid].Add(new RemoveTriples()
                            {
                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(guid, valor, property.prop)
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
                                    triplesModify[guid].Add(new TriplesToModify()
                                    {

                                        Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                        NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(guid, valuesEntity[0], property.prop),
                                        OldValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(guid, valuesLoadedEntity[0], property.prop)
                                    });
                                }
                                else
                                {
                                    //Eliminaciones
                                    foreach (string valor in valuesLoadedEntity.Except(property.values))
                                    {
                                        triplesRemove[guid].Add(new RemoveTriples()
                                        {

                                            Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                            Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(guid, valor, property.prop)
                                        });
                                    }
                                    //Inserciones
                                    foreach (string valor in valuesEntity.Except(valuesLoadedEntity))
                                    {
                                        if (!valor.EndsWith("@@@"))
                                        {
                                            triplesInclude[guid].Add(new TriplesToInclude()
                                            {

                                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                                NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(guid, valor, property.prop)
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
                            triplesInclude[guid].Add(new TriplesToInclude()
                            {

                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(guid, valor, property.prop)
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
                            triplesRemove[guid].Add(new RemoveTriples()
                            {

                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(guid, valor, property.prop)
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
                                    triplesRemove[guid].RemoveAll(x => x.Value.Contains(auxEntityRemove));
                                    //Eliminamos la entidad auxiliar
                                    triplesRemove[guid].Add(new RemoveTriples()
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
            if (triplesRemove[guid].Count > 0)
            {
                update = update && mResourceApi.DeletePropertiesLoadedResources(triplesRemove)[guid];
            }
            if (triplesInclude[guid].Count > 0)
            {
                update = update && mResourceApi.InsertPropertiesLoadedResources(triplesInclude)[guid];
            }
            if (triplesModify[guid].Count > 0)
            {
                update = update && mResourceApi.ModifyPropertiesLoadedResources(triplesModify)[guid];
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


        /// <summary>
        /// Crea un ComplexOntologyResource con los datos de una entidad para su carga
        /// </summary>
        /// <param name="pEntity">Datos de una entidad</param>
        /// <returns>ComplexOntologyResource</returns>
        private ComplexOntologyResource ToGnossApiResource(Entity pEntity)
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

        /// <summary>
        /// Obtiene todos los datos de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns>Entidad completa</returns>
        private Entity GetLoadedEntity(string pId, string pGraph)
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
                    string whereID = $"where{{?x <http://gnoss/hasEntidad> <{pId}>.?x <http://gnoss/hasEntidad> ?s. ?s ?p ?o }}order by desc(?s) desc(?p) desc(?o)}} limit {numLimit} offset {offset}";
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
    }
}