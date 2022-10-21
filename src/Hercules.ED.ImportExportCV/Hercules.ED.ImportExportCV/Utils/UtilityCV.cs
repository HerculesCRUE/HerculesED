using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utils
{
    public class UtilityCV
    {
        /// <summary>
        /// Propiedad para marcar las entidades como públicas
        /// </summary>
        public static string PropertyIspublic { get { return "http://w3id.org/roh/isPublic"; } }

        /// <summary>
        /// Propiedad para comprobar si no es editable, tiene que tener en alguna propiedad
        /// de las claves algún valor de los valores
        /// </summary>
        public static Dictionary<string, List<string>> PropertyNotEditable = new Dictionary<string, List<string>>()
        {
            { "http://w3id.org/roh/crisIdentifier", new List<string>() },
            { "http://w3id.org/roh/isValidated", new List<string>(){ "true"} }
            //TODO estado de validacion
        };

        public static Dictionary<string, string> dicPrefix = new Dictionary<string, string>() {
            { "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
            {"rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
            {"foaf", "http://xmlns.com/foaf/0.1/" },
            {"vivo", "http://vivoweb.org/ontology/core#" },
            {"owl", "http://www.w3.org/2002/07/owl#" },
            {"bibo", "http://purl.org/ontology/bibo/" },
            {"roh", "http://w3id.org/roh/" },
            {"dct", "http://purl.org/dc/terms/" },
            {"xsd", "http://www.w3.org/2001/XMLSchema#" },
            {"obo", "http://purl.obolibrary.org/obo/" },
            {"vcard", "https://www.w3.org/2006/vcard/ns#" },
            {"dc", "http://purl.org/dc/elements/1.1/" },
            {"gn", "http://www.geonames.org/ontology#" }
        };


        /// <summary>
        /// Cambia la propiedad añadiendole elprefijo
        /// </summary>
        /// <param name="pProperty">Propiedad con la URL completa</param>
        /// <returns>Url con prefijo</returns>
        public static string AniadirPrefijo(string pProperty)
        {
            KeyValuePair<string, string> prefix = dicPrefix.First(x => pProperty.StartsWith(x.Value));
            return pProperty.Replace(prefix.Value, prefix.Key + ":");
        }
        
        /// <summary>
        /// Devuelve el identificador numerico apartir del tipo de documento(SAD1, SAD2, SAD3).
        /// </summary>
        /// <param name="tipoDocumento">Tipo de documento</param>
        /// <returns>Identificador del FECYT</returns>
        public static string IdentificadorFECYT(string tipoDocumento)
        {
            if (string.IsNullOrEmpty(tipoDocumento))
            {
                return null;
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD1"))
            {
                return "060.010.010.000";
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD2"))
            {
                return "060.010.020.000";
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD3"))
            {
                return "060.010.030.000";
            }
            return null;
        }

        /// <summary>
        /// Fusiona dos entidades
        /// </summary>
        /// <param name="pIdMainEntity">Identificador de la entidad a la que pertenece la entidad auxiliar</param>
        /// <param name="pPropertyIDs">Propiedades que apuntan a la auxiliar</param>
        /// <param name="pEntityIDs">Entidades que apuntan a la auxiliar</param>
        /// <param name="pLoadedEntity">Entidad cargada en BBDD</param>
        /// <param name="pUpdatedEntity">Nueva entidad</param>
        /// <returns>true si se ha actualizado correctamente</returns>
        public static bool UpdateEntityAux(Guid pIdMainEntity, List<string> pPropertyIDs, List<string> pEntityIDs, Entity pLoadedEntity, Entity pUpdatedEntity, ResourceApi mResourceApi)
        {
            bool update = true;
            Dictionary<Guid, List<TriplesToInclude>> triplesInclude = new Dictionary<Guid, List<TriplesToInclude>>() { { pIdMainEntity, new List<TriplesToInclude>() } };
            Dictionary<Guid, List<RemoveTriples>> triplesRemove = new Dictionary<Guid, List<RemoveTriples>>() { { pIdMainEntity, new List<RemoveTriples>() } };
            Dictionary<Guid, List<TriplesToModify>> triplesModify = new Dictionary<Guid, List<TriplesToModify>>() { { pIdMainEntity, new List<TriplesToModify>() } };

            foreach (Entity.Property property in pUpdatedEntity.properties)
            {
                bool remove = property.values == null || property.values.Count == 0 || !property.values.Exists(x => !string.IsNullOrEmpty(x));
                //Recorremos las propiedades de la entidad a actualizar y modificamos la entidad recuperada de BBDD               
                Entity.Property propertyLoadedEntity = null;
                if (pLoadedEntity != null)
                {
                    propertyLoadedEntity = pLoadedEntity.properties.FirstOrDefault(x => x.prop == property.prop);
                }
                if (propertyLoadedEntity != null)
                {
                    if (remove)
                    {
                        foreach (string valor in propertyLoadedEntity.values)
                        {
                            triplesRemove[pIdMainEntity].Add(new RemoveTriples()
                            {
                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                            });
                        }
                    }
                    else
                    {
                        HashSet<string> items = new HashSet<string>();
                        foreach (string valor in propertyLoadedEntity.values)
                        {
                            items.Add(GetEntityOfValue(valor));
                        }
                        foreach (string valor in property.values)
                        {
                            items.Add(GetEntityOfValue(valor));
                        }

                        foreach (string item in items)
                        {
                            List<string> valuesLoadedEntity = propertyLoadedEntity.values.Where(x => GetEntityOfValue(x) == item).ToList();
                            List<string> valuesEntity = property.values.Where(x => GetEntityOfValue(x) == item).ToList();
                            int numLoaded = valuesLoadedEntity.Count;
                            int numNew = valuesEntity.Count;
                            int numIntersect = valuesLoadedEntity.Intersect(valuesEntity).Count();
                            if (numLoaded != numNew || numLoaded != numIntersect)
                            {
                                if (numLoaded == 1 && numNew == 1)
                                {
                                    triplesModify[pIdMainEntity].Add(new TriplesToModify()
                                    {

                                        Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                        NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valuesEntity[0]),
                                        OldValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valuesLoadedEntity[0])
                                    });
                                }
                                else
                                {
                                    //Eliminaciones
                                    foreach (string valor in valuesLoadedEntity.Except(property.values))
                                    {
                                        triplesRemove[pIdMainEntity].Add(new RemoveTriples()
                                        {

                                            Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                            Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                                        });
                                    }
                                    //Inserciones
                                    foreach (string valor in valuesEntity.Except(valuesLoadedEntity))
                                    {
                                        if (!valor.EndsWith("@@@"))
                                        {
                                            triplesInclude[pIdMainEntity].Add(new TriplesToInclude()
                                            {

                                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                                NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (!remove)
                {
                    foreach (string valor in property.values)
                    {
                        if (!valor.EndsWith("@@@"))
                        {
                            triplesInclude[pIdMainEntity].Add(new TriplesToInclude()
                            {

                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                NewValue = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                            });
                        }
                    }
                }
                else if (remove)
                {
                    List<Entity.Property> propertiesLoadedEntityRemove = pLoadedEntity.properties.Where(x => x.prop.StartsWith(property.prop)).ToList();
                    foreach (Entity.Property propertyToRemove in propertiesLoadedEntityRemove)
                    {
                        foreach (string valor in propertyToRemove.values)
                        {
                            triplesRemove[pIdMainEntity].Add(new RemoveTriples()
                            {

                                Predicate = string.Join("|", pPropertyIDs) + "|" + GetPropUpdateEntityAux(property.prop),
                                Value = string.Join("|", pEntityIDs) + "|" + GetValueUpdateEntityAux(valor)
                            });
                        }
                    }
                }
            }
            if (pUpdatedEntity.auxEntityRemove != null)
            {
                foreach (string auxEntityRemove in pUpdatedEntity.auxEntityRemove)
                {
                    if (auxEntityRemove.StartsWith("http"))
                    {
                        foreach (Entity.Property property in pLoadedEntity.properties)
                        {
                            foreach (string valor in property.values)
                            {
                                if (valor.Contains(auxEntityRemove))
                                {
                                    //Elmiminamos de la lista a aliminar los hijos de la entidad a eliminar
                                    triplesRemove[pIdMainEntity].RemoveAll(x => x.Value.Contains(auxEntityRemove));
                                    //Eliminamos la entidad auxiliar
                                    triplesRemove[pIdMainEntity].Add(new RemoveTriples()
                                    {

                                        Predicate = string.Join("|", pPropertyIDs) + "|" + property.prop.Substring(0, property.prop.IndexOf("@@@")),
                                        Value = string.Join("|", pEntityIDs) + "|" + auxEntityRemove
                                    });
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (triplesRemove[pIdMainEntity].Count > 0)
            {
                update = update && mResourceApi.DeletePropertiesLoadedResources(triplesRemove)[pIdMainEntity];
            }
            if (triplesInclude[pIdMainEntity].Count > 0)
            {
                update = update && mResourceApi.InsertPropertiesLoadedResources(triplesInclude)[pIdMainEntity];
            }
            if (triplesModify[pIdMainEntity].Count > 0)
            {
                update = update && mResourceApi.ModifyPropertiesLoadedResources(triplesModify)[pIdMainEntity];
            }
            return update;
        }

        /// <summary>
        /// Transforma el valor de la propiedad para su carga en una entiadad auxiliar
        /// </summary>
        /// <param name="pValue">Valor</param>
        /// <returns>Valor trasnformado</returns>
        private static string GetValueUpdateEntityAux(string pValue)
        {
            return pValue.Replace("@@@", "|");
        }

        /// <summary>
        /// Transforma la propiedad para su carga en una entiadad auxiliar
        /// </summary>
        /// <param name="pProp">Propiedad</param>
        /// <returns>Propiedad transformada</returns>
        private static string GetPropUpdateEntityAux(string pProp)
        {
            while (pProp.Contains("@@@"))
            {
                int indexInitRdfType = pProp.IndexOf("@@@");
                int indexEndRdfType = pProp.IndexOf("|", indexInitRdfType);
                if (indexEndRdfType > indexInitRdfType)
                {
                    pProp = pProp.Substring(0, indexInitRdfType) + pProp.Substring(indexEndRdfType);
                }
                else
                {
                    pProp = pProp.Substring(0, indexInitRdfType);
                }
            }
            return pProp;
        }

        /// <summary>
        /// Obtiene la entidad del valor
        /// </summary>
        /// <param name="pValue">Valor</param>
        /// <returns>Valor de la entidad</returns>
        private static string GetEntityOfValue(string pValue)
        {
            string entityID = "";
            if (pValue.Contains("@@@"))
            {
                entityID = pValue.Substring(0, pValue.IndexOf("@@@"));
            }
            return entityID;
        }

    }
}