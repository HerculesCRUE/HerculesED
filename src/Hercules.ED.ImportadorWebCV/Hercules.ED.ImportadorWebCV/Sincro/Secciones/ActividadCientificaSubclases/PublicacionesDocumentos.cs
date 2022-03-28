using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.DisambiguationEngine.Models;
using Hercules.ED.ImportadorWebCV.Models;
using Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class PublicacionesDocumentos : DisambiguableEntity
    {
        public string mTitle { get; set; }
        public string title
        {
            get
            {
                return mTitle;
            }
            set
            {
                if (value == null)
                {
                    mTitle = string.Empty;
                }
                else
                {
                    mTitle = value;
                }
            }
        }
        public string mFecha { get; set; }
        public string fecha
        {
            get
            {
                return mFecha;
            }
            set
            {
                if (value == null)
                {
                    mFecha = string.Empty;
                }
                else
                {
                    mFecha = value;
                }
            }
        }
        private HashSet<string> mAutores { get; set; }
        public HashSet<string> autores
        {
            get
            {
                return mAutores;
            }
            set
            {
                if (value == null)
                {
                    mAutores = new HashSet<string>();
                }
                else
                {
                    mAutores = value;
                }
            }
        }

        private static readonly DisambiguationDataConfig configDescripcion = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static readonly DisambiguationDataConfig configFecha = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static readonly DisambiguationDataConfig configAutores = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f
        };


        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>
            {
                new DisambiguationData()
                {
                    property = "descripcion",
                    config = configDescripcion,
                    value = title
                },

                new DisambiguationData()
                {
                    property = "fecha",
                    config = configFecha,
                    value = fecha
                },

                new DisambiguationData()
                {
                    property = "autores",
                    config = configAutores,
                    values = autores
                }
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
        public static Dictionary<string, DisambiguableEntity> GetBBDD(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem, List<Entity> listadoAux)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultados = new Dictionary<string, DisambiguableEntity>();

            //Divido la lista en listas de 1.000 elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), 1000).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate group_concat(?autor;separator=""|"") as ?autores ";
                string where = $@"where {{
                                        ?item a <http://purl.org/ontology/bibo/Document>. 
                                        ?item <{Variables.ActividadCientificaTecnologica.pubDocumentosPubTitulo}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.pubDocumentosPubFecha}> ?itemDate }}
                                        OPTIONAL{{ 
                                            ?item <http://purl.org/ontology/bibo/authorList> ?authorList . 
                                            ?authorList <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor
                                        }}
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    PublicacionesDocumentos publicacionesDocumentos = new PublicacionesDocumentos
                    {
                        ID = fila["item"].value,
                        title = fila["itemTitle"].value,
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : ""
                    };

                    publicacionesDocumentos.autores = new HashSet<string>();
                    if (fila.ContainsKey("autores"))
                    {
                        string[] autores = fila["autores"].value.Split("|");
                        foreach (string autor in autores)
                        {
                            publicacionesDocumentos.autores.Add(autor);
                        }
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), publicacionesDocumentos);
                }
            }

            HashSet<string> listaNombres = new HashSet<string>();
            ConcurrentDictionary<string, List<Persona>> listaPersonasAux = new ConcurrentDictionary<string, List<Persona>>();

            //Selecciono el nombre completo o la firma.
            foreach (Entity item in listadoAux)
            {
                for (int i = 0; i < item.autores.Count; i++)
                {
                    listaNombres.Add(item.autores[i].NombreBuscar);
                }
            }

            //Divido la lista en listas de 10 elementos
            List<List<string>> listaListaNombres = UtilitySecciones.SplitList(listaNombres.ToList(), 10).ToList();

            Parallel.ForEach(listaListaNombres, new ParallelOptions { MaxDegreeOfParallelism = 5 }, firma =>
           {
               Dictionary<string, List<Persona>> personasBBDD = Utility.ObtenerPersonasFirma(pResourceApi, firma);
               foreach (KeyValuePair<string, List<Persona>> valuePair in personasBBDD)
               {
                   listaPersonasAux[valuePair.Key.Trim()] = valuePair.Value;
               }
           });

            //Divido la lista en listas de 1.000 elementos
            List<List<string>> listaListasIdPersonas = UtilitySecciones.SplitList(listaPersonasAux.SelectMany(x => x.Value).Select(x => x.personid).ToList(), 1000).ToList();

            foreach (List<string> lista in listaListasIdPersonas)
            {
                string select = $@"SELECT distinct ?item group_concat(?autor;separator=""|"") as ?autores ";
                string where = $@"where {{
                                        ?item a <http://purl.org/ontology/bibo/Document> . 
                                        ?item <http://purl.org/ontology/bibo/authorList> ?authorList . 
                                        ?authorList <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autorIn .                                        
                                        ?item <http://purl.org/ontology/bibo/authorList> ?authorList2 . 
                                        ?authorList2 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor .
                                        FILTER(?autorIn in (<{string.Join(">,<", lista)}>))                                        
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    string doc = fila["item"].value;
                    HashSet<string> autores = new HashSet<string>();
                    if (fila.ContainsKey("autores"))
                    {
                        foreach (string autor in fila["autores"].value.Split("|"))
                        {
                            autores.Add(autor);
                        }
                    }
                    foreach (string autorIn in autores)
                    {
                        List<Persona> personas = listaPersonasAux.SelectMany(x => x.Value).Where(x => x.personid == autorIn).ToList();
                        foreach (Persona persona in personas)
                        {
                            persona.documentos.Add(doc);
                            persona.coautores.UnionWith(autores.Except(new List<string>() { persona.personid }));
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
                    resultados[persona.ID] = persona;
                }
            }

            return resultados;
        }

    }
}
