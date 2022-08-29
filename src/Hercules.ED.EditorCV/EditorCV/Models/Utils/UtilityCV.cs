using EditorCV.Models.API.Templates;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace EditorCV.Models.Utils
{
    public class UtilityCV
    {
        /// <summary>
        /// Propiedad para marcar las entidades como públicas en el CV
        /// </summary>
        public static string PropertyIspublic { get { return "http://w3id.org/roh/isPublic"; } }

        /// <summary>
        /// Propiedad para marcar las entidades con openAccess
        /// </summary>
        public static string PropertyOpenAccess { get { return "http://w3id.org/roh/openAccess"; } }

        /// <summary>
        /// Propiedad para comprobar si no es editable, tiene que tener en alguna propiedad
        /// de las claves algún valor de los valores
        /// </summary>
        public static Dictionary<string, List<string>> PropertyNotEditable = new Dictionary<string, List<string>>()
        {
            { "http://w3id.org/roh/crisIdentifier", new List<string>() },
            { "http://w3id.org/roh/isValidated", new List<string>(){ "true"} },
            { "http://w3id.org/roh/validationStatusPRC", new List<string>(){ "pendiente", "validado" } },
            { "http://w3id.org/roh/validationStatusProject", new List<string>(){ "pendiente", "validado" } }
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

        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static ConcurrentBag<Tab> mTabTemplates;

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
        /// Devuelve el Identificador de CV del usuario con identificador <paramref name="userId"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GetCVFromUser(string userId)
        {
            string select = $@"select ?cv from <{mResourceApi.GraphsUrl}person.owl>";
            string where = $@"where {{
    ?persona a <http://xmlns.com/foaf/0.1/Person> .
    ?persona <http://w3id.org/roh/gnossUser> <http://gnoss/{userId.ToUpper()}> .
    ?cv <http://w3id.org/roh/cvOf> ?persona .
}}";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (fila.ContainsKey("cv"))
                {
                    return fila["cv"].value;
                }
            }

            return "";
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
        public static Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetProperties(HashSet<string> pIds, string pGraph,
            List<PropertyData> pProperties, string pLang, Dictionary<string, SparqlObject> pDicQueries)
        {
            int paginacion = 10000;
            int maxIn = 1000;
            ConcurrentDictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = new ConcurrentDictionary<string, List<Dictionary<string, SparqlObject.Data>>>();
            ConcurrentDictionary<string, SparqlObject> pConcDicQueries = new ConcurrentDictionary<string, SparqlObject>(pDicQueries);


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
                                                   FILTER(?s in(<{string.Join(">,<", list.OrderByDescending(x => x))}>)) 
                                                   FILTER(?p in(<{string.Join(">,<", pProperties.Where(x => string.IsNullOrEmpty(x.order)).Select(x => x.property).ToList().OrderByDescending(x => x))}>))
                                                }} 
                                                order by asc(?o) asc(?p) asc(?s)
                                            }} limit {limit} offset {offset}";
                        string claveCache = select + where + pGraph;
                        SparqlObject sparqlObjectAux = null;
                        if (pConcDicQueries.ContainsKey(claveCache))
                        {
                            sparqlObjectAux = pConcDicQueries[claveCache];
                        }
                        else
                        {
                            sparqlObjectAux = mResourceApi.VirtuosoQuery(select, where, pGraph);
                            pConcDicQueries[claveCache] = sparqlObjectAux;
                        }
                        limit = sparqlObjectAux.results.bindings.Count;
                        offset += sparqlObjectAux.results.bindings.Count;
                        foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObjectAux.results.bindings)
                        {
                            if (!data.ContainsKey(fila["s"].value))
                            {
                                data.AddOrUpdate(fila["s"].value, new List<Dictionary<string, SparqlObject.Data>>(), (s, list) => new List<Dictionary<string, SparqlObject.Data>>());
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
                            if (pConcDicQueries.ContainsKey(claveCache))
                            {
                                sparqlObjectAux = pConcDicQueries[claveCache];
                            }
                            else
                            {
                                sparqlObjectAux = mResourceApi.VirtuosoQuery(select, where, pGraph);
                                pConcDicQueries[claveCache] = sparqlObjectAux;
                            }
                            limit = sparqlObjectAux.results.bindings.Count;
                            offset += sparqlObjectAux.results.bindings.Count;
                            foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObjectAux.results.bindings)
                            {
                                if (!data.ContainsKey(fila["s"].value))
                                {
                                    data.AddOrUpdate(fila["s"].value, new List<Dictionary<string, SparqlObject.Data>>(), (s, list) => new List<Dictionary<string, SparqlObject.Data>>());
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
                    Dictionary<string, SparqlObject> dicAux = new Dictionary<string, SparqlObject>(pConcDicQueries);
                    Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataAux = GetProperties(ids, property.graph, property.childs.ToList(), pLang, dicAux);
                    foreach (string id in dataAux.Keys)
                    {
                        if (!data.ContainsKey(id))
                        {
                            data.AddOrUpdate(id, new List<Dictionary<string, SparqlObject.Data>>(), (s, list) => new List<Dictionary<string, SparqlObject.Data>>());
                        }
                        data[id].AddRange(dataAux[id]);
                    }
                }
            });
            return data.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        /// <summary>
        /// Obtiene las propiedades para los contadores de las entidades pasadas por parámetro
        /// </summary>
        /// <param name="pIds">Identificadores de las entidades de las que recuperar sus propiedades</param>
        /// <param name="pProperties">Propiedades a recuperar</param>   
        /// <param name="pSoloPublicos">Obtiene los datos sólo de lo públicos</param>
        /// <returns></returns>
        public static Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetPropertiesContadores(HashSet<string> pIds,
            List<PropertyData> pProperties,bool pSoloPublicos=false)
        {
            int paginacion = 10000;
            int maxIn = 1000;

            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> respuesta = new Dictionary<string, List<Dictionary<string, Data>>>();

            string queryPublicos = "";
            if (pSoloPublicos)
            {
                queryPublicos = $"?o <{PropertyIspublic}> 'true'. ";
            }

            SparqlObject sparqlObject = null;
            if (pProperties.Count > 0)
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
                                                select distinct ?s ?p ?o ?p2 ?o2 ?relatedBy";
                        string where = @$"      where
                                                {{
                                                    ?s ?p ?o. 
                                                    FILTER(?s in(<{string.Join(">,<", list.OrderByDescending(x => x))}>)) 
                                                    FILTER(?p in(<{string.Join(">,<", pProperties.Select(x => x.property).ToList().OrderByDescending(x => x))}>))
                                                    ?o ?p2 ?o2.
                                                    FILTER(?p2=<http://vivoweb.org/ontology/core#relatedBy>)
                                                    {queryPublicos}
                                                }} 
                                                order by asc(?o) asc(?p) asc(?s)
                                            }} limit {limit} offset {offset}";

                        SparqlObject sparqlObjectAux = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                        limit = sparqlObjectAux.results.bindings.Count;
                        offset += sparqlObjectAux.results.bindings.Count;
                        foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObjectAux.results.bindings)
                        {
                            Dictionary<string, SparqlObject.Data> fila1 = new Dictionary<string, Data>();
                            fila1.Add("s", fila["s"]);
                            fila1.Add("p", fila["p"]);
                            fila1.Add("o", fila["o"]);
                            if (!respuesta.ContainsKey(fila1["s"].value))
                            {
                                respuesta[fila1["s"].value] = new List<Dictionary<string, Data>>();
                            }
                            respuesta[fila1["s"].value].Add(fila1);
                            Dictionary<string, SparqlObject.Data> fila2 = new Dictionary<string, Data>();
                            fila2.Add("s", fila["o"]);
                            fila2.Add("p", fila["p2"]);
                            fila2.Add("o", fila["o2"]);
                            if (!respuesta.ContainsKey(fila2["s"].value))
                            {
                                respuesta[fila2["s"].value] = new List<Dictionary<string, Data>>();
                            }
                            respuesta[fila2["s"].value].Add(fila2);

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
            return respuesta;
        }

        /// <summary>
        /// Obtiene los datos multiidioma de la entidad en el CV
        /// </summary>
        /// <param name="pCV">Identificador del CV</param>
        /// <param name="pId">Identificador de la entidad</param>
        /// <returns>Diccionario con la entidad/propiedad/valores multiidioma Propiedades con los valores multiidioma</returns>
        public static Dictionary<string, Dictionary<string, List<MultilangProperty>>> GetMultilangPropertiesCV(string pCV, string pId)
        {
            //Clave propieddad
            Dictionary<string, Dictionary<string, List<MultilangProperty>>> entidadPropiedadesMultiIdioma = new Dictionary<string, Dictionary<string, List<MultilangProperty>>>();
            try
            {
                string selectID = "select distinct ?entity ?multilangProperties ?prop ?lang ?value ";
                string whereID = $@"where{{
                                            <{pCV}> <http://w3id.org/roh/multilangProperties> ?multilangProperties.
                                            ?multilangProperties <http://w3id.org/roh/entity> ?entity.
                                            FILTER(?entity=<{pId}>).
                                            ?multilangProperties <http://w3id.org/roh/property> ?prop. 
                                            ?multilangProperties <http://w3id.org/roh/lang> ?lang. 
                                            ?multilangProperties <http://w3id.org/roh/value> ?value. 
                                        }}";
                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, "curriculumvitae");
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    string entity = fila["entity"].value;
                    string multilangProperties = fila["multilangProperties"].value;
                    string prop = fila["prop"].value;
                    string lang = fila["lang"].value;
                    string value = fila["value"].value;
                    if (!entidadPropiedadesMultiIdioma.ContainsKey(entity))
                    {
                        entidadPropiedadesMultiIdioma.Add(entity, new Dictionary<string, List<MultilangProperty>>());
                    }
                    if (!entidadPropiedadesMultiIdioma[entity].ContainsKey(prop))
                    {
                        entidadPropiedadesMultiIdioma[entity].Add(prop, new List<MultilangProperty>());
                    }

                    entidadPropiedadesMultiIdioma[entity][prop].Add(new MultilangProperty()
                    {
                        auxEntityCV = multilangProperties,
                        lang = lang,
                        value = value
                    }
                    );
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            return entidadPropiedadesMultiIdioma;
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
            if (input.Contains("."))
            {
                entero = input.Substring(0, input.IndexOf("."));
                decimales = input.Substring(input.IndexOf(".") + 1);
            }
            else
            {
                entero = input;
            }

            string textNumber = long.Parse(entero, CultureInfo.InvariantCulture).ToString("N0", new System.Globalization.CultureInfo("es-ES"));
            if (!string.IsNullOrEmpty(decimales))
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
        /// Método para normalizar los textos
        /// </summary>
        /// <param name="pText">Texto</param>
        /// <returns></returns>
        public static string NormalizeText(string pText)
        {
            string normalizedString = pText.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char charin in normalizedString)
            {
                if (char.IsLetterOrDigit(charin))
                {
                    sb.Append(charin);
                }
            }
            normalizedString = sb.ToString().Normalize(NormalizationForm.FormD).ToLower();
            return normalizedString;
        }



        /// <summary>
        /// Lista de TabTemplates configurados
        /// </summary>
        public static ConcurrentBag<Tab> TabTemplates
        {
            get
            {
                if (mTabTemplates == null || mTabTemplates.Count != System.IO.Directory.EnumerateFiles($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/TabTemplates").Count())
                {
                    ConcurrentBag<Tab> aux = new ConcurrentBag<Tab>();
                    foreach (string file in System.IO.Directory.EnumerateFiles($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/TabTemplates").OrderByDescending(x => x))
                    {
                        Tab tab = JsonConvert.DeserializeObject<Tab>(System.IO.File.ReadAllText(file));
                        aux.Add(tab);
                    }
                    mTabTemplates = aux;
                }
                return mTabTemplates;
            }
        }
    }
}