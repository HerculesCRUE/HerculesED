using System;
using System.Collections.Generic;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;
using Gnoss.ApiWrapper;
using System.Linq;
using Hercules.ED.ResearcherObjectLoad.Models;
using ResearchobjectOntology;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class UtilityZenodo
    {
        private static ResourceApi mResourceApi = Carga.mResourceApi;

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

        public static void CrearRoZenodoDesambiguado(string idRo, HashSet<string> pListaIds, Dictionary<string, ResearchObjectZenodo> pDicIdRo,
            Dictionary<ResearchObject, HashSet<string>> pListaRosCreados, Dictionary<string, string> pDicAreasBroader,
            Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectZenodo roA = pDicIdRo[idRo];
            ResearchObject roCreado = new ResearchObject();

            foreach (string idSimilar in pListaIds)
            {
                ResearchObjectZenodo roB = pDicIdRo[idSimilar];
                roCreado = Utility.ConstruirRO("Zenodo", roA, pDicAreasBroader, pDicAreasNombre, roB);
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


        public static ResearchObject ConstruirROZenodo(ResearchObject ro, ResearchObjectZenodo pZenodoObj,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre, ResearchObjectZenodo pZenodoObjB = null)
        {
            // ID
            if (pZenodoObj.id.HasValue)
            {
                // TODO.
                ro.Roh_idZenodo = pZenodoObj.id.Value.ToString();

                if (pZenodoObjB != null && pZenodoObjB.id.HasValue && string.IsNullOrEmpty(ro.Roh_idZenodo))
                {
                    ro.Roh_idZenodo = pZenodoObjB.id.Value.ToString();
                }
            }

            // DOI
            if (!string.IsNullOrEmpty(pZenodoObj.doi))
            {
                ro.Bibo_doi = pZenodoObj.doi;

                if (pZenodoObjB != null && !string.IsNullOrEmpty(pZenodoObjB.doi) && string.IsNullOrEmpty(ro.Bibo_doi))
                {
                    ro.Bibo_doi = pZenodoObjB.doi;
                }
            }

            // ResearchObject Type
            if (!string.IsNullOrEmpty(pZenodoObj.tipo))
            {
                switch (pZenodoObj.tipo)
                {
                    case "dataset":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_1";
                        break;
                    case "presentation":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_2";
                        break;
                    case "image":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_3";
                        break;
                    case "video":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_6";
                        break;
                    case "poster":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_7";
                        break;
                    case "lesson":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_8";
                        break;
                    case "software":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_9";
                        break;
                }

                if (pZenodoObjB != null && !string.IsNullOrEmpty(pZenodoObjB.tipo) && string.IsNullOrEmpty(ro.IdDc_type))
                {
                    switch (pZenodoObjB.tipo)
                    {
                        case "dataset":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_1";
                            break;
                        case "presentation":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_2";
                            break;
                        case "image":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_3";
                            break;
                        case "video":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_6";
                            break;
                        case "poster":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_7";
                            break;
                        case "lesson":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_8";
                            break;
                        case "software":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_9";
                            break;
                    }
                }
            }

            // Título.
            if (!string.IsNullOrEmpty(pZenodoObj.titulo))
            {
                ro.Roh_title = pZenodoObj.titulo;

                if (pZenodoObjB != null && !string.IsNullOrEmpty(pZenodoObjB.titulo) && string.IsNullOrEmpty(ro.Roh_title))
                {
                    ro.Roh_title = pZenodoObjB.titulo;
                }
            }

            // Descripción.
            if (!string.IsNullOrEmpty(pZenodoObj.descripcion))
            {
                ro.Bibo_abstract = pZenodoObj.descripcion;

                if (pZenodoObjB != null && !string.IsNullOrEmpty(pZenodoObjB.descripcion) && string.IsNullOrEmpty(ro.Bibo_abstract))
                {
                    ro.Bibo_abstract = pZenodoObjB.descripcion;
                }
            }

            // URL
            if (!string.IsNullOrEmpty(pZenodoObj.url))
            {
                ro.Vcard_url = pZenodoObj.url;

                if (pZenodoObjB != null && !string.IsNullOrEmpty(pZenodoObjB.url) && string.IsNullOrEmpty(ro.Vcard_url))
                {
                    ro.Vcard_url = pZenodoObjB.url;
                }
            }

            // Fecha Publicación
            if (!string.IsNullOrEmpty(pZenodoObj.fechaPublicacion))
            {
                int dia = Int32.Parse(pZenodoObj.fechaPublicacion.Split("-")[2]);
                int mes = Int32.Parse(pZenodoObj.fechaPublicacion.Split("-")[1]);
                int anyo = Int32.Parse(pZenodoObj.fechaPublicacion.Split("-")[0]);

                ro.Dct_issued = new DateTime(anyo, mes, dia);

                if (pZenodoObjB != null && !string.IsNullOrEmpty(pZenodoObjB.fechaPublicacion) && ro.Dct_issued == null)
                {
                    dia = Int32.Parse(pZenodoObjB.fechaPublicacion.Split("-")[2]);
                    mes = Int32.Parse(pZenodoObjB.fechaPublicacion.Split("-")[1]);
                    anyo = Int32.Parse(pZenodoObjB.fechaPublicacion.Split("-")[0]);

                    ro.Dct_issued = new DateTime(anyo, mes, dia);
                }
            }

            // Etiquetas Enriquecidas
            if (pZenodoObj.etiquetasEnriquecidas != null && pZenodoObj.etiquetasEnriquecidas.Any())
            {
                ro.Roh_enrichedKeywords = pZenodoObj.etiquetasEnriquecidas;

                if (pZenodoObjB != null && pZenodoObjB.etiquetasEnriquecidas != null && pZenodoObjB.etiquetasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKeywords = pZenodoObjB.etiquetasEnriquecidas;
                }
            }

            // Categorias Enriquecidas
            HashSet<string> listaIDs = new HashSet<string>();
            if (pZenodoObj.categoriasEnriquecidas != null && pZenodoObj.categoriasEnriquecidas.Count > 0)
            {
                ro.Roh_enrichedKnowledgeArea = new List<CategoryPath>();
                foreach (string area in pZenodoObj.categoriasEnriquecidas)
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

                if (pZenodoObjB != null && pZenodoObjB.categoriasEnriquecidas != null && pZenodoObjB.categoriasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKnowledgeArea = new List<CategoryPath>();
                    foreach (string area in pZenodoObjB.categoriasEnriquecidas)
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

            // Licencia
            if (!string.IsNullOrEmpty(pZenodoObj.licencia))
            {
                ro.Dct_license = pZenodoObj.licencia;

                if (pZenodoObjB != null && !string.IsNullOrEmpty(pZenodoObjB.licencia) && string.IsNullOrEmpty(ro.Dct_license))
                {
                    ro.Dct_license = pZenodoObjB.licencia;
                }
            }

            // Autores
            if (pZenodoObj.autores != null && pZenodoObj.autores.Any())
            {
                ro.Bibo_authorList = new List<BFO_0000023>();
                int orden = 1;
                foreach (Person_JSON personaRO in pZenodoObj.autores)
                {
                    BFO_0000023 bfo_0000023 = new BFO_0000023();
                    bfo_0000023.Rdf_comment = orden;
                    bfo_0000023.IdRdf_member = personaRO.ID;
                    ro.Bibo_authorList.Add(bfo_0000023);
                    orden++;
                }
            }

            return ro;
        }

    }
}
