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

namespace ImportadorWebCV.Exporta.Secciones
{
    public class SeccionBase
    {
        protected static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
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
                        if (!fila.ContainsKey("s"))
                        {
                            continue;
                        }
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

        public Dictionary<string, Entity> GetListLoadedEntityCV(List<Tuple<string, string, string>> listadoId, string pGraph,
            Dictionary<string, List<Dictionary<string, Data>>> MultilangProp = null, List<string> listadoFrom = null)
        {
            Dictionary<string, Entity> listaEntidades = new Dictionary<string, Entity>();
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            Dictionary<string, List<Dictionary<string, Data>>> listResultCV = new Dictionary<string, List<Dictionary<string, Data>>>();

            //Si no envio un listado devuelvo un diccionario vacio
            if (listadoId == null || listadoId.Count() == 0)
            {
                return new Dictionary<string, Entity>();
            }

            try
            {
                int numLimit = 10000;
                int offset = 0;
                bool cargar = true;
                if (listadoFrom == null)
                {
                    listadoFrom = new List<string>();
                }
                listadoFrom.Add(pGraph);
                while (cargar)
                {
                    string selectID = "select * where{ select distinct ?s ?p ?o ?q ?w";
                    string whereID = $@"where{{
        ?x <http://gnoss/hasEntidad> ?s . 
        ?s ?p ?o .
        OPTIONAL{{
            ?o ?q ?w .
        }}
        FILTER(?s in (<{string.Join(">,<", listadoId.Select(x => x.Item1))}>))
    }}
    order by desc(?s) desc(?p) desc(?o)
}} limit {numLimit} offset {offset}";

                    SparqlObject resultData = mResourceApi.VirtuosoQueryMultipleGraph(selectID, whereID, listadoFrom);
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

            try
            {
                int numLimit = 10000;
                int offset = 0;
                bool cargar = true;
                while (cargar)
                {
                    string selectID = "select * where{ select distinct ?s ?p ?o ?q ?w";
                    string whereID = $@"where{{
        ?x <http://gnoss/hasEntidad> ?s . 
        ?s ?p ?o .
        OPTIONAL{{
            ?o ?q ?w .
        }}
        FILTER(?s in (<{string.Join(">,<", listadoId.Select(x => x.Item2).Where(x => !string.IsNullOrEmpty(x)))}>))
    }}
    order by desc(?s) desc(?p) desc(?o)
}} limit {numLimit} offset {offset}";

                    SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, "curriculumvitae");
                    foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                    {
                        if (!listResultCV.ContainsKey(fila["s"].value))
                        {
                            listResultCV.Add(fila["s"].value, new List<Dictionary<string, Data>>());
                        }
                        listResultCV[fila["s"].value].Add(fila);
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


            foreach (string pId in listadoId.Select(x => x.Item1))
            {
                if (!listResult.ContainsKey(pId))
                {
                    continue;
                }
                Entity entity = new Entity()
                {
                    id = pId,
                    ontology = pGraph,
                    rdfType = listResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value,
                    properties = new List<Entity.Property>(),
                    properties_cv = new List<Entity.Property>()
                };
                GetLoadedEntity(pId, "", "", ref entity, listResult, MultilangProp);
                if (!string.IsNullOrEmpty(listadoId.Where(x => x.Item1.Equals(pId)).Select(x => x.Item2).FirstOrDefault()))
                {
                    GetLoadedEntityCV(listadoId.Where(x => x.Item1.Equals(pId)).Select(x => x.Item2).FirstOrDefault(), "", "", ref entity, listResultCV);
                }
                listaEntidades.Add(pId, entity);
            }

            return listaEntidades;
        }

        public Dictionary<string, Entity> GetListLoadedEntity(List<Tuple<string, string>> listadoId, string pGraph, Dictionary<string, List<Dictionary<string, Data>>> MultilangProp = null)
        {
            Dictionary<string, Entity> listaEntidades = new Dictionary<string, Entity>();
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();

            //Si no envio un listado devuelvo un diccionario vacio
            if (listadoId == null || listadoId.Count() == 0)
            {
                return new Dictionary<string, Entity>();
            }

            try
            {
                int numLimit = 10000;
                int offset = 0;
                bool cargar = true;
                while (cargar)
                {
                    string selectID = "select * where{ select distinct ?s ?p ?o ?q ?w";
                    string whereID = $@"where{{
        ?x <http://gnoss/hasEntidad> ?s . 
        ?s ?p ?o .
        OPTIONAL{{
            ?o ?q ?w .
        }}
        FILTER(?s in (<{string.Join(">,<", listadoId.Select(x => x.Item1))}>))
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
            foreach (string pId in listadoId.Select(x => x.Item1))
            {
                Entity entity = new Entity()
                {
                    id = pId,
                    ontology = pGraph,
                    rdfType = listResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value,
                    properties = new List<Entity.Property>()
                };
                GetLoadedEntity(pId, "", "", ref entity, listResult, MultilangProp);
                listaEntidades[pId] = entity;
            }

            return listaEntidades;
        }

        protected Dictionary<string, List<Dictionary<string, Data>>> GetMultilangProperties(string pCVID, string pIdioma)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();

            string select = $@"select distinct ?entity ?multilangProperties ?prop ?lang ?value";
            string where = $@"where{{
    <{pCVID}> <http://w3id.org/roh/multilangProperties> ?multilangProperties . 
    ?multilangProperties <http://w3id.org/roh/entity> ?entity . 
    ?multilangProperties <http://w3id.org/roh/property> ?prop. 
    ?multilangProperties <http://w3id.org/roh/lang> ?lang. 
    ?multilangProperties <http://w3id.org/roh/value> ?value. 
    FILTER(?lang='{pIdioma}')
}}";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (!listResult.ContainsKey(fila["entity"].value))
                {
                    listResult.Add(fila["entity"].value, new List<Dictionary<string, Data>>());
                }
                listResult[fila["entity"].value].Add(fila);
            }

            return listResult;
        }

        /// <summary>
        /// Carga los datos en el objeto entidad con los datos obtenidos
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pPropAcumulado">Propiedad acumulada</param>
        /// <param name="pObjAcumulado">Objeto acumulado</param>
        /// <param name="pEntity">Entidad</param>
        /// <param name="pListResult">Datos de BBDD</param>
        private void GetLoadedEntity(string pId, string pPropAcumulado, string pObjAcumulado, ref Entity pEntity,
            Dictionary<string, List<Dictionary<string, Data>>> pListResult, Dictionary<string, List<Dictionary<string, Data>>> MultilangProp = null)
        {
            foreach (Dictionary<string, Data> prop in pListResult[pId])
            {
                string s = prop["s"].value;
                string p = prop["p"].value;
                string o = prop["o"].value;
                string q = "";
                string w = "";
                if (prop.ContainsKey("q") && prop.ContainsKey("w"))
                {
                    q = prop["q"].value;
                    w = prop["w"].value;
                }

                string rdfType = pListResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value;
                if (s == pId && p != "http://www.w3.org/2000/01/rdf-schema#label" && p != "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                {
                    string qPropAcumuladoAux = "";
                    string wObjAcumuladoAux = "";
                    if (!string.IsNullOrEmpty(q) && q != "http://www.w3.org/2000/01/rdf-schema#label"
                        && q != "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                    {
                        qPropAcumuladoAux = pPropAcumulado;
                        if (string.IsNullOrEmpty(pPropAcumulado))
                        {
                            qPropAcumuladoAux += p;
                        }
                        if (!string.IsNullOrEmpty(qPropAcumuladoAux))
                        {
                            qPropAcumuladoAux += "@@@" + rdfType + "|";
                        }
                        qPropAcumuladoAux += q;

                        wObjAcumuladoAux = pObjAcumulado;
                        if (string.IsNullOrEmpty(pObjAcumulado))
                        {
                            wObjAcumuladoAux += o;
                        }
                        if (!string.IsNullOrEmpty(wObjAcumuladoAux))
                        {
                            wObjAcumuladoAux += "@@@";
                        }
                        wObjAcumuladoAux += w;
                    }

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
                        GetLoadedEntity(o, pPropAcumuladoAux, pObjAcumuladoAux, ref pEntity, pListResult, MultilangProp);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(wObjAcumuladoAux))
                        {
                            Entity.Property property = pEntity.properties.FirstOrDefault(x => x.prop == qPropAcumuladoAux);
                            if (property == null)
                            {
                                property = new Entity.Property(qPropAcumuladoAux, new List<string>());
                                pEntity.properties.Add(property);
                            }
                            property.values.Add(wObjAcumuladoAux);
                        }
                        else if (pObjAcumuladoAux.Split("@@@").Last().Split("_").Count() > 2 &&
                            Guid.TryParse(pObjAcumuladoAux.Split("@@@").Last().Split("_")[1], out Guid res))
                        {
                            //No inserto nada
                        }
                        else
                        {
                            Entity.Property property = pEntity.properties.FirstOrDefault(x => x.prop == pPropAcumuladoAux);
                            if (property == null)
                            {
                                property = new Entity.Property(pPropAcumuladoAux, new List<string>());
                                pEntity.properties.Add(property);
                            }
                            if (MultilangProp != null && MultilangProp.ContainsKey(pId) && MultilangProp[pId].Any(x => x.Values.Where(x => x.value.Equals(p)).Any()))
                            {
                                List<Dictionary<string, Data>> xx = MultilangProp[pId].Where(x => x["prop"].value.Equals(p)).ToList();
                                foreach (Dictionary<string, Data> keyValuePairs in xx)
                                {
                                    if (keyValuePairs.TryGetValue("prop", out Data d) && d.value.Equals(p))
                                    {
                                        keyValuePairs.TryGetValue("value", out Data propiedadInsertar);
                                        property.values.Add(propiedadInsertar.value);
                                    }
                                }
                            }
                            else
                            {
                                property.values.Add(pObjAcumuladoAux);
                            }
                        }
                    }
                }
            }
        }

        private void GetLoadedEntityCV(string pId, string pPropAcumulado, string pObjAcumulado, ref Entity pEntity, Dictionary<string, List<Dictionary<string, Data>>> pListResult)
        {
            foreach (Dictionary<string, Data> prop in pListResult[pId])
            {
                string s = prop["s"].value;
                string p = prop["p"].value;
                string o = prop["o"].value;
                string q = "";
                string w = "";
                if (prop.ContainsKey("q") && prop.ContainsKey("w"))
                {
                    q = prop["q"].value;
                    w = prop["w"].value;
                }

                string rdfType = pListResult[pId].First(x => x["p"].value == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")["o"].value;
                if (s == pId && p != "http://www.w3.org/2000/01/rdf-schema#label" && p != "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                {
                    string qPropAcumuladoAux = "";
                    string wObjAcumuladoAux = "";
                    if (!string.IsNullOrEmpty(q) && q != "http://www.w3.org/2000/01/rdf-schema#label"
                        && q != "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                    {
                        qPropAcumuladoAux = pPropAcumulado;
                        if (string.IsNullOrEmpty(pPropAcumulado))
                        {
                            qPropAcumuladoAux += p;
                        }
                        if (!string.IsNullOrEmpty(qPropAcumuladoAux))
                        {
                            qPropAcumuladoAux += "@@@" + rdfType + "|";
                        }
                        qPropAcumuladoAux += q;

                        wObjAcumuladoAux = pObjAcumulado;
                        if (string.IsNullOrEmpty(pObjAcumulado))
                        {
                            wObjAcumuladoAux += o;
                        }
                        if (!string.IsNullOrEmpty(wObjAcumuladoAux))
                        {
                            wObjAcumuladoAux += "@@@";
                        }
                        wObjAcumuladoAux += w;
                    }

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
                        if (!string.IsNullOrEmpty(wObjAcumuladoAux))
                        {
                            Entity.Property property = pEntity.properties_cv.FirstOrDefault(x => x.prop == qPropAcumuladoAux);
                            if (property == null)
                            {
                                property = new Entity.Property(qPropAcumuladoAux, new List<string>());
                                pEntity.properties_cv.Add(property);
                            }
                            property.values.Add(wObjAcumuladoAux);
                        }
                        else if (pObjAcumuladoAux.Split("@@@").Last().Split("_").Count() > 2 &&
                            Guid.TryParse(pObjAcumuladoAux.Split("@@@").Last().Split("_")[1], out Guid res))
                        {
                            //No inserto nada
                        }
                        else
                        {
                            Entity.Property property = pEntity.properties_cv.FirstOrDefault(x => x.prop == pPropAcumuladoAux);
                            if (property == null)
                            {
                                property = new Entity.Property(pPropAcumuladoAux, new List<string>());
                                pEntity.properties_cv.Add(property);
                            }
                            property.values.Add(pObjAcumuladoAux);
                        }
                    }
                }
            }
        }

    }
}
