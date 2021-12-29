using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace DesnormalizadorHercules.Models
{
    //TODO comentarios completados, falta eliminar froms

    /// <summary>
    /// Clase base para los actualizadores
    /// </summary>
    public class ActualizadorBase
    {
        /// <summary>
        /// API Wrapper de GNOSS
        /// </summary>
        protected ResourceApi mResourceApi;

        /// <summary>
        /// Lista con los prefijos
        /// </summary>
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
            {"gn", "http://www.geonames.org/ontology#" },
            {"skos", "http://www.w3.org/2008/05/skos#" }
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pResourceApi"></param>
        public ActualizadorBase(ResourceApi pResourceApi)
        {
            mResourceApi = pResourceApi;
        }

        /// <summary>
        /// Método para inserción múltiple de triples
        /// </summary>
        /// <param name="pFilas">Filas con los datos para insertar</param>
        /// <param name="pPredicado">Predicado para insertar</param>
        /// <param name="pPropSubject">Propiedad de las filas utilizada para el sujeto</param>
        /// <param name="pPropObject">Propiedad de las filas utilizada para el objeto</param>
        public void InsercionMultiple(List<Dictionary<string, Data>> pFilas, string pPredicado, string pPropSubject, string pPropObject)
        {
            List<string> ids = pFilas.Select(x => x[pPropSubject].value).Distinct().ToList();
            foreach (string id in ids)
            {
                Guid guid = mResourceApi.GetShortGuid(id);
                Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToInclude>> triples = new Dictionary<Guid, List<TriplesToInclude>>() { { guid, new List<TriplesToInclude>() } };
                foreach (string value in pFilas.Where(x => x[pPropSubject].value == id).Select(x => x[pPropObject].value))
                {
                    TriplesToInclude t = new();
                    t.Predicate = pPredicado;
                    t.NewValue = value;
                    triples[guid].Add(t);
                }
                var resultado = mResourceApi.InsertPropertiesLoadedResources(triples);
            }
        }

        /// <summary>
        /// Método para eliminación múltiple de triples
        /// </summary>
        /// <param name="pFilas">Filas con los datos para eliminar</param>
        /// <param name="pPredicado">Predicado para eliminar</param>
        /// <param name="pPropSubject">Propiedad de las filas utilizada para el sujeto</param>
        /// <param name="pPropObject">Propiedad de las filas utilizada para el objeto</param>
        public void EliminacionMultiple(List<Dictionary<string, Data>> pFilas, string pPredicado, string pPropSubject, string pPropObject)
        {
            List<string> ids = pFilas.Select(x => x[pPropSubject].value).Distinct().ToList();
            foreach (string id in ids)
            {
                Guid guid = mResourceApi.GetShortGuid(id);
                Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>> triples = new Dictionary<Guid, List<RemoveTriples>>() { { guid, new List<RemoveTriples>() } };
                foreach (string value in pFilas.Where(x => x[pPropSubject].value == id).Select(x => x[pPropObject].value))
                {
                    RemoveTriples t = new();
                    t.Predicate = pPredicado;
                    t.Value = value;
                    triples[guid].Add(t);
                }
                var resultado = mResourceApi.DeletePropertiesLoadedResources(triples);
            }
        }

        /// <summary>
        /// Método para cargar/actualizar/eliminar triples
        /// </summary>
        /// <param name="pSujeto">Sujeto</param>
        /// <param name="pPredicado">Predicado</param>
        /// <param name="pValorAntiguo">Valor antiguo (si es vacío se inserta el valor nuevo)</param>
        /// <param name="pValorNuevo">Valor nuevo (si es vacío se eliminar el valor antiguo)</param>
        public void ActualizadorTriple(string pSujeto, string pPredicado, string pValorAntiguo, string pValorNuevo)
        {
            Guid guid = mResourceApi.GetShortGuid(pSujeto);
            if (!string.IsNullOrEmpty(pValorAntiguo) && !string.IsNullOrEmpty(pValorNuevo))
            {
                //Si el valor nuevo y el viejo no son nulos -->modificamos
                TriplesToModify t = new TriplesToModify();
                t.NewValue = pValorNuevo;
                t.OldValue = pValorAntiguo;
                t.Predicate = pPredicado;
                var resultado = mResourceApi.ModifyPropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToModify>>() { { guid, new List<Gnoss.ApiWrapper.Model.TriplesToModify>() { t } } });
            }
            else if (string.IsNullOrEmpty(pValorAntiguo) && !string.IsNullOrEmpty(pValorNuevo))
            {
                //Si el valor nuevo no es nulo y viejo si es nulo -->insertamos
                TriplesToInclude t = new();
                t.Predicate = pPredicado;
                t.NewValue = pValorNuevo;
                var resultado = mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToInclude>>() { { guid, new List<Gnoss.ApiWrapper.Model.TriplesToInclude>() { t } } });
            }
            else if (!string.IsNullOrEmpty(pValorAntiguo) && string.IsNullOrEmpty(pValorNuevo))
            {
                //Si el valor nuevo es nulo y viejo si no es nulo -->eliminamos
                RemoveTriples t = new();
                t.Predicate = pPredicado;
                t.Value = pValorAntiguo;
                var resultado = mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { guid, new List<Gnoss.ApiWrapper.Model.RemoveTriples>() { t } } });
            }
        }

        /// <summary>
        /// Método para eliminar propiedades duplicadas que sólo deberían tener un valor
        /// </summary>
        /// <param name="pGraph">Grafo donde realizar la consulta</param>
        /// <param name="pRdfType">Rdftype del sujeto en el que comprobar la propiedad</param>
        /// <param name="pProperty">Propiedad a comprobar</param>
        public void EliminarDuplicados(string pGraph, string pRdfType, string pProperty)
        {
            while (true)
            {
                int limit = 500;
                String select = @"select ?id count(?data) ";
                String where = @$"where
                                {{
                                    ?id a <{pRdfType}>.
                                    ?id <{pProperty}> ?data. 
                                }}group by (?id) HAVING (COUNT(?data) > 1) limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, pGraph);

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string id = fila["id"].value;
                    String select2 = @"select ?data ";
                    String where2 = @$"where
                                {{
                                    <{id}> <{pProperty}> ?data. 
                                }}";
                    SparqlObject resultado2 = mResourceApi.VirtuosoQuery(select2, where2, pGraph);
                    foreach (Dictionary<string, SparqlObject.Data> fila2 in resultado2.results.bindings.GetRange(1, resultado2.results.bindings.Count - 1))
                    {
                        string value = fila2["data"].value;
                        ActualizadorTriple(id, pProperty, value, "");
                    }
                }
                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Método para insertar Categorías
        /// </summary>
        /// <param name="pResultado">Resultado de la query de la que obtener los datos</param>
        /// <param name="pDicAreasBroader">Diccionario con las áreas y sus padres</param>
        /// <param name="pGraphsUrl">Url interna de los grafos</param>
        /// <param name="pVarItem">Ítem en el que insertar las categorias</param>
        /// <param name="pPropCategoria">Propiedad en la que insertar las categorías</param>
        public void InsertarCategorias(SparqlObject pResultado, Dictionary<string, string> pDicAreasBroader, string pGraphsUrl,string pVarItem,string pPropCategoria)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>();
            foreach (Dictionary<string, SparqlObject.Data> fila in pResultado.results.bindings)
            {
                string item = fila[pVarItem].value;
                string categoryNode = fila["categoryNode"].value;

                string idNewAux = pGraphsUrl + "items/CategoryPath_" + mResourceApi.GetShortGuid(item).ToString().ToLower() + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
                string idEntityAux = idNewAux;

                string categoryNodeAux = categoryNode;
                while (!string.IsNullOrEmpty(categoryNodeAux))
                {
                    string predicadoCategoria =pPropCategoria+ "|http://w3id.org/roh/categoryNode";
                    TriplesToInclude tr2 = new TriplesToInclude(idEntityAux + "|" + categoryNodeAux, predicadoCategoria);
                    listaTriples.Add(tr2);
                    categoryNodeAux = pDicAreasBroader[categoryNodeAux];
                }

                Guid idItem = mResourceApi.GetShortGuid(item);
                if (triplesToInclude.ContainsKey(idItem))
                {
                    triplesToInclude[idItem].AddRange(listaTriples);
                }
                else
                {
                    triplesToInclude.Add(idItem, listaTriples);
                }
            }
            foreach (Guid idItem in triplesToInclude.Keys)
            {
                mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { idItem, triplesToInclude[idItem] } });
            }
        }

        /// <summary>
        /// Método para eliminar Categorías
        /// </summary>
        /// <param name="pResultado">Resultado de la query de la que obtener los datos</param>
        /// <param name="pVarItem">Ítem en el que insertar las categorias</param>
        /// <param name="pPropCategoria">Propiedad en la que insertar las categorías</param>
        public void EliminarCategorias(SparqlObject pResultado, string pVarItem,string pPropCategoria)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToRemove = new Dictionary<Guid, List<RemoveTriples>>();
            foreach (Dictionary<string, SparqlObject.Data> fila in pResultado.results.bindings)
            {
                string item = fila[pVarItem].value;
                string hasKnowledgeArea = fila["hasKnowledgeArea"].value;

                RemoveTriples removeTriple = new RemoveTriples();
                removeTriple.Predicate = pPropCategoria;
                removeTriple.Value = hasKnowledgeArea;

                Guid idItem = mResourceApi.GetShortGuid(item);
                if (triplesToRemove.ContainsKey(idItem))
                {
                    triplesToRemove[idItem].Add(removeTriple);
                }
                else
                {
                    triplesToRemove.Add(idItem, new List<RemoveTriples>() { removeTriple });
                }
            }
            foreach (Guid idItem in triplesToRemove.Keys)
            {
                mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<RemoveTriples>>() { { idItem, triplesToRemove[idItem] } });
            }
        }

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
    }
}
