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

namespace EditorCV.Controllers.Properties
{
    public class LoadItems
    {
        // private List<Entity> entities;
        private JsonStructure jsonData;
        
        private ResourceApi resourceApi;

        private Dictionary<string, List<ListItemsData>> loadedEntities;
        private string id;

        private int calls;

        public LoadItems(string id)
        {
            
            this.id = id;

            string config = "Config/ConfigOauth/OAuthV3.config";
            string basePath = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
            // basePath = AppDomain.CurrentDomain.BaseDirectory;
            // string wanted_path = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            string jsonPath = Path.Combine(basePath, config);
            resourceApi = new ResourceApi(jsonPath, null, new LogHelperFile("c:\\logs\\", "logCV"));

            // Inicialite loaded entities
            loadedEntities = new Dictionary<string, List<ListItemsData>>();


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
            calls ++;

            return listResult;
        }


        public JsonStructure GetMainEntity()
        {
            SparqlObject result = new SparqlObject();

            if (jsonData.entities != null && jsonData.structure != null)
            {
                // Get the main Document index
                // var mainKeyEntity = jsonData.entities.FindIndex(e => e.rdftype == "http://purl.org/roh/mirror/bibo#Document");
                var mainKeyEntity = jsonData.structure.rdfType;
                var ontologyName = jsonData.structure.ontologyName;
                var mTKE = ontologyName + "." + mainKeyEntity;
                

                if (jsonData.entities[mTKE] != null)
                {
                    
                    jsonData.entities[mTKE].items = new Dictionary<string, EntityItem>();

                    string sectionsJson = JsonConvert.SerializeObject(jsonData.entities[mTKE].properties);
                    
                    jsonData.entities[mTKE].items.Add(id, new EntityItem() {
                        id = id,
                        properties = null
                    });


                    var dbEntityData = GetValEntity(id, jsonData.structure.rdfType, jsonData.structure.ontologyName);
                    var listFields = new Dictionary<string, Property>();
                    GetFields(dbEntityData, JsonConvert.DeserializeObject<List<Property>>(sectionsJson)).ForEach(e =>
                    {
                        listFields.Add(e.property, e);
                    });
                    jsonData.entities[mTKE].items[id].properties = listFields;
                    
                    if(!loadedEntities.ContainsKey(jsonData.structure.relation.eid)) {
                        loadedEntities.Add(jsonData.structure.relation.eid, new List<ListItemsData>() {
                            new ListItemsData() {
                                id = id,
                                dbEntityData = dbEntityData
                            }
                        });
                    }

                    // jsonData.entities[mTKE].items[id].sections = GetSections(id, JsonConvert.DeserializeObject<List<Section>>(sectionsJson), jsonData.entities[mTKE], dbEntityData);
                    jsonData.structure.sections = GetSections(id, jsonData.structure.sections, jsonData.structure, dbEntityData);
                }
                
            }


            return jsonData;
        }


        public void AddNewJson (DataJsonStructure data)
        {
            jsonData = data.data;
        }

        public JsonStructure GetAsyncSection(string idSection, string section)
        {
            SparqlObject result = new SparqlObject();

            if (jsonData != null && jsonData.entities != null && jsonData.structure != null)
            {
                // Get the main Document index
                // var mainKeyEntity = jsonData.entities.FindIndex(e => e.rdftype == "http://purl.org/roh/mirror/bibo#Document");
                // var mainKeyEntity = jsonData.structure.rdfType;
                var keyEntity = jsonData.sections[section].rdfType;
                var ontologyName = jsonData.sections[section].ontologyName;
                var mTKE = ontologyName + "." + keyEntity;


                if (jsonData.entities[mTKE] != null)
                {
                    if (jsonData.entities[mTKE].items == null)
                    {
                        jsonData.entities[mTKE].items = new Dictionary<string, EntityItem>();
                    }

                    string sectionsJson = JsonConvert.SerializeObject(jsonData.entities[mTKE].properties);

                    if (!jsonData.entities[mTKE].items.ContainsKey(idSection))
                    {
                        jsonData.entities[mTKE].items.Add(idSection, new EntityItem()
                        {
                            id = idSection,
                            properties = null
                        });
                    }


                    var dbEntityData = GetValEntity(idSection, keyEntity, jsonData.entities[mTKE].ontologyName);


                    var listFields = new Dictionary<string, Property>();
                    GetFields(dbEntityData, JsonConvert.DeserializeObject<List<Property>>(sectionsJson)).ForEach(e =>
                    {
                        listFields.Add(e.property, e);
                    });
                    jsonData.entities[mTKE].items[idSection].properties = listFields;

                    // Add the loaded items into the object to get it after
                    if (!loadedEntities.ContainsKey(jsonData.sections[section].relation.eid))
                    {
                        loadedEntities.Add(jsonData.sections[section].relation.eid, new List<ListItemsData>() {
                            new ListItemsData() {
                                id = idSection,
                                dbEntityData = dbEntityData
                            }
                        });
                    }

                    // Init the loop into the sections
                    jsonData.sections[section].sections = GetSections(idSection, jsonData.sections[section].sections, jsonData.sections[section], dbEntityData, true);
                    jsonData.sections[section].uniqueLoaded = true;
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
        public List<Section> GetSections(string id, List<Section> sections, Entity entity, Dictionary<string, List<Dictionary<string, Data>>> dbEntityData, bool externalSection = false) 
        {
            for (int i = 0; i < sections.Count(); i++)
            {
                // var dbEntityData = GetValEntity(id, jsonData.entities[i].rdftype, jsonData.entities[i].ontologiaName);

                // if (sections[i].properties != null)
                // {
                //     sections[i].properties = GetFields(dbEntityData, sections[i].properties);
                // }

                if (sections[i].entities != null)
                {
                    sections[i].entities = GetEntities(id, dbEntityData, sections[i].entities);
                }
                
                if (sections[i].sections != null)
                {
                    // sections[i].sections.ForEach(e => GetSections(id, e, entity));
                    
                    sections[i].sections = GetSections(id, sections[i].sections, entity, dbEntityData);
                }

                if (sections[i].outSection != null)
                {
                    if (sections[i].outSection.defaultLoaded == true)
                    {
                        // sections[i].sections.ForEach(e => GetSections(id, e, entity));
                        sections[i].entities = GetEntities(id, dbEntityData, new List<Entity>() { jsonData.sections[sections[i].outSection.section] });
                    } else
                    {
                        // Load the id?
                        if (sections[i].outSection.loadOnce == true)
                        {
                            if (jsonData.sections[sections[i].outSection.section].uniqueLoaded != true)
                            {
                                sections[i].outSection.currentId = GetIdOutSection(id, dbEntityData, sections[i].outSection.section);
                            }
                        } else
                        {
                            sections[i].outSection.currentId = GetIdOutSection(id, dbEntityData, sections[i].outSection.section);
                        }
                        
                    }
                    // Set like loaded
                    if (sections[i].outSection.loadOnce == true)
                    {

                    }

                }
                
            }

            return sections;
        }


        /// Needs to be changed when call to loadList for the dbEntityData and loadedEntities origin
        /// Or not...
        protected List<Property> GetFields(Dictionary<string, List<Dictionary<string, Data>>> dbEntityData, List<Property> properties)
        {

            foreach (KeyValuePair<string, List<Dictionary<string, Data>>> item in dbEntityData)
            {

                foreach (Dictionary<string, Data> el in item.Value)
                {

                    for (int i = 0; i < properties.Count(); i++)
                    {
                        string uri = properties[i].property;

                        // Check if the property is parameter is empty in the current property, and if it's exist in jsonData properies section
                        //if (properties[i].property == null)
                        //{
                        //    jsonData.properties.ForEach(e => uri = (e.id == properties[i].id) ? e.property : null);
                        //}

                        try
                        {

                            var val = el["p"].value;

                            if (el["p"].type == "uri" && el["p"].value == uri)
                            {
                                properties[i].value = el["o"].value;

                                // 
                                if (properties[i].relation != null && properties[i].relation.entityReference != null && properties[i].relation.entityReference != "")
                                {

                                    string listItemUri = LoadList(item.Key, properties[i], el["o"].value, dbEntityData);
                                    
                                    properties[i].id = el["o"].value;

                                    // Load the options
                                    var options = new Dictionary<string, Dictionary<string, string>>();
                                    GetEntity(listItemUri, properties[i].relation, new Entity(), ref options, dbEntityData);
                                    properties[i].options = options;
                                }

                                // Get order
                                if (properties[i].relation != null && properties[i].relation.orderRdf != null && properties[i].relation.orderRdf != "")
                                {
                                    var orderIndex = item.Value.FindIndex(e => el["p"].value == properties[i].relation.orderRdf);
                                    
                                }
                            }



                        }
                        catch (System.Exception) {}
                    }

                }
            }

            return properties;
        }

        /// <summary>
        /// Load a list of items of elements 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="property"></param>
        /// <param name="listItemId"></param>
        /// <param name="dbEntityData">Database data</param>
        /// <returns></returns>
        protected string LoadList(string entityId, Property property, string listItemId, Dictionary<string, List<Dictionary<string, Data>>> dbEntityData)
        {

            var listItemUri = "";

            try
            {
                // Get the current entityRdfType
                var dbEntityDataItem = dbEntityData[entityId].Find(e => e["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
                var entityRdf = property.relation.ontologyName + "." + dbEntityDataItem["o"].value;

                // Check if exist a list of items
                if (jsonData.entities[entityRdf] != null && property.listItems != null)
                {
                    // Find the list of items of the entity reference given
                    
                    if (property.listItems == null)
                    {
                        property.listItems = new List<ListItems>();

                    }
                    
                    var lItemNew = new ListItems()
                    {
                        id = listItemId,
                    };

                    foreach (var item in dbEntityData[listItemId])
                    {
                        if (item["p"].type == "uri" && item["p"].value == property.relation.orderRdf)
                        {
                            lItemNew.order = int.Parse(item["o"].value);
                        }

                        if (item["p"].type == "uri" && item["p"].value == property.relation.itemRdf)
                        {
                            lItemNew.id = item["o"].value;
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

        /// <summary>
        /// Get all entities from a section
        /// </summary>
        /// <param name="id">Entity id parent</param>
        /// <param name="dbEntityData">Database data</param>
        /// <param name="entities">The entities array objects</param>
        /// <returns></returns>
        protected List<Entity> GetEntities(string id, Dictionary<string, List<Dictionary<string, Data>>> dbEntityData, List<Entity> entities)
        {

            // Item to return
            List<Entity> resEntities = new List<Entity>();
            foreach (var entity in entities)
            {

                // var keyEntity = jsonData.entities.FindIndex(e => e.rdfType == property.rdfType);
                string keyEntity = null;
                if (entity.relation != null)
                {
                    // keyEntity = entity.relation.entityReference;
                    keyEntity = entity.relation.ontologyName + "." + entity.relation.entityReference;
                }


                // get the related item
                if (keyEntity != null && entity.relation != null && entity.relation.itemRdf != null)
                {
                    // Get the current eid
                    string eid = entity.relation.eid;

                    if (entity.listItems == null)
                    {
                        entity.listItems = new Dictionary<string, List<ListItems>>();
                    }

                    // Check if the entityparent has loaded (should be loaded)
                    if (loadedEntities.ContainsKey(entity.relation.eidRel))
                    {

                        loadedEntities[entity.relation.eidRel].ForEach(e => {

                            try
                            {
                                // Search for the ids of the list item
                                var idsData = e.dbEntityData[e.id].FindAll(el => el["p"].value == entity.relation.property);
                                List<string> ids = new List<string>();

                                // Go over each listItem id
                                idsData.ForEach(cid => {

                                    var litid = cid["o"].value;

                                    // Check if the listItem Id exist in the data
                                    if (e.dbEntityData.ContainsKey(litid))
                                    {

                                        // Get the item element ID
                                        var dbEntityDataItem = e.dbEntityData[litid].Find(el => el["p"].value == entity.relation.itemRdf);
                                        var itemId = dbEntityDataItem["o"].value;

                                        // if (jsonData.entities[entityRdf] != null)
                                        // if (jsonData.entities.ContainsKey(entityRdf))
                                        // Check if the Id given is not null & if the entity rdftype is not null
                                        if (itemId != null && jsonData.entities.ContainsKey(keyEntity))
                                        {
                                            if (entity.listItems == null)
                                            {
                                                entity.listItems = new Dictionary<string, List<ListItems>>();
                                            }

                                            if (!entity.listItems.ContainsKey(e.id))
                                            {
                                                entity.listItems.Add(e.id, new List<ListItems>());

                                            }

                                            // Start a list item & set the parent Id
                                            var lItemNew = new ListItems()
                                            {
                                                parentId = e.id,
                                            };

                                            // Set the id
                                            lItemNew.id = itemId;

                                            // Add the Id into the list
                                            ids.Add(itemId);

                                            // Set the order value
                                            var dbOrderDataItem = e.dbEntityData[cid["o"].value].Find(el => el["p"].value == entity.relation.orderRdf);
                                            var itemOrderValue = dbOrderDataItem["o"].value;
                                            lItemNew.order = int.Parse(itemOrderValue);

                                            // foreach (var item in dbEntityData[cid["o"].value])
                                            // {
                                            //     if (item["p"].type == "uri" && item["p"].value == entity.orderRdf)
                                            //     {
                                            //         lItemNew.order = int.Parse(item["o"].value);
                                            //     }

                                            //     if (item["p"].type == "uri" && item["p"].value == entity.itemRdf)
                                            //     {
                                            //         lItemNew.id = item["o"].value;
                                            //     }
                                            // }

                                            var options = new Dictionary<string, Dictionary<string, string>>();
                                            resEntities.Add(GetEntity(itemId, entity.relation, entity, ref options));

                                            entity.listItems[e.id].Add(lItemNew);

                                        }
                                    }

                                });


                            }
                            catch (System.Exception)
                            {
                                throw;
                            }
                        });
                    }
                    else
                    {
                        // In the case that not exist the current db data in loadedEntities
                        try
                        {
                            // Search for the ids of the list item
                            var idsData = dbEntityData[id].FindAll(el => el["p"].value == entity.relation.property);
                            List<string> ids = new List<string>();

                            // Go over each listItem id
                            idsData.ForEach(cid => {

                                var litid = cid["o"].value;

                                // Check if the listItem Id exist in the data
                                if (dbEntityData.ContainsKey(litid))
                                {

                                    // Get the item element ID
                                    var dbEntityDataItem = dbEntityData[litid].Find(el => el["p"].value == entity.relation.itemRdf);
                                    var itemId = dbEntityDataItem["o"].value;

                                    // if (jsonData.entities[entityRdf] != null)
                                    // if (jsonData.entities.ContainsKey(entityRdf))
                                    // Check if the Id given is not null & if the entity rdftype is not null
                                    if (itemId != null && jsonData.entities.ContainsKey(keyEntity))
                                    {
                                        if (entity.listItems == null)
                                        {
                                            entity.listItems = new Dictionary<string, List<ListItems>>();
                                        }

                                        if (!entity.listItems.ContainsKey(id))
                                        {
                                            entity.listItems.Add(id, new List<ListItems>());

                                        }

                                        // Start a list item & set the parent Id
                                        var lItemNew = new ListItems()
                                        {
                                            parentId = id,
                                        };

                                        // Set the id
                                        lItemNew.id = itemId;

                                        // Add the Id into the list
                                        ids.Add(itemId);

                                        // Set the order value
                                        var dbOrderDataItem = dbEntityData[cid["o"].value].Find(el => el["p"].value == entity.relation.orderRdf);
                                        var itemOrderValue = dbOrderDataItem["o"].value;
                                        lItemNew.order = int.Parse(itemOrderValue);

                                        // Get entity
                                        var options = new Dictionary<string, Dictionary<string, string>>();
                                        resEntities.Add(GetEntity(itemId, entity.relation, entity, ref options));

                                        entity.listItems[id].Add(lItemNew);

                                    }
                                }

                            });


                        }
                        catch (System.Exception)
                        {
                            throw;
                        }
                    }

                }
                else if (entity.relation != null && entity.relation.entityReference != null)
                {
                    // Get the current eid
                    string eid = entity.relation.eid;

                    if (entity.listItems == null)
                    {
                        entity.listItems = new Dictionary<string, List<ListItems>>();
                    }

                    // Check if the entityparent has loaded (should be loaded)
                    if (loadedEntities.ContainsKey(entity.relation.eidRel))
                    {

                        loadedEntities[entity.relation.eidRel].ForEach(e => {

                            try
                            {
                                // Search for the ids of the list item
                                var idsData = e.dbEntityData[e.id].FindAll(el => el["p"].value == entity.relation.property);
                                List<string> ids = new List<string>();

                                // Go over each id (Should be only one)
                                idsData.ForEach(cid => {

                                    // The id
                                    var theId = cid["o"].value;


                                    // Check if the Id given is not null & if the entity rdftype is not null
                                    if (theId != null && jsonData.entities.ContainsKey(keyEntity))
                                    {
                                        if (entity.listItems == null)
                                        {
                                            entity.listItems = new Dictionary<string, List<ListItems>>();
                                        }

                                        if (!entity.listItems.ContainsKey(e.id))
                                        {
                                            entity.listItems.Add(e.id, new List<ListItems>());

                                        }

                                        // Start a list item & set the parent Id
                                        var lItemNew = new ListItems()
                                        {
                                            parentId = e.id,
                                        };

                                        // Set the id
                                        lItemNew.id = theId;

                                        // Add the Id into the list
                                        ids.Add(theId);

                                        // Get entity
                                        var options = new Dictionary<string, Dictionary<string, string>>();
                                        resEntities.Add(GetEntity(theId, entity.relation, entity, ref options));

                                        entity.listItems[e.id].Add(lItemNew);

                                    }

                                });


                            }
                            catch (System.Exception)
                            {
                                throw;
                            }
                        });
                    }
                    else
                    {

                        // In the case that not exist the current db data in loadedEntities
                        try
                        {
                            // Search for the ids of the list item
                            var idsData = dbEntityData[id].FindAll(el => el["p"].value == entity.relation.property);
                            List<string> ids = new List<string>();

                            // Go over each listItem id
                            idsData.ForEach(cid => {

                                var litid = cid["o"].value;

                                // Check if the listItem Id exist in the data
                                if (dbEntityData.ContainsKey(litid))
                                {

                                    // Get the item element ID
                                    var dbEntityDataItem = dbEntityData[litid].Find(el => el["p"].value == entity.relation.itemRdf);
                                    var itemId = dbEntityDataItem["o"].value;

                                    // if (jsonData.entities[entityRdf] != null)
                                    // if (jsonData.entities.ContainsKey(entityRdf))
                                    // Check if the Id given is not null & if the entity rdftype is not null
                                    if (itemId != null && jsonData.entities.ContainsKey(keyEntity))
                                    {
                                        if (entity.listItems == null)
                                        {
                                            entity.listItems = new Dictionary<string, List<ListItems>>();
                                        }

                                        if (!entity.listItems.ContainsKey(id))
                                        {
                                            entity.listItems.Add(id, new List<ListItems>());

                                        }

                                        // Start a list item & set the parent Id
                                        var lItemNew = new ListItems()
                                        {
                                            parentId = id,
                                        };

                                        // Set the id
                                        lItemNew.id = itemId;

                                        // Add the Id into the list
                                        ids.Add(itemId);

                                        // Set the order value
                                        var dbOrderDataItem = dbEntityData[cid["o"].value].Find(el => el["p"].value == entity.relation.orderRdf);
                                        var itemOrderValue = dbOrderDataItem["o"].value;
                                        lItemNew.order = int.Parse(itemOrderValue);

                                        // Get the entity and add the reference
                                        var options = new Dictionary<string, Dictionary<string, string>>();
                                        resEntities.Add(GetEntity(itemId, entity.relation, entity, ref options));
                                        
                                        entity.listItems[id].Add(lItemNew);

                                    }
                                }

                            });


                        }
                        catch (System.Exception)
                        {
                            throw;
                        }

                    }
                }

                // // Check if the entities has sections
                // if (entity.sections != null)
                // {
                //     // Get all sections from entity
                //     entity.sections = GetSections(id, entity.sections, entity, dbEntityData);
                // }

                // resEntities.Add(entity);
            }



            return resEntities;
        }


        protected Entity GetEntity(string entityUri, EntityRelation relation, Entity entity, ref Dictionary<string, Dictionary<string, string>> options, Dictionary<string, List<Dictionary<string, Data>>> dbEntityData = null)
        {

            // Item to return

            // var keyEntity = jsonData.entities.FindIndex(e => e.rdfType == property.rdfType);
            var keyEntity = relation.entityReference;
            var ontologyName = relation.ontologyName;
            var mTKE = ontologyName + "." + keyEntity;

            
            try
            {
                // Check if exist the entity
                if (jsonData.entities.ContainsKey(mTKE))
                {
                    
                    // Start the items dictionary if is undefined
                    if (jsonData.entities[mTKE].items == null)
                    {
                        jsonData.entities[mTKE].items = new Dictionary<string, EntityItem>();
                    }

                    // Check if exist and the item, if exist, don't load the item again
                    if (!jsonData.entities[mTKE].items.ContainsKey(entityUri))
                    {
                        // Load the entity calling to the DB
                        if (dbEntityData == null)
                        {
                            dbEntityData = GetValEntity(entityUri, keyEntity, jsonData.entities[mTKE].ontologyName);
                        }


                        // Check if exist the loaded Entity & the current element and add this into the loaded entities local variable
                        if (loadedEntities.ContainsKey(relation.eid))
                        {
                            loadedEntities[relation.eid].Add(new ListItemsData() {
                                id = entityUri,
                                dbEntityData = dbEntityData
                            });
                        } else {
                            loadedEntities.Add(relation.eid, new List<ListItemsData>() {
                                new ListItemsData() {
                                    id = entityUri,
                                    dbEntityData = dbEntityData
                                }
                            });
                        }

                        // Add new entity item
                        jsonData.entities[mTKE].items.Add(entityUri, new EntityItem());

                        if (entity.sections != null)
                        {
                            // Serialize to remove the reference in the loop
                            // string sectionsJson = JsonConvert.SerializeObject(jsonData.entities[mTKE].sections);

                            // Set the id & the sections
                            entity.id = entityUri;
                            // entity.sections = (jsonData.entities[mTKE].sections != null) ? GetSections(entityUri, entity.sections, entity, dbEntityData) : null;
                            entity.sections = GetSections(entityUri, entity.sections, entity, dbEntityData);
                        }


                        // Serialize to remove the reference in the loop
                        string PropertiesJson = JsonConvert.SerializeObject(jsonData.entities[mTKE].properties);

                        // Set the id & the sections
                        jsonData.entities[mTKE].items[entityUri].id = entityUri;
                        var listFields = new Dictionary<string, Property>();

                        // Get the fields of each item
                        GetFields(dbEntityData, JsonConvert.DeserializeObject<List<Property>>(PropertiesJson)).ForEach(e =>
                        {
                            if (e.property != null && !listFields.ContainsKey(e.property))
                            {
                                listFields.Add(e.property, e);
                            }
                        });

                        jsonData.entities[mTKE].items[entityUri].properties = listFields;
                        // jsonData.entities[mTKE].items[entityUri].properties = (jsonData.entities[mTKE].properties != null) ? GetFields(dbEntityData, JsonConvert.DeserializeObject<Dictionary<string,Property>>(PropertiesJson)) : null;

                        // Fill the options
                        options.Add(jsonData.entities[mTKE].items[entityUri].id, new Dictionary<string, string>() {
                            {"id", jsonData.entities[mTKE].items[entityUri].id}
                        });
                        
                        // if (property != null && property.fieldsDB != null)
                        // {
                        //     // Get all values from each element into the select options
                        //     property.fieldsDB.ForEach(e => {
                        //         options[jsonData.entities[mTKE].items[entityUri].id].Add(e.rdfType, jsonData.entities[mTKE].items[entityUri].properties[e.rdfType].value);
                        //     });
                        // }

                    }

                }

            }
            catch (System.Exception e) {
                throw;
            }

            return entity;
        }


        protected string GetIdOutSection(string id, Dictionary<string, List<Dictionary<string, Data>>> dbEntityData, string section)
        {
            // Item to return
            string theId = "";

            var entity = jsonData.sections[section];


            // var keyEntity = jsonData.entities.FindIndex(e => e.rdfType == property.rdfType);
            string keyEntity;
            if (entity.relation != null)
            {
                keyEntity = entity.relation.entityReference;
            }
            else
            {
                keyEntity = entity.rdfType;
            }



            // get the related item
            if (entity.relation != null && entity.relation.itemRdf == null && entity.relation.entityReference != null)
            {
                // Get the current eid
                string eid = entity.relation.eid;

                if (entity.listItems == null)
                {
                    entity.listItems = new Dictionary<string, List<ListItems>>();
                }

                // Check if the entityparent has loaded (should be loaded)
                if (loadedEntities.ContainsKey(entity.relation.eidRel))
                {

                    loadedEntities[entity.relation.eidRel].ForEach(e => {

                        try
                        {
                            // Search for the ids of the list item
                            var idsData = e.dbEntityData[e.id].FindAll(el => el["p"].value == entity.relation.property);
                            
                            // Go over each listItem id
                            idsData.ForEach(cid => {

                                theId = cid["o"].value;

                            });


                        }
                        catch (System.Exception)
                        {
                            throw;
                        }
                    });
                } 
                else
                {
                       
                    // In the case that not exist the current db data in loadedEntities
                    try
                    {
                        // Search for the ids of the list item
                        var idsData = dbEntityData[id].FindAll(el => el["p"].value == entity.relation.property);

                        // Go over each listItem id
                        idsData.ForEach(cid => {

                            theId = cid["o"].value;

                        });


                    }
                    catch (System.Exception)
                    {
                        throw;
                    }
                        
                }
            }


            return theId;


        }


        public void DeserialiceTemplate()
        {
            // var jsonFile = "Config/Template.json";
            // var jsonFile = "Config/Plantilla.json";
            jsonData = new JsonStructure();
            jsonData.entities = new Dictionary<string, Entity>();
            // jsonData.startEntity = "http://purl.org/roh/mirror/bibo#Document";
            // jsonData.startItem = "http://gnoss.com/items/Document_6c5ad967-1775-4960-896b-932a2e407d97_dfd9016f-a9b5-4023-9b22-85c760f187d1";

            string jsonTemplateFolder = "Config/Templates";
            string jsonSectionsFolder = "Config/Sections";
            string mainjsonFile = "Config/Plantilla2.json";

            KeyValuePair<string, Entity> readOnlyJsonFile(string json)
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
                    return new KeyValuePair<string, Entity>(newEntity.rdfType, JsonConvert.DeserializeObject<Entity>(text));
                    
                }
                catch (System.Exception)
                {
                    throw new Exception("Error during read and deserialize the json file config");
                }
            }

            Dictionary<string, Entity> readJsonFiles(string jsonFolder)
            {
                Dictionary<string, string> filesTexts = new Dictionary<string, string>();
                Dictionary<string, Entity> jsons = new Dictionary<string, Entity>();
                // string jsonPath = Path.Combine(AppContext.BaseDirectory, jsonFiles);

                string jsonPath = Path.Combine(AppContext.BaseDirectory, jsonFolder);
                var directoryInfo = new DirectoryInfo(jsonPath);

                if (directoryInfo.Exists)
                {
                    FileInfo[] info = directoryInfo.GetFiles("*.json");

                    foreach (var file in info)
                    {
                        // Read the file
                        StreamReader contentsText = file.OpenText();

                        string text = contentsText.ReadToEnd();

                        // Set the name of the section
                        string fileName = file.Name.Split('.')[0];

                        filesTexts.Add(fileName, text);
                    }

                    try
                    {
                        foreach (var item in filesTexts)
                        {
                            jsons.Add(item.Key, JsonConvert.DeserializeObject<Entity>(item.Value));
                        }
                    }
                    catch (System.Exception)
                    {
                        throw new Exception("Error during read and deserialize the json file config");
                    }
                }
                return jsons;

            }

            // Get each json model template file
            var entityRes = readJsonFiles(jsonTemplateFolder);
            foreach (var jsonItem in entityRes)
            {
                jsonData.entities.Add(jsonItem.Value.ontologyName + "." + jsonItem.Value.rdfType, jsonItem.Value);
            }

            // Get the main structure (Main json)
            var el = readOnlyJsonFile(mainjsonFile);
            jsonData.structure = el.Value;

            // Get the sections
            var sectionsList = readJsonFiles(jsonSectionsFolder);
            jsonData.sections = sectionsList;

        }



    }
}
