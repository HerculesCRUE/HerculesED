using System;
using System.Collections.Generic;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;
using Gnoss.ApiWrapper;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class UtilityZenodo
    {
        public static ResourceApi mResourceApi;

        /// <summary>
        /// Devuelve en <paramref name="listaORCID"/> los ORCID y en <paramref name="listaNombres"/> 
        /// un listado con el nombre de los autores de <paramref name="listadoAux"/>
        /// </summary>
        /// <param name="listaNombres"></param>
        /// <param name="listaORCID"></param>
        /// <param name="listadoAux"></param>
        public static void AutoresZenodo(HashSet<string> listaNombres, HashSet<string> listaORCID, List<ResearchObjectZenodo> listadoAux)
        {
            //Selecciono el nombre completo o la firma.
            foreach (ResearchObjectZenodo item in listadoAux)
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

        public static void CrearRoZenodoDesambiguado(string idRo, HashSet<string> pListaIds, Dictionary<string, ResearchObjectZenodo> pDicIdRo, Dictionary<ResearchobjectOntology.ResearchObject, HashSet<string>> pListaRosCreados, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectZenodo roA = pDicIdRo[idRo];
            ResearchobjectOntology.ResearchObject roCreado = new ResearchobjectOntology.ResearchObject();

            foreach (string idSimilar in pListaIds)
            {
                ResearchObjectZenodo roB = pDicIdRo[idSimilar];
                roCreado = Utility.ConstruirRO("Zenodo", null, null, roA, pDicAreasBroader, pDicAreasNombre, pZenodoObjB: roB);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idRo);
            pListaRosCreados.Add(roCreado, listaTotalIds);
        }

        public static DisambiguationRO GetDisambiguationRoZenodo(ResearchObjectZenodo pResearchObject)
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

    }
}
