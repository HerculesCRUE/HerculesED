using System;
using System.Collections.Generic;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using Gnoss.ApiWrapper;
using System.Linq;
using Hercules.ED.ResearcherObjectLoad.Models;
using ResearchobjectOntology;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class UtilityFigShare
    {
        private static ResourceApi mResourceApi = Carga.mResourceApi;

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

        public static void CrearRoFigshareDesambiguado(string idRo, HashSet<string> pListaIds,
            Dictionary<string, ResearchObjectFigShare> pDicIdRo, Dictionary<ResearchObject, HashSet<string>> pListaRosCreados,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectFigShare roA = pDicIdRo[idRo];
            ResearchObject roCreado = new ResearchObject();

            foreach (string idSimilar in pListaIds)
            {
                ResearchObjectFigShare roB = pDicIdRo[idSimilar];
                roCreado = Utility.ConstruirRO("FigShare", roA, pDicAreasBroader, pDicAreasNombre, roB);
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

        public static ResearchObject ConstruirROFigShare(ResearchObject ro, ResearchObjectFigShare pResearchObject,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre, ResearchObjectFigShare pResearchObjectB = null)
        {
            // ID
            if (pResearchObject.id.HasValue)
            {
                ro.Roh_idFigShare = pResearchObject.id.Value.ToString();

                if (pResearchObjectB != null && pResearchObjectB.id.HasValue && string.IsNullOrEmpty(ro.Roh_idFigShare))
                {
                    ro.Roh_idFigShare = pResearchObjectB.id.Value.ToString();
                }
            }
            // DOI
            if (!string.IsNullOrEmpty(pResearchObject.doi))
            {
                ro.Bibo_doi = pResearchObject.doi;

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.doi) && string.IsNullOrEmpty(ro.Bibo_doi))
                {
                    ro.Bibo_doi = pResearchObjectB.doi;
                }
            }

            // ResearchObject Type
            if (!string.IsNullOrEmpty(pResearchObject.tipo))
            {
                switch (pResearchObject.tipo)
                {
                    case "dataset":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_1";
                        break;
                    case "presentation":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_2";
                        break;
                    case "figure":
                        ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_3";
                        break;
                }

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.tipo) && string.IsNullOrEmpty(ro.IdDc_type))
                {
                    switch (pResearchObjectB.tipo)
                    {
                        case "dataset":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_1";
                            break;
                        case "presentation":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_2";
                            break;
                        case "figure":
                            ro.IdDc_type = $"{mResourceApi.GraphsUrl}items/researchobjecttype_3";
                            break;
                    }
                }
            }

            // Título.
            if (!string.IsNullOrEmpty(pResearchObject.titulo))
            {
                ro.Roh_title = pResearchObject.titulo;

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.titulo) && string.IsNullOrEmpty(ro.Roh_title))
                {
                    ro.Roh_title = pResearchObjectB.titulo;
                }
            }

            // Descripción.
            if (!string.IsNullOrEmpty(pResearchObject.descripcion))
            {
                ro.Bibo_abstract = pResearchObject.descripcion;

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.descripcion) && string.IsNullOrEmpty(ro.Bibo_abstract))
                {
                    ro.Bibo_abstract = pResearchObjectB.descripcion;
                }
            }

            // URL
            if (!string.IsNullOrEmpty(pResearchObject.url))
            {
                ro.Vcard_url = pResearchObject.url;

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.url) && string.IsNullOrEmpty(ro.Vcard_url))
                {
                    ro.Vcard_url = pResearchObjectB.url;
                }
            }

            // Fecha Publicación
            if (!string.IsNullOrEmpty(pResearchObject.fechaPublicacion))
            {
                int dia = Int32.Parse(pResearchObject.fechaPublicacion.Split(" ")[0].Split("/")[1]);
                int mes = Int32.Parse(pResearchObject.fechaPublicacion.Split(" ")[0].Split("/")[0]);
                int anyo = Int32.Parse(pResearchObject.fechaPublicacion.Split(" ")[0].Split("/")[2]);

                ro.Roh_updatedDate = new DateTime(anyo, mes, dia);

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.fechaPublicacion) && ro.Roh_updatedDate == null)
                {
                    dia = Int32.Parse(pResearchObjectB.fechaPublicacion.Split(" ")[0].Split("/")[0]);
                    mes = Int32.Parse(pResearchObjectB.fechaPublicacion.Split(" ")[0].Split("/")[1]);
                    anyo = Int32.Parse(pResearchObjectB.fechaPublicacion.Split(" ")[0].Split("/")[2]);

                    ro.Roh_updatedDate = new DateTime(anyo, mes, dia);
                }
            }

            // Etiquetas
            if (pResearchObject.etiquetas != null && pResearchObject.etiquetas.Any())
            {
                ro.Roh_externalKeywords = pResearchObject.etiquetas;

                if (pResearchObjectB != null && pResearchObjectB.etiquetas != null && pResearchObjectB.etiquetas.Any())
                {
                    ro.Roh_externalKeywords = pResearchObjectB.etiquetas;
                }
            }

            // Etiquetas Enriquecidas
            if (pResearchObject.etiquetasEnriquecidas != null && pResearchObject.etiquetasEnriquecidas.Any())
            {
                ro.Roh_enrichedKeywords = pResearchObject.etiquetasEnriquecidas;

                if (pResearchObjectB != null && pResearchObjectB.etiquetasEnriquecidas != null && pResearchObjectB.etiquetasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKeywords = pResearchObjectB.etiquetasEnriquecidas;
                }
            }

            // Categorias Enriquecidas
            HashSet<string> listaIDs = new HashSet<string>();
            if (pResearchObject.categoriasEnriquecidas != null && pResearchObject.categoriasEnriquecidas.Count > 0)
            {
                ro.Roh_enrichedKnowledgeArea = new List<CategoryPath>();
                foreach (string area in pResearchObject.categoriasEnriquecidas)
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

                if (pResearchObjectB != null && pResearchObjectB.categoriasEnriquecidas != null && pResearchObjectB.categoriasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKnowledgeArea = new List<CategoryPath>();
                    foreach (string area in pResearchObjectB.categoriasEnriquecidas)
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
            if (!string.IsNullOrEmpty(pResearchObject.licencia))
            {
                ro.Dct_license = pResearchObject.licencia;

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.licencia) && string.IsNullOrEmpty(ro.Dct_license))
                {
                    ro.Dct_license = pResearchObjectB.licencia;
                }
            }

            // Autores
            if (pResearchObject.autores != null && pResearchObject.autores.Any())
            {
                ro.Bibo_authorList = new List<BFO_0000023>();
                int orden = 1;
                foreach (Person_JSON personaRO in pResearchObject.autores)
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
