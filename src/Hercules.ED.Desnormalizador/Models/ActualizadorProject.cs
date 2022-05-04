using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

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
        /// Actualizamos en la propiedad http://w3id.org/roh/isValidated de los http://vivoweb.org/ontology/core#Project 
        /// los proyectos validados (son los proyectos oficiales, es decir, los que tienen http://w3id.org/roh/crisIdentifier)
        /// Esta propiedad se utilizará como filtro en el bucador general de proyectos
        /// No tiene dependencias
        /// </summary>
        /// <param name="pProjects">IDs de los proyectos</param>
        public void ActualizarProyectosValidados(List<string> pProjects = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/isPublic");

            List<string> filters = new List<string>();
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project in(<{string.Join(">,<", pProjects)}>))");
            }
            else
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {


                while (true)
                {
                    int limit = 500;
                    String select = @"select * where{ select ?project ?isValidatedCargado ?isValidatedCargar ";
                    String where = @$"where{{
                            ?project a <http://vivoweb.org/ontology/core#Project>.
                            {filter}
                            OPTIONAL
                            {{
                              ?project <http://w3id.org/roh/isValidated> ?isValidatedCargado.
                            }}
                            {{
                              select ?project IF(BOUND(?crisIdentifier),'true','false')  as ?isValidatedCargar
                              Where{{                               
                                ?project a <http://vivoweb.org/ontology/core#Project>.
                                OPTIONAL
                                {{
                                    ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                }}                                
                              }}
                            }}
                            FILTER(?isValidatedCargado!= ?isValidatedCargar OR !BOUND(?isValidatedCargado) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string project = fila["project"].value;
                        string isValidatedCargar = fila["isValidatedCargar"].value;
                        string isValidatedCargado = "";
                        if (fila.ContainsKey("isValidatedCargado"))
                        {
                            isValidatedCargado = fila["isValidatedCargado"].value;
                        }
                        ActualizadorTriple(project, "http://w3id.org/roh/isValidated", isValidatedCargado, isValidatedCargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/mainResearchers y http://w3id.org/roh/researchers de los http://vivoweb.org/ontology/core#Project los miembros de los proyectos,
        /// Además desnormalizamos los nombres dentro de la entidad auxiliar http://w3id.org/roh/PersonAux a la que apuntan estas propiedades
        /// No tiene dependencias
        /// </summary>
        /// <param name="pProjects">IDs de los proyectos</param>
        public void ActualizarMiembros(List<string> pProjects = null)
        {
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");

            //Si el proyecto a terminado, han de aparecer todos los participantes que haya tenido,
            //pero si sigue activo solamente los participantes actuales

            List<string> filters = new List<string>();
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project in(<{string.Join(">,<", pProjects)}>))");
            }
            else
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {

                //Creamos los miembros
                while (true)
                {
                    int limit = 500;
                    //TODO from
                    String select = @"select * where{select distinct ?project ?person ?comment ?nick ?nombre ?ip  from <http://gnoss.com/person.owl>";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?project ?person ?comment ?nick ?nombre ?ip
                                        Where{{
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://vivoweb.org/ontology/core#relates> ?member.    
                                            ?member <http://w3id.org/roh/roleOf> ?person.                                            
                                            ?person <http://xmlns.com/foaf/0.1/name> ?nombre.                                            
                                            ?member <http://w3id.org/roh/isIP> ?ip.
                                            OPTIONAL{{?member <http://www.w3.org/1999/02/22-rdf-syntax-ns#comment> ?comment.}}
                                            OPTIONAL{{?member <http://xmlns.com/foaf/0.1/nick> ?nick.}}
                                            OPTIONAL{{?member <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                            OPTIONAL{{?member <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}                                            
                                            {{
                                                BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?fechaPersonaEndAux)
                                                BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?fechaPersonaInitAux)
                                                FILTER(?fechaPersonaInitAux<={fechaActual} AND ?fechaPersonaEndAux>={fechaActual})
                                            }}UNION
                                            {{
                                                ?project <http://vivoweb.org/ontology/core#end> ?fin.
                                                BIND(xsd:integer(?fin) as ?finAux).
                                                FILTER(?finAux<={fechaActual})
                                            }}
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        {{
                                            ?project <http://w3id.org/roh/mainResearchers> ?mainResearcher.
                                            ?mainResearcher <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person
                                            BIND('true' as ?ip)
                                        }}
                                        UNION
                                        {{
                                            ?project <http://w3id.org/roh/researchers> ?researcher.
                                            ?researcher <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person
                                            BIND('false' as ?ip)
                                        }}
                                    }}
                                }}}}order by desc(?project) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");
                    InsercionMiembrosProyectoGrupo(resultado.results.bindings, "project");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                //Eliminamos los miembros
                while (true)
                {
                    int limit = 500;
                    String select = @"select * where{select distinct ?project ?propPersonAux ?personAux  from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        {{
                                            ?project <http://w3id.org/roh/mainResearchers> ?mainResearcher.
                                            ?mainResearcher <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person
                                            BIND('true' as ?ip)
                                            BIND(?mainResearcher as ?personAux)
                                            BIND(<http://w3id.org/roh/mainResearchers> as ?propPersonAux)
                                        }}
                                        UNION
                                        {{
                                            ?project <http://w3id.org/roh/researchers> ?researcher.
                                            ?researcher <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person
                                            BIND('false' as ?ip)
                                            BIND(?researcher as ?personAux)
                                            BIND(<http://w3id.org/roh/researchers> as ?propPersonAux)
                                        }}
                                    }}MINUS
                                    {{
                                        select distinct ?project ?person ?nick ?ip
                                        Where{{
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            ?project <http://vivoweb.org/ontology/core#relates> ?member.
                                            ?member <http://w3id.org/roh/roleOf> ?person.
                                            ?member <http://w3id.org/roh/isIP> ?ip.
                                            OPTIONAL{{?member <http://xmlns.com/foaf/0.1/nick> ?nick.}}
                                            OPTIONAL{{?member <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                            OPTIONAL{{?member <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}                                            
                                            {{
                                                BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?fechaPersonaEndAux)
                                                BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?fechaPersonaInitAux)
                                                FILTER(?fechaPersonaInitAux<={fechaActual} AND ?fechaPersonaEndAux>={fechaActual})
                                            }}UNION
                                            {{
                                                ?project <http://vivoweb.org/ontology/core#end> ?fin.
                                                BIND(xsd:integer(?fin) as ?finAux).
                                                FILTER(?finAux<={fechaActual})
                                            }}
                                        }}
                                    }}                                    
                                }}}}order by desc(?project) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");
                    EliminacionMiembrosProyectoGrupo(resultado.results.bindings, "project");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                //Asignamos foaf:firstName
                Dictionary<string, string> propiedadesPersonProject = new Dictionary<string, string>();
                propiedadesPersonProject["http://xmlns.com/foaf/0.1/firstName"] = "http://xmlns.com/foaf/0.1/firstName";
                propiedadesPersonProject["http://xmlns.com/foaf/0.1/lastName"] = "http://xmlns.com/foaf/0.1/familyName";
                propiedadesPersonProject["--"] = "http://w3id.org/roh/secondFamilyName";
                foreach (string propPerson in propiedadesPersonProject.Keys)
                {
                    string propProject = propiedadesPersonProject[propPerson];
                    while (true)
                    {
                        int limit = 500;
                        //TODO eliminar from
                        String select = @"select * where{select distinct ?project ?personAux ?propPersonAux ?property ?propertyLoad ?propertyToLoad from <http://gnoss.com/person.owl> ";
                        String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?project ?personAux ?propPersonAux ?property ?propertyLoad ?propertyToLoad
                                        Where{{
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            {{
                                                ?project <http://w3id.org/roh/mainResearchers> ?mainResearcher.
                                                ?mainResearcher <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                                OPTIONAL{{?mainResearcher <{propProject}> ?propertyLoad.}}
                                                BIND(?mainResearcher as ?personAux)
                                                BIND(<http://w3id.org/roh/mainResearchers> as ?propPersonAux)
                                            }}
                                            UNION
                                            {{
                                                ?project <http://w3id.org/roh/researchers> ?researcher.
                                                ?researcher <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                                 OPTIONAL{{?researcher <{propProject}> ?propertyLoad.}}
                                                BIND(?researcher as ?personAux)
                                                BIND(<http://w3id.org/roh/researchers> as ?propPersonAux)
                                            }}
                                            ?person <{propPerson}> ?propertyToLoad.
                                            BIND(<{propProject}> as ?property).
                                            FILTER(
                                                    (!BOUND(?propertyLoad) AND BOUND(?propertyToLoad)) 
                                                    OR 
                                                    (BOUND(?propertyLoad) AND !BOUND(?propertyToLoad)) 
                                                    OR 
                                                    (str(?propertyLoad) != str(?propertyToLoad))
                                            )
                                        }}
                                    }}
                                }}}}order by desc(?project) limit {limit}";
                        SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");
                        ActualizarPropiedadMiembrosProyectoGrupo(resultado.results.bindings, "project");
                        if (resultado.results.bindings.Count != limit)
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://w3id.org/roh/isProducedBy de los http://vivoweb.org/ontology/core#Project validados
        /// los grupos validados a los que pertenecían los miembros  del grupo cuando coinciden en el tiempo su pertenencia en el proyecto y en el grupo
        /// No tiene dependencias
        /// </summary>
        /// <param name="pGrupos">IDs de los grupos</param>
        /// <param name="pProjects">IDs de los proyectos</param>
        public void ActualizarPertenenciaGrupos(List<string> pGrupos = null, List<string> pProjects = null)
        {
            List<string> filters = new List<string>();
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project in (<{string.Join(">,<", pProjects)}>))");
            }
            else if (pGrupos != null && pGrupos.Count > 0)
            {
                filters.Add($" FILTER(?group in (<{string.Join(">,<", pGrupos)}>))");
            }
            else
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {
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
                                            ?group <http://w3id.org/roh/isValidated> 'true'.   
                                            ?proyecto a <http://vivoweb.org/ontology/core#Project>.
                                            ?proyecto <http://w3id.org/roh/isValidated> 'true'.   
                                            ?proyecto <http://vivoweb.org/ontology/core#relates> ?rol.                              
                                            ?rol <http://w3id.org/roh/roleOf> ?person. 
                                            #Fechas proyectos
                                            OPTIONAL{{?proyecto <http://vivoweb.org/ontology/core#start> ?fechaProjInit.}}
                                            OPTIONAL{{?proyecto <http://vivoweb.org/ontology/core#end> ?fechaProjEnd.}} 
                                            BIND(IF(bound(?fechaProjEnd), xsd:integer(?fechaProjEnd), 30000000000000) as ?fechaProjEndAux)
                                            BIND(IF(bound(?fechaProjInit), xsd:integer(?fechaProjInit), 10000000000000) as ?fechaProjInitAux)
                                            #Fechas pertenencias a grupos
                                            ?group <http://vivoweb.org/ontology/core#relates> ?member.
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
                                        ?proyecto <http://w3id.org/roh/isProducedBy> ?group.
                                    }}
                                }}}}order by desc(?proyecto) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                    InsercionMultiple(resultado.results.bindings, "http://w3id.org/roh/isProducedBy", "proyecto", "group");
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
                                        ?proyecto <http://w3id.org/roh/isProducedBy> ?group.                                  
                                    }}
                                    MINUS
                                    {{
                                        select ?proyecto ?group
                                        Where{{
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.   
                                            ?group <http://w3id.org/roh/isValidated> 'true'.   
                                            ?proyecto a <http://vivoweb.org/ontology/core#Project>.
                                            ?proyecto <http://w3id.org/roh/isValidated> 'true'.   
                                            ?proyecto <http://vivoweb.org/ontology/core#relates> ?rol.                    
                                            ?rol <http://w3id.org/roh/roleOf> ?person. 
                                            #Fechas proyectos
                                            OPTIONAL{{?proyecto <http://vivoweb.org/ontology/core#start> ?fechaProjInit.}}
                                            OPTIONAL{{?proyecto <http://vivoweb.org/ontology/core#end> ?fechaProjEnd.}} 
                                            BIND(IF(bound(?fechaProjEnd), xsd:integer(?fechaProjEnd), 30000000000000) as ?fechaProjEndAux)
                                            BIND(IF(bound(?fechaProjInit), xsd:integer(?fechaProjInit), 10000000000000) as ?fechaProjInitAux)
                                            #Fechas pertenencias a grupos
                                            ?group <http://vivoweb.org/ontology/core#relates> ?member.                    
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
                    EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/isProducedBy", "proyecto", "group");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/themedAreasNumber de los http://vivoweb.org/ontology/core#Project 
        /// el número de áreas temáticas de las publicaciones oficiales del proyecto
        /// Depende de ActualizadorDocument.ActualizarDocumentosValidados
        /// </summary>
        /// <param name="pProjects">IDs de los proyectos</param>
        public void ActualizarNumeroAreasTematicas(List<string> pProjects = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/themedAreasNumber");

            List<string> filters = new List<string>();
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project in(<{string.Join(">,<", pProjects)}>))");
            }
            else
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {
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

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string project = fila["project"].value;
                        string numAreasTematicasACargar = fila["numAreasTematicasACargar"].value;
                        string numAreasTematicasCargadas = "";
                        if (fila.ContainsKey("numAreasTematicasCargadas"))
                        {
                            numAreasTematicasCargadas = fila["numAreasTematicasCargadas"].value;
                        }
                        ActualizadorTriple(project, "http://w3id.org/roh/themedAreasNumber", numAreasTematicasCargadas, numAreasTematicasACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/publicationsNumber de los http://vivoweb.org/ontology/core#Project  
        /// el número de publicaciones validadas del proyecto
        /// Depende de ActualizadorDocument.ActualizarDocumentosValidados
        /// </summary>
        /// <param name="pProjects">IDs de los proyectos</param>
        public void ActualizarNumeroPublicaciones(List<string> pProjects = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/publicationsNumber");

            List<string> filters = new List<string>();
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project in(<{string.Join(">,<", pProjects)}>))");
            }
            else
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {
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

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string project = fila["project"].value;
                        string numDocumentosACargar = fila["numDocumentosACargar"].value;
                        string numDocumentosCargados = "";
                        if (fila.ContainsKey("numDocumentosCargados"))
                        {
                            numDocumentosCargados = fila["numDocumentosCargados"].value;
                        }
                        ActualizadorTriple(project, "http://w3id.org/roh/publicationsNumber", numDocumentosCargados, numDocumentosACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/collaboratorsNumber de los http://vivoweb.org/ontology/core#Project  validados
        /// el nº de colaboradores (personas con autorías en documentos validados del proyecto que no son miembros del proyecto)
        /// Depende de ActualizadorProject.ActualizarMiembros y ActualizadorDocument.ActualizarDocumentosValidados
        /// </summary>
        /// <param name="pProjects">IDs de los proyectos</param>
        public void ActualizarNumeroColaboradores(List<string> pProjects = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/collaboratorsNumber");

            List<string> filters = new List<string>();
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project in(<{string.Join(">,<", pProjects)}>))");
            }
            else
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {

                while (true)
                {
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"select * where{ select ?project ?numColaboradoresCargados ?numColaboradoresACargar  from <http://gnoss.com/person.owl> from <http://gnoss.com/document.owl> ";
                    String where = @$"where{{
                            ?project a <http://vivoweb.org/ontology/core#Project>.
                            ?project <http://w3id.org/roh/isValidated> 'true'.
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
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
		                                    {{
			                                    {{
				                                    #Documentos
				                                    SELECT *
				                                    WHERE {{
					                                    ?documento <http://w3id.org/roh/project> ?project.
                                                        ?documento <http://w3id.org/roh/isValidated> 'true'.
					                                    ?documento a <http://purl.org/ontology/bibo/Document>.
					                                    ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores.
					                                    ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
				                                    }}
			                                    }} 
		                                    }}		
		                                    MINUS
		                                    {{
			                                    ?project ?propRol ?rol.
                                                FILTER(?propRol in ( <http://w3id.org/roh/mainResearchers>, <http://w3id.org/roh/researchers>))
                                                ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person
		                                    }}
	                                    }}
                                    }}
                                }}                                
                              }}Group by ?project 
                            }}
                            FILTER(?numColaboradoresCargados!= ?numColaboradoresACargar OR !BOUND(?numColaboradoresCargados) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string project = fila["project"].value;
                        string numColaboradoresACargar = fila["numColaboradoresACargar"].value;
                        string numColaboradoresCargados = "";
                        if (fila.ContainsKey("numColaboradoresCargados"))
                        {
                            numColaboradoresCargados = fila["numColaboradoresCargados"].value;
                        }
                        ActualizadorTriple(project, "http://w3id.org/roh/collaboratorsNumber", numColaboradoresCargados, numColaboradoresACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/researchersNumber de los http://vivoweb.org/ontology/core#Project
        /// el nº de miembros 
        /// Depende de ActualizadorProject.ActualizarMiembros
        /// </summary>
        /// <param name="pProjects">IDs de los proyectos</param>
        public void ActualizarNumeroMiembros(List<string> pProjects = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/researchersNumber");

            List<string> filters = new List<string>();
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project =<{string.Join(">,<", pProjects)}>)");
            }
            else
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {
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
                                    ?project ?propRol ?rol.
                                    FILTER(?propRol in ( <http://w3id.org/roh/mainResearchers>, <http://w3id.org/roh/researchers>))
                                    ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person                       
                                }}                                
                              }}Group by ?project 
                            }}
                            FILTER(?numMiembrosCargados!= ?numMiembrosACargar OR !BOUND(?numMiembrosCargados) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string project = fila["project"].value;
                        string numMiembrosACargar = fila["numMiembrosACargar"].value;
                        string numMiembrosCargados = "";
                        if (fila.ContainsKey("numMiembrosCargados"))
                        {
                            numMiembrosCargados = fila["numMiembrosCargados"].value;
                        }
                        ActualizadorTriple(project, "http://w3id.org/roh/researchersNumber", numMiembrosCargados, numMiembrosACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/members de los http://vivoweb.org/ontology/core#Project
        /// todos los miembros del proyecto 
        /// Depende de ActualizadorProject.ActualizarMiembros
        /// </summary>
        /// <param name="pProjects">IDs de los proyectos</param>
        public void ActualizarMiembrosUnificados(List<string> pProjects = null)
        {
            List<string> filters = new List<string>();
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project =<{string.Join(">,<", pProjects)}>)");
            }
            else
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {
                //Creamos los miembros
                while (true)
                {
                    int limit = 500;
                    String select = @"select * where{select distinct ?project ?person ";
                    String where = @$"where{{
                                    {filter}
                                    {{                                        
                                        select distinct ?project ?person
                                        Where{{                               
                                            ?project a <http://vivoweb.org/ontology/core#Project>.                                                 
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in ( <http://w3id.org/roh/mainResearchers>, <http://w3id.org/roh/researchers>))
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person    
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?project <http://w3id.org/roh/members> ?person.
                                    }}
                                }}}}order by desc(?project) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");
                    InsercionMultiple(resultado.results.bindings, "http://w3id.org/roh/members", "project", "person");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                //Eliminamos los miembros
                while (true)
                {
                    int limit = 500;
                    String select = @"select * where{select distinct ?project ?person ";
                    String where = @$"where{{
                                    {filter}
                                    {{         
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?project <http://w3id.org/roh/members> ?person.                                                      
                                    }}
                                    MINUS
                                    {{
                                        select distinct ?project ?person
                                        Where{{                               
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in ( <http://w3id.org/roh/mainResearchers>, <http://w3id.org/roh/researchers>))
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person      
                                        }}
                                    }}
                                }}}}order by desc(?project) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");
                    EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/members", "project", "person");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

    }
}
