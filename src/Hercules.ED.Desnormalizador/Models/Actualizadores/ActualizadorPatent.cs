using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace DesnormalizadorHercules.Models.Actualizadores
{
    //TODO comentarios completados, falta eliminar froms

    /// <summary>
    /// Clase para actualizar propiedades de patentes
    /// </summary>
    class ActualizadorPatent : ActualizadorBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pResourceApi">API Wrapper de GNOSS</param>
        public ActualizadorPatent(ResourceApi pResourceApi) : base(pResourceApi)
        {
        }

        /// <summary>
        /// Actualizamos en la propiedad http://w3id.org/roh/isValidated de las http://purl.org/ontology/bibo/Patent
        /// los patentes validadas (son las patentes oficiales, es decir, las que tienen http://w3id.org/roh/crisIdentifier)
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPatents">IDs de las patentes</param>
        public void ActualizarPatentesValidadas(List<string> pPatents = null)
        {
            //Eliminamos los duplicados
            EliminarDuplicados("patent", "http://purl.org/ontology/bibo/Patent", "http://w3id.org/roh/isValidated");

            HashSet<string> filters = new HashSet<string>();
            if (pPatents != null && pPatents.Count > 0)
            {
                filters.Add($" FILTER(?patent in(<{string.Join(">,<", pPatents)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {
                while (true)
                {
                    int limit = 500;
                    String select = @"select ?patent ?isValidatedCargado ?isValidatedCargar ";
                    String where = @$"where{{
                            ?patent a <http://purl.org/ontology/bibo/Patent>.
                            {filter}
                            OPTIONAL
                            {{
                              ?patent <http://w3id.org/roh/isValidated> ?isValidatedCargado.
                            }}
                            {{
                              select ?patent IF(BOUND(?crisIdentifier),'true','false')  as ?isValidatedCargar
                              Where{{                               
                                ?patent a <http://purl.org/ontology/bibo/Patent>.
                                OPTIONAL
                                {{
                                    ?patent <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                }}                                
                              }}
                            }}
                            FILTER(?isValidatedCargado!= ?isValidatedCargar OR !BOUND(?isValidatedCargado) )
                            }} limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "patent");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string patent = fila["patent"].value;
                        string isValidatedCargar = fila["isValidatedCargar"].value;
                        string isValidatedCargado = "";
                        if (fila.ContainsKey("isValidatedCargado"))
                        {
                            isValidatedCargado = fila["isValidatedCargado"].value;
                        }
                        ActualizadorTriple(patent, "http://w3id.org/roh/isValidated", isValidatedCargado, isValidatedCargar);
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Desnormalizamos los nombres dentro de la entidad auxiliar http://w3id.org/roh/PersonAux a la que apuntan estas propiedades
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPatents">IDs de las patentes</param>
        public void ActualizarMiembros(List<string> pPatents = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pPatents != null && pPatents.Count > 0)
            {
                filters.Add($" FILTER(?patent in(<{string.Join(">,<", pPatents)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {
                //Asignamos foaf:firstName
                Dictionary<string, string> propiedadesPersonPatent = new Dictionary<string, string>();
                propiedadesPersonPatent["http://xmlns.com/foaf/0.1/firstName"] = "http://xmlns.com/foaf/0.1/firstName";
                propiedadesPersonPatent["http://xmlns.com/foaf/0.1/lastName"] = "http://xmlns.com/foaf/0.1/familyName";
                propiedadesPersonPatent["--"] = "http://w3id.org/roh/secondFamilyName";
                foreach (string propPerson in propiedadesPersonPatent.Keys)
                {
                    string propPatent = propiedadesPersonPatent[propPerson];
                    while (true)
                    {
                        int limit = 500;
                        //TODO eliminar from
                        String select = @"select distinct ?patent ?personAux ?propPersonAux ?property ?propertyLoad ?propertyToLoad from <http://gnoss.com/person.owl> ";
                        String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?patent ?personAux ?propPersonAux ?property ?propertyLoad ?propertyToLoad
                                        Where{{
                                            ?patent a <http://purl.org/ontology/bibo/Patent>.
                                            ?patent <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.                                            
                                            ?patent <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            OPTIONAL{{?autor <{propPatent}> ?propertyLoad.}}
                                            BIND(?autor as ?personAux)
                                            BIND(<http://purl.org/ontology/bibo/authorList> as ?propPersonAux)                                            
                                            ?person <{propPerson}> ?propertyToLoad.
                                            BIND(<{propPatent}> as ?property).
                                            FILTER(
                                                    (!BOUND(?propertyLoad) AND BOUND(?propertyToLoad)) 
                                                    OR 
                                                    (BOUND(?propertyLoad) AND !BOUND(?propertyToLoad)) 
                                                    OR 
                                                    (str(?propertyLoad) != str(?propertyToLoad))
                                            )
                                        }}
                                    }}
                                }}order by desc(?patent) limit {limit}";
                        SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "patent");
                        ActualizarPropiedadMiembrosProyectoGrupoPatente(resultado.results.bindings, "patent");
                        if (resultado.results.bindings.Count != limit)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
