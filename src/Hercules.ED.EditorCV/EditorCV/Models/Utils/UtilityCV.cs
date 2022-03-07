using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using GuardadoCV.Models.API;
using GuardadoCV.Models.API.Templates;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace GuardadoCV.Models.Utils
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

        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configOAuth/OAuthV3.config");
        private static List<Tab> mTabTemplates;

        /// <summary>
        /// Obtiene la persona propietaria de un CV
        /// </summary>
        /// <param name="pCvID">Identificador del CV</param>
        /// <returns>ID de la persona</returns>
        public static string GetPersonFromCV(string pCvID)
        {
            return mResourceApi.VirtuosoQuery("select *", "where{<" + pCvID + "> <http://w3id.org/roh/cvOf> ?person. }", "curriculumvitae").results.bindings.First()["person"].value;
        }

        /// <summary>
        /// Obtiene las propiedades de las entidades pasadas por parámetro
        /// </summary>
        /// <param name="pIds">Identificadores de las entidades de las que recuperar sus propiedades</param>
        /// <param name="pGraph">Grafo en el que realizar las consultas</param>
        /// <param name="pProperties">Propiedades a recuperar</param>        
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <param name="pDicQueries">Caché para las queries (select + where +graph)</param>
        /// <returns></returns>
        public static Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetProperties(HashSet<string> pIds, string pGraph, List<PropertyData> pProperties, string pLang, Dictionary<string, SparqlObject> pDicQueries)
        {
            int paginacion = 10000;
            int maxIn = 1000;
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = new Dictionary<string, List<Dictionary<string, SparqlObject.Data>>>();

            SparqlObject sparqlObject = null;
            //1º Hacemos las que no tienen orden
            if (pProperties.Exists(x => string.IsNullOrEmpty(x.order)))
            {
                List<List<string>> listOfLists = SplitList(pIds.ToList(), maxIn).ToList();
                foreach (List<string> list in listOfLists)
                {
                    int offset = 0;
                    int limit = paginacion;
                    while (limit == paginacion)
                    {
                        string select = @$" select * where
                                            {{
                                                select distinct ?s ?p ?o ";
                        string where = @$"      where
                                                {{
                                                   ?s ?p ?o. 
                                                   FILTER( lang(?o) = '{pLang}' OR lang(?o) = '' OR !isLiteral(?o) )  
                                                   FILTER(?s in(<{string.Join(">,<", list.OrderByDescending(x=>x))}>)) 
                                                   FILTER(?p in(<{string.Join(">,<", pProperties.Where(x => string.IsNullOrEmpty(x.order)).Select(x => x.property).ToList().OrderByDescending(x => x))}>))
                                                }} 
                                                order by asc(?o) asc(?p) asc(?s)
                                            }} limit {limit} offset {offset}";
                        string claveCache = select + where + pGraph;
                        SparqlObject sparqlObjectAux = null;
                        if (pDicQueries.ContainsKey(claveCache))
                        {
                            sparqlObjectAux = pDicQueries[claveCache];
                        }
                        else
                        {
                            sparqlObjectAux = mResourceApi.VirtuosoQuery(select, where, pGraph);
                            pDicQueries[claveCache] = sparqlObjectAux;
                        }
                        limit = sparqlObjectAux.results.bindings.Count;
                        offset += sparqlObjectAux.results.bindings.Count;
                        foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObjectAux.results.bindings)
                        {
                            if (!data.ContainsKey(fila["s"].value))
                            {
                                data.Add(fila["s"].value, new List<Dictionary<string, SparqlObject.Data>>());
                            }
                            data[fila["s"].value].Add(fila);
                        }
                        if (sparqlObject == null)
                        {
                            sparqlObject = sparqlObjectAux;
                        }
                        else
                        {
                            sparqlObject.results.bindings.AddRange(sparqlObjectAux.results.bindings);
                        }
                    }
                }
            }

            //2º Hacemos las que tienen orden
            if (pProperties.Exists(x => !string.IsNullOrEmpty(x.order)))
            {
                foreach (PropertyData property in pProperties.Where(x => !string.IsNullOrEmpty(x.order)))
                {
                    string[] orderArray = property.order.Split(new string[] { "@@@" }, System.StringSplitOptions.RemoveEmptyEntries);
                    string whereOrder = "";

                    int nivel = 0;
                    foreach (string orderIn in orderArray)
                    {
                        if (nivel == 0)
                        {
                            whereOrder += $"?o <{orderIn}> ?level{nivel}.";
                        }
                        else
                        {
                            whereOrder += $"?level{nivel - 1} <{orderIn}> ?level{nivel}.";
                        }
                        nivel++;
                    }
                    List<List<string>> listOfLists = SplitList(pIds.ToList(), maxIn).ToList();
                    foreach (List<string> list in listOfLists)
                    {
                        int offset = 0;
                        int limit = paginacion;
                        while (limit == paginacion)
                        {
                            string select = @$" select * where
                                                {{
                                                    select distinct ?s ?p ?o ";
                            string where = @$"      where
                                                    {{
                                                        ?s ?p ?o. 
                                                        FILTER( lang(?o) = '{pLang}' OR lang(?o) = '' OR !isLiteral(?o) )  
                                                        OPTIONAL{{{whereOrder}}} 
                                                        FILTER(?s in(<{string.Join(">,<", list.OrderByDescending(x => x))}>)) 
                                                        FILTER(?p =<{property.property}>)
                                                    }} 
                                                    order by asc(?level{nivel - 1}) asc(?o) asc(?p) asc(?s)
                                                }} limit {limit} offset {offset}";
                            string claveCache = select + where + pGraph;
                            SparqlObject sparqlObjectAux = null;
                            if (pDicQueries.ContainsKey(claveCache))
                            {
                                sparqlObjectAux = pDicQueries[claveCache];
                            }
                            else
                            {
                                sparqlObjectAux = mResourceApi.VirtuosoQuery(select, where, pGraph);
                                pDicQueries[claveCache] = sparqlObjectAux;
                            }
                            limit = sparqlObjectAux.results.bindings.Count;
                            offset += sparqlObjectAux.results.bindings.Count;
                            foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObjectAux.results.bindings)
                            {
                                if (!data.ContainsKey(fila["s"].value))
                                {
                                    data.Add(fila["s"].value, new List<Dictionary<string, SparqlObject.Data>>());
                                }
                                data[fila["s"].value].Add(fila);
                            }
                            if (sparqlObject == null)
                            {
                                sparqlObject = sparqlObjectAux;
                            }
                            else
                            {
                                sparqlObject.results.bindings.AddRange(sparqlObjectAux.results.bindings);
                            }
                        }
                    }
                }
            }

            Parallel.ForEach(pProperties, new ParallelOptions { MaxDegreeOfParallelism = 5 }, property =>
            {
                if (property.childs != null && property.childs.Count() > 0 && sparqlObject != null)
                {
                    HashSet<string> ids = new HashSet<string>(sparqlObject.results.bindings.Where(x => x["p"].value == property.property).Select(x => x["o"].value).ToList());
                    Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataAux = GetProperties(ids, property.graph, property.childs.ToList(), pLang, pDicQueries);
                    foreach (string id in dataAux.Keys)
                    {
                        if (!data.ContainsKey(id))
                        {
                            data.Add(id, new List<Dictionary<string, SparqlObject.Data>>());
                        }
                        data[id].AddRange(dataAux[id]);
                    }
                }
            });
            return data;
        }

        /// <summary>
        /// Genera los PropertyData para recuperar los datos a través de los PropertyData configurados en el template
        /// </summary>
        /// <param name="pProp">PropertyData del template</param>
        /// <param name="pPropertyData">PropertyData para recuperar los datos</param>
        /// <param name="pGraph">Grafo</param>
        public static void GenerarPropertyData(PropertyDataTemplate pProp, ref PropertyData pPropertyData, string pGraph)
        {
            if (pProp.child != null)
            {
                PropertyData property = new PropertyData()
                {
                    property = pProp.child.property,
                    childs = new List<PropertyData>(),
                    order = pProp.order
                };
                if (!pPropertyData.childs.Exists(x => x.property == property.property))
                {
                    pPropertyData.childs.Add(property);
                }
                else
                {
                    property = pPropertyData.childs.First(x => x.property == property.property);
                }
                string graphAux = pProp.child.graph;
                if (string.IsNullOrEmpty(graphAux))
                {
                    graphAux = pGraph;
                }
                property.graph = graphAux;
                GenerarPropertyData(pProp.child, ref property, graphAux);
            }
        }

        /// <summary>
        /// Obtiene una propiedad completa (con '@@@' para separar los saltos)
        /// </summary>
        /// <param name="propertyData">Property data del template</param>
        /// <returns></returns>
        public static string GetPropComplete(PropertyDataTemplate propertyData)
        {
            if (propertyData == null)
            {
                return "";
            }
            string propAux = GetPropComplete(propertyData.child);
            if (!string.IsNullOrEmpty(propAux))
            {
                propAux = "@@@" + propAux;
            }
            return propertyData.property + propAux;
        }

        /// <summary>
        /// CAmbia la propiedad añadiendole elprefijo
        /// </summary>
        /// <param name="pProperty">Propiedad con la URL completa</param>
        /// <returns>Url con prefijo</returns>
        public static string AniadirPrefijo(string pProperty)
        {
            KeyValuePair<string, string> prefix = dicPrefix.First(x => pProperty.StartsWith(x.Value));
            return pProperty.Replace(prefix.Value, prefix.Key + ":");
        }

        public static void CleanPropertyData(ref PropertyData pPropertyData)
        {
            if (pPropertyData.childs != null)
            {
                List<PropertyData> childs = new List<PropertyData>();
                for (int i = 0; i < pPropertyData.childs.Count; i++)
                {
                    PropertyData propertyDataActual = pPropertyData.childs[i];
                    PropertyData childProcessed = childs.FirstOrDefault(x => x.property == propertyDataActual.property);
                    if (childProcessed == null)
                    {
                        childs.Add(propertyDataActual);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(childProcessed.order) && !string.IsNullOrEmpty(propertyDataActual.order))
                        {
                            childProcessed.order = propertyDataActual.order;
                        }
                        if (propertyDataActual.childs != null)
                        {
                            childProcessed.childs.AddRange(propertyDataActual.childs);
                        }
                        if (!string.IsNullOrEmpty(propertyDataActual.graph))
                        {
                            childProcessed.graph = propertyDataActual.graph;
                        }

                    }
                }
                pPropertyData.childs = childs;
                for (int i = 0; i < pPropertyData.childs.Count; i++)
                {
                    PropertyData propertyDataActual = pPropertyData.childs[i];
                    CleanPropertyData(ref propertyDataActual);
                }
            }
        }


        public static string GetTextLang(string pLang, Dictionary<string, string> pValores)
        {
            if (pValores == null)
            {
                return "";
            }
            else if (pValores.ContainsKey(pLang))
            {
                return pValores[pLang];
            }
            else if (pValores.ContainsKey("es"))
            {
                return pValores["es"];
            }
            else if (pValores.Count > 0)
            {
                return pValores.Values.First();
            }
            else
            {
                return "";
            }
        }

        public static string GetTextNumber(string pInput)
        {
            string input = pInput.Replace(",", ".");
            string entero = "";
            string decimales = "";
            if(input.Contains("."))
            {
                entero = input.Substring(0, input.IndexOf("."));
                decimales = input.Substring(input.IndexOf(".")+1);
            }else
            {
                entero = input;
            }

            string textNumber = long.Parse(entero, CultureInfo.InvariantCulture).ToString("N0", new System.Globalization.CultureInfo("es-ES"));
            if(!string.IsNullOrEmpty(decimales))
            {
                textNumber += "," + decimales;
            }
            return textNumber;
        }


        /// <summary>
        /// Método para dividir listas
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pItems">Listado</param>
        /// <param name="pSize">Tamaño</param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> pItems, int pSize)
        {
            for (int i = 0; i < pItems.Count; i += pSize)
            {
                yield return pItems.GetRange(i, Math.Min(pSize, pItems.Count - i));
            }
        }



        /// <summary>
        /// Lista de TabTemplates configurados
        /// </summary>
        public static List<Tab> TabTemplates
        {
            get
            {
                if (mTabTemplates == null || mTabTemplates.Count != System.IO.Directory.EnumerateFiles($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/TabTemplates").Count())
                {
                    mTabTemplates = new List<Tab>();
                    foreach (string file in System.IO.Directory.EnumerateFiles($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/TabTemplates"))
                    {
                        Tab tab = JsonConvert.DeserializeObject<Tab>(System.IO.File.ReadAllText(file));
                        mTabTemplates.Add(tab);
                    }
                }
                return mTabTemplates;
            }
        }
    }
}