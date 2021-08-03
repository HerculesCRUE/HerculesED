using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace EditorCV.Models
{

    // public class JsonStructure
    // {
    //     public List<Entity> entities { get; set; }
    //     public List<Property> properties { get; set; }
    // }

    // public class Entity
    // {
    //     public string rdgtype { get; set; }
    //     public string ontologiaName { get; set; }
    //     public string name { get; set; }
    //     public int[] nitems { get; set; }
    //     public string[] relation { get; set; }
    //     public List<EntityItem> items { get; set; }
    // }

    // public class EntityItem
    // {
    //     public string id { get; set; }
    //     public List<Entity> entities { get; set; }
    //     public List<Property> properties { get; set; }
    // }


    // public class Section
    // {
    //     public string title { get; set; }
    //     public List<string> @class { get; set; }
    //     public string id { get; set; }
    //     public string type { get; set; }
    //     public List<Property> properties { get; set; }
    // }

    // public class Property
    // {
    //     public string uri { get; set; }
    //     public string id { get; set; }
    //     public string fid { get; set; }
    //     public int[] nitems { get; set; }
    //     public dynamic defaultValue { get; set; }
    //     public dynamic value { get; set; }
    //     public string name { get; set; }
    //     public string type { get; set; }
    //     public bool show { get; set; }
    //     public bool required { get; set; }
    //     public bool multiLanguage { get; set; }
    //     public bool multiple { get; set; }
    //     public List<dynamic> options { get; set; }
    // }
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
    }


    public class View
    {
        public string[] properties { get; set; }
    }


    public class Entity
    {
        public string rdfType { get; set; }
        public string ontologyName { get; set; }
        public string name { get; set; }
        // public List<EntityItem> items { get; set; }
        public List<Section> sections { get; set; }
        public Dictionary<string, EntityItem> items { get; set; }
        public View view { get; set; }
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
        public string id { get; set; }
        public string type { get; set; }
        public List<Section> sections { get; set; }
        public List<Property> properties { get; set; }
    }


    public class Property
    {
        public string property { get; set; }
        public string entityReference { get; set; }
        public string id { get; set; }
        public string rdfType { get; set; }
        public string orderRdf { get; set; }
        public string itemRdf { get; set; }
        public string label { get; set; }
        public string title { get; set; }
        public int? min { get; set; }
        public int? max { get; set; }
        public dynamic defaultValue { get; set; }
        public dynamic value { get; set; }
        public string name { get; set; }
        public string fieldType { get; set; }
        public dynamic show { get; set; }
        public bool couldBeDisabled { get; set; }
        public bool multiLanguage { get; set; }
        public bool multiple { get; set; }
        public List<dynamic> options { get; set; }
        public List<ListItems> listItems { get; set; }
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
        public int? order { get; set; }
        public dynamic value { get; set; }
    }


}
