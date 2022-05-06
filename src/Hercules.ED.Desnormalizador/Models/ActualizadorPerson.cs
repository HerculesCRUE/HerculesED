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
    /// <summary>
    /// Clase para actualizar propiedades de personas
    /// </summary>
    public class ActualizadorPerson : ActualizadorBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pResourceApi">API Wrapper de GNOSS</param>
        public ActualizadorPerson(ResourceApi pResourceApi) : base(pResourceApi)
        {
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/publicationsNumber de las http://xmlns.com/foaf/0.1/Person el nº de publicaciones validadas
        /// Depende de ActualizadorDocument.ActualizarDocumentosValidados
        /// </summary>
        /// <param name="pPersons">IDs de las personas</param>
        public void ActualizarNumeroPublicacionesValidadas(List<string> pPersons = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/publicationsNumber");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
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
                    String select = @"select * where{select ?person  ?numDocumentosCargados ?numDocumentosACargar  from <http://gnoss.com/document.owl> from <http://gnoss.com/curriculumvitae.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/publicationsNumber> ?numDocumentosCargadosAux. 
                              BIND(xsd:int( ?numDocumentosCargadosAux) as  ?numDocumentosCargados)
                            }}
                            {{
                              select ?person count(distinct ?doc) as ?numDocumentosACargar
                              Where{{
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL{{
                                    ?doc a <http://purl.org/ontology/bibo/Document>.
                                    ?doc <http://w3id.org/roh/isValidated> 'true'.
                                    ?doc <http://purl.org/ontology/bibo/authorList> ?listaAutores.
					                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                }}
                              }}Group by ?person 
                            }}
                            FILTER(?numDocumentosCargados!= ?numDocumentosACargar OR !BOUND(?numDocumentosCargados) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string numDocumentosACargar = fila["numDocumentosACargar"].value;
                        string numDocumentosCargados = "";
                        if (fila.ContainsKey("numDocumentosCargados"))
                        {
                            numDocumentosCargados = fila["numDocumentosCargados"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/publicationsNumber", numDocumentosCargados, numDocumentosACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/publicPublicationsNumber de las http://xmlns.com/foaf/0.1/Person el nº de publicaciones publicas (validadas + publicas en su cv)
        /// Depende de ActualizadorCV.ModificarDocumentos y actualizadorCV.CambiarPrivacidadDocumentos
        /// </summary>
        /// <param name="pPersons">IDs de las personas</param>
        public void ActualizarNumeroPublicacionesPublicas(List<string> pPersons = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/publicPublicationsNumber");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
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
                    String select = @"select * where{select ?person  ?numDocumentosCargados ?numDocumentosACargar  from <http://gnoss.com/document.owl> from <http://gnoss.com/curriculumvitae.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/publicPublicationsNumber> ?numDocumentosCargadosAux. 
                              BIND(xsd:int( ?numDocumentosCargadosAux) as  ?numDocumentosCargados)
                            }}
                            {{
                              select ?person count(distinct ?doc) as ?numDocumentosACargar
                              Where{{
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                {{
			                        ?doc a <http://purl.org/ontology/bibo/Document>.
			                        ?cv <http://w3id.org/roh/cvOf> ?person.
			                        ?cv  <http://w3id.org/roh/scientificActivity> ?scientificActivity.
			                        ?scientificActivity ?pAux ?oAux.
			                        ?oAux <http://w3id.org/roh/isPublic> 'true'.
			                        ?oAux <http://vivoweb.org/ontology/core#relatedBy> ?doc
		                        }}UNION
		                        {{
			                        ?doc a <http://purl.org/ontology/bibo/Document>.
			                        ?doc <http://w3id.org/roh/isValidated> 'true'.
			                        ?doc <http://purl.org/ontology/bibo/authorList> ?listaAutores.
			                        ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
		                        }}
                              }}Group by ?person 
                            }}
                            FILTER(?numDocumentosCargados!= ?numDocumentosACargar OR !BOUND(?numDocumentosCargados) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string numDocumentosACargar = fila["numDocumentosACargar"].value;
                        string numDocumentosCargados = "";
                        if (fila.ContainsKey("numDocumentosCargados"))
                        {
                            numDocumentosCargados = fila["numDocumentosCargados"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/publicPublicationsNumber", numDocumentosCargados, numDocumentosACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/ipNumber de las http://xmlns.com/foaf/0.1/Person el nº de proyectos en los que ha sido IP
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPersons">IDs de las personas</param>
        public void ActualizarNumeroIPProyectos(List<string> pPersons = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/ipNumber");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
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
                    String select = @"select * where{select ?person  ?numIPCargados ?numIPACargar  from <http://gnoss.com/project.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/ipNumber> ?numIPCargadosAux. 
                              BIND(xsd:int( ?numIPCargadosAux) as  ?numIPCargados)
                            }}
                            {{
                              select ?person count(distinct ?proy) as ?numIPACargar
                              Where{{
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL{{
			                        ?proy a <http://vivoweb.org/ontology/core#Project>.
			                        ?proy <http://w3id.org/roh/isValidated> 'true'.
                                    ?proy <http://vivoweb.org/ontology/core#relates> ?listprojauth.
                                    ?listprojauth <http://w3id.org/roh/roleOf> ?person.
                                    ?listprojauth <http://w3id.org/roh/isIP> 'true'.
		                        }}
                              }}Group by ?person 
                            }}
                            FILTER(?numIPCargados!= ?numIPACargar OR !BOUND(?numIPCargados) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string numIPACargar = fila["numIPACargar"].value;
                        string numIPCargados = "";
                        if (fila.ContainsKey("numIPCargados"))
                        {
                            numIPCargados = fila["numIPCargados"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/ipNumber", numIPCargados, numIPACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/projectsNumber de las http://xmlns.com/foaf/0.1/Person el nº de proyectos validados
        /// Depende de ActualizadorProject.ActualizarMiembros
        /// </summary>
        /// <param name="pPersons">IDs de las personas</param>
        public void ActualizarNumeroProyectosValidados(List<string> pPersons = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/projectsNumber");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
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
                    String select = @"select * where{select ?person  ?numProyectosCargados ?numProyectosACargar  from <http://gnoss.com/project.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/projectsNumber> ?numProyectosCargadosAux. 
                              BIND(xsd:int( ?numProyectosCargadosAux) as  ?numProyectosCargados)
                            }}
                            {{
                              select ?person count(distinct ?proyecto) as ?numProyectosACargar
                              Where{{
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL{{
                                    ?proyecto a <http://vivoweb.org/ontology/core#Project>.
                                    ?proyecto <http://w3id.org/roh/isValidated> 'true'.
                                    {{
                                        ?proyecto <http://w3id.org/roh/mainResearchers> ?main.
				                        ?main rdf:member ?person .
			                        }}UNION
			                        {{

                                        ?proyecto <http://w3id.org/roh/researchers> ?main.
				                        ?main rdf:member ?person .
			                        }}
                                }}
                              }}Group by ?person 
                            }}
                            FILTER(?numProyectosCargados!= ?numProyectosACargar OR !BOUND(?numProyectosCargados) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string numProyectosACargar = fila["numProyectosACargar"].value;
                        string numProyectosCargados = "";
                        if (fila.ContainsKey("numProyectosCargados"))
                        {
                            numProyectosCargados = fila["numProyectosCargados"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/projectsNumber", numProyectosCargados, numProyectosACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/projectsNumber de las http://xmlns.com/foaf/0.1/Person el nº de proyectos públicos
        /// Depende de ActualizadorProject.ActualizarMiembros y ActualizadorCV.ModificarProyectos();
        /// </summary>
        /// <param name="pPersons">IDs de las personas</param>
        public void ActualizarNumeroProyectosPublicos(List<string> pPersons = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/publicProjectsNumber");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
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
                    String select = @"select * where{select ?person  ?numProyectosCargados ?numProyectosACargar  from <http://gnoss.com/project.owl> from <http://gnoss.com/curriculumvitae.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/publicProjectsNumber> ?numProyectosCargadosAux. 
                              BIND(xsd:int( ?numProyectosCargadosAux) as  ?numProyectosCargados)
                            }}
                            {{
                              select ?person count(distinct ?proyecto) as ?numProyectosACargar
                              Where{{
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL{{
                                    ?proyecto a <http://vivoweb.org/ontology/core#Project>.
                                    {{
			                            ?cv <http://w3id.org/roh/cvOf> ?person.
			                            ?cv  <http://w3id.org/roh/scientificExperience> ?scientificExperience.
			                            ?scientificExperience ?pAux ?oAux.
			                            ?oAux <http://w3id.org/roh/isPublic> 'true'.
			                            ?oAux <http://vivoweb.org/ontology/core#relatedBy> ?proyecto
		                            }}UNION
                                    {{
                                        ?proyecto <http://w3id.org/roh/isValidated> 'true'.
                                        ?proyecto ?propMember ?main.
                                        FILTER(?propMember in (<http://w3id.org/roh/mainResearchers>,<http://w3id.org/roh/researchers>))
                                        ?main rdf:member ?person .
                                    }}
                                }}
                              }}Group by ?person 
                            }}
                            FILTER(?numProyectosCargados!= ?numProyectosACargar OR !BOUND(?numProyectosCargados) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string numProyectosACargar = fila["numProyectosACargar"].value;
                        string numProyectosCargados = "";
                        if (fila.ContainsKey("numProyectosCargados"))
                        {
                            numProyectosCargados = fila["numProyectosCargados"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/publicProjectsNumber", numProyectosCargados, numProyectosACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/lineResearch de las http://xmlns.com/foaf/0.1/Person  
        /// las líneas de investigación activas
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPersons">IDs de las personas</param>
        public void ActualizarPertenenciaLineas(List<string> pPersons = null)
        {
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Añadimos líneas
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"select * where{ select distinct ?person  ?linea  from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?person ?linea
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.     
                                            {{
                                                ?group <http://vivoweb.org/ontology/core#relates> ?rol.
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
                                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                                ?person <http://w3id.org/roh/lineResearch> ?linea. 
                                            }}
                                        }}
                                    }}                                    
                                }}}}order by desc(?person) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                    InsercionMultiple(resultado.results.bindings, "http://w3id.org/roh/lineResearch", "person", "linea");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Eliminamos líneas
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"select * where{select distinct ?person  ?linea  from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <http://w3id.org/roh/lineResearch> ?linea.                                        
                                    }}
                                    MINUS
                                    {{
                                        select distinct ?person ?linea
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.                   
                                            {{
                                                ?group <http://vivoweb.org/ontology/core#relates> ?rol.
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
                                }}}}order by desc(?person) limit {limit}";
                    var resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                    EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/lineResearch", "person", "linea");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://vivoweb.org/ontology/core#hasResearchArea de las http://xmlns.com/foaf/0.1/Person 
        /// las áreas en función de sus publicaciones validadas (a no ser que las haya editado manualmente)
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPersons">ID de las personas</param>
        public void ActualizarAreasPersonas(List<string> pPersons = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else
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
                    String select = @"select ?person ?categoryNode from <http://gnoss.com/taxonomy.owl> ";
                    String where = @$"where{{
                                select distinct ?person ?hasKnowledgeAreaAux  ?categoryNode
                                where{{
                                    {filter}
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?person <http://vivoweb.org/ontology/core#hasResearchArea> ?hasKnowledgeAreaAux .
                                    ?hasKnowledgeAreaAux <http://w3id.org/roh/categoryNode> ?categoryNode.
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                               }}
                            }}group by ?person ?categoryNode HAVING (COUNT(*) > 1) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string categoryNode = fila["categoryNode"].value;
                        //TODO from
                        select = @"select ?person ?hasKnowledgeArea   ?categoryNode from <http://gnoss.com/taxonomy.owl>";
                        where = @$"where{{
                                    FILTER(?person=<{person}>)
                                    FILTER(?categoryNode =<{categoryNode}>)
                                    {{ 
                                        select distinct ?person ?hasKnowledgeArea  ?categoryNode
                                        where{{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?person <http://vivoweb.org/ontology/core#hasResearchArea> ?hasKnowledgeArea.
                                            ?hasKnowledgeArea <http://w3id.org/roh/categoryNode> ?categoryNode.
                                            MINUS{{
                                                ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                            }}
                                        }}
                                    }}
                                }}";
                        resultado = mResourceApi.VirtuosoQuery(select, where, "person");
                        List<RemoveTriples> triplesRemove = new();
                        foreach (string hasKnowledgeArea in resultado.results.bindings.GetRange(1, resultado.results.bindings.Count - 1).Select(x => x["hasKnowledgeArea"].value).ToList())
                        {
                            triplesRemove.Add(new RemoveTriples()
                            {
                                Predicate = "http://vivoweb.org/ontology/core#hasResearchArea",
                                Value = hasKnowledgeArea
                            }); ;
                        }
                        if (triplesRemove.Count > 0)
                        {
                            var resultadox = mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { mResourceApi.GetShortGuid(person), triplesRemove } });
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
                    String select = @"select distinct * where{select ?person ?categoryNode from <http://gnoss.com/document.owl> from <http://gnoss.com/taxonomy.owl>";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            {{
                                select  distinct ?person ?hasKnowledgeAreaDocument ?categoryNode where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    ?document <http://w3id.org/roh/isValidated> 'true'.
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?document <http://purl.org/ontology/bibo/authorList> ?autores.
                                    ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                    ?document  <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeAreaDocument.
                                    ?hasKnowledgeAreaDocument <http://w3id.org/roh/categoryNode> ?categoryNode.
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                                }}
                            }}
                            MINUS{{
                                select distinct ?person ?hasKnowledgeAreaPerson ?categoryNode 
                                where{{
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?person <http://vivoweb.org/ontology/core#hasResearchArea> ?hasKnowledgeAreaPerson.
                                    ?hasKnowledgeAreaPerson <http://w3id.org/roh/categoryNode> ?categoryNode
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                                }}
                            }}
                            }}}}order by (?person) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");
                    InsertarCategorias(resultado, dicAreasBroader, mResourceApi.GraphsUrl, "person", "http://vivoweb.org/ontology/core#hasResearchArea");
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
                    String select = @"select distinct * where{select ?person ?hasKnowledgeArea from <http://gnoss.com/document.owl> from <http://gnoss.com/taxonomy.owl>";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            ?person <http://w3id.org/roh/hasResearchAreaEdited> 'false'.
                            {filter}
                            {{
                                select distinct ?person ?hasKnowledgeArea ?categoryNode 
                                where{{
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?person <http://vivoweb.org/ontology/core#hasResearchArea> ?hasKnowledgeArea.
                                    ?hasKnowledgeArea <http://w3id.org/roh/categoryNode> ?categoryNode
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                                }}                                
                            }}
                            MINUS{{
                                select  distinct ?person ?hasKnowledgeAreaDocument ?categoryNode where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    ?document <http://w3id.org/roh/isValidated> 'true'.
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?document <http://purl.org/ontology/bibo/authorList> ?autores.
                                    ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                    ?document  <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeAreaDocument.
                                    ?hasKnowledgeAreaDocument <http://w3id.org/roh/categoryNode> ?categoryNode.
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                                }}
                                 
                            }}
                            }}}}order by (?person) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");
                    EliminarCategorias(resultado, "person", "http://vivoweb.org/ontology/core#hasResearchArea");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/themedAreasNumber de las http://xmlns.com/foaf/0.1/Person 
        /// el número de áreas temáticas de sus publicaciones públicas
        /// Depende de ActualizadorCV.ModificarDocumentos y actualizadorCV.CambiarPrivacidadDocumentos
        /// </summary>
        /// <param name="pPersons">IDs de la personas</param>
        public void ActualizarNumeroAreasTematicas(List<string> pPersons = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/themedAreasNumber");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
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
                    String select = @"select * where{ select ?person  ?numAreasTematicasCargadas ?numAreasTematicasACargar  from <http://gnoss.com/document.owl> from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/taxonomy.owl>  ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/themedAreasNumber> ?numAreasTematicasCargadasAux. 
                              BIND(xsd:int( ?numAreasTematicasCargadasAux) as  ?numAreasTematicasCargadas)
                            }}
                            {{
                              select ?person count(distinct ?categoria) as ?numAreasTematicasACargar
                              Where{{
                                ?person a <http://xmlns.com/foaf/0.1/Person>.                                
                                OPTIONAL{{
                                    ?documento a <http://purl.org/ontology/bibo/Document>. 
                                    {{
                                        ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores.
					                    ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                        ?documento <http://w3id.org/roh/isValidated> 'true'.
                                    }}UNION
                                    {{
			                            ?cv <http://w3id.org/roh/cvOf> ?person.
			                            ?cv  <http://w3id.org/roh/scientificActivity> ?scientificActivity.
			                            ?scientificActivity ?pAux ?oAux.
			                            ?oAux <http://w3id.org/roh/isPublic> 'true'.
			                            ?oAux <http://vivoweb.org/ontology/core#relatedBy> ?documento
		                            }}
                                    ?documento <http://w3id.org/roh/hasKnowledgeArea> ?area.
                                    ?area <http://w3id.org/roh/categoryNode> ?categoria.
                                    ?categoria <http://www.w3.org/2008/05/skos#prefLabel> ?nombreCategoria.
                                    MINUS
                                    {{
                                        ?categoria <http://www.w3.org/2008/05/skos#narrower> ?hijos
                                    }}
                                }}
                              }}Group by ?person 
                            }}
                            FILTER(?numAreasTematicasCargadas!= ?numAreasTematicasACargar OR !BOUND(?numAreasTematicasCargadas) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string numAreasTematicasACargar = fila["numAreasTematicasACargar"].value;
                        string numAreasTematicasCargadas = "";
                        if (fila.ContainsKey("numAreasTematicasCargadas"))
                        {
                            numAreasTematicasCargadas = fila["numAreasTematicasCargadas"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/themedAreasNumber", numAreasTematicasCargadas, numAreasTematicasACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/collaboratorsNumber de las http://xmlns.com/foaf/0.1/Person 
        /// el nº de colaboradores (personas con coautorías en publicaciones públicos y personas con coautorias en proyectos públicos)
        /// Depende de ActualizadorProject.ActualizarMiembros, ActualizadorCV.ModificarProyectos, ActualizadorCV.ModificarDocumentos y actualizadorCV.CambiarPrivacidadDocumentos
        /// </summary>
        /// <param name="pPersons">ID de las personas</param>
        public void ActualizarNumeroColaboradoresPublicos(List<string> pPersons = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/publicCollaboratorsNumber");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
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
                    String select = @"select * where{ select ?person ?numColaboradoresCargados ?numColaboradoresACargar  from <http://gnoss.com/project.owl> from <http://gnoss.com/document.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/publicCollaboratorsNumber> ?numColaboradoresCargadosAux. 
                              BIND(xsd:int( ?numColaboradoresCargadosAux) as  ?numColaboradoresCargados)
                            }}
                            {{
                              select ?person count(distinct ?collaborator) as ?numColaboradoresACargar
                              Where{{                               
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL
                                {{
                                    {{
	                                    SELECT DISTINCT ?collaborator ?person
	                                    WHERE 
	                                    {{	
                                            ?collaborator a <http://xmlns.com/foaf/0.1/Person>
		                                    {{
			                                    {{
				                                    #Documentos
				                                    SELECT *
				                                    WHERE {{                                                        
			                                            ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutoresActual.
			                                            ?listaAutoresActual <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
					                                    ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores.
					                                    ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?collaborator.
				                                    }}
			                                    }} 
			                                    UNION 
			                                    {{
				                                    #Proyectos
				                                    SELECT *
				                                    WHERE {{
                                                        ?proy a <http://vivoweb.org/ontology/core#Project>.
                                                        ?proy ?propRolActual ?roleActual.
                                                        FILTER(?propRolActual in (<http://w3id.org/roh/mainResearchers>,<http://w3id.org/roh/researchers>))
                                                        ?roleActual rdf:member ?person .                       
					                                    ?proy ?propRol ?role.
					                                    FILTER(?propRol in (<http://w3id.org/roh/researchers>,<http://w3id.org/roh/mainResearchers>))
					                                    ?role <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?collaborator.
				                                    }}
			                                    }}
		                                    }}		
		                                    FILTER(?collaborator!=?person)
	                                    }}
                                    }}
                                }}                                
                              }}Group by ?person 
                            }}
                            FILTER(?numColaboradoresCargados!= ?numColaboradoresACargar OR !BOUND(?numColaboradoresCargados) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");
                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string numColaboradoresACargar = fila["numColaboradoresACargar"].value;
                        string numColaboradoresCargados = "";
                        if (fila.ContainsKey("numColaboradoresCargados"))
                        {
                            numColaboradoresCargados = fila["numColaboradoresCargados"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/publicCollaboratorsNumber", numColaboradoresCargados, numColaboradoresACargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/isIPGroupActually de las http://xmlns.com/foaf/0.1/Person 
        /// si la persona es actualmente IP de algún grupo
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPersons">ID de las personas</param>
        /// <param name="pGroups">ID de los grupos</param>
        public void ActualizarIPGruposActuales(List<string> pPersons = null, List<string> pGroups = null)
        {
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");

            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/isIPGroupActually");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else if (pGroups != null && pGroups.Count > 0)
            {
                filters.Add($" FILTER(?group in (<{string.Join(">,<", pGroups)}>))");
            }else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"select * where{ select ?person ?datoActual ?datoCargar  from <http://gnoss.com/group.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/isIPGroupActually> ?datoActual. 
                            }}
                            {{
                              select ?person IF(BOUND(?group),'true','false') as ?datoCargar
                              Where{{                               
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL
                                {{
                                    ?group a <http://xmlns.com/foaf/0.1/Group>.
                                    ?group <http://vivoweb.org/ontology/core#relates> ?member.
                                    ?member <http://w3id.org/roh/roleOf> ?person.
                                    ?member <http://w3id.org/roh/isIP> 'true'.
                                    OPTIONAL{{?member <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                    OPTIONAL{{?member <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}
                                    BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?fechaPersonaEndAux)
                                    BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?fechaPersonaInitAux)
                                    FILTER(?fechaPersonaInitAux<={fechaActual} AND ?fechaPersonaEndAux>={fechaActual})
                                }}                                
                              }}
                            }}
                            FILTER(?datoActual!= ?datoCargar OR !BOUND(?datoActual) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");
                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string datoCargar = fila["datoCargar"].value;
                        string datoActual = "";
                        if (fila.ContainsKey("datoActual"))
                        {
                            datoActual = fila["datoActual"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/isIPGroupActually", datoActual, datoCargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/isIPGroupHistorically de las http://xmlns.com/foaf/0.1/Person 
        /// si la persona ha sido IP de algún grupo
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPersons">ID de las personas</param>
        /// <param name="pGroups">ID de los grupos</param>
        public void ActualizarIPGruposHistoricos(List<string> pPersons = null, List<string> pGroups = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/isIPGroupHistorically");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else if (pGroups != null && pGroups.Count > 0)
            {
                filters.Add($" FILTER(?group in (<{string.Join(">,<", pGroups)}>))");
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
                    String select = @"select * where{ select ?person ?datoActual ?datoCargar  from <http://gnoss.com/group.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/isIPGroupHistorically> ?datoActual. 
                            }}
                            {{
                              select ?person IF(BOUND(?group),'true','false') as ?datoCargar
                              Where{{                               
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL
                                {{
                                    ?group a <http://xmlns.com/foaf/0.1/Group>.
                                    ?group <http://vivoweb.org/ontology/core#relates> ?member.
                                    ?member <http://w3id.org/roh/roleOf> ?person.
                                    ?member <http://w3id.org/roh/isIP> 'true'.
                                }}                                
                              }}
                            }}
                            FILTER(?datoActual!= ?datoCargar OR !BOUND(?datoActual) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");
                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string datoCargar = fila["datoCargar"].value;
                        string datoActual = "";
                        if (fila.ContainsKey("datoActual"))
                        {
                            datoActual = fila["datoActual"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/isIPGroupHistorically", datoActual, datoCargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/isIPProjectActually de las http://xmlns.com/foaf/0.1/Person 
        /// si la persona es actualmente IP de algún proyecto
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPersons">ID de las personas</param>
        /// <param name="pProjects">ID de los proyectos</param>
        public void ActualizarIPProyectosActuales(List<string> pPersons = null, List<string> pProjects = null)
        {
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");

            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/isIPProjectActually");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project in (<{string.Join(">,<", pProjects)}>))");
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
                    String select = @"select * where{ select ?person ?datoActual ?datoCargar  from <http://gnoss.com/project.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/isIPProjectActually> ?datoActual. 
                            }}
                            {{
                              select ?person IF(BOUND(?project),'true','false') as ?datoCargar
                              Where{{                               
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL
                                {{
                                    ?project a <http://vivoweb.org/ontology/core#Project>.
                                    ?project <http://vivoweb.org/ontology/core#relates> ?member.
                                    ?member <http://w3id.org/roh/roleOf> ?person.
                                    ?member <http://w3id.org/roh/isIP> 'true'.
                                    OPTIONAL{{?member <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                    OPTIONAL{{?member <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}
                                    BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?fechaPersonaEndAux)
                                    BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?fechaPersonaInitAux)
                                    FILTER(?fechaPersonaInitAux<={fechaActual} AND ?fechaPersonaEndAux>={fechaActual})
                                }}                                
                              }}
                            }}
                            FILTER(?datoActual!= ?datoCargar OR !BOUND(?datoActual) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");
                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string datoCargar = fila["datoCargar"].value;
                        string datoActual = "";
                        if (fila.ContainsKey("datoActual"))
                        {
                            datoActual = fila["datoActual"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/isIPProjectActually", datoActual, datoCargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/isIPProjectHistorically de las http://xmlns.com/foaf/0.1/Person 
        /// si la persona ha sido IP de algún grupo
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPersons">ID de las personas</param>
        /// <param name="pProjects">ID de los proyectos</param>
        public void ActualizarIPProyectosHistoricos(List<string> pPersons = null, List<string> pProjects = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/isIPProjectHistorically");

            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project in (<{string.Join(">,<", pProjects)}>))");
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
                    String select = @"select * where{ select ?person ?datoActual ?datoCargar  from <http://gnoss.com/project.owl> ";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/isIPProjectHistorically> ?datoActual. 
                            }}
                            {{
                              select ?person IF(BOUND(?project),'true','false') as ?datoCargar
                              Where{{                               
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL
                                {{
                                    ?project a <http://vivoweb.org/ontology/core#Project>.
                                    ?project <http://vivoweb.org/ontology/core#relates> ?member.
                                    ?member <http://w3id.org/roh/roleOf> ?person.
                                    ?member <http://w3id.org/roh/isIP> 'true'.
                                }}                                
                              }}
                            }}
                            FILTER(?datoActual!= ?datoCargar OR !BOUND(?datoActual) )
                            }}}} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");
                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string person = fila["person"].value;
                        string datoCargar = fila["datoCargar"].value;
                        string datoActual = "";
                        if (fila.ContainsKey("datoActual"))
                        {
                            datoActual = fila["datoActual"].value;
                        }
                        ActualizadorTriple(person, "http://w3id.org/roh/isIPProjectHistorically", datoActual, datoCargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

    }
}
