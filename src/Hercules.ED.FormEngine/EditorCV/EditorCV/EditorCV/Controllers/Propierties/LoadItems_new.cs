using EditorCV.Models;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace EditorCV.Controllers.Propierties
{
    public class LoadItems2
    {
        // private List<Entity> entities;
        private JsonStructure jsonData;
        
        private ResourceApi resourceApi;
        private string id;

        public LoadItems2(string id)
        {
            
            this.id = id;

            this.DeserialiceTemplate();

            string config = "Config/ConfigOauth/OAuthV3.config";
            string basePath = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
            // basePath = AppDomain.CurrentDomain.BaseDirectory;
            // string wanted_path = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            string jsonPath = Path.Combine(basePath, config);
            resourceApi = new ResourceApi(jsonPath, null, new LogHelperFile("c:\\logs\\", "logCV"));
        }


        public List<List<Dictionary<string, Data>>> CargarEntidadOld(string pId, string pGraph)
        {
            SparqlObject result = new SparqlObject();
            List<List<Dictionary<string, Data>>> listResult = new List<List<Dictionary<string, Data>>>();
            
            try
            {
                
                if (resourceApi.ApiUrl.GetType() != typeof(Exception))
                {
                    result = resourceApi.VirtuosoQuery("select distinct ?entity", $"where{{?s <http://gnoss/hasEntidad> <{pId}>.?s <http://gnoss/hasEntidad> ?entity}}", pGraph);
                }


                foreach (Dictionary<string, Data> fila in result.results.bindings)
                {
                    SparqlObject result2 = resourceApi.VirtuosoQuery("select distinct ?p ?o", $"where{{<{fila["entity"].value}> ?p ?o}}", pGraph);
                    listResult.Add(result2.results.bindings);
                }

            }
            catch (System.Exception)
            {
                throw;
            }

            return listResult;
        }


        public Dictionary<string, List<Dictionary<string, Data>>> GetValEntity(string pId, string rdf, string pGraph)
        {
            SparqlObject result = new SparqlObject();
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            
            try
            {
                
                if (resourceApi.ApiUrl.GetType() != typeof(Exception))
                {
                    result = resourceApi.VirtuosoQuery("select distinct ?entity", $"where{{?s <http://gnoss/hasEntidad> <{pId}>.?s <http://gnoss/hasEntidad> ?entity}}", pGraph);
                }


                foreach (Dictionary<string, Data> fila in result.results.bindings)
                {
                    SparqlObject result2 = resourceApi.VirtuosoQuery("select distinct ?p ?o", $"where{{<{fila["entity"].value}> ?p ?o}}", pGraph);
                    listResult.Add(fila["entity"].value, result2.results.bindings);
                }

            }
            catch (System.Exception)
            {
                throw;
            }

            return listResult;
        }


        public JsonStructure GetMainEntity()
        {
            SparqlObject result = new SparqlObject();

            if (jsonData.entities != null)
            {
                // Get the main Document index
                // var mainKeyEntity = jsonData.entities.FindIndex(e => e.rdftype == "http://purl.org/roh/mirror/bibo#Document");
                var mainKeyEntity = "http://purl.org/roh/mirror/bibo#Document";
                

                if (jsonData.entities[mainKeyEntity] != null)
                {
                    
                    jsonData.entities[mainKeyEntity].items = new Dictionary<string, EntityItem>();

                    string sectionsJson = JsonConvert.SerializeObject(jsonData.entities[mainKeyEntity].sections);
                    
                    jsonData.entities[mainKeyEntity].items.Add(id, new EntityItem() {
                        id = id,
                        sections = JsonConvert.DeserializeObject<List<Section>>(sectionsJson)
                    });

                    var dbEntityData = GetValEntity(id, jsonData.entities[mainKeyEntity].rdfType, jsonData.entities[mainKeyEntity].ontologyName);


                    jsonData.entities[mainKeyEntity].items[id].sections = GetSections(id, JsonConvert.DeserializeObject<List<Section>>(sectionsJson), jsonData.entities[mainKeyEntity], dbEntityData);
                }
                
            }


            return jsonData;
        }


        /// <summary>
        /// Go through the all sections to find the properties
        /// </summary>
        /// <param name="sections">Current section</param>
        /// <param name="entity">The current entity</param>
        /// <returns></returns>
        public List<Section> GetSections(string id, List<Section> sections, Entity entity, Dictionary<string, List<Dictionary<string, Data>>> dbEntityData) 
        {
            for (int i = 0; i < sections.Count(); i++)
            {
                // var dbEntityData = GetValEntity(id, jsonData.entities[i].rdftype, jsonData.entities[i].ontologiaName);

                if (sections[i].properties != null)
                {
                    sections[i].properties = GetFields(dbEntityData, sections[i].properties);
                }
                
                if (sections[i].sections != null)
                {
                    // sections[i].sections.ForEach(e => GetSections(id, e, entity));
                    
                    sections[i].sections = GetSections(id, sections[i].sections, entity, dbEntityData);
                }
                
            }

            return sections;
        }

        protected List<Property> GetFields(Dictionary<string, List<Dictionary<string, Data>>> dbEntityData, List<Property> properties)
        {

            var dbText = JsonConvert.SerializeObject(dbEntityData.ToArray());


            // 
            foreach (KeyValuePair<string, List<Dictionary<string, Data>>> item in dbEntityData)
            {

                foreach (Dictionary<string, Data> el in item.Value)
                {

                    for (int i = 0; i < properties.Count(); i++)
                    {
                        string uri = properties[i].property;

                        // Check if the property is parameter is empty in the current property, and if it's exist in jsonData properies section
                        if (properties[i].property == null)
                        {
                            jsonData.properties.ForEach(e => uri = (e.id == properties[i].id) ? e.property : null);
                        }

                        try
                        {

                            var val = el["p"].value;

                            if (el["p"].type == "uri" && el["p"].value == uri)
                            {
                                properties[i].value = el["o"].value;

                                // 
                                if (properties[i].entityReference != null && properties[i].entityReference != "")
                                {

                                    string listItemUri = loadList(item.Key, properties[i], el["o"].value, dbEntityData);
                                    
                                    properties[i].id = el["o"].value;
                                    properties[i].options = loadEntity(listItemUri, properties[i]);
                                }

                                // Get order
                                if (properties[i].orderRdf != null && properties[i].orderRdf != "")
                                {
                                    var orderIndex = item.Value.FindIndex(e => el["p"].value == properties[i].orderRdf);
                                    
                                }
                            }



                        }
                        catch (System.Exception) {}
                    }

                }
            }

            return properties;
        }

        protected string loadList(string entityId, Property property, string listItemId, Dictionary<string, List<Dictionary<string, Data>>> dbEntityData)
        {

            var listItemUri = "";

            try
            {
                // Get the current entityRdfType
                var dbEntityDataItem = dbEntityData[entityId].Find(e => e["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
                var entityRdf = dbEntityDataItem["o"].value;

                // Check if exist a list of items
                if (jsonData.entities[entityRdf] != null && property.listItems != null)
                {
                    // Find the list of items of the entity reference given
                    
                    if (property.listItems == null)
                    {
                        property.listItems = new List<ListItems>();

                    }
                    // if (!property.listItems.ContainsKey(entityId))
                    // {
                    //     property.listItems.Add(entityId, new List<ListItems>());
                    // }
                    var lItemNew = new ListItems()
                    {
                        id = listItemId,
                    };

                    foreach (var item in dbEntityData[listItemId])
                    {
                        if (item["p"].type == "uri" && item["p"].value == property.orderRdf)
                        {
                            lItemNew.order = int.Parse(item["o"].value);
                        }

                        if (item["p"].type == "uri" && item["p"].value == property.itemRdf)
                        {
                            lItemNew.value = item["o"].value;
                            listItemUri = item["o"].value;
                        }
                    }

                    property.listItems.Add(lItemNew);

                }

            }
            catch (System.Exception) {
                throw;
            }

            return listItemUri;

        }


        protected List<dynamic> loadEntity(string entityUri, Property property)
        {

            // Item to return
            List<dynamic> options = new List<dynamic>();

            // var keyEntity = jsonData.entities.FindIndex(e => e.rdfType == property.rdfType);
            var keyEntity = property.rdfType;


            // Load the entity
            var dbEntityData = GetValEntity(entityUri, property.rdfType, jsonData.entities[keyEntity].ontologyName);

            
            try
            {

                // Check if exist the entity
                if (jsonData.entities.ContainsKey(keyEntity))
                {
                    
                    // Start the items dictionary if is undefined
                    if (jsonData.entities[keyEntity].items == null)
                    {
                        jsonData.entities[keyEntity].items = new Dictionary<string, EntityItem>();
                    }

                    // Check if exist and the item, if exist, don't load the item again
                    if (!jsonData.entities[keyEntity].items.ContainsKey(entityUri))
                    {
                        jsonData.entities[keyEntity].items.Add(entityUri, new EntityItem());

                        // Serialize to remove the reference in the loop
                        string sectionsJson = JsonConvert.SerializeObject(jsonData.entities[keyEntity].sections);

                        // Set the id & the sections
                        jsonData.entities[keyEntity].items[entityUri].id = entityUri;
                        jsonData.entities[keyEntity].items[entityUri].sections = (jsonData.entities[keyEntity].sections != null) ? GetSections(entityUri, JsonConvert.DeserializeObject<List<Section>>(sectionsJson), jsonData.entities[keyEntity], dbEntityData) : null;
                        
                    }

                }

            }
            catch (System.Exception e) {
                var sdad = e.ToString();
                throw;
            }


            return options;
        }


        // protected List<dynamic> loadEntity(Property property)
        // {

        //     List<dynamic> options = new List<dynamic>();

        //     // var keyEntity = jsonData.entities.FindIndex(e => e.rdftype == property.rdftype);
        //     var keyEntity = property.rdfType;

        //     // for (int i = 0; i < jsonData.entities[keyEntity].sections.Count(); i++)
        //     // {
        //     //     if (jsonData.entities[keyEntity] != null)
        //     //     {
        //     //         jsonData.entities[keyEntity].sections[i] = GetSections(property.id, jsonData.entities[keyEntity].sections[i], jsonData.entities[keyEntity]);
        //     //     }
        //     // }


        //     // var dbEntityData = GetValEntity(property.id, jsonData.entities[keyEntity].rdftype, jsonData.entities[keyEntity].ontologyName);
        //     var dbEntityData = GetValEntity(property.id, property.property, jsonData.entities[keyEntity].ontologyName);

        //     // List<List<Dictionary<string, Data>>> newDbEntityData;

            
        //     try
        //     {

        //         List<List<Property>> propiedades = new List<List<Property>>();
        //         if (jsonData.entities[keyEntity].items == null)
        //         {
        //             jsonData.entities[keyEntity].items = new Dictionary<string, EntityItem>();
        //         }

        //         // Not get the first element
        //         foreach (var itemKeyPar in dbEntityData)
        //         {
        //             foreach (var el in itemKeyPar.Value)
        //             {

        //                 if (el != null)
        //                 {    

        //                     var tfs = el["p"].value;
        //                     if (el["p"].type == "uri" && el["p"].value == property.rdfType)
        //                     {
                                

        //                         // Check if exist
        //                         // var keyItemFound = jsonData.entities[keyEntity].items.FindIndex(e => e.id == el["o"].value);
        //                         var keyItemFound = jsonData.entities[keyEntity].items[el["o"].value];
        //                         var dfsdfsd = el["o"].value;


        //                         if (jsonData.entities[keyEntity].items[el["o"].value] != null)
        //                         {
        //                             // Load new entity data value
        //                             jsonData.entities[keyEntity].items.Add(el["o"].value, new EntityItem {
        //                                 id = el["o"].value,
        //                                 sections = (jsonData.entities[keyEntity].sections != null) ? GetSections(el["o"].value, jsonData.entities[keyEntity].sections, jsonData.entities[keyEntity], dbEntityData) : null,
        //                                 // entities = (newDbEntityData != null && jsonData.entities[keyEntity].entities != null) ? GetEntities(el["o"].value, newDbEntityData, entityItem.entities) : null,
        //                             });
        //                         }

        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     catch (System.Exception) {
        //         throw;
        //     }


        //     return options;
        // }

        public void DeserialiceTemplate()
        {
            // var jsonFile = "Config/Template.json";
            // var jsonFile = "Config/Plantilla.json";
            jsonData = new JsonStructure();
            jsonData.entities = new Dictionary<string, Entity>();
            jsonData.startEntity = "http://purl.org/roh/mirror/bibo#Document";
            jsonData.startItem = "http://gnoss.com/items/Document_6c5ad967-1775-4960-896b-932a2e407d97_dfd9016f-a9b5-4023-9b22-85c760f187d1";

            string[] jsonFiles = {"Config/PDocumento.json", "Config/PPersona.json"};

            foreach (var json in jsonFiles)
            {
                
                string jsonPath = Path.Combine(AppContext.BaseDirectory, json);

                try
                {
                    // Open the json
                    StreamReader file = File.OpenText(jsonPath);
                    // Read the file
                    string text = file.ReadToEnd();
                    // Deserialize the json
                    var newEntity = JsonConvert.DeserializeObject<Entity>(text);
                    jsonData.entities.Add(newEntity.rdfType, JsonConvert.DeserializeObject<Entity>(text));
                } 
                catch (System.Exception)
                {
                    throw new Exception("Error during read and deserialize the json file config");
                }
            }

        }



    }
}
