using System;
using System.Collections.Generic;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using Gnoss.ApiWrapper;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class UtilityFigShare
    {
        public static ResourceApi mResourceApi;

        public static DisambiguationRO GetDisambiguationRO(ResearchObjectFigShare pResearchObject)
        {
            pResearchObject.ID = Guid.NewGuid().ToString();

            DisambiguationRO ro = new DisambiguationRO()
            {
                ID = pResearchObject.ID,
                doi = pResearchObject.doi,
                title = pResearchObject.titulo,
                idFigshare = pResearchObject.id.ToString()
            };

            return ro;
        }

        public static void CrearRoFigshareDesambiguado(string idRo, HashSet<string> pListaIds, Dictionary<string, ResearchObjectFigShare> pDicIdRo, Dictionary<ResearchobjectOntology.ResearchObject, HashSet<string>> pListaRosCreados, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectFigShare roA = pDicIdRo[idRo];
            ResearchobjectOntology.ResearchObject roCreado = new ResearchobjectOntology.ResearchObject();

            foreach (string idSimilar in pListaIds)
            {
                ResearchObjectFigShare roB = pDicIdRo[idSimilar];
                roCreado = Utility.ConstruirRO("FigShare", roA, null, null, pDicAreasBroader, pDicAreasNombre, pResearchObjectB: roB);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idRo);
            pListaRosCreados.Add(roCreado, listaTotalIds);
        }

        /// <summary>
        /// Obtiene el ORCID, en caso de existir, de la persona a partir del Identificador de FigShare.
        /// </summary>
        /// <param name="figshareID"></param>
        /// <returns></returns>
        public static string ObtenerORCIDPorFigShareID(string figshareID)
        {
            string orcid = "";

            string selectOut = "SELECT DISTINCT ?personID ?orcid";
            string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/usuarioFigShare> ""{figshareID}"" .
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
        /// Devuelve en <paramref name="listaORCID"/> los ORCID y en <paramref name="listaNombres"/> 
        /// un listado con el nombre de los autores de <paramref name="listadoAux"/>
        /// </summary>
        /// <param name="listaNombres"></param>
        /// <param name="listaORCID"></param>
        /// <param name="listadoAux"></param>
        public static void AutoresFigShare(HashSet<string> listaNombres, HashSet<string> listaORCID, List<ResearchObjectFigShare> listadoAux)
        {
            //Selecciono el nombre completo o la firma.
            foreach (ResearchObjectFigShare item in listadoAux)
            {
                for (int i = 0; i < item.autores.Count; i++)
                {
                    if (!string.IsNullOrEmpty(item.autores[i].orcid))
                    {
                        listaORCID.Add(item.autores[i].orcid);
                    }
                    if (!string.IsNullOrEmpty(item.autores[i].nombreCompleto))
                    {
                        listaNombres.Add(item.autores[i].nombreCompleto);
                    };
                }
            }
        }

        /// <summary>
        /// Obtiene el Identificador de la persona a partir de el identificador de FigShare.
        /// </summary>
        /// <param name="figshareID"></param>
        /// <returns></returns>
        public static string ObtenerPersonaPorFigShareID(string figshareID)
        {
            string personID = "";

            string selectOut = "SELECT DISTINCT ?personID";
            string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/usuarioFigShare> ""{figshareID}"" .
                                    }}";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(selectOut, whereOut, "person");
            foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
            {
                personID = fila["personID"].value;
            }
            return personID;
        }

    }
}
