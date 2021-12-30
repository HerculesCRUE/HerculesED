using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DesnormalizadorHercules.Models
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
        /// Actualizamos en la propiedad http://w3id.org/roh/isPublic de los http://purl.org/ontology/bibo/Document
        /// los documentos públicos (son los documentos oficiales, es decir, los que tienen http://w3id.org/roh/crisIdentifier o los documentos que son públicos en algún CV)
        /// Esta propiedad se utilizará como filtro en el bucador general de publicaciones
        /// Depende de ActualizadorCV.InsertarDocumentos y ActualizadorCV.CambiarPrivacidadDocumentos
        /// </summary>
        /// <param name="pDocument">ID del documento</param>
        public void ActualizarDocumentosPublicos(string pDocument = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pDocument))
            {
                filter = $" FILTER(?document =<{pDocument}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/isPublic");

            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?document ?isPublicCargado ?isPublicACargar  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl>";
                String where = @$"where{{
                            ?document a <http://purl.org/ontology/bibo/Document>.
                            {filter}
                            OPTIONAL
                            {{
                                ?document <http://w3id.org/roh/isPublic> ?isPublicCargado.
                            }}
                            {{
                                select distinct ?document IF(BOUND(?isPublicACargar),?isPublicACargar,'false')  as ?isPublicACargar
                                Where
                                {{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    OPTIONAL
                                    {{
                                        ?document <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        BIND('true' as ?isPublicACargar)
                                    }}
                                    OPTIONAL
                                    {{  
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv ?lvl1 ?cvlvl2.
                                        ?cvlvl2 ?lvl2 ?cvlvl3.
                                        ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                        ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                        BIND('true' as ?isPublicACargar)
                                    }}      
                                }}
                            }}
                            FILTER(?isPublicCargado!= ?isPublicACargar OR !BOUND(?isPublicCargado) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string document = fila["document"].value;
                    string isPublicACargar = fila["isPublicACargar"].value;
                    string isPublicCargado = "";
                    if (fila.ContainsKey("isPublicCargado"))
                    {
                        isPublicCargado = fila["isPublicCargado"].value;
                    }
                    ActualizadorTriple(document, "http://w3id.org/roh/isPublic", isPublicCargado, isPublicACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }

        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/publicAuthorList de los http://purl.org/ontology/bibo/Document los autores 'públicos'
        /// Cargamos en los investigadores con CV que tengan la publicación PÚBLICA en su CV
        /// Cargamos en los investigadores sin CV sólo los documentos validados
        /// Esta propiedad se utilizará para mostrar en las fichas de los investigadores el listado de sus publicaciones públicas
        /// Depende de ActualizadorCV.CrearCVs, de ActualizadorCV.InsertarDocumentos y de ActualizadorCV.CambiarPrivacidadDocumentos
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        /// <param name="pDocument">ID del documento</param>
        public void ActualizarPertenenciaPersonas(string pPerson = null, string pDocument = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
            if (!string.IsNullOrEmpty(pDocument))
            {
                filter = $" FILTER(?document =<{pDocument}>)";
            }
            while (true)
            {
                //Añadimos a documentos
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{select distinct ?document  ?person  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?document ?person
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.     
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autores.
                                            ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            {{
                                                    ?cv <http://w3id.org/roh/cvOf> ?person.
                                                    ?cv a <http://w3id.org/roh/CV>.
                                                    ?cv ?lvl1 ?cvlvl2.
                                                    ?cvlvl2 ?lvl2 ?cvlvl3.
                                                    ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                    ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                            }}
                                            UNION
                                            {{
                                                    ?document <http://w3id.org/roh/isValidated> 'true'
                                                    MINUS
                                                    {{
                                                            ?anycv <http://w3id.org/roh/cvOf>?person.
                                                    }}
                                            }}
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document <http://w3id.org/roh/publicAuthorList> ?person.
                                    }}
                                }}}}order by desc(?document) limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                InsercionMultiple(resultado.results.bindings, "http://w3id.org/roh/publicAuthorList", "document", "person");
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
                String select = @"select * where{select distinct ?document  ?person  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document <http://w3id.org/roh/publicAuthorList> ?person.                                                                           
                                    }}
                                    MINUS
                                    {{
                                        select distinct ?document ?person
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.     
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autores.
                                            ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            {{
                                                    ?cv <http://w3id.org/roh/cvOf>?person.
                                                    ?cv a <http://w3id.org/roh/CV>.
                                                    ?cv ?lvl1 ?cvlvl2.
                                                    ?cvlvl2 ?lvl2 ?cvlvl3.
                                                    ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                    ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                            }}
                                            UNION
                                            {{
                                                    ?document <http://w3id.org/roh/isValidated> 'true'
                                                    MINUS
                                                    {{
                                                            ?anycv <http://w3id.org/roh/cvOf>?person.
                                                    }}
                                            }}                                            
                                        }}
                                    }}
                                }}}}order by desc(?document) limit {limit}";
                var resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/publicAuthorList", "document", "person");
                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }


        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/isProducedBy de los http://purl.org/ontology/bibo/Document 
        /// los grupos a los que pertenecen los autores de los documentos oficiales (http://w3id.org/roh/isValidated) en el momento de la publicación del documento
        /// Esta propiedad se utilizará para mostrar en las fichas de los grupos el listado de sus publicaciones oficiales
        /// No tiene dependencias
        /// </summary>
        /// <param name="pGroup">ID del grupo</param>
        /// <param name="pDocument">ID del documento</param>
        public void ActualizarPertenenciaGrupos(string pGroup = null, string pDocument = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pGroup))
            {
                filter = $" FILTER(?grupo =<{pGroup}>)";
            }
            if (!string.IsNullOrEmpty(pDocument))
            {
                filter = $" FILTER(?doc =<{pDocument}>)";
            }
            while (true)
            {
                //Añadimos a documentos
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{select distinct ?doc ?grupo  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl> from <http://gnoss.com/group.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?grupo ?doc
                                        Where{{
                                            ?grupo a <http://xmlns.com/foaf/0.1/Group>.
                                            ?grupo ?propmembers ?members.
                                            ?members <http://w3id.org/roh/roleOf> ?person.
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            FILTER(?propmembers  in (<http://xmlns.com/foaf/0.1/member>, <http://w3id.org/roh/mainResearchers>))
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
                                }}}}order by desc(?doc) limit {limit}";
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
                String select = @"select * where{select distinct ?doc ?grupo  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl> from <http://gnoss.com/group.owl>  ";
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
                                            ?grupo ?propmembers ?members.
                                            ?members <http://w3id.org/roh/roleOf> ?person.
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            FILTER(?propmembers  in (<http://xmlns.com/foaf/0.1/member>, <http://w3id.org/roh/mainResearchers>))
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
                                }}}}order by desc(?doc) limit {limit}";
                var resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/isProducedBy", "doc", "grupo");
                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }


        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/citationCount de los http://purl.org/ontology/bibo/Document
        /// el nº máximo de citas con el que cuenta
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocument">ID del documento</param>
        public void ActualizarNumeroCitasMaximas(string pDocument = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pDocument))
            {
                filter = $" FILTER(?document =<{pDocument}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/citationCount");

            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                String select = @"select * where{select ?document ?numCitasCargadas IF (BOUND (?numCitasACargar), ?numCitasACargar, 0 ) as ?numCitasACargar";
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
                                        ?document <http://w3id.org/roh/wos> ?numCitas.
                                    }}
                                    UNION{{
                                        ?document <http://w3id.org/roh/inrecs> ?numCitas.
                                    }}
                                    UNION{{
                                        ?document <http://w3id.org/roh/scopus> ?numCitas.
                                    }}
                                }}                                
                              }}
                            }}
                            FILTER(?numCitasCargadas!= ?numCitasACargar OR !BOUND(?numCitasCargadas) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string document = fila["document"].value;
                    string numCitasACargar = fila["numCitasACargar"].value;
                    string numCitasCargadas = "";
                    if (fila.ContainsKey("numCitasCargadas"))
                    {
                        numCitasCargadas = fila["numCitasCargadas"].value;
                    }
                    ActualizadorTriple(document, "http://w3id.org/roh/citationCount", numCitasCargadas, numCitasACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/citationLoadedCount de los http://purl.org/ontology/bibo/Document públicos
        /// el nº de citas cargadas en el sistema
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocument">ID del documento</param>
        public void ActualizarNumeroCitasCargadas(string pDocument = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pDocument))
            {
                filter = $" FILTER(?document =<{pDocument}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/citationLoadedCount");

            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                String select = @"select * where{select ?document ?numCitasCargadas IF (BOUND (?numCitasACargar), ?numCitasACargar, 0 ) as ?numCitasACargar";
                String where = @$"where{{
                            ?document a <http://purl.org/ontology/bibo/Document>.
                            {filter}
                            OPTIONAL
                            {{
                              ?document <http://w3id.org/roh/citationLoadedCount> ?numCitasCargadasAux. 
                              BIND(xsd:int( ?numCitasCargadasAux) as  ?numCitasCargadas)
                            }}
                            {{
                              select ?document count(distinct ?documentX) as ?numCitasACargar
                              Where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                OPTIONAL{{
                                    ?documentX <http://purl.org/ontology/bibo/cites> ?document.
                                }}
                              }}
                            }}
                            FILTER(?numCitasCargadas!= ?numCitasACargar OR !BOUND(?numCitasCargadas) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string document = fila["document"].value;
                    string numCitasACargar = fila["numCitasACargar"].value;
                    string numCitasCargadas = "";
                    if (fila.ContainsKey("numCitasCargadas"))
                    {
                        numCitasCargadas = fila["numCitasCargadas"].value;
                    }
                    ActualizadorTriple(document, "http://w3id.org/roh/citationLoadedCount", numCitasCargadas, numCitasACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/referencesLoadedCount de los http://purl.org/ontology/bibo/Document públicos
        /// el nº de referencias cargadas en el sistema
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocument">ID del documento</param>
        public void ActualizarNumeroReferenciasCargadas(string pDocument = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pDocument))
            {
                filter = $" FILTER(?document =<{pDocument}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("document", "http://purl.org/ontology/bibo/Document", "http://w3id.org/roh/referencesLoadedCount");

            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                String select = @"select * where{select ?document ?numReferenciasCargadas IF (BOUND (?numReferenciasACargar), ?numReferenciasACargar, 0 ) as ?numReferenciasACargar";
                String where = @$"where{{
                            ?document a <http://purl.org/ontology/bibo/Document>.
                            {filter}
                            OPTIONAL
                            {{
                              ?document <http://w3id.org/roh/referencesLoadedCount> ?numReferenciasCargadasAux. 
                              BIND(xsd:int( ?numReferenciasCargadasAux) as  ?numReferenciasCargadas)
                            }}
                            {{
                              select ?document count(distinct ?documentX) as ?numReferenciasACargar
                              Where{{
                                ?document a <http://purl.org/ontology/bibo/Document>.
                                OPTIONAL{{
                                    ?document <http://purl.org/ontology/bibo/cites> ?documentX.
                                    ?documentX a <http://purl.org/ontology/bibo/Document>.
                                }}
                              }}
                            }}
                            FILTER(?numReferenciasCargadas!= ?numReferenciasACargar OR !BOUND(?numReferenciasCargadas) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string document = fila["document"].value;
                    string numReferenciasACargar = fila["numReferenciasACargar"].value;
                    string numReferenciasCargadas = "";
                    if (fila.ContainsKey("numReferenciasCargadas"))
                    {
                        numReferenciasCargadas = fila["numReferenciasCargadas"].value;
                    }
                    ActualizadorTriple(document, "http://w3id.org/roh/referencesLoadedCount", numReferenciasCargadas, numReferenciasACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/hasKnowledgeArea de los http://purl.org/ontology/bibo/Document públicos 
        /// las áreas del documento (obtenido de varias propiedades en las que están las áreas en función de su origen)
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocument">ID del documento</param>
        public void ActualizarAreasDocumentos(string pDocument = null)
        {
            //Categorías
            //unificada-->http://w3id.org/roh/hasKnowledgeArea
            //usuario-->http://w3id.org/roh/userKnowledgeArea
            //external-->http://w3id.org/roh/externalKnowledgeArea
            //enriched-->http://w3id.org/roh/enrichedKnowledgeArea

            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pDocument))
                {
                    filter = $" FILTER(?document =<{pDocument}>)";
                }

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

                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["document"].value;
                        string categoryNode = fila["categoryNode"].value;
                        //TODO from
                        select = @"select ?document ?hasKnowledgeArea   ?categoryNode from <http://gnoss.com/taxonomy.owl>";
                        where = @$"where{{
                                    FILTER(?document=<{person}>)
                                    FILTER(?categoryNode =<{categoryNode}>)
                                    {{ 
                                        select distinct ?person ?hasKnowledgeArea  ?categoryNode
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
                        List<RemoveTriples> triplesRemove = new ();
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
                            var resultadox = mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { mResourceApi.GetShortGuid(person), triplesRemove } });
                        }
                    }


                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }



                //Cargamos el tesauro
                Dictionary<string, string> dicAreasBroader = new ();
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
                    String select = @"select distinct * where{select ?document ?categoryNode from <http://gnoss.com/taxonomy.owl>";
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
                            }}}}order by (?document) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                    InsertarCategorias(resultado, dicAreasBroader, graphsUrl,"document", "http://w3id.org/roh/hasKnowledgeArea");
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
                    String select = @"select distinct * where{select ?document ?hasKnowledgeArea from <http://gnoss.com/taxonomy.owl>";
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
                            }}}} limit {limit}";
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
        /// Insertamos en la propiedad http://vivoweb.org/ontology/core#freeTextKeyword de los http://purl.org/ontology/bibo/Document públicos 
        /// los tagso (obtenido de varias propiedades en las que están los tags en función de su origen)
        /// No tiene dependencias
        /// </summary>
        /// <param name="pDocument">ID del documento</param>
        public void ActualizarTagsDocumentos(string pDocument = null)
        {
            //Etiquetas
            //unificada-->http://vivoweb.org/ontology/core#freeTextKeyword
            //usuario-->http://w3id.org/roh/userKeywords
            //external-->http://w3id.org/roh/externalKeywords
            //enriched-->http://w3id.org/roh/enrichedKeywords

            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pDocument))
                {
                    filter = $" FILTER(?document =<{pDocument}>)";
                }

                while (true)
                {
                    int limit = 500;
                    //INSERTAMOS
                    String select = @"select distinct * where{select ?document ?tag";
                    String where = @$"where{{
                            ?document a <http://purl.org/ontology/bibo/Document>.
                            {filter}
                            {{
                                select  distinct ?document ?tag where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    ?document ?props ?tag.
                                    FILTER(?props in (<http://w3id.org/roh/userKeywords>,<http://w3id.org/roh/externalKeywords>,<http://w3id.org/roh/enrichedKeywords>))
                                }}
                            }}
                            MINUS{{
                                select distinct ?document ?tag
                                where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    ?document <http://vivoweb.org/ontology/core#freeTextKeyword> ?tag.
                                }}
                            }}
                            }}}}order by (?document) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                    InsercionMultiple(resultado.results.bindings, "http://vivoweb.org/ontology/core#freeTextKeyword", "document", "tag");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    int limit = 500;
                    //ELIMINAMOS
                    String select = @"select distinct * where{select ?document ?tag";
                    String where = @$"where{{
                            ?document a <http://purl.org/ontology/bibo/Document>.
                            {filter}
                            {{
                                select distinct ?document ?tag
                                where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    ?document <http://vivoweb.org/ontology/core#freeTextKeyword> ?tag.
                                }}                               
                            }}
                            MINUS{{
                                select  distinct ?document ?tag where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    ?document ?props ?tag.
                                    FILTER(?props in (<http://w3id.org/roh/userKeywords>,<http://w3id.org/roh/externalKeywords>,<http://w3id.org/roh/enrichedKeywords>))
                                }}
                                 
                            }}
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");
                    EliminacionMultiple(resultado.results.bindings, "http://vivoweb.org/ontology/core#freeTextKeyword", "document", "tag");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

    }
}
