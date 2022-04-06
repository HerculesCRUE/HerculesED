﻿using Gnoss.ApiWrapper;
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

namespace Hercules.ED.UpdateKeywords
{
    public class UtilKeywords
    {
        private ResourceApi mResourceApi;
        private static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/prefijos.json";
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));
        readonly ConfigService _Configuracion;

        public UtilKeywords(ResourceApi pResourceApi, CommunityApi pCommunityApi)
        {
            this.mResourceApi = pResourceApi;
            this._Configuracion = new ConfigService();
        }

        /// <summary>
        /// Obtiene los recursos que hayan de hacer el matching.
        /// </summary>
        /// <returns>Lista de IDs.</returns>
        public List<string> GetDocument()
        {
            mResourceApi.ChangeOntoly("document");
            List<string> listaDocumentos = new List<string>();
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?s ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("?s roh:getKeyWords 'true'. ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");
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

        public Dictionary<string, string> GetKeywords(string pIdRecurso)
        {
            mResourceApi.ChangeOntoly("document");
            Dictionary<string, string> dicEtiquetas = new Dictionary<string, string>();
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?freeTextKeyword ?etiqueta ");
            where.Append("WHERE { ");
            where.Append($@"<{pIdRecurso}> vivo:freeTextKeyword ?freeTextKeyword. ");
            where.Append("?freeTextKeyword roh:title ?etiqueta. ");
            where.Append("MINUS { ?freeTextKeyword roh:keyWordConcept ?keywordConcept. }");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");
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
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?s ");
            where.Append("WHERE { ");
            where.Append("?s a roh:KeyWordConcept. ");
            where.Append($@"?s roh:url '{pUrl}'. ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "keywordconcept");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                return resultadoQuery.results.bindings[0]["s"].value;
            }
            else
            {
                return null;
            }
        }

        public string CargarDataConceptCompleto(DataConcept pData, Dictionary<string, List<Dictionary<string, string>>> pDicIdsSnomed = null)
        {
            mResourceApi.ChangeOntoly("keywordconcept");

            // 1º - Por cada DataConcept relacionado ejecutamos CargarDataConceptParcial.
            ConcurrentBag<string> listBroaders = new ConcurrentBag<string>();
            if (pData.broader != null)
            {
                Parallel.ForEach(pData.broader.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idBroader =>
                {
                    listBroaders.Add(CargarDataConceptParcial(idBroader, pData.broader[idBroader], pData.type));
                });
            }

            ConcurrentBag<string> listQualifiers = new ConcurrentBag<string>();
            if (pData.qualifiers != null)
            {
                Parallel.ForEach(pData.qualifiers.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idQualifiers =>
                {
                    listQualifiers.Add(CargarDataConceptParcial(idQualifiers, pData.qualifiers[idQualifiers], pData.type));
                });
            }

            ConcurrentBag<string> listRelatedTo = new ConcurrentBag<string>();
            if (pData.relatedTo != null)
            {
                Parallel.ForEach(pData.relatedTo.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idRelatedTo =>
                {
                    listRelatedTo.Add(CargarDataConceptParcial(idRelatedTo, pData.relatedTo[idRelatedTo], pData.type));
                });
            }

            ConcurrentBag<string> listCloseMatch = new ConcurrentBag<string>();
            if (pData.closeMatch != null)
            {
                Parallel.ForEach(pData.closeMatch.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idCloseMatch =>
                {
                    listCloseMatch.Add(CargarDataConceptParcial(idCloseMatch, pData.closeMatch[idCloseMatch], pData.type));
                });
            }

            ConcurrentBag<string> listExactMatch = new ConcurrentBag<string>();
            if (pData.exactMatch != null)
            {
                Parallel.ForEach(pData.exactMatch.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idExactMatch =>
                {
                    listExactMatch.Add(CargarDataConceptParcial(idExactMatch, pData.exactMatch[idExactMatch], pData.type));
                });
            }

            // 2º - Una vez terminado, creamos/modificamos el DataConcept actual.
            KeywordconceptOntology.KeyWordConcept keyWordConcept = new KeywordconceptOntology.KeyWordConcept();
            keyWordConcept.Roh_title = pData.title;
            keyWordConcept.Roh_url = pData.url;
            keyWordConcept.IdsRoh_broaders = listBroaders.Distinct().ToList();
            keyWordConcept.IdsRoh_qualifiers = listQualifiers.Distinct().ToList();
            keyWordConcept.IdsRoh_relatedTo = listRelatedTo.Distinct().ToList();
            keyWordConcept.IdsSkos_closeMatch = listCloseMatch.Distinct().ToList();
            keyWordConcept.IdsSkos_exactMatch = listExactMatch.Distinct().ToList();
            keyWordConcept.Roh_type = pData.type;

            // Si es una KeyWordConcept de Mesh...
            if (keyWordConcept.Roh_url.Contains("/"))
            {
                List<string> listaIdsSnomedCargados = new List<string>();
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
            string id = GetUriTag(pData.url);
            if (string.IsNullOrEmpty(id))
            {
                ComplexOntologyResource resource = keyWordConcept.ToGnossApiResource(mResourceApi, null);
                id = mResourceApi.LoadComplexSemanticResource(resource, false, true);
            }
            else
            {
                string[] idSplit = id.Split('_');
                ComplexOntologyResource resource = keyWordConcept.ToGnossApiResource(mResourceApi, null, new Guid(idSplit[idSplit.Length - 2]), new Guid(idSplit[idSplit.Length - 1]));
                mResourceApi.ModifyComplexOntologyResource(resource, false, true);
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

        public string CargarDataConceptParcial(string pIdUrl, string pNombre, string pTipo)
        {
            // 1º - Si existe devuelve el ID.
            string id = GetUriTag(pIdUrl);

            // 2º - Si no existe cargamos el DataMesh.
            if (string.IsNullOrEmpty(id))
            {
                // Creación del objeto a cargar.
                KeywordconceptOntology.KeyWordConcept keyWordConcept = new KeywordconceptOntology.KeyWordConcept();
                keyWordConcept.Roh_title = pNombre;
                keyWordConcept.Roh_url = pIdUrl;
                keyWordConcept.Roh_type = pTipo;
                ComplexOntologyResource resource = keyWordConcept.ToGnossApiResource(mResourceApi, null);

                // Carga.
                id = mResourceApi.LoadComplexSemanticResource(resource, false, true);
            }

            return id;
        }

        public void ModificarKeyword(string pIdRecurso, string pKeyword)
        {
            Guid guid = mResourceApi.GetShortGuid(pIdRecurso);
            Dictionary<Guid, List<TriplesToInclude>> dicInclude = new Dictionary<Guid, List<TriplesToInclude>>();
            List<TriplesToInclude> listaTriplesInclude = new List<TriplesToInclude>();

            TriplesToInclude triple = new TriplesToInclude();
            triple.Predicate = "http://w3id.org/roh/keyWordConcept";
            triple.NewValue = pKeyword;
            listaTriplesInclude.Add(triple);

            dicInclude.Add(guid, listaTriplesInclude);
            mResourceApi.InsertPropertiesLoadedResources(dicInclude);
        }

        public void BorrarGetKeywordProperty(string pIdRecurso)
        {
            Guid guid = mResourceApi.GetShortGuid(pIdRecurso);
            Dictionary<Guid, List<RemoveTriples>> dicBorrado = new Dictionary<Guid, List<RemoveTriples>>();
            List<RemoveTriples> listaTriplesBorrado = new List<RemoveTriples>();

            RemoveTriples triple = new RemoveTriples();
            triple.Predicate = $@"http://w3id.org/roh/getKeyWords";
            triple.Value = "true";
            listaTriplesBorrado.Add(triple);

            dicBorrado.Add(guid, listaTriplesBorrado);
            mResourceApi.DeletePropertiesLoadedResources(dicBorrado);
        }

        public List<DataConcept> GetDataConcepts(List<Data> pListaData)
        {
            List<DataConcept> listaDataConcepts = new List<DataConcept>();

            foreach (Data data in pListaData)
            {
                DataConcept dataConceptMain = new DataConcept();
                dataConceptMain.title = data.snomedTerm.name;
                dataConceptMain.url = data.snomedTerm.ui;
                dataConceptMain.type = "snomed";

                foreach (ResultRelations itemRelacion in data.relations)
                {
                    switch (itemRelacion.relationLabel)
                    {
                        case "AQ":
                            if (dataConceptMain.qualifiers == null)
                            {
                                dataConceptMain.qualifiers = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.qualifiers.ContainsKey(itemRelacion.ui))
                            {
                                dataConceptMain.qualifiers.Add(itemRelacion.ui, itemRelacion.relatedIdName);
                            }
                            break;
                        case "RL":
                            if (dataConceptMain.closeMatch == null)
                            {
                                dataConceptMain.closeMatch = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.closeMatch.ContainsKey(itemRelacion.ui))
                            {
                                dataConceptMain.closeMatch.Add(itemRelacion.ui, itemRelacion.relatedIdName);
                            }
                            break;
                        case "SY":
                            if (dataConceptMain.exactMatch == null)
                            {
                                dataConceptMain.exactMatch = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.exactMatch.ContainsKey(itemRelacion.ui))
                            {
                                dataConceptMain.exactMatch.Add(itemRelacion.ui, itemRelacion.relatedIdName);
                            }
                            break;
                        case "RO":
                            if (dataConceptMain.relatedTo == null)
                            {
                                dataConceptMain.relatedTo = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.relatedTo.ContainsKey(itemRelacion.ui))
                            {
                                dataConceptMain.relatedTo.Add(itemRelacion.ui, itemRelacion.relatedIdName);
                            }
                            break;
                        case "RU":
                            if (dataConceptMain.relatedTo == null)
                            {
                                dataConceptMain.relatedTo = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.relatedTo.ContainsKey(itemRelacion.ui))
                            {
                                dataConceptMain.relatedTo.Add(itemRelacion.ui, itemRelacion.relatedIdName);
                            }
                            break;
                        case "CHD":
                            if (dataConceptMain.broader == null)
                            {
                                dataConceptMain.broader = new Dictionary<string, string>();
                            }
                            if (!dataConceptMain.broader.ContainsKey(itemRelacion.ui))
                            {
                                dataConceptMain.broader.Add(itemRelacion.ui, itemRelacion.relatedIdName);
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
        /// <param name="pTopicalDescriptor">Tópico a consultar.</param>
        /// <returns>Diccionario resultante.</returns>
        public Dictionary<string, string> SelectDataMesh(string pTopicalDescriptor)
        {
            pTopicalDescriptor = pTopicalDescriptor.Trim().ToLower().Replace("\'", "\\\'");
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();

            // Endpoint.
            string urlConsulta = "https://id.nlm.nih.gov/mesh/sparql";

            // Consulta.
            // ?a -> MeshId
            // ?b -> Label
            // ?c -> Concept
            // ?d -> AltLabel
            // ?e -> PrefLabel
            // ?f -> SortVersion
            // ?g -> ConceptAltLabel
            // ?h -> ConceptPrefLabel
            // ?i -> ConceptSortVersion
            string consulta = $@"
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                PREFIX meshv: <http://id.nlm.nih.gov/mesh/vocab#>
                SELECT ?a ?b ?c ?d ?e ?f ?g ?h ?i
                FROM <http://id.nlm.nih.gov/mesh>
                WHERE {{
                  {{?a a meshv:TopicalDescriptor}}.  
                  {{
                    ?a rdfs:label ?b.
  	                FILTER(REGEX(?b,'^{pTopicalDescriptor}$','i'))
                  }}
                  UNION
                  {{
                    ?a rdfs:label ?b.
                    ?a meshv:concept ?x.
                    ?x rdfs:label ?c.
  	                FILTER(REGEX(?c,'^{pTopicalDescriptor}$','i'))
                  }}
                  UNION
                  {{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredTerm ?x.
                    ?x meshv:altLabel ?d.
  	                FILTER(REGEX(?d,'^{pTopicalDescriptor}$','i'))
                  }}
                  UNION
                  {{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredTerm ?x.
                    ?x meshv:prefLabel ?e.
  	                FILTER(REGEX(?e,'^{pTopicalDescriptor}$','i'))
                  }}
                  UNION
                  {{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredTerm ?x.
                    ?x meshv:sortVersion ?f.
  	                FILTER(REGEX(?f,'^{pTopicalDescriptor}$','i'))
                  }}
                  UNION
                  {{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredConcept ?y.
                    ?y meshv:term ?x.
                    ?x meshv:altLabel ?g.
                    FILTER(REGEX(?g,'^{pTopicalDescriptor}$','i'))
                  }}
                  UNION
                  {{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredConcept ?y.
                    ?y meshv:term ?x.
                    ?x meshv:prefLabel ?h.
                    FILTER(REGEX(?h,'^{pTopicalDescriptor}$','i'))
                  }}
                  UNION
                  {{
                    ?a rdfs:label ?b.
                    ?a meshv:preferredConcept ?y.
                    ?y meshv:term ?x.
                    ?x meshv:sortVersion ?i.
                    FILTER(REGEX(?i,'^{pTopicalDescriptor}$','i'))
                  }}
                }}
                ORDER BY ?a
            ";

            // Tipo de salida.
            string format = "JSON";

            // Petición.
            WebClient webClient = new WebClient();
            string downloadString = string.Empty;

            // TODO: Meter control.
            try
            {
                downloadString = webClient.DownloadString($@"{urlConsulta}?query={HttpUtility.UrlEncode(consulta)}&format={format}");
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }

            SparqlObject resultadoQuery = JsonConvert.DeserializeObject<SparqlObject>(downloadString);

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

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(label))
                    {
                        if (!dicResultados.ContainsKey(id))
                        {
                            dicResultados.Add(id, label);
                        }
                    }
                }
            }

            return dicResultados;
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
                item.relations = GetRelaciones(item.snomedTerm.ui, tgt);
            }
        }

        public void InsertDataMesh(string pIdMesh, string pNombre, List<DataConcept> pListaData)
        {
            // Endpoint.
            string urlConsulta = "https://id.nlm.nih.gov/mesh/sparql";

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
            WebClient webClient = new WebClient();
            string downloadString = string.Empty;

            // TODO: Meter control.
            try
            {
                downloadString = webClient.DownloadString($@"{urlConsulta}?query={HttpUtility.UrlEncode(consulta)}&format={format}");
            }
            catch (Exception)
            {
                return;
            }

            SparqlObject resultadoQuery = JsonConvert.DeserializeObject<SparqlObject>(downloadString);

            // Objeto a devolver
            DataConcept data = new DataConcept();
            data.title = pNombre;
            data.url = $@"http://id.nlm.nih.gov/mesh/{pIdMesh}";
            data.type = "mesh";

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("qualifiers") && !string.IsNullOrEmpty(fila["qualifiers"].value) && fila.ContainsKey("labelQualifiers") && !string.IsNullOrEmpty(fila["labelQualifiers"].value))
                    {
                        if (data.qualifiers == null)
                        {
                            data.qualifiers = new Dictionary<string, string>();
                        }
                        if (!data.qualifiers.ContainsKey(fila["qualifiers"].value))
                        {
                            data.qualifiers.Add(fila["qualifiers"].value, fila["labelQualifiers"].value);
                        }
                    }

                    if (fila.ContainsKey("broader") && !string.IsNullOrEmpty(fila["broader"].value) && fila.ContainsKey("labelBroader") && !string.IsNullOrEmpty(fila["labelBroader"].value))
                    {
                        if (data.broader == null)
                        {
                            data.broader = new Dictionary<string, string>();
                        }
                        if (!data.broader.ContainsKey(fila["broader"].value))
                        {
                            data.broader.Add(fila["broader"].value, fila["labelBroader"].value);
                        }
                    }

                    if (fila.ContainsKey("relatedTo") && !string.IsNullOrEmpty(fila["relatedTo"].value) && fila.ContainsKey("labelRelatedTo") && !string.IsNullOrEmpty(fila["labelRelatedTo"].value))
                    {
                        if (data.relatedTo == null)
                        {
                            data.relatedTo = new Dictionary<string, string>();
                        }
                        if (!data.relatedTo.ContainsKey(fila["relatedTo"].value))
                        {
                            data.relatedTo.Add(fila["relatedTo"].value, fila["labelRelatedTo"].value);
                        }
                    }
                }
            }

            pListaData.Add(data);
        }

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

                    int intentos = 3;
                    while (true)
                    {
                        try
                        {
                            response = await httpClient.SendAsync(request);
                            break;
                        }
                        catch (Exception)
                        {
                            intentos--;
                            if (intentos == 0)
                            {
                                //_FileLogger.Log(pUrl, error.ToString());
                                throw;
                            }
                            else
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
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
            Uri url = new Uri(_Configuracion.GetUrlTGT());
            FormUrlEncodedContent body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("apikey", _Configuracion.GetApiKey())
            });
            string result = httpCall(url.ToString(), "POST", pBody: body).Result;

            // Obtención del dato del HTML de respuesta.
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result);
            string data = doc.DocumentNode.SelectSingleNode("//form").GetAttributeValue("action", "");

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
            Uri url = new Uri($@"{_Configuracion.GetUrlTicket()}/{pTGT}");
            FormUrlEncodedContent body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("service", "http://umlsks.nlm.nih.gov")
            });

            // Ticket.
            return httpCall(url.ToString(), "POST", pBody: body).Result;
        }

        /// <summary>
        /// Mediante un ID MESH, obtiene la equivalencia al ID SNOMED.
        /// </summary>
        /// <param name="pName">Nombre del término a obtener.</param>
        /// <param name="pMeshId">ID MESH.</param>
        /// <param name="pST">Service Ticket.</param>
        /// <returns>ID SNOMED.</returns>
        public void GetSnomedId(string pMeshId, string pST, List<Data> pLista, int contador = 0)
        {
            // Petición.
            Uri url = new Uri($@"{_Configuracion.GetUrlSNOMED()}/{pMeshId}?targetSource=SNOMEDCT_US&ticket={pST}");
            string result = httpCall(url.ToString(), "GET").Result;

            // Obtención del dato del JSON de respuesta.
            try
            {
                CrosswalkObj data = JsonConvert.DeserializeObject<CrosswalkObj>(result);

                if (data.result == null)
                {
                    return;
                }

                // ID SNOMED.                
                string msgNoActivo = "Concept inactivation indicator reference set";
                foreach (Result item in data.result)
                {
                    bool activo = true;
                    if (item.subsetMemberships != null && item.subsetMemberships.Any())
                    {
                        foreach (SubsetMembership itemMembership in item.subsetMemberships)
                        {
                            if (!string.IsNullOrEmpty(itemMembership.name) && itemMembership.name == msgNoActivo)
                            {
                                activo = false;
                                break;
                            }
                        }

                        if (activo)
                        {
                            Data dataActivo = new Data();
                            dataActivo.snomedTerm = item;
                            pLista.Add(dataActivo);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                // TODO:
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
            List<ResultRelations> listaResultados = new List<ResultRelations>();

            // Paginacion.
            int numPagina = 1;
            int numPaginasTotal = int.MaxValue;

            // Petición.
            while (numPagina <= numPaginasTotal)
            {
                // Obtención del Service Ticket.
                string serviceTicket = GetTicket(pTGT);

                // Petición.
                Uri url = new Uri($@"{_Configuracion.GetUrlRelaciones()}/{pSnomedId}/relations?pageNumber={numPagina}&ticket={serviceTicket}");
                string result = httpCall(url.ToString(), "GET").Result;

                // Obtención del dato del JSON de respuesta.
                RelationsObj data = JsonConvert.DeserializeObject<RelationsObj>(result);
                listaResultados.AddRange(data.result);

                // Obtención de número de paginas total y actual.
                numPagina++;
                numPaginasTotal = data.pageCount;
            }

            // Datos.
            return listaResultados;
        }
    }
}
