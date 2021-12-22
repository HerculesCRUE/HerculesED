﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DesnormalizadorHercules.Models
{
    class ActualizadorProject : ActualizadorBase
    {
        public ActualizadorProject(ResourceApi pResourceApi) : base(pResourceApi)
        {
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/publicAuthorList de los http://vivoweb.org/ontology/core#Project los miembros 'públicos'
        /// Cargamos los investigadores con CV que tengan el proyecto PÚBLICO en su CV
        /// Cargamos los investigadores sin CV que aparezcan en algun proyecto PÚBLICO en un CV
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        /// <param name="pProject">ID del proyecto</param>
        public void ActualizarPertenenciaPersonas(string pPerson = null,string pProject=null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
            if (!string.IsNullOrEmpty(pProject))
            {
                filter = $" FILTER(?proyecto =<{pProject}>)";
            }

            //TODO temporal, cambiar por el códiugo comentado y revisar, esta query esta mal hecha
            while (true)
            {
                //Añadimos a proyectos
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{select distinct ?person ?project from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?person ?project
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))                                    
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.                                            
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?project <http://w3id.org/roh/publicAuthorList> ?person.
                                    }}
                                }}}}order by desc(?project) limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");
                InsercionMultiple(resultado.results.bindings, "http://w3id.org/roh/publicAuthorList", "project", "person");
                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }

            while (true)
            {
                //Eliminamos de proyectos
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{select distinct ?person ?project from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?project <http://w3id.org/roh/publicAuthorList> ?person.
                                    }}                                    
                                    MINUS
                                    {{
                                        select distinct ?person ?project
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.                                        
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))                                    
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.                                            
                                        }}
                                    }}                                    
                                }}}}order by desc(?project) limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");
                EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/publicAuthorList", "project", "person");
                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }


        public void ActualizarPertenenciaGrupos(string pGrupo = null, string pProject = null)
        {
            //TODO comentario query
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");
            //TODO propiedad http://w3id.org/roh/publicGroupList

            string filter = "";
            if (!string.IsNullOrEmpty(pGrupo))
            {
                filter = $" FILTER(?group =<{pGrupo}>)";
            }
            if (!string.IsNullOrEmpty(pProject))
            {
                filter = $" FILTER(?proyecto =<{pProject}>)";
            }
            while (true)
            {
                //Añadimos a grupos
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{select distinct ?proyecto  ?group  from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select ?proyecto ?group
                                        Where{{
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.                                            
                                            ?proyecto a <http://vivoweb.org/ontology/core#Project>.
                                            ?proyecto <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            ?proyecto ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))                                    
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person. 
                                            #Fechas proyectos
                                            OPTIONAL{{?proyecto <http://vivoweb.org/ontology/core#start> ?fechaProjInit.}}
                                            OPTIONAL{{?proyecto <http://vivoweb.org/ontology/core#end> ?fechaProjEnd.}} 
                                            BIND(IF(bound(?fechaProjEnd), xsd:integer(?fechaProjEnd), 30000000000000) as ?fechaProjEndAux)
                                            BIND(IF(bound(?fechaProjInit), xsd:integer(?fechaProjInit), 10000000000000) as ?fechaProjInitAux)
                                            #Fechas pertenencias a grupos
                                            ?group ?p2 ?member.
                                            FILTER (?p2 IN (<http://xmlns.com/foaf/0.1/member>, <http://w3id.org/roh/mainResearchers> ) )
                                            ?member <http://w3id.org/roh/roleOf> ?person. 
                                            OPTIONAL{{?member <http://vivoweb.org/ontology/core#start> ?fechaGroupInit.}}
                                            OPTIONAL{{?member <http://vivoweb.org/ontology/core#end> ?fechaGroupEnd.}}
                                            BIND(IF(bound(?fechaGroupEnd), xsd:integer(?fechaGroupEnd), 30000000000000) as ?fechaGroupEndAux)
                                            BIND(IF(bound(?fechaGroupInit), xsd:integer(?fechaGroupInit), 10000000000000) as ?fechaGroupInitAux)
                                            FILTER(?fechaGroupEndAux >= ?fechaProjInitAux AND ?fechaGroupInitAux <= ?fechaProjEndAux)                                                                            
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?proyecto a <http://vivoweb.org/ontology/core#Project>.
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?proyecto <http://w3id.org/roh/publicGroupList> ?group.
                                    }}
                                }}}}order by desc(?proyecto) limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                InsercionMultiple(resultado.results.bindings, "http://w3id.org/roh/publicGroupList", "proyecto", "group");
                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }

            while (true)
            {
                //Eliminamos de grupos
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select distinct ?proyecto  ?group  from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        ?proyecto a <http://vivoweb.org/ontology/core#Project>.
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?proyecto <http://w3id.org/roh/publicGroupList> ?group.                                  
                                    }}
                                    MINUS
                                    {{
                                        select ?proyecto ?group
                                        Where{{
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.                                            
                                            ?proyecto a <http://vivoweb.org/ontology/core#Project>.
                                            ?proyecto <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            ?proyecto ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))                                    
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person. 
                                            #Fechas proyectos
                                            OPTIONAL{{?proyecto <http://vivoweb.org/ontology/core#start> ?fechaProjInit.}}
                                            OPTIONAL{{?proyecto <http://vivoweb.org/ontology/core#end> ?fechaProjEnd.}} 
                                            BIND(IF(bound(?fechaProjEnd), xsd:integer(?fechaProjEnd), 30000000000000) as ?fechaProjEndAux)
                                            BIND(IF(bound(?fechaProjInit), xsd:integer(?fechaProjInit), 10000000000000) as ?fechaProjInitAux)
                                            #Fechas pertenencias a grupos
                                            ?group ?p2 ?member.
                                            FILTER (?p2 IN (<http://xmlns.com/foaf/0.1/member>, <http://w3id.org/roh/mainResearchers> ) )
                                            ?member <http://w3id.org/roh/roleOf> ?person. 
                                            OPTIONAL{{?member <http://vivoweb.org/ontology/core#start> ?fechaGroupInit.}}
                                            OPTIONAL{{?member <http://vivoweb.org/ontology/core#end> ?fechaGroupEnd.}}
                                            BIND(IF(bound(?fechaGroupEnd), xsd:integer(?fechaGroupEnd), 30000000000000) as ?fechaGroupEndAux)
                                            BIND(IF(bound(?fechaGroupInit), xsd:integer(?fechaGroupInit), 10000000000000) as ?fechaGroupInitAux)
                                            FILTER(?fechaGroupEndAux >= ?fechaProjInitAux AND ?fechaGroupInitAux <= ?fechaProjEndAux)                                                                            
                                        }}
                                    }}
                                }}}}order by desc(?proyecto) limit {limit}";
                var resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/publicGroupList", "proyecto", "group");
                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }


        }


        public void ActualizarProyectosPublicos(string pProject = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pProject))
            {
                filter = $" FILTER(?project =<{pProject}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/isPublic");

            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?project ?isPublicCargado ?isPublicACargar ";
                String where = @$"where{{
                            ?project a <http://vivoweb.org/ontology/core#Project>.
                            {filter}
                            OPTIONAL
                            {{
                              ?project <http://w3id.org/roh/isPublic> ?isPublicCargado.
                            }}
                            {{
                              select ?project IF(BOUND(?crisIdentifier),'true','false')  as ?isPublicACargar
                              Where{{                               
                                ?project a <http://vivoweb.org/ontology/core#Project>.
                                OPTIONAL
                                {{
                                    ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                }}                                
                              }}
                            }}
                            FILTER(?isPublicCargado!= ?isPublicACargar OR !BOUND(?isPublicCargado) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string project = fila["project"].value;
                    string isPublicACargar = fila["isPublicACargar"].value;
                    string isPublicCargado = "";
                    if (fila.ContainsKey("isPublicCargado"))
                    {
                        isPublicCargado = fila["isPublicCargado"].value;
                    }
                    ActualizadorTriple(project, "http://w3id.org/roh/isPublic", isPublicCargado, isPublicACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }


        public void ActualizarNumeroAreasTematicas(string pProject = null)
        {
            //TODO cambiar con lo que este público en el CV
            //TODO comentario query


            string filter = "";
            if (!string.IsNullOrEmpty(pProject))
            {
                filter = $" FILTER(?project =<{pProject}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/themedAreasNumber");


            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?project  ?numAreasTematicasCargadas ?numAreasTematicasACargar  from <http://gnoss.com/document.owl>  from <http://gnoss.com/taxonomy.owl>  ";
                String where = @$"where{{
                            ?project a <http://vivoweb.org/ontology/core#Project>.
                            {filter}
                            OPTIONAL
                            {{
                              ?project <http://w3id.org/roh/themedAreasNumber> ?numAreasTematicasCargadasAux. 
                              BIND(xsd:int( ?numAreasTematicasCargadasAux) as  ?numAreasTematicasCargadas)
                            }}
                            {{
                              select ?project count(distinct ?categoria) as ?numAreasTematicasACargar
                              Where{{
                                ?project a <http://vivoweb.org/ontology/core#Project>.
                                OPTIONAL{{
                                    ?documento a <http://purl.org/ontology/bibo/Document>. 
                                    ?documento <http://w3id.org/roh/project> ?project.
                                    ?documento <http://w3id.org/roh/hasKnowledgeArea> ?area.
                                    ?area <http://w3id.org/roh/categoryNode> ?categoria.
                                    ?categoria <http://www.w3.org/2008/05/skos#prefLabel> ?nombreCategoria.
                                    MINUS
                                    {{
                                        ?categoria <http://www.w3.org/2008/05/skos#narrower> ?hijos
                                    }}
                                }}
                              }}Group by ?project 
                            }}
                            FILTER(?numAreasTematicasCargadas!= ?numAreasTematicasACargar OR !BOUND(?numAreasTematicasCargadas) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string project = fila["project"].value;
                    string numAreasTematicasACargar = fila["numAreasTematicasACargar"].value;
                    string numAreasTematicasCargadas = "";
                    if (fila.ContainsKey("numAreasTematicasCargadas"))
                    {
                        numAreasTematicasCargadas = fila["numAreasTematicasCargadas"].value;
                    }
                    ActualizadorTriple(project, "http://w3id.org/roh/themedAreasNumber", numAreasTematicasCargadas, numAreasTematicasACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }


        public void ActualizarNumeroPublicaciones(string pProject = null)
        {
            //TODO cambiar con lo que este público en el CV
            //TODO comentario query


            string filter = "";
            if (!string.IsNullOrEmpty(pProject))
            {
                filter = $" FILTER(?project =<{pProject}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/publicationsNumber");


            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?project  ?numDocumentosCargados ?numDocumentosACargar  from <http://gnoss.com/document.owl>  from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                            ?project a <http://vivoweb.org/ontology/core#Project>.
                            {filter}
                            OPTIONAL
                            {{
                              ?project <http://w3id.org/roh/publicationsNumber> ?numDocumentosCargadosAux. 
                              BIND(xsd:int( ?numDocumentosCargadosAux) as  ?numDocumentosCargados)
                            }}
                            {{
                              select ?project count(distinct ?doc) as ?numDocumentosACargar
                              Where{{
                                ?project a <http://vivoweb.org/ontology/core#Project>.
                                OPTIONAL{{
                                    ?doc a <http://purl.org/ontology/bibo/Document>. 
                                    ?doc <http://w3id.org/roh/project> ?project.
                                }}
                              }}Group by ?project 
                            }}
                            FILTER(?numDocumentosCargados!= ?numDocumentosACargar OR !BOUND(?numDocumentosCargados) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string project = fila["project"].value;
                    string numDocumentosACargar = fila["numDocumentosACargar"].value;
                    string numDocumentosCargados = "";
                    if (fila.ContainsKey("numDocumentosCargados"))
                    {
                        numDocumentosCargados = fila["numDocumentosCargados"].value;
                    }
                    ActualizadorTriple(project, "http://w3id.org/roh/publicationsNumber", numDocumentosCargados, numDocumentosACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }


        public void ActualizarNumeroColaboradores(string pProject = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pProject))
            {
                filter = $" FILTER(?project =<{pProject}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://xmlns.com/foaf/0.1/Group", "http://w3id.org/roh/collaboratorsNumber");

            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?project ?numColaboradoresCargados ?numColaboradoresACargar  from <http://gnoss.com/person.owl> from <http://gnoss.com/project.owl> from <http://gnoss.com/document.owl> ";
                String where = @$"where{{
                            ?project a <http://vivoweb.org/ontology/core#Project>.
                            {filter}
                            OPTIONAL
                            {{
                              ?project <http://w3id.org/roh/collaboratorsNumber> ?numColaboradoresCargadosAux. 
                              BIND(xsd:int( ?numColaboradoresCargadosAux) as  ?numColaboradoresCargados)
                            }}
                            {{
                              select ?project count(distinct ?person) as ?numColaboradoresACargar
                              Where{{                               
                                ?project a <http://vivoweb.org/ontology/core#Project>.
                                OPTIONAL
                                {{
                                    {{
	                                    SELECT DISTINCT ?person ?project
	                                    WHERE 
	                                    {{	
                                            ?person a <http://xmlns.com/foaf/0.1/Person>
		                                    {{
			                                    {{
				                                    #Documentos
				                                    SELECT *
				                                    WHERE {{
					                                    ?documento <http://w3id.org/roh/project> ?project.
					                                    ?documento a <http://purl.org/ontology/bibo/Document>.
					                                    ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores.
					                                    ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
				                                    }}
			                                    }} 
			                                    UNION 
			                                    {{
				                                    #Proyectos
				                                    SELECT *
				                                    WHERE {{
					                                    ?project ?propRol ?role.
					                                    FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))
					                                    ?role <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
				                                    }}
			                                    }}
		                                    }}		
		                                    MINUS
		                                    {{
			                                    ?person <http://vivoweb.org/ontology/core#relates> ?project
		                                    }}
	                                    }}
                                    }}
                                }}                                
                              }}Group by ?project 
                            }}
                            FILTER(?numColaboradoresCargados!= ?numColaboradoresACargar OR !BOUND(?numColaboradoresCargados) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string project = fila["project"].value;
                    string numColaboradoresACargar = fila["numColaboradoresACargar"].value;
                    string numColaboradoresCargados = "";
                    if (fila.ContainsKey("numColaboradoresCargados"))
                    {
                        numColaboradoresCargados = fila["numColaboradoresCargados"].value;
                    }
                    ActualizadorTriple(project, "http://w3id.org/roh/collaboratorsNumber", numColaboradoresCargados, numColaboradoresACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }

        }

    }
}
