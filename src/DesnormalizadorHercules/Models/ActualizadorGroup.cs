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
    public class ActualizadorGroup : ActualizadorBase
    {
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
                String select = @"select ?group ?numMiembrosCargados ?numMiembrosACargar  from <http://gnoss.com/person.owl> ";
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
                                        ?group <http://w3id.org/roh/mainResearcher> ?rol.
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
                            }} limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "group");

                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string person = fila["group"].value;
                    string numMiembrosACargar = fila["numMiembrosACargar"].value;
                    string numMiembrosCargados = "";
                    if (fila.ContainsKey("numMiembrosCargados"))
                    {
                        numMiembrosCargados = fila["numMiembrosCargados"].value;
                    }
                    ActualizadorTriple(person, "http://w3id.org/roh/researchersNumber", numMiembrosCargados, numMiembrosACargar);
                }

                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }

        }

    }
}
