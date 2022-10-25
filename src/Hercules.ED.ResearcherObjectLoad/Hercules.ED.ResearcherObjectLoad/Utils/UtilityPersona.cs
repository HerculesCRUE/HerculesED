using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gnoss.ApiWrapper.ApiModel;
using PersonOntology;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;
using System.Collections.Concurrent;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using System.Runtime.InteropServices;
using Hercules.ED.ResearcherObjectLoad.Models;

namespace Hercules.ED.ResearcherObjectLoad.Utils
{
    public class UtilityPersona
    {
        private static ResourceApi mResourceApi = Carga.mResourceApi;

        public static DisambiguationPerson ObtenerDatosBasicosPersona(List<string> listaDatos)
        {
            DisambiguationPerson persona = new DisambiguationPerson();
            persona.ID = listaDatos.First();
            persona.departamento = DatosDepartamentoPersona(listaDatos).Select(x => x.Value).FirstOrDefault();
            persona.organizacion = DatosOrganizacionPersona(listaDatos).Select(x => x.Value).FirstOrDefault();
            persona.grupos = DatosGrupoPersona(listaDatos).Select(x => x.Value).FirstOrDefault();
            persona.proyectos = DatosProyectoPersona(listaDatos).Select(x => x.Value).FirstOrDefault();

            return persona;
        }

        /// <summary>
        /// Contruye el objeto persona a cargar.
        /// </summary>
        /// <param name="pNombreCompleto">Nombre completo (nombre + apellidos)</param>
        /// <param name="pNombre">Nombre de la persona.</param>
        /// <param name="pApellidos">Apellidos de la persona.</param>
        /// <returns>Objeto persona con los datos.</returns>
        public static Person ConstruirPersona(PersonaPub pPersona = null, Person_JSON pPersonaRO = null)
        {
            if (pPersona != null)
            {
                Person person = new Person();

                if (pPersona.name != null && pPersona.name.given != null && pPersona.name.given.Any() && pPersona.name.given[0] != null && pPersona.name.familia != null && pPersona.name.familia.Any() && pPersona.name.familia[0] != null)
                {
                    person.Foaf_name = pPersona.name.given[0].Trim() + " " + pPersona.name.familia[0].Trim();
                    person.Foaf_firstName = pPersona.name.given[0].Trim();
                    person.Foaf_lastName = pPersona.name.familia[0].Trim();
                }
                else if (pPersona.name != null && pPersona.name.nombre_completo != null && pPersona.name.nombre_completo.Any())
                {
                    person.Foaf_name = pPersona.name.nombre_completo[0].Trim();
                    if (person.Foaf_name.Contains(","))
                    {
                        person.Foaf_firstName = pPersona.name.nombre_completo[0].Split(',')[1].Trim();
                        person.Foaf_lastName = pPersona.name.nombre_completo[0].Split(',')[0].Trim();
                    }
                    else if (person.Foaf_name.Contains(" "))
                    {
                        person.Foaf_firstName = pPersona.name.nombre_completo[0].Trim().Split(' ')[0].Trim();
                        person.Foaf_lastName = pPersona.name.nombre_completo[0].Trim().Substring(pPersona.name.nombre_completo[0].Trim().IndexOf(' ')).Trim();
                    }
                    else
                    {
                        person.Foaf_firstName = pPersona.name.nombre_completo[0].Trim();
                    }
                }
                else
                {
                    if (pPersona.nick.Contains(","))
                    {
                        person.Foaf_firstName = pPersona.nick.Split(',')[1].Trim();
                        person.Foaf_lastName = pPersona.nick.Split(',')[0].Trim();
                    }
                    else if (pPersona.nick.Contains(" "))
                    {
                        person.Foaf_firstName = pPersona.nick.Trim().Split(' ')[1].Trim();
                        person.Foaf_lastName = pPersona.nick.Trim().Split(' ')[0].Trim();
                    }
                    else
                    {
                        person.Foaf_firstName = pPersona.nick.Trim();
                    }
                    person.Foaf_name = person.Foaf_firstName + " " + person.Foaf_lastName;
                }

                if (!string.IsNullOrEmpty(pPersona.orcid))
                {
                    person.Roh_ORCID = pPersona.orcid;
                }

                if (!string.IsNullOrEmpty(pPersona.researcherID))
                {
                    person.Vivo_researcherId = pPersona.researcherID;
                }

                if (pPersona.iDs != null && pPersona.iDs.Any())
                {
                    foreach (string item in pPersona.iDs)
                    {
                        if (item.ToLower().Contains("semanticscholar"))
                        {
                            person.Roh_semanticScholarId = item.Split(":")[1].Trim();
                        }
                    }
                }

                return person;
            }

            if (pPersonaRO != null)
            {
                Person person = new Person();

                if (!string.IsNullOrEmpty(pPersonaRO.orcid))
                {
                    person.Roh_ORCID = pPersonaRO.orcid;
                }

                if (!string.IsNullOrEmpty(pPersonaRO.nombreCompleto) && pPersonaRO.nombreCompleto.Contains(" "))
                {
                    // TODO
                    person.Foaf_firstName = pPersonaRO.nombreCompleto.Split(" ")[0].Trim();
                    person.Foaf_lastName = pPersonaRO.nombreCompleto.Substring(pPersonaRO.nombreCompleto.IndexOf(" ")).Trim();
                    person.Foaf_name = pPersonaRO.nombreCompleto;
                }
                else if (!string.IsNullOrEmpty(pPersonaRO.nombreCompleto))
                {
                    person.Foaf_name = pPersonaRO.nombreCompleto;
                }

                return person;
            }

            return null;
        }

        /// <summary>
        /// Obtiene el objeto para desambiguar personas.
        /// </summary>
        /// <param name="pPersona">Persona a convertir.</param>
        /// <returns>Objeto para desambiguar.</returns>
        public static DisambiguationPerson GetDisambiguationPerson(PersonaPub pPersona = null, Person_JSON pPersonaRo = null, string pPersonaGit = null)
        {
            if (pPersona != null)
            {
                pPersona.ID = Guid.NewGuid().ToString();
                string nombreCompleto = string.Empty;

                if (pPersona.name != null && pPersona.name.nombre_completo != null && pPersona.name.nombre_completo.Any())
                {
                    nombreCompleto = pPersona.name.nombre_completo[0];
                }

                DisambiguationPerson persona = new DisambiguationPerson()
                {
                    ID = pPersona.ID,
                    orcid = pPersona.orcid,
                    completeName = nombreCompleto,
                    idGnoss = pPersona.idGnoss
                };

                return persona;
            }

            if (pPersonaRo != null)
            {
                pPersonaRo.ID = Guid.NewGuid().ToString();
                DisambiguationPerson persona = new DisambiguationPerson()
                {
                    ID = pPersonaRo.ID,
                    orcid = pPersonaRo.orcid,
                    figShareId = pPersonaRo.id.ToString(),
                    completeName = pPersonaRo.nombreCompleto
                };

                return persona;
            }

            if (pPersonaGit != null)
            {
                DisambiguationPerson persona = new DisambiguationPerson()
                {
                    ID = Guid.NewGuid().ToString(),
                    gitHubId = pPersonaGit
                };

                return persona;
            }

            return null;
        }


        public static DocumentoBBDD ObtenerDatosDocumento(string pIdDocumento)
        {
            DocumentoBBDD documento = new DocumentoBBDD();
            documento.categorias = new HashSet<string>();
            documento.etiquetas = new HashSet<string>();

            string select = $@"SELECT DISTINCT ?urlPdf ?label ?etiquetas";
            string where = $@"WHERE {{
                                ?s ?p ?o.
                                OPTIONAL{{ ?s <http://w3id.org/roh/hasFile> ?urlPdf. }}
                                OPTIONAL{{ {{?s <http://w3id.org/roh/userKnowledgeArea> ?categorias. 
                                           ?categorias <http://w3id.org/roh/categoryNode> ?concept. 
                                           ?concept <http://www.w3.org/2008/05/skos#prefLabel> ?label.}}
                                            MINUS
                                            {{
                                            ?concept <http://www.w3.org/2008/05/skos#narrower> ?hijos. 
                                            }}
                                }}
                                OPTIONAL{{ ?s <http://w3id.org/roh/userKeywords> ?etiquetas. }}
                                FILTER(?s = <{pIdDocumento}>)
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "document", "taxonomy" });
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("urlPdf") && !string.IsNullOrEmpty(fila["urlPdf"].value))
                    {
                        documento.urlPdf = fila["urlPdf"].value;
                    }

                    if (fila.ContainsKey("label") && !string.IsNullOrEmpty(fila["label"].value))
                    {
                        documento.categorias.Add(fila["label"].value);
                    }

                    if (fila.ContainsKey("etiquetas") && !string.IsNullOrEmpty(fila["etiquetas"].value))
                    {
                        documento.etiquetas.Add(fila["etiquetas"].value);
                    }
                }
            }

            return documento;
        }

        public static List<Tuple<string, string, string, string, string, string>> ObtenerPersonas(HashSet<string> pIdsPersonas)
        {
            List<Tuple<string, string, string, string, string, string>> listaResultados = new List<Tuple<string, string, string, string, string, string>>();

            List<List<string>> listaListas = Utility.SplitList(pIdsPersonas.ToList(), 500).ToList();

            foreach (List<string> lista in listaListas)
            {
                int limit = 10000;
                int offset = 0;

                // Consulta sparql.
                while (true)
                {
                    string select = "SELECT * WHERE { SELECT DISTINCT ?s ?orcid ?crisIdentifier ?nombre ?apellidos ?nombreCompleto ";
                    string where = $@"WHERE {{
                                ?s <http://xmlns.com/foaf/0.1/name> ?nombreCompleto.
                                OPTIONAL{{?s <http://w3id.org/roh/ORCID> ?orcid. }}
                                OPTIONAL{{?s <http://w3id.org/roh/crisIdentifier> ?crisIdentifier. }}
                                OPTIONAL{{?s <http://xmlns.com/foaf/0.1/firstName> ?nombre. }}
                                OPTIONAL{{?s <http://xmlns.com/foaf/0.1/lastName> ?apellidos. }}
                                FILTER(?s IN (<{string.Join(">,<", lista)}>))
                            }} ORDER BY DESC(?orcid) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            // Comprobaciones
                            string s = fila["s"].value;
                            string orcid = string.Empty;
                            string crisIdentifier = string.Empty;
                            string nombre = string.Empty;
                            string apellidos = string.Empty;
                            string nombreCompleto = fila["nombreCompleto"].value;

                            if (fila.ContainsKey("orcid"))
                            {
                                orcid = fila["orcid"].value;
                            }
                            if (fila.ContainsKey("crisIdentifier"))
                            {
                                crisIdentifier = fila["crisIdentifier"].value;
                            }
                            if (fila.ContainsKey("nombre"))
                            {
                                nombre = fila["nombre"].value;
                            }
                            if (fila.ContainsKey("apellidos"))
                            {
                                apellidos = fila["apellidos"].value;
                            }

                            Tuple<string, string, string, string, string, string> tuplaDatos = new(
                                s,
                                orcid,
                                crisIdentifier,
                                nombre,
                                apellidos,
                                nombreCompleto
                            );
                            listaResultados.Add(tuplaDatos);
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return listaResultados;
        }

        public static Dictionary<string, DisambiguationPerson> ObtenerPersonasNombre(List<string> listado)
        {
            Dictionary<string, DisambiguationPerson> diccionarioPersonas = new Dictionary<string, DisambiguationPerson>();
            string nameInput = "";

            string selectOut = "SELECT DISTINCT ?personID ?name ?num ?nameInput ";
            string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://xmlns.com/foaf/0.1/name> ?name .
                                    {{";

            foreach (string firma in listado)
            {
                string texto = Disambiguation.ObtenerTextosNombresNormalizados(firma);
                string[] wordsTexto = texto.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (wordsTexto.Length > 0)
                {
                    #region Buscamos en nombres
                    {
                        List<string> unions = new List<string>();
                        List<string> unionsOut = new List<string>();
                        foreach (string wordOut in wordsTexto)
                        {
                            List<string> words = new List<string>();
                            if (wordOut.Length == 2)
                            {
                                words.Add(wordOut[0].ToString());
                                words.Add(wordOut[1].ToString());
                            }
                            else
                            {
                                words.Add(wordOut);
                            }

                            foreach (string word in words)
                            {
                                int score = 1;
                                if (word.Length > 1)
                                {
                                    score = 5;
                                }
                                if (score == 1)
                                {
                                    StringBuilder sbUnion = new StringBuilder();
                                    sbUnion.AppendLine("				?personID <http://xmlns.com/foaf/0.1/name> ?name.");
                                    sbUnion.AppendLine($@"				{{  FILTER(lcase(?name) like'{word}%').}} UNION  {{  FILTER(lcase(?name) like'% {word}%').}}  BIND({score} as ?num)  ");
                                    unions.Add(sbUnion.ToString());
                                }
                                else
                                {
                                    StringBuilder sbUnion = new StringBuilder();
                                    sbUnion.AppendLine("				?personID <http://xmlns.com/foaf/0.1/name> ?name.");
                                    sbUnion.AppendLine($@"				{FilterWordComplete(word, "name")} BIND({score} as ?num) ");
                                    //sbUnion.AppendLine($@"				?name bif:contains ""'{word}'"" BIND({score} as ?num) ");
                                    unions.Add(sbUnion.ToString());
                                }
                            }
                        }

                        string select = $@" select distinct ?personID sum(?num) as ?num ?nameInput ";
                        string where = $@" where
                                        {{
                                            ?personID a <http://xmlns.com/foaf/0.1/Person>.
                                            {{{string.Join("}UNION{", unions)}}}           
                                            BIND(""{texto}"" as ?nameInput)
                                        }}order by desc (?num) limit 50
                                     ";
                        string consultaInterna = select + where;
                        whereOut += consultaInterna + "}UNION{";
                    }
                    #endregion
                }
            }
            whereOut = whereOut.Remove(whereOut.Length - 6, 6);
            whereOut += " }order by desc (?nameInput) desc ( ?num)";

            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(selectOut, whereOut, "person");

            foreach (string firma in listado)
            {
                HashSet<int> scores = new HashSet<int>();
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings.Where(x => x["nameInput"].value == Disambiguation.ObtenerTextosNombresNormalizados(firma)))
                {
                    string personID = fila["personID"].value;
                    if (!diccionarioPersonas.ContainsKey(nameInput))
                    {
                        diccionarioPersonas[personID] = new DisambiguationPerson();
                    }
                    string name = fila["name"].value;
                    int score = int.Parse(fila["num"].value);
                    scores.Add(score);
                    if (scores.Count > 2)
                    {
                        break;
                    }
                    DisambiguationPerson persona = new DisambiguationPerson
                    {
                        completeName = name,
                        ID = personID
                    };
                    diccionarioPersonas[personID] = persona;
                }
            }

            return diccionarioPersonas;
        }

        public static string FilterWordComplete(string pWord, string pVar)
        {
            Dictionary<string, string> listaReemplazos = new Dictionary<string, string>();
            listaReemplazos["a"] = "aáàä";
            listaReemplazos["e"] = "eéèë";
            listaReemplazos["i"] = "iíìï";
            listaReemplazos["o"] = "oóòö";
            listaReemplazos["u"] = "uúùü";
            listaReemplazos["n"] = "nñ";
            listaReemplazos["c"] = "cç";
            foreach (string caracter in listaReemplazos.Keys)
            {
                pWord = pWord.Replace(caracter, $"[{listaReemplazos[caracter]}]");
            }
            string filter = @$"FILTER ( regex(?{pVar},""(^| ){pWord}($| )"", ""i""))";
            return filter;
        }

        /// <summary>
        /// Devuelve el identificador de la persona pasado su ORCID.
        /// </summary>
        /// <param name="resourceApi"></param>
        /// <param name="orcid"></param>
        /// <returns></returns>
        public static string ObtenerPersonaPorORCID(string orcid)
        {
            string personID = "";

            // TODO: isActive en teoría todas las personas que se pidan por el ORCID han de ser personal activo.
            string selectOut = "SELECT DISTINCT ?personID";
            string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/ORCID> ""{orcid}"" .
                                    ?persona <http://w3id.org/roh/isActive> 'true'.
                                    }}";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(selectOut, whereOut, "person");
            foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
            {
                personID = fila["personID"].value;
            }
            return personID;
        }

        /// <summary>
        /// Obtiene un diccionario con las personas, su ORCID y el nombre de la misma
        /// </summary>
        /// <param name="resourceApi"></param>
        /// <param name="listado"></param>
        /// <returns></returns>
        public static Dictionary<string, DisambiguationPerson> ObtenerPersonasORCID(List<string> listado)
        {
            Dictionary<string, DisambiguationPerson> diccionarioPersonas = new Dictionary<string, DisambiguationPerson>();

            List<List<string>> listadoLista= Utility.SplitList(listado.Distinct().ToList(), 1000).ToList();
            foreach (List<string> listaIn in listadoLista)
            {
                string selectOut = "SELECT DISTINCT ?personID ?orcid ?name";
                string whereOut = $@"where{{
                                    ?personID a <http://xmlns.com/foaf/0.1/Person> .
                                    ?personID <http://w3id.org/roh/ORCID> ?orcid .
                                    ?personID <http://xmlns.com/foaf/0.1/name> ?name .
                                    FILTER(?orcid in ('{string.Join("','", listaIn)}'))
                                    }}";

                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(selectOut, whereOut, "person");

                foreach (string firma in listado)
                {
                    HashSet<int> scores = new HashSet<int>();
                    foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings.Where(x => x["orcid"].value == firma))
                    {
                        string personID = fila["personID"].value;
                        if (!diccionarioPersonas.ContainsKey(personID))
                        {
                            diccionarioPersonas[personID] = new DisambiguationPerson();
                        }

                        string orcid = fila["orcid"].value;
                        string name = fila["name"].value;
                        DisambiguationPerson persona = new DisambiguationPerson
                        {
                            orcid = orcid,
                            ID = personID,
                            completeName = name
                        };
                        diccionarioPersonas[personID] = persona;
                    }
                }
            }
            return diccionarioPersonas;
        }

        /// <summary>
        /// Obtiene las personas relacionadas en función del valor de una propiedad.
        /// </summary>
        /// <param name="pProperty">Propiedad para buscar en la persona</param>
        /// <param name="pPropertyValue">Valor de la propiedad para buscar en la persona</param>
        /// <returns>Diccionario con el ID del recurso cargado como clave, y el objeto desambiguable como valor.</returns>
        public static ConcurrentDictionary<string, DisambiguationPerson> ObtenerPersonasRelacionaBBDD(List<Publication> listadoAux, string pGnossId)
        {
            HashSet<string> listaNombres = new HashSet<string>();
            HashSet<string> listaORCID = new HashSet<string>();
            ConcurrentDictionary<string, DisambiguationPerson> listaPersonasAux = new ConcurrentDictionary<string, DisambiguationPerson>();

            
            if (true)
            {
                // TODO: isActive en teoría todas las personas que se pidan por el ORCID han de ser personal activo.
                string select = "SELECT DISTINCT ?persona ?nombreCompleto ?orcid ";
                string where = $@"WHERE {{
                                    <{pGnossId}> <http://w3id.org/roh/ORCID> ?orcid. 
                                    <{pGnossId}> <http://xmlns.com/foaf/0.1/name> ?nombreCompleto.
                                    <{pGnossId}> <http://w3id.org/roh/isActive> 'true'.
                                }}";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    PersonaPub persona = new PersonaPub();
                    if(fila.ContainsKey("orcid") && !string.IsNullOrEmpty(fila["nombreCompleto"].value))
                    {
                        listaORCID.Add(fila["orcid"].value);
                        persona.orcid = fila["orcid"].value;
                    }                    

                    persona.name = new Name();
                    persona.name.nombre_completo = new List<string>() { fila["nombreCompleto"].value };
                    persona.idGnoss = pGnossId;
                    DisambiguationPerson person = GetDisambiguationPerson(persona);
                    person.ID = pGnossId;

                    listaNombres.Add(fila["nombreCompleto"].value);
                    listaPersonasAux.TryAdd(pGnossId, person);
                }
            }

            //Selecciono el nombre completo o la firma.
            foreach (Publication item in listadoAux)
            {
                for (int i = 0; i < item.seqOfAuthors.Count; i++)
                {
                    if (!string.IsNullOrEmpty(item.seqOfAuthors[i].orcid))
                    {
                        listaORCID.Add(item.seqOfAuthors[i].orcid);
                    }
                    if (!string.IsNullOrEmpty(item.seqOfAuthors[i].name.nombre_completo.FirstOrDefault()))
                    {
                        listaNombres.Add(item.seqOfAuthors[i].name.nombre_completo.First());
                    };
                }
            }

            //Divido la lista en listas de 10 elementos
            List<List<string>> listaListaNombres = Utility.SplitList(listaNombres.ToList(), 10).ToList();
            Parallel.ForEach(listaListaNombres, new ParallelOptions { MaxDegreeOfParallelism = 5 }, lista =>
            {
                Dictionary<string, DisambiguationPerson> personasBBDD = ObtenerPersonasNombre(lista);
                foreach (KeyValuePair<string, DisambiguationPerson> valuePair in personasBBDD)
                {
                    listaPersonasAux[valuePair.Key.Trim()] = valuePair.Value;
                }
            });
                        
            Dictionary<string, DisambiguationPerson> personasBBDD = ObtenerPersonasORCID(listaORCID.ToList());
            foreach (KeyValuePair<string, DisambiguationPerson> valuePair in personasBBDD)
            {
                listaPersonasAux[valuePair.Key.Trim()] = valuePair.Value;
            }

            Dictionary<string, DisambiguationPerson> listaPersonas = new Dictionary<string, DisambiguationPerson>();
            {
                List<List<string>> listaListasPersonas = Utility.SplitList(listaPersonasAux.Keys.ToList(), 100).ToList();
                foreach (List<string> listaIn in listaListasPersonas)
                {
                    int limit = 10000;
                    int offset = 0;

                    // Consulta sparql.
                    while (true)
                    {
                      
                        //Obtenemos todas las personas hasta con 2 niveles de coautoria tanto en researchObjects como en Documentos               
                        string select = $@"SELECT *
                                      WHERE {{ 
                                          SELECT DISTINCT ?persona ?orcid ?usuarioFigShare ?usuarioGitHub ?nombreCompleto";
                        string where = $@"WHERE {{
                                ?persona a <http://xmlns.com/foaf/0.1/Person>
                                OPTIONAL{{?persona <http://w3id.org/roh/ORCID> ?orcid. }}
                                OPTIONAL{{?persona <http://w3id.org/roh/usuarioFigShare> ?usuarioFigShare. }}
                                OPTIONAL{{?persona <http://w3id.org/roh/usuarioGitHub> ?usuarioGitHub. }}
                                ?persona <http://xmlns.com/foaf/0.1/name> ?nombreCompleto.
                                FILTER(?persona in (<{string.Join(">,<", listaIn)}>))
                            }} ORDER BY DESC(?persona) }} LIMIT {limit} OFFSET {offset}";

                        SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "person" ,"document", "researchobject" });
                        if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                        {
                            offset += limit;
                            foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                            {
                                PersonaPub persona = new PersonaPub();
                                if (fila.ContainsKey("orcid"))
                                {
                                    persona.orcid = fila["orcid"].value;
                                }
                                persona.name = new Name();
                                persona.name.nombre_completo = new List<string>() { fila["nombreCompleto"].value };
                                DisambiguationPerson person = GetDisambiguationPerson(persona);
                                person.ID = fila["persona"].value;
                                if (fila.ContainsKey("usuarioFigShare"))
                                {
                                    person.figShareId = fila["usuarioFigShare"].value;
                                }
                                if (fila.ContainsKey("usuarioGitHub"))
                                {
                                    person.gitHubId = fila["usuarioGitHub"].value;
                                }
                                listaPersonas.Add(fila["persona"].value, person);
                            }
                            if (resultadoQuery.results.bindings.Count < limit)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    };
                }
            }

            Dictionary<string, HashSet<string>> autoresDocRos = new Dictionary<string, HashSet<string>>();
            {
                List<List<string>> listaListasPersonas = Utility.SplitList(listaPersonasAux.Keys.ToList(), 100).ToList();
                foreach (List<string> listaIn in listaListasPersonas)
                {

                    int limit = 10000;
                    int offset = 0;

                    // Consulta sparql.
                    while (true)
                    {
                    
                        //Obtenemos las coautorias
                        string select = $@"SELECT * 
                                      WHERE {{
                                          SELECT DISTINCT ?documento ?persona ";
                        string where = $@"WHERE {{
                                ?documento a ?rdfType. 
                                FILTER(?rdfType in (<http://purl.org/ontology/bibo/Document>,<http://w3id.org/roh/ResearchObject>))
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona. 
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores2. 
                                ?listaAutores2 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?personaList. 
                                FILTER(?personaList in (<{string.Join(">,<",listaIn)}>))
                            }} ORDER BY DESC(?documento) DESC(?persona) }} LIMIT {limit} OFFSET {offset}";

                        SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "person" , "document", "researchobject" });
                        if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                        {
                            offset += limit;
                            foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                            {
                                string persona = fila["persona"].value;
                                string documento = fila["documento"].value;
                                if (!autoresDocRos.ContainsKey(documento))
                                {
                                    autoresDocRos[documento] = new HashSet<string>();
                                }
                                autoresDocRos[documento].Add(persona);
                            }
                            if (resultadoQuery.results.bindings.Count < limit)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    };
                }

                foreach (string idDoc in autoresDocRos.Keys)
                {
                    foreach (string idPersona in autoresDocRos[idDoc])
                    {
                        DisambiguationPerson persona = (DisambiguationPerson)(listaPersonas.Where(x => x.Key == idPersona).FirstOrDefault().Value);
                        if (persona != null)
                        {
                            if (persona.documentos == null)
                            {
                                persona.documentos = new HashSet<string>();
                            }
                            if (persona.coautores == null)
                            {
                                persona.coautores = new HashSet<string>();
                            }
                            persona.documentos.Add(idDoc);
                            persona.coautores.UnionWith(new HashSet<string>(autoresDocRos[idDoc].Except(new HashSet<string>() { idPersona })));
                            persona.distincts.UnionWith(new HashSet<string>(autoresDocRos[idDoc].Except(new HashSet<string>() { idPersona })));
                        }
                    }
                }
            }

            //Divido la lista en listas de 500 elementos
            List<List<string>> listaListasIdPersonas = Utility.SplitList(listaPersonasAux.Select(x => x.Value).Select(x => x.ID).ToList(), 500).ToList();

            foreach (List<string> lista in listaListasIdPersonas)
            {
                //Selecciono los datos de grupos, proyectos comperititvos, proyectos no competitivos, organizaciones y departamentos para la posterior desambiguacion de la persona.
                Dictionary<string, HashSet<string>> departamentos = DatosDepartamentoPersona(lista);
                foreach (string persona in departamentos.Keys)
                {
                    List<DisambiguationPerson> listado = listaPersonasAux.Where(x => x.Key.Equals(persona)).Select(x => x.Value).ToList();
                    foreach (DisambiguationPerson p in listado)
                    {
                        p.departamento.UnionWith(departamentos.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> organizaciones = DatosOrganizacionPersona(lista);
                foreach (string persona in organizaciones.Keys)
                {
                    List<DisambiguationPerson> listado = listaPersonasAux.Where(x => x.Key.Equals(persona)).Select(x => x.Value).ToList();
                    foreach (DisambiguationPerson p in listado)
                    {
                        p.organizacion.UnionWith(organizaciones.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> grupos = DatosGrupoPersona(lista);
                foreach (string persona in grupos.Keys)
                {
                    List<DisambiguationPerson> listado = listaPersonasAux.Where(x => x.Key.Equals(persona)).Select(x => x.Value).ToList();
                    foreach (DisambiguationPerson p in listado)
                    {
                        p.grupos.UnionWith(grupos.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> proyectos = DatosProyectoPersona(lista);
                foreach (string persona in proyectos.Keys)
                {
                    List<DisambiguationPerson> listado = listaPersonasAux.Where(x => x.Key.Equals(persona)).Select(x => x.Value).ToList();
                    foreach (DisambiguationPerson p in listado)
                    {
                        p.proyectos.UnionWith(proyectos.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }
            }

            foreach (KeyValuePair<string, DisambiguationPerson> key in listaPersonasAux)
            {
                foreach (KeyValuePair<string, DisambiguationPerson> key2 in listaPersonas)
                {
                    if (key.Key.Equals(key2.Key))
                    {
                        listaPersonasAux[key.Key].figShareId = listaPersonas[key2.Key].figShareId;
                        listaPersonasAux[key.Key].gitHubId = listaPersonas[key2.Key].gitHubId;
                        listaPersonasAux[key.Key].zenodoId = listaPersonas[key2.Key].zenodoId;
                        listaPersonasAux[key.Key].coautores = listaPersonas[key2.Key].coautores;
                        listaPersonasAux[key.Key].distincts = listaPersonas[key2.Key].distincts;
                        listaPersonasAux[key.Key].documentos = listaPersonas[key2.Key].documentos;
                    }
                }
            }

            if (listaPersonasAux.Values.Any(x => x.ID == null))
            {
                List<KeyValuePair<string, DisambiguationPerson>> listaAux = listaPersonasAux.Where(x => x.Value.ID == null).ToList();
                foreach (KeyValuePair<string, DisambiguationPerson> pair in listaAux)
                {
                    listaPersonasAux[pair.Key].ID = pair.Key;
                }
            }

            return listaPersonasAux;
        }

        /// <summary>
        /// Devuelve el listado de personas relacionadas en los RO
        /// </summary>
        /// <param name="OrcidAutor"></param>
        /// <param name="listadoZenodo"></param>
        /// <param name="listadoGitHub"></param>
        /// <param name="listadoFigShare"></param>
        /// <returns></returns>
        public static ConcurrentDictionary<string, DisambiguationPerson> ObtenerPersonasRelacionaBBDDRO(string OrcidAutor, [Optional] List<ResearchObjectZenodo> listadoZenodo,
            [Optional] List<ResearchObjectGitHub> listadoGitHub, [Optional] List<ResearchObjectFigShare> listadoFigShare)
        {
            HashSet<string> listaNombres = new HashSet<string>();
            HashSet<string> listaORCID = new HashSet<string>();
            HashSet<string> listaGitHub = new HashSet<string>();
            ConcurrentDictionary<string, DisambiguationPerson> listaPersonasAux = new ConcurrentDictionary<string, DisambiguationPerson>();

            listaORCID.Add(OrcidAutor);
            if (true)
            {
                string select = "SELECT DISTINCT ?persona ?nombreCompleto ";
                string where = $@"WHERE {{
                                ?persona a <http://xmlns.com/foaf/0.1/Person> .
                                ?persona <http://w3id.org/roh/ORCID> ""{OrcidAutor}"". 
                                ?persona <http://xmlns.com/foaf/0.1/name> ?nombreCompleto.
                            }}";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    PersonaPub persona = new PersonaPub();
                    persona.orcid = OrcidAutor;

                    persona.name = new Name();
                    persona.name.nombre_completo = new List<string>() { fila["nombreCompleto"].value };
                    DisambiguationPerson person = GetDisambiguationPerson(persona);
                    person.ID = fila["persona"].value;

                    listaNombres.Add(fila["nombreCompleto"].value);
                    listaPersonasAux.TryAdd(fila["persona"].value, person);
                }
            }

            //Selecciono el nombre completo o la firma.
            if (listadoZenodo != null && listadoZenodo.Count > 0)
            {
                UtilityZenodo.AutoresZenodo(listaNombres, listaORCID, listadoZenodo);
            }
            if (listadoFigShare != null && listadoFigShare.Count > 0)
            {
                UtilityFigShare.AutoresFigShare(listaNombres, listaORCID, listadoFigShare);
            }
            if (listadoGitHub != null && listadoGitHub.Count > 0)
            {
                UtilityGitHub.AutoresGitHub(listaGitHub, listadoGitHub);
            }

            //Divido la lista en listas de 10 elementos
            List<List<string>> listaListaNombres = Utility.SplitList(listaNombres.ToList(), 10).ToList();
            Parallel.ForEach(listaListaNombres, new ParallelOptions { MaxDegreeOfParallelism = 5 }, lista =>
            {
                Dictionary<string, DisambiguationPerson> personasBBDD = ObtenerPersonasNombre(lista);
                foreach (KeyValuePair<string, DisambiguationPerson> valuePair in personasBBDD)
                {
                    listaPersonasAux[valuePair.Key.Trim()] = valuePair.Value;
                }
            });

            {
                Dictionary<string, DisambiguationPerson> personasBBDD = UtilityGitHub.ObtenerPersonasGitHub(listaGitHub.ToList());
                foreach (KeyValuePair<string, DisambiguationPerson> valuePair in personasBBDD)
                {
                    listaPersonasAux[valuePair.Key.Trim()] = valuePair.Value;
                }
            }

            {
                Dictionary<string, DisambiguationPerson> personasBBDD = ObtenerPersonasORCID(listaORCID.ToList());
                foreach (KeyValuePair<string, DisambiguationPerson> valuePair in personasBBDD)
                {
                    listaPersonasAux[valuePair.Key.Trim()] = valuePair.Value;
                }
            }


            Dictionary<string, DisambiguationPerson> listaPersonas = new Dictionary<string, DisambiguationPerson>();
            {
                List<List<string>> listaListasPersonas = Utility.SplitList(listaPersonasAux.Keys.ToList(), 100).ToList();
                foreach (List<string> listaIn in listaListasPersonas)
                {
                    int limit = 10000;
                    int offset = 0;

                    // Consulta sparql.
                    while (true)
                    {
                        //Obtenemos todas las personas hasta con 2 niveles de coautoria tanto en researchObjects como en Documentos               
                        string select = $@"SELECT * 
                                       WHERE {{ 
                                           SELECT DISTINCT ?persona ?orcid ?usuarioFigShare ?usuarioGitHub ?nombreCompleto";
                        string where = $@"WHERE {{
                                ?persona a <http://xmlns.com/foaf/0.1/Person>
                                OPTIONAL{{?persona <http://w3id.org/roh/ORCID> ?orcid. }}
                                OPTIONAL{{?persona <http://w3id.org/roh/usuarioFigShare> ?usuarioFigShare. }}
                                OPTIONAL{{?persona <http://w3id.org/roh/usuarioGitHub> ?usuarioGitHub. }}
                                ?persona <http://xmlns.com/foaf/0.1/name> ?nombreCompleto.
                                FILTER(?persona in (<{string.Join(">,<",listaIn)}>))
                            }} ORDER BY DESC(?persona) }} LIMIT {limit} OFFSET {offset}";

                        SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where,new() { "person" ,"document", "researchobject" });
                        if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                        {
                            offset += limit;
                            foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                            {
                                PersonaPub persona = new PersonaPub();
                                if (fila.ContainsKey("orcid"))
                                {
                                    persona.orcid = fila["orcid"].value;
                                }
                                persona.name = new Name();
                                persona.name.nombre_completo = new List<string>() { fila["nombreCompleto"].value };
                                DisambiguationPerson person = GetDisambiguationPerson(persona);
                                person.ID = fila["persona"].value;
                                if (fila.ContainsKey("usuarioFigShare"))
                                {
                                    person.figShareId = fila["usuarioFigShare"].value;
                                }
                                if (fila.ContainsKey("usuarioGitHub"))
                                {
                                    person.gitHubId = fila["usuarioGitHub"].value;
                                }
                                listaPersonas.Add(fila["persona"].value, person);
                            }
                            if (resultadoQuery.results.bindings.Count < limit)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    };
                }
            }

            Dictionary<string, HashSet<string>> autoresDocRos = new Dictionary<string, HashSet<string>>();
            {
                List<List<string>> listaListasPersonas = Utility.SplitList(listaPersonasAux.Keys.ToList(), 100).ToList();
                foreach (List<string> listaIn in listaListasPersonas)
                {

                    int limit = 10000;
                    int offset = 0;

                    // Consulta sparql.
                    while (true)
                    {
                        //Obtenemos las coautorias
                        string select = $@"SELECT * 
                                      WHERE {{
                                          SELECT DISTINCT ?documento ?persona";
                        string where = $@"WHERE {{
                                ?documento a ?rdfType. 
                                FILTER(?rdfType in (<http://purl.org/ontology/bibo/Document>,<http://w3id.org/roh/ResearchObject>))
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona. 
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores2. 
                                ?listaAutores2 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?personaList. 
                                FILTER(?personaList in (<{string.Join(">,<", listaIn)}>))
                            }} ORDER BY DESC(?documento) DESC(?persona) }} LIMIT {limit} OFFSET {offset}";

                        SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "person", "document", "researchobject" });
                        if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                        {
                            offset += limit;
                            foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                            {
                                string persona = fila["persona"].value;
                                string documento = fila["documento"].value;
                                if (!autoresDocRos.ContainsKey(documento))
                                {
                                    autoresDocRos[documento] = new HashSet<string>();
                                }
                                autoresDocRos[documento].Add(persona);
                            }
                            if (resultadoQuery.results.bindings.Count < limit)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    };
                }

                foreach (string idDoc in autoresDocRos.Keys)
                {
                    foreach (string idPersona in autoresDocRos[idDoc])
                    {
                        DisambiguationPerson persona = (DisambiguationPerson)(listaPersonas.Where(x => x.Key == idPersona).FirstOrDefault().Value);
                        if (persona != null)
                        {
                            if (persona.documentos == null)
                            {
                                persona.documentos = new HashSet<string>();
                            }
                            if (persona.coautores == null)
                            {
                                persona.coautores = new HashSet<string>();
                            }
                            persona.documentos.Add(idDoc);
                            persona.coautores.UnionWith(new HashSet<string>(autoresDocRos[idDoc].Except(new HashSet<string>() { idPersona })));
                            persona.distincts.UnionWith(new HashSet<string>(autoresDocRos[idDoc].Except(new HashSet<string>() { idPersona })));
                        }
                    }
                }
            }

            //Divido la lista en listas de 500 elementos
            List<List<string>> listaListasIdPersonas = Utility.SplitList(listaPersonasAux.Select(x => x.Value).Select(x => x.ID).ToList(), 500).ToList();

            foreach (List<string> lista in listaListasIdPersonas)
            {
                //Selecciono los datos de grupos, proyectos comperititvos, proyectos no competitivos, organizaciones y departamentos para la posterior desambiguacion de la persona.
                Dictionary<string, HashSet<string>> departamentos = DatosDepartamentoPersona(lista);
                foreach (string persona in departamentos.Keys)
                {
                    List<DisambiguationPerson> listado = listaPersonasAux.Where(x => x.Key.Equals(persona)).Select(x => x.Value).ToList();
                    foreach (DisambiguationPerson p in listado)
                    {
                        p.departamento.UnionWith(departamentos.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> organizaciones = DatosOrganizacionPersona(lista);
                foreach (string persona in organizaciones.Keys)
                {
                    List<DisambiguationPerson> listado = listaPersonasAux.Where(x => x.Key.Equals(persona)).Select(x => x.Value).ToList();
                    foreach (DisambiguationPerson p in listado)
                    {
                        p.organizacion.UnionWith(organizaciones.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> grupos = DatosGrupoPersona(lista);
                foreach (string persona in grupos.Keys)
                {
                    List<DisambiguationPerson> listado = listaPersonasAux.Where(x => x.Key.Equals(persona)).Select(x => x.Value).ToList();
                    foreach (DisambiguationPerson p in listado)
                    {
                        p.grupos.UnionWith(grupos.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> proyectos = DatosProyectoPersona(lista);
                foreach (string persona in proyectos.Keys)
                {
                    List<DisambiguationPerson> listado = listaPersonasAux.Where(x => x.Key.Equals(persona)).Select(x => x.Value).ToList();
                    foreach (DisambiguationPerson p in listado)
                    {
                        p.proyectos.UnionWith(proyectos.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }
            }

            foreach (KeyValuePair<string, DisambiguationPerson> key in listaPersonasAux)
            {
                foreach (KeyValuePair<string, DisambiguationPerson> key2 in listaPersonas)
                {
                    if (key.Key.Equals(key2.Key))
                    {
                        listaPersonasAux[key.Key].figShareId = listaPersonas[key2.Key].figShareId;
                        listaPersonasAux[key.Key].gitHubId = listaPersonas[key2.Key].gitHubId;
                        listaPersonasAux[key.Key].zenodoId = listaPersonas[key2.Key].zenodoId;
                        listaPersonasAux[key.Key].coautores = listaPersonas[key2.Key].coautores;
                        listaPersonasAux[key.Key].distincts = listaPersonas[key2.Key].distincts;
                        listaPersonasAux[key.Key].documentos = listaPersonas[key2.Key].documentos;
                    }
                }
            }

            if (listaPersonasAux.Values.Any(x => x.ID == null))
            {
                List<KeyValuePair<string, DisambiguationPerson>> listaAux = listaPersonasAux.Where(x => x.Value.ID == null).ToList();
                foreach (KeyValuePair<string, DisambiguationPerson> pair in listaAux)
                {
                    listaPersonasAux[pair.Key].ID = pair.Key;
                }
            }

            return listaPersonasAux;
        }

        /// <summary>
        /// Dada una lista de personas, devuelve las organizaciones correspondientes a cada persona.
        /// </summary>
        /// <param name="personas"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> DatosOrganizacionPersona(List<string> personas)
        {
            Dictionary<string, HashSet<string>> organizaciones = new Dictionary<string, HashSet<string>>();
            string select = $@"select distinct ?person ?organization";
            string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?person <http://w3id.org/roh/hasRole> ?organization .
                                    FILTER(?person in (<{string.Join(">,<", personas)}>))
                                }} ";
            SparqlObject resultData = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "curriculumvitae" ,"person", "group" });
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                //Organizaciones
                if (fila.ContainsKey("organization"))
                {
                    if (organizaciones.ContainsKey(fila["person"].value))
                    {
                        organizaciones[fila["person"].value].Add(fila["organization"].value);
                    }
                    else
                    {
                        organizaciones.Add(fila["person"].value, new HashSet<string>() { fila["organization"].value });
                    }
                }
            }
            return organizaciones;
        }

        /// <summary>
        /// Dada una lista de personas, devuelve los departamentos correspondientes a cada persona.
        /// </summary>
        /// <param name="personas"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> DatosDepartamentoPersona(List<string> personas)
        {
            Dictionary<string, HashSet<string>> departamentos = new Dictionary<string, HashSet<string>>();
            string select = $@"select distinct ?person ?departament";
            string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?person <http://vivoweb.org/ontology/core#departmentOrSchool> ?departament .
                                    FILTER(?person in (<{string.Join(">,<", personas)}>))
                                }} ";
            SparqlObject resultData = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "curriculumvitae", "person", "group" });
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                //Departamentos
                if (fila.ContainsKey("departament"))
                {
                    if (departamentos.ContainsKey(fila["person"].value))
                    {
                        departamentos[fila["person"].value].Add(fila["departament"].value);
                    }
                    else
                    {
                        departamentos.Add(fila["person"].value, new HashSet<string>() { fila["departament"].value });
                    }
                }
            }
            return departamentos;
        }

        /// <summary>
        /// Dada una lista de personas, devuelve los proyectos correspondientes a cada persona.
        /// </summary>
        /// <param name="personas"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> DatosProyectoPersona(List<string> personas)
        {
            Dictionary<string, HashSet<string>> proyectos = new Dictionary<string, HashSet<string>>();
            string select = $@"select distinct ?person ?project";
            string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?project a <http://vivoweb.org/ontology/core#Project>.
                                    ?project ?propRol ?rol .
                                    FILTER(?propRol in (<http://w3id.org/roh/researchers>,<http://w3id.org/roh/mainResearchers>))
                                    ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person .
                                    FILTER(?person in (<{string.Join(">,<", personas)}>))
                                }} ";
            SparqlObject resultData = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "curriculumvitae" , "person", "project" });
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                //Proyectos
                if (fila.ContainsKey("project"))
                {
                    if (proyectos.ContainsKey(fila["person"].value))
                    {
                        proyectos[fila["person"].value].Add(fila["project"].value);
                    }
                    else
                    {
                        proyectos.Add(fila["person"].value, new HashSet<string>() { fila["project"].value });
                    }
                }
            }
            return proyectos;
        }

        /// <summary>
        /// Dada una lista de personas, devuelve los grupos correspondientes a cada persona.
        /// </summary>
        /// <param name="personas"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> DatosGrupoPersona(List<string> personas)
        {
            Dictionary<string, HashSet<string>> grupos = new Dictionary<string, HashSet<string>>();
            string select = $@"select distinct ?person ?group";
            string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?group a <http://xmlns.com/foaf/0.1/Group>.
                                    ?group ?propRol ?rol .
                                    FILTER(?propRol in (<http://w3id.org/roh/researchers>,<http://w3id.org/roh/mainResearchers>))
                                    ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person .
                                    FILTER(?person in (<{string.Join(">,<", personas)}>))
                                }} ";
            SparqlObject resultData = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "curriculumvitae", "person", "group" });
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                //Grupos
                if (fila.ContainsKey("group"))
                {
                    if (grupos.ContainsKey(fila["person"].value))
                    {
                        grupos[fila["person"].value].Add(fila["group"].value);
                    }
                    else
                    {
                        grupos.Add(fila["person"].value, new HashSet<string>() { fila["group"].value });
                    }
                }
            }
            return grupos;
        }

        /// <summary>
        /// Obtiene las personas iguales para poder desambiguarlas.
        /// </summary>
        /// <param name="idPersona">ID de la persona a desambiguar.</param>
        /// <param name="pListaIds">Listado de IDs de las personas iguales.</param>
        /// <param name="pDicIdPersona">Diccionario con el ID de la persona y el objeto al que corresponde.</param>
        /// <param name="pListaPersonasCreadas">Diccionario con el objeto creado y la lista de IDs de las personas que le corresponden.</param>
        public static void CrearPersonDesambiguada(string idPersona, HashSet<string> pListaIds, Dictionary<string, Person> pDicIdPersona, Dictionary<Person, HashSet<string>> pListaPersonasCreadas)
        {
            Person personaA = pDicIdPersona[idPersona];
            foreach (string idSimilar in pListaIds)
            {
                Person personaB = pDicIdPersona[idSimilar];
                FusionarPerson(personaA, personaB);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idPersona);
            pListaPersonasCreadas.Add(personaA, listaTotalIds);
        }

        /// <summary>
        /// Compara las propiedades para rellenar los datos faltantes.
        /// </summary>
        /// <param name="pPersonaA">Primera persona (A).</param>
        /// <param name="pPersonaB">Segunda persona (B).</param>
        public static void FusionarPerson(Person pPersonaA, Person pPersonaB)
        {
            if (string.IsNullOrEmpty(pPersonaA.Roh_ORCID) && !string.IsNullOrEmpty(pPersonaB.Roh_ORCID))
            {
                pPersonaA.Roh_ORCID = pPersonaB.Roh_ORCID;
            }

            if (string.IsNullOrEmpty(pPersonaA.Vivo_researcherId) && !string.IsNullOrEmpty(pPersonaB.Vivo_researcherId))
            {
                pPersonaA.Vivo_researcherId = pPersonaB.Vivo_researcherId;
            }

            int nombreCompletoA = 0;
            int nombreCompletoB = 0;

            if (!string.IsNullOrEmpty(pPersonaA.Foaf_firstName) && !string.IsNullOrEmpty(pPersonaA.Foaf_lastName))
            {
                nombreCompletoA = $@"{pPersonaA.Foaf_firstName} {pPersonaA.Foaf_lastName}".Trim().Length;
            }

            if (!string.IsNullOrEmpty(pPersonaB.Foaf_firstName) && !string.IsNullOrEmpty(pPersonaB.Foaf_lastName))
            {
                nombreCompletoB = $@"{pPersonaB.Foaf_firstName} {pPersonaB.Foaf_lastName}".Trim().Length;
            }

            if (!string.IsNullOrEmpty(pPersonaA.Foaf_firstName) && !string.IsNullOrEmpty(pPersonaB.Foaf_firstName) && !string.IsNullOrEmpty(pPersonaA.Foaf_lastName) && !string.IsNullOrEmpty(pPersonaB.Foaf_lastName))
            {
                if ((nombreCompletoA < nombreCompletoB) || (nombreCompletoA == nombreCompletoB))
                {
                    pPersonaA.Foaf_firstName = pPersonaB.Foaf_firstName.Trim();
                    pPersonaA.Foaf_lastName = pPersonaB.Foaf_lastName.Trim();
                }
            }

            pPersonaA.Foaf_name = $@"{pPersonaA.Foaf_firstName} {pPersonaA.Foaf_lastName}";

            if (string.IsNullOrEmpty(pPersonaA.Roh_semanticScholarId) && !string.IsNullOrEmpty(pPersonaB.Roh_semanticScholarId))
            {
                pPersonaA.Roh_semanticScholarId = pPersonaB.Roh_semanticScholarId;
            }
        }

    }
}
