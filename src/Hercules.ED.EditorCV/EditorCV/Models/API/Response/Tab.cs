using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EditorCV.Models.API.Response
{
    //Contiene la/las clases necesarias que tiene como salida el servicio para la presentación de una pestaña

    /// <summary>
    /// Modelo de una pestaña del CV
    /// </summary>
    public class Tab:AuxTab
    {
        /// <summary>
        /// Secciones de la pestaña del CV
        /// </summary>
        public List<TabSection> sections { get; set; }
    }

    /// <summary>
    /// Sección dentro de una pestaña
    /// </summary>
    public class TabSection
    {
        /// <summary>
        /// Título de la sección
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Identificador de la propiedad de la sección
        /// </summary>
        public string identifier { get; set; }
        /// <summary>
        /// Es parte del CV abreviado del ISCIII
        /// </summary>
        public bool cvaISCIII { get; set; }
        /// <summary>
        /// Es parte del CV abreviado del AEI
        /// </summary>
        public bool cvaAEI { get; set; }
        /// <summary>
        /// Información de la sección
        /// </summary>
        public string information { get; set; }
        /// <summary>
        /// Últimos 5 años
        /// </summary>
        public TabSectionLast5Years last5Years { get; set; }
        /// <summary>
        /// Órdenes del listado de la sección
        /// </summary>
        public List<TabSectionPresentationOrder> orders { get; set; }
        /// <summary>
        /// Items del listado de la sección
        /// </summary>
        public Dictionary<string, TabSectionItem> items { get; set; }
        /// <summary>
        /// Item de la sección
        /// </summary>
        public EntityEdit item { get; set; }
    }

    /// <summary>
    /// Configuración para indicar si el recurso está dentro de los últimos cinco años.
    /// </summary>
    public class TabSectionLast5Years
    {
        /// <summary>
        /// Booleano indicando si debe añadirse siempre independientemente de las fechas de inicio y fin.
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
    /// Orden del listado
    /// </summary>
    public class TabSectionPresentationOrder
    {
        /// <summary>
        /// Nombre
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Propiedades
        /// </summary>
        public List<TabSectionPresentationOrderProperty> properties { get; set; }
    }

    /// <summary>
    /// Propiedades para el orden de la sección
    /// </summary>
    public class TabSectionPresentationOrderProperty
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Indica si es ascendente o descendente
        /// </summary>
        public bool asc { get; set; }
    }

    /// <summary>
    /// Item de un listado
    /// </summary>
    public class TabSectionItem
    {
        /// <summary>
        /// Título
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Identificador de la entidad
        /// </summary>
        public string identifier { get; set; }
        /// <summary>
        /// Identificador de la entidad en BBDD
        /// </summary>
        public string idBBDD { get; set; }
        /// <summary>
        /// Indica si es público o no 
        /// </summary>
        public bool ispublic { get; set; }
        /// <summary>
        /// Indica si es editable o no 
        /// </summary>
        public bool iseditable { get; set; }
        /// <summary>
        /// Indica si es openaccess
        /// </summary>
        public bool isopenaccess { get; set; }
        /// <summary>
        /// Indica si debe estar checkeado por defecto
        /// </summary>
        public bool isChecked { get; set; }
        public bool isBlockedFE { get; set; }
        /// <summary>
        /// Indica si se ha de mostrar el botón de envío a producción cientifica
        /// </summary>
        public bool sendPRC { get; set; }
        /// <summary>
        /// Indica si se ha de mostrar el botón de validación
        /// </summary>
        public bool sendValidationProject { get; set; }
        /// <summary>
        /// Indica el estado de validación
        /// </summary>
        public string validationStatus { get; set; }
        /// <summary>
        /// Propiedades del item
        /// </summary>
        public List<TabSectionItemProperty> properties { get; set; }
        /// <summary>
        /// Propiedades del item para su ordenación
        /// </summary>
        public List<TabSectionItemOrderProperty> orderProperties { get; set; }
        /// <summary>
        /// Diccionario que indica para que idiomas está completo el item
        /// </summary>
        public Dictionary<string,bool> multilang { get; set; }
    }

    /// <summary>
    /// Propiedad de un item del listado
    /// </summary>
    public class TabSectionItemProperty
    {
        /// <summary>
        /// Nombre
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Información del item
        /// </summary>
        public string information { get; set; }
        /// <summary>
        /// Indica si se muestra en la minificha (sin desplegar)
        /// </summary>
        public bool showMini { get; set; }
        /// <summary>
        /// Indica si se muestra en la minificha en negrita(sin desplegar)
        /// </summary>
        public bool showMiniBold { get; set; }
        /// <summary>
        /// Tipo del dato de la propiedad
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Valores de la propiedad
        /// </summary>
        public List<string> values { get; set; }
    }

    /// <summary>
    /// Propiedad de ordenación de un item del listado
    /// </summary>
    public class TabSectionItemOrderProperty
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Valores
        /// </summary>
        public List<string> values { get; set; }
    }

}


