using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using EditorCV.Models.API.Input;
using EditorCV.Models.API.Response;
using EditorCV.Models.API.Templates;
using EditorCV.Models.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using EditorCV.Models.Enrichment;
using EditorCV.Controllers;
using static EditorCV.Models.Enrichment.EnrichmentResponse;
using Hercules.ED.DisambiguationEngine.Models;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using EditorCV.Models.Similarity;

namespace EditorCV.Models
{
    /// <summary>
    /// Clase utilizada para las acciones de recuperación de datos de edición
    /// </summary>
    public class AccionesEdicion
    {
        /// <summary>
        /// API
        /// </summary>
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static readonly CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        private static Tuple<Dictionary<string, string>, Dictionary<string, string>> tuplaTesauro;

        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> dicAutocompletar = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        private static Dictionary<string, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>>> dicCombos = new Dictionary<string, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>>>();
        private static Dictionary<string, List<ThesaurusItem>> dicTesauros = new Dictionary<string, List<ThesaurusItem>>();





        #region Métodos públicos

        /// <summary>
        /// Obtiene un listado de sugerencias con datos existentes para esa propiedad
        /// </summary>
        /// <returns></returns>
        public string GetCVUrl(string userID, string lang)
        {
            string cv = UtilityCV.GetCVFromUser(userID);
            List<ResponseGetUrl> urlList = mResourceApi.GetUrl(new List<Guid>() { mResourceApi.GetShortGuid(cv) }, lang);
            if (urlList.Count > 0)
            {
                return urlList.First(x => x.resource_id == mResourceApi.GetShortGuid(cv)).url;
            }
            return "";
        }

        /// <summary>
        /// Obtiene un listado de sugerencias con datos existentes para esa propiedad
        /// </summary>
        /// <param name="pSearch">Texto por el que se van a buscar sugerencias</param>
        /// <param name="pProperty">Propiedad en la que se quiere buscar</param>
        /// <param name="pPropertiesAux">Propiedades auxiliares que se quieren buscar</param>
        /// <param name="pPrint">Formato de como pintar</param>
        /// <param name="pRdfType">Rdf:type de la entidad en la que se quiere buscar</param>
        /// <param name="pGraph">Grafo en el que se encuentra la propiedad</param>
        /// <param name="pGetEntityID">Obtiene el ID de la entidad además del valor de la propiedad</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pCache">Indica si hay que cachear</param>
        /// <returns></returns>
        public object GetAutocomplete(string pSearch, string pProperty, List<string> pPropertiesAux, string pPrint, string pRdfType, string pGraph, bool pGetEntityID, List<string> pLista, string pLang, bool pCache)
        {
            int numMax = 20;
            string searchText = pSearch.Trim();

            //Subdivido las propiedades, en caso de venir varias, y las concateno para la consulta.
            string pPropertyAux = "";
            string[] pPropertySplit = pProperty.Split("|");
            int contador = 0;
            bool inicio = true;
            foreach (string pProp in pPropertySplit)
            {
                if (inicio)
                {
                    pPropertyAux = pProp + "> ?o" + contador + " . ";
                    inicio = !inicio;
                    continue;
                }
                pPropertyAux += " ?o" + contador + " <" + pProp + "> ?o";
                contador++;
                pPropertyAux += contador + " . ";
            }
            pPropertyAux = pPropertyAux.Substring(0, pPropertyAux.Length - 8);

            if (pCache)
            {
                Dictionary<string, string> dicBuscar = new Dictionary<string, string>();
                string claveAutocompletar = $"{pProperty}{pRdfType}{pGraph}";
                if (dicAutocompletar.ContainsKey(claveAutocompletar) && dicAutocompletar[claveAutocompletar].ContainsKey(pLang))
                {
                    dicBuscar = dicAutocompletar[claveAutocompletar][pLang];
                }
                else
                {
                    int limit = 10000;
                    int offset = 0;
                    while (true)
                    {
                        Dictionary<string, string> dicValores = new Dictionary<string, string>();
                        string select = "SELECT * WHERE { SELECT DISTINCT ?s ?o  ";
                        string where = $@"WHERE {{
                                                            ?s a <{pRdfType}>.  ?s <{pPropertyAux}> ?o . FILTER( lang(?o) = '{pLang}' OR lang(?o) = '')                          
                                                        }} ORDER BY DESC(?o) DESC (?s) }} LIMIT {limit} OFFSET {offset}";
                        SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, pGraph);
                        if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                        {
                            offset += limit;
                            foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                            {
                                dicValores[fila["s"].value] = fila["o"].value;
                            }
                            if (resultadoQuery.results.bindings.Count < limit)
                            {
                                if (!dicAutocompletar.ContainsKey(claveAutocompletar))
                                {
                                    dicAutocompletar[claveAutocompletar] = new Dictionary<string, Dictionary<string, string>>();
                                }
                                if (!dicAutocompletar[claveAutocompletar].ContainsKey(pLang))
                                {
                                    dicAutocompletar[claveAutocompletar][pLang] = new Dictionary<string, string>();
                                }
                                dicAutocompletar[claveAutocompletar][pLang] = dicValores;
                                dicBuscar = dicAutocompletar[claveAutocompletar][pLang];
                                break;
                            }
                        }
                    }
                }
                searchText = UtilityCV.NormalizeText(searchText);
                if (!pGetEntityID)
                {
                    var resultados = dicBuscar.Values.Where(x => UtilityCV.NormalizeText(x).Contains(searchText)).Distinct();
                    if (pLista != null)
                    {
                        resultados = resultados.Except(pLista, StringComparer.OrdinalIgnoreCase);
                    }
                    if (resultados.Count() > numMax)
                    {
                        resultados = resultados.ToList().GetRange(0, numMax);
                    }
                    return resultados.ToList();
                }
                else
                {
                    Dictionary<string, string> respuesta = new Dictionary<string, string>();
                    int i = 0;
                    foreach (KeyValuePair<string, string> fila in dicBuscar.Where(x => UtilityCV.NormalizeText(x.Value).Contains(searchText)))
                    {
                        i++;
                        if (i > numMax)
                        {
                            break;
                        }
                        string s = fila.Key;
                        string o = fila.Value;
                        if (pLista == null || respuesta.Keys.Intersect(pLista).Count() == 0)
                        {
                            respuesta.Add(s, o);
                        }
                    }
                    return respuesta;
                }
            }
            else
            {
                string filter = "";
                if (!pSearch.EndsWith(' '))
                {
                    string[] splitSearch = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (splitSearch.Length > 1)
                    {
                        searchText = searchText.Substring(0, searchText.LastIndexOf(' '));
                        if (splitSearch.Last().Length > 3)
                        {
                            searchText += " " + splitSearch.Last() + "*";
                        }
                        else
                        {
                            filter = $" AND lcase(?o) like \"% {splitSearch.Last()}%\" ";
                        }
                    }
                    else if (searchText.Length > 3)
                    {
                        searchText += "*";
                    }
                    else // Si tiene menos de 4 caracteres y no termina en espacio, buscamos por like
                    {
                        filter = $"  lcase(?o) like \"{searchText}%\" OR lcase(?o) like \"% {searchText}%\" ";
                        searchText = "";
                    }
                }
                if (searchText != "")
                {
                    filter = $"bif:contains(?o, \"'{searchText}'\"){filter}";
                }
                string select = "SELECT DISTINCT ?s ?o ";
                string auxProperties = "";
                if (pPropertiesAux != null && pPropertiesAux.Count() > 0 && !string.IsNullOrEmpty(pPrint))
                {
                    for (int i = 0; i < pPropertiesAux.Count(); i++)
                    {
                        select += " ?o" + (i + 1);
                        auxProperties += $"OPTIONAL{{ ?s <{pPropertiesAux[i]}> ?o{i + 1}.}}";
                    }

                }
                string where = $"WHERE {{ ?s a <{pRdfType}>. ?s <{pPropertyAux}> ?o. {auxProperties} FILTER( {filter} ) FILTER( lang(?o) = '{pLang}' OR lang(?o) = '')   }}ORDER BY ASC(strlen(?o)) ASC (?o)";
                SparqlObject sparqlObjectAux = mResourceApi.VirtuosoQuery(select, where, pGraph);
                if (!pGetEntityID)
                {
                    var resultados = sparqlObjectAux.results.bindings.Select(x => x["o"].value).Distinct();
                    if (pLista != null)
                    {
                        resultados = resultados.Except(pLista, StringComparer.OrdinalIgnoreCase);
                    }
                    if (resultados.Count() > numMax)
                    {
                        resultados = resultados.ToList().GetRange(0, numMax);
                    }
                    return resultados.ToList();
                }
                else
                {
                    Dictionary<string, string> respuesta = new Dictionary<string, string>();
                    int i = 0;
                    foreach (Dictionary<string, Data> fila in sparqlObjectAux.results.bindings)
                    {
                        i++;
                        if (i > numMax)
                        {
                            break;
                        }
                        string s = fila["s"].value;
                        string o = fila["o"].value;
                        if (pPropertiesAux != null && pPropertyAux.Count() > 0 && !string.IsNullOrEmpty(pPrint))
                        {
                            o = "";
                            string[] printSplit = pPrint.Split('|');
                            for (int j = 0; j < printSplit.Count(); j++)
                            {
                                string valor = "";
                                if (j == 0)
                                {
                                    valor = fila["o"].value;
                                }
                                else if (fila.ContainsKey("o" + j))
                                {
                                    valor = fila["o" + j].value;
                                }
                                if (!string.IsNullOrEmpty(valor))
                                {
                                    o += printSplit[j].Replace($"{{{j}}}", valor);
                                }
                            }
                        }

                        if (pLista == null || respuesta.Keys.Intersect(pLista).Count() == 0)
                        {
                            respuesta.Add(s, o);
                        }
                    }
                    return respuesta;
                }
            }
        }

        /// <summary>
        /// Obtiene datos de una entidad
        /// </summary>
        /// <param name="pGraph">Grafo</param>
        /// <param name="pEntity">Entidad de la que obtener datos</param>
        /// <param name="pProperties">Propiedades a obtener</param>
        /// <returns></returns>
        public Dictionary<string, string> GetPropertyEntityData(string pGraph, string pEntity, List<string> pProperties)
        {
            Dictionary<string, string> dicValores = new Dictionary<string, string>();

            string select = "SELECT *  ";
            string where = $@"WHERE {{
                                        ?s a ?rdfType.  
                                        ?s ?p ?o.
                                        FILTER(?s=<{pEntity}>)
                                        FILTER(?p in (<{string.Join(">,<", pProperties)}>))
                                    }}";
            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, pGraph);
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    dicValores[fila["p"].value] = fila["o"].value;
                }
            }
            return dicValores;
        }

        /// <summary>
        /// Funcion para obtener los elementos con duplicidad.
        /// </summary>
        /// <param name="pCVId">ID del CV a tratar</param>
        /// <param name="minSimilarity">Indica el porcentaje de similitud que tienen los items duplicados</param>
        /// <returns>
        /// Un diccionario que tiene el titulo como llave y una lista contiendo las ids de todas las veces que aparece ese titulo.
        /// </returns>
        public List<SimilarityResponse> GetItemsDuplicados(string pCVId, float minSimilarity, string pItemId = null)
        {
            Dictionary<string, HashSet<string>> itemsNoDuplicados = new Dictionary<string, HashSet<string>>();
            string select = $@"SELECT distinct ?group ?id";
            string where = $@"where
                            {{ 
                                <{pCVId}> <http://w3id.org/roh/noDuplicateGroup> ?group.
                                ?group <http://w3id.org/roh/noDuplicateId> ?id.
                            }}";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
            {
                if (!itemsNoDuplicados.ContainsKey(fila["group"].value))
                {
                    itemsNoDuplicados.Add(fila["group"].value, new HashSet<string>());
                }
                itemsNoDuplicados[fila["group"].value].Add(fila["id"].value);
            }


            List<SimilarityResponse> listSimilarity = new List<SimilarityResponse>();

            foreach (API.Templates.Tab tab in UtilityCV.TabTemplates)
            {
                if (tab.sections != null)
                {
                    foreach (API.Templates.TabSection tabSection in tab.sections)
                    {
                        if (tabSection.presentation.listItemsPresentation != null && tabSection.presentation.listItemsPresentation.checkDuplicates)
                        {
                            List<HashSet<string>> similars = new List<HashSet<string>>();

                            Dictionary<string, Tuple<string, string, bool>> itemsTitleValidatedSection = GetItemsTitleParaDuplicados(pCVId, tab.property, tabSection.property, tabSection.presentation.listItemsPresentation.listItemEdit.graph, tabSection.presentation.listItemsPresentation.listItemEdit.proptitle);

                            List<KeyValuePair<string, Tuple<string, string, bool>>> itemsTitleSectionList = itemsTitleValidatedSection.ToList();
                            for (int i = 0; i < itemsTitleValidatedSection.Count; i++)
                            {
                                if (pItemId != null && itemsTitleSectionList[i].Key != pItemId)
                                {
                                    continue;
                                }
                                Dictionary<string, bool> similarsin = new Dictionary<string, bool>();
                                similarsin[itemsTitleSectionList[i].Key] = itemsTitleSectionList[i].Value.Item3;
                                for (int j = i + 1; j < itemsTitleValidatedSection.Count; j++)
                                {
                                    double similitud = Similarity(itemsTitleSectionList[i].Value.Item2, itemsTitleSectionList[j].Value.Item2, minSimilarity);
                                    if (similitud > minSimilarity)
                                    {
                                        similarsin[itemsTitleSectionList[j].Key] = itemsTitleSectionList[j].Value.Item3;
                                    }
                                }
                                //Eliminamos si hay alguno duplicado
                                foreach (string duplicado in similars.SelectMany(x => x))
                                {
                                    if (similarsin.ContainsKey(duplicado))
                                    {
                                        similarsin.Remove(duplicado);
                                    }
                                }

                                //Ordenamos primero con los validados
                                similarsin = similarsin.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                                ////Sólo puede ser validado el primero
                                //if (similarsin.Count > 1)
                                //{
                                //    foreach (string id in similarsin.Keys.ToList().GetRange(1, similarsin.Keys.Count - 1))
                                //    {
                                //        if (similarsin[id])
                                //        {
                                //            similarsin.Remove(id);
                                //        }
                                //    }
                                //}
                                if (similarsin.Count > 1)
                                {
                                    //Si hay mas de uno y hay alguno no validado lo añadimos
                                    similars.Add(new HashSet<string>(similarsin.Keys));
                                }
                            }
                            //Eliminamos de los similares aquellos que estén marcados como no duplicados
                            foreach (HashSet<string> sim in similars.ToList())
                            {
                                string principalAux = sim.ToList()[0];
                                string principal = itemsTitleValidatedSection[principalAux].Item1;
                                List<string> noDuplicados = itemsNoDuplicados.Values.Where(x => x.Contains(principal)).ToList().SelectMany(x => x).Distinct().ToList().Except(new List<string>() { principal }).ToList();
                                foreach (string idAux in sim.ToList())
                                {
                                    string id = itemsTitleValidatedSection[idAux].Item1;
                                    if (idAux != principalAux && noDuplicados.Contains(id))
                                    {
                                        sim.Remove(idAux);
                                    }
                                }
                            }

                            //Eliminamos las entradas que aparezcan más de una vez
                            HashSet<string> procesado = new HashSet<string>();
                            foreach (HashSet<string> sim in similars.ToList())
                            {
                                foreach (string idAux in sim.ToList())
                                {
                                    if (!procesado.Add(idAux))
                                    {
                                        sim.Remove(idAux);
                                    }
                                }
                            }

                            //NOs quedamos sólo con aquellos que tengan más de uno
                            similars = similars.Where(x => x.Count > 1).ToList();
                            foreach (HashSet<string> sim in similars)
                            {
                                SimilarityResponse similarityResponse = new SimilarityResponse();
                                similarityResponse.idSection = tabSection.property;
                                similarityResponse.rdfTypeTab = tab.rdftype;
                                similarityResponse.items = sim;
                                listSimilarity.Add(similarityResponse);
                            }

                        }
                    }
                }
            }

            return listSimilarity;
        }

        public Dictionary<string, Tuple<string, string, bool>> GetItemsTitleParaDuplicados(string pCV, string pTabProperty, string pSectionProperty, string pGraph, string pPropTitle)
        {
            Dictionary<string, Tuple<string, string, bool>> itemTitle = new Dictionary<string, Tuple<string, string, bool>>();
            int limit = 10000;
            int offset = 0;
            while (true)
            {
                string select = $@"SELECT distinct ?itemSection ?item ?title ?validated from <{mResourceApi.GraphsUrl}{pGraph}.owl>";
                string where = $@"where
                            {{ 
                                <{pCV}> <{pTabProperty}> ?tab.
                                ?tab <{pSectionProperty}> ?itemSection.
                                ?itemSection <http://vivoweb.org/ontology/core#relatedBy> ?item .
                                ?item <{pPropTitle}> ?title.
                                OPTIONAL{{?item <http://w3id.org/roh/isValidated> ?validated}}
                            }} LIMIT {limit} OFFSET {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                {
                    bool validated = false;
                    if (fila.ContainsKey("validated") && fila["validated"].value.ToLower() == "true")
                    {
                        validated = true;
                    }
                    itemTitle[fila["itemSection"].value] = new Tuple<string, string, bool>(fila["item"].value, fila["title"].value.ToLower(), validated);
                }
                offset += limit;
                if (sparqlObject.results.bindings.Count < limit)
                {
                    break;
                }
            }
            return itemTitle;
        }

        /// <summary>
        /// Similaridad entre dos string
        /// </summary>
        /// <param name="x">string A</param>
        /// <param name="y">string B</param>
        /// <param name="min">Similitud minima</param>
        /// <returns></returns>
        private double Similarity(string x, string y, float min)
        {
            if (x == null || y == null)
            {
                throw new ArgumentException("Strings must not be null");
            }

            double maxLength = Math.Max(x.Length, y.Length);
            double minLength = Math.Min(x.Length, y.Length);
            if (1 - ((maxLength - minLength) / maxLength) < min)
            {
                return 0;
            }
            if (maxLength > 0)
            {
                int maxEditDistance = ((int)(maxLength - (min * maxLength))) + 1;
                // opcionalmente ignora el caso si es necesario
                return (maxLength - GetEditDistance(x, y, maxEditDistance)) / maxLength;
            }
            return 1.0;
        }

        /// <summary>
        /// Distancia de edición entre dos string
        /// </summary>
        /// <param name="X">string A</param>
        /// <param name="Y">string B</param>
        /// <returns></returns>
        public static int GetEditDistance(string X, string Y, int maxEditDistance)
        {
            int m = X.Length;
            int n = Y.Length;

            int[][] T = new int[m + 1][];
            for (int i = 0; i < m + 1; ++i)
            {
                T[i] = new int[n + 1];
            }
            for (int i = 1; i <= m; i++)
            {
                T[i][0] = i;
            }
            for (int j = 1; j <= n; j++)
            {
                T[0][j] = j;
            }

            int cost;
            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    cost = X[i - 1] == Y[j - 1] ? 0 : 1;
                    T[i][j] = Math.Min(Math.Min(T[i - 1][j] + 1, T[i][j - 1] + 1),
                            T[i - 1][j - 1] + cost);
                    if (i == j && T[i][j] > maxEditDistance)
                    {
                        return Math.Min(X.Length, Y.Length);
                    }
                }
            }
            return T[m][n];
        }

        /// <summary>
        /// Obtiene una sección de pestaña del CV
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pRdfType">Rdf:type de la entidad de la sección</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <param name="pSection">Sección</param>
        /// <returns></returns>
        public AuxTab GetTab(ConfigService pConfig, string pCVId, string pId, string pRdfType, string pLang, string pSection = null, bool pOnlyPublic = false)
        {
            //Obtenemos el template
            API.Templates.Tab template = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfType);
            AuxTab respuesta = null;
            if (!template.personalData)
            {
                //Obtenemos los datos necesarios para el pintado
                Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetTabData(pId, template, pLang, pSection);
                //Obtenemos el modelo para devolver
                respuesta = GetTabModel(pConfig, pCVId, pId, data, template, pLang, pSection, pOnlyPublic);
            }
            else
            {
                respuesta = GetEditModel(pCVId, pId, template.personalDataSections, pLang);
            }
            respuesta.title = UtilityCV.GetTextLang(pLang, template.title);
            return respuesta;
        }

        /// <summary>
        /// Obtiene todas las secciones y pestañas públicas del usuario
        /// </summary>
        /// <param name="pPersonID">Identificador de la persona</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <returns></returns>
        public List<API.Response.Tab> GetAllPublicData(ConfigService pConfig, string pPersonId, string pLang)
        {
            List<string> tabsPublic = new List<string>();
            tabsPublic.Add("http://w3id.org/roh/professionalSituation");
            tabsPublic.Add("http://w3id.org/roh/qualifications");
            tabsPublic.Add("http://w3id.org/roh/teachingExperience");
            tabsPublic.Add("http://w3id.org/roh/scientificExperience");
            tabsPublic.Add("http://w3id.org/roh/scientificActivity");

            //Obtenemos el template
            List<API.Templates.Tab> templates = UtilityCV.TabTemplates.Where(x => tabsPublic.Contains(x.property)).ToList();
            List<API.Response.Tab> respuestas = new List<API.Response.Tab>();
            //Obtenemos los datos necesarios para el pintado
            // Consigo el pId y el pCvId a partir del Id de la persona pPersonId
            string pCVId = "";
            Dictionary<string,Tuple<string, string>> pId = new Dictionary<string, Tuple<string, string>>();
            string select = "SELECT ?cv ?property ?id ?rdftype";
            string where = @$"WHERE {{
                    ?cv<http://w3id.org/roh/cvOf> <{pPersonId}>.
                    ?cv ?property ?id.
                    ?id <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?rdftype.
                    FILTER(?property in (<{string.Join(">,<",tabsPublic)}>))
                }}";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
            {
                pCVId = fila["cv"].value;
                pId[fila["property"].value] = new Tuple<string, string>(fila["id"].value, fila["rdftype"].value);
            }
            if (!string.IsNullOrEmpty(pCVId))
            {
                foreach (API.Templates.Tab template in templates)
                {
                    API.Response.Tab respuesta = null;
                    Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetTabData(pId[template.property].Item1, template, pLang, null, true);
                    //Obtenemos el modelo para devolver
                    respuesta = GetTabModel(pConfig, pCVId, pId[template.property].Item1, data, template, pLang, null, true);
                    respuesta.title = UtilityCV.GetTextLang(pLang, template.title);
                    respuesta.rdftypeTab = pId[template.property].Item2;
                    respuesta.entityIDTab = pId[template.property].Item1;
                    respuesta.sections.RemoveAll(x => x.items == null || x.items.Count == 0);
                    if (respuesta.sections.Count > 0)
                    {
                        respuestas.Add(respuesta);
                    }
                }
            }
            return respuestas;
        }

        /// <summary>
        /// Obtiene una minificha de una entidad de un listado
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntity">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        public TabSectionItem GetItemMini(ConfigService pConfig, string pCVId, string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            TabSectionPresentationListItems presentationListItem = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation.listItemsPresentation;
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetItemMiniData(pEntityID, presentationListItem.listItem, pLang);
            Dictionary<string, Dictionary<string, HashSet<string>>> entidadesMultiidioma = GetMultilangDataCV(pCVId);
            Dictionary<string, HashSet<string>> propiedadesMultiIdiomaCargadas = new Dictionary<string, HashSet<string>>();
            if (entidadesMultiidioma.ContainsKey(pEntityID))
            {
                propiedadesMultiIdiomaCargadas = entidadesMultiidioma[pEntityID];
            }
            List<ItemEditSectionRowProperty> listaPropiedadesConfiguradas = presentationListItem.listItemEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).Where(x => x.multilang).ToList();
            return GetItem(pConfig, pEntityID, data, presentationListItem, pLang, propiedadesMultiIdiomaCargadas, listaPropiedadesConfiguradas);
        }


        /// <summary>
        /// Obtiene los datos de edición de una entidad
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntityID">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        public EntityEdit GetEdit(string pCVId, string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            TabSectionPresentation tabSectionPresentation = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation;
            ItemEdit templateEdit = null;
            string property = "";
            if (tabSectionPresentation.listItemsPresentation != null)
            {
                templateEdit = tabSectionPresentation.listItemsPresentation.listItemEdit;
                property = tabSectionPresentation.listItemsPresentation.property;
            }
            else if (tabSectionPresentation.itemPresentation != null)
            {
                templateEdit = tabSectionPresentation.itemPresentation.itemEdit;
                property = tabSectionPresentation.itemPresentation.property;
            }
            string entityID = pEntityID;

            //obtenemos la entidad correspondiente
            List<PropertyData> propertyData = new List<PropertyData>() { new PropertyData() { property = property } };
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataAux = UtilityCV.GetProperties(new HashSet<string>() { pEntityID }, "curriculumvitae", propertyData, pLang, new Dictionary<string, SparqlObject>());
            if (pEntityID != null && dataAux.ContainsKey(pEntityID))
            {
                entityID = dataAux[pEntityID].First()["o"].value;
            }
            if (tabSectionPresentation.listItemsPresentation != null && !string.IsNullOrEmpty(tabSectionPresentation.listItemsPresentation.property_cv))
            {
                return GetEditModel(pCVId, entityID, templateEdit, pLang, pEntityID, tabSectionPresentation.listItemsPresentation.property_cv);
            }
            else
            {
                return GetEditModel(pCVId, entityID, templateEdit, pLang, pEntityID);
            }
        }

        /// <summary>
        /// Obtiene todos los combos de edición que hay configurados en una serie de propiedades
        /// </summary>
        /// <param name="pProperties"></param>
        /// <returns></returns>
        private List<ItemEditSectionRowPropertyCombo> GetEditCombos(List<ItemEditSectionRowProperty> pProperties)
        {
            List<ItemEditSectionRowPropertyCombo> listCombosConfig = new List<ItemEditSectionRowPropertyCombo>();
            foreach (ItemEditSectionRowPropertyCombo comboConfig in pProperties.Select(x => x.combo).Where(x => x != null))
            {
                if (!listCombosConfig.Exists(x =>
                        UtilityCV.GetPropComplete(x.property) == UtilityCV.GetPropComplete(comboConfig.property) &&
                        x.graph == comboConfig.graph &&
                        x.rdftype == comboConfig.rdftype &&
                        x.filter == comboConfig.filter
                    ))
                {

                    listCombosConfig.Add(comboConfig);
                }
            }
            foreach (ItemEditSectionRowProperty rowProperty in pProperties)
            {
                if (rowProperty.auxEntityData != null && rowProperty.auxEntityData.rows != null)
                {
                    foreach (ItemEditSectionRow row in rowProperty.auxEntityData.rows)
                    {
                        List<ItemEditSectionRowPropertyCombo> aux = GetEditCombos(row.properties);
                        foreach (ItemEditSectionRowPropertyCombo comboConfig in aux)
                        {
                            if (!listCombosConfig.Exists(x =>
                                    UtilityCV.GetPropComplete(x.property) == UtilityCV.GetPropComplete(comboConfig.property) &&
                                    x.graph == comboConfig.graph &&
                                    x.rdftype == comboConfig.rdftype &&
                                    x.filter == comboConfig.filter
                                ))
                            {

                                listCombosConfig.Add(comboConfig);
                            }
                        }
                    }
                }
            }
            return listCombosConfig;
        }

        public List<string> ConseguirNombreTesauro(string tesaurus)
        {
            List<string> listadoTesauros = tesaurus.Split("|||").ToList();
            List<ItemEditSectionRowProperty> listadoTesaurosItem = new List<ItemEditSectionRowProperty>();
            foreach (string tesauro in listadoTesauros)
            {
                listadoTesaurosItem.Add(new ItemEditSectionRowProperty() { thesaurus = tesauro });
            }
            return GetEditThesaurus(listadoTesaurosItem);
        }

        /// <summary>
        /// Obtiene todos los tesauros de edición que hay configurados en una serie de propiedades
        /// </summary>
        /// <param name="pProperties"></param>
        /// <returns></returns>
        private List<string> GetEditThesaurus(List<ItemEditSectionRowProperty> pProperties)
        {
            HashSet<string> listThesaurusConfig = new HashSet<string>();
            foreach (string thesaurusConfig in pProperties.Select(x => x.thesaurus).Where(x => x != null))
            {
                listThesaurusConfig.Add(thesaurusConfig);
            }

            foreach (ItemEditSectionRowProperty rowProperty in pProperties)
            {
                if (rowProperty.auxEntityData != null && rowProperty.auxEntityData.rows != null)
                {
                    foreach (ItemEditSectionRow row in rowProperty.auxEntityData.rows)
                    {
                        listThesaurusConfig.UnionWith(GetEditThesaurus(row.properties));
                    }
                }
            }
            return listThesaurusConfig.ToList();
        }

        public ItemsLoad LoadProps(ItemsLoad pItemsLoad, string pLang)
        {
            if (pItemsLoad.items != null && pItemsLoad.items.Count > 0)
            {
                foreach (LoadProp loadProp in pItemsLoad.items)
                {
                    KeyValuePair<string, PropertyData> propertyData = loadProp.GenerarPropertyData();
                    Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = UtilityCV.GetProperties(new HashSet<string>() { loadProp.id }, propertyData.Key, new List<PropertyData>() { propertyData.Value }, pLang, new Dictionary<string, SparqlObject>());
                    loadProp.values = GetPropValues(loadProp.id, loadProp.GetPropComplete(), data);
                }
            }
            return pItemsLoad;
        }

        public Dictionary<string, List<Person>> ValidateSignatures(string pSignatures, string pCVID, string pPersonID, string pLang)
        {
            Disambiguation.mResourceApi = mResourceApi;
            Dictionary<string, List<Person>> listaPersonas = new Dictionary<string, List<Person>>();

            if (!string.IsNullOrEmpty(pSignatures))
            {
                float scoreDocument = 0.1f;
                float scoreProject = 0.1f;
                float scoreDepartment = 0.2f;
                Dictionary<string, int> colaboradoresDocumentos = ObtenerColaboradoresPublicaciones(pPersonID);
                Dictionary<string, int> colaboradoresProyectos = ObtenerColaboradoresProyectos(pPersonID);
                HashSet<string> colaboradoresDepartament = ObtenerColaboradoresDepartamento(pPersonID);

                List<string> signaturesList = pSignatures.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Distinct().Select(x => x.Trim()).ToList();
                Dictionary<string, List<Person>> listaPersonasAux = new Dictionary<string, List<Person>>();
                Parallel.ForEach(signaturesList, new ParallelOptions { MaxDegreeOfParallelism = 5 }, firma =>
                {
                    if (firma.Trim() != "")
                    {
                        List<Person> personasBBDD = ObtenerPersonasFirma(firma.Trim());
                        Person personaActual = new Person();
                        personaActual.name = firma.Trim();
                        personaActual.ID = Guid.NewGuid().ToString();
                        List<DisambiguableEntity> entidadesActuales = new List<DisambiguableEntity>();
                        entidadesActuales.Add(personaActual);
                        List<DisambiguableEntity> entidadesBBDD = new List<DisambiguableEntity>();
                        foreach (Person personBBDD in personasBBDD)
                        {
                            personBBDD.ID = personBBDD.personid;
                            entidadesBBDD.Add(personBBDD);
                        }
                        Dictionary<string, Dictionary<string, float>> resultadoSimilaridad = Disambiguation.SimilarityBBDDScores(entidadesActuales, entidadesBBDD, 0, 0.5f);
                        foreach (string idBBDD in resultadoSimilaridad[personaActual.ID].Keys)
                        {
                            personasBBDD.First(x => x.ID == idBBDD).score = resultadoSimilaridad[personaActual.ID][idBBDD];
                        }
                        //Ordenamos y nos quedamos sólo con asl que tengan algo de score
                        personasBBDD = personasBBDD.Where(x => x.score > 0f).OrderByDescending(x => x.score).ToList();

                        foreach (Person person in personasBBDD)
                        {
                            float max = person.score + (1 - person.score) * person.score;
                            if (colaboradoresDocumentos.ContainsKey(person.ID))
                            {
                                for (int i = 0; i < colaboradoresDocumentos[person.ID]; i++)
                                {
                                    person.score += (max - person.score) * scoreDocument;
                                }
                            }
                            if (colaboradoresProyectos.ContainsKey(person.ID))
                            {
                                for (int i = 0; i < colaboradoresProyectos[person.ID]; i++)
                                {

                                    person.score += (max - person.score) * scoreProject;
                                }
                            }
                            if (colaboradoresDepartament.Contains(person.ID))
                            {
                                person.score += (max - person.score) * scoreDepartment;
                            }
                            if (person.score > max)
                            {
                                person.score = max;
                            }
                        }
                        personasBBDD = personasBBDD.OrderByDescending(x => x.score).ToList();
                        if (personasBBDD.Count > 20)
                        {
                            personasBBDD = personasBBDD.GetRange(0, 20);
                        }

                        listaPersonasAux[firma.Trim()] = personasBBDD;
                    }
                });
                foreach (string firma in signaturesList)
                {
                    listaPersonas.Add(firma, listaPersonasAux[firma]);
                }
            }

            List<Guid> listaIDs = listaPersonas.SelectMany(x => x.Value).Select(x => mResourceApi.GetShortGuid(x.personid)).Distinct().ToList();
            if (listaIDs.Count > 0)
            {
                List<ResponseGetUrl> urls = mResourceApi.GetUrl(listaPersonas.SelectMany(x => x.Value).Select(x => mResourceApi.GetShortGuid(x.personid)).Distinct().ToList(), pLang);
                foreach (string key in listaPersonas.Keys)
                {
                    foreach (Person person in listaPersonas[key])
                    {
                        person.url = urls.First(x => x.resource_id == mResourceApi.GetShortGuid(person.personid)).url;
                    }
                }
            }
            return listaPersonas;
        }

        public Dictionary<string, int> ObtenerColaboradoresPublicaciones(string pPersonID)
        {
            Dictionary<string, int> colaboradoresPublicaciones = new Dictionary<string, int>();
            int limit = 10000;
            int offset = 0;
            while (true)
            {
                string select = $@"SELECT * WHERE {{select ?personOtherID count(distinct ?s) as ?num ";
                string where = $@"where
                            {{ 
                                ?s a <http://purl.org/ontology/bibo/Document>.
                                ?s <http://purl.org/ontology/bibo/authorList> ?author.     
                                ?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> <{pPersonID}>.
                                ?s <http://purl.org/ontology/bibo/authorList> ?authorOther.     
                                ?authorOther <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?personOtherID.                                
                            }}order by desc(?num) desc (?personOtherID) }} LIMIT {limit} OFFSET {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "document");
                offset += limit;
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                {
                    colaboradoresPublicaciones[fila["personOtherID"].value] = int.Parse(fila["num"].value);
                }
                if (sparqlObject.results.bindings.Count < limit)
                {
                    break;
                }
            }
            return colaboradoresPublicaciones;
        }

        public Dictionary<string, int> ObtenerColaboradoresProyectos(string pPersonID)
        {
            Dictionary<string, int> colaboradoresProyectos = new Dictionary<string, int>();
            int limit = 10000;
            int offset = 0;
            while (true)
            {
                string select = $@"SELECT * WHERE {{select ?personOtherID count(distinct ?s) as ?num ";
                string where = $@"where
                            {{ 
                                ?s a <http://vivoweb.org/ontology/core#Project>.
                                ?s <http://vivoweb.org/ontology/core#relates> ?miembro.     
                                ?miembro <http://w3id.org/roh/roleOf> <{pPersonID}>.
                                ?s <http://vivoweb.org/ontology/core#relates> ?miembroOther.     
                                ?miembroOther  <http://w3id.org/roh/roleOf> ?personOtherID.                                
                            }}order by desc(?num) desc (?personOtherID) }} LIMIT {limit} OFFSET {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "project");
                offset += limit;
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                {
                    colaboradoresProyectos[fila["personOtherID"].value] = int.Parse(fila["num"].value);
                }
                if (sparqlObject.results.bindings.Count < limit)
                {
                    break;
                }
            }

            return colaboradoresProyectos;
        }

        public HashSet<string> ObtenerColaboradoresDepartamento(string pPersonID)
        {
            HashSet<string> colaboradoresDepartamento = new HashSet<string>();
            int limit = 10000;
            int offset = 0;
            while (true)
            {
                string select = $@"SELECT * WHERE {{select distinct ?personOtherID from <{mResourceApi.GraphsUrl}department.owl>";
                string where = $@"where
                            {{ 
                                <{pPersonID}> <http://vivoweb.org/ontology/core#departmentOrSchool> ?depID.
                                ?personOtherID <http://vivoweb.org/ontology/core#departmentOrSchool> ?depID.            
                            }}order by desc (?personOtherID) }} LIMIT {limit} OFFSET {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "person");
                offset += limit;
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                {
                    colaboradoresDepartamento.Add(fila["personOtherID"].value);
                }
                if (sparqlObject.results.bindings.Count < limit)
                {
                    break;
                }
            }
            return colaboradoresDepartamento;
        }

        public List<Person> ObtenerPersonasFirma(string firma)
        {
            List<Person> listaPersonas = new List<Person>();

            string texto = Disambiguation.ObtenerTextosNombresNormalizados(firma);
            string[] wordsTexto = texto.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);


            if (wordsTexto.Length > 0)
            {
                #region Buscamos en nombres
                {
                    List<string> unions = new List<string>();
                    foreach (string wordOut in wordsTexto)
                    {
                        List<string> words = new List<string>();
                        if (wordOut.Length == 2)
                        {
                            words.Add(wordOut[0].ToString());
                            words.Add(wordOut[1].ToString());
                        }
                        else
                        {
                            words.Add(wordOut);
                        }

                        foreach (string word in words)
                        {
                            int score = 1;
                            if (word.Length > 1)
                            {
                                score = 5;
                            }
                            if (score == 1)
                            {
                                StringBuilder sbUnion = new StringBuilder();
                                sbUnion.AppendLine("				?personID <http://xmlns.com/foaf/0.1/name> ?name.");
                                sbUnion.AppendLine($@"				{{  FILTER(lcase(?name) like'{word}%').}} UNION  {{  FILTER(lcase(?name) like'% {word}%').}}  BIND({score} as ?num)  ");
                                unions.Add(sbUnion.ToString());
                            }
                            else
                            {
                                StringBuilder sbUnion = new StringBuilder();
                                sbUnion.AppendLine("				?personID <http://xmlns.com/foaf/0.1/name> ?name.");
                                sbUnion.AppendLine($@"              {FilterWordComplete(word, "name")} BIND({score} as ?num)");
                                //sbUnion.AppendLine($@"				?name bif:contains ""'{word}'"" BIND({score} as ?num) ");
                                unions.Add(sbUnion.ToString());
                            }
                        }
                    }

                    string select = $@"select distinct ?signature ?personID ?ORCID ?name ?num ?departamento from <{mResourceApi.GraphsUrl}person.owl> from <{mResourceApi.GraphsUrl}department.owl>";
                    string where = $@"where
                            {{
                                {{
                                    select ?personID ?ORCID ?name ?signature sum(?num) as ?num
                                    where
                                    {{
                                        ?personID  a <http://xmlns.com/foaf/0.1/Person>.                                        
                                        {{{string.Join("}UNION{", unions)}}}
                                        OPTIONAL{{?personID <http://w3id.org/roh/ORCID> ?ORCID}}
                                    }}
                                }}
                                OPTIONAL
                                {{
                                    ?s a <http://purl.org/ontology/bibo/Document>.
                                    ?s <http://purl.org/ontology/bibo/authorList> ?author.
                                    ?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?personID.
                                    ?author <http://xmlns.com/foaf/0.1/nick> ?signature.    
                                }}
                                OPTIONAL
                                {{
                                    ?personID <http://vivoweb.org/ontology/core#departmentOrSchool> ?depID.
                                    ?depID <http://purl.org/dc/elements/1.1/title> ?departamento.
                                }}
                            }}order by desc (?num)limit 500";
                    SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "document");
                    foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                    {
                        string personID = fila["personID"].value;
                        string name = fila["name"].value;
                        Person persona = listaPersonas.FirstOrDefault(x => x.personid == personID);
                        if (persona == null)
                        {
                            persona = new Person();
                            persona.name = name;
                            persona.personid = personID;
                            persona.signatures = new HashSet<string>();
                            listaPersonas.Add(persona);
                        }
                        if (fila.ContainsKey("signature"))
                        {
                            string signature = fila["signature"].value;
                            persona.signatures.Add(signature);
                        }
                        if (fila.ContainsKey("ORCID"))
                        {
                            persona.orcid = fila["ORCID"].value;
                        }
                        if (fila.ContainsKey("departamento"))
                        {
                            persona.department = fila["departamento"].value;
                        }
                    }
                }
                #endregion
            }
            return listaPersonas;
        }

        public string FilterWordComplete(string pWord, string pVar)
        {
            Dictionary<string, string> listaReemplazos = new Dictionary<string, string>();
            listaReemplazos["a"] = "aáàä";
            listaReemplazos["e"] = "eéèë";
            listaReemplazos["i"] = "iíìï";
            listaReemplazos["o"] = "oóòö";
            listaReemplazos["u"] = "uúùü";
            listaReemplazos["n"] = "nñ";
            listaReemplazos["c"] = "cç";
            foreach (string caracter in listaReemplazos.Keys)
            {
                pWord = pWord.Replace(caracter, $"[{listaReemplazos[caracter]}]");
            }
            string filter = @$"FILTER ( regex(?{pVar},""(^| ){pWord}($| )"", ""i""))";

            return filter;
        }



        #endregion

        #region Métodos para pestañas
        /// <summary>
        /// Obtiene los datos de una pestaña 
        /// </summary>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pTemplate">Plantilla a utilizar</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <param name="pSection">Orden de la sección para la carga parcial</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetTabData(string pId, API.Templates.Tab pTemplate, string pLang, string pSection = null, bool onlyPublicCounters = false)
        {
            List<PropertyData> propertyDatas = new List<PropertyData>();
            List<PropertyData> propertyDatasContadores = new List<PropertyData>();
            List<PropertyData> propertyDatasContadoresPublicos = new List<PropertyData>();
            string graph = "curriculumvitae";
            foreach (API.Templates.TabSection templateSection in pTemplate.sections)
            {
                if (onlyPublicCounters)
                {
                    if (templateSection.presentation.listItemsPresentation != null && templateSection.presentation.listItemsPresentation.isPublishable
                        && templateSection.presentation.listItemsPresentation.listItemEdit.rdftype != "http://vivoweb.org/ontology/core#Project" && templateSection.presentation.listItemsPresentation.listItemEdit.rdftype != "http://purl.org/ontology/bibo/Document")
                    {
                        propertyDatasContadoresPublicos.Add(templateSection.GenerarPropertyDataContadores(graph));
                    }
                }
                else if (string.IsNullOrEmpty(pSection))
                {
                    propertyDatas.Add(templateSection.GenerarPropertyData(graph));
                }
                else if (pSection == "0")
                {
                    if (pTemplate.sections.IndexOf(templateSection) == 0 || templateSection.presentation.listItemsPresentation == null)
                    {
                        propertyDatas.Add(templateSection.GenerarPropertyData(graph));
                    }
                    else
                    {
                        propertyDatasContadores.Add(templateSection.GenerarPropertyDataContadores(graph));
                    }
                }
                else if (pSection == templateSection.property)
                {
                    propertyDatas.Add(templateSection.GenerarPropertyData(graph));
                }
            }

            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataPropiedades = UtilityCV.GetProperties(new HashSet<string>() { pId }, graph, propertyDatas, pLang, new Dictionary<string, SparqlObject>());
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataContadores = UtilityCV.GetPropertiesContadores(new HashSet<string>() { pId }, propertyDatasContadores);
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataContadoresPublicos = UtilityCV.GetPropertiesContadores(new HashSet<string>() { pId }, propertyDatasContadoresPublicos, true);
            if (dataContadores.Count > 0)
            {
                foreach (string key in dataContadores.Keys)
                {
                    if (!dataPropiedades.ContainsKey(key))
                    {
                        dataPropiedades[key] = new List<Dictionary<string, Data>>();
                    }
                    dataPropiedades[key].AddRange(dataContadores[key]);
                }
            }
            if (dataContadoresPublicos.Count > 0)
            {
                foreach (string key in dataContadoresPublicos.Keys)
                {
                    if (!dataPropiedades.ContainsKey(key))
                    {
                        dataPropiedades[key] = new List<Dictionary<string, Data>>();
                    }
                    dataPropiedades[key].AddRange(dataContadoresPublicos[key]);
                }
            }
            return dataPropiedades;
        }

        /// <summary>
        /// Obtiene los datos de un item dentro de un listado
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pListItemConfig">Configuración del item</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetItemMiniData(string pId, TabSectionListItem pListItemConfig, string pLang)
        {
            string graph = "curriculumvitae";

            List<PropertyData> propertyDatas = pListItemConfig.GenerarPropertyData(graph).childs;
            //Editabilidad
            foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
            {
                PropertyData propertyItem = propertyDatas.FirstOrDefault(x => x.property == "http://vivoweb.org/ontology/core#relatedBy");
                if (propertyItem != null)
                {
                    propertyItem.childs.Add(
                        //Editabilidad
                        new Utils.PropertyData()
                        {
                            property = propEditabilidad,
                            childs = new List<Utils.PropertyData>()
                        }
                    );
                }
            }

            //OpenAccess
            {
                PropertyData propertyItem = propertyDatas.FirstOrDefault(x => x.property == "http://vivoweb.org/ontology/core#relatedBy");
                if (propertyItem != null)
                {
                    propertyItem.childs.Add(
                        //OpenAccess
                        new Utils.PropertyData()
                        {
                            property = UtilityCV.PropertyOpenAccess,
                            childs = new List<Utils.PropertyData>()
                        }
                    );
                }
            }

            //ProjectAuthorization
            {
                PropertyData propertyItem = propertyDatas.FirstOrDefault(x => x.property == "http://vivoweb.org/ontology/core#relatedBy");
                if (propertyItem != null)
                {
                    propertyItem.childs.Add(
                        //ProjectAuthorization
                        new Utils.PropertyData()
                        {
                            property = "http://w3id.org/roh/projectAuthorization",
                            childs = new List<Utils.PropertyData>()
                        }
                    );
                }
            }

            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = UtilityCV.GetProperties(new HashSet<string>() { pId }, graph, propertyDatas, pLang, new Dictionary<string, SparqlObject>());
            return data;
        }

        /// <summary>
        /// Obtiene los items del CVque tienen propiedades multiidioma junto con la propiedad y los idiomas los datos multiidioma de un CV
        /// </summary>
        /// <param name="pCVId">Identificador de un CV</param>
        /// <returns></returns>
        private Dictionary<string, Dictionary<string, HashSet<string>>> GetMultilangDataCV(string pCVId)
        {
            Dictionary<string, Dictionary<string, HashSet<string>>> respuesta = new Dictionary<string, Dictionary<string, HashSet<string>>>();

            string selectID = "select distinct ?entityAux ?prop ?lang ";
            string whereID = $@"where{{
                                            ?cv <http://w3id.org/roh/multilangProperties> ?multilangProperties.
                                            ?multilangProperties <http://w3id.org/roh/entity> ?entity.
                                            ?multilangProperties <http://w3id.org/roh/property> ?prop. 
                                            ?multilangProperties <http://w3id.org/roh/lang> ?lang. 
                                            ?cv ?p ?o.
                                            ?o ?p2 ?entityAux.
                                            ?entityAux ?p3 ?entity.
                                            FILTER(?cv =<{pCVId}>)
                                        }}";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                string entityAux = fila["entityAux"].value;
                string prop = fila["prop"].value;
                string lang = fila["lang"].value;
                if (!respuesta.ContainsKey(entityAux))
                {
                    respuesta.Add(entityAux, new Dictionary<string, HashSet<string>>());
                }
                if (!respuesta[entityAux].ContainsKey(prop))
                {
                    respuesta[entityAux].Add(prop, new HashSet<string>());
                }
                respuesta[entityAux][prop].Add(lang);
            }
            return respuesta;
        }

        /// <summary>
        /// Obtiene una sección de pestaña del CV una vez que tenemos los datos cargados
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pData">Datos cargados de BBDD</param>
        /// <param name="pTemplate">Plantilla para generar el template</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private API.Response.Tab GetTabModel(ConfigService pConfig, string pCVId, string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, API.Templates.Tab pTemplate, string pLang, string pSection = null, bool pOnlyPublic = false)
        {
            //Obtenemos todas las entidades del CV con sus propiedades multiidioma
            Dictionary<string, Dictionary<string, HashSet<string>>> entidadesMultiidioma = GetMultilangDataCV(pCVId);

            API.Response.Tab tab = new API.Response.Tab()
            {
                sections = new List<API.Response.TabSection>()
            };
            foreach (API.Templates.TabSection templateSection in pTemplate.sections)
            {

                if (templateSection.presentation != null)
                {
                    API.Response.TabSection tabSection = new API.Response.TabSection()
                    {
                        identifier = templateSection.property,
                        title = UtilityCV.GetTextLang(pLang, templateSection.presentation.title),
                        information = UtilityCV.GetTextLang(pLang, templateSection.presentation.information),
                        cvaISCIII = templateSection.presentation.cvaISCIII,
                        cvaAEI = templateSection.presentation.cvaAEI,
                        orders = new List<TabSectionPresentationOrder>()
                    };
                    switch (templateSection.presentation.type)
                    {
                        case TabSectionPresentationType.listitems:
                            {
                                if (templateSection.presentation.listItemsPresentation.listItem.orders != null)
                                {
                                    foreach (TabSectionListItemOrder listItemOrder in templateSection.presentation.listItemsPresentation.listItem.orders)
                                    {
                                        TabSectionPresentationOrder presentationOrderTabSection = new TabSectionPresentationOrder()
                                        {
                                            name = UtilityCV.GetTextLang(pLang, listItemOrder.name),
                                            properties = new List<TabSectionPresentationOrderProperty>()
                                        };

                                        if (listItemOrder.properties != null)
                                        {
                                            foreach (TabSectionListItemOrderProperty listItemConfigOrderProperty in listItemOrder.properties)
                                            {
                                                TabSectionPresentationOrderProperty presentationOrderTabSectionProperty = new TabSectionPresentationOrderProperty()
                                                {
                                                    property = UtilityCV.GetPropComplete(listItemConfigOrderProperty),
                                                    asc = listItemConfigOrderProperty.asc
                                                };
                                                presentationOrderTabSection.properties.Add(presentationOrderTabSectionProperty);
                                            }
                                        }
                                        tabSection.orders.Add(presentationOrderTabSection);
                                    }
                                }

                                tabSection.items = new Dictionary<string, TabSectionItem>();
                                string propiedadIdentificador = templateSection.property;
                                if (pData.ContainsKey(pId))
                                {
                                    bool soloID = false;
                                    if ((pOnlyPublic && pSection == null) || (pSection == "0" && pTemplate.sections.IndexOf(templateSection) > 0))
                                    {
                                        soloID = true;
                                    }
                                    foreach (string idEntity in pData[pId].Where(x => x["p"].value == templateSection.property).Select(x => x["o"].value).Distinct())
                                    {
                                        Dictionary<string, HashSet<string>> propiedadesMultiIdiomaCargadas = new Dictionary<string, HashSet<string>>();
                                        List<ItemEditSectionRowProperty> listaPropiedadesConfiguradas = templateSection.presentation.listItemsPresentation.listItemEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).Where(x => x.multilang).ToList();
                                        if (entidadesMultiidioma.ContainsKey(idEntity))
                                        {
                                            propiedadesMultiIdiomaCargadas = entidadesMultiidioma[idEntity];
                                        }
                                        if (soloID)
                                        {
                                            tabSection.items.Add(idEntity, null);
                                        }
                                        else
                                        {
                                            tabSection.items.Add(idEntity, GetItem(pConfig, idEntity, pData, templateSection.presentation.listItemsPresentation, pLang, propiedadesMultiIdiomaCargadas, listaPropiedadesConfiguradas, templateSection.presentation.listItemsPresentation.last5Years));
                                            if (pOnlyPublic && !tabSection.items.Last().Value.ispublic)
                                            {
                                                tabSection.items.Remove(idEntity);
                                            }
                                        }
                                    }
                                }
                                tab.sections.Add(tabSection);
                            }
                            break;
                        case TabSectionPresentationType.item:
                            {
                                string id = null;
                                if (pData.ContainsKey(pId))
                                {
                                    foreach (string idEntity in pData[pId].Where(x => x["p"].value == templateSection.property).Select(x => x["o"].value).Distinct())
                                    {
                                        id = idEntity;
                                    }
                                    if (id != null && pData.ContainsKey(id))
                                    {
                                        foreach (string idEntity in pData[id].Where(x => x["p"].value == templateSection.presentation.itemPresentation.property).Select(x => x["o"].value).Distinct())
                                        {
                                            id = idEntity;
                                        }
                                    }
                                }
                                tabSection.item = GetEditModel(pCVId, id, templateSection.presentation.itemPresentation.itemEdit, pLang);
                                if (string.IsNullOrEmpty(tabSection.item.entityID))
                                {
                                    tabSection.item.entityID = Guid.NewGuid().ToString().ToLower();
                                }
                                tab.sections.Add(tabSection);
                            }
                            break;
                        default:
                            throw new Exception("No está implementado el código para el tipo " + templateSection.presentation.type.ToString());
                    }
                }
            }
            if (pOnlyPublic)
            {
                tab.sections.RemoveAll(x => x.items == null || x.items.Count == 0);
            }
            return tab;
        }


        /// <summary>
        /// Obtiene un item de un listado de una sección
        /// </summary>
        /// <param name="pId">Identificador del item</param>
        /// <param name="pData">Datos cargados</param>
        /// <param name="pListItemConfig">Configuración del item</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pPropiedadesMultiidiomaCargadas">Listado con las propiedades cargadas multiidioma del item junto con su idioma</param>
        /// <param name="pListaPropiedadesMultiidiomaConfiguradas">Lista de propiedades que tienen el multiidoima configurado</param>
        /// <returns></returns>
        private TabSectionItem GetItem(ConfigService pConfig, string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData,
            TabSectionPresentationListItems pListItemConfig, string pLang, Dictionary<string, HashSet<string>> pPropiedadesMultiidiomaCargadas,
            List<ItemEditSectionRowProperty> pListaPropiedadesMultiidiomaConfiguradas, [Optional] Last5Years last5Years)
        {
            TabSectionItem item = new TabSectionItem();
            string propertyInTitle = "";
            if (pListItemConfig.listItem.propertyTitleOR != null && pListItemConfig.listItem.propertyTitleOR.Count > 0)
            {
                propertyInTitle = "";
                string aux = "";
                foreach (PropertyDataTemplate propertyData in pListItemConfig.listItem.propertyTitleOR)
                {
                    propertyInTitle += aux + UtilityCV.GetPropComplete(propertyData);
                    aux = "||";
                }
            }
            else
            {
                propertyInTitle = UtilityCV.GetPropComplete(pListItemConfig.listItem.propertyTitle);
            }
            item.title = GetPropValues(pId, propertyInTitle, pData).FirstOrDefault();
            if (pListItemConfig.listItem?.propertyTitle?.auxTitle != null)
            {
                item.title = UtilityCV.GetTextLang(pLang, pListItemConfig.listItem?.propertyTitle?.auxTitle) + " " + item.title;
            }

            if (item.title == null)
            {
                item.title = "";
            }
            item.identifier = mResourceApi.GetShortGuid(GetPropValues(pId, pListItemConfig.listItem.propertyTitle.property, pData).FirstOrDefault()).ToString().ToLower();

            //Editable
            item.iseditable = true;
            //Borrable (se comprueba lo mismo que si son editables excepto 'http://w3id.org/roh/isValidated')
            item.iserasable = true;
            if (!string.IsNullOrEmpty(pId))
            {
                foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
                {
                    string valorPropiedadEditabilidad = GetPropValues(pId, pListItemConfig.property + "@@@" + propEditabilidad, pData).FirstOrDefault();
                    if (propEditabilidad.Equals("http://w3id.org/roh/validationStatusPRC"))
                    {
                        if (UtilityCV.PropertyNotEditable[propEditabilidad] == null || UtilityCV.PropertyNotEditable[propEditabilidad].Count == 0 && !string.IsNullOrEmpty(valorPropiedadEditabilidad))
                        {
                            item.sendPRC = true;
                        }
                        else if (UtilityCV.PropertyNotEditable[propEditabilidad].Contains(valorPropiedadEditabilidad))
                        {
                            item.sendPRC = false;
                        }
                    }
                    if ((Utils.UtilityCV.PropertyNotEditable[propEditabilidad] == null || Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Count == 0) && !string.IsNullOrEmpty(valorPropiedadEditabilidad))
                    {
                        item.iseditable = false;
                        if (propEditabilidad != "http://w3id.org/roh/isValidated")
                        {
                            item.iserasable = false;
                        }
                    }
                    else if (Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Contains(valorPropiedadEditabilidad))
                    {
                        item.iseditable = false;
                        if (propEditabilidad != "http://w3id.org/roh/isValidated")
                        {
                            item.iserasable = false;
                        }
                    }
                }
            }

            //OpenAccess
            item.isopenaccess = false;
            if (!string.IsNullOrEmpty(pId))
            {
                string valorPropiedadOpenAccess = GetPropValues(pId, pListItemConfig.property + "@@@" + UtilityCV.PropertyOpenAccess, pData).FirstOrDefault();
                if (valorPropiedadOpenAccess == "true")
                {
                    item.isopenaccess = true;
                }
            }
            //Publicable
            item.isPublishable = false;
            if (pListItemConfig.isPublishable)
            {
                item.isPublishable = true;
            }
            //Visibilidad
            string valorVisibilidad = GetPropValues(pId, UtilityCV.PropertyIspublic, pData).FirstOrDefault();
            item.ispublic = false;
            if (!string.IsNullOrEmpty(valorVisibilidad) && valorVisibilidad == "true")
            {
                item.ispublic = true;
            }

            //Ultimos 5 años
            item.isChecked = false;
            if (last5Years != null && last5Years.always)
            {
                item.isChecked = true;
            }

            List<string> listadoPropiedades = new List<string>();
            foreach (TabSectionListItemProperty property in pListItemConfig.listItem.properties)
            {
                if (property.childOR != null && property.childOR.Count > 0)
                {
                    string aux = "";
                    string propertyIn = "";
                    foreach (PropertyDataTemplate propertyData in property.childOR)
                    {
                        propertyIn += aux + UtilityCV.GetPropComplete(propertyData);
                        aux = "||";
                        listadoPropiedades.Add(propertyIn);
                    }
                }
                else
                {
                    listadoPropiedades.Add(UtilityCV.GetPropComplete(property.child));

                }
            }

            if (last5Years != null)
            {
                foreach (string propertyIn in listadoPropiedades)
                {
                    string propEnd = "";
                    if (item.isChecked)
                    {
                        break;
                    }
                    if (listadoPropiedades.Select(x => x.Split("@@@").Last()).Contains(last5Years.end))
                    {
                        propEnd = listadoPropiedades.Where(x => x.Split("@@@").Last().Equals(last5Years.end)).First();
                        if (!string.IsNullOrEmpty(propEnd))
                        {
                            List<string> value = GetPropValues(pId, propEnd, pData);

                            if (value.Count > 0)
                            {
                                string fecha = value.First();
                                int anio = int.Parse(fecha.Substring(0, 4));
                                int mes = int.Parse(fecha.Substring(4, 2));
                                int dia = int.Parse(fecha.Substring(6, 2));
                                int horas = int.Parse(fecha.Substring(8, 2));
                                int minutos = int.Parse(fecha.Substring(10, 2));
                                int segundos = int.Parse(fecha.Substring(12, 2));
                                DateTime dateTime = new DateTime(anio, mes, dia);
                                DateTime dateMenos5Anio = DateTime.Now.AddYears(-5);

                                if (dateTime > dateMenos5Anio)
                                {
                                    item.isChecked = true;
                                }
                            }
                        }
                    }
                    else if (propEnd == null && listadoPropiedades.Select(x => x.Split("@@@").Last()).Contains(last5Years.start))
                    {
                        string propStart = listadoPropiedades.Where(x => x.Split("@@@").Last().Equals(last5Years.start)).First();
                        if (!string.IsNullOrEmpty(propStart))
                        {
                            List<string> value = GetPropValues(pId, propStart, pData);

                            if (value.Count > 0)
                            {
                                string fecha = value.First();
                                int anio = int.Parse(fecha.Substring(0, 4));
                                int mes = int.Parse(fecha.Substring(4, 2));
                                int dia = int.Parse(fecha.Substring(6, 2));
                                int horas = int.Parse(fecha.Substring(8, 2));
                                int minutos = int.Parse(fecha.Substring(10, 2));
                                int segundos = int.Parse(fecha.Substring(12, 2));
                                DateTime dateTime = new DateTime(anio, mes, dia);
                                DateTime dateMenos5Anio = DateTime.Now.AddYears(-5);

                                if (dateTime > dateMenos5Anio)
                                {
                                    item.isChecked = true;
                                }
                            }
                        }
                    }
                }
            }

            item.properties = new List<TabSectionItemProperty>();
            if (pListItemConfig.listItem.properties != null)
            {
                foreach (TabSectionListItemProperty property in pListItemConfig.listItem.properties)
                {
                    string propertyIn = "";
                    TabSectionItemProperty itemProperty = new TabSectionItemProperty()
                    {
                        showMini = property.showMini,
                        showMiniBold = property.showMiniBold,
                        information = UtilityCV.GetTextLang(pLang, property.information),
                        name = UtilityCV.GetTextLang(pLang, property.name)
                    };
                    if (property.childOR != null && property.childOR.Count > 0)
                    {
                        propertyIn = "";
                        string aux = "";
                        foreach (PropertyDataTemplate propertyData in property.childOR)
                        {
                            propertyIn += aux + UtilityCV.GetPropComplete(propertyData);
                            aux = "||";
                        }
                    }
                    else
                    {
                        propertyIn = UtilityCV.GetPropComplete(property.child);
                    }
                    itemProperty.type = property.type.ToString();
                    itemProperty.values = GetPropValues(pId, propertyIn, pData);
                    if (property.type == DataTypeListItem.number)
                    {
                        itemProperty.values = itemProperty.values.Select(x => UtilityCV.GetTextNumber(x)).ToList();
                    }
                    else if (property.type == DataTypeListItem.boolean)
                    {
                        List<string> valuesAux = new List<string>(itemProperty.values);
                        itemProperty.values = new List<string>();
                        string si = "";
                        string no = "";
                        switch (pLang)
                        {
                            case "es":
                                si = "Sí";
                                no = "No";
                                break;
                            case "en":
                                si = "Yes";
                                no = "No";
                                break;
                            default:
                                si = "Sí";
                                no = "No";
                                break;
                        }
                        foreach (string value in valuesAux)
                        {
                            if (value.ToLower() == "true")
                            {
                                itemProperty.values.Add(si);
                            }
                            else if (value.ToLower() == "false")
                            {
                                itemProperty.values.Add(no);
                            }
                        }
                    }
                    item.properties.Add(itemProperty);
                }
            }
            item.orderProperties = new List<TabSectionItemOrderProperty>();
            if (pListItemConfig.listItem.orders != null)
            {
                foreach (TabSectionListItemOrder order in pListItemConfig.listItem.orders)
                {
                    if (order.properties != null)
                    {
                        foreach (TabSectionListItemOrderProperty data in order.properties)
                        {
                            TabSectionItemOrderProperty itemOrderProperty = new TabSectionItemOrderProperty()
                            {
                                property = UtilityCV.GetPropComplete(data),
                                values = GetPropValues(pId, UtilityCV.GetPropComplete(data), pData)
                            };
                            if (!item.orderProperties.Exists(x => x.property == itemOrderProperty.property))
                            {
                                item.orderProperties.Add(itemOrderProperty);
                            }
                        }
                    }
                }
            }


            //Multiidiomas cargados
            item.multilang = new Dictionary<string, bool>();
            /*pPropiedadesMultiidiomaCargadas,List<string> pListaPropiedadesMultiidiomaConfiguradas*/
            if (pListaPropiedadesMultiidiomaConfiguradas.Count > 0)
            {
                //Cargamos las propiedades visibles y que tienen valor en el idioma base
                List<string> propiedadesMultiidiomaConfiguradas = new List<string>();
                foreach (ItemEditSectionRowProperty proEdit in pListaPropiedadesMultiidiomaConfiguradas)
                {
                    string valorPropiedadIdiomaBase = GetPropValues(pId, "http://vivoweb.org/ontology/core#relatedBy@@@" + proEdit.property, pData).FirstOrDefault();
                    if (string.IsNullOrEmpty(valorPropiedadIdiomaBase))
                    {
                        continue;
                    }
                    if (proEdit.dependency == null)
                    {
                        propiedadesMultiidiomaConfiguradas.Add(proEdit.property);
                    }
                    else
                    {
                        string propiedadDependencia = proEdit.dependency.property;
                        if (!string.IsNullOrEmpty(propiedadDependencia))
                        {
                            if (!string.IsNullOrEmpty(proEdit.dependency.propertyValue))
                            {
                                string propiedadValorDependencia = proEdit.dependency.propertyValue.Replace("{GraphsUrl}", mResourceApi.GraphsUrl);
                                string valorPropiedadCargadaDependencia = GetPropValues(pId, "http://vivoweb.org/ontology/core#relatedBy@@@" + propiedadDependencia, pData).FirstOrDefault();
                                if (valorPropiedadCargadaDependencia == propiedadValorDependencia)
                                {
                                    propiedadesMultiidiomaConfiguradas.Add(proEdit.property);
                                }
                            }
                            else if (!string.IsNullOrEmpty(proEdit.dependency.propertyValueDistinct))
                            {
                                string propiedadValorDependenciaDistinto = proEdit.dependency.propertyValueDistinct.Replace("{GraphsUrl}", mResourceApi.GraphsUrl);
                                string valorPropiedadCargadaDependencia = GetPropValues(pId, "http://vivoweb.org/ontology/core#relatedBy@@@" + propiedadDependencia, pData).FirstOrDefault();
                                if (valorPropiedadCargadaDependencia != propiedadValorDependenciaDistinto)
                                {
                                    propiedadesMultiidiomaConfiguradas.Add(proEdit.property);
                                }
                            }
                        }
                    }
                }

                List<string> idiomas = new List<string>() { "en", "ca", "eu", "gl", "fr" };
                foreach (string idioma in idiomas)
                {
                    item.multilang[idioma] = false;
                    if (pPropiedadesMultiidiomaCargadas.Where(x => x.Value.Contains(idioma)).Select(x => x.Key).Intersect(propiedadesMultiidiomaConfiguradas).Count() == propiedadesMultiidiomaConfiguradas.Count)
                    {
                        item.multilang[idioma] = true;
                    }
                }
            }

            //SendPRC
            item.sendPRC = false;
            if (pListItemConfig.listItemEdit.rdftype.Equals("http://purl.org/ontology/bibo/Document"))
            {
                item.sendPRC = true;
                if (!string.IsNullOrEmpty(pId))
                {
                    // Si el estado de validación es "pendiente" o "validado", no permito el envío a PRC.
                    string validationStatus = GetPropValues(pId, pListItemConfig.property + "@@@" + "http://w3id.org/roh/validationStatusPRC", pData).FirstOrDefault();
                    if (validationStatus == "pendiente" || validationStatus == "validado")
                    {
                        item.sendPRC = false;
                    }
                    //Si el item no tiene fecha, no permito el envío
                    if (!item.properties.Where(x => x.name.Equals("Fecha de publicación")).Where(x => x.values.Any()).Any())
                    {
                        item.sendPRC = false;
                    }
                    else
                    {
                        string fechaPublicacion = item.properties.Where(x => x.name.Equals("Fecha de publicación")).Where(x => x.values.Any()).First().values.First();
                        int anio = int.Parse(fechaPublicacion.Substring(0, 4));
                        int mes = int.Parse(fechaPublicacion.Substring(4, 2));
                        int dia = int.Parse(fechaPublicacion.Substring(6, 2));
                        DateTime fecha = new DateTime(anio, mes, dia);
                        DateTime fechaMax = DateTime.Now;
                        fechaMax = fechaMax.AddMonths(-pConfig.GetMaxMonthsValidationDocument());
                        if (fechaMax > fecha)
                        {
                            item.sendPRC = false;
                        }
                    }
                    //Si es de tipo publicación y no tiene tipo de proyecto, no permito el envío
                    if (pListItemConfig.rdftype_cv.Equals("http://w3id.org/roh/RelatedScientificPublicationCV")
                        && !item.properties.Where(x => x.name.Equals("Tipo de producción")).Where(x => x.values.Any()).Any())
                    {
                        item.sendPRC = false;
                    }
                    //Si es de tipo congreso y no tiene tipo de proyecto, no permito el envío
                    if (pListItemConfig.rdftype_cv.Equals("http://w3id.org/roh/RelatedWorkSubmittedConferencesCV")
                        && !item.properties.Where(x => x.name.Equals("Tipo de producción")).Where(x => x.values.Any()).Any())
                    {
                        item.sendPRC = false;
                    }
                    //Si el documento es de tipo "Trabajos presentados en jornadas", no permito el envío
                    if (pListItemConfig.rdftype_cv.Equals("http://w3id.org/roh/RelatedWorkSubmittedSeminarsCV"))
                    {
                        item.sendPRC = false;
                    }

                }
            }

            //Estado de validación
            item.validationStatus = "";
            string valorPropiedad = "";
            //Estado de validación de documentos
            if (!string.IsNullOrEmpty(pListItemConfig.rdftype_cv) && pListItemConfig.rdftype_cv.Equals("http://w3id.org/roh/RelatedScientificPublicationCV"))
            {
                valorPropiedad = GetPropValues(pId, pListItemConfig.property + "@@@" + "http://w3id.org/roh/validationStatusPRC", pData).FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(pListItemConfig.rdftype_cv) && pListItemConfig.rdftype_cv.Equals("http://w3id.org/roh/RelatedWorkSubmittedConferencesCV"))
            {
                valorPropiedad = GetPropValues(pId, pListItemConfig.property + "@@@" + "http://w3id.org/roh/validationStatusPRC", pData).FirstOrDefault();
            }
            //Estado de validación para los proyectos
            if (!string.IsNullOrEmpty(pListItemConfig.rdftype_cv) && (
                pListItemConfig.rdftype_cv.Equals("http://w3id.org/roh/RelatedNonCompetitiveProjectCV") || pListItemConfig.rdftype_cv.Equals("http://w3id.org/roh/RelatedCompetitiveProjectCV")))
            {
                valorPropiedad = GetPropValues(pId, pListItemConfig.property + "@@@" + "http://w3id.org/roh/validationStatusProject", pData).FirstOrDefault();
            }

            // Si el estado de validación es "pendiente".
            if (valorPropiedad == "pendiente")
            {
                item.validationStatus = "pendiente";
            }
            // Si el estado de validación es "validado".
            if (valorPropiedad == "validado")
            {
                item.validationStatus = "validado";
            }

            //Boton de envío a validación de proyectos
            if (!string.IsNullOrEmpty(pListItemConfig.rdftype_cv) &&
                (pListItemConfig.rdftype_cv.Equals("http://w3id.org/roh/RelatedCompetitiveProjectCV") || pListItemConfig.rdftype_cv.Equals("http://w3id.org/roh/RelatedNonCompetitiveProjectCV")))
            {
                valorPropiedad = GetPropValues(pId, pListItemConfig.property + "@@@" + "http://w3id.org/roh/projectAuthorization", pData).FirstOrDefault();
                if (!string.IsNullOrEmpty(valorPropiedad))
                {
                    valorPropiedad = GetPropValues(pId, pListItemConfig.property + "@@@" + "http://w3id.org/roh/validationStatusProject", pData).FirstOrDefault();
                    if (string.IsNullOrEmpty(valorPropiedad))
                    {
                        item.sendValidationProject = true;
                    }
                }
            }

            return item;
        }


        #endregion

        #region Métodos para edición de entidades
        /// <summary>
        /// Obtiene todos los datos de una entidad de BBDD para su posterior edición
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <param name="pId">Identificador</param>
        /// <param name="pItemEdit">Configuración de edición</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pEntityCV">Entidad del cv desde la que se apunta a la entidad</param>
        /// <param name="pPropertyCV">Propiedad que apunta a la entidad en el CV</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetEditData(string pCVId, string pId, ItemEdit pItemEdit, string pGraph, string pLang, string pEntityCV = null, string pPropertyCV = null)
        {
            List<PropertyData> propertyDatas = pItemEdit.GenerarPropertyDatas(pGraph);
            //Editabilidad
            foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
            {
                propertyDatas.Add(new Utils.PropertyData()
                {
                    property = propEditabilidad,
                    childs = new List<Utils.PropertyData>()
                });
            }
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> respuesta = UtilityCV.GetProperties(new HashSet<string>() { pId }, pGraph, propertyDatas, pLang, new Dictionary<string, SparqlObject>());

            List<PropertyData> propertyDatasCV = pItemEdit.GenerarPropertyDatas(pGraph, true);
            if (propertyDatasCV.Count > 0)
            {
                SparqlObject idEntityInCV = mResourceApi.VirtuosoQuery("select ?id", "where{<" + pEntityCV + "> <" + pPropertyCV + "> ?id}", "curriculumvitae");
                if (idEntityInCV.results.bindings.Count > 0)
                {
                    string id = idEntityInCV.results.bindings[0]["id"].value;
                    Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> respuestaCV = UtilityCV.GetProperties(new HashSet<string>() { id }, "curriculumvitae", propertyDatasCV, pLang, new Dictionary<string, SparqlObject>());
                    foreach (string idrespuesta in respuestaCV.Keys)
                    {
                        string idAux = idrespuesta;
                        if (idrespuesta == id)
                        {
                            idAux = pId;
                        }
                        if (!respuesta.ContainsKey(idAux))
                        {
                            respuesta.Add(idAux, new List<Dictionary<string, Data>>());
                        }
                        foreach (Dictionary<string, SparqlObject.Data> fila in respuestaCV[idrespuesta])
                        {
                            Dictionary<string, SparqlObject.Data> filaAux = new Dictionary<string, Data>();
                            filaAux.Add("s", fila["s"]);
                            filaAux.Add("p", fila["p"]);
                            filaAux.Add("o", fila["o"]);
                            filaAux["s"].value = filaAux["s"].value.Replace(id, pId);
                            respuesta[idAux].Add(filaAux);
                        }
                    }
                }
            }
            //Si tiene multidioma cargamos los datos
            if (pItemEdit.sections.Exists(x => x.rows.Exists(y => y.properties.Exists(z => z.multilang))))
            {
                Dictionary<string, Dictionary<string, List<MultilangProperty>>> multilangData = UtilityCV.GetMultilangPropertiesCV(pCVId, pId);
                if (!string.IsNullOrEmpty(pId) && multilangData.ContainsKey(pId))
                {
                    foreach (string prop in multilangData[pId].Keys)
                    {
                        foreach (MultilangProperty multilangProperty in multilangData[pId][prop])
                        {
                            Dictionary<string, SparqlObject.Data> filaAux = new Dictionary<string, Data>();
                            filaAux.Add("s", new Data() { value = pId, type = "uri" });
                            filaAux.Add("p", new Data() { value = prop, type = "uri" });
                            filaAux.Add("o", new Data() { value = multilangProperty.value, type = "literal" });
                            filaAux.Add("lang", new Data() { value = multilangProperty.lang, type = "literal" });
                            respuesta[pId].Add(filaAux);
                        }
                    }
                }
            }

            return respuesta;
        }

        /// <summary>
        /// Genera el modelo de edición de una entidad una vez tenemos todos los datos de la entidad cargados
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pPresentationEdit">Configuración de presentación</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pEntityCV">Entidad del cv desde la que se apunta a la entidad</param>
        /// <param name="pPropertyCV">Propiedad que apunta a la entidad en el CV</param>
        /// <returns></returns>
        private EntityEdit GetEditModel(string pCVId, string pId, ItemEdit pPresentationEdit, string pLang, string pEntityCV = null, string pPropertyCV = null)
        {
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetEditData(pCVId, pId, pPresentationEdit, pPresentationEdit.graph, pLang, pEntityCV, pPropertyCV);

            List<ItemEditSectionRowPropertyCombo> listCombosConfig = GetEditCombos(pPresentationEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).ToList());
            Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> combos = new Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>>();
            Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> combosDependency = new Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>>();

            foreach (ItemEditSectionRowPropertyCombo combo in listCombosConfig)
            {
                Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataCombo = GetSubjectsCombo(combo, pLang);
                Dictionary<string, string> valoresCombo = new Dictionary<string, string>() { { "", "-" } };
                Dictionary<string, string> valoresDependency = new Dictionary<string, string>() { };
                foreach (string id in dataCombo.Keys)
                {
                    valoresCombo.Add(id, GetPropValues(id, UtilityCV.GetPropComplete(combo.property), dataCombo).First());

                    if (combo.dependency != null)
                    {
                        valoresDependency.Add(id, GetPropValues(id, combo.dependency.property, dataCombo).First());
                    }
                }
                valoresCombo = valoresCombo.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                combos.Add(combo, valoresCombo);
                if (combo.dependency != null)
                {
                    combosDependency.Add(combo, valoresDependency);
                }
            }

            //Añadimos el combo de autorizaciones de proyecto con las autorizaciones de la persona que no esten asignadas a algún proyecto
            Dictionary<string, string> comboAutorizacionesProyecto = new Dictionary<string, string>() { { "", "-" } };
            if (pPresentationEdit.sections.Exists(x => x.rows.Exists(y => y.properties.Exists(z => z.type == DataTypeEdit.projectauthorization))))
            {
                string person = UtilityCV.GetPersonFromCV(pCVId);
                string select = $@"select ?s ?nombre from <{mResourceApi.GraphsUrl}project.owl>";
                string where = $@"  where
                                    {{
                                        ?s a <http://w3id.org/roh/ProjectAuthorization>.
                                        ?s <http://w3id.org/roh/title> ?nombre.
                                        ?s <http://w3id.org/roh/owner> <{person}>.
                                        MINUS
                                        {{
                                            ?proy a <http://vivoweb.org/ontology/core#Project>.
                                            ?proy <http://w3id.org/roh/projectAuthorization> ?s.
                                            FILTER(?proy !=<{pId}>)
                                        }}
                                    }}";
                SparqlObject respuesta = mResourceApi.VirtuosoQuery(select, where, "projectauthorization");
                foreach (Dictionary<string, SparqlObject.Data> fila in respuesta.results.bindings)
                {
                    comboAutorizacionesProyecto[fila["s"].value] = fila["nombre"].value;
                }
            }

            List<string> listaTesaurosConfig = GetEditThesaurus(pPresentationEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).ToList());
            Dictionary<string, List<ThesaurusItem>> tesauros = GetTesauros(listaTesaurosConfig, pLang);

            EntityEdit entityEdit = new EntityEdit()
            {
                entityID = pId,
                ontology = pPresentationEdit.graph,
                rdftype = pPresentationEdit.rdftype,
                sections = new List<EntityEditSection>()
            };

            foreach (ItemEditSection itemEditSection in pPresentationEdit.sections)
            {
                EntityEditSection entityEditSection = new EntityEditSection()
                {
                    title = UtilityCV.GetTextLang(pLang, itemEditSection.title),
                    rows = GetRowsEdit(pId, itemEditSection.rows, data, combos, comboAutorizacionesProyecto, combosDependency, tesauros, pLang, pPresentationEdit.graph)
                };
                entityEdit.sections.Add(entityEditSection);
            }

            //Editabilidad
            entityEdit.iseditable = true;
            if (!string.IsNullOrEmpty(pId))
            {
                foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
                {
                    string valorPropiedad = GetPropValues(pId, propEditabilidad, data).FirstOrDefault();
                    if ((Utils.UtilityCV.PropertyNotEditable[propEditabilidad] == null || Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Count == 0) && !string.IsNullOrEmpty(valorPropiedad))
                    {
                        entityEdit.iseditable = false;
                    }
                    else if (Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Contains(valorPropiedad))
                    {
                        entityEdit.iseditable = false;
                    }
                }
            }
            return entityEdit;
        }


        /// <summary>
        /// Genera el modelo de edición de las filas de edición de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pItemEditSectionRows">Filas de edición configuradas</param>
        /// <param name="pData">Datos de la entidad</param>        
        /// <param name="pCombos">Combos</param>
        /// <param name="pComboAutorizacionesProyectos">Combo con las autorizaciones de proyectos</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns></returns>
        private List<EntityEditSectionRow> GetRowsEdit(string pId, List<ItemEditSectionRow> pItemEditSectionRows, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, Dictionary<string, string> pComboAutorizacionesProyectos, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombosDependency, Dictionary<string, List<ThesaurusItem>> pTesauros, string pLang, string pGraph)
        {
            List<EntityEditSectionRow> entityEditSectionRows = new List<EntityEditSectionRow>();
            foreach (ItemEditSectionRow itemEditSectionRow in pItemEditSectionRows)
            {
                EntityEditSectionRow entityEditSectionRow = new EntityEditSectionRow()
                {
                    properties = new List<EntityEditSectionRowProperty>()
                };
                foreach (ItemEditSectionRowProperty itemEditSectionRowProperty in itemEditSectionRow.properties)
                {
                    if (string.IsNullOrEmpty(itemEditSectionRowProperty.compossed))
                    {
                        EntityEditSectionRowProperty entityEditSectionRowProperty = GetPropertiesEdit(pId, itemEditSectionRowProperty, pData, pCombos, pComboAutorizacionesProyectos, pCombosDependency, pTesauros, pLang, pGraph);
                        entityEditSectionRow.properties.Add(entityEditSectionRowProperty);
                    }
                }
                entityEditSectionRows.Add(entityEditSectionRow);
            }
            return entityEditSectionRows;
        }

        /// <summary>
        /// Genera el modelo de edición de una propiedad de edición de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pItemEditSectionRowProperty">Propiedad de edición configuradas</param>
        /// <param name="pData">Datos de la entidad</param>        
        /// <param name="pCombos">Combos</param>
        /// <param name="pComboAutorizacionesProyectos">Combo con las autorizaciones de proyectos</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns></returns>
        private EntityEditSectionRowProperty GetPropertiesEdit(string pId, ItemEditSectionRowProperty pItemEditSectionRowProperty, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, Dictionary<string, string> pComboAutorizacionesProyectos, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombosDependency, Dictionary<string, List<ThesaurusItem>> pTesauros, string pLang, string pGraph)
        {
            if (pItemEditSectionRowProperty.type == DataTypeEdit.auxEntityAuthorList)
            {
                EntityEditSectionRowProperty entityEditSectionRowProperty = new EntityEditSectionRowProperty()
                {
                    property = pItemEditSectionRowProperty.property,
                    width = pItemEditSectionRowProperty.width,
                    required = pItemEditSectionRowProperty.required,
                    editable = pItemEditSectionRowProperty.editable,
                    blocked = pItemEditSectionRowProperty.blocked,
                    multilang = pItemEditSectionRowProperty.multilang,
                    multiple = true,
                    autocomplete = pItemEditSectionRowProperty.autocomplete,
                    title = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.title),
                    information = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.information),
                    type = DataTypeEdit.auxEntityAuthorList.ToString(),
                    values = new List<string>(),
                    entityAuxData = new EntityEditAuxEntity()
                    {
                        childsOrder = new Dictionary<string, int>(),
                        rdftype = "http://purl.obolibrary.org/obo/BFO_0000023",
                        propertyOrder = "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment",
                        titleConfig = new EntityEditRepresentativeProperty()
                        {
                            route = "http://xmlns.com/foaf/0.1/nick"
                        },
                        propertiesConfig = new List<EntityEditRepresentativeProperty>()
                        {
                            new EntityEditRepresentativeProperty()
                            {
                                //TODO multiidioma
                                name = "Nombre",
                                route = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://xmlns.com/foaf/0.1/name"
                            },
                            new EntityEditRepresentativeProperty()
                            {
                                name = "ORCID",
                                route = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://w3id.org/roh/ORCID"
                            }
                        }
                    }
                };
                if (pId != null && pData.ContainsKey(pId))
                {
                    foreach (string value in pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.property).Select(x => x["o"].value).Distinct())
                    {
                        entityEditSectionRowProperty.values.Add(value);
                    }
                }

                entityEditSectionRowProperty.entityAuxData.entities = new Dictionary<string, List<EntityEditSectionRow>>();

                List<ItemEditSectionRow> rowsEdit = new List<ItemEditSectionRow>() {
                    new ItemEditSectionRow()
                    {
                        properties = new List<ItemEditSectionRowProperty>(){
                            new ItemEditSectionRowProperty(){
                                //TODO multiidioma
                                title = new Dictionary<string, string>() { { "es", "Firma" } },
                                type = DataTypeEdit.text,
                                required = true,
                                property = "http://xmlns.com/foaf/0.1/nick",
                                width = 2
                            },
                            new ItemEditSectionRowProperty(){
                                //TODO multiidioma
                                title = new Dictionary<string, string>() { { "es", "Persona" } },
                                type = DataTypeEdit.entity,
                                required = true,
                                property = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member",
                                width = 2,
                                entityData=new ItemEditEntityData()
                                {
                                    rdftype="http://xmlns.com/foaf/0.1/Person",
                                    graph="person",
                                    propertyTitle= new PropertyDataTemplate(){ property="http://xmlns.com/foaf/0.1/name"},
                                    properties=new List<ItemEditEntityProperty>(){
                                        new ItemEditEntityProperty()
                                        {
                                            name=new Dictionary<string, string>(){ { "es", "ORCID" } },
                                            child=new PropertyDataTemplate()
                                            {
                                                property="http://w3id.org/roh/ORCID"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };



                entityEditSectionRowProperty.entityAuxData.rows = GetRowsEdit(null, rowsEdit, pData, pCombos, pComboAutorizacionesProyectos, pCombosDependency, pTesauros, pLang, pGraph);
                entityEditSectionRowProperty.entityAuxData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                entityEditSectionRowProperty.entityAuxData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                foreach (string id in entityEditSectionRowProperty.values)
                {
                    entityEditSectionRowProperty.entityAuxData.entities.Add(id, GetRowsEdit(id, rowsEdit, pData, pCombos, pComboAutorizacionesProyectos, pCombosDependency, pTesauros, pLang, pGraph));
                    if (!string.IsNullOrEmpty(entityEditSectionRowProperty.entityAuxData.propertyOrder))
                    {
                        string orden = GetPropertiesEdit(id, new ItemEditSectionRowProperty() { property = entityEditSectionRowProperty.entityAuxData.propertyOrder }, pData, pCombos, pComboAutorizacionesProyectos, pCombosDependency, pTesauros, pLang, pGraph).values.FirstOrDefault();
                        int.TryParse(orden, out int ordenInt);
                        entityEditSectionRowProperty.entityAuxData.childsOrder[id] = ordenInt;
                    }

                    //Title
                    string title = GetPropValues(id, "http://xmlns.com/foaf/0.1/nick", pData).FirstOrDefault();
                    if (string.IsNullOrEmpty(title))
                    {
                        title = "";
                    }
                    entityEditSectionRowProperty.entityAuxData.titles.Add(id, new EntityEditRepresentativeProperty()
                    {
                        value = title,
                        route = "http://xmlns.com/foaf/0.1/nick"
                    });


                    entityEditSectionRowProperty.entityAuxData.properties[id] = new List<EntityEditRepresentativeProperty>()
                    {
                        new EntityEditRepresentativeProperty()
                        {
                            //TODO  multiidioma
                            name = "Nombre",
                            value = string.Join(", ", GetPropValues(id, "http://www.w3.org/1999/02/22-rdf-syntax-ns#member@@@http://xmlns.com/foaf/0.1/name", pData)),
                            route = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://xmlns.com/foaf/0.1/name"
                        },
                        new EntityEditRepresentativeProperty()
                        {
                            name = "ORCID",
                            value = string.Join(", ", GetPropValues(id, "http://www.w3.org/1999/02/22-rdf-syntax-ns#member@@@http://w3id.org/roh/ORCID", pData)),
                            route = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://w3id.org/roh/ORCID"
                        }
                    };
                }

                return entityEditSectionRowProperty;
            }
            else
            {
                EntityEditSectionRowProperty entityEditSectionRowProperty = new EntityEditSectionRowProperty()
                {
                    property = pItemEditSectionRowProperty.property,
                    width = pItemEditSectionRowProperty.width,
                    required = pItemEditSectionRowProperty.required,
                    editable = pItemEditSectionRowProperty.editable,
                    blocked = pItemEditSectionRowProperty.blocked,
                    multilang = pItemEditSectionRowProperty.multilang,
                    multiple = pItemEditSectionRowProperty.multiple,
                    autocomplete = pItemEditSectionRowProperty.autocomplete,
                    title = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.title),
                    placeholder = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.placeholder),
                    information = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.information),
                    type = pItemEditSectionRowProperty.type.ToString(),
                    thesaurusID = pItemEditSectionRowProperty.thesaurus,
                    values = new List<string>()
                };
                if (pItemEditSectionRowProperty.auxEntityData != null)
                {
                    entityEditSectionRowProperty.entityAuxData = new EntityEditAuxEntity()
                    {
                        childsOrder = new Dictionary<string, int>(),
                        rdftype = pItemEditSectionRowProperty.auxEntityData.rdftype,
                        propertyOrder = pItemEditSectionRowProperty.auxEntityData.propertyOrder,
                    };

                    if (pItemEditSectionRowProperty.auxEntityData.propertyTitle != null)
                    {
                        entityEditSectionRowProperty.entityAuxData.titleConfig = new EntityEditRepresentativeProperty()
                        {
                            route = pItemEditSectionRowProperty.auxEntityData.propertyTitle.GetRoute()
                        };
                    }

                    entityEditSectionRowProperty.entityAuxData.propertiesConfig = new List<EntityEditRepresentativeProperty>();
                    if (pItemEditSectionRowProperty.auxEntityData.properties != null)
                    {
                        foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.auxEntityData.properties)
                        {
                            entityEditSectionRowProperty.entityAuxData.propertiesConfig.Add(new EntityEditRepresentativeProperty()
                            {
                                name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                                route = entityProperty.child.GetRoute()
                            });
                        }
                    }
                }
                if (pItemEditSectionRowProperty.entityData != null)
                {
                    entityEditSectionRowProperty.entityData = new EntityEditEntity()
                    {
                        rdftype = pItemEditSectionRowProperty.entityData.rdftype,
                        titleConfig = new EntityEditRepresentativeProperty()
                    };
                    if (pItemEditSectionRowProperty.entityData.propertyTitle != null)
                    {
                        entityEditSectionRowProperty.entityData.titleConfig = new EntityEditRepresentativeProperty()
                        {
                            route = pItemEditSectionRowProperty.entityData.graph + "||" + pItemEditSectionRowProperty.entityData.propertyTitle.GetRoute()
                        };
                    }
                    entityEditSectionRowProperty.entityData.propertiesConfig = new List<EntityEditRepresentativeProperty>();
                    foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.entityData.properties)
                    {
                        entityEditSectionRowProperty.entityData.propertiesConfig.Add(new EntityEditRepresentativeProperty()
                        {
                            name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                            route = pItemEditSectionRowProperty.entityData.graph + "||" + entityProperty.child.GetRoute()
                        });
                    }
                }

                if (pId != null && pData.ContainsKey(pId))
                {
                    foreach (string value in pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.property && !x.ContainsKey("lang")).Select(x => x["o"].value).Distinct())
                    {
                        entityEditSectionRowProperty.values.Add(value);
                    }
                    if (pItemEditSectionRowProperty.multilang)
                    {
                        entityEditSectionRowProperty.valuesmultilang = new Dictionary<string, string>();
                        foreach (Dictionary<string, SparqlObject.Data> fila in pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.property && x.ContainsKey("lang")))
                        {
                            entityEditSectionRowProperty.valuesmultilang[fila["lang"].value] = fila["o"].value;
                        }
                    }
                }

                if (pItemEditSectionRowProperty.type == DataTypeEdit.boolean)
                {
                    entityEditSectionRowProperty.comboValues = new Dictionary<string, string>();
                    entityEditSectionRowProperty.comboValues[""] = "-";
                    string si = "";
                    string no = "";
                    switch (pLang)
                    {
                        case "es":
                            si = "Sí";
                            no = "No";
                            break;
                        case "en":
                            si = "Yes";
                            no = "No";
                            break;
                        default:
                            si = "Sí";
                            no = "No";
                            break;
                    }
                    entityEditSectionRowProperty.comboValues["true"] = si;
                    entityEditSectionRowProperty.comboValues["false"] = no;
                }
                else if (pItemEditSectionRowProperty.combo != null)
                {
                    entityEditSectionRowProperty.comboValues = pCombos.FirstOrDefault(x =>
                      UtilityCV.GetPropComplete(x.Key.property) == UtilityCV.GetPropComplete(pItemEditSectionRowProperty.combo.property) &&
                       x.Key.graph == pItemEditSectionRowProperty.combo.graph &&
                       x.Key.rdftype == pItemEditSectionRowProperty.combo.rdftype && x.Key.filter == pItemEditSectionRowProperty.combo.filter
                    ).Value;

                    if (pItemEditSectionRowProperty.combo.dependency != null)
                    {
                        Dictionary<string, string> parentDependency = pCombosDependency.FirstOrDefault(x =>
                          UtilityCV.GetPropComplete(x.Key.property) == UtilityCV.GetPropComplete(pItemEditSectionRowProperty.combo.property) &&
                           x.Key.graph == pItemEditSectionRowProperty.combo.graph &&
                           x.Key.rdftype == pItemEditSectionRowProperty.combo.rdftype && x.Key.dependency == pItemEditSectionRowProperty.combo.dependency
                        ).Value;

                        if (parentDependency != null)
                        {
                            entityEditSectionRowProperty.comboDependency = new ComboDependency { parent = pItemEditSectionRowProperty.combo.dependency.propertyValue, parentDependency = parentDependency };
                        }
                    }
                }
                else if (pItemEditSectionRowProperty.type == DataTypeEdit.projectauthorization)
                {
                    entityEditSectionRowProperty.type = "selectCombo";
                    entityEditSectionRowProperty.comboValues = pComboAutorizacionesProyectos;
                }

                if (pItemEditSectionRowProperty.autocompleteConfig != null)
                {
                    entityEditSectionRowProperty.autocompleteConfig = new AutocompleteConfig()
                    {
                        property = UtilityCV.GetPropComplete(pItemEditSectionRowProperty.autocompleteConfig.property),
                        rdftype = pItemEditSectionRowProperty.autocompleteConfig.rdftype,
                        graph = pItemEditSectionRowProperty.autocompleteConfig.graph,
                        cache = pItemEditSectionRowProperty.autocompleteConfig.cache,
                        getEntityId = !string.IsNullOrEmpty(pItemEditSectionRowProperty.autocompleteConfig.propertyEntity),
                        mandatory = pItemEditSectionRowProperty.autocompleteConfig.mandatory,
                        propertiesAux = pItemEditSectionRowProperty.autocompleteConfig.propertyAux?.properties,
                        printAux = pItemEditSectionRowProperty.autocompleteConfig.propertyAux?.print
                    };
                }

                if (pItemEditSectionRowProperty.autocompleteConfig != null && !string.IsNullOrEmpty(pItemEditSectionRowProperty.autocompleteConfig.propertyEntity))
                {
                    entityEditSectionRowProperty.propertyEntity = pItemEditSectionRowProperty.autocompleteConfig.propertyEntity;
                    if (pId != null && pData.ContainsKey(pId))
                    {
                        entityEditSectionRowProperty.propertyEntityValue = pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.propertyEntity).Select(x => x["o"].value).Distinct().FirstOrDefault();
                    }
                    entityEditSectionRowProperty.propertyEntityGraph = pItemEditSectionRowProperty.autocompleteConfig.graph;
                    if(pItemEditSectionRowProperty.autocompleteConfig.selectPropertyEntity!=null && pItemEditSectionRowProperty.autocompleteConfig.selectPropertyEntity.Count>0)
                    {
                        entityEditSectionRowProperty.selectPropertyEntity = new List<SelectPropertyEntity>();
                        foreach(var item in pItemEditSectionRowProperty.autocompleteConfig.selectPropertyEntity)
                        {
                            entityEditSectionRowProperty.selectPropertyEntity.Add(new SelectPropertyEntity() { propertyEntity = item.propertyEntity, propertyCV = item.propertyCV });
                        }
                    }
                   
                }

                if (pItemEditSectionRowProperty.autocompleteConfig != null && pItemEditSectionRowProperty.type == DataTypeEdit.entityautocomplete)
                {
                    entityEditSectionRowProperty.propertyEntityValue = "";
                    if (pId != null && pData.ContainsKey(pId))
                    {
                        string entity = pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.property).Select(x => x["o"].value).Distinct().FirstOrDefault();
                        if (!string.IsNullOrEmpty(entity) && pData.ContainsKey(entity))
                        {
                            if (pItemEditSectionRowProperty.autocompleteConfig.propertyAux != null)
                            {
                                string entityText = "";
                                string[] printSplit = pItemEditSectionRowProperty.autocompleteConfig.propertyAux.print.Split('|');
                                for (int j = 0; j < printSplit.Count(); j++)
                                {
                                    string valor = "";
                                    if (j == 0)
                                    {
                                        valor = pData[entity].Where(x => x["p"].value == pItemEditSectionRowProperty.autocompleteConfig.property.property).Select(x => x["o"].value).Distinct().FirstOrDefault();
                                    }
                                    else
                                    {
                                        valor = pData[entity].Where(x => x["p"].value == pItemEditSectionRowProperty.autocompleteConfig.propertyAux.properties[j - 1]).Select(x => x["o"].value).Distinct().FirstOrDefault();
                                    }
                                    if (!string.IsNullOrEmpty(valor))
                                    {
                                        entityText += printSplit[j].Replace($"{{{j}}}", valor);
                                    }
                                }
                                entityEditSectionRowProperty.propertyEntityValue = entityText;
                            }
                            else
                            {
                                string entityText = pData[entity].Where(x => x["p"].value == pItemEditSectionRowProperty.autocompleteConfig.property.property).Select(x => x["o"].value).Distinct().FirstOrDefault();
                                entityEditSectionRowProperty.propertyEntityValue = entityText;
                            }
                        }
                    }
                }

                if (pItemEditSectionRowProperty.type == DataTypeEdit.thesaurus)
                {
                    entityEditSectionRowProperty.thesaurus = new List<ThesaurusItem>(pTesauros[pItemEditSectionRowProperty.thesaurus]);
                    entityEditSectionRowProperty.entityAuxData = new EntityEditAuxEntity()
                    {
                        childsOrder = new Dictionary<string, int>(),
                        rdftype = "http://w3id.org/roh/CategoryPath",
                        titleConfig = new EntityEditRepresentativeProperty()
                        {
                        }
                    };
                    entityEditSectionRowProperty.entityAuxData.entities = new Dictionary<string, List<EntityEditSectionRow>>();
                    List<ItemEditSectionRow> rowsEdit = new List<ItemEditSectionRow>() {
                        new ItemEditSectionRow()
                        {
                            properties = new List<ItemEditSectionRowProperty>(){
                                new ItemEditSectionRowProperty(){
                                    //TODO multiidioma
                                    title = new Dictionary<string, string>() { { "es", "Categoria" } },
                                    type = DataTypeEdit.text,
                                    multiple=true,
                                    property = "http://w3id.org/roh/categoryNode",
                                    width = 1
                                }
                            }
                        }
                    };
                    entityEditSectionRowProperty.entityAuxData.rows = GetRowsEdit(null, rowsEdit, pData, pCombos, pComboAutorizacionesProyectos, pCombosDependency, pTesauros, pLang, pGraph);
                    entityEditSectionRowProperty.entityAuxData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                    entityEditSectionRowProperty.entityAuxData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                    foreach (string id in entityEditSectionRowProperty.values)
                    {
                        entityEditSectionRowProperty.entityAuxData.entities.Add(id, GetRowsEdit(id, rowsEdit, pData, pCombos, pComboAutorizacionesProyectos, pCombosDependency, pTesauros, pLang, pGraph));
                    }

                    //Lista categorias seleccionadas
                    List<string> cat = new List<string>();
                    foreach (var value in entityEditSectionRowProperty.entityAuxData.entities.Values)
                    {
                        foreach (var propertiesValue in value.Select(x => x.properties))
                        {
                            foreach (var selectionValues in propertiesValue.Select(x => x.values))
                            {
                                cat.AddRange(selectionValues);
                            }
                        }
                    }
                    entityEditSectionRowProperty.thesaurus.RemoveAll(y => !cat.Contains(y.id));
                    //HashSet<string> cat = entityEditSectionRowProperty.entityAuxData.entities.Values.Select(x => x.Select(x => x.properties.Select(x => x.values)));

                    return entityEditSectionRowProperty;
                }

                if (pItemEditSectionRowProperty.type == DataTypeEdit.auxEntity)
                {
                    if (pItemEditSectionRowProperty.auxEntityData != null && pItemEditSectionRowProperty.auxEntityData.rows != null && pItemEditSectionRowProperty.auxEntityData.rows.Count > 0)
                    {
                        entityEditSectionRowProperty.entityAuxData.entities = new Dictionary<string, List<EntityEditSectionRow>>();
                        entityEditSectionRowProperty.entityAuxData.rows = GetRowsEdit(null, pItemEditSectionRowProperty.auxEntityData.rows, pData, pCombos, pComboAutorizacionesProyectos, pCombosDependency, pTesauros, pLang, pGraph);
                        entityEditSectionRowProperty.entityAuxData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                        entityEditSectionRowProperty.entityAuxData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                        foreach (string id in entityEditSectionRowProperty.values)
                        {
                            entityEditSectionRowProperty.entityAuxData.entities.Add(id, GetRowsEdit(id, pItemEditSectionRowProperty.auxEntityData.rows, pData, pCombos, pComboAutorizacionesProyectos, pCombosDependency, pTesauros, pLang, pGraph));
                            if (!string.IsNullOrEmpty(entityEditSectionRowProperty.entityAuxData.propertyOrder))
                            {
                                string orden = GetPropertiesEdit(id, new ItemEditSectionRowProperty() { property = entityEditSectionRowProperty.entityAuxData.propertyOrder }, pData, pCombos, pComboAutorizacionesProyectos, pCombosDependency, pTesauros, pLang, pGraph).values.FirstOrDefault();
                                int.TryParse(orden, out int ordenInt);
                                entityEditSectionRowProperty.entityAuxData.childsOrder[id] = ordenInt;
                            }

                            if (pItemEditSectionRowProperty.auxEntityData.propertyTitle != null)
                            {
                                string title = GetPropValues(id, UtilityCV.GetPropComplete(pItemEditSectionRowProperty.auxEntityData.propertyTitle), pData).FirstOrDefault();
                                string routeTitle = pItemEditSectionRowProperty.auxEntityData.propertyTitle.GetRoute();
                                if (string.IsNullOrEmpty(title))
                                {
                                    title = "";
                                }
                                entityEditSectionRowProperty.entityAuxData.titles.Add(id, new EntityEditRepresentativeProperty() { value = title, route = routeTitle });
                            }

                            if (pItemEditSectionRowProperty.auxEntityData.properties != null)
                            {
                                foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.auxEntityData.properties)
                                {
                                    List<string> valores = GetPropValues(id, UtilityCV.GetPropComplete(entityProperty.child), pData);
                                    string routeProp = entityProperty.child.GetRoute();
                                    if (!entityEditSectionRowProperty.entityAuxData.properties.ContainsKey(id))
                                    {
                                        entityEditSectionRowProperty.entityAuxData.properties[id] = new List<EntityEditRepresentativeProperty>();
                                    }

                                    entityEditSectionRowProperty.entityAuxData.properties[id].Add(new EntityEditRepresentativeProperty()
                                    {
                                        name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                                        value = string.Join(", ", valores),
                                        route = routeProp
                                    });

                                }
                            }
                        }
                    }
                }

                if (pItemEditSectionRowProperty.type == DataTypeEdit.entity)
                {
                    if (pItemEditSectionRowProperty.entityData != null)
                    {
                        entityEditSectionRowProperty.entityData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                        entityEditSectionRowProperty.entityData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                        foreach (string id in entityEditSectionRowProperty.values)
                        {
                            if (pItemEditSectionRowProperty.entityData.propertyTitle != null)
                            {
                                string title = GetPropValues(id, UtilityCV.GetPropComplete(pItemEditSectionRowProperty.entityData.propertyTitle), pData).FirstOrDefault();
                                string routeTitle = pItemEditSectionRowProperty.entityData.propertyTitle.GetRoute();
                                if (string.IsNullOrEmpty(title))
                                {
                                    title = "";
                                }
                                entityEditSectionRowProperty.entityData.titles.Add(id, new EntityEditRepresentativeProperty() { value = title, route = pItemEditSectionRowProperty.entityData.graph + "||" + routeTitle });
                            }

                            if (pItemEditSectionRowProperty.entityData.properties != null)
                            {
                                foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.entityData.properties)
                                {
                                    List<string> valores = GetPropValues(id, UtilityCV.GetPropComplete(entityProperty.child), pData);
                                    string routeProp = entityProperty.child.GetRoute();
                                    if (!entityEditSectionRowProperty.entityData.properties.ContainsKey(id))
                                    {
                                        entityEditSectionRowProperty.entityData.properties[id] = new List<EntityEditRepresentativeProperty>();
                                    }

                                    entityEditSectionRowProperty.entityData.properties[id].Add(new EntityEditRepresentativeProperty()
                                    {
                                        name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                                        value = string.Join(", ", valores),
                                        route = pItemEditSectionRowProperty.entityData.graph + "||" + routeProp
                                    });

                                }
                            }
                        }
                    }
                }

                if (pItemEditSectionRowProperty.dependency != null)
                {
                    entityEditSectionRowProperty.dependency = new Dependency()
                    {
                        parent = pItemEditSectionRowProperty.dependency.property
                    };
                    if (!string.IsNullOrEmpty(pItemEditSectionRowProperty.dependency.propertyValue))
                    {
                        entityEditSectionRowProperty.dependency.parentDependencyValue = pItemEditSectionRowProperty.dependency.propertyValue.Replace("{GraphsUrl}", mResourceApi.GraphsUrl);
                    }
                    if (!string.IsNullOrEmpty(pItemEditSectionRowProperty.dependency.propertyValueDistinct))
                    {
                        entityEditSectionRowProperty.dependency.parentDependencyValueDistinct = pItemEditSectionRowProperty.dependency.propertyValueDistinct.Replace("{GraphsUrl}", mResourceApi.GraphsUrl);
                    }
                }
                entityEditSectionRowProperty.entity_cv = pItemEditSectionRowProperty.entity_cv;

                return entityEditSectionRowProperty;
            }
        }
        #endregion

        #region Métodos de recolección de datos

        public Dictionary<string, List<ThesaurusItem>> GetTesauros(List<string> pListaTesauros, string pLang)
        {
            Dictionary<string, List<ThesaurusItem>> elementosTesauros = new Dictionary<string, List<ThesaurusItem>>();
            foreach (string tesauro in pListaTesauros)
            {
                string claveTesauros = $@"{tesauro} {pLang}";
                if (dicTesauros.ContainsKey(claveTesauros))
                {
                    elementosTesauros.Add(tesauro, dicTesauros[claveTesauros]);
                }
                else
                {

                    string select = "select * ";
                    string where = @$"where {{
                    ?s a <http://www.w3.org/2008/05/skos#Concept>.
                    ?s <http://www.w3.org/2008/05/skos#prefLabel> ?nombre.
                    FILTER( lang(?nombre) = '{pLang}' OR lang(?nombre) = '')  
                    ?s <http://purl.org/dc/elements/1.1/source> '{tesauro}'
                    OPTIONAL {{ ?s <http://www.w3.org/2008/05/skos#broader> ?padre }}
                }} ORDER BY ?padre ?s ";
                    SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "taxonomy");

                    List<ThesaurusItem> items = sparqlObject.results.bindings.Select(x => new ThesaurusItem()
                    {
                        id = x["s"].value,
                        name = x["nombre"].value,
                        parentId = x.ContainsKey("padre") ? x["padre"].value : ""
                    }).ToList();
                    elementosTesauros.Add(tesauro, items);
                    dicTesauros[claveTesauros] = items;
                }
            }

            return elementosTesauros;
        }

        /// <summary>
        /// Obtiene los datos para los combos (entidad y texto)
        /// </summary>
        /// <param name="pItemEditSectionRowPropertyCombo">Configuración de un combo para edición</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetSubjectsCombo(ItemEditSectionRowPropertyCombo pItemEditSectionRowPropertyCombo, string pLang)
        {
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> respuesta = new Dictionary<string, List<Dictionary<string, Data>>>();
            string claveCombos = $@"{pItemEditSectionRowPropertyCombo.property.property} {pItemEditSectionRowPropertyCombo.property.graph} {pItemEditSectionRowPropertyCombo.property.property} {pItemEditSectionRowPropertyCombo.rdftype} {pItemEditSectionRowPropertyCombo.graph} {pLang}";
            if (pItemEditSectionRowPropertyCombo.filter != null)
            {

                claveCombos += $@"{pItemEditSectionRowPropertyCombo.filter.property} {pItemEditSectionRowPropertyCombo.filter.value}";
            }
            if (pItemEditSectionRowPropertyCombo.cache && dicCombos.ContainsKey(claveCombos))
            {
                respuesta = dicCombos[claveCombos];
            }
            else
            {
                int paginacion = 10000;
                int offset = 0;
                int limit = paginacion;
                HashSet<string> ids = new HashSet<string>();
                while (limit == paginacion)
                {
                    string filter = "";
                    if (pItemEditSectionRowPropertyCombo.filter != null)
                    {
                        filter = $" . ?s <{pItemEditSectionRowPropertyCombo.filter.property}> '{pItemEditSectionRowPropertyCombo.filter.value}'";
                    }
                    //Obtenemos los IDS
                    string select = "select * where{select distinct ?s ";
                    string where = $"where{{?s a <{pItemEditSectionRowPropertyCombo.rdftype}> {filter} }} order by asc(?s)}} limit {limit} offset {offset}";
                    SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, pItemEditSectionRowPropertyCombo.graph);
                    limit = sparqlObject.results.bindings.Count;
                    offset += sparqlObject.results.bindings.Count;
                    foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObject.results.bindings)
                    {
                        ids.Add(fila["s"].value);
                    }
                }
                List<PropertyData> propertyDatas = new List<PropertyData>() { pItemEditSectionRowPropertyCombo.property.GenerarPropertyData(pItemEditSectionRowPropertyCombo.graph) };


                if (pItemEditSectionRowPropertyCombo.dependency != null)
                {
                    propertyDatas.Add(new Utils.PropertyData()
                    {
                        property = pItemEditSectionRowPropertyCombo.dependency.property,
                        order = null,
                        childs = new List<Utils.PropertyData>()
                    });
                }
                respuesta = UtilityCV.GetProperties(ids, pItemEditSectionRowPropertyCombo.graph, propertyDatas, pLang, new Dictionary<string, SparqlObject>());
                dicCombos[claveCombos] = respuesta;
            }
            return respuesta;
        }

        /// <summary>
        /// Obtiene los valores de una propiedad para una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pProp">Propiedad</param>
        /// <param name="pData">Datos cargados</param>
        /// <returns></returns>
        public static List<string> GetPropValues(string pId, string pProp, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData)
        {
            List<string> idAux = new List<string>();

            List<string> propInList = pProp.Split(new string[] { "||" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            if (propInList.Count > 1)
            {
                List<PropertyData> propertyDataAux = new List<PropertyData>();
                foreach (string propIn in propInList)
                {
                    PropertyData last = null;
                    string[] props = propIn.Split(new string[] { "@@@" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string prop in props)
                    {
                        if (last == null)
                        {
                            if (propertyDataAux.Exists(x => x.property == prop))
                            {
                                last = propertyDataAux.First(x => x.property == prop);
                            }
                            else
                            {
                                last = new PropertyData()
                                {
                                    property = prop,
                                    childs = new List<PropertyData>()
                                };
                                propertyDataAux.Add(last);
                            }
                        }
                        else
                        {
                            if (last.childs.Exists(x => x.property == prop))
                            {
                                last = last.childs.First(x => x.property == prop);
                            }
                            else
                            {
                                PropertyData aux = last;
                                last = new PropertyData()
                                {
                                    property = prop,
                                    childs = new List<PropertyData>()
                                };
                                aux.childs.Add(last);
                            }
                        }
                    }
                }
                List<string> finalList = new List<string>();
                foreach (PropertyData propertyData in propertyDataAux)
                {
                    idAux.AddRange(GetPropValuesAux(new List<string>() { pId }, propertyData, pData));
                }
            }
            else
            {
                idAux = new List<string>() { pId };
                string[] props = pProp.Split(new string[] { "@@@" }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string prop in props)
                {
                    List<string> idAux2 = new List<string>();
                    foreach (string id in idAux)
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            if (pData.ContainsKey(id))
                            {
                                idAux2.AddRange(pData[id].Where(x => x["p"].value == prop).Select(x => x["o"].value).Distinct().ToList());
                            }
                        }
                    }
                    idAux = idAux2;
                }
            }
            return idAux;
        }

        /// <summary>
        /// Método auxiliar para cuando la propiedad es un OR
        /// </summary>
        /// <param name="pIds">Identificadores</param>
        /// <param name="pPropertyData">Propiedades a recuperar</param>
        /// <param name="pData">Datos cargados</param>
        /// <returns></returns>
        private static List<string> GetPropValuesAux(List<string> pIds, PropertyData pPropertyData, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData)
        {
            List<string> values = new List<string>();
            List<string> aux = new List<string>();
            foreach (string id in pIds)
            {
                aux = GetPropValues(id, pPropertyData.property, pData);
            }
            if (pPropertyData.childs != null && pPropertyData.childs.Count > 0)
            {
                foreach (string id in aux)
                {
                    List<string> aux2 = new List<string>();
                    foreach (PropertyData propertyData in pPropertyData.childs)
                    {
                        aux2 = GetPropValuesAux(new List<string>() { id }, propertyData, pData);
                        if (aux2.Count > 0)
                        {
                            break;
                        }
                    }
                    values.AddRange(aux2);
                }
            }
            else
            {
                values = aux;
            }
            return values;
        }

        /// <summary>
        /// Hace la petición al servicio para obtener los descriptores temáticos y específicos.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        /// <param name="pTitulo">Título.</param>
        /// <param name="pDesc">Descripción.</param>
        /// <param name="pUrlPdf">URL del PDF.</param>
        /// <returns>Objeto con los datos obtenidos.</returns>
        /// <exception cref="Exception">StatusCode del error (Error al hacer la petición).</exception>
        public EnrichmentResponseGlobal GetEnrichment(ConfigService pConfig, string pTitulo, string pDesc, string pUrlPdf)
        {
            // Obtención del tesáuro.
            if (tuplaTesauro == null)
            {
                tuplaTesauro = ObtenerDatosTesauro();
            }

            // Cliente.
            EnrichmentResponseGlobal respuesta = new EnrichmentResponseGlobal();
            respuesta.tags = new EnrichmentResponse();
            respuesta.tags.topics = new List<EnrichmentResponseItem>();
            respuesta.categories = new EnrichmentResponseCategory();
            respuesta.categories.topics = new List<List<EnrichmentResponseCategory.EnrichmentResponseItem>>();
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromDays(1);

            // Si la descripción llega nula o vacía...
            if (string.IsNullOrEmpty(pDesc))
            {
                pDesc = string.Empty;
            }

            EnrichmentData enrichmentData = new EnrichmentData() { rotype = "papers", title = pTitulo, abstract_ = pDesc, pdfurl = pUrlPdf };

            // Conversión de los datos.
            string informacion = JsonConvert.SerializeObject(enrichmentData,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            StringContent contentData = new StringContent(informacion, System.Text.Encoding.UTF8, "application/json");

            #region --- Tópicos Específicos (Tags)
            var responseSpecific = client.PostAsync(pConfig.GetUrlSpecificEnrichment(), contentData).Result;

            if (responseSpecific.IsSuccessStatusCode)
            {
                respuesta.tags = responseSpecific.Content.ReadAsAsync<EnrichmentResponse>().Result;
            }
            else
            {
                return respuesta;
            }
            #endregion

            #region --- Tópicos Temáticos (Categorías)            
            var responseThematic = client.PostAsync(pConfig.GetUrlThematicEnrichment(), contentData).Result;

            EnrichmentResponse categoriasObtenidas = null;

            if (responseThematic.IsSuccessStatusCode)
            {
                categoriasObtenidas = responseThematic.Content.ReadAsAsync<EnrichmentResponse>().Result;
                EnrichmentResponseCategory listaCategoria = new EnrichmentResponseCategory();
                listaCategoria.topics = new List<List<EnrichmentResponseCategory.EnrichmentResponseItem>>();

                foreach (EnrichmentResponseItem cat in categoriasObtenidas.topics)
                {
                    List<EnrichmentResponseCategory.EnrichmentResponseItem> listaTesauro = new List<EnrichmentResponseCategory.EnrichmentResponseItem>();

                    if (tuplaTesauro.Item2.ContainsKey("general " + cat.word.ToLower().Trim()))
                    {
                        string idTesauro = tuplaTesauro.Item2["general " + cat.word.ToLower().Trim()];
                        listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });

                        while (!idTesauro.EndsWith(".0.0.0"))
                        {
                            idTesauro = ObtenerIdTesauro(idTesauro);
                            listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });
                        }
                    }
                    else if (tuplaTesauro.Item2.ContainsKey(cat.word.ToLower().Trim()))
                    {
                        string idTesauro = tuplaTesauro.Item2[cat.word.ToLower().Trim()];
                        listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });

                        while (!idTesauro.EndsWith(".0.0.0"))
                        {
                            idTesauro = ObtenerIdTesauro(idTesauro);
                            listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });
                        }
                    }


                    if (listaTesauro.Count > 0)
                    {
                        listaCategoria.topics.Add(listaTesauro);
                    }
                }
                respuesta.categories = listaCategoria;
            }
            else
            {
                return respuesta;
            }
            #endregion

            return respuesta;
        }

        /// <summary>
        /// Obtiene la ctaegoría padre.
        /// </summary>
        /// <param name="pIdTesauro">Categoría a consultar.</param>
        /// <returns>Categoría padre.</returns>
        private string ObtenerIdTesauro(string pIdTesauro)
        {
            string idTesauro = pIdTesauro.Split(new[] { "researcharea_" }, StringSplitOptions.None)[1];
            int num1 = Int32.Parse(idTesauro.Split('.')[0]);
            int num2 = Int32.Parse(idTesauro.Split('.')[1]);
            int num3 = Int32.Parse(idTesauro.Split('.')[2]);
            int num4 = Int32.Parse(idTesauro.Split('.')[3]);

            if (num4 != 0)
            {
                idTesauro = $@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.{num2}.{num3}.0";
            }
            else if (num3 != 0 && num4 == 0)
            {
                idTesauro = $@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.{num2}.0.0";
            }
            else if (num2 != 0 && num3 == 0 && num4 == 0)
            {
                idTesauro = $@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.0.0.0";
            }

            return idTesauro;
        }

        /// <summary>
        /// Obtiene las categorías del tesáuro.
        /// </summary>
        /// <returns>Tupla con (clave) diccionario de las idCategorias-idPadre y (valor) diccionario de nombreCategoria-idCategoria.</returns>
        private static Tuple<Dictionary<string, string>, Dictionary<string, string>> ObtenerDatosTesauro()
        {
            Dictionary<string, string> dicAreasBroader = new Dictionary<string, string>();
            Dictionary<string, string> dicAreasNombre = new Dictionary<string, string>();

            string select = @"SELECT DISTINCT * ";
            string where = @$"WHERE {{
                ?concept a <http://www.w3.org/2008/05/skos#Concept>.
                ?concept <http://www.w3.org/2008/05/skos#prefLabel> ?nombre.
                ?concept <http://purl.org/dc/elements/1.1/source> 'researcharea'
                OPTIONAL{{?concept <http://www.w3.org/2008/05/skos#broader> ?broader}}
                }}";
            SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "taxonomy");

            foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            {
                string concept = fila["concept"].value;
                string nombre = fila["nombre"].value;
                string broader = "";
                if (fila.ContainsKey("broader"))
                {
                    broader = fila["broader"].value;
                }
                dicAreasBroader.Add(concept, broader);
                if (!dicAreasNombre.ContainsKey(nombre.ToLower()))
                {
                    dicAreasNombre.Add(nombre.ToLower(), concept);
                }
            }

            Dictionary<string, string> dicAreasUltimoNivel = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> item in dicAreasNombre)
            {
                bool tieneHijos = false;
                string id = item.Value.Split(new[] { "researcharea_" }, StringSplitOptions.None)[1];
                int num1 = Int32.Parse(id.Split('.')[0]);
                int num2 = Int32.Parse(id.Split('.')[1]);
                int num3 = Int32.Parse(id.Split('.')[2]);
                int num4 = Int32.Parse(id.Split('.')[3]);

                if (num2 == 0 && num3 == 0 && num4 == 0)
                {
                    tieneHijos = dicAreasNombre.ContainsValue($@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.1.0.0");
                }
                else if (num3 == 0 && num4 == 0)
                {
                    tieneHijos = dicAreasNombre.ContainsValue($@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.{num2}.1.0");
                }
                else if (num4 == 0)
                {
                    tieneHijos = dicAreasNombre.ContainsValue($@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.{num2}.{num3}.1");
                }

                if (!tieneHijos)
                {
                    dicAreasUltimoNivel.Add(item.Key, item.Value);
                }
            }

            return new Tuple<Dictionary<string, string>, Dictionary<string, string>>(dicAreasBroader, dicAreasUltimoNivel);
        }

        #endregion



    }
}