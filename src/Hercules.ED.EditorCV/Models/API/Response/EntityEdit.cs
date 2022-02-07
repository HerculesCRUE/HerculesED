using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API.Response
{
    //Contiene la/las clases necesarias que tiene como salida el servicio para la carga/edcii�n de una entidad

    /// <summary>
    /// Entidad para edici�n
    /// </summary>
    public class EntityEdit
    {
        /// <summary>
        /// Identificador de la entidad
        /// </summary>
        public string entityID { get; set; }
        /// <summary>
        /// Ontolog�a
        /// </summary>
        public string ontology { get; set; }
        /// <summary>
        /// rdf:type
        /// </summary>
        public string rdftype { get; set; }
        /// <summary>
        /// Secciones de edici�n
        /// </summary>
        public List<EntityEditSection> sections { get; set; }
        /// <summary>
        /// Especifica si la entiadad es editable
        /// </summary>
        public bool iseditable { get; set; }
    }

    /// <summary>
    /// Secci�n de edici�n
    /// </summary>
    public class EntityEditSection
    {
        /// <summary>
        /// T�tulo
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Filas de edici�n
        /// </summary>
        public List<EntityEditSectionRow> rows { get; set; }
    }

    /// <summary>
    /// Fila con propiedades de edici�n
    /// </summary>
    public class EntityEditSectionRow
    {
        /// <summary>
        /// Propiedades de edici�n
        /// </summary>
        public List<EntityEditSectionRowProperty> properties { get; set; }
    }

    /// <summary>
    /// Propiedad de edici�n
    /// </summary>
    public class EntityEditSectionRowProperty
    {
        /// <summary>
        /// Url de la propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Url de la propiedad (en caso de que se trate de un autocompletar con la entidad)
        /// </summary>
        public string propertyEntity { get; set; }
        /// <summary>
        /// Valor de la entidad (en caso de que se trate de un autocompletar con la entidad)
        /// </summary>
        public string propertyEntityValue { get; set; }
        /// <summary>
        /// T�tulo/nopmbre
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Placeholder para la edici�n
        /// </summary>
        public string placeholder { get; set; }
        /// <summary>
        /// Tipo, de la enumeraci�n 'DataTypeEdit'
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Anchura del campo (1, 2 o 3)
        /// </summary>
        public int width { get; set; }
        /// <summary>
        /// Indica si es multiple
        /// </summary>
        public bool multiple { get; set; }
        /// <summary>
        /// Indica si tiene autocompletar
        /// </summary>
        public bool autocomplete { get; set; }
        /// <summary>
        /// indica si es obligatorio
        /// </summary>
        public bool required { get; set; }
        /// <summary>
        /// Lista de valores
        /// </summary>
        public List<string> values { get; set; }
        /// <summary>
        /// Lista de valores disponibles para el combo
        /// </summary>
        public Dictionary<string, string> comboValues { get; set; }
        /// <summary>
        /// Lista de valores disponibles para el combo
        /// </summary>
        public ComboDependency comboDependency { get; set; }
        /// <summary>
        /// Lista de valores del tesauro
        /// </summary>
        public List<ThesaurusItem> thesaurus { get; set; }
        /// <summary>
        /// Datos de una entidad auxiliar dentro de una entidad
        /// </summary>
        public EntityEditAuxEntity entityAuxData { get; set; }
        /// <summary>
        /// Datos de una entidad principal dentro de una entidad
        /// </summary>
        public EntityEditEntity entityData { get; set; }
        /// <summary>
        /// Lista de valores disponibles para el combo
        /// </summary>
        public Dependency dependency { get; set; }
        /// <summary>
        /// Configuracion del autocompletar
        /// </summary>
        public AutocompleteConfig autocompleteConfig { get; set; }
        /// <summary>
        /// Indica si la propiedad pertenece al CV y no a la entidad
        /// </summary>
        public bool entity_cv { get; set; }
    }

    //TODO comentarios
    public class AutocompleteConfig
    {
        public string property { get; set; }
        public string rdftype { get; set; }
        public string graph { get; set; }
        public bool getEntityId { get; set; }
        /// <summary>
        /// Indica si s�lo se pueden seleccionar opciones del autocompletar
        /// </summary>
        public bool mandatory;
    }


    public class Dependency
    {
        public string parent { get; set; }
        public string parentDependencyValue { get; set; }
        public string parentDependencyValueDistinct { get; set; }
    }

    public class ComboDependency
    {
        public string parent { get; set; }
        public Dictionary<string, string> parentDependency { get; set; }
    }

    /// <summary>
    /// Datos de entidad auxiliar
    /// </summary>
    public class EntityEditAuxEntity
    {
        /// <summary>
        /// Propiedad de orden (en caso de que exista)
        /// </summary>
        public string propertyOrder { get; set; }
        /// <summary>
        /// rdf:type de la entiad auxiliar
        /// </summary>
        public string rdftype { get; set; }
        /// <summary>
        /// Filas de edici�n (configuraci�n)
        /// </summary>
        public List<EntityEditSectionRow> rows { get; set; }
        /// <summary>
        /// T�tulos de las entidad auxiliares (para mostar como representantes)
        /// </summary>
        public Dictionary<string, EntityEditRepresentativeProperty> titles { get; set; }
        /// <summary>
        /// Configuraci�n para los t�tulos de las entidad auxiliares (para mostar como representantes)
        /// </summary>
        public EntityEditRepresentativeProperty titleConfig { get; set; }
        /// <summary>
        /// Propiedades de las entidades auxiliares (para mostar como representantes)
        /// </summary>
        public Dictionary<string, List<EntityEditRepresentativeProperty>> properties { get; set; }
        /// <summary>
        /// Configuraci�n para las propiedades de las entidades auxiliares (para mostar como representantes)
        /// </summary>
        public List<EntityEditRepresentativeProperty> propertiesConfig { get; set; }
        /// <summary>
        /// Entidades auxiliares
        /// </summary>
        public Dictionary<string, List<EntityEditSectionRow>> entities { get; set; }
        /// <summary>
        /// Orden de las diferentes entidades auxiliares
        /// </summary>
        public Dictionary<string, int> childsOrder { get; set; }
    }

    /// <summary>
    /// Datos de entidad principal
    /// </summary>
    public class EntityEditEntity
    {
        /// <summary>
        /// rdf:type de la entiad
        /// </summary>
        public string rdftype { get; set; }
        /// <summary>
        /// T�tulos de las entidad (para mostar como representantes)
        /// </summary>
        public Dictionary<string, EntityEditRepresentativeProperty> titles { get; set; }
        /// <summary>
        /// Configuraci�n para los t�tulos de las entidades (para mostar como representantes)
        /// </summary>
        public EntityEditRepresentativeProperty titleConfig { get; set; }
        /// <summary>
        /// Propiedades de las entidades (para mostar como representantes)
        /// </summary>
        public Dictionary<string, List<EntityEditRepresentativeProperty>> properties { get; set; }
        /// <summary>
        /// Configuraci�n para las propiedades de las entidades (para mostar como representantes)
        /// </summary>
        public List<EntityEditRepresentativeProperty> propertiesConfig { get; set; }
    }

    /// <summary>
    /// Propiedad representante de una entidad
    /// </summary>
    public class EntityEditRepresentativeProperty
    {
        /// <summary>
        /// 'Ruta'de la propiedad
        /// </summary>
        public string route { get; set; }
        /// <summary>
        /// Nombre de la propiedad
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Valor de la propiedad
        /// </summary>
        public string value { get; set; }
    }


    public class ThesaurusItem
    {        
        /// <summary>
        /// Id del elemento
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Nombre del elemento
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Padre del elemento
        /// </summary>
        public string parentId { get; set; }

    }
}


