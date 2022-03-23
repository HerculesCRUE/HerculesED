using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API.Templates
{
    //Clases de configuraci�n para la presetaci�n de las pesta�as

    /// <summary>
    /// Representa el template de una pesta�a del CV
    /// </summary>
    public class Tab
    {
        /// <summary>
        /// rdf:type de la entidad representante de la pesta�a
        /// </summary>
        public string rdftype;

        /// <summary>
        /// propiedad que apunta a la entidad representante de la pesta�a
        /// </summary>
        public string property;

        /// <summary>
        /// Secciones de la pesta�a
        /// </summary>
        public List<TabSection> sections;

        /// <summary>
        /// Indica si se trata de los datos personales
        /// </summary>
        public bool personalData;

        /// <summary>
        /// Secciones de los datos personales
        /// </summary>
        public ItemEdit personalDataSections;
    }

    /// <summary>
    /// Representa una secci�n de la pesta�a
    /// </summary>
    public class TabSection
    {
        /// <summary>
        /// Propiedad que apunta a la secci�n
        /// </summary>
        public string property;

        /// <summary>
        /// rdf:type de la entidad auxiliar
        /// </summary>
        public string rdftype;

        /// <summary>
        /// Presentacion de la secci�n
        /// </summary>
        public TabSectionPresentation presentation;

        /// <summary>
        /// Genera el PropertyData para recuperar los datos
        /// </summary>
        /// <param name="pGraph">grafo</param>
        /// <returns></returns>
        public Utils.PropertyData GenerarPropertyData(string pGraph)
        {
            switch (presentation.type)
            {
                case TabSectionPresentationType.listitems:
                    {
                        Utils.PropertyData propertyDataListItems = this.presentation.listItemsPresentation.listItem.GenerarPropertyData(pGraph);
                        //Editabilidad
                        foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
                        {
                            propertyDataListItems.childs.First(x => x.graph == this.presentation.listItemsPresentation.listItemEdit.graph).childs.Add(
                                //Editabilidad
                                new Utils.PropertyData()
                                {
                                    property = propEditabilidad,
                                    childs = new List<Utils.PropertyData>()
                                }
                            );
                        }
                        propertyDataListItems.property = this.property;
                        return propertyDataListItems;
                    }
                case TabSectionPresentationType.item:
                    {
                        List<Utils.PropertyData> propertyDatasItem = this.presentation.itemPresentation.itemEdit.GenerarPropertyDatas(pGraph);
                        Utils.PropertyData propertyData = new Utils.PropertyData();
                        propertyData.property = this.property;
                        propertyData.childs = new List<Utils.PropertyData>() { 
                            new Utils.PropertyData() { 
                                property = this.presentation.itemPresentation.property,
                                childs=propertyDatasItem,
                                graph=this.presentation.itemPresentation.itemEdit.graph
                            } 
                        };
                        propertyData.graph = "curriculumvitae";
                        return propertyData;
                    }
                default:
                    throw new Exception("No est� implementado la presentaci�n del tipo " + presentation.type);
            }
        }
    }

    /// <summary>
    /// Datos de presentaci�n de una secci�n
    /// </summary>
    public class TabSectionPresentation
    {
        /// <summary>
        /// Tipo de presentaci�n de una secci�n
        /// </summary>
        public TabSectionPresentationType type;
        /// <summary>
        /// T�tulo de la secci�n
        /// </summary>
        public Dictionary<string, string> title;
        /// <summary>
        /// Configuraci�n de la presentaci�n para los listados de items
        /// </summary>
        public TabSectionPresentationListItems listItemsPresentation;
        /// <summary>
        /// Configuraci�n de la presentaci�n para un item
        /// </summary>
        public TabSectionPresentationItem itemPresentation;
    }

    /// <summary>
    /// Configuraci�n de la presentaci�n para los listados de items
    /// </summary>
    public class TabSectionPresentationListItems
    {
        /// <summary>
        /// Propiedad para acceder a la entidad desde la minificha
        /// </summary>
        public string property;
        /// <summary>
        /// Propiedad para acceder a la entidad del cv desde la minificha
        /// </summary>
        public string property_cv;
        /// <summary>
        /// Rdf:type de �a entidad del cv 
        /// </summary>
        public string rdftype_cv;        
        /// <summary>
        /// Datos de configuraci�n para la presentaci�n de los items del listado de la lista
        /// </summary>
        public TabSectionListItem listItem;
        /// <summary>
        /// Datos de configuraci�n de edici�n para los items del listado de la lista
        /// </summary>
        public ItemEdit listItemEdit;
    }

    /// <summary>
    /// Configuraci�n de la presentaci�n para un item
    /// </summary>
    public class TabSectionPresentationItem
    {
        /// <summary>
        /// Propiedad para acceder a la entidad desde la minificha
        /// </summary>
        public string property;
        /// <summary>
        /// Datos de configuraci�n de edici�n para el item
        /// </summary>
        public ItemEdit itemEdit;
    }

    /// <summary>
    /// Datos de configuraci�n para los items del listado de la lista
    /// </summary>
    public class TabSectionListItem
    {
        /// <summary>
        /// Propiedad para pintar el t�tulo del �tem
        /// </summary>
        public PropertyDataTemplate propertyTitle;
        /// <summary>
        /// Propiedad para pintar el t�tulo del �tem
        /// </summary>
        public List<PropertyDataTemplate> propertyTitleOR;

        /// <summary>
        /// �rdenes disponibles en el listado
        /// </summary>
        public List<TabSectionListItemOrder> orders;
        /// <summary>
        /// Listado con las propiedades a pintar en el listado
        /// </summary>
        public List<TabSectionListItemProperty> properties;

        public Utils.PropertyData GenerarPropertyData(string pGraph)
        {
            Utils.PropertyData propertyData = new Utils.PropertyData()
            {
                graph = pGraph,
                childs = new List<Utils.PropertyData>()
            };
            if (orders != null)
            {
                foreach (TabSectionListItemOrder presentation in orders)
                {
                    foreach (TabSectionListItemOrderProperty presentationOrderProperty in presentation.properties)
                    {
                        propertyData.childs.Add(presentationOrderProperty.GenerarPropertyData(pGraph));
                    }
                }
            }
            if (propertyTitle != null)
            {
                propertyData.childs.Add(propertyTitle.GenerarPropertyData(pGraph));
            }
            if(propertyTitleOR!=null)
            {
                foreach (PropertyDataTemplate propOR in propertyTitleOR)
                {
                    propertyData.childs.Add(propOR.GenerarPropertyData(pGraph));
                }
            }

            //Visibilidad
            Utils.PropertyData property = new Utils.PropertyData()
            {
                property = Utils.UtilityCV.PropertyIspublic,
                childs = new List<Utils.PropertyData>()
            };
            if (!propertyData.childs.Exists(x => x.property == property.property))
            {
                propertyData.childs.Add(property);
            }
            if (properties != null)
            {
                foreach (TabSectionListItemProperty prop in properties)
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
            }
            Utils.UtilityCV.CleanPropertyData(ref propertyData);
            return propertyData;
        }
    }

    /// <summary>
    /// Configuraci�n de un orden de presentaci�n para el listado
    /// </summary>
    public class TabSectionListItemOrder
    {
        /// <summary>
        /// Nombre
        /// </summary>
        public Dictionary<string, string> name;
        /// <summary>
        /// Propiedades utilizadas para los �rdenes
        /// </summary>
        public List<TabSectionListItemOrderProperty> properties;
    }


    /// <summary>
    /// Configuraci�n de una propiedad utilizada para los �rdenes
    /// </summary>
    public class TabSectionListItemOrderProperty : PropertyDataTemplate
    {
        /// <summary>
        /// TRUE para orden ascendente
        /// </summary>
        public bool asc;
    }


    /// <summary>
    /// Propiedad a pintar en el listado 
    /// </summary>
    public class TabSectionListItemProperty
    {
        /// <summary>
        /// Tipo de la propiedad a pintar
        /// TODO
        /// </summary>
        public DataTypeListItem type;
        /// <summary>
        /// Nombre de la propiedad
        /// </summary>
        public Dictionary<string, string> name;
        /// <summary>
        /// Indica si se muestra en la minificha (sin desplegar)
        /// </summary>
        public bool showMini;
        /// <summary>
        /// Indica si se muestra en la minificha en negrita(sin desplegar)
        /// </summary>
        public bool showMiniBold;
        /// <summary>
        /// Propiedad a pintar
        /// </summary>
        public PropertyDataTemplate child;
        /// <summary>
        /// Propiedades a pintar de forma alternativa (si no est� la primera se pinta la siguiente y as� sucesivamente)
        /// </summary>
        public List<PropertyDataTemplate> childOR;
    }

}


