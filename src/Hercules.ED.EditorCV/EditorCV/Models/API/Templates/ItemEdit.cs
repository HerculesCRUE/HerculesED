using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API.Templates
{  
    //Contiene las clases mecesarias para la configuraci�n de la edici�n de un item de un listado de una pesta�a


    /// <summary>
    /// Datos de configuraci�n de edici�n para una entidad
    /// </summary>
    public class ItemEdit
    {
        /// <summary>
        /// Secciones
        /// </summary>
        public List<ItemEditSection> sections;        
        /// <summary>
        /// Grafo en el que est� la entidad
        /// </summary>
        public string graph;        
        /// <summary>
        /// Propiedad utilizada para el t�tulo del recurso
        /// </summary>
        public string proptitle;
        /// <summary>
        /// Propiedad utilizada para la descripci�n del recurso
        /// </summary>
        public string propdescription;
        /// <summary>
        /// Propiedad utilizada para explicar la obligatori
        /// </summary>
        public propAuthor propAuthor;
        /// <summary>
        /// rdf:type de la entidad
        /// </summary>
        public string rdftype;
        /// <summary>
        /// Propiedades a cargar
        /// </summary>
        public List<LoadPropertyValues> loadPropertyValues;
        /// <summary>
        /// Genera el PropertyData para recuperar los datos
        /// </summary>
        /// <param name="pGraph">Grafo</param>
        /// <param name="pCV">Indica si la propiedad se busca en realidad en el cv</param>
        /// <returns></returns>
        public List<Utils.PropertyData> GenerarPropertyDatas(string pGraph, bool pCV = false)
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
                            List<Utils.PropertyData> aux = itemEditSectionRow.GenerarPropertyDatas(pGraph,pCV);
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
    /// Propiedad para indicar la obligatoriedad de a�adirse a s� mismo
    /// </summary>
    public class propAuthor
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Mensaje de error
        /// </summary>
        public Dictionary<string, string> message { get; set; }
    }

    /// <summary>
    /// Propiedades a cargar
    /// </summary>
    public class LoadPropertyValues
    {
        public string property { get; set; }
        public List<string> values { get; set; }
    }

    /// <summary>
    /// Datos de presentaci�n de una secci�n de una minificha para la edici�n
    /// </summary>
    public class ItemEditSection
    {
        /// <summary>
        /// T�tulo de la secci�n
        /// </summary>
        public Dictionary<string, string> title;
        /// <summary>
        /// Filas de edici�n de la secci�n
        /// </summary>
        public List<ItemEditSectionRow> rows;
    }

    /// <summary>
    /// Fila de edici�n de la secci�n
    /// </summary>
    public class ItemEditSectionRow
    {
        /// <summary>
        /// Propiedad de edici�n dentro de la secci�n
        /// </summary>
        public List<ItemEditSectionRowProperty> properties;
        /// <summary>
        /// Genera el PropertyData para recuperar los datos
        /// </summary>
        /// <param name="pGraph">Grafo</param>
        /// <param name="pCV">Indica si la propiedad se busca en realidad en el cv</param>
        /// <returns></returns>
        public List<Utils.PropertyData> GenerarPropertyDatas(string pGraph, bool pCV = false)
        {
            List<Utils.PropertyData> propertyDatas = new List<Utils.PropertyData>();
            foreach (ItemEditSectionRowProperty itemEditSectionRowProperty in this.properties)
            {
                if ((!pCV && !itemEditSectionRowProperty.entity_cv) || (pCV && itemEditSectionRowProperty.entity_cv))
                {
                    if (itemEditSectionRowProperty.autocompleteConfig != null && !string.IsNullOrEmpty(itemEditSectionRowProperty.autocompleteConfig.propertyEntity))
                    {
                        if (!string.IsNullOrEmpty(itemEditSectionRowProperty.autocompleteConfig.propertyEntity))
                        {
                            Utils.PropertyData propertyAC = new Utils.PropertyData()
                            {
                                property = itemEditSectionRowProperty.autocompleteConfig.propertyEntity,
                                childs = new List<Utils.PropertyData>(),
                                graph = pGraph
                            };
                            if (!propertyDatas.Exists(x => x.property == itemEditSectionRowProperty.property))
                            {
                                propertyDatas.Add(propertyAC);
                            }
                        }
                    }
                    Utils.PropertyData property = new Utils.PropertyData()
                    {
                        property = itemEditSectionRowProperty.property,
                        childs = new List<Utils.PropertyData>(),
                        graph = pGraph
                    };
                    if (itemEditSectionRowProperty.type == DataTypeEdit.entityautocomplete)
                    {
                        Utils.PropertyData propertyAC = new Utils.PropertyData()
                        {
                            property = itemEditSectionRowProperty.autocompleteConfig.property.property,
                            childs = new List<Utils.PropertyData>()
                        };
                        property.graph = itemEditSectionRowProperty.autocompleteConfig.graph;
                        if (!property.childs.Exists(x => x.property == itemEditSectionRowProperty.property))
                        {
                            property.childs.Add(propertyAC);
                        }
                    }
                    if (!propertyDatas.Exists(x => x.property == itemEditSectionRowProperty.property))
                    {
                        propertyDatas.Add(property);
                    }
                    else
                    {
                        property = propertyDatas.First(x => x.property == property.property);
                    }
                    if (itemEditSectionRowProperty.type == DataTypeEdit.auxEntityAuthorList)
                    {
                        property.childs.AddRange(GetPropertyDataAuthorList());
                    }
                    if (itemEditSectionRowProperty.type == DataTypeEdit.thesaurus)
                    {
                        property.childs.AddRange(new List<Utils.PropertyData>() { new Utils.PropertyData { property = "http://w3id.org/roh/categoryNode" } });
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
            }
            return propertyDatas;
        }

        /// <summary>
        /// Genera los PropertyData para obtener los datos de autores
        /// </summary>
        /// <returns></returns>
        private List<Utils.PropertyData> GetPropertyDataAuthorList()
        {
            List<Utils.PropertyData> listado = new List<Utils.PropertyData>() {
                new Utils.PropertyData() {
                    property= "http://xmlns.com/foaf/0.1/nick"
                },
                new Utils.PropertyData() {
                    property = "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment"
                },
                new Utils.PropertyData() {
                    property = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member",
                    graph= "person",
                    childs=new List<Utils.PropertyData>() {
                        new Utils.PropertyData(){
                            property="http://xmlns.com/foaf/0.1/name"
                        },
                        new Utils.PropertyData(){
                            property="http://w3id.org/roh/ORCID"
                        }
                    }
                }
            };
            return listado;
        }
    }

    /// <summary>
    /// Propiedad de edici�n dentro de la fila de una secci�n de la secci�n
    /// </summary>
    public class ItemEditSectionRowProperty
    {
        /// <summary>
        /// Propiedad de edici�n dentro de la fila de la secci�n
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
        /// Indica si la propiedad tiene autocompletar
        /// </summary>
        public bool autocomplete;
        /// <summary>
        /// Configuracion del autocompletar
        /// </summary>
        public ItemEditSectionRowPropertyAutocompleteConfig autocompleteConfig { get; set; }
        /// <summary>
        /// Indica si es obligatorio
        /// </summary>
        public bool required;
        /// <summary>
        /// Indica si es editable aunque la entidad est� bloqueada
        /// </summary>
        public bool editable;
        /// <summary>
        /// Indica si es multiidioma
        /// </summary>
        public bool multilang; 
        /// <summary>
        /// Datos para el combo
        /// </summary>
        public ItemEditSectionRowPropertyCombo combo;
        /// <summary>
        /// Indica el source del tesauro de la propiedad
        /// </summary>
        public string thesaurus;
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
        /// Composici�n de la propiedad
        /// </summary>
        public string compossed;
        /// <summary>
        /// Dependencia (valor que tiene que tener una determinada propiedad para que el campo sea editable)
        /// </summary>
        public ItemEditSectionRowPropertyDependency dependency;
        /// <summary>
        /// Indica si la propiedad pertenece al CV y no a la entidad
        /// </summary>
        public bool entity_cv;
    }

    public class ItemEditSectionRowPropertyDependency
    {
        /// <summary>
        /// Propiedad de la que depende
        /// </summary>
        public string property;
        /// <summary>
        /// Valor de la propiedad para que se muestre
        /// </summary>
        public string propertyValue;
        /// <summary>
        /// Valor de la propiedad para que no se muestre
        /// </summary>
        public string propertyValueDistinct;
    }

    /// <summary>
    /// Configuraci�n de un autocompletar para edici�n
    /// </summary>
    public class ItemEditSectionRowPropertyAutocompleteConfig
    {
        /// <summary>
        /// Propiedad a recuperar de la entidad para el nombre
        /// </summary>
        public PropertyDataTemplate property;
        /// <summary>
        /// Grafo en el que est� la entidad
        /// </summary>
        public string graph;
        /// <summary>
        /// Indica si hay que cachear
        /// </summary>
        public bool cache;
        /// <summary>
        /// Propiedad en la que se carga el ID de la entidad
        /// </summary>
        public string propertyEntity;
        /// <summary>
        /// rdf:type de la entidad a recuperar
        /// </summary>
        public string rdftype;
        /// <summary>
        /// Indica si s�lo se pueden seleccionar opciones del autocompletar
        /// </summary>
        public bool mandatory;
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
        /// Propiedad representante para el t�tulo
        /// </summary>
        public PropertyDataTemplate propertyTitle;
        /// <summary>
        /// Propiedades representantes
        /// </summary>
        public List<ItemEditEntityProperty> properties;
        /// <summary>
        /// Filas con las propiedades de edici�n
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
        /// Propiedad representante para el t�tulo
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
    /// Configuraci�n de un combo para edici�n
    /// </summary>
    public class ItemEditSectionRowPropertyCombo
    {
        /// <summary>
        /// Propiedad a recuperar de la entidad para el nombre
        /// </summary>
        public PropertyDataTemplate property;
        /// <summary>
        /// Grafo en el que est� la entidad
        /// </summary>
        public string graph;
        /// <summary>
        /// rdf:type de la entidad a recuperar
        /// </summary>
        public string rdftype;
        /// <summary>
        /// Indica si hay que cachear el combo
        /// </summary>
        public bool cache;
        public ItemEditSectionRowPropertyComboFilter filter;
        public ItemEditSectionRowPropertyComboDependency dependency;
    }


    public class ItemEditSectionRowPropertyComboFilter
    {
        /// <summary>
        /// Propiedad por la que se va a filtrar
        /// </summary>
        public string property;
        /// <summary>
        /// Valor por el que se va a filtrar
        /// </summary>
        public string value;
    }


    public class ItemEditSectionRowPropertyComboDependency
    {
        /// <summary>
        /// Propiedad de la que depende
        /// </summary>
        public string property;
        /// <summary>
        /// Propiedad de la que va a obtener el valor
        /// </summary>
        public string propertyValue;
    }
}


