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
    public class LoadItems
    {
        // private List<Entity> entities;
        private JsonStructure jsonData;
        private ResourceApi resourceApi;

        public LoadItems()
        {
            // this.DeserialiceTemplate();

            string jsonPath = "Config/ConfigOauth/OAuthV3.config";
            resourceApi = new ResourceApi(jsonPath, null, new LogHelperFile("c:\\logs\\", "logCV"));
        }

        /*
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

        public List<List<Dictionary<string, Data>>> GetValEntity(string pId, string rdf, string pGraph)
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
                    SparqlObject result2 = resourceApi.VirtuosoQuery("select distinct ?s ?p ?o", $"where{{<{fila["entity"].value}> ?p ?o}}", pGraph);
                    listResult.Add(result2.results.bindings);
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
                for (int i = 0; i < jsonData.entities.Count(); i++)
                {
                    var dbEntityData = GetValEntity(jsonData.entities[i].items[0].id, jsonData.entities[i].rdftype, jsonData.entities[i].ontologiaName);

                    // Read the file 
                    jsonData.entities[i].items[0].properties = GetFields(dbEntityData, jsonData.entities[i].items[0].properties);
                    jsonData.entities[i].items[0].entities = GetEntities(dbEntityData, jsonData.entities[i].items[0].entities);
                }
            }


            return jsonData;
        }

        protected List<Property> GetFields(List<List<Dictionary<string, Data>>> dbEntityData, List<Property> properties)
        {

            for (int i = 0; i < properties.Count(); i++)
            {
                string uri = properties[i].uri;
                if (properties[i].uri == null)
                {
                    jsonData.properties.ForEach(e => uri = (e.id == properties[i].id) ? e.uri : null);
                }

                try
                {
                    // foreach (List<Dictionary<string, Data>> item in dbEntityData)
                    // {
                    //     foreach (Dictionary<string, Data> el in dbEntityData[0])
                    //     {
                    //         var tfs = el["p"].value;
                    //         if (el["p"].type == "uri" && el["p"].value == uri)
                    //         {
                    //             properties[i].value = el["o"].value;
                    //         }
                    //     }
                    // }

                    
                    foreach (Dictionary<string, Data> el in dbEntityData[0])
                    {
                        var tfs = el["p"].value;
                        if (el["p"].type == "uri" && el["p"].value == uri)
                        {
                            properties[i].value = el["o"].value;
                        }
                    }
                }
                catch (System.Exception) {}
            }


            return properties;
        }

        protected List<Entity> GetEntities(List<List<Dictionary<string, Data>>> dbEntityData, List<Entity> entity)
        {

            List<List<Dictionary<string, Data>>> newDbEntityData;

            for (int i = 0; i < entity.Count(); i++)
            {
                try
                {

                    List<List<Property>> propiedades = new List<List<Property>>();
                    EntityItem entityItem = entity[i].items[0];
                    entity[i].items = new List<EntityItem>();
                    
                    // Not get the first element
                    for (int n = 1; n < dbEntityData.Count(); n++) 
                    {
                        foreach (Dictionary<string, Data> el in dbEntityData[n])
                        {

                            if (el != null)
                            {    

                                var tfs = el["p"].value;
                                if (el["p"].type == "uri" && el["p"].value == entity[i].rdftype)
                                {
                                    // EntityItem newEntity = new EntityItem();
                                    // // newEntity.properties = entityItem.properties;
                                    // // newEntity.entities = entityItem.entities;

                                    newDbEntityData = GetValEntity(el["o"].value, entity[i].rdftype, entity[i].ontologiaName);

                                    // if (newDbEntityData != null && entityItem.entities != null)
                                    // {
                                    //     newEntity.entities = GetEntities(newDbEntityData, entityItem.entities);
                                    // }
                                    // //  Get the propierties
                                    // if (newDbEntityData != null && entityItem.properties != null)
                                    // {
                                    //     newEntity.properties = GetFields(newDbEntityData, entityItem.properties);
                                    // }

                                    // newEntity.id = el["o"].value;
                                    
                                    propiedades.Add(GetFields(newDbEntityData, entityItem.properties));

                                    // entity[i].items.Add(new EntityItem {
                                    //     id = el["o"].value,
                                    //     properties = (newDbEntityData != null && entityItem.properties != null) ? GetFields(newDbEntityData, entityItem.properties) : null,
                                    //     entities = (newDbEntityData != null && entityItem.entities != null) ? GetEntities(newDbEntityData, entityItem.entities) : null,
                                    // });
                                    
                                    //  Get the entities

                                    // entity[i].items.Add(newEntity);
                                }
                            }
                        }
                    }
                    // foreach (List<Dictionary<string, Data>> item in dbEntityData)
                    // {
                    //     foreach (Dictionary<string, Data> el in item)
                    //     {


                    //         var entityItem = entity[i].items[0];

                    //         var tfs = el["p"].value;
                    //         if (el["p"].type == "uri" && el["p"].value == entity[i].rdftype)
                    //         {
                    //             entity[i].id = el["o"].value;

                    //             newDbEntityData = GetValEntity(entity[i].id, jsonData.entities[i].rdftype);

                    //             // Read the file 
                    //             entity[i].properties = GetFields(newDbEntityData, entity[i].properties);
                    //             entity[i].entities = GetEntities(newDbEntityData, entity[i].entities);


                    //         }
                    //     }
                    // }
                }
                catch (System.Exception) {
                    throw;
                }
            }


            return entity;
        }

        public void DeserialiceTemplate()
        {
            // var jsonFile = "Config/Template.json";
            var jsonFile = "Config/Plantilla.json";

            string jsonPath = Path.Combine(AppContext.BaseDirectory, jsonFile);

            try
            {
                // Open the json
                StreamReader file = File.OpenText(jsonPath);
                // Read the file
                string text = file.ReadToEnd();
                // Deserialize the json
                jsonData = JsonConvert.DeserializeObject<JsonStructure>(text);
            } 
            catch (System.Exception)
            {
                throw new Exception("Error during read and deserialize the json file config");
            }
        }
        */


    }
}
