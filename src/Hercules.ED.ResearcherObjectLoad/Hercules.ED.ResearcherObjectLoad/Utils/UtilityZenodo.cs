﻿using System;
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
            Utility.Titulo(pZenodoObj, pZenodoObjB, ro);

            // Descripción.
            Utility.Descripcion(pZenodoObj, pZenodoObjB, ro);

            // URL
            Utility.URL(pZenodoObj, pZenodoObjB, ro);

            // Fecha Publicación
            if (!string.IsNullOrEmpty(pZenodoObj.fechaPublicacion))
            {
                int dia = int.Parse(pZenodoObj.fechaPublicacion.Split("-")[2]);
                int mes = int.Parse(pZenodoObj.fechaPublicacion.Split("-")[1]);
                int anyo = int.Parse(pZenodoObj.fechaPublicacion.Split("-")[0]);

                ro.Dct_issued = new DateTime(anyo, mes, dia);

                if (pZenodoObjB != null && !string.IsNullOrEmpty(pZenodoObjB.fechaPublicacion) && ro.Dct_issued == null)
                {
                    dia = int.Parse(pZenodoObjB.fechaPublicacion.Split("-")[2]);
                    mes = int.Parse(pZenodoObjB.fechaPublicacion.Split("-")[1]);
                    anyo = int.Parse(pZenodoObjB.fechaPublicacion.Split("-")[0]);

                    ro.Dct_issued = new DateTime(anyo, mes, dia);
                }
            }

            // Etiquetas Enriquecidas
            Utility.EtiquetasEnriquecidas(pZenodoObj, pZenodoObjB, ro);

            // Categorias Enriquecidas
            Utility.CategoriasEnriquecidas(pZenodoObj, pZenodoObjB, pDicAreasNombre, pDicAreasBroader, ro);

            // Licencia
            Utility.Licencia(pZenodoObj, pZenodoObjB, ro);

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
