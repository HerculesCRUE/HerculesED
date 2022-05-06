using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace Hercules.ED.ImportadorWebCV.Exporta.Secciones
{
    public class SeccionBase
    {
        protected static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configOAuth/OAuthV3.config");
        protected cvnRootResultBean mCvn { get; set; }
        protected string mCvID { get; set; }
        protected string mPersonID { get; set; }
        public SeccionBase(cvnRootResultBean cvn, string cvID)
        {
            mCvn = cvn;
            mCvID = cvID;
        }
        public SeccionBase(cvnRootResultBean cvn, string cvID, string personID)
        {
            mCvn = cvn;
            mCvID = cvID;
            mPersonID = personID;
        }

        /// <summary>
        /// Obtiene todos los datos de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns>Entidad completa</returns>
        public Entity GetLoadedEntity(string pId, string pGraph)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            try
            {
                int numLimit = 10000;
                int offset = 0;
                bool cargar = true;
                while (cargar)
                {
                    string selectID = "select * where{ select distinct ?s ?p ?o";
                    string whereID = $"where{{?x <http://gnoss/hasEntidad> <{pId}> . ?x <http://gnoss/hasEntidad> ?s . ?s ?p ?o }}order by desc(?s) desc(?p) desc(?o)}} limit {numLimit} offset {offset}";
                    SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
                    foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                    {
                        if (!listResult.ContainsKey(fila["s"].value))
                        {
                            listResult.Add(fila["s"].value, new List<Dictionary<string, Data>>());
                        }
                        listResult[fila["s"].value].Add(fila);
                    }
                    offset += numLimit;
                    if (resultData.results.bindings.Count < numLimit)
                    {
                        cargar = false;
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            if (listResult.Count > 0 && listResult.ContainsKey(pId))
            {
                Entity entity = new Entity()
                {
                    id = pId,
                    ontology = pGraph,
                    rdfType = listResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value,
                    properties = new List<Entity.Property>()
                };
                GetLoadedEntity(pId, "", "", ref entity, listResult);
                return entity;
            }
            return null;
        }


        public Dictionary<string, Entity> GetListLoadedEntity(List<string> listadoId, string pGraph)
        {
            Dictionary<string, Entity> listaEntidades = new Dictionary<string, Entity>();
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            try
            {
                int numLimit = 10000;
                int offset = 0;
                bool cargar = true;
                while (cargar)
                {
                    string selectID = "select * where{ select distinct ?s ?p ?o";
                    string whereID = $@"where{{
        ?x <http://gnoss/hasEntidad> ?s . 
        ?s ?p ?o .
        FILTER(?s in (<{string.Join(">,<", listadoId)}>))
    }}
    order by desc(?s) desc(?p) desc(?o)
}} limit {numLimit} offset {offset}";
                    SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
                    foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                    {
                        if (!listResult.ContainsKey(fila["s"].value))
                        {
                            listResult.Add(fila["s"].value, new List<Dictionary<string, Data>>());
                        }
                        listResult[fila["s"].value].Add(fila);
                    }
                    offset += numLimit;
                    if (resultData.results.bindings.Count < numLimit)
                    {
                        cargar = false;
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            foreach(string pId in listadoId)
            {
                Entity entity = new Entity()
                {
                    id = pId,
                    ontology = pGraph,
                    rdfType = listResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value,
                    properties = new List<Entity.Property>()
                };
                GetLoadedEntity(pId, "", "", ref entity, listResult);
                listaEntidades.Add(pId, entity);
            }

            //if (listResult.Count > 0 && listResult.ContainsKey(pId))
            //{
            //    Entity entity = new Entity()
            //    {
            //        id = pId,
            //        ontology = pGraph,
            //        rdfType = listResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value,
            //        properties = new List<Entity.Property>()
            //    };
            //    GetLoadedEntity(pId, "", "", ref entity, listResult);
            //    return entity;
            //}
            return listaEntidades;
        }



        /// <summary>
        /// Carga los datos en el objeto entidad con los datos obtenidos
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pPropAcumulado">Propiedad acumulada</param>
        /// <param name="pObjAcumulado">Objeto acumulado</param>
        /// <param name="pEntity">Entidad</param>
        /// <param name="pListResult">Datos de BBDD</param>
        private void GetLoadedEntity(string pId, string pPropAcumulado, string pObjAcumulado, ref Entity pEntity, Dictionary<string, List<Dictionary<string, Data>>> pListResult)
        {
            foreach (Dictionary<string, Data> prop in pListResult[pId])
            {
                string s = prop["s"].value;
                string p = prop["p"].value;
                string o = prop["o"].value;

                string rdfType = pListResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value;
                if (s == pId && p != "http://www.w3.org/2000/01/rdf-schema#label" && p != "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                {
                    string pPropAcumuladoAux = pPropAcumulado;
                    if (!string.IsNullOrEmpty(pPropAcumulado))
                    {
                        pPropAcumuladoAux += "@@@" + rdfType + "|";
                    }
                    pPropAcumuladoAux += p;
                    string pObjAcumuladoAux = pObjAcumulado;
                    if (!string.IsNullOrEmpty(pObjAcumulado))
                    {
                        pObjAcumuladoAux += "@@@";
                    }
                    pObjAcumuladoAux += o;
                    if (pListResult.ContainsKey(o))
                    {
                        GetLoadedEntity(o, pPropAcumuladoAux, pObjAcumuladoAux, ref pEntity, pListResult);
                    }
                    else
                    {
                        Entity.Property property = pEntity.properties.FirstOrDefault(x => x.prop == pPropAcumuladoAux);
                        if (property == null)
                        {
                            property = new Entity.Property(pPropAcumuladoAux, new List<string>());
                            pEntity.properties.Add(property);
                        }
                        property.values.Add(pObjAcumuladoAux);
                    }
                }
            }
        }

        /// <summary>
        /// Transforma la propiedad para su carga en una entiadad auxiliar
        /// </summary>
        /// <param name="pProp">Propiedad</param>
        /// <returns></returns>
        private string GetPropUpdateEntityAux(string pProp)
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
        /// Transforma el valor de la propiedad para su carga en una entiadad auxiliar
        /// </summary>
        /// <param name="pValue">Valor</param>
        /// <returns></returns>
        private string GetValueUpdateEntityAux(string pValue)
        {
            return pValue.Replace("@@@", "|");
        }

        /// <summary>
        /// Obtiene la entidad del valor
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        private string GetEntityOfValue(string pValue)
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
