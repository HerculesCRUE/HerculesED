using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using Hercules.ED.ResearcherObjectLoad.Models;
using ResearchobjectOntology;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class UtilityGitHub
    {
        private static ResourceApi mResourceApi = Carga.mResourceApi;

        public static void CrearRoGitHubDesambiguado(string idRo, HashSet<string> pListaIds, Dictionary<string, ResearchObjectGitHub> pDicIdRo,
            Dictionary<ResearchObject, HashSet<string>> pListaRosCreados,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectGitHub roA = pDicIdRo[idRo];
            ResearchObject roCreado = new();

            foreach (string idSimilar in pListaIds)
            {
                ResearchObjectGitHub roB = pDicIdRo[idSimilar];
                roCreado = Utility.ConstruirRO("GitHub", roA, pDicAreasBroader, pDicAreasNombre, roB);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idRo);
            pListaRosCreados.Add(roCreado, listaTotalIds);
        }

        /// <summary>
        /// Consulta en SPARQL si hay alguna persona con el usuario de git.
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
                return resultadoQuery.results.bindings.First()["person"].value;
            }

            return string.Empty;
        }

        public static DisambiguationRO GetDisambiguationRoGithub(ResearchObjectGitHub pGithubObj)
        {
            pGithubObj.ID = Guid.NewGuid().ToString();

            DisambiguationRO ro = new()
            {
                ID = pGithubObj.ID,
                title = pGithubObj.Titulo,
                idGithub = pGithubObj.Id.ToString()
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
            Dictionary<string, DisambiguationPerson> diccionarioPersonas = new();
            List<List<string>> listadoLista = Utility.SplitList(listado.Distinct().ToList(), 1000).ToList();
            foreach (List<string> listaIn in listadoLista)
            {

                string selectOut = "SELECT DISTINCT ?personID ?github ?name ?orcid ";
                string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/usuarioGitHub> ?github .
                                    OPTIONAL{{ ?personID <http://w3id.org/roh/ORCID> ?orcid }}
                                    ?personID <http://xmlns.com/foaf/0.1/name> ?name .
                                    FILTER(?github in ('{string.Join("','", listaIn)}'))
                                    }}";

                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(selectOut, whereOut, "person");

                foreach (string firma in listado)
                {
                    foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings.Where(x => x["orcid"].value == firma))
                    {
                        string personID = fila["personID"].value;
                        if (!diccionarioPersonas.ContainsKey(personID))
                        {
                            diccionarioPersonas[personID] = new();
                        }

                        string github = fila["github"].value;
                        string orcid = "";
                        if (fila.ContainsKey("orcid"))
                        {
                            orcid = fila["orcid"].value;
                        }
                        string name = fila["name"].value;
                        DisambiguationPerson persona = new()
                        {
                            gitHubId = github,
                            orcid = orcid,
                            ID = personID,
                            completeName = name
                        };
                        diccionarioPersonas[personID] = persona;
                    }
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
                for (int i = 0; i < item.ListaAutores.Count; i++)
                {
                    if (!string.IsNullOrEmpty(item.ListaAutores[i]))
                    {
                        listaGitHub.Add(item.ListaAutores[i]);
                    }
                }
            }
        }

        public static ResearchObject ConstruirROGithub(ResearchObject ro, ResearchObjectGitHub pGitHubObj,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre, ResearchObjectGitHub pGitHubObjB = null)
        {
            // ID
            if (pGitHubObj.Id.HasValue)
            {
                ro.Roh_idGit = pGitHubObj.Id.Value.ToString();

                if (pGitHubObjB != null && pGitHubObjB.Id.HasValue && string.IsNullOrEmpty(ro.Roh_idGit))
                {
                    ro.Roh_idGit = pGitHubObjB.Id.Value.ToString();
                }
            }

            // ResearchObject Type
            if (!string.IsNullOrEmpty(pGitHubObj.Tipo))
            {
                ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_9";

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.Tipo) && string.IsNullOrEmpty(ro.IdDc_type))
                {
                    ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_9";
                }
            }

            // Título.
            Utility.Titulo(pGitHubObj, pGitHubObjB, ro);

            // Descripción.
            Utility.Descripcion(pGitHubObj, pGitHubObjB, ro);

            // URL
            Utility.URL(pGitHubObj, pGitHubObjB, ro);

            // Fecha Actualización
            if (!string.IsNullOrEmpty(pGitHubObj.FechaActualizacion))
            {
                int dia = int.Parse(pGitHubObj.FechaActualizacion.Split(" ")[0].Split("/")[0]);
                int mes = int.Parse(pGitHubObj.FechaActualizacion.Split(" ")[0].Split("/")[1]);
                int anyo = int.Parse(pGitHubObj.FechaActualizacion.Split(" ")[0].Split("/")[2]);

                ro.Roh_updatedDate = new DateTime(anyo, mes, dia);

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.FechaActualizacion) && ro.Roh_updatedDate == null)
                {
                    dia = int.Parse(pGitHubObjB.FechaActualizacion.Split(" ")[0].Split("/")[0]);
                    mes = int.Parse(pGitHubObjB.FechaActualizacion.Split(" ")[0].Split("/")[1]);
                    anyo = int.Parse(pGitHubObjB.FechaActualizacion.Split(" ")[0].Split("/")[2]);

                    ro.Roh_updatedDate = new DateTime(anyo, mes, dia);
                }
            }

            // Fecha Creación
            if (!string.IsNullOrEmpty(pGitHubObj.FechaCreacion))
            {
                int dia = int.Parse(pGitHubObj.FechaCreacion.Split(" ")[0].Split("/")[0]);
                int mes = int.Parse(pGitHubObj.FechaCreacion.Split(" ")[0].Split("/")[1]);
                int anyo = int.Parse(pGitHubObj.FechaCreacion.Split(" ")[0].Split("/")[2]);

                ro.Dct_issued = new DateTime(anyo, mes, dia);

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.FechaCreacion) && ro.Roh_updatedDate == null)
                {
                    dia = int.Parse(pGitHubObjB.FechaCreacion.Split(" ")[0].Split("/")[0]);
                    mes = int.Parse(pGitHubObjB.FechaCreacion.Split(" ")[0].Split("/")[1]);
                    anyo = int.Parse(pGitHubObjB.FechaCreacion.Split(" ")[0].Split("/")[2]);

                    ro.Dct_issued = new DateTime(anyo, mes, dia);
                }
            }

            // Lenguajes de programación.
            if (pGitHubObj.Lenguajes != null && pGitHubObj.Lenguajes.Any())
            {
                ro.Vcard_hasLanguage = new List<Language>();

                foreach (KeyValuePair<string, float> item in pGitHubObj.Lenguajes)
                {
                    Language lenguajeProg = new();
                    lenguajeProg.Roh_title = item.Key;
                    lenguajeProg.Roh_percentage = item.Value;
                    ro.Vcard_hasLanguage.Add(lenguajeProg);
                }

                if (pGitHubObjB != null && pGitHubObjB.Lenguajes != null && pGitHubObjB.Lenguajes.Any() && ro.Vcard_hasLanguage == null)
                {
                    ro.Vcard_hasLanguage = new List<Language>();

                    foreach (KeyValuePair<string, float> item in pGitHubObjB.Lenguajes)
                    {
                        Language lenguajeProg = new();
                        lenguajeProg.Roh_title = item.Key;
                        lenguajeProg.Roh_percentage = item.Value;
                        ro.Vcard_hasLanguage.Add(lenguajeProg);
                    }
                }
            }

            // Licencia
            Utility.Licencia(pGitHubObj, pGitHubObjB, ro);

            // Número de Releases
            if (pGitHubObj.NumReleases.HasValue)
            {
                ro.Roh_releasesNumber = pGitHubObj.NumReleases.Value;

                if (pGitHubObjB != null && pGitHubObjB.NumReleases.HasValue && ro.Roh_releasesNumber == null)
                {
                    ro.Roh_releasesNumber = pGitHubObjB.NumReleases.Value;
                }
            }

            // Número de Forks
            if (pGitHubObj.NumForks.HasValue)
            {
                ro.Roh_forksNumber = pGitHubObj.NumForks.Value;

                if (pGitHubObjB != null && pGitHubObjB.NumForks.HasValue && ro.Roh_forksNumber == null)
                {
                    ro.Roh_forksNumber = pGitHubObjB.NumForks.Value;
                }
            }

            // Número de Issues
            if (pGitHubObj.NumIssues.HasValue)
            {
                ro.Roh_issuesNumber = pGitHubObj.NumIssues.Value;

                if (pGitHubObjB != null && pGitHubObjB.NumIssues.HasValue && ro.Roh_issuesNumber == null)
                {
                    ro.Roh_issuesNumber = pGitHubObjB.NumIssues.Value;
                }
            }

            // Etiquetas
            if (pGitHubObj.Etiquetas != null && pGitHubObj.Etiquetas.Any())
            {
                ro.Roh_externalKeywords = pGitHubObj.Etiquetas;

                if (pGitHubObjB != null && pGitHubObjB.Etiquetas != null && pGitHubObjB.Etiquetas.Any())
                {
                    ro.Roh_externalKeywords = pGitHubObjB.Etiquetas;
                }
            }

            // Etiquetas Enriquecidas
            Utility.EtiquetasEnriquecidas(pGitHubObj, pGitHubObjB, ro);

            // Categorias Enriquecidas
            Utility.CategoriasEnriquecidas(pGitHubObj, pGitHubObjB, pDicAreasNombre, pDicAreasBroader, ro);

            // Autores.
            if (pGitHubObj.ListaAutores != null && pGitHubObj.ListaAutores.Any())
            {
                ro.Bibo_authorList = new List<BFO_0000023>();
                int seqAutor = 1;
                foreach (string nombre in pGitHubObj.ListaAutores)
                {
                    string idRecursoPersona = UtilityGitHub.ComprobarPersonaUsuarioGitHub(nombre);
                    if (!string.IsNullOrEmpty(idRecursoPersona))
                    {
                        // Autores.   
                        BFO_0000023 miembro = new();
                        miembro.IdRdf_member = idRecursoPersona;
                        miembro.Rdf_comment = seqAutor;
                        seqAutor++;
                        ro.Bibo_authorList.Add(miembro);
                    }
                }

                if (pGitHubObjB != null && pGitHubObjB.ListaAutores != null && pGitHubObjB.ListaAutores.Any())
                {
                    ro.Bibo_authorList = new List<BFO_0000023>();
                    seqAutor = 1;

                    foreach (string nombre in pGitHubObjB.ListaAutores)
                    {
                        string idRecursoPersona = UtilityGitHub.ComprobarPersonaUsuarioGitHub(nombre);
                        if (!string.IsNullOrEmpty(idRecursoPersona))
                        {
                            // Autores.   
                            BFO_0000023 miembro = new();
                            miembro.IdRdf_member = idRecursoPersona;
                            miembro.Rdf_comment = seqAutor;
                            seqAutor++;
                            ro.Bibo_authorList.Add(miembro);
                        }
                    }
                }
            }

            return ro;
        }

    }
}
