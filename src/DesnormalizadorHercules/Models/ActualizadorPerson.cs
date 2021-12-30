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
        /// Actualizamos en la propiedad http://w3id.org/roh/isPublic de las http://xmlns.com/foaf/0.1/Person
        /// los personas públicos (son las personas oficiales, es decir, los que tienen http://w3id.org/roh/crisIdentifier)
        /// Esta propiedad se utilizará como filtro en el bucador general de investigadores
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        public void ActualizarPersonasPublicas(string pPerson = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/isPublic");

            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?person ?isPublicCargado ?isPublicACargar ";
                String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            {filter}
                            OPTIONAL
                            {{
                              ?person <http://w3id.org/roh/isPublic> ?isPublicCargado.
                            }}
                            {{
                              select ?person IF(BOUND(?crisIdentifier),'true','false')  as ?isPublicACargar
                              Where{{                               
                                ?person a <http://xmlns.com/foaf/0.1/Person>.
                                OPTIONAL
                                {{
                                    ?person <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                }}                                
                              }}
                            }}
                            FILTER(?isPublicCargado!= ?isPublicACargar OR !BOUND(?isPublicCargado) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["person"].value;
                    string isPublicACargar = fila["isPublicACargar"].value;
                    string isPublicCargado = "";
                    if (fila.ContainsKey("isPublicCargado"))
                    {
                        isPublicCargado = fila["isPublicCargado"].value;
                    }
                    ActualizadorTriple(person, "http://w3id.org/roh/isPublic", isPublicCargado, isPublicACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/publicationsNumber de las http://xmlns.com/foaf/0.1/Person el nº de publicaciones públicas
        /// (publicaciones que apuntan a la personas con la propiedad 'http://w3id.org/roh/publicAuthorList')
        /// Depende de ActualizadorDocument.ActualizarPertenenciaPersonas 
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        public void ActualizarNumeroPublicaciones(string pPerson = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/publicationsNumber");

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
                                    ?doc <http://w3id.org/roh/publicAuthorList> ?person.
                                }}
                              }}Group by ?person 
                            }}
                            FILTER(?numDocumentosCargados!= ?numDocumentosACargar OR !BOUND(?numDocumentosCargados) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["person"].value;
                    string numDocumentosACargar = fila["numDocumentosACargar"].value;
                    string numDocumentosCargados = "";
                    if (fila.ContainsKey("numDocumentosCargados"))
                    {
                        numDocumentosCargados = fila["numDocumentosCargados"].value;
                    }
                    ActualizadorTriple(person, "http://w3id.org/roh/publicationsNumber", numDocumentosCargados, numDocumentosACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/projectsNumber de las http://xmlns.com/foaf/0.1/Person el nº de proyectos públicos
        /// a los que pertenece (proyectos que apuntan a la personas con la propiedad 'http://w3id.org/roh/publicAuthorList')
        /// Depende de ActualizadorProject.ActualizarPertenenciaPersonas
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        public void ActualizarNumeroProyectos(string pPerson = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/projectsNumber");


            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{select ?person  ?numProyectosCargados ?numProyectosACargar  from <http://gnoss.com/project.owl> ";
                String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            ?person <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
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
                                    ?proyecto <http://w3id.org/roh/publicAuthorList> ?person.
                                }}
                              }}Group by ?person 
                            }}
                            FILTER(?numProyectosCargados!= ?numProyectosACargar OR !BOUND(?numProyectosCargados) )
                            }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["person"].value;
                    string numProyectosACargar = fila["numProyectosACargar"].value;
                    string numProyectosCargados = "";
                    if (fila.ContainsKey("numProyectosCargados"))
                    {
                        numProyectosCargados = fila["numProyectosCargados"].value;
                    }
                    ActualizadorTriple(person, "http://w3id.org/roh/projectsNumber", numProyectosCargados, numProyectosACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Insertamos en la propiedad http://vivoweb.org/ontology/core#relates de las http://xmlns.com/foaf/0.1/Person 
        /// los grupos a los que pertenecen en la actualidad
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        /// <param name="pGrupo">ID del grupo</param>
        public void ActualizarPertenenciaGrupos(string pPerson = null, string pGrupo = null)
        {
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");

            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
            if (!string.IsNullOrEmpty(pGrupo))
            {
                filter = $" FILTER(?group =<{pGrupo}>)";
            }
            while (true)
            {
                //Añadimos a grupos
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select distinct ?person  ?group  from <http://gnoss.com/person.owl> from <http://gnoss.com/group.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?person ?group
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                                            {{
                                                ?group <http://w3id.org/roh/mainResearchers> ?rol.
                                                ?rol <http://w3id.org/roh/roleOf> ?person.
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                                BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                                BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                            }}
                                            UNION
                                            {{ 
                                                ?group <http://xmlns.com/foaf/0.1/member> ?rol.
                                                ?rol <http://w3id.org/roh/roleOf> ?person.
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                                BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                                BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                            }}
                                            FILTER(?start<={fechaActual} AND ?end>={fechaActual} )
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?person <http://vivoweb.org/ontology/core#relates> ?group.
                                    }}
                                }}}} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                InsercionMultiple(resultado.results.bindings, "http://vivoweb.org/ontology/core#relates", "person", "group");
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
                String select = @"select * where{ select distinct ?person  ?group  from <http://gnoss.com/person.owl> from <http://gnoss.com/group.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?person <http://vivoweb.org/ontology/core#relates> ?group.                                        
                                    }}
                                    MINUS
                                    {{
                                        select distinct ?person ?group
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                                            {{
                                                ?group <http://w3id.org/roh/mainResearchers> ?rol.
                                                ?rol <http://w3id.org/roh/roleOf> ?person.
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                                BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                                BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                            }}
                                            UNION
                                            {{ 
                                                ?group <http://xmlns.com/foaf/0.1/member> ?rol.
                                                ?rol <http://w3id.org/roh/roleOf> ?person.
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#start> ?startAux.}}
                                                OPTIONAL{{?rol <http://vivoweb.org/ontology/core#end> ?endAux.}}
                                                BIND(IF(BOUND(?endAux),xsd:integer(?endAux) ,30000000000000)  as ?end)
                                                BIND(IF(BOUND(?startAux),xsd:integer(?startAux),10000000000000)  as ?start)
                                            }}
                                            FILTER(?start<={fechaActual} AND ?end>={fechaActual} )
                                        }}
                                    }}
                                }}}} limit {limit}";
                var resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                EliminacionMultiple(resultado.results.bindings, "http://vivoweb.org/ontology/core#relates", "person", "group");
                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }


        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/lineResearch de las http://xmlns.com/foaf/0.1/Person  
        /// las líneas de investigación activas
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        public void ActualizarPertenenciaLineas(string pPerson = null)
        {
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");
            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
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
                                }}}}order by desc(?person) limit {limit}";
                var resultado = mResourceApi.VirtuosoQuery(select, where, "group");
                EliminacionMultiple(resultado.results.bindings, "http://w3id.org/roh/lineResearch", "person", "linea");
                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }

        }

        /// <summary>
        /// Actualizamos en la propiedad http://vivoweb.org/ontology/core#hasResearchArea de las http://xmlns.com/foaf/0.1/Person 
        /// las áreas en función de sus publicaciones públicas (a no ser que las haya editado manualmente)
        /// Depende de ActualizadorDocument.ActualizarPertenenciaPersonas
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        public void ActualizarAreasPersonas(string pPerson = null)
        {
            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pPerson))
                {
                    filter = $" FILTER(?person =<{pPerson}>)";
                }

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

                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
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
                        List<RemoveTriples> triplesRemove = new ();
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
                    String select = @"select distinct * where{select ?person ?categoryNode from <http://gnoss.com/document.owl> from <http://gnoss.com/taxonomy.owl>";
                    String where = @$"where{{
                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                            ?person <http://w3id.org/roh/hasResearchAreaEdited> 'false'.
                            {filter}
                            {{
                                select  distinct ?person ?hasKnowledgeAreaDocument ?categoryNode where{{
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?document <http://w3id.org/roh/publicAuthorList> ?person.                                    
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
                    InsertarCategorias(resultado, dicAreasBroader, graphsUrl,"person", "http://vivoweb.org/ontology/core#hasResearchArea");
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
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?document <http://w3id.org/roh/publicAuthorList> ?person.                                    
                                    ?document  <http://w3id.org/roh/hasKnowledgeArea> ?hasKnowledgeAreaDocument.
                                    ?hasKnowledgeAreaDocument <http://w3id.org/roh/categoryNode> ?categoryNode.
                                    MINUS{{
                                        ?categoryNode <http://www.w3.org/2008/05/skos#narrower> ?hijo.
                                    }}
                                }}
                                 
                            }}
                            }}}}order by (?person) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");
                    EliminarCategorias(resultado,"person", "http://vivoweb.org/ontology/core#hasResearchArea");
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
        /// Depende de ActualizadorDocument.ActualizarPertenenciaPersonas
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        public void ActualizarNumeroAreasTematicas(string pPerson = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/themedAreasNumber");


            //Actualizamos los datos
            while (true)
            {
                int limit = 500;
                //TODO eliminar from
                String select = @"select * where{ select ?person  ?numAreasTematicasCargadas ?numAreasTematicasACargar  from <http://gnoss.com/document.owl>  from <http://gnoss.com/taxonomy.owl>  ";
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
                                    ?documento <http://w3id.org/roh/publicAuthorList> ?person.
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

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["person"].value;
                    string numAreasTematicasACargar = fila["numAreasTematicasACargar"].value;
                    string numAreasTematicasCargadas = "";
                    if (fila.ContainsKey("numAreasTematicasCargadas"))
                    {
                        numAreasTematicasCargadas = fila["numAreasTematicasCargadas"].value;
                    }
                    ActualizadorTriple(person, "http://w3id.org/roh/themedAreasNumber", numAreasTematicasCargadas, numAreasTematicasACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/collaboratorsNumber de las http://xmlns.com/foaf/0.1/Person 
        /// el nº de colaboradores (personas con coautorías en publicaciones publicas y personas con coautorias en proyectos públicos)
        /// Depende de ActualizadorDocument.ActualizarPertenenciaPersonas y ActualizadorProject.ActualizarPertenenciaPersonas
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        public void ActualizarNumeroColaboradores(string pPersona = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pPersona))
            {
                filter = $" FILTER(?person =<{pPersona}>)";
            }
            //Eliminamos los duplicados
            EliminarDuplicados("person", "http://xmlns.com/foaf/0.1/Person", "http://w3id.org/roh/collaboratorsNumber");

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
                              ?person <http://w3id.org/roh/collaboratorsNumber> ?numColaboradoresCargadosAux. 
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
					                                    ?documento <http://w3id.org/roh/publicAuthorList> ?person.
					                                    ?documento a <http://purl.org/ontology/bibo/Document>.
					                                    ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores.
					                                    ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?collaborator.
				                                    }}
			                                    }} 
			                                    UNION 
			                                    {{
				                                    #Proyectos
				                                    SELECT *
				                                    WHERE {{
					                                    ?proy <http://w3id.org/roh/publicAuthorList> ?person.
					                                    ?proy a <http://vivoweb.org/ontology/core#Project>.
					                                    ?proy ?propRol ?role.
					                                    FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))
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

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["person"].value;
                    string numColaboradoresACargar = fila["numColaboradoresACargar"].value;
                    string numColaboradoresCargados = "";
                    if (fila.ContainsKey("numColaboradoresCargados"))
                    {
                        numColaboradoresCargados = fila["numColaboradoresCargados"].value;
                    }
                    ActualizadorTriple(person, "http://w3id.org/roh/collaboratorsNumber", numColaboradoresCargados, numColaboradoresACargar);
                }

                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }

        }



        public void EliminarPersonasNoReferenciadas()
        {
            //TODO
            return;
            //string graphsUrl = mResourceApi.GraphsUrl;
            //if (!string.IsNullOrEmpty(graphsUrl))
            //{
            //    //Eliminamos las persona no referenciadas
            //    while (true)
            //    {
            //        int limit = 500;
            //        //TODO from
            //        String select = @"select ?person from <http://gnoss.com/document.owl> ";
            //        String where = @$"where{{
            //                    select distinct ?person
            //                    where{{
            //                        ?person a <http://xmlns.com/foaf/0.1/Person>.
            //                        MINUS{{
            //                            ?person <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
            //                        }}
            //                        MINUS{{
            //                             ?doc <http://purl.org/ontology/bibo/authorList> ?autores.
            //                            ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
            //                        }}
            //                   }}
            //                }}limit {limit}";
            //        SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

            //        foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            //        {

            //        }


            //        if (resultado.results.bindings.Count != limit)
            //        {
            //            break;
            //        }
            //    }

            //}
        }

    }
}
