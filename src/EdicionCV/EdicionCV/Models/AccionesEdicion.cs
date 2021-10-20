using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using GuardadoCV.Models.API;
using GuardadoCV.Models.API.Template;
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
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configOAuth\OAuthV3.config");


        #region Métodos públicos
        /// <summary>
        /// Obtiene una sección de pestaña del CV
        /// </summary>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pRdfType">Rdf:type de la entidad de la sección</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <returns></returns>
        public Tab GetTab(string pId, string pRdfType, string pLang)
        {
            //Obtenemos el template
            TabTemplate template = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfType);
            //Obtenemos los datos necesarios para el pintado
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetTabData(pId, template, pLang);
            //Obtenemos el modelo para devolver
            Tab respuesta = GetTabModel(pId, data, template, pLang);
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
        public ItemTabSection GetItemMini(string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            ListItemConfig presentationMini = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation.listItemConfig;
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
            ListItemEdit presentationEdit = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation.listItemEdit;
            string entityID = pEntityID;

            //obtenemos la entidad correspondiente
            List<PropertyData> propertyData = new List<PropertyData>() { new PropertyData() { property = presentationEdit.property } };
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataAux = UtilityCV.GetProperties(new HashSet<string>() { pEntityID }, "curriculumvitae", propertyData, pLang);
            if (dataAux.ContainsKey(pEntityID))
            {
                entityID = dataAux[pEntityID].First()["o"].value;
            }

            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetEditData(entityID, presentationEdit, presentationEdit.graph, pLang);

            List<ListItemEditSectionRowPropertyCombo> listCombosConfig = GetEditCombos(presentationEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).ToList());
            Dictionary<ListItemEditSectionRowPropertyCombo, Dictionary<string, string>> combos = new Dictionary<ListItemEditSectionRowPropertyCombo, Dictionary<string, string>>();
            foreach (ListItemEditSectionRowPropertyCombo combo in listCombosConfig)
            {
                Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataCombo = GetSubjectsCombo(combo, pLang);
                Dictionary<string, string> valoresCombo = new Dictionary<string, string>();
                valoresCombo.Add("", "-");
                foreach (string id in dataCombo.Keys)
                {
                    valoresCombo.Add(id, GetPropValues(id, UtilityCV.GetPropComplete(combo.property), dataCombo).First());
                }
                combos.Add(combo, valoresCombo);
            }
            return GetEditModel(entityID, data, presentationEdit, pLang, combos, presentationEdit.graph);
        }

        /// <summary>
        /// Obtiene todos los combos de edición que hay configurados en una serie de propiedades
        /// </summary>
        /// <param name="pProperties"></param>
        /// <returns></returns>
        private List<ListItemEditSectionRowPropertyCombo> GetEditCombos(List<ListItemEditSectionRowProperty> pProperties)
        {
            List<ListItemEditSectionRowPropertyCombo> listCombosConfig = new List<ListItemEditSectionRowPropertyCombo>();
            foreach (ListItemEditSectionRowPropertyCombo comboConfig in pProperties.Select(x => x.combo).Where(x => x != null))
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
            foreach (ListItemEditSectionRowProperty rowProperty in pProperties)
            {
                if (rowProperty.auxEntityData != null && rowProperty.auxEntityData.rows != null)
                {
                    foreach (ListItemEditSectionRow row in rowProperty.auxEntityData.rows)
                    {
                        List<ListItemEditSectionRowPropertyCombo> aux = GetEditCombos(row.properties);
                        foreach (ListItemEditSectionRowPropertyCombo comboConfig in aux)
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
            ListItemEdit presentationEdit = UtilityCV.EntityTemplates.First(x => x.rdftype == pRdfType);
            string entityID = pEntityID;

            //obtenemos la entidad correspondiente
            List<PropertyData> propertyData = new List<PropertyData>() { new PropertyData() { property = presentationEdit.property } };
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataAux = UtilityCV.GetProperties(new HashSet<string>() { pEntityID }, "curriculumvitae", propertyData, pLang);
            if (dataAux.ContainsKey(pEntityID))
            {
                entityID = dataAux[pEntityID].First()["o"].value;
            }

            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetEditData(entityID, presentationEdit, presentationEdit.graph, pLang);

            List<ListItemEditSectionRowPropertyCombo> listCombosConfig = GetEditCombos(presentationEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).ToList());
            Dictionary<ListItemEditSectionRowPropertyCombo, Dictionary<string, string>> combos = new Dictionary<ListItemEditSectionRowPropertyCombo, Dictionary<string, string>>();
            foreach (ListItemEditSectionRowPropertyCombo combo in listCombosConfig)
            {
                Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataCombo = GetSubjectsCombo(combo, pLang);
                Dictionary<string, string> valoresCombo = new Dictionary<string, string>();
                valoresCombo.Add("", "-");
                foreach (string id in dataCombo.Keys)
                {
                    valoresCombo.Add(id, GetPropValues(id, UtilityCV.GetPropComplete(combo.property), dataCombo).First());
                }
                combos.Add(combo, valoresCombo);
            }
            return GetEditModel(entityID, data, presentationEdit, pLang, combos, presentationEdit.graph);
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
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetTabData(string pId, TabTemplate pTemplate, string pLang)
        {
            List<PropertyData> propertyDatas = new List<PropertyData>();
            string graph = "curriculumvitae";
            foreach (Section templateSection in pTemplate.sections)
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
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetItemMiniData(string pId, ListItemConfig pListItemConfig, string pLang)
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
        private Tab GetTabModel(string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, TabTemplate pTemplate, string pLang)
        {
            Tab tab = new Tab();
            tab.sections = new List<TabSection>();
            foreach (Section templateSection in pTemplate.sections)
            {
                if (templateSection.presentation != null)
                {
                    TabSection tabSection = new TabSection();
                    tabSection.identifier = templateSection.property;
                    tabSection.title = UtilityCV.GetTextLang(pLang, templateSection.presentation.title);
                    tabSection.orders = new List<PresentationOrderTabSection>();
                    if (templateSection.presentation.listItemConfig.orders != null)
                    {
                        foreach (ListItemConfigOrder listItemConfigOrder in templateSection.presentation.listItemConfig.orders)
                        {
                            PresentationOrderTabSection presentationOrderTabSection = new PresentationOrderTabSection();
                            presentationOrderTabSection.name = UtilityCV.GetTextLang(pLang, listItemConfigOrder.name);
                            presentationOrderTabSection.properties = new List<PresentationOrderTabSectionProperty>();
                            if (listItemConfigOrder.properties != null)
                            {
                                foreach (ListItemConfigOrderProperty listItemConfigOrderProperty in listItemConfigOrder.properties)
                                {
                                    PresentationOrderTabSectionProperty presentationOrderTabSectionProperty = new PresentationOrderTabSectionProperty();
                                    presentationOrderTabSectionProperty.property = UtilityCV.GetPropComplete(listItemConfigOrderProperty);
                                    presentationOrderTabSectionProperty.asc = listItemConfigOrderProperty.asc;
                                    presentationOrderTabSection.properties.Add(presentationOrderTabSectionProperty);
                                }
                            }
                            tabSection.orders.Add(presentationOrderTabSection);
                        }
                    }
                    tabSection.items = new Dictionary<string, ItemTabSection>();
                    string propiedadIdentificador = templateSection.property;
                    foreach (string idEntity in pData[pId].Where(x => x["p"].value == templateSection.property).Select(x => x["o"].value).Distinct())
                    {
                        tabSection.items.Add(idEntity, GetItem(idEntity, pData, templateSection.presentation.listItemConfig, pLang));
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
        private ItemTabSection GetItem(string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, ListItemConfig pListItemConfig, string pLang)
        {
            ItemTabSection item = new ItemTabSection();
            item.title = GetPropValues(pId, UtilityCV.GetPropComplete(pListItemConfig.propertyTitle), pData).FirstOrDefault();
            if (item.title == null)
            {
                item.title = "";
            }
            item.ispublic = GetPropValues(pId, UtilityCV.PropertyIspublic, pData).FirstOrDefault();
            if (string.IsNullOrEmpty(item.ispublic))
            {
                item.ispublic = "";
            }
            item.properties = new List<ItemTabSectionProperty>();
            foreach (ListItemConfigProperty property in pListItemConfig.properties)
            {
                string propertyIn = "";
                ItemTabSectionProperty itemProperty = new ItemTabSectionProperty();
                itemProperty.showMini = property.showMini;
                itemProperty.showMiniBold = property.showMiniBold;
                itemProperty.name = UtilityCV.GetTextLang(pLang, property.name);
                if (property.childOR != null && property.childOR.Count > 0)
                {
                    //TODO ca con todas no una a una
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
            item.orderProperties = new List<ItemTabSectionOrderProperty>();
            if (pListItemConfig.orders != null)
            {
                foreach (ListItemConfigOrder order in pListItemConfig.orders)
                {
                    if (order.properties != null)
                    {
                        foreach (ListItemConfigOrderProperty data in order.properties)
                        {
                            ItemTabSectionOrderProperty itemOrderProperty = new ItemTabSectionOrderProperty();
                            itemOrderProperty.property = UtilityCV.GetPropComplete(data);
                            itemOrderProperty.values = GetPropValues(pId, itemOrderProperty.property, pData);
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
        /// <param name="pListItemEdit">Configuración de edición</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetEditData(string pId, ListItemEdit pListItemEdit, string pGraph, string pLang)
        {
            List<PropertyData> propertyDatas = pListItemEdit.GenerarPropertyDatas(pGraph);
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> respuesta = UtilityCV.GetProperties(new HashSet<string>() { pId }, pGraph, propertyDatas, pLang);
            return respuesta;
        }

        //private void GenerarPropertyDataEdit(List<PresentationEditSectionRowProperty> pProps, ref PropertyDataX pPropertyData, string pGraph)
        //{
        //    /*public PropertyData propertyTitle { get; set; }
        ///// <summary>
        ///// Listado con las propiedades a pintar
        ///// </summary>
        //public List<PresentationMiniProperty> properties { get; set; }*/

        //    if (pProps != null)
        //    {
        //        foreach (PresentationEditSectionRowProperty propInt in pProps)
        //        {
        //            PropertyDataX property = new PropertyDataX();
        //            property.property = propInt.property;
        //            property.childs = new List<PropertyDataX>();
        //            if (!pPropertyData.childs.Exists(x => x.property == property.property))
        //            {
        //                pPropertyData.childs.Add(property);
        //            }
        //            else
        //            {
        //                property = pPropertyData.childs.First(x => x.property == property.property);
        //            }
        //            if (!string.IsNullOrEmpty(propInt.propertyOrder))
        //            {
        //                property.childs.Add(new PropertyDataX() { property = propInt.propertyOrder, graph = pGraph });
        //            }
        //            if (propInt.rows != null)
        //            {
        //                foreach (PresentationEditSectionRow presentationEditSectionRowIn in propInt.rows)
        //                {
        //                    GenerarPropertyDataEdit(presentationEditSectionRowIn.properties, ref property, pGraph);
        //                }
        //            }
        //            if (propInt.propertyTitle != null)
        //            {
        //                if (!property.childs.Exists(x => x.property == propInt.propertyTitle.property))
        //                {
        //                    property.childs.Add(propInt.propertyTitle);
        //                }
        //            }
        //            if (propInt.properties != null)
        //            {
        //                foreach (ListItemConfigProperty presentationMiniProperty in propInt.properties)
        //                {
        //                    if (!property.childs.Exists(x => x.property == presentationMiniProperty.child.property))
        //                    {
        //                        property.childs.Add(presentationMiniProperty.child);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

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
        private EntityEdit GetEditModel(string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, ListItemEdit pPresentationEdit, string pLang, Dictionary<ListItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, string pGraph)
        {
            //TODO X
            EntityEdit entityEdit = new EntityEdit();
            entityEdit.entityID = pId;
            entityEdit.ontology = pPresentationEdit.graph;
            entityEdit.rdftype = pPresentationEdit.rdftype;
            entityEdit.sections = new List<EntityEditSection>();

            foreach (ListItemEditSection listItemEditSection in pPresentationEdit.sections)
            {
                EntityEditSection entityEditSection = new EntityEditSection();
                entityEditSection.title = UtilityCV.GetTextLang(pLang, listItemEditSection.title);
                entityEditSection.rows = GetRowsEdit(pId, listItemEditSection.rows, pData, pCombos, pLang, pGraph);
                entityEdit.sections.Add(entityEditSection);
            }
            return entityEdit;
        }

        private List<EntityEditSectionRow> GetRowsEdit(string pId, List<ListItemEditSectionRow> pListItemEditSectionRows, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, Dictionary<ListItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, string pLang, string pGraph)
        {
            //TODO X
            List<EntityEditSectionRow> entityEditSectionRows = new List<EntityEditSectionRow>();
            foreach (ListItemEditSectionRow listItemEditSectionRow in pListItemEditSectionRows)
            {
                EntityEditSectionRow entityEditSectionRow = new EntityEditSectionRow();
                entityEditSectionRow.properties = new List<EntityEditSectionRowProperty>();
                foreach (ListItemEditSectionRowProperty listItemEditSectionRowProperty in listItemEditSectionRow.properties)
                {
                    if (string.IsNullOrEmpty(listItemEditSectionRowProperty.compossed))
                    {
                        EntityEditSectionRowProperty entityEditSectionRowProperty = GetPropertiesEdit(pId, listItemEditSectionRowProperty, pData, pCombos, pLang, pGraph);
                        entityEditSectionRow.properties.Add(entityEditSectionRowProperty);
                    }
                }
                entityEditSectionRows.Add(entityEditSectionRow);
            }
            return entityEditSectionRows;
        }

        private EntityEditSectionRowProperty GetPropertiesEdit(string pId, ListItemEditSectionRowProperty pListItemEditSectionRowProperty, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, Dictionary<ListItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, string pLang, string pGraph)
        {
            //TODO X recursivo
            EntityEditSectionRowProperty entityEditSectionRowProperty = new EntityEditSectionRowProperty();
            entityEditSectionRowProperty.property = pListItemEditSectionRowProperty.property;
            entityEditSectionRowProperty.width = pListItemEditSectionRowProperty.width;
            entityEditSectionRowProperty.required = pListItemEditSectionRowProperty.required;
            entityEditSectionRowProperty.multiple = pListItemEditSectionRowProperty.multiple;
            if (pListItemEditSectionRowProperty.auxEntityData != null)
            {
                entityEditSectionRowProperty.entityAuxData = new AuxEntityDataResponse();
                entityEditSectionRowProperty.entityAuxData.childsOrder = new Dictionary<string, int>();
                entityEditSectionRowProperty.entityAuxData.rdftype = pListItemEditSectionRowProperty.auxEntityData.rdftype;
                entityEditSectionRowProperty.entityAuxData.propertyOrder = pListItemEditSectionRowProperty.auxEntityData.propertyOrder;

                entityEditSectionRowProperty.entityAuxData.titleConfig = new EntityAuxProperty();
                entityEditSectionRowProperty.entityAuxData.titleConfig = new EntityAuxProperty() { route = pListItemEditSectionRowProperty.auxEntityData.propertyTitle.GetRoute() };


                entityEditSectionRowProperty.entityAuxData.propertiesConfig = new List<EntityAuxProperty>();
                foreach (ListItemConfigProperty listItemConfigProperty in pListItemEditSectionRowProperty.auxEntityData.properties)
                {
                    entityEditSectionRowProperty.entityAuxData.propertiesConfig.Add(new EntityAuxProperty() { name = UtilityCV.GetTextLang(pLang, listItemConfigProperty.name), route = listItemConfigProperty.child.GetRoute() });
                }

            }
            if (pListItemEditSectionRowProperty.entityData != null)
            {
                entityEditSectionRowProperty.entityData = new EntityDataResponse();
                entityEditSectionRowProperty.entityData.rdftype = pListItemEditSectionRowProperty.entityData.rdftype;

                entityEditSectionRowProperty.entityData.titleConfig = new EntityAuxProperty();
                if (pListItemEditSectionRowProperty.entityData.propertyTitle != null)
                {
                    entityEditSectionRowProperty.entityData.titleConfig = new EntityAuxProperty() { route = pListItemEditSectionRowProperty.entityData.graph+"||"+ pListItemEditSectionRowProperty.entityData.propertyTitle.GetRoute() };
                }


                entityEditSectionRowProperty.entityData.propertiesConfig = new List<EntityAuxProperty>();
                foreach (ListItemConfigProperty listItemConfigProperty in pListItemEditSectionRowProperty.entityData.properties)
                {
                    entityEditSectionRowProperty.entityData.propertiesConfig.Add(new EntityAuxProperty() { name = UtilityCV.GetTextLang(pLang, listItemConfigProperty.name), route = pListItemEditSectionRowProperty.entityData.graph + "||" + listItemConfigProperty.child.GetRoute() });
                }
            }
            entityEditSectionRowProperty.title = UtilityCV.GetTextLang(pLang, pListItemEditSectionRowProperty.title);
            entityEditSectionRowProperty.placeholder = UtilityCV.GetTextLang(pLang, pListItemEditSectionRowProperty.placeholder);
            entityEditSectionRowProperty.type = pListItemEditSectionRowProperty.type.ToString();

            entityEditSectionRowProperty.values = new List<string>();
            if (pId != null && pData.ContainsKey(pId))
            {
                foreach (string value in pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.property).Select(x => x["o"].value).Distinct())
                {
                    entityEditSectionRowProperty.values.Add(value);
                }
            }
            if (pListItemEditSectionRowProperty.combo != null)
            {
                entityEditSectionRowProperty.comboValues = pCombos.Where(x =>
                  UtilityCV.GetPropComplete(x.Key.property) == UtilityCV.GetPropComplete(pListItemEditSectionRowProperty.combo.property) &&
                   x.Key.graph == pListItemEditSectionRowProperty.combo.graph &&
                   x.Key.rdftype == pListItemEditSectionRowProperty.combo.rdftype
                ).FirstOrDefault().Value;
            }
            if (pListItemEditSectionRowProperty.type == DataTypeEdit.auxEntity)
            {
                if (pListItemEditSectionRowProperty.auxEntityData != null && pListItemEditSectionRowProperty.auxEntityData.rows != null && pListItemEditSectionRowProperty.auxEntityData.rows.Count > 0)
                {
                    entityEditSectionRowProperty.entityAuxData.entities = new Dictionary<string, List<EntityEditSectionRow>>();
                    entityEditSectionRowProperty.entityAuxData.entityRows = GetRowsEdit(null, pListItemEditSectionRowProperty.auxEntityData.rows, pData, pCombos, pLang, pGraph);
                    entityEditSectionRowProperty.entityAuxData.title = new Dictionary<string, EntityAuxProperty>();
                    entityEditSectionRowProperty.entityAuxData.properties = new Dictionary<string, List<EntityAuxProperty>>();
                    foreach (string id in entityEditSectionRowProperty.values)
                    {
                        entityEditSectionRowProperty.entityAuxData.entities.Add(id, GetRowsEdit(id, pListItemEditSectionRowProperty.auxEntityData.rows, pData, pCombos, pLang, pGraph));
                        if (!string.IsNullOrEmpty(entityEditSectionRowProperty.entityAuxData.propertyOrder))
                        {
                            string orden = GetPropertiesEdit(id, new ListItemEditSectionRowProperty() { property = entityEditSectionRowProperty.entityAuxData.propertyOrder }, pData, pCombos, pLang, pGraph).values.FirstOrDefault();
                            int ordenInt = 0;
                            int.TryParse(orden, out ordenInt);
                            entityEditSectionRowProperty.entityAuxData.childsOrder[id] = ordenInt;
                        }

                        if (pListItemEditSectionRowProperty.auxEntityData.propertyTitle != null)
                        {
                            string title = GetPropValues(id, UtilityCV.GetPropComplete(pListItemEditSectionRowProperty.auxEntityData.propertyTitle), pData).FirstOrDefault();
                            string routeTitle = pListItemEditSectionRowProperty.auxEntityData.propertyTitle.GetRoute();
                            if (string.IsNullOrEmpty(title))
                            {
                                title = "";
                            }
                            entityEditSectionRowProperty.entityAuxData.title.Add(id, new EntityAuxProperty() { value = title, route = routeTitle });
                        }

                        if (pListItemEditSectionRowProperty.auxEntityData.properties != null)
                        {
                            foreach (ListItemConfigProperty listItemConfigProperty in pListItemEditSectionRowProperty.auxEntityData.properties)
                            {
                                List<string> valores = GetPropValues(id, UtilityCV.GetPropComplete(listItemConfigProperty.child), pData);
                                string routeProp = listItemConfigProperty.child.GetRoute();
                                if (!entityEditSectionRowProperty.entityAuxData.properties.ContainsKey(id))
                                {
                                    entityEditSectionRowProperty.entityAuxData.properties[id] = new List<EntityAuxProperty>();
                                }

                                entityEditSectionRowProperty.entityAuxData.properties[id].Add(new EntityAuxProperty() { name = UtilityCV.GetTextLang(pLang, listItemConfigProperty.name), value = string.Join(", ", valores), route = routeProp });

                            }
                        }
                    }
                }
            }

            if (pListItemEditSectionRowProperty.type == DataTypeEdit.entity)
            {
                if (pListItemEditSectionRowProperty.entityData != null )
                {
                    entityEditSectionRowProperty.entityData.title = new Dictionary<string, EntityAuxProperty>();
                    entityEditSectionRowProperty.entityData.properties = new Dictionary<string, List<EntityAuxProperty>>();
                    foreach (string id in entityEditSectionRowProperty.values)
                    {
                        if (pListItemEditSectionRowProperty.entityData.propertyTitle != null)
                        {
                            string title = GetPropValues(id, UtilityCV.GetPropComplete(pListItemEditSectionRowProperty.entityData.propertyTitle), pData).FirstOrDefault();
                            string routeTitle = pListItemEditSectionRowProperty.entityData.propertyTitle.GetRoute();
                            if (string.IsNullOrEmpty(title))
                            {
                                title = "";
                            }
                            entityEditSectionRowProperty.entityData.title.Add(id, new EntityAuxProperty() { value = title, route = pListItemEditSectionRowProperty.entityData.graph + "||" + routeTitle });
                        }

                        if (pListItemEditSectionRowProperty.entityData.properties != null)
                        {
                            foreach (ListItemConfigProperty listItemConfigProperty in pListItemEditSectionRowProperty.entityData.properties)
                            {
                                List<string> valores = GetPropValues(id, UtilityCV.GetPropComplete(listItemConfigProperty.child), pData);
                                string routeProp = listItemConfigProperty.child.GetRoute();
                                if (!entityEditSectionRowProperty.entityData.properties.ContainsKey(id))
                                {
                                    entityEditSectionRowProperty.entityData.properties[id] = new List<EntityAuxProperty>();
                                }

                                entityEditSectionRowProperty.entityData.properties[id].Add(new EntityAuxProperty() { name = UtilityCV.GetTextLang(pLang, listItemConfigProperty.name), value = string.Join(", ", valores), route = pListItemEditSectionRowProperty.entityData.graph + "||" + routeProp });

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
        /// <param name="pListItemEditSectionRowPropertyCombo">Configuración de un combo para edición</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetSubjectsCombo(ListItemEditSectionRowPropertyCombo pListItemEditSectionRowPropertyCombo, string pLang)
        {
            int paginacion = 10000;
            int offset = 0;
            int limit = paginacion;
            HashSet<string> ids = new HashSet<string>();
            while (limit == paginacion)
            {
                //Obtenemos los IDS
                string select = "select * where{select distinct ?s";
                string where = $"where{{?s a <{pListItemEditSectionRowPropertyCombo.rdftype}> }} order by asc(?s)}} limit {limit} offset {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, pListItemEditSectionRowPropertyCombo.graph);
                limit = sparqlObject.results.bindings.Count;
                offset += sparqlObject.results.bindings.Count;
                foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObject.results.bindings)
                {
                    ids.Add(fila["s"].value);
                }
            }
            List<PropertyData> propertyDatas = new List<PropertyData>();
            propertyDatas.Add(pListItemEditSectionRowPropertyCombo.property.GenerarPropertyData(pListItemEditSectionRowPropertyCombo.graph));
            return UtilityCV.GetProperties(ids, pListItemEditSectionRowPropertyCombo.graph, propertyDatas, pLang);
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
                                last = new PropertyData();
                                last.property = prop;
                                last.childs = new List<PropertyData>();
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
                                last = new PropertyData();
                                last.property = prop;
                                last.childs = new List<PropertyData>();
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