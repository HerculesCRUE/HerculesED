using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.UpdateKeywords.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Hercules.ED.UpdateKeywords
{
    public class Program
    {
        private static string RUTA_OAUTH = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config";
        private static ResourceApi mResourceApi = new ResourceApi(RUTA_OAUTH);
        private static CommunityApi mCommunityApi = new CommunityApi(RUTA_OAUTH);
        private static ConfigService configService = new ConfigService();

        static void Main(string[] args)
        {
            UtilKeywords utilKeywords = new UtilKeywords(mResourceApi, mCommunityApi, configService);

            while (true)
            {
                // Lista de los IDs de los recursos a hacer Matching.
                List<string> listaIds = utilKeywords.GetDocument();

                // Contador de publicaciones
                int contadorPub = 1;

                // Obtención del ID de MESH.
                foreach (string id in listaIds)
                {
                    FileLogger.Log($@"{DateTime.Now} ---------- Procesando publicación {contadorPub}/{listaIds.Count}");
                    Dictionary<string, string> dicEtiquetas = utilKeywords.GetKeywords(id);

                    foreach (KeyValuePair<string, string> etiquetaTag in dicEtiquetas)
                    {
                        string idEtiquetaAux = etiquetaTag.Key;

                        // 1.- Probamos con el término en "Exact Match".
                        List<string> listaAux = new List<string>() { etiquetaTag.Value.Replace(")", "").Replace("(", "") };
                        Dictionary<string, string> dicResultados = new Dictionary<string, string>();

                        if (!utilKeywords.ComprobarCaracteres(etiquetaTag.Value))
                        {
                            dicResultados = utilKeywords.SelectDataMesh(listaAux.ToArray(), true);
                        }

                        // 2.- Si no se ha encontrado resultado y el término contiene más de una palabra...
                        if (dicResultados.Count() == 0 && etiquetaTag.Value.Contains(" "))
                        {
                            // 2.1.- Buscamos por el término en "All fragments".
                            string[] partes = etiquetaTag.Value.Split(" ");

                            // Si la etiqueta tiene más de 3 palabras, la consideramos inválida para esta búsqueda
                            // debido al excesivo tamaño de la query. 
                            if (partes.Count() <= 3)
                            {
                                dicResultados = ConsultarDatos(utilKeywords, partes);
                            }

                            // 2.2.- Buscamos por combinación de palabras en "All fragments" en el caso que tenga más de dos.
                            if (dicResultados.Count() != 1 && partes.Count() >= 2)
                            {
                                for (int i = 0; i < partes.Length; i++)
                                {
                                    string parte1 = partes[i];
                                    if (utilKeywords.preposicionesEng.Contains(parte1) || utilKeywords.preposicionesEsp.Contains(parte1) || utilKeywords.ComprobarCaracteres(parte1))
                                    {
                                        continue;
                                    }

                                    for (int x = i + 1; x < partes.Length; x++)
                                    {
                                        string parte2 = partes[x];
                                        if (utilKeywords.preposicionesEng.Contains(parte2) || utilKeywords.preposicionesEsp.Contains(parte2) || utilKeywords.ComprobarCaracteres(parte2))
                                        {
                                            continue;
                                        }

                                        List<string> lista = new List<string>() { parte1, parte2 };
                                        string[] arrayParte = lista.ToArray();

                                        dicResultados = ConsultarDatos(utilKeywords, arrayParte);

                                        if (dicResultados.Count() == 1)
                                        {
                                            break;
                                        }
                                    }

                                    if (dicResultados.Count() == 1)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        // Obtencón de información de SNOMED.
                        List<Data> listaSnomed = new List<Data>();
                        Dictionary<string, List<Dictionary<string, string>>> dicIds = new Dictionary<string, List<Dictionary<string, string>>>();
                        foreach (KeyValuePair<string, string> item in dicResultados)
                        {
                            utilKeywords.InsertDataSnomed(item.Key, listaSnomed);

                            // Borra los términos obsoletos.
                            listaSnomed.RemoveAll(x => x.snomedTerm.obsolete == true);

                            // Relación IDs.
                            dicIds = new Dictionary<string, List<Dictionary<string, string>>>();
                            dicIds.Add(item.Key, new List<Dictionary<string, string>>());
                            foreach (Data itemSnomed in listaSnomed)
                            {
                                Dictionary<string, string> dicAux = new Dictionary<string, string>();
                                dicAux.Add(itemSnomed.snomedTerm.ui, string.Empty);
                                dicIds[item.Key].Add(dicAux);
                            }
                        }

                        // Cambio de modelo con los datos necesarios.                    
                        List<DataConcept> listaSnomedConcepts = utilKeywords.GetDataConcepts(listaSnomed);

                        // Carga de etiquetas.
                        if (listaSnomedConcepts != null)
                        {
                            foreach (DataConcept tag in listaSnomedConcepts)
                            {
                                utilKeywords.CargarDataConceptCompleto(tag, dicIds);
                            }
                        }

                        // Obtención de información de MESH.
                        List<DataConcept> listaMesh = new List<DataConcept>();
                        foreach (KeyValuePair<string, string> itemAux in dicResultados)
                        {
                            utilKeywords.InsertDataMesh(itemAux.Key, itemAux.Value, listaMesh);
                        }

                        // Carga de etiquetas.
                        foreach (DataConcept tag in listaMesh)
                        {
                            string idRecursoMesh = utilKeywords.CargarDataConceptCompleto(tag, dicIds);
                            utilKeywords.ModificarKeyword(id, "http://w3id.org/roh/keyWordConcept", idEtiquetaAux, idRecursoMesh);
                        }
                    }

                    // Borrar triple de obtención de etiquetas.
                    utilKeywords.ModificarGetKeywordDocument(id);

                    contadorPub++;
                }

                // Sleep de 1min...
                Thread.Sleep(60000);
            }
        }

        public static Dictionary<string, string> ConsultarDatos(UtilKeywords pUtilKeywords, string[] pPartes)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();

            // Buscar en el label y concept label

            string consulta = $@"
                        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> PREFIX meshv: <http://id.nlm.nih.gov/mesh/vocab#>
                        SELECT ?item ?label FROM <http://mesh.gnoss.com/edma>
                        WHERE {{{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            {pUtilKeywords.ContruirFiltro("label", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:concept ?x.
                            ?x rdfs:label ?labelConcept.
                            {pUtilKeywords.ContruirFiltro("labelConcept", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:concept ?x.
                            ?x meshv:term ?x2.
                            ?x2 meshv:prefLabel ?prefLabel. 
                            {pUtilKeywords.ContruirFiltro("prefLabel", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:concept ?x.
                            ?x meshv:term ?x2.
                            ?x2 meshv:altLabel ?altLabel. 
                            {pUtilKeywords.ContruirFiltro("altLabel", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:concept ?x.
                            ?x meshv:term ?x2.
                            ?x2 meshv:sortVersion ?sortVersion. 
                            {pUtilKeywords.ContruirFiltro("sortVersion", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:concept ?x.
                            ?x meshv:preferredConcept ?x2.
                            ?x2 meshv:prefLabel ?prefLabel. 
                            {pUtilKeywords.ContruirFiltro("prefLabel", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:concept ?x.
                            ?x meshv:preferredConcept ?x2.
                            ?x2 meshv:altLabel ?altLabel. 
                            {pUtilKeywords.ContruirFiltro("altLabel", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:concept ?x.
                            ?x meshv:preferredConcept ?x2.
                            ?x2 meshv:sortVersion ?sortVersion. 
                            {pUtilKeywords.ContruirFiltro("sortVersion", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:preferredConcept ?x.
                            ?x meshv:term ?x2.
                            ?x2 meshv:prefLabel ?prefLabel. 
                            {pUtilKeywords.ContruirFiltro("prefLabel", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:preferredConcept ?x.
                            ?x meshv:term ?x2.
                            ?x2 meshv:altLabel ?altLabel. 
                            {pUtilKeywords.ContruirFiltro("altLabel", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:preferredConcept ?x.
                            ?x meshv:term ?x2.
                            ?x2 meshv:sortVersion ?sortVersion. 
                            {pUtilKeywords.ContruirFiltro("sortVersion", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:preferredTerm ?x.
                            ?x meshv:prefLabel ?prefLabel. 
                            {pUtilKeywords.ContruirFiltro("prefLabel", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:preferredTerm ?x.
                            ?x meshv:altLabel ?altLabel. 
                            {pUtilKeywords.ContruirFiltro("altLabel", pPartes, false)}
                            }}UNION{{
                            ?item a meshv:TopicalDescriptor.
                            ?item rdfs:label ?label.
                            ?item meshv:preferredTerm ?x.
                            ?x meshv:sortVersion ?sortVersion. 
                            {pUtilKeywords.ContruirFiltro("sortVersion", pPartes, false)}
                            }}}}";


            dicResultados = pUtilKeywords.SelectDataMeshAllFragments(consulta);

            return dicResultados;
        }

        /// <summary>
        /// Clase FileLogger.
        /// </summary>
        public static class FileLogger
        {
            /// <summary>
            /// Sobreescribe el método Log para pintar el mensaje de error en un fichero.
            /// </summary>
            /// <param name="messsage"></param>
            public static void Log(string messsage)
            {
                using var fileStream = new FileStream(configService.GetLogPath(), FileMode.Append);
                using var writter = new StreamWriter(fileStream);
                writter.WriteLine(messsage);
            }
        }
    }
}
