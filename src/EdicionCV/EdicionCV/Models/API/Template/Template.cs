using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API.Template
{

    /// <summary>
    /// Enumeración con los tipos de las secciones
    /// </summary>
    public enum SectionPresentationType
    {
        table,
    }

    /// <summary>
    /// Enumeración con los tipos disponibles en los listados de items
    /// </summary>
    public enum DataTypeListItem
    {
        text,
        date
    }

    /// <summary>
    /// Enumeración con los tipos disponibles en los datos de edición
    /// </summary>
    public enum DataTypeEdit
    {
        text,
        textarea,
        date,
        number,
        selectCombo,
        auxEntity,
        entity,
    }


    /// <summary>
    /// Representa el template de una pestaña del CV
    /// </summary>
    public class TabTemplate
    {
        /// <summary>
        /// rdf:type de la entidad representante de la pestaña
        /// </summary>
        public string rdftype { get; set; }
        /// <summary>
        /// propiedad que apunta a la entidad representante de la pestaña
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Secciones de la pestaña
        /// </summary>
        public List<Section> sections { get; set; }
    }

    /// <summary>
    /// Representa una sección de la pestaña
    /// </summary>
    public class Section
    {
        /// <summary>
        /// Propiedad que apunta a la sección
        /// </summary>
        public string property { get; set; }

        /// <summary>
        /// rdf:type de la entidad auxiliar
        /// </summary>
        public string rdftype { get; set; }

        /// <summary>
        /// Presentacion de la sección
        /// </summary>
        public SectionPresentation presentation { get; set; }

        /// <summary>
        /// Genera el PropertyData para recuperar los datos
        /// </summary>
        /// <param name="pGraph">grafo</param>
        /// <returns></returns>
        public Utils.PropertyData GenerarPropertyData(string pGraph)
        {
            Utils.PropertyData propertyData = this.presentation.listItemConfig.GenerarPropertyData(pGraph);
            propertyData.property = this.property;
            return propertyData;
        }
    }

    /// <summary>
    /// Datos de presentación de una sección
    /// </summary>
    public class SectionPresentation
    {
        /// <summary>
        /// Tipo de presentación de una sección
        /// </summary>
        public SectionPresentationType type { get; set; }
        /// <summary>
        /// Título de la sección
        /// </summary>
        public Dictionary<string, string> title { get; set; }
        /// <summary>
        /// Datos de configuración para los items del listado de la tabla
        /// </summary>
        public ListItemConfig listItemConfig { get; set; }
        /// <summary>
        /// Datos de configuración de edición para los items del listado de la tabla
        /// </summary>
        public ListItemEdit listItemEdit { get; set; }
    }


    /// <summary>
    /// Datos de configuración para los items del listado de la tabla
    /// </summary>
    public class ListItemConfig
    {
        /// <summary>
        /// Propiedad para pintar el título del ítem
        /// </summary>
        public PropertyDataTemplate propertyTitle { get; set; }
        /// <summary>
        /// Órdenes disponibles en el listado
        /// </summary>
        public List<ListItemConfigOrder> orders { get; set; }
        /// <summary>
        /// Listado con las propiedades a pintar en el listado
        /// </summary>
        public List<ListItemConfigProperty> properties { get; set; }

        public Utils.PropertyData GenerarPropertyData(string pGraph)
        {
            Utils.PropertyData propertyData = new Utils.PropertyData();
            propertyData.graph = pGraph;
            propertyData.childs = new List<Utils.PropertyData>();

            if (orders != null)
            {
                foreach (ListItemConfigOrder presentation in orders)
                {
                    foreach (ListItemConfigOrderProperty presentationOrderProperty in presentation.properties)
                    {
                        propertyData.childs.Add(presentationOrderProperty.GenerarPropertyData(pGraph));
                    }
                }
            }

            if (propertyTitle != null)
            {
                propertyData.childs.Add(propertyTitle.GenerarPropertyData(pGraph));
            }

            Utils.PropertyData property = new Utils.PropertyData();
            property.property = Utils.UtilityCV.PropertyIspublic;
            property.childs = new List<Utils.PropertyData>();
            if (!propertyData.childs.Exists(x => x.property == property.property))
            {
                propertyData.childs.Add(property);
            }

            foreach (ListItemConfigProperty prop in properties)
            {
                if (prop.child != null)
                {
                    propertyData.childs.Add(prop.child.GenerarPropertyData(pGraph));
                }
                else if (prop.childOR != null)
                {
                    foreach (PropertyDataTemplate propOR in prop.childOR)
                    {
                        propertyData.childs.Add(propOR.GenerarPropertyData(pGraph));
                    }
                }
            }
            Utils.UtilityCV.CleanPropertyData(ref propertyData);
            return propertyData;
        }
    }

    /// <summary>
    /// Configuración de un orden de presentación para el listado
    /// </summary>
    public class ListItemConfigOrder
    {
        /// <summary>
        /// Nombre
        /// </summary>
        public Dictionary<string, string> name { get; set; }
        /// <summary>
        /// Propiedades utilizadas para los órdenes
        /// </summary>
        public List<ListItemConfigOrderProperty> properties { get; set; }
    }


    /// <summary>
    /// Configuración de una propiedad utilizada para los órdenes
    /// </summary>
    public class ListItemConfigOrderProperty : PropertyDataTemplate
    {
        /// <summary>
        /// TRUE para orden ascendente
        /// </summary>
        public bool asc { get; set; }
    }


    /// <summary>
    /// Listado con las propiedades a pintar en el listado 
    /// </summary>
    public class ListItemConfigProperty
    {
        /// <summary>
        /// Tipo de la propiedad a pintar
        /// </summary>
        public DataTypeListItem type { get; set; }
        /// <summary>
        /// Nombre de la propiedad
        /// </summary>
        public Dictionary<string, string> name { get; set; }
        /// <summary>
        /// Indica si se muestra en la minificha (sin desplegar)
        /// </summary>
        public bool showMini { get; set; }
        /// <summary>
        /// Indica si se muestra en la minificha en negrita(sin desplegar)
        /// </summary>
        public bool showMiniBold { get; set; }
        /// <summary>
        /// Propiedad a pintar
        /// </summary>
        public PropertyDataTemplate child { get; set; }
        /// <summary>
        /// Propiedades a pintar de forma alternativa (si no está la primera se pinta la siguiente y así sucesivamente)
        /// </summary>
        public List<PropertyDataTemplate> childOR { get; set; }
    }


    /// <summary>
    /// Datos de configuración de edición para los items del listado de la tabla
    /// //TODO cambiar ItemEdit
    /// </summary>
    public class ListItemEdit
    {
        /// <summary>
        /// Secciones
        /// </summary>
        public List<ListItemEditSection> sections { get; set; }
        /// <summary>
        /// Propiedad para acceder a la entidad desde la minificha
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Grafo en el que está la entidad
        /// </summary>
        public string graph { get; set; }
        /// <summary>
        /// Propiedad utilizada para el título del recurso
        /// </summary>
        public string proptitle { get; set; }
        /// <summary>
        /// Propiedad utilizada para la descripción del recurso
        /// </summary>
        public string propdescription { get; set; }
        /// <summary>
        /// rdf:type de la entidad
        /// </summary>
        public string rdftype { get; set; }
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
                foreach (ListItemEditSection listItemEditSection in this.sections)
                {
                    if (listItemEditSection.rows != null)
                    {
                        foreach (ListItemEditSectionRow listItemEditSectionRow in listItemEditSection.rows)
                        {
                            List<Utils.PropertyData> aux = listItemEditSectionRow.GenerarPropertyDatas(pGraph);
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
    public class ListItemEditSection
    {
        /// <summary>
        /// Título de la sección
        /// </summary>
        public Dictionary<string, string> title { get; set; }
        /// <summary>
        /// Filas de edición de la sección
        /// </summary>
        public List<ListItemEditSectionRow> rows { get; set; }
    }

    /// <summary>
    /// Fila de edición de la sección
    /// </summary>
    public class ListItemEditSectionRow
    {
        /// <summary>
        /// Propiedad de edición dentro de la sección
        /// </summary>
        public List<ListItemEditSectionRowProperty> properties { get; set; }
        /// <summary>
        /// Genera el PropertyData para recuperar los datos
        /// </summary>
        /// <param name="pGraph">Grafo</param>
        /// <returns></returns>
        public List<Utils.PropertyData> GenerarPropertyDatas(string pGraph)
        {
            List<Utils.PropertyData> propertyDatas = new List<Utils.PropertyData>();
            foreach (ListItemEditSectionRowProperty listItemEditSectionRowProperty in this.properties)
            {
                Utils.PropertyData property = new Utils.PropertyData();
                property.property = listItemEditSectionRowProperty.property;
                property.childs = new List<Utils.PropertyData>();
                property.graph = pGraph;
                if (!propertyDatas.Exists(x => x.property == listItemEditSectionRowProperty.property))
                {
                    propertyDatas.Add(property);
                }
                else
                {
                    property = propertyDatas.First(x => x.property == property.property);
                }
                if (listItemEditSectionRowProperty.auxEntityData != null && listItemEditSectionRowProperty.auxEntityData.rows != null)
                {
                    foreach (ListItemEditSectionRow listItemEditSectionRowIn in listItemEditSectionRowProperty.auxEntityData.rows)
                    {
                        property.childs.AddRange(listItemEditSectionRowIn.GenerarPropertyDatas(pGraph));
                    }
                }
                if (listItemEditSectionRowProperty.auxEntityData != null)
                {
                    if (!string.IsNullOrEmpty(listItemEditSectionRowProperty.auxEntityData.propertyOrder))
                    {
                        property.childs.Add(new Utils.PropertyData() { property = listItemEditSectionRowProperty.auxEntityData.propertyOrder, graph = pGraph });
                    }
                    if (listItemEditSectionRowProperty.auxEntityData.propertyTitle != null)
                    {
                        property.childs.Add(listItemEditSectionRowProperty.auxEntityData.propertyTitle.GenerarPropertyData(pGraph));
                    }
                    if (listItemEditSectionRowProperty.auxEntityData.properties != null)
                    {
                        foreach (ListItemConfigProperty listItemConfigProperty in listItemEditSectionRowProperty.auxEntityData.properties)
                        {
                            property.childs.Add(listItemConfigProperty.child.GenerarPropertyData(pGraph));
                        }
                    }
                }

                if (listItemEditSectionRowProperty.entityData != null)
                {
                    property.graph = listItemEditSectionRowProperty.entityData.graph;
                    if (listItemEditSectionRowProperty.entityData.propertyTitle != null)
                    {
                        property.childs.Add(listItemEditSectionRowProperty.entityData.propertyTitle.GenerarPropertyData(property.graph));
                    }
                    if (listItemEditSectionRowProperty.entityData.properties != null)
                    {
                        foreach (ListItemConfigProperty listItemConfigProperty in listItemEditSectionRowProperty.entityData.properties)
                        {
                            property.childs.Add(listItemConfigProperty.child.GenerarPropertyData(property.graph));
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
    public class ListItemEditSectionRowProperty
    {
        /// <summary>
        /// Propiedad de edición dentro de la fila de la sección
        /// </summary>
        public string property { get; set; }
        //public string propertyOrder { get; set; }
        /// <summary>
        /// Nombre de la propiedad
        /// </summary>
        public Dictionary<string, string> title { get; set; }
        /// <summary>
        /// Nombre para el placeholder
        /// </summary>
        public Dictionary<string, string> placeholder { get; set; }
        /// <summary>
        /// Anchura de la propiedad
        /// </summary>
        public int width { get; set; }
        /// <summary>
        /// Indica si es multiple
        /// </summary>
        public bool multiple { get; set; }
        /// <summary>
        /// Indica si es obligatorio
        /// </summary>
        public bool required { get; set; }
        /// <summary>
        /// Datos para el combo
        /// </summary>
        public ListItemEditSectionRowPropertyCombo combo { get; set; }
        /// <summary>
        /// Tipo de la propiedad
        /// </summary>
        public DataTypeEdit type { get; set; }
        /// <summary>
        /// Datos de la entidad auxiliar
        /// </summary>
        public AuxEntityData auxEntityData { get; set; }

        //TODO
        /// <summary>
        /// Datos de la entidad a la que apunta
        /// </summary>
        public EntityData entityData { get; set; }

        //TODO
        public string compossed { get; set; }
    }

    public class AuxEntityData
    {
        public string rdftype { get; set; }
        public string propertyOrder { get; set; }
        public PropertyDataTemplate propertyTitle { get; set; }
        //cambiar el objeto
        public List<ListItemConfigProperty> properties { get; set; }
        public List<ListItemEditSectionRow> rows { get; set; }
    }

    public class EntityData
    {
        public string rdftype { get; set; }
        public string graph { get; set; }
        public PropertyDataTemplate propertyTitle { get; set; }
        public List<ListItemConfigProperty> properties { get; set; }
    }


    /// <summary>
    /// Configuración de un combo para edición
    /// </summary>
    public class ListItemEditSectionRowPropertyCombo
    {
        /// <summary>
        /// Propiedad a recuperar de la entidad para el nombre
        /// </summary>
        public PropertyDataTemplate property { get; set; }
        /// <summary>
        /// Grafo en el que está la entidad
        /// </summary>
        public string graph { get; set; }
        /// <summary>
        /// rdf:type de la entidad a recuperar
        /// </summary>
        public string rdftype { get; set; }

    }






    ///// <summary>
    ///// Datos de las propiedades a recuperar //TODO dividir
    ///// </summary>
    //public class PropertyDataX
    //{
    //    /// <summary>
    //    /// Propiedad
    //    /// </summary>
    //    public string property { get; set; }
    //    /// <summary>
    //    /// Grafo de los valores de la propiedad
    //    /// </summary>
    //    public string graph { get; set; }
    //    /// <summary>
    //    /// Propiedad para ordenar los valores
    //    /// </summary>
    //    public string order { get; set; }
    //    /// <summary>
    //    /// Propiedades 'hijas' 
    //    /// </summary>
    //    public List<PropertyDataX> childs { get; set; }
    //    public bool asc { get; set; }
    //    /// <summary>
    //    /// Propiedad (separado por || si hay que coger alternativas)
    //    /// </summary>
    //    public PropertyDataX child { get; set; }

    //}


    /// <summary>
    /// Clase genérica para la configuración de las propiedades
    /// </summary>
    public class PropertyDataTemplate
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Grafo en el que buscar los 'hijos'
        /// </summary>
        public string graph { get; set; }
        /// <summary>
        /// 'Hijos'
        /// </summary>
        public PropertyDataTemplate child { get; set; }

        /// <summary>
        /// Orden
        /// </summary>
        public string order { get; set; }

        public Utils.PropertyData GenerarPropertyData(string pGraph)
        {
            Utils.PropertyData property = new Utils.PropertyData();
            property.property = this.property;
            property.order = order;
            property.childs = new List<Utils.PropertyData>();
            if (this.child != null)
            {
                string graphAux = this.graph;
                if (string.IsNullOrEmpty(graphAux))
                {
                    graphAux = pGraph;
                }
                property.graph = graphAux;
                Utils.UtilityCV.GenerarPropertyData(this, ref property, graphAux);
                Utils.UtilityCV.CleanPropertyData(ref property);
            }
            return property;
        }

        public string GetRoute()
        {
            string route = property;
            if (!string.IsNullOrEmpty(graph))
            {
                route += "||" + graph;
            }
            if (child != null)
            {
                route += "||" + child.GetRoute();
            }
            while (route.EndsWith("||"))
            {
                route = route.Substring(0, route.Length - 2);

            }
            return route;
        }
    }
}


