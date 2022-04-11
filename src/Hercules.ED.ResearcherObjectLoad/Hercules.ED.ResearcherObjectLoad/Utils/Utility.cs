using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class Utility
    {
        public static ResourceApi mResourceApi;

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

        public static ResearchobjectOntology.ResearchObject ConstruirRO(string pTipo,
            ResearchObjectFigShare pResearchObject, ResearchObjectGitHub pGitHubObj, ResearchObjectZenodo pZenodoObj,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre,
            ResearchObjectFigShare pResearchObjectB = null, ResearchObjectGitHub pGitHubObjB = null, ResearchObjectZenodo pZenodoObjB = null)
        {
            ResearchobjectOntology.ResearchObject ro = new ResearchobjectOntology.ResearchObject();

            // Estado de validación (IsValidated)
            ro.Roh_isValidated = true;

            if (pTipo == "FigShare")
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
                    ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                    foreach (string area in pResearchObject.categoriasEnriquecidas)
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

                    if (pResearchObjectB != null && pResearchObjectB.categoriasEnriquecidas != null && pResearchObjectB.categoriasEnriquecidas.Any())
                    {
                        ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                        foreach (string area in pResearchObjectB.categoriasEnriquecidas)
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
                    ro.Bibo_authorList = new List<ResearchobjectOntology.BFO_0000023>();
                    int orden = 1;
                    foreach (PersonRO personaRO in pResearchObject.autores)
                    {
                        ResearchobjectOntology.BFO_0000023 bfo_0000023 = new ResearchobjectOntology.BFO_0000023();
                        bfo_0000023.Rdf_comment = orden;
                        bfo_0000023.IdRdf_member = personaRO.ID;
                        ro.Bibo_authorList.Add(bfo_0000023);
                        orden++;
                    }
                }

            }
            else if (pTipo == "GitHub")
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
            }
            else if (pTipo == "Zenodo")
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
                    ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                    foreach (string area in pZenodoObj.categoriasEnriquecidas)
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

                    if (pZenodoObjB != null && pZenodoObjB.categoriasEnriquecidas != null && pZenodoObjB.categoriasEnriquecidas.Any())
                    {
                        ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                        foreach (string area in pZenodoObjB.categoriasEnriquecidas)
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
                    ro.Bibo_authorList = new List<ResearchobjectOntology.BFO_0000023>();
                    int orden = 1;
                    foreach (PersonZenodo personaRO in pZenodoObj.autores)
                    {
                        ResearchobjectOntology.BFO_0000023 bfo_0000023 = new ResearchobjectOntology.BFO_0000023();
                        bfo_0000023.Rdf_comment = orden;
                        bfo_0000023.IdRdf_member = personaRO.ID;
                        ro.Bibo_authorList.Add(bfo_0000023);
                        orden++;
                    }
                }
            }
            return ro;
        }

        
    }
}
