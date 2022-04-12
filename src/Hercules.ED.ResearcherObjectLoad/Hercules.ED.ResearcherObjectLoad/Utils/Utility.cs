﻿using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models;
using ResearchobjectOntology;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class Utility
    {
        private static ResourceApi mResourceApi = Carga.mResourceApi;

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

        public static ResearchObject ConstruirRO(string pTipo, object pResearchObject,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre,
            object pResearchObject_b = null)
        {
            ResearchObject ro = new ResearchObject();

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

        /// <summary>
        /// Metodo generico para insertar el valor de la licencia en las clases hijas de RO_JSON
        /// </summary>
        /// <typeparam name="T">T:RO_JSON</typeparam>
        /// <param name="researchObject"></param>
        /// <param name="researchObject_B"></param>
        /// <param name="ro"></param>
        public static void Licencia<T>(T researchObject, T researchObject_B, ResearchObject ro) where T : RO_JSON
        {
            if (!string.IsNullOrEmpty(researchObject.licencia))
            {
                ro.Dct_license = researchObject.licencia;

                if (researchObject_B != null && !string.IsNullOrEmpty(researchObject_B.licencia) && string.IsNullOrEmpty(ro.Dct_license))
                {
                    ro.Dct_license = researchObject_B.licencia;
                }
            }
        }

        /// <summary>
        /// Metodo generico para insertar el valor del titulo en las clases hijas de RO_JSON
        /// </summary>
        /// <typeparam name="T">T:RO_JSON</typeparam>
        /// <param name="researchObject"></param>
        /// <param name="researchObject_B"></param>
        /// <param name="ro"></param>
        public static void Titulo<T>(T researchObject, T researchObject_B, ResearchObject ro) where T : RO_JSON
        {
            if (!string.IsNullOrEmpty(researchObject.titulo))
            {
                ro.Roh_title = researchObject.titulo;

                if (researchObject_B != null && !string.IsNullOrEmpty(researchObject_B.titulo) && string.IsNullOrEmpty(ro.Roh_title))
                {
                    ro.Roh_title = researchObject_B.titulo;
                }
            }
        }

        /// <summary>
        /// Metodo generico para insertar el valor de la descripción en las clases hijas de RO_JSON
        /// </summary>
        /// <typeparam name="T">T:RO_JSON</typeparam>
        /// <param name="researchObject"></param>
        /// <param name="researchObject_B"></param>
        /// <param name="ro"></param>
        public static void Descripcion<T>(T researchObject, T researchObject_B, ResearchObject ro) where T : RO_JSON
        {
            if (!string.IsNullOrEmpty(researchObject.titulo))
            {
                ro.Bibo_abstract = researchObject.descripcion;

                if (researchObject_B != null && !string.IsNullOrEmpty(researchObject_B.descripcion) && string.IsNullOrEmpty(ro.Bibo_abstract))
                {
                    ro.Bibo_abstract = researchObject_B.descripcion;
                }
            }
        }

        /// <summary>
        /// Metodo generico para insertar el valor de la URL en las clases hijas de RO_JSON
        /// </summary>
        /// <typeparam name="T">T:RO_JSON</typeparam>
        /// <param name="researchObject"></param>
        /// <param name="researchObject_B"></param>
        /// <param name="ro"></param>
        public static void URL<T>(T researchObject, T researchObject_B, ResearchObject ro) where T : RO_JSON
        {
            if (!string.IsNullOrEmpty(researchObject.url))
            {
                ro.Vcard_url = researchObject.url;

                if (researchObject_B != null && !string.IsNullOrEmpty(researchObject_B.url) && string.IsNullOrEmpty(ro.Vcard_url))
                {
                    ro.Vcard_url = researchObject_B.url;
                }
            }
        }

        /// <summary>
        /// Metodo generico para insertar el valor de las etiquetas enriquecidas en las clases hijas de RO_JSON
        /// </summary>
        /// <typeparam name="T">T:RO_JSON</typeparam>
        /// <param name="researchObject"></param>
        /// <param name="researchObject_B"></param>
        /// <param name="ro"></param>
        public static void EtiquetasEnriquecidas<T>(T researchObject, T researchObject_B, ResearchObject ro) where T : RO_JSON
        {
            if (researchObject.etiquetasEnriquecidas != null && researchObject.etiquetasEnriquecidas.Any())
            {
                ro.Roh_enrichedKeywords = researchObject.etiquetasEnriquecidas;

                if (researchObject_B != null && researchObject_B.etiquetasEnriquecidas != null && researchObject_B.etiquetasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKeywords = researchObject_B.etiquetasEnriquecidas;
                }
            }
        }

        /// <summary>
        /// Metodo generico para insertar el valor de las categorias enriquecidas en las clases hijas de RO_JSON
        /// </summary>
        /// <typeparam name="T">T:RO_JSON</typeparam>
        /// <param name="researchObject"></param>
        /// <param name="researchObject_B"></param>
        /// <param name="ro"></param>
        public static void CategoriasEnriquecidas<T>(T researchObject, T researchObject_B, 
            Dictionary<string, string> pDicAreasNombre, Dictionary<string, string> pDicAreasBroader, ResearchObject ro) where T : RO_JSON
        {
            HashSet<string> listaIDs = new HashSet<string>();
            if (researchObject.categoriasEnriquecidas != null && researchObject.categoriasEnriquecidas.Count > 0)
            {
                ro.Roh_enrichedKnowledgeArea = new List<CategoryPath>();
                foreach (string area in researchObject.categoriasEnriquecidas)
                {
                    if (pDicAreasNombre.ContainsKey(area.ToLower()))
                    {
                        CategoryPath categoria = new CategoryPath();
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

                if (researchObject_B != null && researchObject_B.categoriasEnriquecidas != null && researchObject_B.categoriasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKnowledgeArea = new List<CategoryPath>();
                    foreach (string area in researchObject_B.categoriasEnriquecidas)
                    {
                        if (pDicAreasNombre.ContainsKey(area.ToLower()))
                        {
                            CategoryPath categoria = new CategoryPath();
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
        }

    }
}
