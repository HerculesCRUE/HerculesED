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

            DisambiguationRO ro = new()
            {
                ID = pResearchObject.ID,
                doi = pResearchObject.Doi,
                title = pResearchObject.Titulo,
                idFigshare = pResearchObject.Id.ToString()
            };

            return ro;
        }

        public static void CrearRoFigshareDesambiguado(string idRo, HashSet<string> pListaIds,
            Dictionary<string, ResearchObjectFigShare> pDicIdRo, Dictionary<ResearchObject, HashSet<string>> pListaRosCreados,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectFigShare roA = pDicIdRo[idRo];
            ResearchObject roCreado = new();

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
        public static Dictionary<string, string> ObtenerORCIDPorTokenFigshare(string figshareID)
        {
            Dictionary<string, string> dicDatos = new();

            string selectOut = "SELECT DISTINCT ?personID ?orcid ?usuarioFigShare ";
            string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/tokenFigShare> ""{figshareID}"" .
                                    ?personID <http://w3id.org/roh/usuarioFigShare> ?usuarioFigShare .
                                    ?personID <http://w3id.org/roh/ORCID> ?orcid.
                                    }}";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(selectOut, whereOut, "person");
            foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
            {
                dicDatos.Add(fila["orcid"].value, fila["usuarioFigShare"].value);
            }
            return dicDatos;
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
                for (int i = 0; i < item.Autores.Count; i++)
                {
                    if (!string.IsNullOrEmpty(item.Autores[i].orcid))
                    {
                        listaORCID.Add(item.Autores[i].orcid);
                    }
                    if (!string.IsNullOrEmpty(item.Autores[i].nombreCompleto))
                    {
                        listaNombres.Add(item.Autores[i].nombreCompleto);
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene el Identificador de la persona a partir de el identificador de FigShare.
        /// </summary>
        /// <param name="figshareID"></param>
        /// <returns></returns>
        public static string ObtenerPersonaPorTokenFigShare(string figshareID)
        {
            string personID = "";

            string selectOut = "SELECT DISTINCT ?personID";
            string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/tokenFigShare> ""{figshareID}"" .
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
            if (pResearchObject.Id.HasValue)
            {
                ro.Roh_idFigShare = pResearchObject.Id.Value.ToString();

                if (pResearchObjectB != null && pResearchObjectB.Id.HasValue && string.IsNullOrEmpty(ro.Roh_idFigShare))
                {
                    ro.Roh_idFigShare = pResearchObjectB.Id.Value.ToString();
                }
            }

            // DOI
            if (!string.IsNullOrEmpty(pResearchObject.Doi))
            {
                ro.Bibo_doi = pResearchObject.Doi;

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.Doi) && string.IsNullOrEmpty(ro.Bibo_doi))
                {
                    ro.Bibo_doi = pResearchObjectB.Doi;
                }
            }

            // ResearchObject Type
            if (!string.IsNullOrEmpty(pResearchObject.Tipo))
            {
                switch (pResearchObject.Tipo)
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

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.Tipo) && string.IsNullOrEmpty(ro.IdDc_type))
                {
                    switch (pResearchObjectB.Tipo)
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
            Utility.Titulo(pResearchObject, pResearchObjectB, ro);

            // Descripción.
            Utility.Descripcion(pResearchObject, pResearchObjectB, ro);

            // URL
            Utility.URL(pResearchObject, pResearchObjectB, ro);

            // Fecha Publicación
            if (!string.IsNullOrEmpty(pResearchObject.FechaPublicacion))
            {
                int dia = int.Parse(pResearchObject.FechaPublicacion.Split(" ")[0].Split("/")[1]);
                int mes = int.Parse(pResearchObject.FechaPublicacion.Split(" ")[0].Split("/")[0]);
                int anyo = int.Parse(pResearchObject.FechaPublicacion.Split(" ")[0].Split("/")[2]);

                ro.Roh_updatedDate = new DateTime(anyo, mes, dia);

                if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.FechaPublicacion) && ro.Roh_updatedDate == null)
                {
                    dia = int.Parse(pResearchObjectB.FechaPublicacion.Split(" ")[0].Split("/")[0]);
                    mes = int.Parse(pResearchObjectB.FechaPublicacion.Split(" ")[0].Split("/")[1]);
                    anyo = int.Parse(pResearchObjectB.FechaPublicacion.Split(" ")[0].Split("/")[2]);

                    ro.Roh_updatedDate = new DateTime(anyo, mes, dia);
                }
            }

            // Etiquetas
            if (pResearchObject.Etiquetas != null && pResearchObject.Etiquetas.Any())
            {
                ro.Roh_externalKeywords = pResearchObject.Etiquetas;

                if (pResearchObjectB != null && pResearchObjectB.Etiquetas != null && pResearchObjectB.Etiquetas.Any())
                {
                    ro.Roh_externalKeywords = pResearchObjectB.Etiquetas;
                }
            }

            // Etiquetas Enriquecidas
            Utility.EtiquetasEnriquecidas(pResearchObject, pResearchObjectB, ro);

            // Categorias Enriquecidas
            Utility.CategoriasEnriquecidas(pResearchObject, pResearchObjectB, pDicAreasNombre, pDicAreasBroader, ro);

            // Licencia
            Utility.Licencia(pResearchObject, pResearchObjectB, ro);

            // Autores
            if (pResearchObject.Autores != null && pResearchObject.Autores.Any())
            {
                ro.Bibo_authorList = new List<BFO_0000023>();
                int orden = 1;
                foreach (Person_JSON personaRO in pResearchObject.Autores)
                {
                    BFO_0000023 bfo_0000023 = new();
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
