using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Hercules.ED.ImportExportCV.Models;
using Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class TrabajosCongresos : DisambiguableEntity
    {
        public string Titulo { get; set; }
        public string Fecha { get; set; }
        public HashSet<string> Autores { get; set; }

        private static readonly DisambiguationDataConfig configTituloTraCon = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaTraCon = new(DisambiguationDataConfigType.equalsItem, 0.5f);
        private static readonly DisambiguationDataConfig configAutoresTraCon = new(DisambiguationDataConfigType.equalsItemList, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configTituloTraCon, "titulo", Titulo),
                new DisambiguationData(configFechaTraCon, "fecha", Fecha),
                new DisambiguationData(configAutoresTraCon, "autores", Autores)
            };

            return data;
        }

        /// <summary>
        /// Devuelve las entidades de BBDD del <paramref name="pCVID"/> de con las propiedades de <paramref name="propiedadesItem"/>
        /// </summary>
        /// <param name="pResourceApi">pResourceApi</param>
        /// <param name="pCVID">pCVID</param>
        /// <param name="graph">graph</param>
        /// <param name="propiedadesItem">propiedadesItem</param>
        /// <returns></returns>
        public static Dictionary<string, DisambiguableEntity> GetBBDDTraCon(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem, List<Entity> listadoAux)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);
            Dictionary<string, DisambiguableEntity> resultadosTraCon = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate group_concat(?autor;separator=""|"") as ?autores ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.trabajosCongresosTitulo}> ?itemTitle .
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.trabajosCongresosPubFecha}> ?itemDate }} .
                                        OPTIONAL{{ 
                                            ?item <http://purl.org/ontology/bibo/authorList> ?authorList . 
                                            ?authorList <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor
                                        }}
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    TrabajosCongresos trabajosCongresos = new()
                    {
                        ID = fila["item"].value,
                        Titulo = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        Autores = new HashSet<string>()
                    };
                    if (fila.ContainsKey("autores"))
                    {
                        string[] filasAutores = fila["autores"].value.Split("|");
                        foreach (string autor in filasAutores)
                        {
                            trabajosCongresos.Autores.Add(autor);
                        }
                    }
                    resultadosTraCon.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), trabajosCongresos);
                }
            }
            HashSet<string> listaNombresTC = new HashSet<string>();
            ConcurrentDictionary<string, List<Persona>> listaPersonasAux = new();

            //Selecciono el nombre completo o la firma.
            foreach (Entity item in listadoAux)
            {
                for (int i = 0; i < item.autores.Count; i++)
                {
                    listaNombresTC.Add(item.autores[i].NombreBuscar);
                }
            }

            //Divido la lista en listas de 5 elementos
            List<List<string>> listaListaNombresTC = UtilitySecciones.SplitList(listaNombresTC.ToList(), 5).ToList();

            Parallel.ForEach(listaListaNombresTC, new ParallelOptions { MaxDegreeOfParallelism = 5 }, firma =>
            {
                Dictionary<string, List<Persona>> personasBBDD = Utility.ObtenerPersonasFirma(pResourceApi, firma);
                foreach (KeyValuePair<string, List<Persona>> valuePair in personasBBDD)
                {
                    listaPersonasAux[valuePair.Key.Trim()] = valuePair.Value;
                }
            });

            //Divido la lista en listas de elementos
            List<List<string>> listaListasIdPersonas = UtilitySecciones.SplitList(listaPersonasAux.SelectMany(x => x.Value).Select(x => x.personid).Distinct().ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListasIdPersonas)
            {
                //Selecciono los datos de grupos, proyectos comperititvos, proyectos no competitivos, organizaciones y departamentos para la posterior desambiguacion de la persona.
                Dictionary<string, HashSet<string>> departamentos = Utility.DatosDepartamentoPersona(lista);
                foreach (string persona in departamentos.Keys)
                {
                    List<Persona> listado = listaPersonasAux.SelectMany(x => x.Value).Where(x => x.personid.Equals(persona)).ToList();
                    foreach (Persona p in listado)
                    {
                        p.departamento.UnionWith(departamentos.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> organizaciones = Utility.DatosOrganizacionPersona(lista);
                foreach (string persona in organizaciones.Keys)
                {
                    List<Persona> listado = listaPersonasAux.SelectMany(x => x.Value).Where(x => x.personid.Equals(persona)).ToList();
                    foreach (Persona p in listado)
                    {
                        p.organizacion.UnionWith(organizaciones.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> grupos = Utility.DatosGrupoPersona(lista);
                foreach (string persona in grupos.Keys)
                {
                    List<Persona> listado = listaPersonasAux.SelectMany(x => x.Value).Where(x => x.personid.Equals(persona)).ToList();
                    foreach (Persona p in listado)
                    {
                        p.grupos.UnionWith(grupos.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> proyectos = Utility.DatosProyectoPersona(lista);
                foreach (string persona in proyectos.Keys)
                {
                    List<Persona> listado = listaPersonasAux.SelectMany(x => x.Value).Where(x => x.personid.Equals(persona)).ToList();
                    foreach (Persona p in listado)
                    {
                        p.proyectos.UnionWith(proyectos.Where(x => x.Key.Equals(persona)).Select(x => x.Value).FirstOrDefault());
                    }
                }

                Dictionary<string, HashSet<string>> docAutores = new Dictionary<string, HashSet<string>>();
                int offset = 0;
                int limit = 10000;
                while (true)
                {
                    string selectTC = $@"select * where{{ SELECT distinct ?item ?autor ";
                    string whereTC = $@"where {{
                                        ?item a <http://purl.org/ontology/bibo/Document> . 
                                        ?item <http://purl.org/ontology/bibo/authorList> ?authorList . 
                                        ?authorList <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autorIn .                                        
                                        ?item <http://purl.org/ontology/bibo/authorList> ?authorList2 . 
                                        ?authorList2 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor .
                                        FILTER(?autorIn in (<{string.Join(">,<", lista)}>))                                        
                                    }}order by desc(?item) desc(?autor)}} offset {offset} limit {limit}";
                    SparqlObject resultDataX = pResourceApi.VirtuosoQuery(selectTC, whereTC, graph);
                    foreach (Dictionary<string, Data> fila in resultDataX.results.bindings)
                    {
                        string doc = fila["item"].value;
                        string autor = fila["autor"].value;
                        if (!docAutores.ContainsKey(doc))
                        {
                            docAutores.Add(doc, new HashSet<string>());
                        }
                        docAutores[doc].Add(autor);
                    }
                    offset += limit;
                    if (resultDataX.results.bindings.Count < limit)
                    {
                        break;
                    }
                }


                foreach (string doc in docAutores.Keys)
                {
                    foreach (string autorIn in docAutores[doc])
                    {
                        List<Persona> personas = listaPersonasAux.SelectMany(x => x.Value).Where(x => x.personid == autorIn).ToList();
                        foreach (Persona persona in personas)
                        {
                            persona.documentos.Add(doc);
                            persona.coautores.UnionWith(docAutores[doc].Except(new List<string>() { persona.personid }));
                        }
                    }
                }
            }

            //Añado los autores de BBDD para la desambiguación
            for (int i = 0; i < listaPersonasAux.Count; i++)
            {
                foreach (Persona persona in listaPersonasAux.ElementAt(i).Value)
                {
                    persona.ID = persona.personid;
                    resultadosTraCon[persona.ID] = persona;
                }
            }

            return resultadosTraCon;
        }
    }
}
