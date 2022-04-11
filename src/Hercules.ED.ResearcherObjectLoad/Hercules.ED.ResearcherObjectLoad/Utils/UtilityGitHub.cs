using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class UtilityGitHub
    {
        public static ResourceApi mResourceApi;

        public static void CrearRoGitHubDesambiguado(string idRo, HashSet<string> pListaIds, Dictionary<string, ResearchObjectGitHub> pDicIdRo,
            Dictionary<ResearchobjectOntology.ResearchObject, HashSet<string>> pListaRosCreados,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectGitHub roA = pDicIdRo[idRo];
            ResearchobjectOntology.ResearchObject roCreado = new ResearchobjectOntology.ResearchObject();

            foreach (string idSimilar in pListaIds)
            {
                ResearchObjectGitHub roB = pDicIdRo[idSimilar];
                roCreado = Utility.ConstruirRO("GitHub", null, roA, null, pDicAreasBroader, pDicAreasNombre, pGitHubObjB: roB);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idRo);
            pListaRosCreados.Add(roCreado, listaTotalIds);
        }

        /// <summary>
        /// Consulta en SPARQL si hay alguna persona con el usuario de git.
        /// TODO: Revisar si hay más de una persona con el mismo ID.
        /// </summary>
        /// <param name="pNombre"></param>
        /// <returns></returns>
        public static string ComprobarPersonaUsuarioGitHub(string pNombre)
        {
            // Consulta sparql.
            string select = "SELECT ?person";
            string where = $@"WHERE {{
                                ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                ?person <http://w3id.org/roh/usuarioGitHub> ?nombre. 
                                FILTER(?nombre = '{pNombre}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["person"].value;
                }
            }

            return string.Empty;
        }

        public static DisambiguationRO GetDisambiguationRoGithub(ResearchObjectGitHub pGithubObj)
        {
            pGithubObj.ID = Guid.NewGuid().ToString();

            DisambiguationRO ro = new DisambiguationRO()
            {
                ID = pGithubObj.ID,
                title = pGithubObj.titulo,
                idGithub = pGithubObj.id.ToString()
            };

            return ro;
        }

        /// <summary>
        /// Obtiene el Identificador de la persona a partir de el identificador de GitHub.
        /// </summary>
        /// <param name="githubID"></param>
        /// <returns></returns>
        public static string ObtenerPersonaPorGitHubID(string githubID)
        {
            string personID = "";

            string selectOut = "SELECT DISTINCT ?personID";
            string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/usuarioGitHub> ""{githubID}"" .
                                    }}";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(selectOut, whereOut, "person");
            foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
            {
                personID = fila["personID"].value;
            }
            return personID;
        }

        /// <summary>
        /// Obtiene el ORCID, en caso de existir, de la persona a partir del Identificador de GitHub.
        /// </summary>
        /// <param name="githubID"></param>
        /// <returns></returns>
        public static string ObtenerORCIDPorGitHubID(string githubID)
        {
            string orcid = "";

            string selectOut = "SELECT DISTINCT ?personID ?orcid";
            string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/usuarioGitHub> ""{githubID}"" .
                                    ?personID <http://w3id.org/roh/ORCID> ?orcid.
                                    }}";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(selectOut, whereOut, "person");
            foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
            {
                orcid = fila["orcid"].value;
            }
            return orcid;
        }

        /// <summary>
        /// Obtiene un diccionario con las personas y su usuario de GitHub.
        /// </summary>
        /// <param name="resourceApi"></param>
        /// <param name="listado"></param>
        /// <returns></returns>
        public static Dictionary<string, DisambiguationPerson> ObtenerPersonasGitHub(List<string> listado)
        {
            Dictionary<string, DisambiguationPerson> diccionarioPersonas = new Dictionary<string, DisambiguationPerson>();

            string selectOut = "SELECT DISTINCT ?personID ?github ?name ?orcid ";
            string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/usuarioGitHub> ?github .
                                    OPTIONAL{{ ?personID <http://w3id.org/roh/ORCID> ?orcid }}
                                    ?personID <http://xmlns.com/foaf/0.1/name> ?name .
                                    }}";

            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(selectOut, whereOut, "person");

            foreach (string firma in listado)
            {
                HashSet<int> scores = new HashSet<int>();
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings.Where(x => x["orcid"].value == firma))
                {
                    string personID = fila["personID"].value;
                    if (!diccionarioPersonas.ContainsKey(personID))
                    {
                        diccionarioPersonas[personID] = new DisambiguationPerson();
                    }

                    string github = fila["github"].value;
                    string orcid = "";
                    if (fila.ContainsKey("orcid"))
                    {
                        orcid = fila["orcid"].value;
                    }
                    string name = fila["name"].value;
                    DisambiguationPerson persona = new DisambiguationPerson
                    {
                        gitHubId = github,
                        orcid = orcid,
                        ID = personID,
                        completeName = name
                    };
                    diccionarioPersonas[personID] = persona;
                }
            }

            return diccionarioPersonas;
        }

        /// <summary>
        /// Devuelve en <paramref name="listaGitHub"/> un listado con los autores de <paramref name="listadoAux"/>
        /// </summary>
        /// <param name="listaGitHub"></param>
        /// <param name="listadoAux"></param>
        public static void AutoresGitHub(HashSet<string> listaGitHub, List<ResearchObjectGitHub> listadoAux)
        {
            //Selecciono el identificador de GitHub
            foreach (ResearchObjectGitHub item in listadoAux)
            {
                for (int i = 0; i < item.listaAutores.Count; i++)
                {
                    if (!string.IsNullOrEmpty(item.listaAutores[i]))
                    {
                        listaGitHub.Add(item.listaAutores[i]);
                    };
                }
            }
        }

    }
}
