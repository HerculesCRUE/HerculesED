using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Models;
using ResearchobjectOntology;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class Utility
    {
        private static ResourceApi mResourceApi = Carga.mResourceApi;

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

        #region ModificarDocumentos
        /// <summary>
        /// Insertamos/eliminamos en los CV las publicaciones de las que el dueño del CV es autor con la privacidad correspondiente
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pDocuments">IDs del documento</param>
        /// <param name="pCVs">IDs del CV</param>
        public static void ModificarDocumentos(List<string> pPersons = null, List<string> pDocuments = null, List<string> pCVs = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                foreach (List<string> lista in SplitList(pPersons, 500))
                {
                    filters.Add($" FILTER(?person in (<{string.Join(">,<", lista)}>))");
                }
            }
            else if (pDocuments != null && pDocuments.Count > 0)
            {
                foreach (List<string> lista in SplitList(pDocuments, 500))
                {
                    filters.Add($" FILTER(?document in (<{string.Join(">,<", lista)}>))");
                }
            }
            else if (pCVs != null && pCVs.Count > 0)
            {
                foreach (List<string> lista in SplitList(pCVs, 500))
                {
                    filters.Add($" FILTER(?cv in (<{string.Join(">,<", lista)}>))");
                }
            }
            else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Añadimos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificActivity ?document ?isValidated ?typeDocument  from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificactivitydocument.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?scientificActivity ?document ?isValidated ?typeDocument
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?document <http://w3id.org/roh/scientificActivityDocument> ?scientificActivityDocument.
                                            OPTIONAL{{?document <http://w3id.org/roh/isValidated> ?isValidated.}}
                                            ?scientificActivityDocument <http://purl.org/dc/elements/1.1/identifier> ?typeDocument.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/scientificPublications> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD1"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedConferences> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD2"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedSeminars> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD3"" as ?typeDocument)
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarDocumentosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificActivity ?item ?typeDocument from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificactivitydocument.owl>  ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/scientificPublications> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD1"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedConferences> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD2"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedSeminars> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD3"" as ?typeDocument)
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?scientificActivity ?document ?typeDocument
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?document <http://w3id.org/roh/scientificActivityDocument> ?scientificActivityDocument.
                                            ?scientificActivityDocument <http://purl.org/dc/elements/1.1/identifier> ?typeDocument.
                                        }}                                        
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarDocumentosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos duplicados
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv (group_concat(?item;separator="";"") as ?items) count(?item) as ?numItems  ?document from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        ?scientificActivity ?p ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                    }}
                                }}}}GROUP BY ?cv ?document HAVING (?numItems > 1)  order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarDocumentosDuplicadosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Inserta documentos en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private static void InsertarDocumentosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificActivity = fila["scientificActivity"].value;
                string document = fila["document"].value;
                string typeDocument = fila["typeDocument"].value;
                string isValidated = "false";
                if (fila.ContainsKey("isValidated"))
                {
                    isValidated = fila["isValidated"].value;
                }

                string rdftype = "";
                string property = "";
                switch (typeDocument)
                {
                    case "SAD1":
                        rdftype = "http://w3id.org/roh/RelatedScientificPublication";
                        property = "http://w3id.org/roh/scientificPublications";
                        break;
                    case "SAD2":
                        rdftype = "http://w3id.org/roh/RelatedWorkSubmittedConferences";
                        property = "http://w3id.org/roh/worksSubmittedConferences";
                        break;
                    case "SAD3":
                        rdftype = "http://w3id.org/roh/RelatedWorkSubmittedSeminars";
                        property = "http://w3id.org/roh/worksSubmittedSeminars";
                        break;
                }

                //Obtenemos la auxiliar en la que cargar la entidad  
                string rdfTypePrefix = AniadirPrefijo(rdftype);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = mResourceApi.GraphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = scientificActivity + "|" + idNewAux;

                //Privacidad            
                string predicadoPrivacidad = "http://w3id.org/roh/scientificActivity|" + property + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(idEntityAux + "|" + isValidated, predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = "http://w3id.org/roh/scientificActivity|" + property + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new(idEntityAux + "|" + document, predicadoEntidad);
                listaTriples.Add(tr1);

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToInclude.ContainsKey(idCV))
                {
                    triplesToInclude[idCV].AddRange(listaTriples);
                }
                else
                {
                    triplesToInclude.Add(mResourceApi.GetShortGuid(cv), listaTriples);
                }
            }

            Parallel.ForEach(triplesToInclude.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idCV =>
            {
                List<List<TriplesToInclude>> listasDeListas = SplitList(triplesToInclude[idCV], 50).ToList();
                foreach (List<TriplesToInclude> triples in listasDeListas)
                {
                    mResourceApi.InsertPropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Elimina documentos duplicados en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private static void EliminarDocumentosDuplicadosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                List<string> items = fila["items"].value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                items.RemoveAt(0);

                String select = @"  select distinct ?cv ?scientificActivity ?prop ?item from <http://gnoss.com/document.owl>";
                String where = @$"  where                                 
                                    {{
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        ?scientificActivity ?prop ?item.
                                        FILTER(?item in (<{string.Join(">,<", items)}>))                                 
                                        FILTER(?cv = <{cv}>)                                 
                                    }}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                foreach (Dictionary<string, SparqlObject.Data> filaIn in resultado.results.bindings)
                {
                    string scientificActivity = filaIn["scientificActivity"].value;
                    string prop = filaIn["prop"].value;
                    string item = filaIn["item"].value;
                    RemoveTriples removeTriple = new();
                    removeTriple.Predicate = "http://w3id.org/roh/scientificActivity|" + prop;
                    removeTriple.Value = scientificActivity + "|" + item;
                    Guid idCV = mResourceApi.GetShortGuid(cv);
                    if (triplesToDelete.ContainsKey(idCV))
                    {
                        triplesToDelete[idCV].Add(removeTriple);
                    }
                    else
                    {
                        triplesToDelete.Add(idCV, new() { removeTriple });
                    }
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idCV =>
            {
                List<List<RemoveTriples>> listasDeListas = SplitList(triplesToDelete[idCV], 50).ToList();
                foreach (List<RemoveTriples> triples in listasDeListas)
                {
                    mResourceApi.DeletePropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Elimina documentos en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private static void EliminarDocumentosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificActivity = fila["scientificActivity"].value;
                string item = fila["item"].value;
                string typeDocument = fila["typeDocument"].value;

                string property = "";
                switch (typeDocument)
                {
                    case "SAD1":
                        property = "http://w3id.org/roh/scientificPublications";
                        break;
                    case "SAD2":
                        property = "http://w3id.org/roh/worksSubmittedConferences";
                        break;
                    case "SAD3":
                        property = "http://w3id.org/roh/worksSubmittedSeminars";
                        break;
                }

                RemoveTriples removeTriple = new();
                removeTriple.Predicate = "http://w3id.org/roh/scientificActivity|" + property;
                removeTriple.Value = scientificActivity + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV, new() { removeTriple });
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idCV =>
            {
                List<List<RemoveTriples>> listasDeListas = SplitList(triplesToDelete[idCV], 50).ToList();
                foreach (List<RemoveTriples> triples in listasDeListas)
                {
                    mResourceApi.DeletePropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }
        #endregion

        #region ModificarReserchObject
        /// <summary>       
        /// Insertamos/eliminamos en los CV los researchobjects de las que el dueño del CV es autor con la privacidad correspondiente
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pResearchObjects">IDs del research object</param>
        /// <param name="pCVs">IDs del CV</param>
        public static void ModificarResearchObjects(List<string> pPersons = null, List<string> pResearchObjects = null, List<string> pCVs = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                foreach (List<string> lista in SplitList(pPersons, 500))
                {
                    filters.Add($" FILTER(?person in ( <{string.Join(">,<", lista)}>))");
                }
            }
            else if (pResearchObjects != null && pResearchObjects.Count > 0)
            {
                foreach (List<string> lista in SplitList(pResearchObjects, 500))
                {
                    filters.Add($" FILTER(?ro in ( <{string.Join(">,<", lista)}>))");
                }
            }
            else if (pCVs != null && pCVs.Count > 0)
            {
                foreach (List<string> lista in SplitList(pCVs, 500))
                {
                    filters.Add($" FILTER(?cv in ( <{string.Join(">,<", lista)}>))");
                }
            }
            else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Añadimos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?researchObject ?ro from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl>   ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?researchObject ?ro 
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?ro a <http://w3id.org/roh/ResearchObject>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                            ?ro <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?ro a <http://w3id.org/roh/ResearchObject>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                        ?researchObject <http://w3id.org/roh/researchObjects> ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?ro.
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarResearchObjectsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?researchObject ?item from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl>  ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?ro a <http://w3id.org/roh/ResearchObject>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                        ?researchObject <http://w3id.org/roh/researchObjects> ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?ro.
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?researchObject ?ro 
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?ro a <http://w3id.org/roh/ResearchObject>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                            ?ro <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                        }}                                      
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarResearchObjectsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos duplicados
                    int limit = 500;
                    //TODO eliminar from
                    string select = @"SELECT * WHERE{select distinct ?cv ?researchObject (group_concat(?item;separator="";"") as ?items) count(?item) as ?numItems  ?ro from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl> ";
                    string where = @$"where{{
                                    {filter}                                    
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?ro a <http://w3id.org/roh/ResearchObject>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                        ?researchObject ?p ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?ro.
                                    }}
                                }}}}GROUP BY ?cv ?researchObject ?ro HAVING (?numItems > 1)  order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarResearchObjectsDuplicadosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///  Elimina ResearchObjects duplicados en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private static void EliminarResearchObjectsDuplicadosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string ResearchObjects = fila["researchObject"].value;
                List<string> items = fila["items"].value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                items.RemoveAt(0);
                foreach (string item in items)
                {
                    RemoveTriples removeTriple = new();
                    removeTriple.Predicate = "http://w3id.org/roh/researchObject|http://w3id.org/roh/researchObjects";
                    removeTriple.Value = ResearchObjects + "|" + item;
                    Guid idCV = mResourceApi.GetShortGuid(cv);
                    if (triplesToDelete.ContainsKey(idCV))
                    {
                        triplesToDelete[idCV].Add(removeTriple);
                    }
                    else
                    {
                        triplesToDelete.Add(idCV, new() { removeTriple });
                    }
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idCV =>
            {
                List<List<RemoveTriples>> listasDeListas = SplitList(triplesToDelete[idCV], 50).ToList();
                foreach (List<RemoveTriples> triples in listasDeListas)
                {
                    mResourceApi.DeletePropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Elimina ResearchObjects en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private static void EliminarResearchObjectsCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string researchObject = fila["researchObject"].value;
                string item = fila["item"].value;

                string property = "http://w3id.org/roh/researchObjects";
                RemoveTriples removeTriple = new();
                removeTriple.Predicate = "http://w3id.org/roh/researchObject|" + property;
                removeTriple.Value = researchObject + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV, new() { removeTriple });
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idCV =>
            {
                List<List<RemoveTriples>> listasDeListas = SplitList(triplesToDelete[idCV], 50).ToList();
                foreach (List<RemoveTriples> triples in listasDeListas)
                {
                    mResourceApi.DeletePropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Inserta ResearchObjects en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private static void InsertarResearchObjectsCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string researchObject = fila["researchObject"].value;
                string ro = fila["ro"].value;

                string rdftype = "http://w3id.org/roh/RelatedResearchObject";
                string property = "http://w3id.org/roh/researchObjects";

                //Obtenemos la auxiliar en la que cargar la entidad  
                string rdfTypePrefix = AniadirPrefijo(rdftype);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = mResourceApi.GraphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = researchObject + "|" + idNewAux;

                //Privacidad            
                string predicadoPrivacidad = "http://w3id.org/roh/researchObject|" + property + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(idEntityAux + "|true", predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = "http://w3id.org/roh/researchObject|" + property + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new(idEntityAux + "|" + ro, predicadoEntidad);
                listaTriples.Add(tr1);

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToInclude.ContainsKey(idCV))
                {
                    triplesToInclude[idCV].AddRange(listaTriples);
                }
                else
                {
                    triplesToInclude.Add(mResourceApi.GetShortGuid(cv), listaTriples);
                }
            }

            Parallel.ForEach(triplesToInclude.Keys, new ParallelOptions { MaxDegreeOfParallelism = 5 }, idCV =>
            {
                List<List<TriplesToInclude>> listasDeListas = SplitList(triplesToInclude[idCV], 50).ToList();
                foreach (List<TriplesToInclude> triples in listasDeListas)
                {
                    mResourceApi.InsertPropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }
        #endregion
                
        public static string IdentificadorFECYT(string tipoDocumento)
        {
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD1")) { 
                return "060.010.010.000"; 
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD2")) { 
                return "060.010.020.000"; 
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD3")) { 
                return "060.010.030.000"; 
            }
            return null;
        }
    }
}
