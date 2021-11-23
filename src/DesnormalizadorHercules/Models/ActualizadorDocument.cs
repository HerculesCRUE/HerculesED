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
    class ActualizadorDocument : ActualizadorBase
    {
        public void ActualizarPertenenciaPersonas(string pPerson = null,string pDocument=null)
        {
            //TODO comentario query
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
                String select = @"select distinct ?document  ?person  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl>  ";
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
                                            ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                            ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                            
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document <http://w3id.org/roh/publicAuthorList> ?person.
                                    }}
                                }} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string document = fila["document"].value;
                    string person = fila["person"].value;
                    
                    ActualizadorTriple(document, "http://w3id.org/roh/publicAuthorList", "", person);
                };



                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
            
            while (true)
            {
                //Eliminamos de dcumentpos
                int limit = 500;
                //TODO eliminar from
                String select = @"select distinct ?document  ?person  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl>  ";
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
                                            ?cvlvl3 <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                            ?cvlvl3  <http://w3id.org/roh/isPublic> 'true'.
                                            
                                        }}
                                    }}
                                }} limit {limit}";
                var resultado = mResourceApi.VirtuosoQuery(select, where, "document");



                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string document = fila["document"].value;
                    string person = fila["person"].value;

                    ActualizadorTriple(document, "http://w3id.org/roh/publicAuthorList", person, "");
                };



                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }


        }
    }
}
