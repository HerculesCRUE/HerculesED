using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using GuardadoCV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace HerculesAplicacionConsola.Utils
{
    class UtilityIdentidad
    {
        /// <summary>
        /// API
        /// </summary>
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}../../../Config/configOAuth/OAuthV3.config");


        public static string GetIdentificadorDatosPersonales(string pId, string pGraph) 
        {
            string selectID = "select ?o";
            string whereID = $@"where{{
                                    <{pId}> <http://w3id.org/roh/personalData> ?o .                
                                    }}";

            SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                return fila["o"].value;
            }

            throw new Exception("No existe la entidad http://w3id.org/roh/personalData");
        }

        /// <summary>
        /// Dado el identificador de una persona y el grafo al que pertenece.
        /// Devuelve el string de identificacion de "Identificadores - Otros".
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pGraph"></param>
        /// <returns></returns>
        public static string GetIdentificadorOtrosId(string pId, string pGraph)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            try
            {
                int numLimit = 1000;
                int offset = 0;

                string selectID = "select * where{ select distinct ?s ?p ?o";
                string whereID = $@"where{{?x <http://gnoss/hasEntidad> <{pId}>.
                                            ?x <http://gnoss/hasEntidad> ?s.
                                            ?s <http://w3id.org/roh/otherIds> ?o .
                                            ?s ?p ?o 
                                        }}
                                    order by desc(?s) desc(?p) desc(?o)
                                    }}
                                    limit {numLimit} offset {offset}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    if (!listResult.ContainsKey(fila["o"].value))
                    {
                        return fila["o"].value;
                    }
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Dado el identificador de una persona y el grafo al que pertenece.
        /// Devuelve el string de identificacion del movil.
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pGraph"></param>
        /// <returns></returns>
        public static string GetMovilId(string pId, string pGraph)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            try
            {
                int numLimit = 1000;
                int offset = 0;

                string selectID = "select * where{ select distinct ?s ?p ?o";
                string whereID = $@"where{{?x <http://gnoss/hasEntidad> <{pId}>.
                                            ?x <http://gnoss/hasEntidad> ?s.
                                            ?s <http://w3id.org/roh/hasMobilePhone> ?o .
                                            ?s ?p ?o 
                                        }}
                                    order by desc(?s) desc(?p) desc(?o)
                                    }}
                                    limit {numLimit} offset {offset}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    if (!listResult.ContainsKey(fila["o"].value))
                    {
                        return fila["o"].value;
                    }
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Dado el identificador de una persona y el grafo al que pertenece.
        /// Devuelve el string de identificacion del telefono fijo.
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pGraph"></param>
        /// <returns></returns>
        public static string GetTelefonoId(string pId, string pGraph)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            try
            {
                int numLimit = 1000;
                int offset = 0;

                string selectID = "select * where{ select distinct ?s ?p ?o";
                string whereID = $@"where{{?x <http://gnoss/hasEntidad> <{pId}>.
                                            ?x <http://gnoss/hasEntidad> ?s.
                                            ?s <https://www.w3.org/2006/vcard/ns#hasTelephone> ?o .
                                            ?s ?p ?o 
                                        }}
                                    order by desc(?s) desc(?p) desc(?o)
                                    }}
                                    limit {numLimit} offset {offset}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    if (!listResult.ContainsKey(fila["o"].value))
                    {
                        return fila["o"].value;
                    }
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Dado el identificador de una persona y el grafo al que pertenece.
        /// Devuelve el string de identificacion del fax.
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pGraph"></param>
        /// <returns></returns>
        public static string GetFaxId(string pId, string pGraph)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            try
            {
                int numLimit = 1000;
                int offset = 0;

                string selectID = "select * where{ select distinct ?s ?p ?o";
                string whereID = $@"where{{?x <http://gnoss/hasEntidad> <{pId}>.
                                            ?x <http://gnoss/hasEntidad> ?s.
                                            ?s <http://w3id.org/roh/hasFax> ?o .
                                            ?s ?p ?o 
                                        }}
                                    order by desc(?s) desc(?p) desc(?o)
                                    }}
                                    limit {numLimit} offset {offset}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    if (!listResult.ContainsKey(fila["o"].value))
                    {
                        return fila["o"].value;
                    }
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Dado el identificador de una persona y el grafo al que pertenece.
        /// Devuelve el string de identificacion de la direccion de contacto.
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pGraph"></param>
        /// <returns></returns>
        public static string GetDireccionId(string pId, string pGraph)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            try
            {
                int numLimit = 1000;
                int offset = 0;

                string selectID = "select * where{ select distinct ?s ?p ?o";
                string whereID = $@"where{{?x <http://gnoss/hasEntidad> <{pId}>.
                                            ?x <http://gnoss/hasEntidad> ?s.
                                            ?s <https://www.w3.org/2006/vcard/ns#address> ?o .
                                            ?s ?p ?o 
                                        }}
                                    order by desc(?s) desc(?p) desc(?o)
                                    }}
                                    limit {numLimit} offset {offset}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    if (!listResult.ContainsKey(fila["o"].value))
                    {
                        return fila["o"].value;
                    }
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Dado el identificador de una persona y el grafo al que pertenece.
        /// Devuelve el string de identificacion de la direccion de nacimiento.
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pGraph"></param>
        /// <returns></returns>
        public static string GetDireccionNacimientoId(string pId, string pGraph)
        {
            Dictionary<string, List<Dictionary<string, Data>>> listResult = new Dictionary<string, List<Dictionary<string, Data>>>();
            try
            {
                int numLimit = 1000;
                int offset = 0;

                string selectID = "select * where{ select distinct ?s ?p ?o";
                string whereID = $@"where{{?x <http://gnoss/hasEntidad> <{pId}>.
                                            ?x <http://gnoss/hasEntidad> ?s.
                                            ?s <http://w3id.org/roh/birthplace> ?o .
                                            ?s ?p ?o 
                                        }}
                                    order by desc(?s) desc(?p) desc(?o)
                                    }}
                                    limit {numLimit} offset {offset}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, pGraph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    if (!listResult.ContainsKey(fila["o"].value))
                    {
                        return fila["o"].value;
                    }
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
