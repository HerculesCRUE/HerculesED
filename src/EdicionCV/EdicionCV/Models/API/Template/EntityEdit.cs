using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API.Template
{
    /// <summary>
    /// Entidad para edición
    /// </summary>
    public class EntityEdit
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
        /// Título/nopmbre
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Placeholder para la edición
        /// </summary>
        public string placeholder { get; set; }
        /// <summary>
        /// Tipo, de la enumeración 'DataTypeEdit'
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





        public AuxEntityDataResponse entityAuxData { get; set; }
        public EntityDataResponse entityData { get; set; }
    }

    public class AuxEntityDataResponse
    {
        public string propertyOrder { get; set; }
        public string rdftype { get; set; }
        public List<EntityEditSectionRow> entityRows { get; set; }
        public Dictionary<string, List<EntityEditSectionRow>> entities { get; set; }
        public Dictionary<string, int> childsOrder { get; set; }
        public Dictionary<string, EntityAuxProperty> title { get; set; }
        public EntityAuxProperty titleConfig { get; set; }
        public Dictionary<string, List<EntityAuxProperty>> properties { get; set; }
        public List<EntityAuxProperty> propertiesConfig { get; set; }
    }

    public class EntityDataResponse
    {
        public string rdftype { get; set; }
        public Dictionary<string, EntityAuxProperty> title { get; set; }
        public EntityAuxProperty titleConfig { get; set; }
        public Dictionary<string, List<EntityAuxProperty>> properties { get; set; }
        public List<EntityAuxProperty> propertiesConfig { get; set; }
    }

    public class EntityAuxProperty
    {
        public string route { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }

}


