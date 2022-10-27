using EditorCV.Models;
using EditorCV.Models.API;
using EditorCV.Models.API.Input;
using EditorCV.Models.API.Response;
using EditorCV.Models.API.Templates;
using EditorCV.Models.ORCID;
using EditorCV.Models.Similarity;
using EditorCV.Models.Utils;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.ResearcherObjectLoad.Models.NotificationOntology;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace EditorCV.Models
{
    /// <summary>
    /// Clase utilizada para las acciones de modificación de datos en el CV
    /// </summary>
    public class AccionesGuardado
    {
        /// <summary>
        /// API
        /// </summary>
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");

        /// <summary>
        /// Cambia la privacidad de un item
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntity">Identificador de la entidad</param>
        /// <param name="pIsPublic">TRUE si es público</param>
        /// <returns></returns>
        public JsonResult ChangePrivacityItem(ConfigService pConfigService, string pIdSection, string pRdfTypeTab, string pEntity, bool pIsPublic)
        {
            TabSectionListItem presentationMini = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation.listItemsPresentation.listItem;

            mResourceApi.ChangeOntoly("curriculumvitae");
            Tuple<List<string>, List<string>, List<string>> subjectPropertiesAndRdfTypes = GetSubjectsPropertiesAndRdftypesFromAuxCV(pEntity, UtilityCV.PropertyIspublic);
            List<string> properties = subjectPropertiesAndRdfTypes.Item2;
            List<string> entities = subjectPropertiesAndRdfTypes.Item1.GetRange(1, subjectPropertiesAndRdfTypes.Item2.Count - 1);

            //Obtenemos el valor actual de la propiedad
            string valorActual = "";
            SparqlObject resultDataProperty = mResourceApi.VirtuosoQuery("select *", "where{<" + pEntity + "> <" + UtilityCV.PropertyIspublic + "> ?o. }", "curriculumvitae");
            if (resultDataProperty.results.bindings.Count > 0)
            {
                valorActual = resultDataProperty.results.bindings[0]["o"].value;
            }

            //Insertamos en la cola del desnormalizador
            RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new RabbitServiceWriterDenormalizer(pConfigService);
            rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.person, new HashSet<string> { UtilityCV.GetPersonFromCV(subjectPropertiesAndRdfTypes.Item1[0]) }));


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
        /// <param name="pConfigService">Configuración</param>
        /// <param name="pEntity">Entidad a eliminar</param>
        /// <returns></returns>
        public JsonResult RemoveItem(ConfigService pConfigService, string pEntity)
        {
            string accion = "delete";
            //Obtenemos la entidad para luego borrarla si es necesario
            string entityDestino = "";
            string entityCV = "";
            SparqlObject resultadoEntityCV = mResourceApi.VirtuosoQuery("select ?idCV ?idEntity", "where{?idCV ?p ?o. ?o ?p2 ?idAux. ?idAux <http://vivoweb.org/ontology/core#relatedBy> ?idEntity. FILTER(?idAux=<" + pEntity + ">) FILTER(?p!=<http://gnoss/hasEntidad>) }", "curriculumvitae");
            entityDestino = resultadoEntityCV.results.bindings.First()["idEntity"].value;
            entityCV = resultadoEntityCV.results.bindings.First()["idCV"].value;

            mResourceApi.ChangeOntoly("curriculumvitae");
            Tuple<List<string>, List<string>, List<string>> subjectPropertiesAndRdfTypes = GetSubjectsPropertiesAndRdftypesFromAuxCV(pEntity, "");
            List<string> entities = subjectPropertiesAndRdfTypes.Item1;
            List<string> properties = subjectPropertiesAndRdfTypes.Item2;
            List<string> rdftypes = subjectPropertiesAndRdfTypes.Item3;

            API.Templates.Tab template = UtilityCV.TabTemplates.First(x => x.rdftype == rdftypes[1]);
            API.Templates.TabSection templateSection = template.sections.First(x => x.property == properties[1]);

            string personCV = UtilityCV.GetPersonFromCV(entities.First());
            if (templateSection.presentation.listItemsPresentation.listItemEdit.propAuthor != null)
            {
                //Si tiene autores eliminamos la persona actual de la lista de autores de la entidad                
                Entity entityBBDD = GetLoadedEntity(entityDestino, templateSection.presentation.listItemsPresentation.listItemEdit.graph);
                if (entityBBDD != null)
                {
                    if (entityBBDD.properties.Exists(x => x.prop == templateSection.presentation.listItemsPresentation.listItemEdit.propAuthor.property))
                    {
                        Entity.Property property = entityBBDD.properties.FirstOrDefault(x => x.prop == templateSection.presentation.listItemsPresentation.listItemEdit.propAuthor.property);
                        if (property != null && property.values != null)
                        {
                            string autorEliminar = property.values.FirstOrDefault(x => x.Contains(personCV));
                            if (!string.IsNullOrEmpty(autorEliminar))
                            {
                                string auxEntityEliminar = autorEliminar.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries)[0];
                                RemoveTriples t = new();
                                t.Predicate = templateSection.presentation.listItemsPresentation.listItemEdit.propAuthor.property.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries)[0];
                                t.Value = auxEntityEliminar;
                                mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { mResourceApi.GetShortGuid(entityDestino), new List<RemoveTriples>() { t } } });
                            }
                        }
                    }

                    Thread thread = new Thread(() => ModificacionNotificacion(entityBBDD, template, templateSection, personCV, accion));
                    thread.Start();
                }
                //Si estaba validado añadimos en la persona que hay que ignorar el item
                if (entityBBDD.properties.Exists(x => x.prop == "http://w3id.org/roh/isValidated" && x.values != null && x.values.Select(x => x.ToLower()).Contains("true")))
                {
                    Dictionary<string, string> ignorar = new Dictionary<string, string>();
                    //DOI
                    Entity.Property doi = entityBBDD.properties.FirstOrDefault(x => x.prop == "http://purl.org/ontology/bibo/doi");
                    if (doi != null)
                    {
                        ignorar["doi"] = doi.values[0];
                    }
                    //HANDLE
                    Entity.Property handle = entityBBDD.properties.FirstOrDefault(x => x.prop == "http://purl.org/ontology/bibo/handle");
                    if (handle != null)
                    {
                        ignorar["handle"] = handle.values[0];
                    }
                    //PMID
                    Entity.Property pmid = entityBBDD.properties.FirstOrDefault(x => x.prop == "http://purl.org/ontology/bibo/pmid");
                    if (pmid != null)
                    {
                        ignorar["pmid"] = pmid.values[0];
                    }
                    //Otros identificadores
                    Entity.Property nombreIdentificador = entityBBDD.properties.FirstOrDefault(x => x.prop == "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic");
                    Entity.Property valorIdentificador = entityBBDD.properties.FirstOrDefault(x => x.prop == "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title");
                    if (nombreIdentificador != null & valorIdentificador != null && nombreIdentificador.values != null && valorIdentificador.values != null)
                    {
                        foreach (string identificador in nombreIdentificador.values)
                        {
                            string idbase = identificador.Split("@@@")[0];
                            string nombreIdentificadorActual = identificador.Split("@@@")[1];
                            string valor = valorIdentificador.values.FirstOrDefault(x => x.StartsWith(idbase + "@@@"));
                            if (!string.IsNullOrEmpty(valor))
                            {
                                string valorIdentificadorActual = valor.Split("@@@")[1];
                                ignorar[nombreIdentificadorActual] = valorIdentificadorActual;
                            }
                        }
                    }


                    Guid guid = mResourceApi.GetShortGuid(personCV);
                    Dictionary<Guid, List<TriplesToInclude>> triples = new() { { guid, new List<TriplesToInclude>() } };
                    foreach (string nombreIdentificadorIgnorar in ignorar.Keys)
                    {
                        string idAux = mResourceApi.GraphsUrl + "items/IgnorePublication_" + guid.ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                        TriplesToInclude t1 = new();
                        t1.Predicate = "http://w3id.org/roh/ignorePublication|http://xmlns.com/foaf/0.1/topic";
                        t1.NewValue = idAux + "|" + nombreIdentificadorIgnorar;
                        triples[guid].Add(t1);
                        TriplesToInclude t2 = new();
                        t2.Predicate = "http://w3id.org/roh/ignorePublication|http://w3id.org/roh/title";
                        t2.NewValue = idAux + "|" + ignorar[nombreIdentificadorIgnorar];
                        triples[guid].Add(t2);
                    }
                    var resultado = mResourceApi.InsertPropertiesLoadedResources(triples);

                }

            }

            List<RemoveTriples> lista = new List<RemoveTriples>();
            Dictionary<Guid, List<RemoveTriples>> dicEliminar = new Dictionary<Guid, List<RemoveTriples>>();
            RemoveTriples rt = new RemoveTriples() { Predicate = string.Join("|", properties), Value = string.Join("|", entities.GetRange(1, entities.Count - 1)) };
            lista.Add(rt);
            dicEliminar.Add(mResourceApi.GetShortGuid(pEntity), lista);
            Dictionary<Guid, bool> dicCorrecto = mResourceApi.DeletePropertiesLoadedResources(dicEliminar);

            //Elimianmos los valores multiidioma de la entidad
            Dictionary<string, Dictionary<string, List<MultilangProperty>>> propiedadesActualesAux = UtilityCV.GetMultilangPropertiesCV(entityCV, entityDestino);
            Dictionary<string, List<MultilangProperty>> propiedadesActuales = new Dictionary<string, List<MultilangProperty>>();
            if (propiedadesActualesAux.ContainsKey(entityDestino))
            {
                propiedadesActuales = propiedadesActualesAux[entityDestino];
            }
            Dictionary<string, List<MultilangProperty>> propiedadesNuevas = new Dictionary<string, List<MultilangProperty>>();
            UpdateMultilangProperties(propiedadesActuales, propiedadesNuevas, entityCV, entityDestino);

            //Si la entidaad no está referenciada desde ningún CV se elimina también la entidad
            if (mResourceApi.VirtuosoQuery("select ?cv", @$"where{{?cv a <http://w3id.org/roh/CV>. ?cv ?p1 ?lv1.?lv1 ?p2 ?lv2. ?lv2 ?p3 <{entityDestino}>}}", "curriculumvitae").results.bindings.Count == 0)
            {
                try
                {
                    mResourceApi.PersistentDelete(mResourceApi.GetShortGuid(entityDestino), true);
                }
                catch (Exception)
                {

                }
            }

            RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new RabbitServiceWriterDenormalizer(pConfigService);
            Dictionary<string, DenormalizerItemQueue.ItemType> tiposDesnormalizar = new Dictionary<string, DenormalizerItemQueue.ItemType>();
            tiposDesnormalizar.Add("Document_", DenormalizerItemQueue.ItemType.document);
            tiposDesnormalizar.Add("ResearchObject_", DenormalizerItemQueue.ItemType.researchobject);
            tiposDesnormalizar.Add("Group_", DenormalizerItemQueue.ItemType.group);
            tiposDesnormalizar.Add("Project_", DenormalizerItemQueue.ItemType.project);
            string claveDiccionario = tiposDesnormalizar.Keys.Where(x => entityDestino.Contains(x)).FirstOrDefault();
            if (claveDiccionario != null && tiposDesnormalizar.ContainsKey(claveDiccionario))
            {
                rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(tiposDesnormalizar[claveDiccionario], new HashSet<string> { entityDestino }));
            }
            rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.person, new HashSet<string> { personCV }));

            return new JsonResult() { ok = dicCorrecto[mResourceApi.GetShortGuid(pEntity)] };
        }

        /// <summary>
        /// Crea/actualiza una entidad
        /// </summary>
        /// <param name="pConfigService">Configuración</param>
        /// <param name="pEntity">Datos de la entidad a crear/actualizar</param>
        /// <param name="pCvID">Identificador del CV</param>
        /// <param name="pSectionID">Identifiador de la sección (para la edición de un item de un listado)</param>
        /// <param name="pRdfTypeTab">rdf:type de la pestaña (para la edición de un item de un listado)</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        public JsonResult ActualizarEntidad(ConfigService pConfigService, Entity pEntity, string pCvID, string pSectionID, string pRdfTypeTab, string pLang)
        {
            string accion = "";
            string personCV = UtilityCV.GetPersonFromCV(pCvID);
            API.Templates.Tab template = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab);

            //Comprobamos campos con expresiones regulares            
            JsonResult error = ComprobarErrores(template, pEntity, pSectionID, pRdfTypeTab,pLang);
            if (error != null)
            {
                return error;
            }



            if (pRdfTypeTab == "http://w3id.org/roh/PersonalData")
            {
                //Modificamos
                Entity loadedEntity = GetLoadedEntity(pEntity.id, "curriculumvitae");
                bool updated = UpdateEntityAux(mResourceApi.GetShortGuid(pCvID), new List<string>() { "http://w3id.org/roh/personalData" }, new List<string>() { pEntity.id }, loadedEntity, pEntity);

                if (updated)
                {
                    return new JsonResult() { ok = true, id = pEntity.id };
                }
                else
                {
                    return new JsonResult() { ok = false };
                }

            }
            else
            {
                //Item de CV
                API.Templates.TabSection templateSection = template.sections.First(x => x.property == pSectionID);

                ItemEdit itemEditConfig = null;
                if (templateSection.presentation.listItemsPresentation != null)
                {
                    List<ItemEditSectionRowProperty> propsUnique = templateSection.presentation.listItemsPresentation.listItemEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).Where(x => x.unique).ToList();
                    if (propsUnique.Count > 0)
                    {
                        Entity loadedEntity = null;
                        if (!string.IsNullOrEmpty(pEntity.id) && !Guid.TryParse(pEntity.id, out Guid xx))
                        {
                            loadedEntity = GetLoadedEntity(pEntity.id, templateSection.presentation.listItemsPresentation.listItemEdit.graph);
                        }

                        foreach (ItemEditSectionRowProperty prop in propsUnique)
                        {
                            //Si se ha cambiado la propiedad comprobamos que no exista otra entidad con esa propiedad
                            bool cambiado = false;
                            string valorCargar = pEntity.properties.Where(x => x.prop == prop.property).SelectMany(x => x.values).ToList().FirstOrDefault();
                            if (loadedEntity == null)
                            {
                                cambiado = true;
                            }
                            else
                            {
                                string valorCargado = loadedEntity.properties.Where(x => x.prop == prop.property).SelectMany(x => x.values).ToList().FirstOrDefault();
                                cambiado = valorCargado?.ToLower() != valorCargar?.ToLower();
                            }

                            if (cambiado && !string.IsNullOrEmpty(valorCargar))
                            {
                                //Comprobamos que no exista otra entidad con esta propiedad
                                string select = "select ?s";
                                string where = $@"where{{?s <{prop.property}> ?id. FILTER(lcase(?id)='{valorCargar.Replace("'", "\\'").ToLower()}')}}";
                                var existe = mResourceApi.VirtuosoQuery(select, where, templateSection.presentation.listItemsPresentation.listItemEdit.graph);
                                if (existe.results.bindings.Count > 0)
                                {
                                    return new JsonResult() { ok = false, error = "PROPREPETIDA|" + prop.title[pLang] };
                                }
                            }
                        }
                    }

                    mResourceApi.ChangeOntoly(templateSection.presentation.listItemsPresentation.listItemEdit.graph);
                    pEntity.ontology = templateSection.presentation.listItemsPresentation.listItemEdit.graph;
                    itemEditConfig = templateSection.presentation.listItemsPresentation.listItemEdit;
                    if (templateSection.presentation.listItemsPresentation.listItemEdit.propAuthor != null)
                    {

                        if (!pEntity.properties.Exists(x => x.prop == templateSection.presentation.listItemsPresentation.listItemEdit.propAuthor.property) ||
                            !pEntity.properties.First(x => x.prop == templateSection.presentation.listItemsPresentation.listItemEdit.propAuthor.property).values.Exists(x => x.Contains(personCV)))
                        {
                            return new JsonResult() { ok = false, error = UtilityCV.GetTextLang(pLang, templateSection.presentation.listItemsPresentation.listItemEdit.propAuthor.message) };
                        }
                    }
                    if (!string.IsNullOrEmpty(itemEditConfig.cvnsection))
                    {
                        pEntity.properties.Add(new Entity.Property()
                        {
                            prop = "http://w3id.org/roh/cvnCode",
                            values = new List<string>()
                            {
                                itemEditConfig.cvnsection
                            }
                        });
                    }
                    if (!string.IsNullOrEmpty(itemEditConfig.propertyowner))
                    {
                        pEntity.properties.Add(new Entity.Property()
                        {
                            prop = itemEditConfig.propertyowner,
                            values = new List<string>()
                            {
                                personCV
                            }
                        });
                    }
                }
                else if (templateSection.presentation.itemPresentation != null)
                {
                    mResourceApi.ChangeOntoly(templateSection.presentation.itemPresentation.itemEdit.graph);
                    pEntity.ontology = templateSection.presentation.itemPresentation.itemEdit.graph;
                    itemEditConfig = templateSection.presentation.itemPresentation.itemEdit;
                }
                else
                {
                    throw new Exception("Código no implementado");
                }

                //Almacenamos las propiedades multiidioma para posteriormente procesarlas
                List<Entity.Property> propiedadesMultiidiomaNoCV = new List<Entity.Property>();
                propiedadesMultiidiomaNoCV = pEntity.properties.Where(x => x.valuesmultilang != null).ToList();

                string entityID = "";
                string entityIDResponse = "";
                //Entidad externa CV
                if (string.IsNullOrEmpty(pEntity.id) || Guid.TryParse(pEntity.id, out Guid x))
                {
                    //Creamos
                    accion = "create";

                    pEntity.propTitle = itemEditConfig.proptitle;
                    pEntity.propDescription = itemEditConfig.propdescription;
                    ProcesCompossedProperties(itemEditConfig, pEntity);
                    ProcesLoadPropertyValues(itemEditConfig, pEntity);
                    if (pEntity.ontology != "curriculumvitae")
                    {
                        //Creamos el recurso que no pertenece al CV
                        ComplexOntologyResource resource = ToGnossApiResource(pEntity);
                        string result = mResourceApi.LoadComplexSemanticResource(resource, false, true);
                        if (!resource.Uploaded)
                        {
                            return new JsonResult() { ok = false };
                        }
                        //En el caso de añadir en un listado añadir la entidad al listado
                        if (resource.Uploaded && !string.IsNullOrEmpty(templateSection.property))
                        {
                            //Obtenemos la auxiliar en la que cargar la entidad
                            SparqlObject tab = mResourceApi.VirtuosoQuery("select *", "where{<" + pCvID + "> ?s ?o. ?o a <" + pRdfTypeTab + "> }", "curriculumvitae");
                            string idTab = tab.results.bindings[0]["o"].value;
                            string rdfTypePrefix = UtilityCV.AniadirPrefijo(templateSection.rdftype);
                            rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                            string idNewAux = $"{mResourceApi.GraphsUrl}items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(pCvID) + "_" + Guid.NewGuid();

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
                            if (!respuesta[mResourceApi.GetShortGuid(pCvID)])
                            {
                                return new JsonResult() { ok = false, id = idNewAux };
                            }
                            entityID = result;
                            entityIDResponse = idNewAux;
                        }
                    }
                    else
                    {
                        string select = "   select ?id";
                        string where = $@"  where{{
                                                <{pCvID}> <{template.property}> ?id.
                                            }}";
                        //Creamos el recurso que pertenece al CV
                        string id1 = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae").results.bindings.First()["id"].value;
                        string nombreEntidad2 = templateSection.rdftype;
                        if (nombreEntidad2.Contains("#"))
                        {
                            nombreEntidad2 = nombreEntidad2.Substring(nombreEntidad2.LastIndexOf("#") + 1);
                        }
                        if (nombreEntidad2.Contains("/"))
                        {
                            nombreEntidad2 = nombreEntidad2.Substring(nombreEntidad2.LastIndexOf("/") + 1);
                        }
                        string id2 = mResourceApi.GraphsUrl + "items/" + nombreEntidad2 + "_" + mResourceApi.GetShortGuid(pCvID).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                        string nombreEntidad3 = pEntity.rdfType;
                        if (nombreEntidad3.Contains("#"))
                        {
                            nombreEntidad3 = nombreEntidad3.Substring(nombreEntidad3.LastIndexOf("#") + 1);
                        }
                        if (nombreEntidad3.Contains("/"))
                        {
                            nombreEntidad3 = nombreEntidad3.Substring(nombreEntidad3.LastIndexOf("/") + 1);
                        }
                        string id3 = mResourceApi.GraphsUrl + "items/" + nombreEntidad3 + "_" + mResourceApi.GetShortGuid(pCvID).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();

                        bool updated = UpdateEntityAux(mResourceApi.GetShortGuid(pCvID), new List<string>() { template.property, templateSection.property, templateSection.presentation.itemPresentation.property }, new List<string>() { id1, id2, id3 }, null, pEntity);

                        if (!updated)
                        {
                            return new JsonResult() { ok = false };
                        }
                        entityID = pEntity.id;
                        entityIDResponse = id3;
                    }
                }
                else
                {
                    //Modificamos
                    accion = "edit";

                    //Si está bloqueado sólo hay que editar los campos editables                   
                    List<PropertyData> propertyDatas = new List<PropertyData>();
                    foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
                    {
                        propertyDatas.Add(
                            //Editabilidad
                            new Utils.PropertyData()
                            {
                                property = propEditabilidad,
                                childs = new List<Utils.PropertyData>()
                            }
                        );
                    }

                    var dataBlock = UtilityCV.GetProperties(new HashSet<string>() { pEntity.id }, pEntity.ontology, propertyDatas, "", new Dictionary<string, SparqlObject>());

                    bool editable = true;
                    foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
                    {
                        string valorPropiedad = AccionesEdicion.GetPropValues(pEntity.id, propEditabilidad, dataBlock).FirstOrDefault();
                        if ((Utils.UtilityCV.PropertyNotEditable[propEditabilidad] == null || Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Count == 0) && !string.IsNullOrEmpty(valorPropiedad))
                        {
                            editable = false;
                        }
                        else if (Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Contains(valorPropiedad))
                        {
                            editable = false;
                        }
                    }

                    if (!editable)
                    {
                        //Si no es editable eliminamos las propiedades que no sean editables
                        List<string> propertiesEditables = templateSection.presentation.listItemsPresentation.listItemEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).Where(x => x.editable).Select(x => x.property).ToList();
                        pEntity.properties.RemoveAll(x => !propertiesEditables.Contains(x.prop.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries)[0]));
                    }
                    else
                    {
                        //Si es editable eliminamos las propiedades bloqueadas
                        if (templateSection.presentation.listItemsPresentation != null)
                        {
                            List<string> propertiesBloqueadas = templateSection.presentation.listItemsPresentation.listItemEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).Where(x => x.blocked).Select(x => x.property).ToList();
                            pEntity.properties.RemoveAll(x => propertiesBloqueadas.Contains(x.prop.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries)[0]));
                        }
                    }

                    Entity loadedEntity = GetLoadedEntity(pEntity.id, pEntity.ontology);
                    loadedEntity.propTitle = itemEditConfig.proptitle;
                    loadedEntity.propDescription = itemEditConfig.propdescription;
                    ProcesCompossedProperties(itemEditConfig, loadedEntity);
                    ProcesLoadPropertyValues(itemEditConfig, loadedEntity);
                    if (pEntity.ontology != "curriculumvitae")
                    {
                        bool hasChange = MergeLoadedEntity(loadedEntity, pEntity);

                        if (hasChange)
                        {
                            ComplexOntologyResource resource = ToGnossApiResource(loadedEntity);
                            mResourceApi.ModifyComplexOntologyResource(resource, false, true);
                            if (!resource.Modified)
                            {
                                return new JsonResult() { ok = resource.Modified, id = pEntity.id };
                            }
                        }
                        entityID = pEntity.id;
                    }
                    else
                    {
                        //Modificamos
                        string select1 = "   select ?id1";
                        string where1 = $@"  where{{
                                                <{pCvID}> <{template.property}> ?id1.
                                            }}";
                        string id1 = mResourceApi.VirtuosoQuery(select1, where1, "curriculumvitae").results.bindings.First()["id1"].value;

                        string select2 = "   select ?id2";
                        string where2 = $@"  where{{
                                                <{id1}> <{templateSection.property}> ?id2.
                                            }}";
                        string id2 = mResourceApi.VirtuosoQuery(select2, where2, "curriculumvitae").results.bindings.First()["id2"].value;

                        string select3 = "   select ?id3";
                        string where3 = $@"  where{{
                                                <{id2}> <{templateSection.presentation.itemPresentation.property}> ?id3.
                                            }}";
                        string id3 = mResourceApi.VirtuosoQuery(select3, where3, "curriculumvitae").results.bindings.First()["id3"].value;

                        bool updated = UpdateEntityAux(mResourceApi.GetShortGuid(pCvID), new List<string>() { template.property, templateSection.property, templateSection.presentation.itemPresentation.property }, new List<string>() { id1, id2, id3 }, loadedEntity, pEntity);

                        if (!updated)
                        {
                            return new JsonResult() { ok = false };
                        }
                        entityID = pEntity.id;
                    }
                    entityIDResponse = entityID;
                }

                //Entidad CV
                if (templateSection.presentation.listItemsPresentation != null
                    && !string.IsNullOrEmpty(templateSection.presentation.listItemsPresentation.property_cv)
                    && !string.IsNullOrEmpty(templateSection.presentation.listItemsPresentation.rdftype_cv))
                {
                    //Obtenemos la auxiliar en la que cargar la entidad
                    SparqlObject tab = mResourceApi.VirtuosoQuery("select *", "where{<" + pCvID + "> ?s ?o. ?o a <" + pRdfTypeTab + "> }", "curriculumvitae");
                    string idTab = tab.results.bindings[0]["o"].value;

                    string idEntity = mResourceApi.VirtuosoQuery("select *", "where{?s a <" + templateSection.rdftype + ">. ?s <" + templateSection.presentation.listItemsPresentation.property + "> <" + entityID + "> }", "curriculumvitae").results.bindings[0]["s"].value;

                    SparqlObject entityCV = mResourceApi.VirtuosoQuery("select *", "where{<" + idEntity + "> <" + templateSection.presentation.listItemsPresentation.property_cv + "> ?o. ?o a <" + templateSection.presentation.listItemsPresentation.rdftype_cv + "> }", "curriculumvitae");
                    string entityCVID = "";
                    if (entityCV.results.bindings.Count > 0)
                    {
                        //existe
                        entityCVID = entityCV.results.bindings[0]["o"].value;
                    }
                    else
                    {
                        //no existe
                        string rdfTypePrefix = UtilityCV.AniadirPrefijo(templateSection.presentation.listItemsPresentation.rdftype_cv);
                        rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                        entityCVID = $"{mResourceApi.GraphsUrl}items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(pCvID) + "_" + Guid.NewGuid();
                    }

                    List<string> propertyIDs = new List<string>()
                    {
                        template.property,
                        templateSection.property,
                        templateSection.presentation.listItemsPresentation.property_cv
                    };
                    List<string> entityIDs = new List<string>()
                    {
                        idTab,
                        idEntity,
                        entityCVID
                    };

                    Entity entityToLoad = new Entity();
                    entityToLoad.id = entityCVID;
                    entityToLoad.ontology = "curriculumvitae";
                    entityToLoad.properties = pEntity.properties_cv;
                    entityToLoad.rdfType = templateSection.presentation.listItemsPresentation.rdftype_cv;
                    Entity entityBBDD = GetLoadedEntity(entityCVID, "curriculumvitae");
                    if (!UpdateEntityAux(mResourceApi.GetShortGuid(pCvID), propertyIDs, entityIDs, entityBBDD, entityToLoad))
                    {
                        return new JsonResult() { ok = false, id = entityID };
                    }
                }

                //Actualizamos las propiedades multiidioma
                {
                    //Entidad entityID
                    Dictionary<string, Dictionary<string, List<MultilangProperty>>> propiedadesActualesAux = UtilityCV.GetMultilangPropertiesCV(pCvID, entityID);
                    Dictionary<string, List<MultilangProperty>> propiedadesActuales = new Dictionary<string, List<MultilangProperty>>();
                    if (propiedadesActualesAux.ContainsKey(entityID))
                    {
                        propiedadesActuales = propiedadesActualesAux[entityID];
                    }

                    Dictionary<string, List<MultilangProperty>> propiedadesNuevas = new Dictionary<string, List<MultilangProperty>>();
                    if (propiedadesMultiidiomaNoCV != null)
                    {
                        foreach (Entity.Property propMultiidiomaNoCV in propiedadesMultiidiomaNoCV)
                        {
                            if (propMultiidiomaNoCV.valuesmultilang != null)
                            {
                                foreach (string idioma in propMultiidiomaNoCV.valuesmultilang.Keys)
                                {
                                    if (!string.IsNullOrEmpty(propMultiidiomaNoCV.valuesmultilang[idioma]))
                                    {
                                        if (!propiedadesNuevas.ContainsKey(propMultiidiomaNoCV.prop))
                                        {
                                            propiedadesNuevas.Add(propMultiidiomaNoCV.prop, new List<MultilangProperty>());
                                        }
                                        MultilangProperty multilangProperty = new MultilangProperty()
                                        {
                                            lang = idioma,
                                            value = propMultiidiomaNoCV.valuesmultilang[idioma]
                                        };
                                        propiedadesNuevas[propMultiidiomaNoCV.prop].Add(multilangProperty);
                                    }
                                }
                            }
                        }
                    }
                    if (pEntity.properties_cv != null)
                    {
                        foreach (Entity.Property prop in pEntity.properties_cv)
                        {
                            if (prop.valuesmultilang != null)
                            {
                                foreach (string idioma in prop.valuesmultilang.Keys)
                                {
                                    if (!string.IsNullOrEmpty(prop.valuesmultilang[idioma]))
                                    {
                                        if (!propiedadesNuevas.ContainsKey(prop.prop))
                                        {
                                            propiedadesNuevas.Add(prop.prop, new List<MultilangProperty>());
                                        }
                                        MultilangProperty multilangProperty = new MultilangProperty()
                                        {
                                            lang = idioma,
                                            value = prop.valuesmultilang[idioma]
                                        };
                                        propiedadesNuevas[prop.prop].Add(multilangProperty);
                                    }
                                }
                            }
                        }
                    }
                    UpdateMultilangProperties(propiedadesActuales, propiedadesNuevas, pCvID, entityID);
                }

                //Si es un Documento lo modifico e informo a los usuarios correspondientes.
                if (pEntity.rdfType.Equals("http://purl.org/ontology/bibo/Document") || pEntity.rdfType.Equals("http://w3id.org/roh/ResearchObject"))
                {
                    string personaCV = UtilityCV.GetPersonFromCV(pCvID);
                    pEntity.id = entityID;
                    ProcesLoadPropertyValues(itemEditConfig, pEntity);
                    Entity entityBBDD = GetLoadedEntity(entityID, templateSection.presentation.listItemsPresentation.listItemEdit.graph);
                    Thread thread = new Thread(() => ModificacionNotificacion(entityBBDD, template, templateSection, personCV, accion));
                    thread.Start();
                }

                //Insertamos en la cola del desnormalizador
                RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new RabbitServiceWriterDenormalizer(pConfigService);
                Dictionary<string, DenormalizerItemQueue.ItemType> tiposDesnormalizar = new Dictionary<string, DenormalizerItemQueue.ItemType>();
                tiposDesnormalizar.Add("http://purl.org/ontology/bibo/Document", DenormalizerItemQueue.ItemType.document);
                tiposDesnormalizar.Add("http://w3id.org/roh/ResearchObject", DenormalizerItemQueue.ItemType.researchobject);
                tiposDesnormalizar.Add("http://xmlns.com/foaf/0.1/Group", DenormalizerItemQueue.ItemType.group);
                tiposDesnormalizar.Add("http://vivoweb.org/ontology/core#Project", DenormalizerItemQueue.ItemType.project);
                if (tiposDesnormalizar.ContainsKey(pEntity.rdfType))
                {
                    rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(tiposDesnormalizar[pEntity.rdfType], new HashSet<string> { entityID }));
                }
                rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.person, new HashSet<string> { personCV }));

                return new JsonResult() { ok = true, id = entityIDResponse };
            }
        }

        private JsonResult ComprobarErrores(API.Templates.Tab pTemplate, Entity pEntity, string pSectionID, string pRdfTypeTab, string pLang)
        {
            ItemEdit itemEdit = null;
            List<ItemEditSectionRowProperty> validar = new();
            
            if (pRdfTypeTab == "http://w3id.org/roh/PersonalData")
            {
                itemEdit = pTemplate.personalDataSections;
            }
            else
            {
                API.Templates.TabSection templateSection = pTemplate.sections.First(x => x.property == pSectionID);
                itemEdit = templateSection.presentation.listItemsPresentation.listItemEdit;
            }
            itemEdit.sections.ForEach(section => section.rows.ForEach(x => validar.AddRange(x.properties.FindAll(x => x.validation != null))));
            
            foreach(ItemEditSectionRowProperty item in validar)
            {
                Regex rx = new(item.validation.regex);
                Entity.Property p = pEntity.properties.First(x =>x.prop == item.property);
                if (p.values != null)
                {
                    foreach (string value in p.values)
                    {
                        
                        if (!string.IsNullOrEmpty(value) && !rx.IsMatch(value))
                        {
                            return new JsonResult() { ok = false, error = item.validation.error[pLang] };
                        }

                    };
                }

            };
            return null;
        }

        /// <summary>
        /// Modificamos un recurso y lanzamos una notificación a las personas que se vean afectadas por las modificaciones.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="template"></param>
        /// <param name="templateSection"></param>
        private void ModificacionNotificacion(Entity entity, API.Templates.Tab template, API.Templates.TabSection templateSection, string personaCV, string accion)
        {
            if (accion == null)
            {
                return;
            }

            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = $" FILTER(?document =<{entity.id}>)";

                ConcurrentBag<Notification> notificaciones = new ConcurrentBag<Notification>();

                //Añadimos o modificamos un item
                while (true)
                {
                    int limit = 500;
                    string select = @$"select distinct ?cv ?cvSection ?person ";
                    string where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?cvSection ?document
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                            ?document a <{entity.rdfType}>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <{template.property}> ?cvSection.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                        ?document a <{entity.rdfType}>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <{template.property}> ?cvSection.
                                        ?cvSection ?p ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                    }}
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string>() { "curriculumvitae", "document", "researchobject", "person" });

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = 5 }, fila =>
                    {
                        //Obtenemos la auxiliar en la que cargar la entidad                        
                        string rdfTypePrefix = UtilityCV.AniadirPrefijo(templateSection.rdftype);
                        rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                        string idNewAux = $"{mResourceApi.GraphsUrl}items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(fila["cv"].value) + "_" + Guid.NewGuid();

                        List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
                        string idEntityAux = fila["cvSection"].value + "|" + idNewAux;

                        //Privacidad, por defecto falso                    
                        string predicadoPrivacidad = template.property + "|" + templateSection.property + "|" + UtilityCV.PropertyIspublic;
                        TriplesToInclude tr2 = new TriplesToInclude(idEntityAux + "|false", predicadoPrivacidad);
                        listaTriples.Add(tr2);

                        //Entidad
                        string predicadoEntidad = template.property + "|" + templateSection.property + "|" + templateSection.presentation.listItemsPresentation.property;
                        TriplesToInclude tr1 = new TriplesToInclude(idEntityAux + "|" + entity.id, predicadoEntidad);
                        listaTriples.Add(tr1);

                        Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>()
                            {
                                {
                                    mResourceApi.GetShortGuid(fila["cv"].value), listaTriples
                                }
                            };
                        Dictionary<Guid, bool> respuesta = mResourceApi.InsertPropertiesLoadedResources(triplesToInclude);

                        foreach (KeyValuePair<Guid, bool> keyValue in respuesta)
                        {
                            Notification notificacion = new Notification();
                            notificacion.IdRoh_trigger = personaCV;
                            notificacion.Roh_tabPropertyCV = template.property;
                            notificacion.Roh_entity = entity.id;
                            notificacion.IdRoh_owner = fila["person"].value;
                            notificacion.Dct_issued = DateTime.UtcNow;
                            notificacion.Roh_type = accion;
                            notificacion.CvnCode = IdentificadorFECYT(entity.properties.Where(x => x.prop.Equals("http://w3id.org/roh/scientificActivityDocument")).SelectMany(x => x.values).FirstOrDefault());

                            notificaciones.Add(notificacion);
                        }
                    });
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                //Eliminamos un item
                while (true)
                {
                    int limitEliminar = 500;
                    string selectEliminar = @$"select distinct ?cv ?cvSection ?item ?person ";
                    string whereEliminar = @$"where{{
                                    {filter} 
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                        ?document a <{entity.rdfType}>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <{template.property}> ?cvSection.
                                        ?cvSection  ?p ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?cvSection  ?document
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                            ?document a <{entity.rdfType}>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <{template.property}> ?cvSection.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                        }}                                        
                                    }}
                                }} order by desc(?cv) limit {limitEliminar}";
                    SparqlObject resultadoEliminar = mResourceApi.VirtuosoQueryMultipleGraph(selectEliminar, whereEliminar, new List<string>() { "curriculumvitae", "document", "researchobject", "person" });

                    Parallel.ForEach(resultadoEliminar.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = 5 }, fila =>
                    {
                        Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();

                        RemoveTriples removeTriple = new();
                        removeTriple.Predicate = template.property + "|" + templateSection.property;
                        removeTriple.Value = fila["cvSection"].value + "|" + fila["item"].value;
                        Guid idCV = mResourceApi.GetShortGuid(fila["cv"].value);
                        if (triplesToDelete.ContainsKey(idCV))
                        {
                            triplesToDelete[idCV].Add(removeTriple);
                        }
                        else
                        {
                            triplesToDelete.Add(idCV, new() { removeTriple });
                        }

                        Dictionary<Guid, bool> respuesta = mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<RemoveTriples>>() { { idCV, triplesToDelete.First().Value } });

                        foreach (KeyValuePair<Guid, bool> keyValue in respuesta)
                        {
                            Notification notificacion = new Notification();
                            notificacion.IdRoh_trigger = personaCV;
                            notificacion.Roh_tabPropertyCV = template.property;
                            notificacion.Roh_entity = entity.id;
                            notificacion.IdRoh_owner = fila["person"].value;
                            notificacion.Dct_issued = DateTime.UtcNow;
                            notificacion.Roh_type = "delete";
                            notificacion.CvnCode = IdentificadorFECYT(entity.properties.Where(x => x.prop.Equals("http://w3id.org/roh/scientificActivityDocument")).SelectMany(x => x.values).FirstOrDefault());

                            notificaciones.Add(notificacion);
                        }
                    });
                    if (resultadoEliminar.results.bindings.Count != limitEliminar)
                    {
                        break;
                    }
                }


                string propiedad = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://www.w3.org/1999/02/22-rdf-syntax-ns#member";
                //Generamos notificaciones de edición
                List<string> personasEdicion = entity.properties.First(x => x.prop == propiedad).values.Select(x => x.Split("@@@")[1])
                    .Where(x => !notificaciones.Select(x => x.IdRoh_owner).Contains(x) && x != personaCV && !string.IsNullOrEmpty(x)).ToList();
                foreach (string persona in personasEdicion)
                {
                    Notification notificacion = new Notification();
                    notificacion.IdRoh_trigger = personaCV;
                    notificacion.Roh_tabPropertyCV = template.property;
                    notificacion.Roh_entity = entity.id;
                    notificacion.IdRoh_owner = persona;
                    notificacion.Dct_issued = DateTime.UtcNow;
                    notificacion.Roh_type = accion;
                    notificacion.CvnCode = IdentificadorFECYT(entity.properties.Where(x => x.prop.Equals("http://w3id.org/roh/scientificActivityDocument")).SelectMany(x => x.values).FirstOrDefault());

                    notificaciones.Add(notificacion);
                }
                List<Notification> notificacionesCargar = notificaciones.ToList();
                notificacionesCargar.RemoveAll(x => x.IdRoh_owner == personaCV);
                mResourceApi.ChangeOntoly("notification");
                Parallel.ForEach(notificacionesCargar, new ParallelOptions { MaxDegreeOfParallelism = 5 }, notificacion =>
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
                });
            }
        }

        public object ValidateORCID(ConfigService pConfigService, string pORCID)
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
                        string name = "";
                        if (person.name.given_names != null)
                        {
                            name = person.name.given_names.value.Trim();
                        }
                        string lastName = "";
                        if (person.name.family_name != null)
                        {
                            lastName = person.name.family_name.value.Trim();
                        }
                        entity.properties = new List<Entity.Property>()
                        {
                            new Entity.Property()
                            {
                                prop = "http://xmlns.com/foaf/0.1/name",
                                values = new List<string>() { (name + " "+ lastName).Trim() }
                            },
                            new Entity.Property()
                            {
                                prop = "http://xmlns.com/foaf/0.1/firstName",
                                values = new List<string>() { name.Trim() }
                            },
                            new Entity.Property()
                            {
                                prop = "http://xmlns.com/foaf/0.1/lastName",
                                values = new List<string>() { lastName.Trim() }
                            },
                            new Entity.Property()
                            {
                                prop = "http://w3id.org/roh/ORCID",
                                values = new List<string>() {pORCID }
                            }
                        };
                        mResourceApi.ChangeOntoly("person");
                        ComplexOntologyResource resource = ToGnossApiResource(entity);
                        string result = mResourceApi.LoadComplexSemanticResource(resource, false, true);

                        //Insertamos en la cola del desnormalizador
                        RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new RabbitServiceWriterDenormalizer(pConfigService);
                        rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.person, new HashSet<string> { result }));

                        if (resource.Uploaded)
                        {
                            idPerson = result;
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            if (!string.IsNullOrEmpty(idPerson))
            {
                return GetPerson(idPerson);
            }
            return new JsonResult() { ok = false, id = "", error = "El código introducido no es válio" };
        }

        public Object CreatePerson(ConfigService pConfigService, string pName, string pSurname)
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
            mResourceApi.ChangeOntoly("person");
            ComplexOntologyResource resource = ToGnossApiResource(entity);
            string result = mResourceApi.LoadComplexSemanticResource(resource, false, true);

            if (resource.Uploaded)
            {
                //Insertamos en la cola del desnormalizador
                RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new RabbitServiceWriterDenormalizer(pConfigService);
                rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.person, new HashSet<string> { result }));

                return GetPerson(result);
            }
            return new JsonResult() { ok = false, id = "", error = "Se ha producido un error al crear la persona" };
        }

        private Person GetPerson(string pIdPerson)
        {
            string select = $@"select distinct ?ORCID ?name ?departamento";
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
            SparqlObject sparqlObject = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string> { "person", "department" });
            if (sparqlObject != null && sparqlObject.results != null && sparqlObject.results.bindings != null && sparqlObject.results.bindings.Count > 0)
            {
                Dictionary<string, Data> fila = sparqlObject.results.bindings[0];
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
                        values = propertyValue.values.Select(x => x.Replace("{GraphsUrl}", mResourceApi.GraphsUrl)).ToList()
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
        private static string IdentificadorFECYT(string tipoDocumento)
        {
            if (string.IsNullOrEmpty(tipoDocumento))
            {
                return null;
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD1"))
            {
                return "060.010.010.000";
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD2"))
            {
                return "060.010.020.000";
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD3"))
            {
                return "060.010.030.000";
            }
            return null;
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
        private Tuple<List<string>, List<string>, List<string>> GetSubjectsPropertiesAndRdftypesFromAuxCV(string pEntity, string pProperty)
        {
            List<string> properties = new List<string>();
            List<string> entities = new List<string>() { pEntity };
            if (!string.IsNullOrEmpty(pProperty))
            {
                properties.Add(pProperty);
            }
            List<string> rdftypes = new List<string>();

            while (true)
            {
                SparqlObject resultData = mResourceApi.VirtuosoQuery("select *", "where{?s a ?rdftype. ?s ?p <" + pEntity + ">. }", "curriculumvitae");
                if(resultData.results.bindings.Count==0 || !resultData.results.bindings.Any(x=> x["p"].value != "http://gnoss/hasEntidad"))
                {
                    break;
                }
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    if (fila["p"].value != "http://gnoss/hasEntidad")
                    {
                        properties.Add(fila["p"].value);
                        entities.Add(fila["s"].value);
                        pEntity = fila["s"].value;
                        rdftypes.Add(fila["rdftype"].value);
                    }
                }
            }
            properties.Reverse();
            entities.Reverse();
            rdftypes.Reverse();
            return new Tuple<List<string>, List<string>, List<string>>(entities, properties, rdftypes);
        }

        /// <summary>
        /// Fusiona dos entidades, sobreescribe en pLoadedEntity las propiedades que están en pUpdatedEntity
        /// </summary>
        /// <param name="pLoadedEntity">Entidad cargada en BBDD</param>
        /// <param name="pUpdatedEntity">Nueva entidad</param>
        /// <returns>Devuelve true si ha detetado cambios</returns>
        private bool MergeLoadedEntity(Entity pLoadedEntity, Entity pUpdatedEntity, bool pModificarPropiedadesOriginal = true)
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
                    if (pModificarPropiedadesOriginal)
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
                }
                else if (!remove)
                {
                    if ((property.values != null && property.values.Exists(x => !x.EndsWith("@@@"))) ||
                        (property.valuesmultilang != null && property.valuesmultilang.Values.ToList().Exists(x => !x.EndsWith("@@@"))))
                    {
                        change = true;
                        pLoadedEntity.properties.Add(property);
                    }
                }
                else if (remove)
                {
                    if (pModificarPropiedadesOriginal)
                    {
                        List<Entity.Property> propertiesLoadedEntityRemove = pLoadedEntity.properties.Where(x => x.prop == property.prop || x.prop.StartsWith(property.prop + "|") || x.prop.StartsWith(property.prop + "@@@")).ToList();
                        foreach (Entity.Property propertyToRemove in propertiesLoadedEntityRemove)
                        {
                            pLoadedEntity.properties.Remove(propertyToRemove);
                            change = true;
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
                        if (pLoadedEntity != null && pLoadedEntity.properties != null)
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
            }
            return change;
        }

        /// <summary>
        /// Fusiona dos entidades
        /// </summary>
        /// <param name="pIdMainEntity">Identificador de la entidad a la que pertenece la entidad auxiliar</param>
        /// <param name="pPropertyIDs">Propiedades que apuntan a la auxiliar</param>
        /// <param name="pEntityIDs">Entidades que apuntan a la auxiliar</param>
        /// <param name="pLoadedEntity">Entidad cargada en BBDD</param>
        /// <param name="pUpdatedEntity">Nueva entidad</param>
        /// <returns>Devuelve true si se ha actualizado correctamente</returns>
        private bool UpdateEntityAux(Guid pIdMainEntity, List<string> pPropertyIDs, List<string> pEntityIDs, Entity pLoadedEntity, Entity pUpdatedEntity)
        {
            bool update = true;
            Dictionary<Guid, List<TriplesToInclude>> triplesIncludeGuardado = new Dictionary<Guid, List<TriplesToInclude>>() { { pIdMainEntity, new List<TriplesToInclude>() } };
            Dictionary<Guid, List<RemoveTriples>> triplesRemoveGuardado = new Dictionary<Guid, List<RemoveTriples>>() { { pIdMainEntity, new List<RemoveTriples>() } };
            Dictionary<Guid, List<TriplesToModify>> triplesModifyGuardado = new Dictionary<Guid, List<TriplesToModify>>() { { pIdMainEntity, new List<TriplesToModify>() } };

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
                            triplesRemoveGuardado[pIdMainEntity].Add(new RemoveTriples()
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
                                    triplesModifyGuardado[pIdMainEntity].Add(new TriplesToModify()
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
                                        triplesRemoveGuardado[pIdMainEntity].Add(new RemoveTriples()
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
                                            triplesIncludeGuardado[pIdMainEntity].Add(new TriplesToInclude()
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
                            triplesIncludeGuardado[pIdMainEntity].Add(new TriplesToInclude()
                            {

                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(pIdMainEntity, valor, property.prop)
                            });
                        }
                    }
                }
                else if (remove)
                {
                    if (pLoadedEntity != null)
                    {
                        List<Entity.Property> propertiesLoadedEntityRemove = pLoadedEntity.properties.Where(x => x.prop.StartsWith(property.prop)).ToList();
                        foreach (Entity.Property propertyToRemove in propertiesLoadedEntityRemove)
                        {
                            foreach (string valor in propertyToRemove.values)
                            {
                                triplesRemoveGuardado[pIdMainEntity].Add(new RemoveTriples()
                                {

                                    Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                    Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(pIdMainEntity, valor, property.prop)
                                });
                            }
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
                        if (pLoadedEntity != null && pLoadedEntity.properties != null)
                        {
                            foreach (Entity.Property property in pLoadedEntity.properties)
                            {
                                foreach (string valor in property.values)
                                {
                                    if (valor.Contains(auxEntityRemove))
                                    {
                                        //Elmiminamos de la lista a aliminar los hijos de la entidad a eliminar
                                        triplesRemoveGuardado[pIdMainEntity].RemoveAll(x => x.Value.Contains(auxEntityRemove));
                                        //Eliminamos la entidad auxiliar
                                        triplesRemoveGuardado[pIdMainEntity].Add(new RemoveTriples()
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
            }
            if (triplesRemoveGuardado[pIdMainEntity].Count > 0)
            {
                update = update && mResourceApi.DeletePropertiesLoadedResources(triplesRemoveGuardado)[pIdMainEntity];
            }
            if (triplesIncludeGuardado[pIdMainEntity].Count > 0)
            {
                update = update && mResourceApi.InsertPropertiesLoadedResources(triplesIncludeGuardado)[pIdMainEntity];
            }
            if (triplesModifyGuardado[pIdMainEntity].Count > 0)
            {
                update = update && mResourceApi.ModifyPropertiesLoadedResources(triplesModifyGuardado)[pIdMainEntity];
            }
            return update;
        }

        /// <summary>
        /// Actualiza las propiedades multiidioma de una entidad
        /// </summary>
        /// <param name="pValoresAntiguos">Valores antiguos multiidioma</param>
        /// <param name="pValoresNuevos">Valores nuevos multiidioma</param>
        /// <param name="pIdCV">Identificador del CV</param>
        /// <param name="pIdEntity">Identificador de la entidad</param>
        /// <returns></returns>
        private bool UpdateMultilangProperties(Dictionary<string, List<MultilangProperty>> pValoresAntiguos, Dictionary<string, List<MultilangProperty>> pValoresNuevos, string pIdCV, string pIdEntity)
        {
            bool update = true;
            Guid guidCV = mResourceApi.GetShortGuid(pIdCV);
            Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToInclude>> triplesInclude = new Dictionary<Guid, List<TriplesToInclude>>() { { guidCV, new List<TriplesToInclude>() } };
            Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>> triplesRemove = new Dictionary<Guid, List<RemoveTriples>>() { { guidCV, new List<RemoveTriples>() } };
            Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToModify>> triplesModify = new Dictionary<Guid, List<TriplesToModify>>() { { guidCV, new List<TriplesToModify>() } };

            //Añadimos
            foreach (string property in pValoresNuevos.Keys)
            {
                foreach (MultilangProperty multilangProperty in pValoresNuevos[property])
                {
                    //Si no existe en las propiedades cargados esa propiedad en ese idioma la cargamos
                    if (!pValoresAntiguos.ContainsKey(property) || !pValoresAntiguos[property].Exists(x => x.lang == multilangProperty.lang))
                    {
                        string idNewAux = $"{mResourceApi.GraphsUrl}items/MultilangProperties_" + guidCV + "_" + Guid.NewGuid();

                        triplesInclude[guidCV].Add(new TriplesToInclude()
                        {

                            Predicate = "http://w3id.org/roh/multilangProperties|http://w3id.org/roh/entity",
                            NewValue = idNewAux + "|" + pIdEntity
                        });
                        triplesInclude[guidCV].Add(new TriplesToInclude()
                        {

                            Predicate = "http://w3id.org/roh/multilangProperties|http://w3id.org/roh/property",
                            NewValue = idNewAux + "|" + property
                        });
                        triplesInclude[guidCV].Add(new TriplesToInclude()
                        {

                            Predicate = "http://w3id.org/roh/multilangProperties|http://w3id.org/roh/lang",
                            NewValue = idNewAux + "|" + multilangProperty.lang
                        });
                        triplesInclude[guidCV].Add(new TriplesToInclude()
                        {

                            Predicate = "http://w3id.org/roh/multilangProperties|http://w3id.org/roh/value",
                            NewValue = idNewAux + "|" + multilangProperty.value
                        });
                    }
                }
            }

            //Modificamos
            foreach (string property in pValoresNuevos.Keys)
            {
                foreach (MultilangProperty multilangProperty in pValoresNuevos[property])
                {
                    if (!string.IsNullOrEmpty(multilangProperty.value))
                    {
                        //Si existe en las propiedades cargados en ese idioma y tiene valor la modificamos
                        if (pValoresAntiguos.ContainsKey(property) && pValoresAntiguos[property].Exists(x => x.lang == multilangProperty.lang))
                        {
                            MultilangProperty multiAntitguo = pValoresAntiguos[property].First(x => x.lang == multilangProperty.lang);
                            if (multiAntitguo.value != multilangProperty.value)
                            {
                                string idNewAux = multiAntitguo.auxEntityCV;
                                triplesModify[guidCV].Add(new TriplesToModify()
                                {

                                    Predicate = "http://w3id.org/roh/multilangProperties|http://w3id.org/roh/value",
                                    NewValue = idNewAux + "|" + multilangProperty.value,
                                    OldValue = idNewAux + "|" + multiAntitguo.value
                                });
                            }
                        }
                    }
                }
            }

            //Eliminamos
            foreach (string property in pValoresAntiguos.Keys)
            {
                foreach (MultilangProperty multilangProperty in pValoresAntiguos[property])
                {
                    //Si no existe en las propiedades nuevas esa propiedad en ese idioma la eliminamos
                    if (!pValoresNuevos.ContainsKey(property) || !pValoresNuevos[property].Exists(x => x.lang == multilangProperty.lang) || pValoresNuevos[property].Exists(x => x.lang == multilangProperty.lang && string.IsNullOrEmpty(x.value)))
                    {
                        triplesRemove[guidCV].Add(new RemoveTriples()
                        {
                            Predicate = "http://w3id.org/roh/multilangProperties",
                            Value = multilangProperty.auxEntityCV
                        });
                    }
                }
            }

            if (triplesRemove[guidCV].Count > 0)
            {
                update = update && mResourceApi.DeletePropertiesLoadedResources(triplesRemove)[guidCV];
            }
            if (triplesInclude[guidCV].Count > 0)
            {
                update = update && mResourceApi.InsertPropertiesLoadedResources(triplesInclude)[guidCV];
            }
            if (triplesModify[guidCV].Count > 0)
            {
                update = update && mResourceApi.ModifyPropertiesLoadedResources(triplesModify)[guidCV];
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
                    string whereID = $@"where{{
                                            ?x <http://gnoss/hasEntidad> <{pId}>.
                                            ?x <http://gnoss/hasEntidad> ?s. 
                                            ?s ?p ?o 
                                        }}
                                        order by desc(?s) desc(?p) desc(?o)
                                }} limit {numLimit} offset {offset}";
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

        private string GetEntityIdWithAux(string pId)
        {
            string select = "select ?id";
            string where = @$"where{{<{pId}> <http://vivoweb.org/ontology/core#relatedBy> ?id.}}";
            SparqlObject response = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            if (response.results.bindings.Count > 0)
            {
                return response.results.bindings[0]["id"].value;
            }
            else
            {
                return null;
            }
        }

        private Entity GetLoadedEntityWithAux(string pId, string pGraph)
        {
            string entityID = GetEntityIdWithAux(pId);
            if (!string.IsNullOrEmpty(entityID))
            {
                return GetLoadedEntity(entityID, pGraph);
            }
            else
            {
                return null;
            }
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

        public JsonResult ProcesarItemsDuplicados(ConfigService pConfigService, ProcessSimilarity pProcessSimilarity)
        {
            API.Templates.Tab tab = UtilityCV.TabTemplates.First(x => x.rdftype == pProcessSimilarity.rdfTypeTab);
            API.Templates.TabSection tabSection = tab.sections.First(x => x.property == pProcessSimilarity.idSection);
            HashSet<string> modificados = new HashSet<string>();
            foreach (string idSecundario in pProcessSimilarity.secundarios.Keys)
            {
                switch (pProcessSimilarity.secundarios[idSecundario])
                {
                    case ProcessSimilarity.ProcessSimilarityAction.fusionar:
                        FusionarEntidadesDuplicadas(pConfigService, pProcessSimilarity.idCV, pProcessSimilarity.principal, idSecundario, tab, tabSection);
                        modificados.Add(pProcessSimilarity.principal);
                        break;
                    case ProcessSimilarity.ProcessSimilarityAction.eliminar:
                        //Eliminamos la secundaria del usuario
                        RemoveItem(pConfigService, idSecundario);
                        break;
                    case ProcessSimilarity.ProcessSimilarityAction.noduplicado:
                        Dictionary<Guid, List<TriplesToInclude>> triplesInclude = new Dictionary<Guid, List<TriplesToInclude>>();
                        string idAux = $"{mResourceApi.GraphsUrl}items/NoDuplicateGroup_" + mResourceApi.GetShortGuid(pProcessSimilarity.idCV) + "_" + Guid.NewGuid();
                        triplesInclude[mResourceApi.GetShortGuid(pProcessSimilarity.idCV)] = new List<TriplesToInclude>();

                        triplesInclude[mResourceApi.GetShortGuid(pProcessSimilarity.idCV)].Add(new TriplesToInclude()
                        {

                            Predicate = "http://w3id.org/roh/noDuplicateGroup|http://w3id.org/roh/noDuplicateId",
                            NewValue = idAux + "|" + GetEntityIdWithAux(pProcessSimilarity.principal)
                        });
                        triplesInclude[mResourceApi.GetShortGuid(pProcessSimilarity.idCV)].Add(new TriplesToInclude()
                        {

                            Predicate = "http://w3id.org/roh/noDuplicateGroup|http://w3id.org/roh/noDuplicateId",
                            NewValue = idAux + "|" + GetEntityIdWithAux(idSecundario)
                        });
                        mResourceApi.InsertPropertiesLoadedResources(triplesInclude);
                        break;
                }
            }

            foreach (string id in modificados)
            {
                Entity mainEntity = GetLoadedEntityWithAux(id, tabSection.presentation.listItemsPresentation.listItemEdit.graph);

                //Insertamos en la cola del desnormalizador
                RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new RabbitServiceWriterDenormalizer(pConfigService);
                Dictionary<string, DenormalizerItemQueue.ItemType> tiposDesnormalizar = new Dictionary<string, DenormalizerItemQueue.ItemType>();
                tiposDesnormalizar.Add("http://purl.org/ontology/bibo/Document", DenormalizerItemQueue.ItemType.document);
                tiposDesnormalizar.Add("http://w3id.org/roh/ResearchObject", DenormalizerItemQueue.ItemType.researchobject);
                tiposDesnormalizar.Add("http://xmlns.com/foaf/0.1/Group", DenormalizerItemQueue.ItemType.group);
                tiposDesnormalizar.Add("http://vivoweb.org/ontology/core#Project", DenormalizerItemQueue.ItemType.project);
                if (tiposDesnormalizar.ContainsKey(mainEntity.rdfType))
                {
                    rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(tiposDesnormalizar[mainEntity.rdfType], new HashSet<string> { mainEntity.id }));
                }
                rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.person, new HashSet<string> { UtilityCV.GetPersonFromCV(pProcessSimilarity.idCV) }));
            }

            return new JsonResult() { ok = true };
        }

        public void FusionarEntidadesDuplicadas(ConfigService pConfigService, string pCVId, string pIdPrincipal, string pIdSecundaria, API.Templates.Tab pTab, API.Templates.TabSection pTabSection)
        {
            Entity mainEntity = GetLoadedEntityWithAux(pIdPrincipal, pTabSection.presentation.listItemsPresentation.listItemEdit.graph);
            mainEntity.propTitle = pTabSection.presentation.listItemsPresentation.listItemEdit.proptitle;
            mainEntity.propDescription = pTabSection.presentation.listItemsPresentation.listItemEdit.propdescription;
            Entity secEntity = GetLoadedEntityWithAux(pIdSecundaria, pTabSection.presentation.listItemsPresentation.listItemEdit.graph);
            secEntity.propTitle = pTabSection.presentation.listItemsPresentation.listItemEdit.proptitle;
            secEntity.propDescription = pTabSection.presentation.listItemsPresentation.listItemEdit.propdescription;
            //Fusionamos los datos de la entidad principal si ninguna está validada o al menos lo está la primera
            if ((!mainEntity.IsValidated() && !secEntity.IsValidated()) || mainEntity.IsValidated())
            {
                bool cambios = MergeLoadedEntity(mainEntity, secEntity, false);
                if (cambios)
                {
                    ComplexOntologyResource resource = ToGnossApiResource(mainEntity);
                    mResourceApi.ModifyComplexOntologyResource(resource, false, true);
                }
            }

            //Fusionamos los datos de la entidad auxiliar
            if (pTabSection.presentation.listItemsPresentation != null
                    && !string.IsNullOrEmpty(pTabSection.presentation.listItemsPresentation.property_cv)
                    && !string.IsNullOrEmpty(pTabSection.presentation.listItemsPresentation.rdftype_cv))
            {
                //Datos auxiliares de la entidad principal
                string selectP = "select ?s ?p ?o ";
                string whereP = @$"where
{{
    <{pIdPrincipal}> <{pTabSection.presentation.listItemsPresentation.property_cv}> ?s. 
    ?s a <{pTabSection.presentation.listItemsPresentation.rdftype_cv}>.
    ?s ?p ?o.
}}";

                SparqlObject auxP = mResourceApi.VirtuosoQuery(selectP, whereP, "curriculumvitae");

                //Datos auxiliar de la entidad secundaria
                string selectS = "select ?s ?p ?o ";
                string whereS = @$"where
{{
    <{pIdSecundaria}> <{pTabSection.presentation.listItemsPresentation.property_cv}> ?s. 
    ?s a <{pTabSection.presentation.listItemsPresentation.rdftype_cv}>.
    ?s ?p ?o.
}}";

                SparqlObject auxS = mResourceApi.VirtuosoQuery(selectS, whereS, "curriculumvitae");

                if (auxS.results.bindings.Count > 0)
                {
                    //Si la auxiliar de la secundaria tiene cosas hay que intentar pasarlas a la principal

                    List<string> propsOmitir = new List<string>();
                    propsOmitir.Add("http://www.w3.org/2000/01/rdf-schema#label");
                    propsOmitir.Add("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");


                    Dictionary<string, List<string>> datosSecundaria = new Dictionary<string, List<string>>();
                    foreach (Dictionary<string, SparqlObject.Data> fila in auxS.results.bindings)
                    {
                        if (!propsOmitir.Contains(fila["p"].value))
                        {
                            if (!datosSecundaria.ContainsKey(fila["p"].value))
                            {
                                datosSecundaria[fila["p"].value] = new List<string>();
                            }
                            datosSecundaria[fila["p"].value].Add(fila["o"].value);
                        }
                    }

                    Dictionary<string, List<string>> datosPrincipal = new Dictionary<string, List<string>>();
                    if (auxP.results.bindings.Count > 0)
                    {
                        foreach (Dictionary<string, SparqlObject.Data> fila in auxP.results.bindings)
                        {
                            if (!propsOmitir.Contains(fila["p"].value))
                            {
                                if (!datosPrincipal.ContainsKey(fila["p"].value))
                                {
                                    datosPrincipal[fila["p"].value] = new List<string>();
                                }
                                datosPrincipal[fila["p"].value].Add(fila["o"].value);
                            }
                        }
                    }

                    Dictionary<string, List<string>> datosAniadir = new Dictionary<string, List<string>>();
                    foreach (string prop in datosSecundaria.Keys)
                    {
                        if (!datosPrincipal.ContainsKey(prop))
                        {
                            datosAniadir[prop] = datosSecundaria[prop];
                        }
                    }

                    if (datosAniadir.Count > 0)
                    {
                        SparqlObject tab = mResourceApi.VirtuosoQuery("select *", "where{<" + pCVId + "> ?s ?o. ?o a <" + pTab.rdftype + "> }", "curriculumvitae");
                        string idTab = tab.results.bindings[0]["o"].value;

                        SparqlObject entityCV = mResourceApi.VirtuosoQuery("select *", "where{<" + pIdPrincipal + "> <" + pTabSection.presentation.listItemsPresentation.property_cv + "> ?o. ?o a <" + pTabSection.presentation.listItemsPresentation.rdftype_cv + "> }", "curriculumvitae");
                        string entityCVID = "";
                        if (entityCV.results.bindings.Count > 0)
                        {
                            //existe
                            entityCVID = entityCV.results.bindings[0]["o"].value;
                        }
                        else
                        {
                            //no existe
                            string rdfTypePrefix = UtilityCV.AniadirPrefijo(pTabSection.presentation.listItemsPresentation.rdftype_cv);
                            rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                            entityCVID = $"{mResourceApi.GraphsUrl}items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(pCVId) + "_" + Guid.NewGuid();
                        }

                        List<string> propertyIDs = new List<string>()
                            {
                                pTab.property,
                                pTabSection.property,
                                pTabSection.presentation.listItemsPresentation.property_cv
                            };
                        List<string> entityIDs = new List<string>()
                            {
                                idTab,
                                pIdPrincipal,
                                entityCVID
                            };


                        Dictionary<Guid, List<TriplesToInclude>> triplesInclude = new Dictionary<Guid, List<TriplesToInclude>>();
                        triplesInclude[mResourceApi.GetShortGuid(pCVId)] = new List<TriplesToInclude>();
                        foreach (string prop in datosAniadir.Keys)
                        {
                            foreach (string value in datosAniadir[prop])
                            {
                                triplesInclude[mResourceApi.GetShortGuid(pCVId)].Add(new TriplesToInclude()
                                {

                                    Predicate = string.Join("|", propertyIDs) + "|" + prop,
                                    NewValue = string.Join("|", entityIDs) + "|" + value
                                });
                            }
                        }
                        mResourceApi.InsertPropertiesLoadedResources(triplesInclude);
                    }
                }
            }


            ////Eliminamos del CV del usuario la secundaria
            //if (!secEntity.IsValidated())
            //{
            //    //Eliminamos la secundaria del usuario si no está validada
            RemoveItem(pConfigService, pIdSecundaria);
            //}
        }
    }
}