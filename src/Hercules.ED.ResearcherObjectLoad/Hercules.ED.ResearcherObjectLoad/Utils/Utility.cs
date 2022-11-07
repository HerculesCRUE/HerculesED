using System;
using System.Collections.Generic;
using System.Linq;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using ResearchobjectOntology;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class Utility
    {
        /// <summary>
        /// Lista con los prefijos
        /// </summary>
        private readonly static Dictionary<string, string> dicPrefix = new()
        {
            { "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
            { "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
            { "foaf", "http://xmlns.com/foaf/0.1/" },
            { "vivo", "http://vivoweb.org/ontology/core#" },
            { "owl", "http://www.w3.org/2002/07/owl#" },
            { "bibo", "http://purl.org/ontology/bibo/" },
            { "roh", "http://w3id.org/roh/" },
            { "dct", "http://purl.org/dc/terms/" },
            { "xsd", "http://www.w3.org/2001/XMLSchema#" },
            { "obo", "http://purl.obolibrary.org/obo/" },
            { "vcard", "https://www.w3.org/2006/vcard/ns#" },
            { "dc", "http://purl.org/dc/elements/1.1/" },
            { "gn", "http://www.geonames.org/ontology#" },
            { "skos", "http://www.w3.org/2008/05/skos#" }
        };

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
            ResearchObject ro = new();

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
            if (!string.IsNullOrEmpty(researchObject.Licencia))
            {
                ro.Dct_license = researchObject.Licencia;

                if (researchObject_B != null && !string.IsNullOrEmpty(researchObject_B.Licencia) && string.IsNullOrEmpty(ro.Dct_license))
                {
                    ro.Dct_license = researchObject_B.Licencia;
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
            if (!string.IsNullOrEmpty(researchObject.Titulo))
            {
                ro.Roh_title = researchObject.Titulo;

                if (researchObject_B != null && !string.IsNullOrEmpty(researchObject_B.Titulo) && string.IsNullOrEmpty(ro.Roh_title))
                {
                    ro.Roh_title = researchObject_B.Titulo;
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
            if (!string.IsNullOrEmpty(researchObject.Titulo))
            {
                ro.Bibo_abstract = researchObject.Descripcion;

                if (researchObject_B != null && !string.IsNullOrEmpty(researchObject_B.Descripcion) && string.IsNullOrEmpty(ro.Bibo_abstract))
                {
                    ro.Bibo_abstract = researchObject_B.Descripcion;
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
            if (!string.IsNullOrEmpty(researchObject.Url))
            {
                ro.Vcard_url = researchObject.Url;

                if (researchObject_B != null && !string.IsNullOrEmpty(researchObject_B.Url) && string.IsNullOrEmpty(ro.Vcard_url))
                {
                    ro.Vcard_url = researchObject_B.Url;
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
            if (researchObject.EtiquetasEnriquecidas != null && researchObject.EtiquetasEnriquecidas.Any())
            {
                ro.Roh_enrichedKeywords = researchObject.EtiquetasEnriquecidas;

                if (researchObject_B != null && researchObject_B.EtiquetasEnriquecidas != null && researchObject_B.EtiquetasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKeywords = researchObject_B.EtiquetasEnriquecidas;
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
            HashSet<string> listaIDs = new();
            if (researchObject.CategoriasEnriquecidas != null && researchObject.CategoriasEnriquecidas.Count > 0)
            {
                ro.Roh_enrichedKnowledgeArea = new();
                foreach (string area in researchObject.CategoriasEnriquecidas)
                {
                    if (pDicAreasNombre.ContainsKey(area.ToLower()))
                    {
                        CategoryPath categoria = new();
                        categoria.IdsRoh_categoryNode = new();
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

                if (researchObject_B != null && researchObject_B.CategoriasEnriquecidas != null && researchObject_B.CategoriasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKnowledgeArea = new List<CategoryPath>();
                    foreach (string area in researchObject_B.CategoriasEnriquecidas)
                    {
                        if (pDicAreasNombre.ContainsKey(area.ToLower()))
                        {
                            CategoryPath categoria = new();
                            categoria.IdsRoh_categoryNode = new();
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

        /// <summary>
        /// Cambia la propiedad añadiendole elprefijo
        /// </summary>
        /// <param name="pProperty">Propiedad con la URL completa</param>
        /// <returns>Url con prefijo</returns>
        public static string AniadirPrefijo(string pProperty)
        {
            KeyValuePair<string, string> prefix = dicPrefix.First(x => pProperty.StartsWith(x.Value));
            return pProperty.Replace(prefix.Value, prefix.Key + ":");
        }


        public static string IdentificadorFECYT(string tipoDocumento)
        {
            if (string.IsNullOrEmpty(tipoDocumento))
            {
                return null;
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD1"))
            {
                return "060.010.010.000";
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD2"))
            {
                return "060.010.020.000";
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD3"))
            {
                return "060.010.030.000";
            }
            return null;
        }

    }
}
