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
    /// Clase para actualizar propiedades de proyectos
    /// </summary>
    class ActualizadorProject : ActualizadorBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pResourceApi">API Wrapper de GNOSS</param>
        public ActualizadorProject(ResourceApi pResourceApi) : base(pResourceApi)
        {
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/isPublic de los http://vivoweb.org/ontology/core#Project 
        /// los proyectos públicos (son los proyectos oficiales, es decir, los que tienen http://w3id.org/roh/crisIdentifier)
        /// Esta propiedad se utilizará como filtro en el bucador general de proyectos
        /// No tiene dependencias
        /// </summary>
        /// <param name="pProject">ID del proyecto</param>
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

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/publicAuthorList de los http://vivoweb.org/ontology/core#Project
        /// los miembros actuales (sin fecha de alta o fecha de alta anterior a la actual y sin fecha de baja o fecha de baja posterior a la actual) 
        /// Cargamos en los investigadores con CV que tengan el proyecto PÚBLICO en su CV
        /// Cargamos en los investigadores sin CV sólo los proyectos OFICIALES
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        /// <param name="pProject">ID del proyecto</param>
        public void ActualizarPertenenciaPersonas(string pPerson = null,string pProject=null)
        {
            //TODO hablar con Esteban, bajas... fecha actual... fecha fin...
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");

            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
            if (!string.IsNullOrEmpty(pProject))
            {
                filter = $" FILTER(?proyecto =<{pProject}>)";
            }

            while (true)
            {
                //Añadimos a proyectos
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{select distinct ?person ?project from <http://gnoss.com/person.owl> from <http://gnoss.com/curriculumvitae.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?person ?project
                                        Where
                                        {{
                                            {{
                                                    
                                                    ?cv <http://w3id.org/roh/cvOf> ?person.
                                                    ?cv a <http://w3id.org/roh/CV>.
                                                    ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                                    ?scientificExperience ?lvl2 ?cvlvl3.
                                                    ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                    ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                            }}
                                            UNION
                                            {{
                                                    ?project <http://w3id.org/roh/isPublic> 'true'
                                                    MINUS
                                                    {{
                                                            ?anycv <http://w3id.org/roh/cvOf> ?person.
                                                    }}
                                            }}
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?project a <http://vivoweb.org/ontology/core#Project>.            
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))                                    
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.  
                                            OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                            OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}
	                                        BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?end)
                                            BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?start)
                                            FILTER(?start<={fechaActual} AND ?end>={fechaActual} )
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
                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }

            while (true)
            {
                //Eliminamos de proyectos
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{select distinct ?person ?project  from <http://gnoss.com/person.owl> from <http://gnoss.com/curriculumvitae.owl>   ";
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
                                            {{
                                                    
                                                    ?cv <http://w3id.org/roh/cvOf> ?person.
                                                    ?cv a <http://w3id.org/roh/CV>.
                                                    ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                                    ?scientificExperience ?lvl2 ?cvlvl3.
                                                    ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                    ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                            }}
                                            UNION
                                            {{
                                                    ?project <http://w3id.org/roh/isPublic> 'true'
                                                    MINUS
                                                    {{
                                                            ?anycv <http://w3id.org/roh/cvOf> ?person.
                                                    }}
                                            }}
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?project a <http://vivoweb.org/ontology/core#Project>.            
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))                                    
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.  
                                            OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                            OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}
	                                        BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?end)
                                            BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?start)
                                            FILTER(?start<={fechaActual} AND ?end>={fechaActual} )
                                        }}
                                    }}                                    
                                }}}}order by desc(?project) limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");
                EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/publicAuthorList", "project", "person");
                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/publicGroupList de los http://vivoweb.org/ontology/core#Project 
        /// los grupos a los que pertenecían los miembros  del grupo cuando coinciden en el tiempo su pertenencia en el proyecto y en el grupo
        /// No tiene dependencias
        /// </summary>
        /// <param name="pGrupo">ID del grupo</param>
        /// <param name="pProject">ID del proyecto</param>
        public void ActualizarPertenenciaGrupos(string pGrupo = null, string pProject = null)
        {
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
                                            ?person <http://w3id.org/roh/crisIdentifier> ?crisIdentifier
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
                if (resultado.results.bindings.Count != limit)
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
                                            ?person <http://w3id.org/roh/crisIdentifier> ?crisIdentifier
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
                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }


        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/themedAreasNumber de los http://vivoweb.org/ontology/core#Project 
        /// el número de áreas temáticas de las publicaciones oficiales del proyecto, este dato es inferido de las publicaciones validadas que pertenecen al proyecto
        /// No tiene dependencias
        /// </summary>
        /// <param name="pProject">ID del proyecto</param>
        public void ActualizarNumeroAreasTematicas(string pProject = null)
        {
            //TODO Estebam como se vvinculan docs a proyectos?
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
                                    ?documento <http://w3id.org/roh/isValidated> 'true'.
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

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/publicationsNumber de los http://vivoweb.org/ontology/core#Project  
        /// el número de publicaciones validades del proyecto
        /// No tiene dependencias
        /// </summary>
        /// <param name="pProject">ID del proyecto</param>
        public void ActualizarNumeroPublicaciones(string pProject = null)
        {
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
                                    ?doc <http://w3id.org/roh/isValidated> 'true'.
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

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/collaboratorsNumber de los http://vivoweb.org/ontology/core#Project  
        /// el nº de colaboradores (personas con autorías en documentos del proyecto que no son miembros públicos del proyecto)
        /// Depende de ActualizadorProject.ActualizarPertenenciaPersonas y de ActualizadorDocument.ActualizarPertenenciaPersonas
        /// </summary>
        /// <param name="pProject">ID del proyecto</param>
        public void ActualizarNumeroColaboradores(string pProject = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pProject))
            {
                filter = $" FILTER(?project =<{pProject}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/collaboratorsNumber");

            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?project ?numColaboradoresCargados ?numColaboradoresACargar  from <http://gnoss.com/person.owl> from <http://gnoss.com/document.owl> ";
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
		                                    }}		
		                                    MINUS
		                                    {{
			                                    ?project <http://w3id.org/roh/publicAuthorList> ?person
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

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }

        }


        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/researchersNumber de los http://vivoweb.org/ontology/core#Project
        /// el nº de miembros 
        /// Depende de ActualizadorProject.ActualizarPertenenciaPersonas
        /// </summary>
        /// <param name="pProject">ID del proyecto</param>
        public void ActualizarNumeroMiembros(string pProject = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pProject))
            {
                filter = $" FILTER(?project =<{pProject}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/researchersNumber");

            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?project ?numMiembrosCargados ?numMiembrosACargar  from <http://gnoss.com/person.owl> from <http://gnoss.com/document.owl> ";
                String where = @$"where{{
                            ?project a <http://vivoweb.org/ontology/core#Project>.
                            {filter}
                            OPTIONAL
                            {{
                              ?project <http://w3id.org/roh/researchersNumber> ?numMiembrosCargadosAux. 
                              BIND(xsd:int( ?numMiembrosCargadosAux) as  ?numMiembrosCargados)
                            }}
                            {{
                              select ?project count(distinct ?person) as ?numMiembrosACargar
                              Where{{                               
                                ?project a <http://vivoweb.org/ontology/core#Project>.
                                OPTIONAL
                                {{                                    	
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
		                            ?project <http://w3id.org/roh/publicAuthorList> ?person	                                    
                                }}                                
                              }}Group by ?project 
                            }}
                            FILTER(?numMiembrosCargados!= ?numMiembrosACargar OR !BOUND(?numMiembrosCargados) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string project = fila["project"].value;
                    string numMiembrosACargar = fila["numMiembrosACargar"].value;
                    string numMiembrosCargados = "";
                    if (fila.ContainsKey("numMiembrosCargados"))
                    {
                        numMiembrosCargados = fila["numMiembrosCargados"].value;
                    }
                    ActualizadorTriple(project, "http://w3id.org/roh/researchersNumber", numMiembrosCargados, numMiembrosACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }

        }

    }
}
