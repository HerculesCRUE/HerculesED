using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API.Templates
{  
    //Contiene las clases mecesarias para la configuración de la edición de un item de un listado de una pestaña


    /// <summary>
    /// Datos de configuración de edición para una entidad
    /// </summary>
    public class ItemEdit
    {
        /// <summary>
        /// Secciones
        /// </summary>
        public List<ItemEditSection> sections;        
        /// <summary>
        /// Grafo en el que está la entidad
        /// </summary>
        public string graph;        
        /// <summary>
        /// Propiedad utilizada para el título del recurso
        /// </summary>
        public string proptitle;
        /// <summary>
        /// Propiedad utilizada para la descripción del recurso
        /// </summary>
        public string propdescription;
        /// <summary>
        /// rdf:type de la entidad
        /// </summary>
        public string rdftype;
        /// <summary>
        /// Genera el PropertyData para recuperar los datos
        /// </summary>
        /// <param name="pGraph">Grafo</param>
        /// <returns></returns>
        public List<Utils.PropertyData> GenerarPropertyDatas(string pGraph)
        {
            List<Utils.PropertyData> propertyDatas = new List<Utils.PropertyData>();
            if (this.sections != null)
            {
                foreach (ItemEditSection itemEditSection in this.sections)
                {
                    if (itemEditSection.rows != null)
                    {
                        foreach (ItemEditSectionRow itemEditSectionRow in itemEditSection.rows)
                        {
                            List<Utils.PropertyData> aux = itemEditSectionRow.GenerarPropertyDatas(pGraph);
                            propertyDatas.AddRange(aux);
                        }
                    }
                }
            }
            for (int i = 0; i < propertyDatas.Count; i++)
            {
                Utils.PropertyData propertyData = propertyDatas[i];
                Utils.UtilityCV.CleanPropertyData(ref propertyData);
            }
            return propertyDatas;
        }
    }

    /// <summary>
    /// Datos de presentación de una sección de una minificha para la edición
    /// </summary>
    public class ItemEditSection
    {
        /// <summary>
        /// Título de la sección
        /// </summary>
        public Dictionary<string, string> title;
        /// <summary>
        /// Filas de edición de la sección
        /// </summary>
        public List<ItemEditSectionRow> rows;
    }

    /// <summary>
    /// Fila de edición de la sección
    /// </summary>
    public class ItemEditSectionRow
    {
        /// <summary>
        /// Propiedad de edición dentro de la sección
        /// </summary>
        public List<ItemEditSectionRowProperty> properties;
        /// <summary>
        /// Genera el PropertyData para recuperar los datos
        /// </summary>
        /// <param name="pGraph">Grafo</param>
        /// <returns></returns>
        public List<Utils.PropertyData> GenerarPropertyDatas(string pGraph)
        {
            List<Utils.PropertyData> propertyDatas = new List<Utils.PropertyData>();
            foreach (ItemEditSectionRowProperty itemEditSectionRowProperty in this.properties)
            {
                Utils.PropertyData property = new Utils.PropertyData()
                {
                    property = itemEditSectionRowProperty.property,
                    childs = new List<Utils.PropertyData>(),
                    graph = pGraph
                };
                if (!propertyDatas.Exists(x => x.property == itemEditSectionRowProperty.property))
                {
                    propertyDatas.Add(property);
                }
                else
                {
                    property = propertyDatas.First(x => x.property == property.property);
                }
                if (itemEditSectionRowProperty.auxEntityData != null && itemEditSectionRowProperty.auxEntityData.rows != null)
                {
                    foreach (ItemEditSectionRow itemEditSectionRowIn in itemEditSectionRowProperty.auxEntityData.rows)
                    {
                        property.childs.AddRange(itemEditSectionRowIn.GenerarPropertyDatas(pGraph));
                    }
                }
                if (itemEditSectionRowProperty.auxEntityData != null)
                {
                    if (!string.IsNullOrEmpty(itemEditSectionRowProperty.auxEntityData.propertyOrder))
                    {
                        property.childs.Add(new Utils.PropertyData() { property = itemEditSectionRowProperty.auxEntityData.propertyOrder, graph = pGraph });
                    }
                    if (itemEditSectionRowProperty.auxEntityData.propertyTitle != null)
                    {
                        property.childs.Add(itemEditSectionRowProperty.auxEntityData.propertyTitle.GenerarPropertyData(pGraph));
                    }
                    if (itemEditSectionRowProperty.auxEntityData.properties != null)
                    {
                        foreach (ItemEditEntityProperty entityProperty in itemEditSectionRowProperty.auxEntityData.properties)
                        {
                            property.childs.Add(entityProperty.child.GenerarPropertyData(pGraph));
                        }
                    }
                }

                if (itemEditSectionRowProperty.entityData != null)
                {
                    property.graph = itemEditSectionRowProperty.entityData.graph;
                    if (itemEditSectionRowProperty.entityData.propertyTitle != null)
                    {
                        property.childs.Add(itemEditSectionRowProperty.entityData.propertyTitle.GenerarPropertyData(property.graph));
                    }
                    if (itemEditSectionRowProperty.entityData.properties != null)
                    {
                        foreach (ItemEditEntityProperty entityProperty in itemEditSectionRowProperty.entityData.properties)
                        {
                            property.childs.Add(entityProperty.child.GenerarPropertyData(property.graph));
                        }
                    }
                }

                if (property.childs == null || property.childs.Count == 0)
                {
                    property.graph = null;
                }
            }
            return propertyDatas;
        }
    }

    /// <summary>
    /// Propiedad de edición dentro de la fila de una sección de la sección
    /// </summary>
    public class ItemEditSectionRowProperty
    {
        /// <summary>
        /// Propiedad de edición dentro de la fila de la sección
        /// </summary>
        public string property;
        //public string propertyOrder { get; set; }
        /// <summary>
        /// Nombre de la propiedad
        /// </summary>
        public Dictionary<string, string> title;
        /// <summary>
        /// Nombre para el placeholder
        /// </summary>
        public Dictionary<string, string> placeholder;
        /// <summary>
        /// Anchura de la propiedad
        /// </summary>
        public int width;
        /// <summary>
        /// Indica si es multiple
        /// </summary>
        public bool multiple;
        /// <summary>
        /// Indica si es obligatorio
        /// </summary>
        public bool required;
        /// <summary>
        /// Datos para el combo
        /// </summary>
        public ItemEditSectionRowPropertyCombo combo;
        /// <summary>
        /// Tipo de la propiedad
        /// TODO
        /// </summary>
        public DataTypeEdit type;
        /// <summary>
        /// Datos de la entidad auxiliar
        /// </summary>
        public ItemEditAuxEntityData auxEntityData;
        /// <summary>
        /// Datos de la entidad a la que apunta
        /// </summary>
        public ItemEditEntityData entityData;
        /// <summary>
        /// Composición de la propiedad
        /// </summary>
        public string compossed;
    }

    public class ItemEditAuxEntityData
    {
        /// <summary>
        /// rdf:type de la entidad auxiliar
        /// </summary>
        public string rdftype;
        /// <summary>
        /// Propiedad (int) para ordenar (en caso de que exista)
        /// </summary>
        public string propertyOrder;
        /// <summary>
        /// Propiedad representante para el título
        /// TODO
        /// </summary>
        public PropertyDataTemplate propertyTitle;
        /// <summary>
        /// Propiedades representantes
        /// </summary>
        public List<ItemEditEntityProperty> properties;
        /// <summary>
        /// Filas con las propiedades de edición
        /// </summary>
        public List<ItemEditSectionRow> rows;
    }

    public class ItemEditEntityData
    {
        /// <summary>
        /// rdf:type de la entidad
        /// </summary>
        public string rdftype;
        /// <summary>
        /// Grafo de la entidad
        /// </summary>
        public string graph;
        /// <summary>
        /// Propiedad representante para el título
        /// </summary>
        public PropertyDataTemplate propertyTitle;
        /// <summary>
        /// Propiedades representantes
        /// </summary>
        public List<ItemEditEntityProperty> properties;
    }

    /// <summary>
    /// Configuracion para una propiedad
    /// </summary>
    public class ItemEditEntityProperty
    {
        /// <summary>
        /// Tipo de la propiedad a pintar
        /// </summary>
        public DataTypeListItem type;
        /// <summary>
        /// Nombre de la propiedad
        /// </summary>
        public Dictionary<string, string> name;
        /// <summary>
        /// Propiedad a pintar
        /// TODO
        /// </summary>
        public PropertyDataTemplate child;
    }

    /// <summary>
    /// Configuración de un combo para edición
    /// </summary>
    public class ItemEditSectionRowPropertyCombo
    {
        /// <summary>
        /// Propiedad a recuperar de la entidad para el nombre
        /// TODO
        /// </summary>
        public PropertyDataTemplate property;
        /// <summary>
        /// Grafo en el que está la entidad
        /// </summary>
        public string graph;
        /// <summary>
        /// rdf:type de la entidad a recuperar
        /// </summary>
        public string rdftype;

    }

}


