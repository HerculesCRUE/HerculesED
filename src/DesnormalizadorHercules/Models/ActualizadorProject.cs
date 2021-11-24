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
    class ActualizadorProject : ActualizadorBase
    {
        public void ActualizarPertenenciaPersonas(string pPerson = null,string pProject=null)
        {
            //TODO comentario query

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
                String select = @"select distinct ?document  ?project  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?document ?project
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://vivoweb.org/ontology/core#relates> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
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
                                            ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                            ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                            
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?document <http://w3id.org/roh/publicAuthorList> ?person.
                                    }}
                                }} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string document = fila["document"].value;
                    string project = fila["project"].value;
                    
                    ActualizadorTriple(document, "http://w3id.org/roh/publicAuthorList", "", project);
                };



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
                String select = @"select distinct ?document  ?project  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl>  ";
                String where = @$"where{{
                                    {filter}
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?document <http://w3id.org/roh/publicAuthorList> ?person.
                                    }}                                    
                                    MINUS
                                    {{
                                        select distinct ?document ?project
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://vivoweb.org/ontology/core#relates> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
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
                                            ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                            ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                            
                                        }}
                                    }}
                                    
                                }} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "project");



                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string project = fila["project"].value;
                    string person = fila["person"].value;

                    ActualizadorTriple(project, "http://w3id.org/roh/publicAuthorList", person, "");
                };



                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }


        }
    }
}
