﻿using Gnoss.ApiWrapper;
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
    class TrabajosJornadasSeminarios : DisambiguableEntity
    {
        public string Titulo { get; set; }
        public string Fecha { get; set; }
        public HashSet<string> Autores { get; set; }

        private static readonly DisambiguationDataConfig configTituloTraJorSem = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaTraJorSem = new(DisambiguationDataConfigType.equalsItem, 0.5f);
        private static readonly DisambiguationDataConfig configAutoresTraJorSem = new(DisambiguationDataConfigType.equalsItemList, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configTituloTraJorSem, "titulo", Titulo),
                new DisambiguationData(configFechaTraJorSem, "fecha", Fecha),
                new DisambiguationData(configAutoresTraJorSem, "autores", Autores)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDTraJorSem(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem, List<Entity> listadoAux)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosTraJorSem = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate group_concat(?autor;separator=""|"") as ?autores ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.trabajosJornSemTituloTrabajo}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.trabajosJornSemPubFecha}> ?itemDate }}
                                        OPTIONAL{{ 
                                            ?item <http://purl.org/ontology/bibo/authorList> ?authorList . 
                                            ?authorList <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor
                                        }}
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    TrabajosJornadasSeminarios trabajosJornadas = new TrabajosJornadasSeminarios
                    {
                        ID = fila["item"].value,
                        Titulo = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : ""
                    };

                    trabajosJornadas.Autores = new HashSet<string>();
                    if (fila.ContainsKey("autores"))
                    {
                        string[] autores = fila["autores"].value.Split("|");
                        foreach (string autor in autores)
                        {
                            trabajosJornadas.Autores.Add(autor);
                        }
                    }
                    resultadosTraJorSem.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), trabajosJornadas);
                }
            }

            HashSet<string> listaNombresTJS = new HashSet<string>();
            ConcurrentDictionary<string, List<Persona>> listaPersonasAux = new ConcurrentDictionary<string, List<Persona>>();

            //Selecciono el nombre completo o la firma.
            foreach (Entity item in listadoAux)
            {
                for (int i = 0; i < item.autores.Count; i++)
                {
                    listaNombresTJS.Add(item.autores[i].NombreBuscar);
                }
            }

            //Divido la lista en listas de 10 elementos
            List<List<string>> listaListaNombresTJS = UtilitySecciones.SplitList(listaNombresTJS.ToList(), 10).ToList();

            Parallel.ForEach(listaListaNombresTJS, new ParallelOptions { MaxDegreeOfParallelism = 5 }, firma =>
            {
                Dictionary<string, List<Persona>> personasBBDD = Utility.ObtenerPersonasFirma(pResourceApi, firma);
                foreach (KeyValuePair<string, List<Persona>> valuePair in personasBBDD)
                {
                    listaPersonasAux[valuePair.Key.Trim()] = valuePair.Value;
                }
            });

            //Divido la lista en listas de elementos
            List<List<string>> listaListasIdPersonasTJS = UtilitySecciones.SplitList(listaPersonasAux.SelectMany(x => x.Value).Select(x => x.personid).Distinct().ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListasIdPersonasTJS)
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
                    string selectTJS = $@"select * where{{ SELECT distinct ?item ?autor ";
                    string whereTJS = $@"where {{
                                        ?item a <http://purl.org/ontology/bibo/Document> . 
                                        ?item <http://purl.org/ontology/bibo/authorList> ?authorList . 
                                        ?authorList <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autorIn .                                        
                                        ?item <http://purl.org/ontology/bibo/authorList> ?authorList2 . 
                                        ?authorList2 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor .
                                        FILTER(?autorIn in (<{string.Join(">,<", lista)}>))                                        
                                    }}order by desc(?item) desc(?autor)}} offset {offset} limit {limit}";
                    SparqlObject resultDataX = pResourceApi.VirtuosoQuery(selectTJS, whereTJS, graph);
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
                    resultadosTraJorSem[persona.ID] = persona;
                }
            }

            return resultadosTraJorSem;
        }
    }
}
