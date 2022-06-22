using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EditorCV.Models.API.Response
{
    //Contiene la/las clases necesarias que tiene como salida el servicio para la presentaci�n de una pesta�a

    /// <summary>
    /// Modelo de una pesta�a del CV
    /// </summary>
    public class Tab:AuxTab
    {
        /// <summary>
        /// Secciones de la pesta�a del CV
        /// </summary>
        public List<TabSection> sections { get; set; }
    }

    /// <summary>
    /// Secci�n dentro de una pesta�a
    /// </summary>
    public class TabSection
    {
        /// <summary>
        /// T�tulo de la secci�n
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Identificador de la propiedad de la secci�n
        /// </summary>
        public string identifier { get; set; }
        /// <summary>
        /// �rdenes del listado de la secci�n
        /// </summary>
        public List<TabSectionPresentationOrder> orders { get; set; }
        /// <summary>
        /// Items del listado de la secci�n
        /// </summary>
        public Dictionary<string, TabSectionItem> items { get; set; }
        /// <summary>
        /// Item de la secci�n
        /// </summary>
        public EntityEdit item { get; set; }
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
    /// Propiedades para el orden de la secci�n
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
        /// T�tulo
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
        /// Indica si es p�blico o no 
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
        /// Indica si se ha de mostrar el bot�n de env�o a producci�n cientifica
        /// </summary>
        public bool sendPRC { get; set; }
        /// <summary>
        /// Indica si se ha de mostrar el bot�n de validaci�n
        /// </summary>
        public bool sendValidationProject { get; set; }
        /// <summary>
        /// Indica el estado de validaci�n
        /// </summary>
        public string validationStatus { get; set; }
        /// <summary>
        /// Propiedades del item
        /// </summary>
        public List<TabSectionItemProperty> properties { get; set; }
        /// <summary>
        /// Propiedades del item para su ordenaci�n
        /// </summary>
        public List<TabSectionItemOrderProperty> orderProperties { get; set; }
        /// <summary>
        /// Diccionario que indica para que idiomas est� completo el item
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
    /// Propiedad de ordenaci�n de un item del listado
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


