using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using GuardadoCV.Models.API.Input;
using GuardadoCV.Models.API.Response;
using GuardadoCV.Models.API.Templates;
using GuardadoCV.Models.API.Response;
using GuardadoCV.Models.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace GuardadoCV.Models
{
    /// <summary>
    /// Clase utilizada para las acciones de recuperación de datos de edición
    /// </summary>
    public class AccionesEdicion
    {
        /// <summary>
        /// API
        /// </summary>
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configOAuth\OAuthV3.config");


        #region Métodos públicos
        /// <summary>
        /// Obtiene una sección de pestaña del CV
        /// </summary>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pRdfType">Rdf:type de la entidad de la sección</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <returns></returns>
        public API.Response.Tab GetTab(string pId, string pRdfType, string pLang)
        {
            //Obtenemos el template
            API.Templates.Tab template = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfType);
            //Obtenemos los datos necesarios para el pintado
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetTabData(pId, template, pLang);
            //Obtenemos el modelo para devolver
            API.Response.Tab respuesta = GetTabModel(pId, data, template, pLang);
            return respuesta;
        }

        /// <summary>
        /// Obtiene una minificha de una entidad de un listado
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntity">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        public TabSectionItem GetItemMini(string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            TabSectionListItem presentationMini = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation.listItemsPresentation.listItem;
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetItemMiniData(pEntityID, presentationMini, pLang);
            return GetItem(pEntityID, data, presentationMini, pLang);
        }


        /// <summary>
        /// Obtiene los datos de edición de una entidad
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntity">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        public EntityEdit GetEdit(string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            TabSectionPresentationListItems presentationListItems = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation.listItemsPresentation;
            ItemEdit templateEdit = presentationListItems.listItemEdit;
            string entityID = pEntityID;

            //obtenemos la entidad correspondiente
            List<PropertyData> propertyData = new List<PropertyData>() { new PropertyData() { property = presentationListItems.property } };
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataAux = UtilityCV.GetProperties(new HashSet<string>() { pEntityID }, "curriculumvitae", propertyData, pLang);
            if (pEntityID!=null && dataAux.ContainsKey(pEntityID))
            {
                entityID = dataAux[pEntityID].First()["o"].value;
            }

            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetEditData(entityID, templateEdit, templateEdit.graph, pLang);

            List<ItemEditSectionRowPropertyCombo> listCombosConfig = GetEditCombos(templateEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).ToList());
            Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> combos = new Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>>();
            foreach (ItemEditSectionRowPropertyCombo combo in listCombosConfig)
            {
                Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataCombo = GetSubjectsCombo(combo, pLang);
                Dictionary<string, string> valoresCombo = new Dictionary<string, string>() { { "", "-" } };
                foreach (string id in dataCombo.Keys)
                {
                    valoresCombo.Add(id, GetPropValues(id, UtilityCV.GetPropComplete(combo.property), dataCombo).First());
                }
                combos.Add(combo, valoresCombo);
            }
            return GetEditModel(entityID, data, templateEdit, pLang, combos, templateEdit.graph);
        }

        /// <summary>
        /// Obtiene todos los combos de edición que hay configurados en una serie de propiedades
        /// </summary>
        /// <param name="pProperties"></param>
        /// <returns></returns>
        private List<ItemEditSectionRowPropertyCombo> GetEditCombos(List<ItemEditSectionRowProperty> pProperties)
        {
            List<ItemEditSectionRowPropertyCombo> listCombosConfig = new List<ItemEditSectionRowPropertyCombo>();
            foreach (ItemEditSectionRowPropertyCombo comboConfig in pProperties.Select(x => x.combo).Where(x => x != null))
            {
                if (!listCombosConfig.Exists(x =>
                                     UtilityCV.GetPropComplete(x.property) == UtilityCV.GetPropComplete(comboConfig.property) &&
                                     x.graph == comboConfig.graph &&
                                     x.rdftype == comboConfig.rdftype
                                    ))
                {

                    listCombosConfig.Add(comboConfig);
                }
            }
            foreach (ItemEditSectionRowProperty rowProperty in pProperties)
            {
                if (rowProperty.auxEntityData != null && rowProperty.auxEntityData.rows != null)
                {
                    foreach (ItemEditSectionRow row in rowProperty.auxEntityData.rows)
                    {
                        List<ItemEditSectionRowPropertyCombo> aux = GetEditCombos(row.properties);
                        foreach (ItemEditSectionRowPropertyCombo comboConfig in aux)
                        {
                            if (!listCombosConfig.Exists(x =>
                                                 UtilityCV.GetPropComplete(x.property) == UtilityCV.GetPropComplete(comboConfig.property) &&
                                                 x.graph == comboConfig.graph &&
                                                 x.rdftype == comboConfig.rdftype
                                                ))
                            {

                                listCombosConfig.Add(comboConfig);
                            }
                        }
                    }
                }
            }
            return listCombosConfig;
        }

        public EntityEdit GetEditEntity(string pRdfType, string pEntityID, string pLang)
        {
            ItemEdit templateEdit = UtilityCV.EntityTemplates.First(x => x.rdftype == pRdfType);
            string entityID = pEntityID;

            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetEditData(entityID, templateEdit, templateEdit.graph, pLang);

            List<ItemEditSectionRowPropertyCombo> listCombosConfig = GetEditCombos(templateEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).ToList());
            Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> combos = new Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>>();
            foreach (ItemEditSectionRowPropertyCombo combo in listCombosConfig)
            {
                Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataCombo = GetSubjectsCombo(combo, pLang);
                Dictionary<string, string> valoresCombo = new Dictionary<string, string>() { { "", "-" } };
                foreach (string id in dataCombo.Keys)
                {
                    valoresCombo.Add(id, GetPropValues(id, UtilityCV.GetPropComplete(combo.property), dataCombo).First());
                }
                combos.Add(combo, valoresCombo);
            }
            return GetEditModel(entityID, data, templateEdit, pLang, combos, templateEdit.graph);
        }

        public ItemsLoad LoadProps(ItemsLoad pItemsLoad, string pLang)
        {
            if (pItemsLoad.items != null && pItemsLoad.items.Count > 0)
            {
                foreach (LoadProp loadProp in pItemsLoad.items)
                {
                    KeyValuePair<string, PropertyData> propertyData = loadProp.GenerarPropertyData();
                    Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = UtilityCV.GetProperties(new HashSet<string>() { loadProp.id }, propertyData.Key, new List<PropertyData>() { propertyData.Value }, pLang);
                    loadProp.values = GetPropValues(loadProp.id, loadProp.GetPropComplete(), data);
                }
            }
            return pItemsLoad;
        }



        #endregion



        #region Métodos para pestañas
        /// <summary>
        /// Obtiene los datos de una pestaña 
        /// </summary>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pTemplate">Plantilla a utilizar</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetTabData(string pId, API.Templates.Tab pTemplate, string pLang)
        {
            List<PropertyData> propertyDatas = new List<PropertyData>();
            string graph = "curriculumvitae";
            foreach (API.Templates.TabSection templateSection in pTemplate.sections)
            {
                propertyDatas.Add(templateSection.GenerarPropertyData(graph));
            }
            return UtilityCV.GetProperties(new HashSet<string>() { pId }, graph, propertyDatas, pLang);
        }

        /// <summary>
        /// Obtiene los datos de un item dentro de un listado
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pListItemConfig">Configuración del item</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetItemMiniData(string pId, TabSectionListItem pListItemConfig, string pLang)
        {
            string graph = "curriculumvitae";
            return UtilityCV.GetProperties(new HashSet<string>() { pId }, graph, pListItemConfig.GenerarPropertyData(graph).childs, pLang);
        }

        /// <summary>
        /// Obtiene una sección de pestaña del CV una vez que tenemos los datos cargados
        /// </summary>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pData">Datos cargados de BBDD</param>
        /// <param name="pTemplate">Plantilla para generar el template</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private API.Response.Tab GetTabModel(string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, API.Templates.Tab pTemplate, string pLang)
        {
            API.Response.Tab tab = new API.Response.Tab()
            {
                sections = new List<API.Response.TabSection>()
            };
            foreach (API.Templates.TabSection templateSection in pTemplate.sections)
            {
                if (templateSection.presentation != null)
                {
                    API.Response.TabSection tabSection = new API.Response.TabSection()
                    {
                        identifier = templateSection.property,
                        title = UtilityCV.GetTextLang(pLang, templateSection.presentation.title),
                        orders = new List<TabSectionPresentationOrder>()
                    };
                    if (templateSection.presentation.listItemsPresentation.listItem.orders != null)
                    {
                        foreach (TabSectionListItemOrder listItemOrder in templateSection.presentation.listItemsPresentation.listItem.orders)
                        {
                            TabSectionPresentationOrder presentationOrderTabSection = new TabSectionPresentationOrder()
                            {
                                name = UtilityCV.GetTextLang(pLang, listItemOrder.name),
                                properties = new List<TabSectionPresentationOrderProperty>()
                            };

                            if (listItemOrder.properties != null)
                            {
                                foreach (TabSectionListItemOrderProperty listItemConfigOrderProperty in listItemOrder.properties)
                                {
                                    TabSectionPresentationOrderProperty presentationOrderTabSectionProperty = new TabSectionPresentationOrderProperty()
                                    {
                                        property = UtilityCV.GetPropComplete(listItemConfigOrderProperty),
                                        asc = listItemConfigOrderProperty.asc
                                    };
                                    presentationOrderTabSection.properties.Add(presentationOrderTabSectionProperty);
                                }
                            }
                            tabSection.orders.Add(presentationOrderTabSection);
                        }
                    }
                    tabSection.items = new Dictionary<string, TabSectionItem>();
                    string propiedadIdentificador = templateSection.property;
                    if (pData.ContainsKey(pId))
                    {
                        foreach (string idEntity in pData[pId].Where(x => x["p"].value == templateSection.property).Select(x => x["o"].value).Distinct())
                        {
                            tabSection.items.Add(idEntity, GetItem(idEntity, pData, templateSection.presentation.listItemsPresentation.listItem, pLang));
                        }
                    }
                    tab.sections.Add(tabSection);
                }
            }
            return tab;
        }


        /// <summary>
        /// Obtiene un item de un listado de una sección
        /// </summary>
        /// <param name="pId">Identificador del item</param>
        /// <param name="pData">Datos cargados</param>
        /// <param name="pListItemConfig">Configuración del item</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private TabSectionItem GetItem(string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, TabSectionListItem pListItemConfig, string pLang)
        {
            TabSectionItem item = new TabSectionItem()
            {
                title = GetPropValues(pId, UtilityCV.GetPropComplete(pListItemConfig.propertyTitle), pData).FirstOrDefault()
            };
            if (item.title == null)
            {
                item.title = "";
            }
            item.ispublic = GetPropValues(pId, UtilityCV.PropertyIspublic, pData).FirstOrDefault();
            if (string.IsNullOrEmpty(item.ispublic))
            {
                item.ispublic = "";
            }
            item.properties = new List<TabSectionItemProperty>();
            foreach (TabSectionListItemProperty property in pListItemConfig.properties)
            {
                string propertyIn = "";
                TabSectionItemProperty itemProperty = new TabSectionItemProperty()
                {
                    showMini = property.showMini,
                    showMiniBold = property.showMiniBold,
                    name = UtilityCV.GetTextLang(pLang, property.name)
                };
                if (property.childOR != null && property.childOR.Count > 0)
                {
                    propertyIn = "";
                    string aux = "";
                    foreach (PropertyDataTemplate propertyData in property.childOR)
                    {
                        propertyIn += aux + UtilityCV.GetPropComplete(propertyData);
                        aux = "||";
                    }
                }
                else
                {
                    propertyIn = UtilityCV.GetPropComplete(property.child);
                }
                itemProperty.type = property.type.ToString();
                itemProperty.values = GetPropValues(pId, propertyIn, pData);
                item.properties.Add(itemProperty);
            }
            item.orderProperties = new List<TabSectionItemOrderProperty>();
            if (pListItemConfig.orders != null)
            {
                foreach (TabSectionListItemOrder order in pListItemConfig.orders)
                {
                    if (order.properties != null)
                    {
                        foreach (TabSectionListItemOrderProperty data in order.properties)
                        {
                            TabSectionItemOrderProperty itemOrderProperty = new TabSectionItemOrderProperty()
                            {
                                property = UtilityCV.GetPropComplete(data),
                                values = GetPropValues(pId, UtilityCV.GetPropComplete(data), pData)
                            };
                            if (!item.orderProperties.Exists(x => x.property == itemOrderProperty.property))
                            {
                                item.orderProperties.Add(itemOrderProperty);
                            }
                        }
                    }
                }
            }
            return item;
        }


        #endregion

        #region Métodos para edición de entidades
        /// <summary>
        /// Obtiene todos los datos de una entidad de BBDD para su posterior edición
        /// </summary>
        /// <param name="pId">Identificador</param>
        /// <param name="pItemEdit">Configuración de edición</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetEditData(string pId, ItemEdit pItemEdit, string pGraph, string pLang)
        {
            List<PropertyData> propertyDatas = pItemEdit.GenerarPropertyDatas(pGraph);
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> respuesta = UtilityCV.GetProperties(new HashSet<string>() { pId }, pGraph, propertyDatas, pLang);
            return respuesta;
        }

        /// <summary>
        /// Genera el modelo de edición de una entidad una vez tenemos todos los datos de la entidad cargados
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pData">Datos de la entidad</param>
        /// <param name="pPresentationEdit">Configuración de presentación</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pCombos">Combos</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns></returns>
        private EntityEdit GetEditModel(string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, ItemEdit pPresentationEdit, string pLang, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, string pGraph)
        {
            EntityEdit entityEdit = new EntityEdit()
            {
                entityID = pId,
                ontology = pPresentationEdit.graph,
                rdftype = pPresentationEdit.rdftype,
                sections = new List<EntityEditSection>()
            };
            foreach (ItemEditSection itemEditSection in pPresentationEdit.sections)
            {
                EntityEditSection entityEditSection = new EntityEditSection()
                {
                    title = UtilityCV.GetTextLang(pLang, itemEditSection.title),
                    rows = GetRowsEdit(pId, itemEditSection.rows, pData, pCombos, pLang, pGraph)
                };
                entityEdit.sections.Add(entityEditSection);
            }
            return entityEdit;
        }


        /// <summary>
        /// Genera el modelo de edición de las filas de edición de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pItemEditSectionRows">Filas de edición configuradas</param>
        /// <param name="pData">Datos de la entidad</param>        
        /// <param name="pCombos">Combos</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns></returns>
        private List<EntityEditSectionRow> GetRowsEdit(string pId, List<ItemEditSectionRow> pItemEditSectionRows, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, string pLang, string pGraph)
        {
            List<EntityEditSectionRow> entityEditSectionRows = new List<EntityEditSectionRow>();
            foreach (ItemEditSectionRow itemEditSectionRow in pItemEditSectionRows)
            {
                EntityEditSectionRow entityEditSectionRow = new EntityEditSectionRow()
                {
                    properties = new List<EntityEditSectionRowProperty>()
                };
                foreach (ItemEditSectionRowProperty itemEditSectionRowProperty in itemEditSectionRow.properties)
                {
                    if (string.IsNullOrEmpty(itemEditSectionRowProperty.compossed))
                    {
                        EntityEditSectionRowProperty entityEditSectionRowProperty = GetPropertiesEdit(pId, itemEditSectionRowProperty, pData, pCombos, pLang, pGraph);
                        entityEditSectionRow.properties.Add(entityEditSectionRowProperty);
                    }
                }
                entityEditSectionRows.Add(entityEditSectionRow);
            }
            return entityEditSectionRows;
        }

        /// <summary>
        /// Genera el modelo de edición de una propiedad de edición de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pItemEditSectionRowProperty">Propiedad de edición configuradas</param>
        /// <param name="pData">Datos de la entidad</param>        
        /// <param name="pCombos">Combos</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns></returns>
        private EntityEditSectionRowProperty GetPropertiesEdit(string pId, ItemEditSectionRowProperty pItemEditSectionRowProperty, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, string pLang, string pGraph)
        {
            EntityEditSectionRowProperty entityEditSectionRowProperty = new EntityEditSectionRowProperty()
            {
                property = pItemEditSectionRowProperty.property,
                width = pItemEditSectionRowProperty.width,
                required = pItemEditSectionRowProperty.required,
                multiple = pItemEditSectionRowProperty.multiple,
                title = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.title),
                placeholder = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.placeholder),
                type = pItemEditSectionRowProperty.type.ToString(),
                values = new List<string>()
            };
            if (pItemEditSectionRowProperty.auxEntityData != null)
            {
                entityEditSectionRowProperty.entityAuxData = new EntityEditAuxEntity()
                {
                    childsOrder = new Dictionary<string, int>(),
                    rdftype = pItemEditSectionRowProperty.auxEntityData.rdftype,
                    propertyOrder = pItemEditSectionRowProperty.auxEntityData.propertyOrder,
                    titleConfig = new EntityEditRepresentativeProperty()
                    {
                        route = pItemEditSectionRowProperty.auxEntityData.propertyTitle.GetRoute()
                    }
                };
                entityEditSectionRowProperty.entityAuxData.propertiesConfig = new List<EntityEditRepresentativeProperty>();
                foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.auxEntityData.properties)
                {
                    entityEditSectionRowProperty.entityAuxData.propertiesConfig.Add(new EntityEditRepresentativeProperty()
                    {
                        name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                        route = entityProperty.child.GetRoute()
                    });
                }

            }
            if (pItemEditSectionRowProperty.entityData != null)
            {
                entityEditSectionRowProperty.entityData = new EntityEditEntity()
                {
                    rdftype = pItemEditSectionRowProperty.entityData.rdftype,
                    titleConfig = new EntityEditRepresentativeProperty()
                };
                if (pItemEditSectionRowProperty.entityData.propertyTitle != null)
                {
                    entityEditSectionRowProperty.entityData.titleConfig = new EntityEditRepresentativeProperty()
                    {
                        route = pItemEditSectionRowProperty.entityData.graph + "||" + pItemEditSectionRowProperty.entityData.propertyTitle.GetRoute()
                    };
                }
                entityEditSectionRowProperty.entityData.propertiesConfig = new List<EntityEditRepresentativeProperty>();
                foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.entityData.properties)
                {
                    entityEditSectionRowProperty.entityData.propertiesConfig.Add(new EntityEditRepresentativeProperty()
                    {
                        name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                        route = pItemEditSectionRowProperty.entityData.graph + "||" + entityProperty.child.GetRoute()
                    });
                }
            }

            if (pId != null && pData.ContainsKey(pId))
            {
                foreach (string value in pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.property).Select(x => x["o"].value).Distinct())
                {
                    entityEditSectionRowProperty.values.Add(value);
                }
            }
            if (pItemEditSectionRowProperty.combo != null)
            {
                entityEditSectionRowProperty.comboValues = pCombos.Where(x =>
                  UtilityCV.GetPropComplete(x.Key.property) == UtilityCV.GetPropComplete(pItemEditSectionRowProperty.combo.property) &&
                   x.Key.graph == pItemEditSectionRowProperty.combo.graph &&
                   x.Key.rdftype == pItemEditSectionRowProperty.combo.rdftype
                ).FirstOrDefault().Value;
            }
            if (pItemEditSectionRowProperty.type == DataTypeEdit.auxEntity)
            {
                if (pItemEditSectionRowProperty.auxEntityData != null && pItemEditSectionRowProperty.auxEntityData.rows != null && pItemEditSectionRowProperty.auxEntityData.rows.Count > 0)
                {
                    entityEditSectionRowProperty.entityAuxData.entities = new Dictionary<string, List<EntityEditSectionRow>>();
                    entityEditSectionRowProperty.entityAuxData.rows = GetRowsEdit(null, pItemEditSectionRowProperty.auxEntityData.rows, pData, pCombos, pLang, pGraph);
                    entityEditSectionRowProperty.entityAuxData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                    entityEditSectionRowProperty.entityAuxData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                    foreach (string id in entityEditSectionRowProperty.values)
                    {
                        entityEditSectionRowProperty.entityAuxData.entities.Add(id, GetRowsEdit(id, pItemEditSectionRowProperty.auxEntityData.rows, pData, pCombos, pLang, pGraph));
                        if (!string.IsNullOrEmpty(entityEditSectionRowProperty.entityAuxData.propertyOrder))
                        {
                            string orden = GetPropertiesEdit(id, new ItemEditSectionRowProperty() { property = entityEditSectionRowProperty.entityAuxData.propertyOrder }, pData, pCombos, pLang, pGraph).values.FirstOrDefault();
                            int.TryParse(orden, out int ordenInt);
                            entityEditSectionRowProperty.entityAuxData.childsOrder[id] = ordenInt;
                        }

                        if (pItemEditSectionRowProperty.auxEntityData.propertyTitle != null)
                        {
                            string title = GetPropValues(id, UtilityCV.GetPropComplete(pItemEditSectionRowProperty.auxEntityData.propertyTitle), pData).FirstOrDefault();
                            string routeTitle = pItemEditSectionRowProperty.auxEntityData.propertyTitle.GetRoute();
                            if (string.IsNullOrEmpty(title))
                            {
                                title = "";
                            }
                            entityEditSectionRowProperty.entityAuxData.titles.Add(id, new EntityEditRepresentativeProperty() { value = title, route = routeTitle });
                        }

                        if (pItemEditSectionRowProperty.auxEntityData.properties != null)
                        {
                            foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.auxEntityData.properties)
                            {
                                List<string> valores = GetPropValues(id, UtilityCV.GetPropComplete(entityProperty.child), pData);
                                string routeProp = entityProperty.child.GetRoute();
                                if (!entityEditSectionRowProperty.entityAuxData.properties.ContainsKey(id))
                                {
                                    entityEditSectionRowProperty.entityAuxData.properties[id] = new List<EntityEditRepresentativeProperty>();
                                }

                                entityEditSectionRowProperty.entityAuxData.properties[id].Add(new EntityEditRepresentativeProperty()
                                {
                                    name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                                    value = string.Join(", ", valores),
                                    route = routeProp
                                });

                            }
                        }
                    }
                }
            }

            if (pItemEditSectionRowProperty.type == DataTypeEdit.entity)
            {
                if (pItemEditSectionRowProperty.entityData != null)
                {
                    entityEditSectionRowProperty.entityData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                    entityEditSectionRowProperty.entityData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                    foreach (string id in entityEditSectionRowProperty.values)
                    {
                        if (pItemEditSectionRowProperty.entityData.propertyTitle != null)
                        {
                            string title = GetPropValues(id, UtilityCV.GetPropComplete(pItemEditSectionRowProperty.entityData.propertyTitle), pData).FirstOrDefault();
                            string routeTitle = pItemEditSectionRowProperty.entityData.propertyTitle.GetRoute();
                            if (string.IsNullOrEmpty(title))
                            {
                                title = "";
                            }
                            entityEditSectionRowProperty.entityData.titles.Add(id, new EntityEditRepresentativeProperty() { value = title, route = pItemEditSectionRowProperty.entityData.graph + "||" + routeTitle });
                        }

                        if (pItemEditSectionRowProperty.entityData.properties != null)
                        {
                            foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.entityData.properties)
                            {
                                List<string> valores = GetPropValues(id, UtilityCV.GetPropComplete(entityProperty.child), pData);
                                string routeProp = entityProperty.child.GetRoute();
                                if (!entityEditSectionRowProperty.entityData.properties.ContainsKey(id))
                                {
                                    entityEditSectionRowProperty.entityData.properties[id] = new List<EntityEditRepresentativeProperty>();
                                }

                                entityEditSectionRowProperty.entityData.properties[id].Add(new EntityEditRepresentativeProperty()
                                {
                                    name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                                    value = string.Join(", ", valores),
                                    route = pItemEditSectionRowProperty.entityData.graph + "||" + routeProp
                                });

                            }
                        }
                    }
                }
            }
            return entityEditSectionRowProperty;
        }
        #endregion


        #region Métodos de recolección de datos

        /// <summary>
        /// Obtiene los datos para los combos (entidad y texto)
        /// </summary>
        /// <param name="pItemEditSectionRowPropertyCombo">Configuración de un combo para edición</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetSubjectsCombo(ItemEditSectionRowPropertyCombo pItemEditSectionRowPropertyCombo, string pLang)
        {
            int paginacion = 10000;
            int offset = 0;
            int limit = paginacion;
            HashSet<string> ids = new HashSet<string>();
            while (limit == paginacion)
            {
                //Obtenemos los IDS
                string select = "select * where{select distinct ?s";
                string where = $"where{{?s a <{pItemEditSectionRowPropertyCombo.rdftype}> }} order by asc(?s)}} limit {limit} offset {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, pItemEditSectionRowPropertyCombo.graph);
                limit = sparqlObject.results.bindings.Count;
                offset += sparqlObject.results.bindings.Count;
                foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObject.results.bindings)
                {
                    ids.Add(fila["s"].value);
                }
            }
            List<PropertyData> propertyDatas = new List<PropertyData>() { pItemEditSectionRowPropertyCombo.property.GenerarPropertyData(pItemEditSectionRowPropertyCombo.graph) };
            return UtilityCV.GetProperties(ids, pItemEditSectionRowPropertyCombo.graph, propertyDatas, pLang);
        }

        /// <summary>
        /// Obtiene los valores de una propiedad para una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pProp">Propiedad</param>
        /// <param name="pData">Datos cargados</param>
        /// <returns></returns>
        private List<string> GetPropValues(string pId, string pProp, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData)
        {
            List<string> idAux = new List<string>();

            List<string> propInList = pProp.Split(new string[] { "||" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            if (propInList.Count > 1)
            {
                List<PropertyData> propertyDataAux = new List<PropertyData>();
                foreach (string propIn in propInList)
                {
                    PropertyData last = null;
                    string[] props = propIn.Split(new string[] { "@@@" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string prop in props)
                    {
                        if (last == null)
                        {
                            if (propertyDataAux.Exists(x => x.property == prop))
                            {
                                last = propertyDataAux.First(x => x.property == prop);
                            }
                            else
                            {
                                last = new PropertyData()
                                {
                                    property = prop,
                                    childs = new List<PropertyData>()
                                };
                                propertyDataAux.Add(last);
                            }
                        }
                        else
                        {
                            if (last.childs.Exists(x => x.property == prop))
                            {
                                last = last.childs.First(x => x.property == prop);
                            }
                            else
                            {
                                PropertyData aux = last;
                                last = new PropertyData()
                                {
                                    property = prop,
                                    childs = new List<PropertyData>()
                                };
                                aux.childs.Add(last);
                            }
                        }
                    }
                }
                List<string> finalList = new List<string>();
                foreach (PropertyData propertyData in propertyDataAux)
                {
                    idAux.AddRange(GetPropValuesAux(new List<string>() { pId }, propertyData, pData));
                }
            }
            else
            {
                idAux = new List<string>() { pId };
                string[] props = pProp.Split(new string[] { "@@@" }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string prop in props)
                {
                    List<string> idAux2 = new List<string>();
                    foreach (string id in idAux)
                    {
                        if (pData.ContainsKey(id))
                        {
                            idAux2.AddRange(pData[id].Where(x => x["p"].value == prop).Select(x => x["o"].value).Distinct().ToList());
                        }
                    }
                    idAux = idAux2;
                }
            }
            return idAux;
        }

        /// <summary>
        /// Método auxiliar para cuando la propiedad es un OR
        /// </summary>
        /// <param name="pIds">Identificadores</param>
        /// <param name="pPropertyData">Propiedades a recuperar</param>
        /// <param name="pData">Datos cargados</param>
        /// <returns></returns>
        private List<string> GetPropValuesAux(List<string> pIds, PropertyData pPropertyData, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData)
        {
            List<string> values = new List<string>();
            List<string> aux = new List<string>();
            foreach (string id in pIds)
            {
                aux = GetPropValues(id, pPropertyData.property, pData);
            }
            if (pPropertyData.childs != null && pPropertyData.childs.Count > 0)
            {
                foreach (string id in aux)
                {
                    List<string> aux2 = new List<string>();
                    foreach (PropertyData propertyData in pPropertyData.childs)
                    {
                        aux2 = GetPropValuesAux(new List<string>() { id }, propertyData, pData);
                        if (aux2.Count > 0)
                        {
                            break;
                        }
                    }
                    values.AddRange(aux2);
                }
            }
            else
            {
                values = aux;
            }
            return values;
        }

        #endregion



    }
}