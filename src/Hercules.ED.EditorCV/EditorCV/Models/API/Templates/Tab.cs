using EditorCV.Models.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EditorCV.Models.API.Templates
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
        public string rdftype { get; set; }

        /// <summary>
        /// propiedad que apunta a la entidad representante de la pesta�a
        /// </summary>
        public string property { get; set; }

        /// <summary>
        /// Secciones de la pesta�a
        /// </summary>
        public List<TabSection> sections { get; set; }

        /// <summary>
        /// Indica si se trata de los datos personales
        /// </summary>
        public bool personalData { get; set; }

        /// <summary>
        /// Secciones de los datos personales
        /// </summary>
        public ItemEdit personalDataSections { get; set; }

        /// <summary>
        /// T�tulo de la secci�n
        /// </summary>
        public Dictionary<string,string> title { get; set; }
    }

    /// <summary>
    /// Representa una secci�n de la pesta�a
    /// </summary>
    public class TabSection
    {
        /// <summary>
        /// Propiedad que apunta a la secci�n
        /// </summary>
        public string property { get; set; }

        /// <summary>
        /// rdf:type de la entidad auxiliar
        /// </summary>
        public string rdftype { get; set; }

        /// <summary>
        /// Presentacion de la secci�n
        /// </summary>
        public TabSectionPresentation presentation { get; set; }

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
                        PropertyData propertyDataListItems = this.presentation.listItemsPresentation.listItem.GenerarPropertyData(pGraph);
                        //Editabilidad
                        foreach (string propEditabilidad in UtilityCV.PropertyNotEditable.Keys)
                        {
                            propertyDataListItems.childs.First(x => x.graph == this.presentation.listItemsPresentation.listItemEdit.graph).childs.Add(
                                //Editabilidad
                                new PropertyData()
                                {
                                    property = propEditabilidad,
                                    childs = new List<PropertyData>()
                                }
                            );
                        }
                        //OpenAccess
                        propertyDataListItems.childs.First(x => x.graph == this.presentation.listItemsPresentation.listItemEdit.graph).childs.Add(
                                //Editabilidad
                                new PropertyData()
                                {
                                    property = UtilityCV.PropertyOpenAccess,
                                    childs = new List<PropertyData>()
                                }
                            );

                        //ProjectAuthorization
                        propertyDataListItems.childs.First(x => x.graph == this.presentation.listItemsPresentation.listItemEdit.graph).childs.Add(
                               //Editabilidad
                               new PropertyData()
                               {
                                   property = "http://w3id.org/roh/projectAuthorization",
                                   childs = new List<PropertyData>()
                               }
                           );

                        propertyDataListItems.property = this.property;
                        return propertyDataListItems;
                    }
                case TabSectionPresentationType.item:
                    {
                        List<PropertyData> propertyDatasItem = this.presentation.itemPresentation.itemEdit.GenerarPropertyDatas(pGraph);
                        PropertyData propertyData = new PropertyData();
                        propertyData.property = this.property;
                        propertyData.childs = new List<PropertyData>() { 
                            new PropertyData() { 
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

        /// <summary>
        /// Genera el PropertyData para recuperar los datos de los contadores
        /// </summary>
        /// <param name="pGraph">grafo</param>
        /// <returns></returns>
        public PropertyData GenerarPropertyDataContadores(string pGraph)
        {
            PropertyData propertyDataListItems = this.presentation.listItemsPresentation.listItem.GenerarPropertyData(pGraph);
            propertyDataListItems.property = this.property;
            
            //Eliminamos 'ispublic'
            propertyDataListItems.childs.RemoveAll(x => x.property == "http://w3id.org/roh/isPublic");
            
            //Nos quedamos s�lo con el relatedBy
            propertyDataListItems.childs.First(x => x.property == "http://vivoweb.org/ontology/core#relatedBy").childs = null;
            return propertyDataListItems;
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
        public TabSectionPresentationType type { get; set; }
        /// <summary>
        /// T�tulo de la secci�n
        /// </summary>
        public Dictionary<string, string> title { get; set; }
        /// <summary>
        /// Informaci�n de la secci�n
        /// </summary>
        public Dictionary<string, string> information { get; set; }
        /// <summary>
        /// Es parte del CV abreviado del ISCIII
        /// </summary>
        public bool cvaISCIII { get; set; }
        /// <summary>
        /// Es parte del CV abreviado del AEI
        /// </summary>
        public bool cvaAEI { get; set; }
        /// <summary>
        /// Configuraci�n de la presentaci�n para los listados de items
        /// </summary>
        public TabSectionPresentationListItems listItemsPresentation { get; set; }
        /// <summary>
        /// Configuraci�n de la presentaci�n para un item
        /// </summary>
        public TabSectionPresentationItem itemPresentation { get; set; }
    }

    /// <summary>
    /// Configuraci�n de la presentaci�n para los listados de items
    /// </summary>
    public class TabSectionPresentationListItems
    {
        /// <summary>
        /// Propiedad para indicar los valores indicando si el recurso est� dentro de los ultimos 5 a�os.
        /// </summary>
        public Last5Years last5Years { get; set; }
        /// <summary>
        /// Propiedad para acceder a la entidad desde la minificha
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Propiedad para acceder a la entidad del cv desde la minificha
        /// </summary>
        public string property_cv { get; set; }
        /// <summary>
        /// Propiedad para indicar que se quieren revisar los duplicados
        /// </summary>
        public bool checkDuplicates { get; set; }
        /// <summary>
        /// Propiedad para indicar si los items son publicables
        /// </summary>
        public bool isPublishable { get; set; }
        /// <summary>
        /// Rdf:type de �a entidad del cv 
        /// </summary>
        public string rdftype_cv { get; set; }
        /// <summary>
        /// Datos de configuraci�n para la presentaci�n de los items del listado de la lista
        /// </summary>
        public TabSectionListItem listItem { get; set; }
        /// <summary>
        /// Datos de configuraci�n de edici�n para los items del listado de la lista
        /// </summary>
        public ItemEdit listItemEdit { get; set; }
        /// <summary>
        /// Propiedad con el ID de la secci�n de CVN
        /// </summary>
        public string cvnsection { get; set; }
    }

    /// <summary>
    /// Configuraci�n para indicar si el recurso est� dentro de los �ltimos cinco a�os.
    /// </summary>
    public class Last5Years
    {
        /// <summary>
        /// Booleano indicando si debe a�adirse siempre independientemente de las fechas de inicio y fin.
        /// </summary>
        public bool always { get; set; }
        /// <summary>
        /// Propiedad indicadora del fin temporal del atributo.
        /// </summary>
        public string end { get; set; }
        /// <summary>
        /// Propiedad indicadora del inicio temporal del atributo.
        /// </summary>
        public string start { get; set; }
    }

    /// <summary>
    /// Configuraci�n de la presentaci�n para un item
    /// </summary>
    public class TabSectionPresentationItem
    {
        /// <summary>
        /// Propiedad para acceder a la entidad desde la minificha
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Datos de configuraci�n de edici�n para el item
        /// </summary>
        public ItemEdit itemEdit { get; set; }
    }

    /// <summary>
    /// Datos de configuraci�n para los items del listado de la lista
    /// </summary>
    public class TabSectionListItem
    {
        /// <summary>
        /// Propiedad para pintar el t�tulo del �tem
        /// </summary>
        public PropertyDataTemplate propertyTitle { get; set; }
        /// <summary>
        /// Propiedad para pintar el t�tulo del �tem
        /// </summary>
        public List<PropertyDataTemplate> propertyTitleOR { get; set; }
        /// <summary>
        /// �rdenes disponibles en el listado
        /// </summary>
        public List<TabSectionListItemOrder> orders { get; set; }
        /// <summary>
        /// Listado con las propiedades a pintar en el listado
        /// </summary>
        public List<TabSectionListItemProperty> properties { get; set; }
        public PropertyData GenerarPropertyData(string pGraph)
        {
            PropertyData propertyData = new PropertyData()
            {
                graph = pGraph,
                childs = new List<PropertyData>()
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
            PropertyData property = new PropertyData()
            {
                property = UtilityCV.PropertyIspublic,
                childs = new List<PropertyData>()
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
            UtilityCV.CleanPropertyData(ref propertyData);
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
        public Dictionary<string, string> name { get; set; }
        /// <summary>
        /// Propiedades utilizadas para los �rdenes
        /// </summary>
        public List<TabSectionListItemOrderProperty> properties { get; set; }
    }


    /// <summary>
    /// Configuraci�n de una propiedad utilizada para los �rdenes
    /// </summary>
    public class TabSectionListItemOrderProperty : PropertyDataTemplate
    {
        /// <summary>
        /// TRUE para orden ascendente
        /// </summary>
        public bool asc { get; set; }
    }


    /// <summary>
    /// Propiedad a pintar en el listado 
    /// </summary>
    public class TabSectionListItemProperty
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
        /// Informaci�n del item
        /// </summary>
        public Dictionary<string, string> information { get; set; }
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
        /// Propiedades a pintar de forma alternativa (si no est� la primera se pinta la siguiente y as� sucesivamente)
        /// </summary>
        public List<PropertyDataTemplate> childOR { get; set; }
    }

}


