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
    public class ActualizadorGroup : ActualizadorBase
    {
        public ActualizadorGroup(ResourceApi pResourceApi) : base(pResourceApi)
        {
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/researchersNumber de los http://xmlns.com/foaf/0.1/Group el nº de miembros en la fecha actual
        /// </summary>
        /// <param name="pGrupo">ID del grupo</param>
        public void ActualizarNumeroMiembros(string pGrupo=null)
        {
            //TODO comentario query
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");

            string filter = "";
            if(!string.IsNullOrEmpty(pGrupo))
            {
                filter = $" FILTER(?group =<{pGrupo}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("group", "http://xmlns.com/foaf/0.1/Group", "http://w3id.org/roh/researchersNumber");

            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?group ?numMiembrosCargados ?numMiembrosACargar  from <http://gnoss.com/person.owl> ";
                String where = @$"where{{
                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                            {filter}
                            OPTIONAL
                            {{
                              ?group <http://w3id.org/roh/researchersNumber> ?numMiembrosCargadosAux. 
                              BIND(xsd:int( ?numMiembrosCargadosAux) as  ?numMiembrosCargados)
                            }}
                            {{
                              select ?group count(distinct ?person) as ?numMiembrosACargar
                              Where{{                               
                                ?group a <http://xmlns.com/foaf/0.1/Group>.
                                OPTIONAL
                                {{
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?group <http://w3id.org/roh/mainResearchers> ?rol.
                                        ?rol <http://w3id.org/roh/roleOf> ?person.
                                        OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                        OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                        BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                        BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                        FILTER(?start<={fechaActual} AND ?end>={fechaActual} )
                                    }}
                                    UNION
                                    {{ 
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?group <http://xmlns.com/foaf/0.1/member> ?rol.
                                        ?rol <http://w3id.org/roh/roleOf> ?person.
                                        OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                        OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                        BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                        BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                        FILTER(?start<={fechaActual} AND ?end>={fechaActual} )
                                    }}
                                }}                                
                              }}Group by ?group 
                            }}
                            FILTER(?numMiembrosCargados!= ?numMiembrosACargar OR !BOUND(?numMiembrosCargados) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string group = fila["group"].value;
                    string numMiembrosACargar = fila["numMiembrosACargar"].value;
                    string numMiembrosCargados = "";
                    if (fila.ContainsKey("numMiembrosCargados"))
                    {
                        numMiembrosCargados = fila["numMiembrosCargados"].value;
                    }
                    ActualizadorTriple(group, "http://w3id.org/roh/researchersNumber", numMiembrosCargados, numMiembrosACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }

        }


        public void ActualizarNumeroColaboradores(string pGrupo = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pGrupo))
            {
                filter = $" FILTER(?group =<{pGrupo}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("group", "http://xmlns.com/foaf/0.1/Group", "http://w3id.org/roh/collaboratorsNumber");

            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?group ?numColaboradoresCargados ?numColaboradoresACargar  from <http://gnoss.com/person.owl> from <http://gnoss.com/project.owl> from <http://gnoss.com/document.owl> ";
                String where = @$"where{{
                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                            {filter}
                            OPTIONAL
                            {{
                              ?group <http://w3id.org/roh/collaboratorsNumber> ?numColaboradoresCargadosAux. 
                              BIND(xsd:int( ?numColaboradoresCargadosAux) as  ?numColaboradoresCargados)
                            }}
                            {{
                              select ?group count(distinct ?person) as ?numColaboradoresACargar
                              Where{{                               
                                ?group a <http://xmlns.com/foaf/0.1/Group>.
                                OPTIONAL
                                {{
                                    {{
	                                    SELECT DISTINCT ?person ?group
	                                    WHERE 
	                                    {{	
                                            ?person a <http://xmlns.com/foaf/0.1/Person>
		                                    {{
			                                    {{
				                                    #Documentos
				                                    SELECT *
				                                    WHERE {{
					                                    ?documento <http://w3id.org/roh/isProducedBy> ?group.
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
					                                    ?proy <http://w3id.org/roh/publicGroupList> ?group.
					                                    ?proy a <http://vivoweb.org/ontology/core#Project>.
					                                    ?proy ?propRol ?role.
					                                    FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))
					                                    ?role <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
				                                    }}
			                                    }}
		                                    }}		
		                                    MINUS
		                                    {{
			                                    ?person <http://vivoweb.org/ontology/core#relates> ?group
		                                    }}
	                                    }}
                                    }}
                                }}                                
                              }}Group by ?group 
                            }}
                            FILTER(?numColaboradoresCargados!= ?numColaboradoresACargar OR !BOUND(?numColaboradoresCargados) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string group = fila["group"].value;
                    string numColaboradoresACargar = fila["numColaboradoresACargar"].value;
                    string numColaboradoresCargados = "";
                    if (fila.ContainsKey("numColaboradoresCargados"))
                    {
                        numColaboradoresCargados = fila["numColaboradoresCargados"].value;
                    }
                    ActualizadorTriple(group, "http://w3id.org/roh/collaboratorsNumber", numColaboradoresCargados, numColaboradoresACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }

        }

        public void ActualizarNumeroProyectos(string pGrupo = null,string pProyecto=null)
        {
            //TODO Cambiar por los que estén públicos en el CV

            
            //TODO comentario query
            string filter = "";
            if (!string.IsNullOrEmpty(pProyecto))
            {
                filter = $" FILTER(?proyecto =<{pProyecto}>)";
            }
            if (!string.IsNullOrEmpty(pProyecto))
            {
                filter = $" FILTER(?group =<{pGrupo}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("group", "http://xmlns.com/foaf/0.1/Group", "http://w3id.org/roh/projectsNumber");

            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?group  ?numProyectosCargados ?numProyectosACargar  from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl> ";
                String where = @$"where{{
                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                            {filter}
                            OPTIONAL
                            {{
                              ?group <http://w3id.org/roh/projectsNumber> ?numProyectosCargadosAux. 
                              BIND(xsd:int( ?numProyectosCargadosAux) as  ?numProyectosCargados)
                            }}
                            {{
                              select ?group count(distinct ?proyecto) as ?numProyectosACargar
                              Where{{
                                ?group a <http://xmlns.com/foaf/0.1/Group>.
                                OPTIONAL{{
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
                              }}Group by ?group 
                            }}
                            FILTER(?numProyectosCargados!= ?numProyectosACargar OR !BOUND(?numProyectosCargados) )
                        }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string grupo = fila["group"].value;
                    string numProyectosACargar = fila["numProyectosACargar"].value;
                    string numProyectosCargados = "";
                    if (fila.ContainsKey("numProyectosCargados"))
                    {
                        numProyectosCargados = fila["numProyectosCargados"].value;
                    }
                    ActualizadorTriple(grupo, "http://w3id.org/roh/projectsNumber", numProyectosCargados, numProyectosACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }

        public void ActualizarNumeroPublicaciones(string pGrupo = null)
        {
            //TODO cambiar con lo que este público en el CV
            //TODO comentario query
           

            string filter = "";
            if (!string.IsNullOrEmpty(pGrupo))
            {
                filter = $" FILTER(?grupo =<{pGrupo}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("group", "http://xmlns.com/foaf/0.1/Group", "http://w3id.org/roh/publicationsNumber");


            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?grupo  ?numDocumentosCargados ?numDocumentosACargar  from <http://gnoss.com/document.owl>  from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                            ?grupo a <http://xmlns.com/foaf/0.1/Group>.
                            {filter}
                            OPTIONAL
                            {{
                              ?grupo <http://w3id.org/roh/publicationsNumber> ?numDocumentosCargadosAux. 
                              BIND(xsd:int( ?numDocumentosCargadosAux) as  ?numDocumentosCargados)
                            }}
                            {{
                              select ?grupo count(distinct ?doc) as ?numDocumentosACargar
                              Where{{
                                ?grupo a <http://xmlns.com/foaf/0.1/Group>.
                                OPTIONAL{{
                                    ?grupo ?propmembers ?members.
                                    ?members <http://w3id.org/roh/roleOf> ?person.
	                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    FILTER(?propmembers  in (<http://xmlns.com/foaf/0.1/member>, <http://w3id.org/roh/mainResearchers>))
                                    OPTIONAL{{?members <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                    OPTIONAL{{?members <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}
	                                BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?fechaPersonaEndAux)
                                    BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?fechaPersonaInitAux)
                                    ?doc a <http://purl.org/ontology/bibo/Document>.
                                    ?doc <http://w3id.org/roh/isPublic> 'true'.
                                    ?doc <http://w3id.org/roh/isValidated> 'true'.
                                    ?doc <http://purl.org/ontology/bibo/authorList> ?autores.
                                    ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                    ?doc <http://purl.org/dc/terms/issued> ?fechaPublicacion.
                                    FILTER(xsd:integer(?fechaPublicacion)>= ?fechaPersonaInitAux AND xsd:integer(?fechaPublicacion)<= ?fechaPersonaEndAux)
                                }}
                              }}Group by ?grupo 
                            }}
                            FILTER(?numDocumentosCargados!= ?numDocumentosACargar OR !BOUND(?numDocumentosCargados) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string grupo = fila["grupo"].value;
                    string numDocumentosACargar = fila["numDocumentosACargar"].value;
                    string numDocumentosCargados = "";
                    if (fila.ContainsKey("numDocumentosCargados"))
                    {
                        numDocumentosCargados = fila["numDocumentosCargados"].value;
                    }
                    ActualizadorTriple(grupo, "http://w3id.org/roh/publicationsNumber", numDocumentosCargados, numDocumentosACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }

        public void ActualizarNumeroAreasTematicas(string pGrupo = null)
        {
            //TODO cambiar con lo que este público en el CV
            //TODO comentario query


            string filter = "";
            if (!string.IsNullOrEmpty(pGrupo))
            {
                filter = $" FILTER(?grupo =<{pGrupo}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("group", "http://xmlns.com/foaf/0.1/Group", "http://w3id.org/roh/themedAreasNumber");


            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?grupo  ?numAreasTematicasCargadas ?numAreasTematicasACargar  from <http://gnoss.com/document.owl>  from <http://gnoss.com/taxonomy.owl>  ";
                String where = @$"where{{
                            ?grupo a <http://xmlns.com/foaf/0.1/Group>.
                            {filter}
                            OPTIONAL
                            {{
                              ?grupo <http://w3id.org/roh/themedAreasNumber> ?numAreasTematicasCargadasAux. 
                              BIND(xsd:int( ?numAreasTematicasCargadasAux) as  ?numAreasTematicasCargadas)
                            }}
                            {{
                              select ?grupo count(distinct ?categoria) as ?numAreasTematicasACargar
                              Where{{
                                ?grupo a <http://xmlns.com/foaf/0.1/Group>.
                                OPTIONAL{{
                                    ?documento a <http://purl.org/ontology/bibo/Document>. 
                                    ?documento <http://w3id.org/roh/isProducedBy> ?grupo.
                                    ?documento <http://w3id.org/roh/hasKnowledgeArea> ?area.
                                    ?area <http://w3id.org/roh/categoryNode> ?categoria.
                                    ?categoria <http://www.w3.org/2008/05/skos#prefLabel> ?nombreCategoria.
                                    MINUS
                                    {{
                                        ?categoria <http://www.w3.org/2008/05/skos#narrower> ?hijos
                                    }}
                                }}
                              }}Group by ?grupo 
                            }}
                            FILTER(?numAreasTematicasCargadas!= ?numAreasTematicasACargar OR !BOUND(?numAreasTematicasCargadas) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string grupo = fila["grupo"].value;
                    string numAreasTematicasACargar = fila["numAreasTematicasACargar"].value;
                    string numAreasTematicasCargadas = "";
                    if (fila.ContainsKey("numAreasTematicasCargadas"))
                    {
                        numAreasTematicasCargadas = fila["numAreasTematicasCargadas"].value;
                    }
                    ActualizadorTriple(grupo, "http://w3id.org/roh/themedAreasNumber", numAreasTematicasCargadas, numAreasTematicasACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }

        public void ActualizarPertenenciaLineas(string pGrupo = null)
        {
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");
            string filter = "";
            if (!string.IsNullOrEmpty(pGrupo))
            {
                filter = $" FILTER(?group =<{pGrupo}>)";
            }
            while (true)
            {
                //Añadimos líneas
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select distinct ?group  ?linea  from <http://gnoss.com/person.owl> ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?group ?linea
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.                                            
                                            ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            {{
                                                ?group <http://w3id.org/roh/mainResearchers> ?rol.
                                                ?rol <http://w3id.org/roh/roleOf> ?person.                                                
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                                BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                                BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                                ?rol <http://vivoweb.org/ontology/core#hasResearchArea> ?lineaAux.
                                                ?lineaAux <http://w3id.org/roh/title> ?linea.        
                                                OPTIONAL{{?linea <http://vivoweb.org/ontology/core#start> ?startLineAux.}}
                                                OPTIONAL{{?linea <http://vivoweb.org/ontology/core#end> ?endLineAux.}}
                                                BIND(IF(BOUND(?endLineAux),xsd:integer(?endLineAux) ,30000000000000)  as ?endLine)
                                                BIND(IF(BOUND(?startLineAux),xsd:integer(?startLineAux),10000000000000)  as ?startLine)
                                            }}
                                            UNION
                                            {{ 
                                                ?group <http://xmlns.com/foaf/0.1/member> ?rol.
                                                ?rol <http://w3id.org/roh/roleOf> ?person.
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                                BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                                BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                                ?rol <http://vivoweb.org/ontology/core#hasResearchArea> ?lineaAux.
                                                ?lineaAux <http://w3id.org/roh/title> ?linea.  
                                                OPTIONAL{{?linea <http://vivoweb.org/ontology/core#start> ?startLineAux.}}
                                                OPTIONAL{{?linea <http://vivoweb.org/ontology/core#end> ?endLineAux.}}
                                                BIND(IF(BOUND(?endLineAux),xsd:integer(?endLineAux) ,30000000000000)  as ?endLine)
                                                BIND(IF(BOUND(?startLineAux),xsd:integer(?startLineAux),10000000000000)  as ?startLine)
                                            }}
                                            FILTER(?start<={fechaActual} AND ?end>={fechaActual} )
                                            FILTER(?startLine<={fechaActual} AND ?endLine>={fechaActual} )
                                            MINUS
                                            {{
                                                ?group a <http://xmlns.com/foaf/0.1/Group>.
                                                ?group <http://w3id.org/roh/lineResearch> ?linea. 
                                            }}
                                        }}
                                    }}                                    
                                }}}}order by desc(?group) limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                InsercionMultiple(resultado.results.bindings, "http://w3id.org/roh/lineResearch", "group", "linea");
                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }

            while (true)
            {
                //Eliminamos líneas
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select distinct ?group  ?linea  from <http://gnoss.com/person.owl> ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/lineResearch> ?linea.                                        
                                    }}
                                    MINUS
                                    {{
                                        select distinct ?person ?linea
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.                                            
                                            ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            {{
                                                ?group <http://w3id.org/roh/mainResearchers> ?rol.
                                                ?rol <http://w3id.org/roh/roleOf> ?person.                                                
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                                BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                                BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                                ?rol <http://vivoweb.org/ontology/core#hasResearchArea> ?lineaAux.
                                                ?lineaAux <http://w3id.org/roh/title> ?linea.        
                                                OPTIONAL{{?linea <http://vivoweb.org/ontology/core#start> ?startLineAux.}}
                                                OPTIONAL{{?linea <http://vivoweb.org/ontology/core#end> ?endLineAux.}}
                                                BIND(IF(BOUND(?endLineAux),xsd:integer(?endLineAux) ,30000000000000)  as ?endLine)
                                                BIND(IF(BOUND(?startLineAux),xsd:integer(?startLineAux),10000000000000)  as ?startLine)
                                            }}
                                            UNION
                                            {{ 
                                                ?group <http://xmlns.com/foaf/0.1/member> ?rol.
                                                ?rol <http://w3id.org/roh/roleOf> ?person.
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                                BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                                BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                                ?rol <http://vivoweb.org/ontology/core#hasResearchArea> ?lineaAux.
                                                ?lineaAux <http://w3id.org/roh/title> ?linea.  
                                                OPTIONAL{{?linea <http://vivoweb.org/ontology/core#start> ?startLineAux.}}
                                                OPTIONAL{{?linea <http://vivoweb.org/ontology/core#end> ?endLineAux.}}
                                                BIND(IF(BOUND(?endLineAux),xsd:integer(?endLineAux) ,30000000000000)  as ?endLine)
                                                BIND(IF(BOUND(?startLineAux),xsd:integer(?startLineAux),10000000000000)  as ?startLine)
                                            }}
                                            FILTER(?start<={fechaActual} AND ?end>={fechaActual} )
                                            FILTER(?startLine<={fechaActual} AND ?endLine>={fechaActual} )
                                        }}
                                    }}
                                }}}}order by desc(?group) limit {limit}";
                var resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/lineResearch", "group", "linea");
                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }
                
        public void ActualizarGruposPublicos(string pGrupo = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pGrupo))
            {
                filter = $" FILTER(?group =<{pGrupo}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("group", "http://xmlns.com/foaf/0.1/Group", "http://w3id.org/roh/isPublic");

            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?group ?isPublicCargado ?isPublicACargar ";
                String where = @$"where{{
                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                            {filter}
                            OPTIONAL
                            {{
                              ?group <http://w3id.org/roh/isPublic> ?isPublicCargado.
                            }}
                            {{
                              select ?group IF(BOUND(?crisIdentifier),'true','false')  as ?isPublicACargar
                              Where{{                               
                                ?group a <http://xmlns.com/foaf/0.1/Group>.
                                OPTIONAL
                                {{
                                    ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                }}                                
                              }}
                            }}
                            FILTER(?isPublicCargado!= ?isPublicACargar OR !BOUND(?isPublicCargado) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string group = fila["group"].value;
                    string isPublicACargar = fila["isPublicACargar"].value;
                    string isPublicCargado = "";
                    if (fila.ContainsKey("isPublicCargado"))
                    {
                        isPublicCargado = fila["isPublicCargado"].value;
                    }
                    ActualizadorTriple(group, "http://w3id.org/roh/isPublic", isPublicCargado, isPublicACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }

        }

        public void ActualizarAreasGrupos(string pGroup = null)
        {
            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pGroup))
                {
                    filter = $" FILTER(?group =<{pGroup}>)";
                }

                //Eliminamos las categorías duplicadas
                while (true)
                {
                    int limit = 500;
                    //TODO from
                    String select = @"select ?group ?categoryNode from <http://gnoss.com/taxonomy.owl> ";
                    String where = @$"where{{
                                select distinct ?group ?hasKnowledgeAreaAux  ?categoryNode
                                where{{
                                    {filter}
                                    ?group a <http://xmlns.com/foaf/0.1/Group>.
                                    ?group <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeAreaAux .
                                    ?hasKnowledgeAreaAux <http://w3id.org/roh/categoryNode> ?categoryNode.
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                               }}
                            }}group by ?group ?categoryNode HAVING (COUNT(*) > 1) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string group = fila["group"].value;
                        string categoryNode = fila["categoryNode"].value;
                        //TODO from
                        select = @"select ?group ?hasKnowledgeArea  ?categoryNode from <http://gnoss.com/taxonomy.owl>";
                        where = @$"where{{
                                    FILTER(?group=<{group}>)
                                    FILTER(?categoryNode =<{categoryNode}>)
                                    {{ 
                                        select distinct ?group ?hasKnowledgeArea  ?categoryNode
                                        where{{
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                                            ?group <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeArea.
                                            ?hasKnowledgeArea <http://w3id.org/roh/categoryNode> ?categoryNode.
                                            MINUS{{
                                                ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                            }}
                                        }}
                                    }}
                                }}";
                        resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                        List<RemoveTriples> triplesRemove = new List<RemoveTriples>();
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
                            var resultadox = mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { mResourceApi.GetShortGuid(group), triplesRemove } });
                        }
                    }


                    if (resultado.results.bindings.Count() != limit)
                    {
                        break;
                    }
                }



                //Cargamos el tesauro
                Dictionary<string, string> dicAreasBroader = new Dictionary<string, string>();
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
                    String select = @"select distinct * where{select ?group ?categoryNode from <http://gnoss.com/person.owl> from <http://gnoss.com/taxonomy.owl>";
                    String where = @$"where{{
                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                            {filter}
                            {{
                                select  distinct ?group ?hasKnowledgeAreaPerson ?categoryNode where{{
                                    ?group a <http://xmlns.com/foaf/0.1/Group>.
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?person <http://vivoweb.org/ontology/core#relates> ?group.                                    
                                    ?person  <http://vivoweb.org/ontology/core#hasResearchArea> ?hasKnowledgeAreaPerson.
                                    ?hasKnowledgeAreaPerson <http://w3id.org/roh/categoryNode> ?categoryNode.
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                                }}
                            }}
                            MINUS{{
                                select distinct ?group ?hasKnowledgeArea ?categoryNode 
                                where{{
                                    ?group a <http://xmlns.com/foaf/0.1/Group>.
                                    ?group <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeArea.
                                    ?hasKnowledgeArea <http://w3id.org/roh/categoryNode> ?categoryNode
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                                }}
                            }}
                            }}}}order by (?group) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                    InsertarCategorias(resultado, dicAreasBroader, graphsUrl, "group", "http://w3id.org/roh/hasKnowledgeArea");
                    if (resultado.results.bindings.Count() != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    int limit = 500;
                    //ELIMINAMOS
                    //TODO eliminar from
                    String select = @"select distinct * where{select ?group ?hasKnowledgeArea from <http://gnoss.com/person.owl> from <http://gnoss.com/taxonomy.owl>";
                    String where = @$"where{{
                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                            {filter}
                            {{
                                select distinct ?group ?hasKnowledgeArea ?categoryNode 
                                where{{
                                    ?group a <http://xmlns.com/foaf/0.1/Group>.
                                    ?group <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeArea.
                                    ?hasKnowledgeArea <http://w3id.org/roh/categoryNode> ?categoryNode
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                                }}                               
                            }}
                            MINUS{{
                                select  distinct ?group ?hasKnowledgeAreaPerson ?categoryNode where{{
                                    ?group a <http://xmlns.com/foaf/0.1/Group>.
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?person <http://vivoweb.org/ontology/core#relates> ?group.                                    
                                    ?person  <http://vivoweb.org/ontology/core#hasResearchArea> ?hasKnowledgeAreaPerson.
                                    ?hasKnowledgeAreaPerson <http://w3id.org/roh/categoryNode> ?categoryNode.
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                                }}
                                 
                            }}
                            }}}}order by (?group) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                    EliminarCategorias(resultado, "group", "http://w3id.org/roh/hasKnowledgeArea");
                    if (resultado.results.bindings.Count() != limit)
                    {
                        break;
                    }
                }
            }
        }

    }
}
