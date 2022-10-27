using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.UpdateKeywords.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using static Hercules.ED.UpdateKeywords.Program;

namespace Hercules.ED.UpdateKeywords
{
    public class UtilKeywords
    {
        private readonly ResourceApi mResourceApi;
        private readonly static string RUTA_PREFIJOS = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}prefijos.json";
        private readonly static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));
        readonly ConfigService _Configuracion;
        private const int MAX_NUM_HILOS = 6;
        private const int MAX_NUM_INTENTOS = 10;

        // Lista de palabras y carácteres.
        private readonly List<string> preposicionesEng = new() { "above", "across", "along", "around", "against", "at", "behind", "beside", "below", "beneath", "between", "by", "close to", "in", "in front of", "inside", "near", "on", "opposite", "outside", "over", "under", "underneath", "upon", "about", "after", "around", "before", "beyond", "by", "during", "for", "past", "since", "throughout", "until", "across", "along", "around", "away from", "down", "from", "into", "off", "onto", "out of", "over", "past", "to", "towards", "under", "up", "in", "at", "on", "ago", "circa", "per", "about", "at", "from", "for", "in", "of", "to", "with", "a", "an", "some", "the", "it", "its", "after", "although", "and", "as", "as long as", "as soon as", "as well as", "because", "befpre", "both", "but", "either", "even if", "even though", "however", "if", "in case", "in order to", "moreover", "neither", "nor", "nevertheless", "now that", "or", "once", "since", "so", "so that", "then", "therefore", "though", "unless", "until", "when", "whereas", "whether", "yet" };
        private readonly List<string> preposicionesEsp = new() { "a", "ante", "bajo", "cabe", "con", "contra", "de", "desde", "durante", "en", "entre", "hacia", "hasta", "mediante", "para", "por", "según", "sin", "so", "sobre", "tras", "versus", "via", "y", "el", "la", "los", "las", "un", "una", "unos", "unas", "lo" };
        private readonly List<string> caracteres = new() { "\\", "|", "\"", "·", "$", "%", "&", "/", "=", "?", "¿", "º", "!", "@", "#", "~", "€", "¬", "¡", "[", "]", "{", "}", "^", "*", "¨", ";", ":", "_", "`", "+", "´", ",", "<", ">", ".", "(", ")", "'" };

        public UtilKeywords(ResourceApi pResourceApi, ConfigService pConfig)
        {
            this.mResourceApi = pResourceApi;
            this._Configuracion = pConfig;
        }

        /// <summary>
        /// Obtiene los recursos que hayan de hacer el matching.
        /// </summary>
        /// <returns>Lista de IDs.</returns>
        public List<string> GetDocument()
        {
            mResourceApi.ChangeOntoly("document");
            List<string> listaDocumentos = new();
            SparqlObject resultadoQuery;

            // Consulta sparql.
            string select = $@"{mPrefijos} SELECT DISTINCT ?s ";
            string where = $@"WHERE {{
                            ?s a bibo:Document.
                            ?s roh:getKeyWords 'true'. 
                            ?s roh:hasKnowledgeArea ?area. 
                            ?area roh:categoryNode <{mResourceApi.GraphsUrl}items/researcharea_3.0.0.0>. 
                            ?s bibo:authorList ?listaAutores. 
                            ?listaAutores rdf:member ?persona. 
                            ?persona roh:useMatching 'true'. 
                        }}";

            while (true)
            {
                try
                {
                    resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "document", "person" });
                    break;
                }
                catch (Exception error)
                {
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.Message}");
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.StackTrace}");
                    Thread.Sleep(1000);
                }
            }

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("s") && !string.IsNullOrEmpty(fila["s"].value))
                    {
                        listaDocumentos.Add(fila["s"].value);
                    }
                }
            }

            return listaDocumentos;
        }

        /// <summary>
        /// Obtiene las palabras clave.
        /// </summary>
        /// <param name="pIdRecurso">ID del recurso.</param>
        /// <returns>Diccionario resultante.</returns>
        public Dictionary<string, string> GetKeywords(string pIdRecurso)
        {
            mResourceApi.ChangeOntoly("document");
            Dictionary<string, string> dicEtiquetas = new();
            SparqlObject resultadoQuery;

            // Consulta sparql.
            string select = $@"{mPrefijos} SELECT DISTINCT ?freeTextKeyword ?etiqueta ";
            string where = $@"WHERE {{ 
                                <{pIdRecurso}> vivo:freeTextKeyword ?freeTextKeyword.
                                ?freeTextKeyword roh:title ?etiqueta. 
                                MINUS {{ ?freeTextKeyword roh:keyWordConcept ?keywordConcept. }}
                            }}";

            while (true)
            {
                try
                {
                    resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
                    break;
                }
                catch (Exception error)
                {
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.Message}");
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.StackTrace}");
                    Thread.Sleep(1000);
                }
            }

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    string idRecurso = string.Empty;
                    string nombreEtiqueta = string.Empty;

                    if (fila.ContainsKey("freeTextKeyword") && !string.IsNullOrEmpty(fila["freeTextKeyword"].value))
                    {
                        idRecurso = fila["freeTextKeyword"].value;
                    }

                    if (fila.ContainsKey("etiqueta") && !string.IsNullOrEmpty(fila["etiqueta"].value))
                    {
                        nombreEtiqueta = fila["etiqueta"].value;
                    }

                    if (!string.IsNullOrEmpty(idRecurso) && (!string.IsNullOrEmpty(nombreEtiqueta)))
                    {
                        dicEtiquetas.Add(idRecurso, nombreEtiqueta);
                    }
                }
            }

            return dicEtiquetas;
        }

        /// <summary>
        /// Obtiene el objeto de la etiqueta si está cargada en BBDD.
        /// </summary>
        /// <param name="pUrl">URL del dato a consultar.</param>
        /// <returns>Objeto con datos recuperados.</returns>
        public string GetUriTag(string pUrl)
        {
            SparqlObject resultadoQuery;

            // Consulta sparql.
            string select = $@"{mPrefijos} SELECT DISTINCT ?s ";
            string where = $@"WHERE {{
                                ?s a roh:KeyWordConcept. 
                                ?s roh:url '{pUrl}'. 
                            ";

            while (true)
            {
                try
                {
                    resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "keywordconcept");
                    break;
                }
                catch (Exception error)
                {
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.Message}");
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.StackTrace}");
                    Thread.Sleep(1000);
                }
            }

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                return resultadoQuery.results.bindings[0]["s"].value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Carga de datos.
        /// </summary>
        /// <param name="pData">Datos de SNOMED.</param>
        /// <param name="pDicIdsSnomed">IDs de SNOMED.</param>
        /// <returns>ID del recurso.</returns>
        public string CargarDataConceptCompleto(DataConcept pData, Dictionary<string, List<Dictionary<string, string>>> pDicIdsSnomed = null)
        {
            mResourceApi.ChangeOntoly("keywordconcept");

            // 1º - Por cada DataConcept relacionado ejecutamos CargarDataConceptParcial.
            ConcurrentBag<string> listBroaders = new();
            if (pData.Broader != null)
            {
                Parallel.ForEach(pData.Broader.Keys, new ParallelOptions { MaxDegreeOfParallelism = MAX_NUM_HILOS }, idBroader =>
                {
                    listBroaders.Add(CargarDataConceptParcial(idBroader, pData.Broader[idBroader], pData.Type));
                });
            }

            ConcurrentBag<string> listQualifiers = new();
            if (pData.Qualifiers != null)
            {
                Parallel.ForEach(pData.Qualifiers.Keys, new ParallelOptions { MaxDegreeOfParallelism = MAX_NUM_HILOS }, idQualifiers =>
                {
                    listQualifiers.Add(CargarDataConceptParcial(idQualifiers, pData.Qualifiers[idQualifiers], pData.Type));
                });
            }

            ConcurrentBag<string> listRelatedTo = new();
            if (pData.RelatedTo != null)
            {
                Parallel.ForEach(pData.RelatedTo.Keys, new ParallelOptions { MaxDegreeOfParallelism = MAX_NUM_HILOS }, idRelatedTo =>
                {
                    listRelatedTo.Add(CargarDataConceptParcial(idRelatedTo, pData.RelatedTo[idRelatedTo], pData.Type));
                });
            }

            ConcurrentBag<string> listCloseMatch = new();
            if (pData.CloseMatch != null)
            {
                Parallel.ForEach(pData.CloseMatch.Keys, new ParallelOptions { MaxDegreeOfParallelism = MAX_NUM_HILOS }, idCloseMatch =>
                {
                    listCloseMatch.Add(CargarDataConceptParcial(idCloseMatch, pData.CloseMatch[idCloseMatch], pData.Type));
                });
            }

            ConcurrentBag<string> listExactMatch = new();
            if (pData.ExactMatch != null)
            {
                Parallel.ForEach(pData.ExactMatch.Keys, new ParallelOptions { MaxDegreeOfParallelism = MAX_NUM_HILOS }, idExactMatch =>
                {
                    listExactMatch.Add(CargarDataConceptParcial(idExactMatch, pData.ExactMatch[idExactMatch], pData.Type));
                });
            }

            // 2º - Una vez terminado, creamos/modificamos el DataConcept actual.
            KeywordconceptOntology.KeyWordConcept keyWordConcept = new();
            keyWordConcept.Roh_title = pData.Title;
            keyWordConcept.Roh_url = pData.Url;
            keyWordConcept.IdsRoh_broaders = listBroaders.Distinct().ToList();
            keyWordConcept.IdsRoh_qualifiers = listQualifiers.Distinct().ToList();
            keyWordConcept.IdsRoh_relatedTo = listRelatedTo.Distinct().ToList();
            keyWordConcept.IdsSkos_closeMatch = listCloseMatch.Distinct().ToList();
            keyWordConcept.IdsSkos_exactMatch = listExactMatch.Distinct().ToList();
            keyWordConcept.Roh_type = pData.Type;

            // Si es una KeyWordConcept de Mesh...
            if (keyWordConcept.Roh_url.Contains('/'))
            {
                List<string> listaIdsSnomedCargados = new();
                string idMesh = keyWordConcept.Roh_url.Substring(keyWordConcept.Roh_url.LastIndexOf("/") + 1);

                if (pDicIdsSnomed.ContainsKey(idMesh))
                {
                    foreach (KeyValuePair<string, List<Dictionary<string, string>>> item1 in pDicIdsSnomed)
                    {
                        if (item1.Key == idMesh)
                        {
                            foreach (Dictionary<string, string> item2 in item1.Value)
                            {
                                foreach (string idGnoss in item2.Values)
                                {
                                    listaIdsSnomedCargados.Add(idGnoss);
                                }
                            }
                        }
                    }
                }

                // Asignación del ID relacionado con Snomed.
                if (listaIdsSnomedCargados.Any())
                {
                    keyWordConcept.IdsRoh_match = listaIdsSnomedCargados;
                }
            }

            // Carga/Modificación.
            string id = GetUriTag(pData.Url);
            if (string.IsNullOrEmpty(id))
            {
                ComplexOntologyResource resource = keyWordConcept.ToGnossApiResource(mResourceApi, null);
                int numIntentos = 0;
                while (!resource.Uploaded)
                {
                    numIntentos++;

                    if (numIntentos > MAX_NUM_INTENTOS)
                    {
                        break;
                    }

                    mResourceApi.LoadComplexSemanticResource(resource, false, true);

                    if (!resource.Uploaded)
                    {
                        Thread.Sleep(numIntentos * 1000);
                    }
                }
            }
            else
            {
                string[] idSplit = id.Split('_');
                ComplexOntologyResource resource = keyWordConcept.ToGnossApiResource(mResourceApi, null, new Guid(idSplit[idSplit.Length - 2]), new Guid(idSplit[idSplit.Length - 1]));
                int numIntentos = 0;
                while (!resource.Modified)
                {
                    numIntentos++;

                    if (numIntentos > MAX_NUM_INTENTOS)
                    {
                        break;
                    }

                    mResourceApi.ModifyComplexOntologyResource(resource, false, true);

                    if (!resource.Modified)
                    {
                        Thread.Sleep(numIntentos * 1000);
                    }
                }
            }

            foreach (KeyValuePair<string, List<Dictionary<string, string>>> item1 in pDicIdsSnomed)
            {
                foreach (Dictionary<string, string> item2 in item1.Value)
                {
                    if (item2.ContainsKey(keyWordConcept.Roh_url))
                    {
                        item2[keyWordConcept.Roh_url] = id;
                    }
                }
            }

            return id;
        }

        /// <summary>
        /// Carga los datos de concept de forma parcial.
        /// </summary>
        /// <param name="pIdUrl">URL.</param>
        /// <param name="pNombre">Nombre.</param>
        /// <param name="pTipo">Tipo.</param>
        /// <returns>ID del recurso.</returns>
        public string CargarDataConceptParcial(string pIdUrl, string pNombre, string pTipo)
        {
            // 1º - Si existe devuelve el ID.
            string id = GetUriTag(pIdUrl);

            // 2º - Si no existe cargamos el DataMesh.
            if (string.IsNullOrEmpty(id))
            {
                // Creación del objeto a cargar.
                KeywordconceptOntology.KeyWordConcept keyWordConcept = new();
                keyWordConcept.Roh_title = pNombre;
                keyWordConcept.Roh_url = pIdUrl;
                keyWordConcept.Roh_type = pTipo;
                ComplexOntologyResource resource = keyWordConcept.ToGnossApiResource(mResourceApi, null);

                // Carga.
                int numIntentos = 0;
                while (!resource.Uploaded)
                {
                    numIntentos++;

                    if (numIntentos > MAX_NUM_INTENTOS)
                    {
                        break;
                    }

                    mResourceApi.LoadComplexSemanticResource(resource, false, true);

                    if (!resource.Uploaded)
                    {
                        Thread.Sleep(numIntentos * 1000);
                    }
                }
            }

            return id;
        }

        /// <summary>
        /// Modifica la entidad Auxiliar de KeyWord.
        /// </summary>
        /// <param name="idDocumento">ID del documento.</param>
        /// <param name="pIdRecurso">ID del recurso a modificar.</param>
        /// <param name="pKeyword">Palabra a cambiar.</param>
        public void ModificarKeyword(string idDocumento, string pIdRecurso, string pKeyword)
        {
            Guid guid = mResourceApi.GetShortGuid(idDocumento);
            Dictionary<Guid, List<TriplesToInclude>> dicInclude = new();
            List<TriplesToInclude> listaTriplesInclude = new();

            TriplesToInclude triple = new();
            triple.Predicate = $@"http://vivoweb.org/ontology/core#freeTextKeyword|http://w3id.org/roh/keyWordConcept";
            triple.NewValue = pIdRecurso + "|" + pKeyword;
            listaTriplesInclude.Add(triple);

            dicInclude.Add(guid, listaTriplesInclude);
            mResourceApi.InsertPropertiesLoadedResources(dicInclude);
        }

        /// <summary>
        /// Modifica un documento.
        /// </summary>
        /// <param name="idDocumento">ID del documento.</param>
        public void ModificarGetKeywordDocument(string idDocumento)
        {
            Guid guid = mResourceApi.GetShortGuid(idDocumento);
            Dictionary<Guid, List<TriplesToModify>> dicInclude = new();
            List<TriplesToModify> listaTriplesModificacion = new();

            TriplesToModify triple = new();
            triple.Predicate = $@"http://w3id.org/roh/getKeyWords";
            triple.NewValue = "false";
            triple.OldValue = "true";
            listaTriplesModificacion.Add(triple);

            dicInclude.Add(guid, listaTriplesModificacion);
            mResourceApi.ModifyPropertiesLoadedResources(dicInclude);
        }

        /// <summary>
        /// Borra un triple.
        /// </summary>
        /// <param name="pIdRecurso">ID del recurso a borrar el triple.</param>
        public void BorrarGetKeywordProperty(string pIdRecurso)
        {
            Guid guid = mResourceApi.GetShortGuid(pIdRecurso);
            Dictionary<Guid, List<RemoveTriples>> dicBorrado = new();
            List<RemoveTriples> listaTriplesBorrado = new();

            RemoveTriples triple = new();
            triple.Predicate = $@"http://w3id.org/roh/getKeyWords";
            triple.Value = "true";
            listaTriplesBorrado.Add(triple);

            dicBorrado.Add(guid, listaTriplesBorrado);
            mResourceApi.DeletePropertiesLoadedResources(dicBorrado);
        }

        /// <summary>
        /// Obtención de los dataconcepts de SNOMED.
        /// </summary>
        /// <param name="pListaData">Lista de datos.</param>
        /// <returns>Devuelve la lista con los datos con objetos.</returns>
        public List<DataConcept> GetDataConcepts(List<Data> pListaData)
        {
            List<DataConcept> listaDataConcepts = new();

            foreach (Data data in pListaData)
            {
                DataConcept dataConceptMain = new();
                dataConceptMain.Title = data.SnomedTerm.Name;
                dataConceptMain.Url = data.SnomedTerm.Ui;
                dataConceptMain.Type = "snomed";

                foreach (ResultRelations itemRelacion in data.Relations)
                {
                    switch (itemRelacion.RelationLabel)
                    {
                        case "AQ":
                            if (dataConceptMain.Qualifiers == null)
                            {
                                dataConceptMain.Qualifiers = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.Qualifiers.ContainsKey(itemRelacion.Ui))
                            {
                                dataConceptMain.Qualifiers.Add(itemRelacion.Ui, itemRelacion.RelatedIdName);
                            }
                            break;
                        case "RL":
                            if (dataConceptMain.CloseMatch == null)
                            {
                                dataConceptMain.CloseMatch = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.CloseMatch.ContainsKey(itemRelacion.Ui))
                            {
                                dataConceptMain.CloseMatch.Add(itemRelacion.Ui, itemRelacion.RelatedIdName);
                            }
                            break;
                        case "SY":
                            if (dataConceptMain.ExactMatch == null)
                            {
                                dataConceptMain.ExactMatch = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.ExactMatch.ContainsKey(itemRelacion.Ui))
                            {
                                dataConceptMain.ExactMatch.Add(itemRelacion.Ui, itemRelacion.RelatedIdName);
                            }
                            break;
                        case "RO":
                            if (dataConceptMain.RelatedTo == null)
                            {
                                dataConceptMain.RelatedTo = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.RelatedTo.ContainsKey(itemRelacion.Ui))
                            {
                                dataConceptMain.RelatedTo.Add(itemRelacion.Ui, itemRelacion.RelatedIdName);
                            }
                            break;
                        case "RU":
                            if (dataConceptMain.RelatedTo == null)
                            {
                                dataConceptMain.RelatedTo = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.RelatedTo.ContainsKey(itemRelacion.Ui))
                            {
                                dataConceptMain.RelatedTo.Add(itemRelacion.Ui, itemRelacion.RelatedIdName);
                            }
                            break;
                        case "CHD":
                            if (dataConceptMain.Broader == null)
                            {
                                dataConceptMain.Broader = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.Broader.ContainsKey(itemRelacion.Ui))
                            {
                                dataConceptMain.Broader.Add(itemRelacion.Ui, itemRelacion.RelatedIdName);
                            }
                            break;
                    }
                }

                listaDataConcepts.Add(dataConceptMain);
            }

            if (!listaDataConcepts.Any())
            {
                return null;
            }
            else
            {
                return listaDataConcepts;
            }
        }

        /// <summary>
        /// Obtiene el ID y Valor de la consulta a Mesh.
        /// </summary>
        /// <param name="listaTopicalDescriptors">Tópico a consultar.</param>
        /// <param name="pTipoFiltro">True: Exact Match // False: All Fragments</param>
        /// <returns>Diccionario resultante.</returns>
        public Dictionary<string, string> SelectDataMesh(string[] listaTopicalDescriptors, bool pTipoFiltro)
        {
            Dictionary<string, string> dicResultados = new();

            // Endpoint.
            string urlConsulta = _Configuracion.GetSparqlEndpoint();

            // Consulta.
            string consulta = $@"
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> PREFIX meshv: <http://id.nlm.nih.gov/mesh/vocab#>
                SELECT ?a ?b ?c ?d ?e ?f ?g ?h ?i ?j ?k ?l ?m ?n ?o FROM <http://mesh.gnoss.com/edma>
                WHERE {{
                  {{?a a meshv:TopicalDescriptor}}.  
                  {{
                    ?a rdfs:label ?b.                    
                    {ContruirFiltro("b", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:concept ?x.
                    ?x rdfs:label ?c.
                    {ContruirFiltro("c", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:concept ?x.
                    ?x meshv:term ?x2.
                    ?x2 meshv:altLabel ?d.
                    {ContruirFiltro("d", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:concept ?x.
                    ?x meshv:term ?x2.
                    ?x2 meshv:prefLabel ?e.
                    {ContruirFiltro("e", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:concept ?x.
                    ?x meshv:term ?x2.
                    ?x2 meshv:sortVersion ?f.
                    {ContruirFiltro("f", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:concept ?x.
                    ?x meshv:preferredTerm ?x2.
                    ?x2 meshv:altLabel ?g.
                    {ContruirFiltro("g", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:concept ?x.
                    ?x meshv:preferredTerm ?x2.
                    ?x2 meshv:prefLabel ?h.
                    {ContruirFiltro("h", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:concept ?x.
                    ?x meshv:preferredTerm ?x2.
                    ?x2 meshv:sortVersion ?i.
                    {ContruirFiltro("i", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredConcept ?y.
                    ?y meshv:term ?x.
                    ?x meshv:altLabel ?j.
                    {ContruirFiltro("j", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredConcept ?y.
                    ?y meshv:term ?x.
                    ?x meshv:prefLabel ?k.
                    {ContruirFiltro("k", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredConcept ?y.
                    ?y meshv:term ?x.
                    ?x meshv:sortVersion ?l.
                    {ContruirFiltro("l", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredTerm ?x.
                    ?x meshv:altLabel ?m.
                    {ContruirFiltro("m", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredTerm ?x.
                    ?x meshv:prefLabel ?n.
                    {ContruirFiltro("n", listaTopicalDescriptors, pTipoFiltro)}
                  }}UNION{{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredTerm ?x.
                    ?x meshv:sortVersion ?o.
                    {ContruirFiltro("o", listaTopicalDescriptors, pTipoFiltro)}
                  }}}}ORDER BY ?a";

            // Petición.
            WebClient webClient = new();
            webClient.Encoding = Encoding.UTF8;
            webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

            NameValueCollection parametros = new();
            parametros.Add("query", consulta);
            parametros.Add("format", "application/sparql-results+json");

            byte[] responseArray = null;
            int numIntentos = 0;

            Exception exception = null;
            while (responseArray == null && numIntentos < 3)
            {
                numIntentos++;
                try
                {
                    responseArray = webClient.UploadValues(urlConsulta, "POST", parametros);
                    exception = null;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }
            if (exception != null)
            {
                return dicResultados;
            }

            // Obtención de datos.
            SparqlObject resultadoQuery = null;
            string jsonRespuesta = Encoding.UTF8.GetString(responseArray);
            if (!string.IsNullOrEmpty(jsonRespuesta))
            {
                resultadoQuery = JsonConvert.DeserializeObject<SparqlObject>(jsonRespuesta);
            }

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    string id = string.Empty;
                    string label = string.Empty;

                    if (fila.ContainsKey("a") && !string.IsNullOrEmpty(fila["a"].value))
                    {
                        id = fila["a"].value;
                        id = id.Substring(id.LastIndexOf("/") + 1);
                    }

                    if (fila.ContainsKey("b") && !string.IsNullOrEmpty(fila["b"].value))
                    {
                        label = fila["b"].value;
                    }

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(label) && !dicResultados.ContainsKey(id))
                    {
                        dicResultados.Add(id, label);
                    }
                }
            }

            webClient.Dispose();
            return dicResultados;
        }

        /// <summary>
        /// Obtiene los datos de MESH.
        /// </summary>
        /// <param name="pConsulta">Consulta SPARQL.</param>
        /// <returns>Diccionario resultante.</returns>
        public Dictionary<string, string> SelectDataMeshAllFragments(string pConsulta)
        {
            Dictionary<string, string> dicResultados = new();

            // Endpoint.
            string urlConsulta = _Configuracion.GetSparqlEndpoint();

            // Petición.
            WebClient webClient = new();

            SparqlObject resultadoQuery = null;
            webClient.Encoding = Encoding.UTF8;
            webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

            NameValueCollection parametros = new();
            parametros.Add("query", pConsulta);
            parametros.Add("format", "application/sparql-results+json");

            byte[] responseArray = null;
            int numIntentos = 0;

            // Reintenta la petición un número de veces por si falla.
            Exception exception = null;
            while (responseArray == null && numIntentos < MAX_NUM_INTENTOS)
            {
                numIntentos++;
                try
                {
                    responseArray = webClient.UploadValues(urlConsulta, "POST", parametros);
                    exception = null;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }
            if (exception != null)
            {
                return dicResultados;
            }

            // Obtención de los datos del resultado.
            string jsonRespuesta = Encoding.UTF8.GetString(responseArray);
            if (!string.IsNullOrEmpty(jsonRespuesta))
            {
                resultadoQuery = JsonConvert.DeserializeObject<SparqlObject>(jsonRespuesta);
            }

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    string id = string.Empty;
                    string label = string.Empty;

                    if (fila.ContainsKey("item") && !string.IsNullOrEmpty(fila["item"].value))
                    {
                        id = fila["item"].value;
                        id = id.Substring(id.LastIndexOf("/") + 1);
                    }

                    if (fila.ContainsKey("label") && !string.IsNullOrEmpty(fila["label"].value))
                    {
                        label = fila["label"].value;
                    }

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(label) && !dicResultados.ContainsKey(id))
                    {
                        dicResultados.Add(id, label);
                    }
                }
            }

            webClient.Dispose();
            if (dicResultados.Count != 1)
            {
                return new Dictionary<string, string>();
            }
            else
            {
                return dicResultados;
            }
        }

        /// <summary>
        /// Contruye los filtros.
        /// </summary>
        /// <param name="pVariable">Variable anterior.</param>
        /// <param name="pPalabras">Palabra a filtrar.</param>
        /// <param name="pTipoFiltro">Tipo de filtro.</param>
        /// <returns></returns>
        public string ContruirFiltro(string pVariable, string[] pPalabras, bool pTipoFiltro)
        {
            StringBuilder filtro = new();

            if (pTipoFiltro)
            {
                foreach (string palabra in pPalabras)
                {
                    string palabraAux = palabra.ToLower().Replace("\'", "\\\'");

                    if (GetPreposicionesEng().Contains(palabraAux) || GetPreposicionesEsp().Contains(palabraAux) || ComprobarCaracteres(palabraAux))
                    {
                        continue;
                    }

                    filtro.Append($@"FILTER(REGEX(?{pVariable},'^{palabraAux}$','i')) ");
                }
            }
            else
            {
                foreach (string palabra in pPalabras)
                {
                    string palabraAux = palabra.ToLower().Replace("\'", "\\\'");

                    if (GetPreposicionesEng().Contains(palabraAux) || GetPreposicionesEsp().Contains(palabraAux) || ComprobarCaracteres(palabraAux))
                    {
                        continue;
                    }

                    filtro.Append($@"{{
                        FILTER(REGEX(?{pVariable}, ' {palabraAux} ', 'i'))
                      }} UNION {{
                        FILTER(REGEX(?{pVariable}, '^{palabraAux}$', 'i'))
                      }} UNION {{
                        FILTER(REGEX(?{pVariable}, '^{palabraAux} ', 'i'))
                      }} UNION {{
                        FILTER(REGEX(?{pVariable}, ' {palabraAux}$', 'i'))
                    }} ");
                }
            }

            return filtro.ToString();
        }

        /// <summary>
        /// Obtiene la información de SNOMED mediante el IDMesh.
        /// </summary>
        /// <param name="pIdMesh">ID de Mesh.</param>
        /// <param name="pListaData">Lista de datos.</param>
        public void InsertDataSnomed(string pIdMesh, List<Data> pListaData)
        {
            // Obtención del TGT.
            string tgt = GetTGT();

            // Obtención del ST.
            string st = GetTicket(tgt);

            // Petición al servicio de obtención de ID.
            GetSnomedId(pIdMesh.Trim(), st, pListaData);

            // Obtención de la información de las relaciones.
            foreach (Data item in pListaData)
            {
                item.Relations = GetRelaciones(item.SnomedTerm.Ui, tgt);
            }
        }

        /// <summary>
        /// Inserta los datos en BBDD.
        /// </summary>
        /// <param name="pIdMesh">ID de MESH.</param>
        /// <param name="pNombre">Título.</param>
        /// <param name="pListaData">Lista de información.</param>
        public void InsertDataMesh(string pIdMesh, string pNombre, List<DataConcept> pListaData)
        {
            // Consulta.
            string consulta = $@"
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                PREFIX meshv: <http://id.nlm.nih.gov/mesh/vocab#>
                SELECT ?labelQualifiers ?qualifiers ?labelBroader ?broader ?labelRelatedTo ?relatedTo
                FROM <http://id.nlm.nih.gov/mesh>
                WHERE 
                {{
                  {{
	                ?item a meshv:TopicalDescriptor.
	                ?item meshv:identifier '{pIdMesh}'.
	                ?item meshv:allowableQualifier ?qualifiers.
                    ?qualifiers rdfs:label ?labelQualifiers.
                  }}
                  UNION
                  {{
	                ?item a meshv:TopicalDescriptor.
	                ?item meshv:identifier '{pIdMesh}'.
	                ?item meshv:broaderDescriptor ?broader.
                    ?broader rdfs:label ?labelBroader.
                  }} 
                  UNION
                  {{
	                ?item a meshv:TopicalDescriptor.
	                ?item meshv:identifier '{pIdMesh}'.
	                ?item meshv:seeAlso ?relatedTo.
                    ?relatedTo rdfs:label ?labelRelatedTo.
                  }} 
                }} 
            ";

            // Tipo de salida.
            string format = "JSON";

            // Petición.
            WebClient webClient = new();
            string downloadString = string.Empty;

            int contadorDownload = 0;
            while (string.IsNullOrEmpty(downloadString))
            {
                contadorDownload++;
                try
                {
                    downloadString = webClient.DownloadString($@"https://id.nlm.nih.gov/mesh/sparql?query={HttpUtility.UrlEncode(consulta)}&format={format}");
                    if (contadorDownload == MAX_NUM_INTENTOS)
                    {
                        return;
                    }
                }
                catch (Exception error)
                {
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.Message}");
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.StackTrace}");
                    Thread.Sleep(1000);
                    if (contadorDownload == MAX_NUM_INTENTOS)
                    {
                        return;
                    }
                }
            }

            SparqlObject resultadoQuery = JsonConvert.DeserializeObject<SparqlObject>(downloadString);

            // Objeto a devolver.
            DataConcept data = new();
            data.Title = pNombre;
            data.Url = $@"http://id.nlm.nih.gov/mesh/{pIdMesh}";
            data.Type = "mesh";

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("qualifiers") && !string.IsNullOrEmpty(fila["qualifiers"].value) && fila.ContainsKey("labelQualifiers") && !string.IsNullOrEmpty(fila["labelQualifiers"].value))
                    {
                        if (data.Qualifiers == null)
                        {
                            data.Qualifiers = new Dictionary<string, string>();
                        }
                        if (!data.Qualifiers.ContainsKey(fila["qualifiers"].value))
                        {
                            data.Qualifiers.Add(fila["qualifiers"].value, fila["labelQualifiers"].value);
                        }
                    }

                    if (fila.ContainsKey("broader") && !string.IsNullOrEmpty(fila["broader"].value) && fila.ContainsKey("labelBroader") && !string.IsNullOrEmpty(fila["labelBroader"].value))
                    {
                        if (data.Broader == null)
                        {
                            data.Broader = new Dictionary<string, string>();
                        }
                        if (!data.Broader.ContainsKey(fila["broader"].value))
                        {
                            data.Broader.Add(fila["broader"].value, fila["labelBroader"].value);
                        }
                    }

                    if (fila.ContainsKey("relatedTo") && !string.IsNullOrEmpty(fila["relatedTo"].value) && fila.ContainsKey("labelRelatedTo") && !string.IsNullOrEmpty(fila["labelRelatedTo"].value))
                    {
                        if (data.RelatedTo == null)
                        {
                            data.RelatedTo = new Dictionary<string, string>();
                        }
                        if (!data.RelatedTo.ContainsKey(fila["relatedTo"].value))
                        {
                            data.RelatedTo.Add(fila["relatedTo"].value, fila["labelRelatedTo"].value);
                        }
                    }
                }
            }
            webClient.Dispose();
            pListaData.Add(data);
        }

        /// <summary>
        /// Llamada.
        /// </summary>
        /// <param name="pUrl">URL.</param>
        /// <param name="pMethod">Tipo de envío.</param>
        /// <param name="pHeader">Cabecera.</param>
        /// <param name="pBody">Cuerpo.</param>
        /// <returns></returns>
        protected async Task<string> httpCall(string pUrl, string pMethod, Dictionary<string, string> pHeader = null, FormUrlEncodedContent pBody = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(pMethod), pUrl))
                {
                    if (pMethod == "GET")
                    {
                        if (pHeader != null && pHeader.Any())
                        {
                            foreach (KeyValuePair<string, string> item in pHeader)
                            {
                                request.Headers.Add(item.Key, item.Value);
                            }
                        }
                    }
                    else if (pMethod == "POST")
                    {
                        request.Content = pBody;
                    }

                    response = await httpClient.SendAsync(request);
                }
            }

            if (response.Content != null)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtiene el "Ticket-Granting Ticket". El TGT tiene una validez de 8 horas.
        /// </summary>
        /// <returns>TGT.</returns>
        public string GetTGT()
        {
            // Petición.
            string result;
            string data = string.Empty;

            while (true)
            {
                Uri url = new(_Configuracion.GetUrlTGT());
                FormUrlEncodedContent body = new(new[]
                {
                new KeyValuePair<string, string>("apikey", _Configuracion.GetApiKey())
                });

                try
                {
                    result = httpCall(url.ToString(), "POST", pBody: body).Result;
                    HtmlDocument doc = new();
                    doc.LoadHtml(result);
                    data = doc.DocumentNode.SelectSingleNode("//form").GetAttributeValue("action", "");
                }
                catch (Exception error)
                {
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.Message}");
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.StackTrace}");
                    Thread.Sleep(1000);
                }

                if (!string.IsNullOrEmpty(data))
                {
                    break;
                }
            }

            // TGT.
            return data.Substring(data.LastIndexOf('/') + 1);
        }

        /// <summary>
        /// Obtiene el "Service Ticket". El ST expira a los 5 minutos o por cada petición usada.
        /// </summary>
        /// <param name="pTGT"></param>
        /// <returns>ST.</returns>
        public string GetTicket(string pTGT)
        {
            // Petición.
            Uri url = new($@"{_Configuracion.GetUrlTicket()}/{pTGT}");
            FormUrlEncodedContent body = new(new[]
            {
                new KeyValuePair<string, string>("service", "http://umlsks.nlm.nih.gov")
            });

            // Ticket.
            string result;
            while (true)
            {
                try
                {
                    result = httpCall(url.ToString(), "POST", pBody: body).Result;
                    break;
                }
                catch (Exception error)
                {
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.Message}");
                    FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.StackTrace}");
                    Thread.Sleep(1000);
                    string tgt = GetTGT();
                    url = new Uri($@"{_Configuracion.GetUrlTicket()}/{tgt}");
                }
            }
            return result;
        }

        /// <summary>
        /// Mediante un ID MESH, obtiene la equivalencia al ID SNOMED.
        /// </summary>
        /// <param name="pName">Nombre del término a obtener.</param>
        /// <param name="pMeshId">ID MESH.</param>
        /// <param name="pST">Service Ticket.</param>
        /// <returns>ID SNOMED.</returns>
        public void GetSnomedId(string pMeshId, string pST, List<Data> pLista)
        {
            // Petición.
            Uri url = new($@"{_Configuracion.GetUrlSNOMED()}/{pMeshId}?targetSource=SNOMEDCT_US&ticket={pST}");
            string result;
            try
            {
                result = httpCall(url.ToString(), "GET").Result;
            }
            catch (Exception error)
            {
                FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.Message}");
                FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.StackTrace}");
                return;
            }

            // Obtención del dato del JSON de respuesta.
            try
            {
                CrosswalkObj data = JsonConvert.DeserializeObject<CrosswalkObj>(result);

                if (data.Result == null)
                {
                    return;
                }

                // ID SNOMED.                
                string msgNoActivo = "Concept inactivation indicator reference set";
                foreach (Result item in data.Result)
                {
                    bool activo = true;
                    if (item.SubsetMemberships != null && item.SubsetMemberships.Any())
                    {
                        foreach (SubsetMembership itemMembership in item.SubsetMemberships)
                        {
                            if (!string.IsNullOrEmpty(itemMembership.Name) && itemMembership.Name == msgNoActivo)
                            {
                                activo = false;
                                break;
                            }
                        }

                        if (activo)
                        {
                            Data dataActivo = new();
                            dataActivo.SnomedTerm = item;
                            pLista.Add(dataActivo);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.Message}");
                FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.StackTrace}");
                return;
            }
        }

        /// <summary>
        /// Obtiene todas las relaciones que tiene el término buscado.
        /// </summary>
        /// <param name="pSnomedId">ID SNOMED.</param>
        /// <param name="pTGT">TGT.</param>
        /// <returns>Lista de relaciones del termino buscado.</returns>
        public List<ResultRelations> GetRelaciones(string pSnomedId, string pTGT)
        {
            List<ResultRelations> listaResultados = new();

            // Paginacion.
            int numPagina = 1;
            int numPaginasTotal = int.MaxValue;

            // Petición.
            while (numPagina <= numPaginasTotal)
            {
                // Obtención del Service Ticket.
                string serviceTicket = GetTicket(pTGT);

                // Petición.
                Uri url = new($@"{_Configuracion.GetUrlRelaciones()}/{pSnomedId}/relations?pageNumber={numPagina}&ticket={serviceTicket}");
                string result = string.Empty;

                int contador = 0;
                while (contador <= 10)
                {
                    try
                    {
                        result = httpCall(url.ToString(), "GET").Result;
                        break;
                    }
                    catch (Exception error)
                    {
                        FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.Message}");
                        FileLogger.Log($@"[ERROR] {DateTime.Now} ---------- {error.StackTrace}");
                        Thread.Sleep(1000);
                        contador++;
                    }
                }

                // Obtención del dato del JSON de respuesta.
                RelationsObj data = JsonConvert.DeserializeObject<RelationsObj>(result);
                if (data.Result != null && data.Result.Any())
                {
                    listaResultados.AddRange(data.Result);
                }

                // Obtención de número de paginas total y actual.
                numPagina++;
                numPaginasTotal = data.PageCount;
            }

            //Dictionary<string, List<string>> dicAux = new Dictionary<string, List<string>>();
            //foreach(ResultRelations item in listaResultados)
            //{
            //    string id = item.relationLabel;
            //    string nom = item.relatedIdName;
            //    if(!dicAux.ContainsKey(id))
            //    {
            //        dicAux[id] = new List<string>() { nom };
            //    }
            //    else
            //    {
            //        dicAux[id].Add(nom);
            //    }
            //}

            // Datos.
            return listaResultados;
        }

        /// <summary>
        /// Comprueba si la palabrá está en la lista de carácteres.
        /// </summary>
        /// <param name="pPalabra">Palabra a comprobar.</param>
        /// <returns>True --> Existe // False --> No existe</returns>
        public bool ComprobarCaracteres(string pPalabra)
        {
            foreach (string caracter in caracteres)
            {
                if (pPalabra.Contains(caracter))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Devuelve la lista de preposiciones en inglés.
        /// </summary>
        /// <returns>Lista de preposiciones en inglés.</returns>
        public List<string> GetPreposicionesEng()
        {
            return preposicionesEng;
        }

        /// <summary>
        /// Devuelve la lista de preposiciones en castellano.
        /// </summary>
        /// <returns>Lista de preposiciones en castellano.</returns>
        public List<string> GetPreposicionesEsp()
        {
            return preposicionesEsp;
        }

        /// <summary>
        /// Devuelve la lista de carácteres especiales.
        /// </summary>
        /// <returns>Lista de carácteres especiales.</returns>
        public List<string> GetCaracteres()
        {
            return caracteres;
        }
    }
}
