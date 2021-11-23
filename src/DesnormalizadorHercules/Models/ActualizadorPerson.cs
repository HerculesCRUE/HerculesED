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
    public class ActualizadorPerson : ActualizadorBase
    {
        public void ActualizarNumeroPublicaciones(string pPerson=null)
        {
            //TODO comentario query
            string filter = "";
            if(!string.IsNullOrEmpty(pPerson))
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
                String select = @"select ?person  ?numDocumentosCargados ?numDocumentosACargar  from <http://gnoss.com/document.owl> from <http://gnoss.com/curriculumvitae.owl> ";
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
                                    ?doc <http://purl.org/ontology/bibo/authorList> ?autores.
                                    ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                    {{
                                           ?cv <http://w3id.org/roh/cvOf>?person.
                                    }}
                                    UNION
                                    {{
                                            MINUS
                                            {{
                                                    ?anycv <http://w3id.org/roh/cvOf>?person.
                                            }}
                                    }}
                                    ?cv a <http://w3id.org/roh/CV>.
                                    ?cv ?lvl1 ?cvlvl2.
                                    ?cvlvl2 ?lvl2 ?cvlvl3.
                                    ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?doc.
                                    ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                }}
                              }}Group by ?person 
                            }}
                            FILTER(?numDocumentosCargados!= ?numDocumentosACargar OR !BOUND(?numDocumentosCargados) )
                            }} limit {limit}";
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

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }

        public void ActualizarNumeroProyectos(string pPerson = null)
        {
            //TODO comentario query
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
                String select = @"select ?person  ?numProyectosCargados ?numProyectosACargar  from <http://gnoss.com/project> ";
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
                                    ?proyecto <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                    ?proyecto <http://vivoweb.org/ontology/core#relates> ?rol.
                                    ?rol <http://w3id.org/roh/roleOf> ?person.
                                }}
                              }}Group by ?person 
                            }}
                            FILTER(?numProyectosCargados!= ?numProyectosACargar OR !BOUND(?numProyectosCargados) )
                            }} limit {limit}";
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

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }

        public void ActualizarPertenenciaGrupos(string pPerson = null)
        {
            //TODO comentario query
            string fechaActual = DateTime.UtcNow.ToString("yyyyMMdd000000");


            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }
            while (true)
            {
                //Añadimos a grupos
                int limit = 500;
                //TODO eliminar from
                String select = @"select distinct ?person  ?group  from <http://gnoss.com/person.owl> from <http://gnoss.com/group.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?person ?group
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.
                                            ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            {{
                                                ?group <http://w3id.org/roh/mainResearcher> ?rol.
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
                                }} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["person"].value;
                    string group = fila["group"].value;
                    ActualizadorTriple(person, "http://vivoweb.org/ontology/core#relates", "", group);
                };



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
                String select = @"select distinct ?person  ?group  from <http://gnoss.com/person.owl> from <http://gnoss.com/group.owl>  ";
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
                                            ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            {{
                                                ?group <http://w3id.org/roh/mainResearcher> ?rol.
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
                                }} limit {limit}";
                var resultado = mResourceApi.VirtuosoQuery(select, where, "group");



                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["person"].value;
                    string group = fila["group"].value;
                    ActualizadorTriple(person, "http://vivoweb.org/ontology/core#relates", group, "");
                }



                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }


        }

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
                String select = @"select distinct ?person  ?linea  from <http://gnoss.com/person.owl> ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?person ?linea
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?group a <http://xmlns.com/foaf/0.1/Group>.                                            
                                            ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            {{
                                                ?group <http://w3id.org/roh/mainResearcher> ?rol.
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
                                }} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["person"].value;
                    string linea = fila["linea"].value;
                    ActualizadorTriple(person, "http://w3id.org/roh/lineResearch", "", linea);
                };



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
                String select = @"select distinct ?person  ?linea  from <http://gnoss.com/person.owl> ";
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
                                            ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            {{
                                                ?group <http://w3id.org/roh/mainResearcher> ?rol.
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
                                }} limit {limit}";
                var resultado = mResourceApi.VirtuosoQuery(select, where, "group");



                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["person"].value;
                    string linea = fila["linea"].value;
                    ActualizadorTriple(person, "http://w3id.org/roh/lineResearch", linea, "");
                }



                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }







            // RemoveTriples t3 = new();
            // t3.Predicate = "http://w3id.org/roh/publicationsNumber";
            // t3.Value = "9";
            // var z = mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { g2, new List<Gnoss.ApiWrapper.Model.RemoveTriples>() { t3 } } });


            //TODO Eliminar duplicados


        }
    }
}
