using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API.Template
{
    /// <summary>
    /// Modelo de una pestaña del CV
    /// </summary>
    public class Tab
    {
        /// <summary>
        /// Secciones de la pestaña del CV
        /// </summary>
        public List<TabSection> sections {get;set;}
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
        /// Órdenes del listado de la sección
        /// </summary>
        public List<PresentationOrderTabSection> orders { get; set; }
        /// <summary>
        /// Items del listado de la sección
        /// </summary>
        public Dictionary<string,ItemTabSection> items { get; set; }
    }

    /// <summary>
    /// Orden del listado
    /// </summary>
    public class PresentationOrderTabSection
    {
        /// <summary>
        /// Nombre
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Propiedades
        /// </summary>
        public List<PresentationOrderTabSectionProperty> properties { get; set; }
    }

    /// <summary>
    /// Propiedades para el orden de la sección
    /// </summary>
    public class PresentationOrderTabSectionProperty
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
    public class ItemTabSection
    {
        /// <summary>
        /// Título
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Indica si es público o no (o no tiene privacidad)
        /// </summary>
        public string ispublic { get; set; }
        /// <summary>
        /// Propiedades del item
        /// </summary>
        public List<ItemTabSectionProperty> properties { get; set; }
        /// <summary>
        /// Propiedades del item para su ordenación
        /// </summary>
        public List<ItemTabSectionOrderProperty> orderProperties { get; set; }
    }

    /// <summary>
    /// Propiedad de un item del listado
    /// </summary>
    public class ItemTabSectionProperty
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
    /// Propiedad de ordenación de un item del listado
    /// </summary>
    public class ItemTabSectionOrderProperty
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


