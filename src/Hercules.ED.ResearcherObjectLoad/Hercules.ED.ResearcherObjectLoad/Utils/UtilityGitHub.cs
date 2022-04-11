using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using Hercules.ED.ResearcherObjectLoad.Models;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class UtilityGitHub
    {
        private static ResourceApi mResourceApi = Carga.mResourceApi;

        public static void CrearRoGitHubDesambiguado(string idRo, HashSet<string> pListaIds, Dictionary<string, ResearchObjectGitHub> pDicIdRo,
            Dictionary<ResearchobjectOntology.ResearchObject, HashSet<string>> pListaRosCreados,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectGitHub roA = pDicIdRo[idRo];
            ResearchobjectOntology.ResearchObject roCreado = new ResearchobjectOntology.ResearchObject();

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
        public static ResearchobjectOntology.ResearchObject ConstruirROGithub(ResearchobjectOntology.ResearchObject ro, ResearchObjectGitHub pGitHubObj,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre, ResearchObjectGitHub pGitHubObjB = null)
        {
            // ID
            if (pGitHubObj.id.HasValue)
            {
                ro.Roh_idGit = pGitHubObj.id.Value.ToString();

                if (pGitHubObjB != null && pGitHubObjB.id.HasValue && string.IsNullOrEmpty(ro.Roh_idGit))
                {
                    ro.Roh_idGit = pGitHubObjB.id.Value.ToString();
                }
            }

            // ResearchObject Type
            if (!string.IsNullOrEmpty(pGitHubObj.tipo))
            {
                ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_9";

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.tipo) && string.IsNullOrEmpty(ro.IdDc_type))
                {
                    ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_9";
                }
            }

            // Título.
            if (!string.IsNullOrEmpty(pGitHubObj.titulo))
            {
                ro.Roh_title = pGitHubObj.titulo;

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.titulo) && string.IsNullOrEmpty(ro.Roh_title))
                {
                    ro.Roh_title = pGitHubObjB.titulo;
                }
            }

            // Descripción.
            if (!string.IsNullOrEmpty(pGitHubObj.descripcion))
            {
                ro.Bibo_abstract = pGitHubObj.descripcion;

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.descripcion) && string.IsNullOrEmpty(ro.Bibo_abstract))
                {
                    ro.Bibo_abstract = pGitHubObjB.descripcion;
                }
            }

            // URL
            if (!string.IsNullOrEmpty(pGitHubObj.url))
            {
                ro.Vcard_url = pGitHubObj.url;

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.url) && string.IsNullOrEmpty(ro.Vcard_url))
                {
                    ro.Vcard_url = pGitHubObjB.url;
                }
            }

            // Fecha Actualización
            if (!string.IsNullOrEmpty(pGitHubObj.fechaActualizacion))
            {
                int dia = Int32.Parse(pGitHubObj.fechaActualizacion.Split(" ")[0].Split("/")[0]);
                int mes = Int32.Parse(pGitHubObj.fechaActualizacion.Split(" ")[0].Split("/")[1]);
                int anyo = Int32.Parse(pGitHubObj.fechaActualizacion.Split(" ")[0].Split("/")[2]);

                ro.Roh_updatedDate = new DateTime(anyo, mes, dia);

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.fechaActualizacion) && ro.Roh_updatedDate == null)
                {
                    dia = Int32.Parse(pGitHubObjB.fechaActualizacion.Split(" ")[0].Split("/")[0]);
                    mes = Int32.Parse(pGitHubObjB.fechaActualizacion.Split(" ")[0].Split("/")[1]);
                    anyo = Int32.Parse(pGitHubObjB.fechaActualizacion.Split(" ")[0].Split("/")[2]);

                    ro.Roh_updatedDate = new DateTime(anyo, mes, dia);
                }
            }

            // Fecha Creación
            if (!string.IsNullOrEmpty(pGitHubObj.fechaCreacion))
            {
                int dia = Int32.Parse(pGitHubObj.fechaCreacion.Split(" ")[0].Split("/")[0]);
                int mes = Int32.Parse(pGitHubObj.fechaCreacion.Split(" ")[0].Split("/")[1]);
                int anyo = Int32.Parse(pGitHubObj.fechaCreacion.Split(" ")[0].Split("/")[2]);

                ro.Dct_issued = new DateTime(anyo, mes, dia);

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.fechaCreacion) && ro.Roh_updatedDate == null)
                {
                    dia = Int32.Parse(pGitHubObjB.fechaCreacion.Split(" ")[0].Split("/")[0]);
                    mes = Int32.Parse(pGitHubObjB.fechaCreacion.Split(" ")[0].Split("/")[1]);
                    anyo = Int32.Parse(pGitHubObjB.fechaCreacion.Split(" ")[0].Split("/")[2]);

                    ro.Dct_issued = new DateTime(anyo, mes, dia);
                }
            }

            // Lenguajes de programación.
            if (pGitHubObj.lenguajes != null && pGitHubObj.lenguajes.Any())
            {
                ro.Vcard_hasLanguage = new List<ResearchobjectOntology.Language>();

                foreach (KeyValuePair<string, float> item in pGitHubObj.lenguajes)
                {
                    ResearchobjectOntology.Language lenguajeProg = new ResearchobjectOntology.Language();
                    lenguajeProg.Roh_title = item.Key;
                    lenguajeProg.Roh_percentage = item.Value;
                    ro.Vcard_hasLanguage.Add(lenguajeProg);
                }

                if (pGitHubObjB != null && pGitHubObjB.lenguajes != null && pGitHubObjB.lenguajes.Any() && ro.Vcard_hasLanguage == null)
                {
                    ro.Vcard_hasLanguage = new List<ResearchobjectOntology.Language>();

                    foreach (KeyValuePair<string, float> item in pGitHubObjB.lenguajes)
                    {
                        ResearchobjectOntology.Language lenguajeProg = new ResearchobjectOntology.Language();
                        lenguajeProg.Roh_title = item.Key;
                        lenguajeProg.Roh_percentage = item.Value;
                        ro.Vcard_hasLanguage.Add(lenguajeProg);
                    }
                }
            }

            // Licencia
            if (!string.IsNullOrEmpty(pGitHubObj.licencia))
            {
                ro.Dct_license = pGitHubObj.licencia;

                if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.licencia) && string.IsNullOrEmpty(ro.Dct_license))
                {
                    ro.Dct_license = pGitHubObjB.licencia;
                }
            }

            // Número de Releases
            if (pGitHubObj.numReleases.HasValue)
            {
                ro.Roh_releasesNumber = pGitHubObj.numReleases.Value;

                if (pGitHubObjB != null && pGitHubObjB.numReleases.HasValue && ro.Roh_releasesNumber == null)
                {
                    ro.Roh_releasesNumber = pGitHubObjB.numReleases.Value;
                }
            }

            // Número de Forks
            if (pGitHubObj.numForks.HasValue)
            {
                ro.Roh_forksNumber = pGitHubObj.numForks.Value;

                if (pGitHubObjB != null && pGitHubObjB.numForks.HasValue && ro.Roh_forksNumber == null)
                {
                    ro.Roh_forksNumber = pGitHubObjB.numForks.Value;
                }
            }

            // Número de Issues
            if (pGitHubObj.numIssues.HasValue)
            {
                ro.Roh_issuesNumber = pGitHubObj.numIssues.Value;

                if (pGitHubObjB != null && pGitHubObjB.numIssues.HasValue && ro.Roh_issuesNumber == null)
                {
                    ro.Roh_issuesNumber = pGitHubObjB.numIssues.Value;
                }
            }

            // Etiquetas
            if (pGitHubObj.etiquetas != null && pGitHubObj.etiquetas.Any())
            {
                ro.Roh_externalKeywords = pGitHubObj.etiquetas;

                if (pGitHubObjB != null && pGitHubObjB.etiquetas != null && pGitHubObjB.etiquetas.Any())
                {
                    ro.Roh_externalKeywords = pGitHubObjB.etiquetas;
                }
            }

            // Etiquetas Enriquecidas
            if (pGitHubObj.etiquetasEnriquecidas != null && pGitHubObj.etiquetasEnriquecidas.Any())
            {
                ro.Roh_enrichedKeywords = pGitHubObj.etiquetasEnriquecidas;

                if (pGitHubObjB != null && pGitHubObjB.etiquetasEnriquecidas != null && pGitHubObjB.etiquetasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKeywords = pGitHubObjB.etiquetasEnriquecidas;
                }
            }

            // Categorias Enriquecidas
            HashSet<string> listaIDs = new HashSet<string>();
            if (pGitHubObj.categoriasEnriquecidas != null && pGitHubObj.categoriasEnriquecidas.Count > 0)
            {
                ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                foreach (string area in pGitHubObj.categoriasEnriquecidas)
                {
                    if (pDicAreasNombre.ContainsKey(area.ToLower()))
                    {
                        ResearchobjectOntology.CategoryPath categoria = new ResearchobjectOntology.CategoryPath();
                        categoria.IdsRoh_categoryNode = new List<string>();
                        categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.ToLower()]);
                        string idHijo = pDicAreasNombre[area.ToLower()];
                        string idHijoAux = idHijo;
                        if (!listaIDs.Contains(idHijo))
                        {
                            while (!idHijo.EndsWith(".0.0.0"))
                            {
                                categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                idHijo = pDicAreasBroader[idHijo];
                            }
                            if (categoria.IdsRoh_categoryNode.Count > 0)
                            {
                                ro.Roh_enrichedKnowledgeArea.Add(categoria);
                            }
                        }
                        listaIDs.Add(idHijoAux);
                    }
                }

                if (pGitHubObjB != null && pGitHubObjB.categoriasEnriquecidas != null && pGitHubObjB.categoriasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                    foreach (string area in pGitHubObjB.categoriasEnriquecidas)
                    {
                        if (pDicAreasNombre.ContainsKey(area.ToLower()))
                        {
                            ResearchobjectOntology.CategoryPath categoria = new ResearchobjectOntology.CategoryPath();
                            categoria.IdsRoh_categoryNode = new List<string>();
                            categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.ToLower()]);
                            string idHijo = pDicAreasNombre[area.ToLower()];
                            string idHijoAux = idHijo;
                            if (!listaIDs.Contains(idHijo))
                            {
                                while (!idHijo.EndsWith(".0.0.0"))
                                {
                                    categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                    idHijo = pDicAreasBroader[idHijo];
                                }
                                if (categoria.IdsRoh_categoryNode.Count > 0)
                                {
                                    ro.Roh_enrichedKnowledgeArea.Add(categoria);
                                }
                            }
                            listaIDs.Add(idHijoAux);
                        }
                    }
                }
            }

            // Autores.
            if (pGitHubObj.listaAutores != null && pGitHubObj.listaAutores.Any())
            {
                ro.Bibo_authorList = new List<ResearchobjectOntology.BFO_0000023>();
                int seqAutor = 1;
                foreach (string nombre in pGitHubObj.listaAutores)
                {
                    string idRecursoPersona = UtilityGitHub.ComprobarPersonaUsuarioGitHub(nombre);
                    if (!string.IsNullOrEmpty(idRecursoPersona))
                    {
                        // Autores.   
                        ResearchobjectOntology.BFO_0000023 miembro = new ResearchobjectOntology.BFO_0000023();
                        miembro.IdRdf_member = idRecursoPersona;
                        miembro.Rdf_comment = seqAutor;
                        seqAutor++;
                        ro.Bibo_authorList.Add(miembro);
                    }
                }

                if (pGitHubObjB != null && pGitHubObjB.listaAutores != null && pGitHubObjB.listaAutores.Any())
                {
                    ro.Bibo_authorList = new List<ResearchobjectOntology.BFO_0000023>();
                    seqAutor = 1;

                    foreach (string nombre in pGitHubObjB.listaAutores)
                    {
                        string idRecursoPersona = UtilityGitHub.ComprobarPersonaUsuarioGitHub(nombre);
                        if (!string.IsNullOrEmpty(idRecursoPersona))
                        {
                            // Autores.   
                            ResearchobjectOntology.BFO_0000023 miembro = new ResearchobjectOntology.BFO_0000023();
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
