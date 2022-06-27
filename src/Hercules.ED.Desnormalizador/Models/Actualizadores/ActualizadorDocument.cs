using Es.Riam.Gnoss.Web.MVC.Models.IntegracionContinua;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DesnormalizadorHercules.Models.Actualizadores
{
    //TODO comentarios completados, falta eliminar froms

    /// <summary>
    /// Clase para actualizar propiedades de documentos
    /// </summary>
    class ActualizadorDocument : ActualizadorBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pResourceApi">API Wrapper de GNOSS</param>
        public ActualizadorDocument(ResourceApi pResourceApi) : base(pResourceApi)
        {
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/isValidated de los http://purl.org/ontology/bibo/Document
        /// los documentos validados (son los documentos oficiales, es decir, los que tienen http://w3id.org/roh/crisIdentifier o validados por la universidad)
        /// Esta propiedad se utilizará como filtro en el bucador general de publicaciones
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocuments">ID de documentos</param>
        public void ActualizarDocumentosValidados(List<string> pDocuments = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }
            //Eliminamos los duplicados
            EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/isValidated");

            foreach (string filter in filters)
            {
                while (true)
                {
                    int limit = 500;
                    String select = @"select ?document ?isValidatedCargado ?isValidatedCargar";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                {filter}
                                MINUS
                                {{
                                    ?document <http://w3id.org/roh/isValidated> 'true'.
                                }}
                                OPTIONAL
                                {{
                                    ?document <http://w3id.org/roh/isValidated> ?isValidatedCargado.
                                }}
                                {{
                                    select distinct ?document IF(BOUND(?isValidatedCargar),?isValidatedCargar,'false')  as ?isValidatedCargar
                                    Where
                                    {{
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        OPTIONAL
                                        {{
                                            ?document <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            BIND('true' as ?isValidatedCargar)
                                        }}
                                        OPTIONAL
                                        {{
                                            ?document <http://w3id.org/roh/validationStatusPRC> 'validado'.
                                            BIND('true' as ?isValidatedCargar)
                                        }} 
                                    }}
                                }}
                                FILTER(?isValidatedCargado!= ?isValidatedCargar OR !BOUND(?isValidatedCargado) )
                            }} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string document = fila["document"].value;
                        string isValidatedCargar = fila["isValidatedCargar"].value;
                        string isValidatedCargado = "";
                        if (fila.ContainsKey("isValidatedCargado"))
                        {
                            isValidatedCargado = fila["isValidatedCargado"].value;
                        }
                        ActualizadorTriple(document, "http://w3id.org/roh/isValidated", isValidatedCargado, isValidatedCargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/isProducedBy de los http://purl.org/ontology/bibo/Document 
        /// los grupos a los que pertenecen los autores de los documentos oficiales (http://w3id.org/roh/isValidated) en el momento de la publicación del documento
        /// Esta propiedad se utilizará para mostrar en las fichas de los grupos el listado de sus publicaciones oficiales
        /// No tiene dependencias
        /// </summary>
        /// <param name="pGroups">IDs de los grupos</param>
        /// <param name="pDocuments">IDs de los documentos</param>
        public void ActualizarPertenenciaGrupos(List<string> pGroups = null, List<string> pDocuments = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pGroups != null && pGroups.Count > 0)
            {
                filters.Add($" FILTER(?grupo in (<{string.Join(">,<", pGroups)}>))");
            }
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?doc in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Añadimos a documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"select distinct ?doc ?grupo  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl> from <http://gnoss.com/group.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?grupo ?doc
                                        Where{{
                                            ?grupo a <http://xmlns.com/foaf/0.1/Group>.
                                            ?grupo <http://vivoweb.org/ontology/core#relates> ?members.
                                            ?members <http://w3id.org/roh/roleOf> ?person.
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            OPTIONAL{{?members <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                            OPTIONAL{{?members <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}
                                            BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?fechaPersonaEndAux)
                                            BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?fechaPersonaInitAux)
                                            ?doc a <http://purl.org/ontology/bibo/Document>.
                                            ?doc <http://w3id.org/roh/isValidated> 'true'.
                                            ?doc <http://purl.org/ontology/bibo/authorList> ?autores.
                                            ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?doc <http://purl.org/dc/terms/issued> ?fechaPublicacion.
                                            FILTER(xsd:integer(?fechaPublicacion)>= ?fechaPersonaInitAux AND xsd:integer(?fechaPublicacion)<= ?fechaPersonaEndAux)
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?grupo a <http://xmlns.com/foaf/0.1/Group>.
                                        ?doc a <http://purl.org/ontology/bibo/Document>.
                                        ?doc <http://w3id.org/roh/isProducedBy> ?grupo.
                                    }}
                                }}order by desc(?doc) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                    InsercionMultiple(resultado.results.bindings, "http://w3id.org/roh/isProducedBy", "doc", "grupo");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Eliminamos de dcumentpos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"select distinct ?doc ?grupo  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl> from <http://gnoss.com/group.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        ?grupo a <http://xmlns.com/foaf/0.1/Group>.
                                        ?doc a <http://purl.org/ontology/bibo/Document>.
                                        ?doc <http://w3id.org/roh/isProducedBy> ?grupo.                                                                   
                                    }}
                                    MINUS
                                    {{
                                        select distinct ?grupo ?doc
                                        Where{{
                                            ?grupo a <http://xmlns.com/foaf/0.1/Group>.
                                            ?grupo <http://vivoweb.org/ontology/core#relates> ?members.
                                            ?members <http://w3id.org/roh/roleOf> ?person.
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            OPTIONAL{{?members <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                            OPTIONAL{{?members <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}
                                            BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?fechaPersonaEndAux)
                                            BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?fechaPersonaInitAux)
                                            ?doc a <http://purl.org/ontology/bibo/Document>.
                                            ?doc <http://w3id.org/roh/isValidated> 'true'.
                                            ?doc <http://purl.org/ontology/bibo/authorList> ?autores.
                                            ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?doc <http://purl.org/dc/terms/issued> ?fechaPublicacion.
                                            FILTER(xsd:integer(?fechaPublicacion)>= ?fechaPersonaInitAux AND xsd:integer(?fechaPublicacion)<= ?fechaPersonaEndAux)
                                        }}
                                    }}
                                }}order by desc(?doc) limit {limit}";
                    var resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                    EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/isProducedBy", "doc", "grupo");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

            }
        }


        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/citationCount de los http://purl.org/ontology/bibo/Document
        /// el nº máximo de citas con el que cuenta
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocuments">IDs de documentos</param>
        public void ActualizarNumeroCitasMaximas(List<string> pDocuments = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            //Eliminamos los duplicados
            EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/citationCount");

            foreach (string filter in filters)
            {
                //Actualizamos los datos
                while (true)
                {
                    int limit = 500;
                    String select = @"select ?document ?numCitasCargadas IF (BOUND (?numCitasACargar), ?numCitasACargar, 0 ) as ?numCitasACargar";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                {filter}
                                OPTIONAL
                                {{
                                  ?document <http://w3id.org/roh/citationCount> ?numCitasCargadasAux. 
                                  BIND(xsd:int( ?numCitasCargadasAux) as  ?numCitasCargadas)
                                }}
                                {{
                                  select ?document max(xsd:int( ?numCitas)) as ?numCitasACargar
                                  Where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    OPTIONAL{{
                                        {{
                                            ?document <http://w3id.org/roh/hasMetric> ?metric.
                                            ?metric  <http://w3id.org/roh/citationCount> ?numCitas.
                                        }}UNION
                                        {{
                                            ?document <http://w3id.org/roh/wosCitationCount> ?numCitas.
                                        }}
                                        UNION{{
                                            ?document <http://w3id.org/roh/inrecsCitationCount> ?numCitas.
                                        }}
                                        UNION{{
                                            ?document <http://w3id.org/roh/scopusCitationCount> ?numCitas.
                                        }}
                                        UNION{{
                                            ?document <http://w3id.org/roh/semanticScholarCitationCount> ?numCitas.
                                        }}
                                    }}                                
                                  }}
                                }}
                                FILTER(?numCitasCargadas!= ?numCitasACargar OR !BOUND(?numCitasCargadas) )
                            }} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string document = fila["document"].value;
                        string numCitasACargar = fila["numCitasACargar"].value;
                        string numCitasCargadas = "";
                        if (fila.ContainsKey("numCitasCargadas"))
                        {
                            numCitasCargadas = fila["numCitasCargadas"].value;
                        }
                        ActualizadorTriple(document, "http://w3id.org/roh/citationCount", numCitasCargadas, numCitasACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/hasKnowledgeArea de los http://purl.org/ontology/bibo/Document 
        /// las áreas del documento (obtenido de varias propiedades en las que están las áreas en función de su origen)
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocuments">ID de documentos</param>
        public void ActualizarAreasDocumentos(List<string> pDocuments = null)
        {
            //Categorías
            //unificada-->http://w3id.org/roh/hasKnowledgeArea
            //usuario-->http://w3id.org/roh/userKnowledgeArea
            //external-->http://w3id.org/roh/externalKnowledgeArea
            //enriched-->http://w3id.org/roh/enrichedKnowledgeArea

            HashSet<string> filters = new HashSet<string>();
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                //Eliminamos las categorías duplicadas
                while (true)
                {
                    int limit = 500;
                    //TODO from
                    String select = @"select ?document ?categoryNode from <http://gnoss.com/taxonomy.owl> ";
                    String where = @$"where{{
                                select distinct ?document ?hasKnowledgeAreaAux  ?categoryNode
                                where{{
                                    {filter}
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    ?document  <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeAreaAux.
                                    ?hasKnowledgeAreaAux <http://w3id.org/roh/categoryNode> ?categoryNode.
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                               }}
                            }}group by ?document ?categoryNode HAVING (COUNT(*) > 1) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string document = fila["document"].value;
                        string categoryNode = fila["categoryNode"].value;
                        //TODO from
                        select = @"select ?document ?hasKnowledgeArea   ?categoryNode from <http://gnoss.com/taxonomy.owl>";
                        where = @$"where{{
                                    FILTER(?document=<{document}>)
                                    FILTER(?categoryNode =<{categoryNode}>)
                                    {{ 
                                        select distinct ?document ?hasKnowledgeArea  ?categoryNode
                                        where{{
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?document  <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeArea.
                                            ?hasKnowledgeArea <http://w3id.org/roh/categoryNode> ?categoryNode.
                                            MINUS{{
                                                ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                            }}
                                        }}
                                    }}
                                }}";
                        resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                        List<RemoveTriples> triplesRemove = new();
                        foreach (string hasKnowledgeArea in resultado.results.bindings.GetRange(1, resultado.results.bindings.Count - 1).Select(x => x["hasKnowledgeArea"].value).ToList())
                        {
                            triplesRemove.Add(new RemoveTriples()
                            {
                                Predicate = "http://w3id.org/roh/hasKnowledgeArea",
                                Value = hasKnowledgeArea
                            }); ;
                        }
                        if (triplesRemove.Count > 0)
                        {
                            var resultadox = mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { mResourceApi.GetShortGuid(document), triplesRemove } });
                        }
                    });


                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }



                //Cargamos el tesauro
                Dictionary<string, string> dicAreasBroader = new();
                {
                    String select = @"select distinct * ";
                    String where = @$"where{{
                                ?concept a <http://www.w3.org/2008/05/skos#Concept>.
                                ?concept <http://purl.org/dc/elements/1.1/source> 'researcharea'
                                OPTIONAL{{?concept <http://www.w3.org/2008/05/skos#broader> ?broader}}
                            }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "taxonomy");

                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string concept = fila["concept"].value;
                        string broader = "";
                        if (fila.ContainsKey("broader"))
                        {
                            broader = fila["broader"].value;
                        }
                        dicAreasBroader.Add(concept, broader);
                    }
                }


                while (true)
                {
                    int limit = 500;
                    //INSERTAMOS
                    //TODO eliminar from
                    String select = @"select ?document ?categoryNode from <http://gnoss.com/taxonomy.owl>";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                {filter}
                                {{
                                    select  distinct ?document ?hasKnowledgeAreaDocument ?categoryNode where{{
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document ?props ?hasKnowledgeAreaDocument.
                                        FILTER(?props in (<http://w3id.org/roh/userKnowledgeArea>,<http://w3id.org/roh/externalKnowledgeArea>,<http://w3id.org/roh/enrichedKnowledgeArea>))
                                        ?hasKnowledgeAreaDocument <http://w3id.org/roh/categoryNode> ?categoryNode.
                                        MINUS{{
                                            ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                        }}
                                    }}
                                }}
                                MINUS{{
                                    select distinct ?document ?hasKnowledgeAreaDocumentAux ?categoryNode 
                                    where{{
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeAreaDocumentAux.
                                        ?hasKnowledgeAreaDocumentAux <http://w3id.org/roh/categoryNode> ?categoryNode
                                        MINUS{{
                                            ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                        }}
                                    }}
                                }}
                            }}order by (?document) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                    InsertarCategorias(resultado, dicAreasBroader, mResourceApi.GraphsUrl, "document", "http://w3id.org/roh/hasKnowledgeArea");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    int limit = 500;
                    //ELIMINAMOS
                    //TODO eliminar from
                    String select = @"select ?document ?hasKnowledgeArea from <http://gnoss.com/taxonomy.owl>";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                {filter}
                                {{
                                    select distinct ?document ?hasKnowledgeArea ?categoryNode 
                                    where{{
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeArea.
                                        ?hasKnowledgeArea <http://w3id.org/roh/categoryNode> ?categoryNode
                                        MINUS{{
                                            ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                        }}
                                    }}                               
                                }}
                                MINUS{{
                                    select  distinct ?document ?hasKnowledgeAreaDocument ?categoryNode where{{
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document ?props ?hasKnowledgeAreaDocument.
                                        FILTER(?props in (<http://w3id.org/roh/userKnowledgeArea>,<http://w3id.org/roh/externalKnowledgeArea>,<http://w3id.org/roh/enrichedKnowledgeArea>))
                                        ?hasKnowledgeAreaDocument <http://w3id.org/roh/categoryNode> ?categoryNode.
                                        MINUS{{
                                            ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                        }}
                                    }}
                                 
                                }}
                            }} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                    EliminarCategorias(resultado, "document", "http://w3id.org/roh/hasKnowledgeArea");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://vivoweb.org/ontology/core#freeTextKeyword de los http://purl.org/ontology/bibo/Document 
        /// los tags (obtenido de varias propiedades en las que están los tags en función de su origen)
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocuments">ID de documentos</param>
        public void ActualizarTagsDocumentos(List<string> pDocuments = null)
        {
            //Etiquetas
            //unificada-->http://vivoweb.org/ontology/core#freeTextKeyword
            //usuario-->http://w3id.org/roh/userKeywords
            //external-->http://w3id.org/roh/externalKeywords
            //enriched-->http://w3id.org/roh/enrichedKeywords@@@http://w3id.org/roh/title

            HashSet<string> filters = new HashSet<string>();
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {
                while (true)
                {
                    int limit = 500;
                    //INSERTAMOS
                    String select = @"select ?document ?tag";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                {filter}
                                {{
                                    {{
                                        select  distinct ?document ?tag where{{
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?document <http://w3id.org/roh/enrichedKeywords> ?aux.
                                            ?aux <http://w3id.org/roh/title> ?tag.                                       
                                        }}
                                    }}
                                    UNION
                                    {{
                                        select  distinct ?document ?tag where{{
                                            ?document ?props ?tag.
                                            FILTER(?props in (<http://w3id.org/roh/userKeywords>,<http://w3id.org/roh/externalKeywords>))
                                        }}
                                    }}
                                }}
                                MINUS{{
                                    select distinct ?document ?tag
                                    where{{
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document <http://vivoweb.org/ontology/core#freeTextKeyword> ?tagAux.
                                        ?tagAux <http://w3id.org/roh/title> ?tag.
                                    }}
                                }}
                            }}order by (?document) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                    InsercionMultipleTags(resultado.results.bindings, "document", "tag");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    int limit = 500;
                    //ELIMINAMOS
                    String select = @"select ?document ?tagAux";
                    String where = @$"where{{
                            ?document a <http://purl.org/ontology/bibo/Document>.
                                {filter}
                                {{
                                    select distinct ?document ?tagAux ?tag
                                    where{{
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document <http://vivoweb.org/ontology/core#freeTextKeyword> ?tagAux.
                                        ?tagAux <http://w3id.org/roh/title> ?tag.
                                    }}                               
                                }}
                                MINUS{{
                                    {{
                                        select  distinct ?document ?tag where{{
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?document <http://w3id.org/roh/enrichedKeywords> ?aux.
                                            ?aux <http://w3id.org/roh/title> ?tag.                                       
                                        }}
                                    }}
                                    UNION
                                    {{
                                        select  distinct ?document ?tag where{{
                                            ?document ?props ?tag.
                                            FILTER(?props in (<http://w3id.org/roh/userKeywords>,<http://w3id.org/roh/externalKeywords>))
                                        }}
                                    }}
                                }}
                            }} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                    EliminacionMultiple(resultado.results.bindings, "http://vivoweb.org/ontology/core#freeTextKeyword", "document", "tagAux");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                //Eliminar duplicados
                while (true)
                {
                    int limit = 500;
                    String select = @"select ?document count(?tag) ?tag ";
                    String where = @$"where
                                {{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    {filter}
                                    ?document <http://vivoweb.org/ontology/core#freeTextKeyword> ?freeTextKeyword. 
                                    ?freeTextKeyword <http://w3id.org/roh/title> ?tag. 
                                }}group by (?document) (?tag) HAVING (COUNT(?tag) > 1) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string document = fila["document"].value;
                        string tag = fila["tag"].value;
                        String select2 = @"select ?document ?data ";
                        String where2 = @$"where
                            {{                            
                                ?document <http://vivoweb.org/ontology/core#freeTextKeyword> ?data. 
                                ?data <http://w3id.org/roh/title> '{tag.Replace("'", "\\'")}'. 
                                FILTER(?document=<{document}>)
                            }}";
                        SparqlObject resultado2 = mResourceApi.VirtuosoQuery(select2, where2, "document");

                        foreach (Dictionary<string, SparqlObject.Data> fila2 in resultado2.results.bindings.GetRange(1, resultado2.results.bindings.Count - 1))
                        {
                            string value = fila2["data"].value;
                            ActualizadorTriple(document, "http://vivoweb.org/ontology/core#freeTextKeyword", value, "");
                        }
                    });
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/year de los http://purl.org/ontology/bibo/Document públicos 
        /// el año de publicación
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocuments">ID de documentos</param>
        public void ActualizarAnios(List<string> pDocuments = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/year");
            foreach (string filter in filters)
            {
                //Inserciones
                while (true)
                {
                    int limit = 500;

                    String select = @"select distinct * where{select ?document ?yearCargado ?yearCargar  ";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                {filter}
                                OPTIONAL{{
	                                ?document <http://purl.org/dc/terms/issued> ?fechaDoc.
	                                BIND(substr(str(?fechaDoc),0,4) as ?yearCargar).
                                }}
                                OPTIONAL{{
                                    ?document <http://w3id.org/roh/year> ?yearCargado.      
                                }}
                                
                                FILTER(?yearCargado!= ?yearCargar)

                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string document = fila["document"].value;
                        string yearCargar = "";
                        if (fila.ContainsKey("yearCargar"))
                        {
                            yearCargar = fila["yearCargar"].value;
                        }
                        string yearCargado = "";
                        if (fila.ContainsKey("yearCargado"))
                        {
                            yearCargado = fila["yearCargado"].value;
                        }
                        ActualizadorTriple(document, "http://w3id.org/roh/year", yearCargado, yearCargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/genderIP de los http://purl.org/ontology/bibo/Document 
        /// el gender del primer autor (si esta validado) y '-' si el primer autor no está validado o está validado pero no tiene gender
        /// No tiene dependencias
        /// <param name="pDocuments">ID de documentos</param>
        public void ActualizarGenderAutorPrincipal(List<string> pDocuments = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            //Eliminamos los duplicados
            EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/genderIP");

            foreach (string filter in filters)
            {
                while (true)
                {
                    int limit = 500;
                    String select = @"select ?document ?genderIPCargado ?genderIPACargar from <http://gnoss.com/person.owl>";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                {filter}
                                OPTIONAL
                                {{
                                  ?document <http://w3id.org/roh/genderIP> ?genderIPCargado.
                                }}
                                OPTIONAL{{
                                  select ?document ?genderIPACargar
                                  Where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.                                    
                                    ?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                    ?person <http://xmlns.com/foaf/0.1/gender> ?genderIPACargar.
                                    ?person <http://w3id.org/roh/isActive>  'true'.
                                    {{
		                                select ?document  min(?orden) as ?orden sample(?author) as ?author where
		                                {{
			                                select ?document  ?orden ?author where
			                                {{
				                                ?document a <http://purl.org/ontology/bibo/Document>.
				                                ?document <http://purl.org/ontology/bibo/authorList> ?author.
				                                ?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#comment> ?ordenAux.
				                                BIND(xsd:int(?ordenAux) as ?orden)
			                                }}order by asc(?orden) 
		                                }}group by (?document)
	                                }}  
                                  }}
                                }}
                                
                                FILTER(
                                    #Hay que cambiarlo
                                    (BOUND(?genderIPCargado) AND BOUND(?genderIPACargar) AND ?genderIPCargado!= ?genderIPACargar) OR
                                    #Hay que insertarlo
                                    (!BOUND(?genderIPCargado) AND BOUND(?genderIPACargar)) OR
                                    #Hay que eliminarlo
                                    (BOUND(?genderIPCargado) AND !BOUND(?genderIPACargar)) 
                                    )
                            }} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string document = fila["document"].value;
                        string genderIPACargar = "";
                        if (fila.ContainsKey("genderIPACargar"))
                        {
                            genderIPACargar = fila["genderIPACargar"].value;
                        }
                        string genderIPCargado = "";
                        if (fila.ContainsKey("genderIPCargado"))
                        {
                            genderIPCargado = fila["genderIPCargado"].value;
                        }
                        ActualizadorTriple(document, "http://w3id.org/roh/genderIP", genderIPCargado, genderIPACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/positionIP de los http://purl.org/ontology/bibo/Document 
        /// el position del primer autor (si esta validado) y '-' si el primer autor no está validado o está validado pero no tiene position
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocuments">ID de documentos</param>
        public void ActualizarPositionAutorPrincipal(List<string> pDocuments = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            //Eliminamos los duplicados
            EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/positionIP");

            foreach (string filter in filters)
            {
                //Actualizamos los datos
                while (true)
                {
                    int limit = 500;
                    String select = @"select ?document ?positionIPCargado ?positionIPACargar from <http://gnoss.com/person.owl>";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                {filter}
                                OPTIONAL
                                {{
                                  ?document <http://w3id.org/roh/positionIP> ?positionIPCargado.
                                }}
                                OPTIONAL{{
                                  select ?document IF (BOUND (?positionIPACargar), ?positionIPACargar, '-' ) as ?positionIPACargar
                                  Where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    OPTIONAL{{
                                        ?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                        ?person <http://w3id.org/roh/hasPosition> ?positionIPACargar.
                                        ?person <http://w3id.org/roh/isActive>  'true'.
                                        {{
		                                    select ?document  min(?orden) as ?orden sample(?author) as ?author where
		                                    {{
			                                    select ?document  ?orden ?author where
			                                    {{
				                                    ?document a <http://purl.org/ontology/bibo/Document>.
				                                    ?document <http://purl.org/ontology/bibo/authorList> ?author.
				                                    ?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#comment> ?ordenAux.
				                                    BIND(xsd:int(?ordenAux) as ?orden)
			                                    }}order by asc(?orden) 
		                                    }}group by (?document)
	                                    }}                                    
                                    }}

                                  }}
                                }}
                                FILTER(?positionIPCargado!= ?positionIPACargar OR !BOUND(?positionIPCargado) )
                            }} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string document = fila["document"].value;
                        string positionIPACargar = fila["positionIPACargar"].value;
                        string positionIPCargado = "";
                        if (fila.ContainsKey("positionIPCargado"))
                        {
                            positionIPCargado = fila["positionIPCargado"].value;
                        }
                        ActualizadorTriple(document, "http://w3id.org/roh/positionIP", positionIPCargado, positionIPACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// TODO hacer bien, añadir cuartiles
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocuments"></param>
        public void ActualizarIndicesImpacto(List<string> pDocuments = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                //Inserciones
                while (true)
                {
                    int limit = 500;

                    String select = @"select distinct * where{select ?document ?impactIndexInYear ?impactSource ?impactSourceOther from <http://gnoss.com/maindocument.owl> ";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.                               
                                {filter}
                                {{
                                    #Deseables
                                    ?document <http://w3id.org/roh/isValidated>  'true'.
	                                ?document <http://purl.org/dc/terms/issued> ?fechaDoc.
	                                ?document<http://w3id.org/roh/year> ?anioDoc.
	                                ?document <http://vivoweb.org/ontology/core#hasPublicationVenue> ?revista. 
	                                ?revista a <http://w3id.org/roh/MainDocument>.
	                                ?revista <http://w3id.org/roh/impactIndex> ?impactIndex.
	                                ?impactIndex <http://w3id.org/roh/impactIndexInYear> ?impactIndexInYear.
	                                ?impactIndex <http://w3id.org/roh/year> ?anioFactorImpactoRevista.                          
	                                FILTER(?anioDoc=?anioFactorImpactoRevista)
                                    OPTIONAL{{?impactIndex  <http://w3id.org/roh/impactSource> ?impactSource. }}
                                    OPTIONAL{{?impactIndex  <http://w3id.org/roh/impactSourceOther> ?impactSourceOther. }}  
                                }}MINUS
                                {{
                                    #Actuales
                                    ?document <http://w3id.org/roh/impactIndex> ?impactIndexDoc.
                                    ?impactIndexDoc <http://w3id.org/roh/impactIndexInYear> ?impactIndexInYear.
                                    OPTIONAL{{?impactIndexDoc  <http://w3id.org/roh/impactSource> ?impactSource. }}
                                    OPTIONAL{{?impactIndexDoc  <http://w3id.org/roh/impactSourceOther> ?impactSourceOther. }}  	                                
                                }}
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                    InsercionIndicesImpacto(resultado.results.bindings);

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                //Eliminaciones
                while (true)
                {
                    int limit = 500;

                    String select = @"select distinct * where{select ?document ?impactIndexDoc from <http://gnoss.com/maindocument.owl> ";
                    String where = @$"where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                ?document <http://w3id.org/roh/isValidated>  'true'.
                                {filter}
                                {{
                                    #Actuales
                                    ?document <http://w3id.org/roh/impactIndex> ?impactIndexDoc.
                                    ?impactIndexDoc <http://w3id.org/roh/impactIndexInYear> ?impactIndexInYear.
                                    OPTIONAL{{?impactIndexDoc  <http://w3id.org/roh/impactSource> ?impactSource. }}
                                    OPTIONAL{{?impactIndexDoc  <http://w3id.org/roh/impactSourceOther> ?impactSourceOther. }}  
                                }}MINUS
                                {{
                                    #Deseables
                                    ?document <http://w3id.org/roh/isValidated>  'true'.
	                                ?document <http://purl.org/dc/terms/issued> ?fechaDoc.
	                                ?document<http://w3id.org/roh/year> ?anioDoc.
	                                ?document <http://vivoweb.org/ontology/core#hasPublicationVenue> ?revista. 
	                                ?revista a <http://w3id.org/roh/MainDocument>.
	                                ?revista <http://w3id.org/roh/impactIndex> ?impactIndex.
	                                ?impactIndex <http://w3id.org/roh/impactIndexInYear> ?impactIndexInYear.
	                                ?impactIndex <http://w3id.org/roh/year> ?anioFactorImpactoRevista.                  
	                                FILTER(?anioDoc=?anioFactorImpactoRevista)
	                                OPTIONAL{{?impactIndex  <http://w3id.org/roh/impactSource> ?impactSource. }}
                                    OPTIONAL{{?impactIndex  <http://w3id.org/roh/impactSourceOther> ?impactSourceOther. }}  
                                }}
                            }}}} limit {limit}";

                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string document = fila["document"].value;
                        string indiceImpactoAEliminar = fila["impactIndexDoc"].value;
                        ActualizadorTriple(document, "http://w3id.org/roh/impactIndex", indiceImpactoAEliminar, "");
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                ////Actualizamos publicationPosition
                //while (true)
                //{
                //    int limit = 500;

                //    String select = @"select distinct * where{select ?document ?impactIndexDoc ?cargado ?cargar from <http://gnoss.com/maindocument.owl> ";
                //    String where = @$"where{{
                //                    ?document a <http://purl.org/ontology/bibo/Document>.
                //                    {filter}

                //                    ?document <http://w3id.org/roh/impactIndex> ?impactIndexDoc.
                //                    ?document <http://w3id.org/roh/year> ?year.
                //                    ?impactIndexDoc <http://w3id.org/roh/impactIndexInYear> ?impactIndexInYear.
                //                    OPTIONAL{{?impactIndexDoc  <http://w3id.org/roh/impactSource> ?impactSource. }}
                //                    OPTIONAL{{?impactIndexDoc  <http://w3id.org/roh/impactSourceOther> ?impactSourceOther. }}  
                //              ?impactIndexDoc  <http://w3id.org/roh/impactIndexCategory> ?impactIndexCategory.
                //                    ?document <http://vivoweb.org/ontology/core#hasPublicationVenue> ?revista. 
                //                    ?revista a <http://w3id.org/roh/MainDocument>.
                //                 ?revista <http://w3id.org/roh/impactIndex> ?impactIndexRevista.
                //                    ?impactIndexRevista <http://w3id.org/roh/year> ?year.
                //                    ?impactIndexRevista  <http://w3id.org/roh/impactCategory> ?impactCategory.
                //                 ?impactCategory  <http://w3id.org/roh/impactIndexCategory> ?impactIndexCategory.   


                //                    OPTIONAL{{
                //                     ?impactIndexDoc <http://w3id.org/roh/publicationPosition> ?cargado.
                //                    }}
                //                    OPTIONAL{{
                //                        ?impactCategory <http://w3id.org/roh/publicationPosition> ?cargar.      
                //                    }}                                
                //                    FILTER(?cargado!= ?cargar)
                //                }}}} limit {limit}";

                //    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                //    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                //    {
                //        string document = fila["document"].value;
                //        string impactIndexDoc = fila["impactIndexDoc"].value;
                //        string cargar = "";
                //        if (fila.ContainsKey("cargar"))
                //        {
                //            cargar = impactIndexDoc + "|" + fila["cargar"].value;
                //        }
                //        string cargado = "";
                //        if (fila.ContainsKey("cargado"))
                //        {
                //            cargado = impactIndexDoc + "|" + fila["cargado"].value;
                //        }
                //        ActualizadorTriple(document, "http://w3id.org/roh/impactIndex|http://w3id.org/roh/publicationPosition", cargado, cargar);
                //    });

                //    if (resultado.results.bindings.Count != limit)
                //    {
                //        break;
                //    }
                //}
            }
        }

        ///// <summary>
        ///// TODO hacer bien
        ///// </summary>
        ///// <param name="pDocument"></param>
        //public void ActualizarIndiceImpacto(string pDocument = null)
        //{
        //    string filter = "";
        //    if (!string.IsNullOrEmpty(pDocument))
        //    {
        //        filter = $" FILTER(?document =<{pDocument}>)";
        //    }
        //    //Eliminamos los duplicados
        //    EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/impactIndexInYear");

        //    //Actualizamos los datos
        //    while (true)
        //    {
        //        int limit = 500;
        //        //TODO Eliminar from
        //        //TODO eliminar decha competa revista
        //        //TODO agregar la fuente para el factor de impacto
        //        String select = @"select distinct * where{select ?document ?indiceImpactoCargado ?indiceImpactoACargar  from <http://gnoss.com/maindocument.owl> ";
        //        String where = @$"where{{
        //                    ?document a <http://purl.org/ontology/bibo/Document>.
        //                    {filter}
        //                    OPTIONAL
        //                    {{
        //                      ?document <http://w3id.org/roh/impactIndexInYear> ?indiceImpactoCargado. 
        //                    }}
        //                    OPTIONAL
        //                    {{
        //                      select ?document max(?indiceImpactoACargar) as ?indiceImpactoACargar
        //                      Where{{
        //                        ?document a <http://purl.org/ontology/bibo/Document>.
        //                        ?document <http://purl.org/dc/terms/issued> ?fechaDoc.
        //                        BIND(substr(str(?fechaDoc),0,4) as ?anioDoc).
        //                        ?document <http://vivoweb.org/ontology/core#hasPublicationVenue> ?revista. 
        //                        ?revista a <http://w3id.org/roh/MainDocument>.
        //                        ?revista <http://w3id.org/roh/impactIndex> ?impactIndex.
        //                        ?impactIndex <http://w3id.org/roh/impactIndexInYear> ?indiceImpactoACargar.
        //                        ?impactIndex <http://w3id.org/roh/year> ?fechaFactorImpactoRevista.
        //                        BIND(substr(str(?fechaFactorImpactoRevista),0,4) as ?anioFactorImpactoRevista).                                
        //                        FILTER(?anioDoc=?anioFactorImpactoRevista)                               
        //                      }}
        //                    }}
        //                    FILTER(?indiceImpactoCargado!= ?indiceImpactoACargar)
        //                    }}}} limit {limit}";
        //        SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

        //        Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
        //        {
        //            string document = fila["document"].value;
        //            string indiceImpactoACargar = "";
        //            if (fila.ContainsKey("indiceImpactoACargar"))
        //            {
        //                indiceImpactoACargar = fila["indiceImpactoACargar"].value;
        //            }
        //            string indiceImpactoCargado = "";
        //            if (fila.ContainsKey("indiceImpactoCargado"))
        //            {
        //                indiceImpactoCargado = fila["indiceImpactoCargado"].value;
        //            }
        //            ActualizadorTriple(document, "http://w3id.org/roh/impactIndexInYear", indiceImpactoCargado, indiceImpactoACargar);
        //        });

        //        if (resultado.results.bindings.Count != limit)
        //        {
        //            break;
        //        }
        //    }
        //}

        ///// <summary>
        ///// TODO hacer bien
        ///// </summary>
        ///// <param name="pDocument"></param>
        //public void ActualizarCuartil(string pDocument = null)
        //{
        //    string filter = "";
        //    if (!string.IsNullOrEmpty(pDocument))
        //    {
        //        filter = $" FILTER(?document =<{pDocument}>)";
        //    }
        //    //Eliminamos los duplicados
        //    EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/quartile");

        //    //Actualizamos los datos
        //    while (true)
        //    {
        //        int limit = 500;
        //        //TODO Eliminar from
        //        String select = @"select distinct * where{select ?document ?quartileCargado ?quartileCargar  from <http://gnoss.com/maindocument.owl> ";
        //        String where = @$"where{{
        //                    ?document a <http://purl.org/ontology/bibo/Document>.
        //                    {filter}
        //                    OPTIONAL
        //                    {{
        //                      ?document <http://w3id.org/roh/quartile> ?quartileCargado. 
        //                    }}
        //                    OPTIONAL
        //                    {{
        //                      select ?document min(?quartile) as ?quartileCargar
        //                      Where{{
        //                        ?document a <http://purl.org/ontology/bibo/Document>.
        //                        ?document <http://purl.org/dc/terms/issued> ?fecha.  
        //                        BIND(substr(str(?fecha),0,4) as ?anioDoc).
        //                        ?document <http://vivoweb.org/ontology/core#hasPublicationVenue> ?revista.
        //                        ?revista <http://w3id.org/roh/impactIndex> ?indiceImpacto.
        //                        ?indiceImpacto <http://w3id.org/roh/year> ?anioIndiceImpacto.
        //                        ?indiceImpacto  <http://w3id.org/roh/impactCategory> ?categoriaIndiceImpacto.
        //                        ?categoriaIndiceImpacto <http://w3id.org/roh/quartile> ?quartile.
        //                        FILTER(?anioIndiceImpacto=?anioFecha)        
        //                      }}
        //                    }}
        //                    FILTER(?quartileCargado!= ?quartileCargar)
        //                    }}}} limit {limit}";
        //        SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

        //        Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
        //        {
        //            string document = fila["document"].value;
        //            string quartileCargar = "";
        //            if (fila.ContainsKey("quartileCargar"))
        //            {
        //                quartileCargar = fila["quartileCargar"].value;
        //            }
        //            string quartileCargado = "";
        //            if (fila.ContainsKey("quartileCargado"))
        //            {
        //                quartileCargado = fila["quartileCargado"].value;
        //            }
        //            ActualizadorTriple(document, "http://w3id.org/roh/quartile", quartileCargado, quartileCargar);
        //        });

        //        if (resultado.results.bindings.Count != limit)
        //        {
        //            break;
        //        }
        //    }
        //}

        /// <summary>
        /// Método para inserción múltiple de indices de impacto
        /// </summary>
        /// <param name="pFilas">Filas con los datos para insertar</param>
        public void InsercionIndicesImpacto(List<Dictionary<string, SparqlObject.Data>> pFilas)
        {
            List<string> ids = pFilas.Select(x => x["document"].value).Distinct().ToList();
            if (ids.Count > 0)
            {
                Parallel.ForEach(ids, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, id =>
                {
                    Guid guid = mResourceApi.GetShortGuid(id);

                    Dictionary<Guid, List<TriplesToInclude>> triples = new() { { guid, new List<TriplesToInclude>() } };
                    foreach (Dictionary<string, SparqlObject.Data> fila in pFilas.Where(x => x["document"].value == id))
                    {
                        string idAux = mResourceApi.GraphsUrl + "items/ImpactIndex_" + guid.ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                        string document = fila["document"].value;
                        {
                            TriplesToInclude t = new();
                            t.Predicate = "http://w3id.org/roh/impactIndex|http://w3id.org/roh/impactIndexInYear";
                            t.NewValue = idAux + "|" + fila["impactIndexInYear"].value;
                            triples[guid].Add(t);
                        }

                        if (fila.ContainsKey("impactSource"))
                        {
                            TriplesToInclude t = new();
                            t.Predicate = "http://w3id.org/roh/impactIndex|http://w3id.org/roh/impactSource";
                            t.NewValue = idAux + "|" + fila["impactSource"].value;
                            triples[guid].Add(t);
                        }
                        if (fila.ContainsKey("impactSourceOther"))
                        {
                            TriplesToInclude t = new();
                            t.Predicate = "http://w3id.org/roh/impactIndex|http://w3id.org/roh/impactSourceOther";
                            t.NewValue = idAux + "|" + fila["impactSourceOther"].value;
                            triples[guid].Add(t);
                        }
                    }
                    if (triples[guid].Count > 0)
                    {
                        var resultado = mResourceApi.InsertPropertiesLoadedResources(triples);
                    }
                });
            }
        }

        ///// <summary>
        ///// Método para inserción múltiple de indices de impacto
        ///// </summary>
        ///// <param name="pFilas">Filas con los datos para insertar</param>
        //public void InsercionIndicesImpacto(List<Dictionary<string, SparqlObject.Data>> pFilas)
        //{
        //    List<string> ids = pFilas.Select(x => x["document"].value).Distinct().ToList();
        //    if (ids.Count > 0)
        //    {
        //        Parallel.ForEach(ids, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, id =>
        //        {
        //            Guid guid = mResourceApi.GetShortGuid(id);

        //            Dictionary<Guid, List<TriplesToInclude>> triples = new() { { guid, new List<TriplesToInclude>() } };
        //            foreach (Dictionary<string, SparqlObject.Data> fila in pFilas.Where(x => x["document"].value == id))
        //            {
        //                string idAux = mResourceApi.GraphsUrl + "items/ImpactIndex_" + guid.ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
        //                string document = fila["document"].value;
        //                {
        //                    TriplesToInclude t = new();
        //                    t.Predicate = "http://w3id.org/roh/impactIndex|http://w3id.org/roh/impactIndexInYear";
        //                    t.NewValue = idAux + "|" + fila["impactIndexInYear"].value;
        //                    triples[guid].Add(t);
        //                }

        //                if (fila.ContainsKey("impactSource"))
        //                {
        //                    TriplesToInclude t = new();
        //                    t.Predicate = "http://w3id.org/roh/impactIndex|http://w3id.org/roh/impactSource";
        //                    t.NewValue = idAux + "|" + fila["impactSource"].value;
        //                    triples[guid].Add(t);
        //                }
        //                if (fila.ContainsKey("impactSourceOther"))
        //                {
        //                    TriplesToInclude t = new();
        //                    t.Predicate = "http://w3id.org/roh/impactIndex|http://w3id.org/roh/impactSourceOther";
        //                    t.NewValue = idAux + "|" + fila["impactSourceOther"].value;
        //                    triples[guid].Add(t);
        //                }
        //                if (fila.ContainsKey("impactIndexCategory"))
        //                {
        //                    TriplesToInclude t = new();
        //                    t.Predicate = "http://w3id.org/roh/impactIndex|http://w3id.org/roh/impactIndexCategory";
        //                    t.NewValue = idAux + "|" + fila["impactIndexCategory"].value;
        //                    triples[guid].Add(t);
        //                }
        //            }
        //            if (triples[guid].Count > 0)
        //            {
        //                var resultado = mResourceApi.InsertPropertiesLoadedResources(triples);
        //            }
        //        });
        //    }
        //}

        /// <summary>
        /// Método para inserción múltiple de indices de impacto
        /// </summary>
        /// <param name="pFilas">Filas con los datos para insertar</param>
        public void EnvioServicioSimilaridad(string pIdDoc)
        {
            EnrichmentSimilarityPut enrichmentSimilarityPut = null;

            //Obtenemos título y descripción
            string select = "select ?doc ?title ?abstract";
            string where = $@"
where{{
    ?doc a <http://purl.org/ontology/bibo/Document>.
    ?doc <http://w3id.org/roh/title> ?title.
    FILTER(?doc=<{pIdDoc}>)
    OPTIONAL{{?doc <http://purl.org/ontology/bibo/abstract> ?abstract}}
}}";
            SparqlObject response = mResourceApi.VirtuosoQuery(select, where, "document");
            foreach (Dictionary<string, SparqlObject.Data> fila in response.results.bindings)
            {
                string text = fila["title"].value.Trim();
                if (fila.ContainsKey("abstract"))
                {
                    text += " " + fila["abstract"].value.Trim();
                }
                enrichmentSimilarityPut = new EnrichmentSimilarityPut();
                enrichmentSimilarityPut.ro_id = pIdDoc;
                enrichmentSimilarityPut.ro_type = "research_paper";
                enrichmentSimilarityPut.text = text;
                enrichmentSimilarityPut.authors = new List<string>();
                enrichmentSimilarityPut.specific_descriptors = new List<List<object>>();
                enrichmentSimilarityPut.thematic_descriptors = new List<List<object>>();
            }


            if (enrichmentSimilarityPut != null)
            {
                #region Obtenemos autores
                select = "select ?orden ?authorName from <http://gnoss.com/person.owl>";
                where = $@"
where
{{
	?doc a <http://purl.org/ontology/bibo/Document>.
	?doc <http://purl.org/ontology/bibo/authorList> ?author.
    FILTER(?doc=<{pIdDoc}>)
	?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#comment> ?ordenAux.
    ?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
    ?person <http://xmlns.com/foaf/0.1/name> ?authorName
	BIND(xsd:int(?ordenAux) as ?orden)
}}order by asc(?orden) ";
                response = mResourceApi.VirtuosoQuery(select, where, "document");
                foreach (Dictionary<string, SparqlObject.Data> fila in response.results.bindings)
                {
                    enrichmentSimilarityPut.authors.Add(fila["authorName"].value.Trim());
                }
                #endregion

                #region Obtenemos etiquetas

                select = "select ?tag";
                where = $@"
where
{{
	?doc a <http://purl.org/ontology/bibo/Document>.
	?doc <http://vivoweb.org/ontology/core#freeTextKeyword> ?tagAux.
    ?tagAux <http://w3id.org/roh/title> ?tag
    FILTER(?doc=<{pIdDoc}>)
}} ";
                response = mResourceApi.VirtuosoQuery(select, where, "document");
                foreach (Dictionary<string, SparqlObject.Data> fila in response.results.bindings)
                {
                    enrichmentSimilarityPut.specific_descriptors.Add(new List<object>() { fila["tag"].value.Trim(), 1 });
                }

                select = "select ?tag ?score";
                where = $@"
where
{{
	?doc a <http://purl.org/ontology/bibo/Document>.
	?doc <http://w3id.org/roh/enrichedKeywords> ?enrichedKeywords.
    ?enrichedKeywords <http://w3id.org/roh/title> ?tag.
    ?enrichedKeywords <http://w3id.org/roh/score> ?score.
    FILTER(?doc=<{pIdDoc}>)
}} ";
                response = mResourceApi.VirtuosoQuery(select, where, "document");
                foreach (Dictionary<string, SparqlObject.Data> fila in response.results.bindings)
                {
                    string tag = fila["tag"].value.Trim();
                    float score = float.Parse(fila["score"].value.Trim().Replace(",", "."), CultureInfo.InvariantCulture);
                    if (enrichmentSimilarityPut.specific_descriptors.Exists(x => (string)x[0] == tag))
                    {
                        enrichmentSimilarityPut.specific_descriptors.First(x => (string)x[0] == tag)[1] = score;
                    }
                    else
                    {
                        enrichmentSimilarityPut.specific_descriptors.Add(new List<object>() { tag, score });
                    }
                }
                #endregion

                #region Obtenemos categorías

                select = "select ?category from <http://gnoss.com/taxonomy.owl>";
                where = $@"
where
{{
	?doc a <http://purl.org/ontology/bibo/Document>.
    ?doc  <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeAreaAux.
    ?hasKnowledgeAreaAux <http://w3id.org/roh/categoryNode> ?categoryNode.
    ?categoryNode <http://www.w3.org/2008/05/skos#prefLabel> ?category
    MINUS{{
        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
    }}
    FILTER(?doc=<{pIdDoc}>)
}} ";
                response = mResourceApi.VirtuosoQuery(select, where, "document");
                foreach (Dictionary<string, SparqlObject.Data> fila in response.results.bindings)
                {
                    enrichmentSimilarityPut.thematic_descriptors.Add(new List<object>() { fila["category"].value.Trim(), 1 });
                }
                #endregion

                //TODO enriquecidas


                // Cliente.
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromDays(1);

                // Conversión de los datos.
                string informacion = JsonConvert.SerializeObject(enrichmentSimilarityPut, new DecimalFormatConverter());
                StringContent contentData = new StringContent(informacion, System.Text.Encoding.UTF8, "application/json");

                var responsePost = client.PostAsync("http://herculesapi.elhuyar.eus/similarity/add_ro", contentData).Result;
                if (responsePost.IsSuccessStatusCode)
                {
                    EnrichmentSimilarityPutResponse responsePostObject = responsePost.Content.ReadAsAsync<EnrichmentSimilarityPutResponse>().Result;
                }
                else
                {

                }





                // Cliente.
                HttpClient client2 = new HttpClient();
                client2.Timeout = TimeSpan.FromDays(1);

                EnrichmentSimilarityGet enrichmentSimilarityGet = new EnrichmentSimilarityGet();
                enrichmentSimilarityGet.ro_id = pIdDoc;
                enrichmentSimilarityGet.ro_type_target = "research_paper";

                // Conversión de los datos.
                string informacion2 = JsonConvert.SerializeObject(enrichmentSimilarityGet, new DecimalFormatConverter());
                StringContent contentData2 = new StringContent(informacion2, System.Text.Encoding.UTF8, "application/json");

                var responsePost2 = client2.PostAsync("http://herculesapi.elhuyar.eus/similarity/query_similar", contentData2).Result;
                if (responsePost2.IsSuccessStatusCode)
                {
                    EnrichmentSimilarityGetResponse responseGetObject = responsePost2.Content.ReadAsAsync<EnrichmentSimilarityGetResponse>().Result;
                    Dictionary<string, Dictionary<string, float>> similar = responseGetObject.similar_ros_calculado;
                }
                else
                {

                }
            }




            //Obtenemos categorias

            /*
             {
    "ro_id": "2-s2.0-85032573110",
    "ro_type": "research_paper",
    "text": "Analysis of the microstructure and mechanical properties of titanium-based composites reinforced by secondary phases and B In the last decade, titanium metal matrix composites (TMCs) have received considerable attention thanks to their interesting properties as a consequence of the clear interface between the matrix and the reinforcing phases formed. In this work, TMCs with 30 vol % of B",
    "authors": ["Montealegre-Melendez, Isabel", "Arévalo, Cristina", "Ariza, Enrique", "Pérez-Soriano, Eva M.", "Rubio-Escudero, Cristina", "Kitzmantel, Michael", "Neubauer, Erich"],
    "thematic_descriptors": [("Physical Sciences", 0.998)],
    "specific_descriptors": [("tmcs", 0.777), ("titanium-based composites", 0.702), ("secondary phases", 0.664), ("clear interface", 0.564), ("reinforcing phases", 0.534), ("their interesting properties", 0.476), ("titanium metal matrix composites", 0.394), ("consequence", 0.376), ("considerable attention", 0.347), ("thanks", 0.291), ("interesting properties", 0.276), ("analysis", 0.243), ("microstructure and mechanical properties", 0.188), ("decade", 0.187), ("last decade", 0.083), ("work", 0.025)]
}
             */

            //// Cliente.
            //EnrichmentResponseGlobal respuesta = new EnrichmentResponseGlobal();
            //respuesta.tags = new EnrichmentResponse();
            //respuesta.tags.topics = new List<EnrichmentResponseItem>();
            //respuesta.categories = new EnrichmentResponseCategory();
            //respuesta.categories.topics = new List<List<EnrichmentResponseCategory.EnrichmentResponseItem>>();
            //HttpClient client = new HttpClient();
            //client.Timeout = TimeSpan.FromDays(1);

            //// Si la descripción llega nula o vacía...
            //if (string.IsNullOrEmpty(pDesc))
            //{
            //    pDesc = string.Empty;
            //}

            //EnrichmentData enrichmentData = new EnrichmentData() { rotype = "papers", title = pTitulo, abstract_ = pDesc, pdfurl = pUrlPdf };

            //// Conversión de los datos.
            //string informacion = JsonConvert.SerializeObject(enrichmentData,
            //                Newtonsoft.Json.Formatting.None,
            //                new JsonSerializerSettings
            //                {
            //                    NullValueHandling = NullValueHandling.Ignore
            //                });
            //StringContent contentData = new StringContent(informacion, System.Text.Encoding.UTF8, "application/json");

            //#region --- Tópicos Específicos (Tags)
            //var responseSpecific = client.PostAsync(pConfig.GetUrlSpecificEnrichment(), contentData).Result;

            //if (responseSpecific.IsSuccessStatusCode)
            //{
            //    respuesta.tags = responseSpecific.Content.ReadAsAsync<EnrichmentResponse>().Result;
            //}
            //else
            //{
            //    return respuesta;
            //}
            //#endregion

            //#region --- Tópicos Temáticos (Categorías)            
            //var responseThematic = client.PostAsync(pConfig.GetUrlThematicEnrichment(), contentData).Result;

            //EnrichmentResponse categoriasObtenidas = null;

            //if (responseThematic.IsSuccessStatusCode)
            //{
            //    categoriasObtenidas = responseThematic.Content.ReadAsAsync<EnrichmentResponse>().Result;
            //    EnrichmentResponseCategory listaCategoria = new EnrichmentResponseCategory();
            //    listaCategoria.topics = new List<List<EnrichmentResponseCategory.EnrichmentResponseItem>>();

            //    foreach (EnrichmentResponseItem cat in categoriasObtenidas.topics)
            //    {
            //        List<EnrichmentResponseCategory.EnrichmentResponseItem> listaTesauro = new List<EnrichmentResponseCategory.EnrichmentResponseItem>();

            //        if (tuplaTesauro.Item2.ContainsKey("general " + cat.word.ToLower().Trim()))
            //        {
            //            string idTesauro = tuplaTesauro.Item2["general " + cat.word.ToLower().Trim()];
            //            listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });

            //            while (!idTesauro.EndsWith(".0.0.0"))
            //            {
            //                idTesauro = ObtenerIdTesauro(idTesauro);
            //                listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });
            //            }
            //        }
            //        else if (tuplaTesauro.Item2.ContainsKey(cat.word.ToLower().Trim()))
            //        {
            //            string idTesauro = tuplaTesauro.Item2[cat.word.ToLower().Trim()];
            //            listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });

            //            while (!idTesauro.EndsWith(".0.0.0"))
            //            {
            //                idTesauro = ObtenerIdTesauro(idTesauro);
            //                listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });
            //            }
            //        }


            //        if (listaTesauro.Count > 0)
            //        {
            //            listaCategoria.topics.Add(listaTesauro);
            //        }
            //    }
            //    respuesta.categories = listaCategoria;
            //}
            //else
            //{
            //    return respuesta;
            //}
            //#endregion

            //return respuesta;
        }

        public class DecimalFormatConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(int));
            }

            public override void WriteJson(JsonWriter writer, object value,
                                           JsonSerializer serializer)
            {
                if (value.GetType() == 1.GetType())
                {
                    double x = (int)value;
                    writer.WriteValue(x);
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType,
                                         object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
