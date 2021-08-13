using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace EditorCV.Models
{

    public class DataJsonStructure
    {
        public JsonStructure data { get; set; }
    }


    public class JsonStructure
    {
        public Dictionary<string, Entity> entities { get; set; }
        public string startEntity { get; set; }
        public string startItem { get; set; }
        public List<Property> properties { get; set; }
        public Entity structure { get; set; }
        public Dictionary<string, Entity> sections { get; set; }
    }


    public class View
    {
        public string[] properties { get; set; }
    }


    public class Entity
    {
        // Rdf type of the entity
        public string rdfType { get; set; }
        public string ontologyName { get; set; }
        // Property of the entity with the relation
        public EntityRelation relation { get; set; }
        public bool loadedAll { get; set; }
        public bool uniqueLoaded { get; set; }
        // Unique identificator of the entity
        public string id { get; set; }
        public string name { get; set; }
        // You only can choose between design or make the structure
        public string design { get; set; }
        // List of the sections of the Entity (Only for the structure JSON)
        public List<Section> sections { get; set; }
        // List of entities related under hiself
        public List<Entity> entities { get; set; }
        // Properties of the entity
        public List<Property> properties { get; set; }
        // Custom designs for standarstructures
        public Dictionary<string, Section> designs { get; set; }
        // Each entity, only for the Entity model
        public Dictionary<string, EntityItem> items { get; set; }
        // List items for the structure section, parentId, items child
        public Dictionary<string, List<ListItems>> listItems { get; set; }
        public View view { get; set; }
    }


    public class EntityRelation
    {
        // Rdf type of the entity
        public string rdfType { get; set; }
        public string property { get; set; }
        public string entityReference { get; set; }
        public string orderRdf { get; set; }
        public string itemRdf { get; set; }
        public string eidRel { get; set; }
        public string eid { get; set; }
    }


    public class EntityItem
    {
        public string id { get; set; }
        public List<Section> sections { get; set; }
        public List<Property> properties { get; set; }
    }


    public class Section
    {
        public string title { get; set; }
        public List<string> @class { get; set; }
        public string tag { get; set; }
        public OutSection outSection { get; set; }
        public string load { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string fileName { get; set; }
        public List<Section> sections { get; set; }
        public List<Property> properties { get; set; }
        public List<Entity> entities { get; set; }
    }

    public class OutSection
    {
        public string section { get; set; }
        public string currentId { get; set; }
        public bool defaultLoaded { get; set; }
        // Load once like ti
        public bool loadOnce { get; set; }
    }


    public class Property
    {
        // Property of the entity with the re
        public string property { get; set; }
        public string id { get; set; } // "all" for all items
        public Dictionary<string, string> filters { get; set; }
        public EntityRelation relation { get; set; }
        public string label { get; set; }
        public string title { get; set; }
        public int? min { get; set; }
        public int? max { get; set; }
        public dynamic defaultValue { get; set; }
        public dynamic value { get; set; }
        public string name { get; set; }
        public string viewMode { get; set; }
        public string fieldType { get; set; }
        public dynamic show { get; set; }
        public bool couldBeDisabled { get; set; }
        public bool multiLanguage { get; set; }
        public bool multiple { get; set; }
        // List of "theId" => {id: "theId", rdfType: "value"}
        public Dictionary<string, Dictionary<string,string>> options { get; set; }
        public List<FieldsDB> fieldsDB { get; set; }
        public List<ListItems> listItems { get; set; }
    }


    public class FieldsDB
    {
        public string rdfType { get; set; }
        public string name { get; set; }
        public bool main { get; set; }
    }


    public class Actions
    {
        // Property of the target, like a order button
        public List<string> propertyTarget { get; set; }
        // asc or desc
        public string order { get; set; }
        // entity referenc of the action
        public string entityReference { get; set; }
        public string idReference { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public string icon { get; set; }
        // Could be "edit", "Add", "order", "search", "delete"
        public string type { get; set; }
        public dynamic show { get; set; }
        public bool couldBeDisabled { get; set; }
        public bool multiLanguage { get; set; }
        public List<Actions> menuActions { get; set; }
    }


    public class Order
    {
        public int? order { get; set; }
        public string id { get; set; }
    }


    public class ListItems
    {
        public string id { get; set; }
        public string parentId { get; set; }
        public int? order { get; set; }
    }


    public class ListItemsData
    {
        public string id { get; set; }
        public string parentId { get; set; }
        public Dictionary<string, List<Dictionary<string, Data>>> dbEntityData { get; set; }
    }
}
