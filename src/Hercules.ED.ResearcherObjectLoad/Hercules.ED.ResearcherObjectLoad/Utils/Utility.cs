using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class Utility
    {
        public static ResourceApi mResourceApi = Carga.mResourceApi;

        /// <summary>
        /// Método para dividir listas
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pItems">Listado</param>
        /// <param name="pSize">Tamaño</param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> pItems, int pSize)
        {
            for (int i = 0; i < pItems.Count; i += pSize)
            {
                yield return pItems.GetRange(i, Math.Min(pSize, pItems.Count - i));
            }
        }

        public static ResearchobjectOntology.ResearchObject ConstruirRO(string pTipo, object pResearchObject,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre,
            object pResearchObject_b = null)
        {
            ResearchobjectOntology.ResearchObject ro = new ResearchobjectOntology.ResearchObject();

            // Estado de validación (IsValidated)
            ro.Roh_isValidated = true;

            if (pTipo == "FigShare")
            {
                ResearchObjectFigShare pRO = (ResearchObjectFigShare)pResearchObject;
                ResearchObjectFigShare pRO_b = null;
                if (pResearchObject_b != null)
                {
                    pRO_b = (ResearchObjectFigShare)pResearchObject_b;
                }

                UtilityFigShare.ConstruirROFigShare(ro, pRO, pDicAreasBroader, pDicAreasNombre, pRO_b);
            }
            else if (pTipo == "GitHub")
            {
                ResearchObjectGitHub pRO = (ResearchObjectGitHub)pResearchObject;
                ResearchObjectGitHub pRO_b = null;
                if (pResearchObject_b != null)
                {
                    pRO_b = (ResearchObjectGitHub)pResearchObject_b;
                }

                UtilityGitHub.ConstruirROGithub(ro, pRO, pDicAreasBroader, pDicAreasNombre, pRO_b);
            }
            else if (pTipo == "Zenodo")
            {
                ResearchObjectZenodo pRO = (ResearchObjectZenodo)pResearchObject;
                ResearchObjectZenodo pRO_b = null;
                if (pResearchObject_b != null)
                {
                    pRO_b = (ResearchObjectZenodo)pResearchObject_b;
                }

                UtilityZenodo.ConstruirROZenodo(ro, pRO, pDicAreasBroader, pDicAreasNombre, pRO_b);
            }
            return ro;
        }


    }
}
