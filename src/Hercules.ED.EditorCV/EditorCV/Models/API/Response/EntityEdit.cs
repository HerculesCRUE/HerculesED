using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EditorCV.Models.API.Response
{
    //Contiene la/las clases necesarias que tiene como salida el servicio para la carga/edciión de una entidad

    /// <summary>
    /// Entidad para edición
    /// </summary>
    public class EntityEdit:AuxTab
    {
        /// <summary>
        /// Identificador de la entidad
        /// </summary>
        public string entityID { get; set; }
        /// <summary>
        /// Ontología
        /// </summary>
        public string ontology { get; set; }
        /// <summary>
        /// rdf:type
        /// </summary>
        public string rdftype { get; set; }
        /// <summary>
        /// Secciones de edición
        /// </summary>
        public List<EntityEditSection> sections { get; set; }
        /// <summary>
        /// Especifica si la entiadad es editable
        /// </summary>
        public bool iseditable { get; set; }
    }

    /// <summary>
    /// Sección de edición
    /// </summary>
    public class EntityEditSection
    {
        /// <summary>
        /// Título
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Filas de edición
        /// </summary>
        public List<EntityEditSectionRow> rows { get; set; }
    }

    /// <summary>
    /// Fila con propiedades de edición
    /// </summary>
    public class EntityEditSectionRow
    {
        /// <summary>
        /// Propiedades de edición
        /// </summary>
        public List<EntityEditSectionRowProperty> properties { get; set; }
    }

    /// <summary>
    /// Propiedad de edición
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
        /// Grafo de la propiedad (en caso de que se trate de un autocompletar con la entidad)
        /// </summary>
        public string propertyEntityGraph { get; set; }
        /// <summary>
        /// Configuración de propiedades a recuperar de la entidad del autocompletar
        /// </summary>
        public List<SelectPropertyEntity> selectPropertyEntity { get; set; }
        /// <summary>
        /// Valor de la entidad (en caso de que se trate de un autocompletar con la entidad)
        /// </summary>
        public string propertyEntityValue { get; set; }
        /// <summary>
        /// Título/nopmbre
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Información de la propiedad
        /// </summary>
        public string information { get; set; }
        /// <summary>
        /// Placeholder para la edición
        /// </summary>
        public string placeholder { get; set; }
        /// <summary>
        /// Tipo, de la enumeración 'DataTypeEdit'
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Identificador del tesauro
        /// </summary>
        public string thesaurusID { get; set; }
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
        /// Indica si es editable aunque la entidad esté bloqueada
        /// </summary>
        public bool editable { get; set; }
        /// <summary>
        /// Indica si no es editable aunque la entidad no esté bloqueada
        /// </summary>
        public bool blocked { get; set; }
        /// <summary>
        /// Indica si es multiidioma
        /// </summary>
        public bool multilang { get; set; }
        /// <summary>
        /// Lista de valores
        /// </summary>
        public List<string> values { get; set; }
        /// <summary>
        /// Lista de valores nultiidioma
        /// </summary>
        public Dictionary<string, string> valuesmultilang { get; set; }
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

    public class SelectPropertyEntity
    {
        /// <summary>
        /// Propiedad de la entidad que se quiere recuperar
        /// </summary>
        public string propertyEntity { get; set; }
        /// <summary>
        /// Propiedad de la entidad del CV en la que se va a cargar la propiedad
        /// </summary>
        public string propertyCV { get; set; }
    }

    /// <summary>
    /// Clase para la configuración del autocompletar
    /// </summary>
    public class AutocompleteConfig
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Rdf:type de la entidad a buscar en el autocompletar
        /// </summary>
        public string rdftype { get; set; }
        /// <summary>
        /// Grafo
        /// </summary>
        public string graph { get; set; }
        /// <summary>
        /// Indica si hay que cachear el autocompletar
        /// </summary>
        public bool cache { get; set; }
        /// <summary>
        /// Indica si hay que obtener los IDs
        /// </summary>
        public bool getEntityId { get; set; }
        /// <summary>
        /// Indica si sólo se pueden seleccionar opciones del autocompletar
        /// </summary>
        public bool mandatory { get; set; }
        /// <summary>
        /// Propiedades auxiliares
        /// </summary>
        public List<string> propertiesAux { get; set; }
        /// <summary>
        /// Forma de pintado
        /// </summary>
        public string printAux { get; set; }
    }

    /// <summary>
    /// Indica si hay una dependencia
    /// </summary>
    public class Dependency
    {
        /// <summary>
        /// Propiedad de la que depende
        /// </summary>
        public string parent { get; set; }
        /// <summary>
        /// Valor que tiene que tner la propiedad dependiente para mostrar el campo
        /// </summary>
        public string parentDependencyValue { get; set; }
        /// <summary>
        /// Valor que NO tiene que tner la propiedad dependiente para mostrar el campo
        /// </summary>
        public string parentDependencyValueDistinct { get; set; }
    }

    /// <summary>
    /// Clase para la dependencia de combos (regiones)
    /// </summary>
    public class ComboDependency
    {
        /// <summary>
        /// Propiedad de la que depende
        /// </summary>
        public string parent { get; set; }
        /// <summary>
        /// Valores de dependencia
        /// </summary>
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
        /// Filas de edición (configuración)
        /// </summary>
        public List<EntityEditSectionRow> rows { get; set; }
        /// <summary>
        /// Títulos de las entidad auxiliares (para mostar como representantes)
        /// </summary>
        public Dictionary<string, EntityEditRepresentativeProperty> titles { get; set; }
        /// <summary>
        /// Configuración para los títulos de las entidad auxiliares (para mostar como representantes)
        /// </summary>
        public EntityEditRepresentativeProperty titleConfig { get; set; }
        /// <summary>
        /// Propiedades de las entidades auxiliares (para mostar como representantes)
        /// </summary>
        public Dictionary<string, List<EntityEditRepresentativeProperty>> properties { get; set; }
        /// <summary>
        /// Configuración para las propiedades de las entidades auxiliares (para mostar como representantes)
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
        /// Títulos de las entidad (para mostar como representantes)
        /// </summary>
        public Dictionary<string, EntityEditRepresentativeProperty> titles { get; set; }
        /// <summary>
        /// Configuración para los títulos de las entidades (para mostar como representantes)
        /// </summary>
        public EntityEditRepresentativeProperty titleConfig { get; set; }
        /// <summary>
        /// Propiedades de las entidades (para mostar como representantes)
        /// </summary>
        public Dictionary<string, List<EntityEditRepresentativeProperty>> properties { get; set; }
        /// <summary>
        /// Configuración para las propiedades de las entidades (para mostar como representantes)
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


