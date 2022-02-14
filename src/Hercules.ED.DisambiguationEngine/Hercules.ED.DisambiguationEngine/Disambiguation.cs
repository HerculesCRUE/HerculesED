using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hercules.ED.DisambiguationEngine.Models
{
    public static class Disambiguation
    {
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configOAuth\OAuthV3.config");

        //TODO comentarios
        /// <summary>
        /// Frecuencia absoluta de los nombres
        /// </summary>
        private static Dictionary<string, int> mFrecuenciaNombres = null;
        /// <summary>
        /// Frecuencia relativa de los nombres
        /// </summary>
        private static Dictionary<string, float> mScoreNombres = null;
        private static Dictionary<string, float> mScoreNombresCalculado = null;
        //Valores entre 0.4 y 0.99
        private static float minimoScoreNombres = 0.4f;
        private static float maximoScoreNombres = 0.99f;

        private static Dictionary<string, int> FrecuenciaNombres
        {
            get
            {
                if (mFrecuenciaNombres == null)
                {
                    new Thread(delegate ()
                    {
                        try
                        {
                            while (true)
                            {
                                Dictionary<string, int> frecuenciaNombresAux = new Dictionary<string, int>();
                                Dictionary<string, float> scoreNombresAux = new Dictionary<string, float>();
                                Dictionary<string, float> scoreNombresCalculadoAux = new Dictionary<string, float>();
                                int limit = 10000;
                                int offset = 0;
                                int numPersonas = 0;
                                while (true)
                                {
                                    string select = "SELECT * WHERE { SELECT DISTINCT ?persona ?nombreCompleto FROM <http://gnoss.com/person.owl> ";
                                    string where = $@"WHERE {{
                                ?persona a <http://xmlns.com/foaf/0.1/Person>. 
                                ?persona <http://xmlns.com/foaf/0.1/name> ?nombreCompleto.                                
                            }} ORDER BY DESC(?persona) }} LIMIT {limit} OFFSET {offset}";
                                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
                                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                                    {
                                        offset += limit;
                                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                                        {
                                            numPersonas++;
                                            string nombreNormalizado = ObtenerTextosNombresNormalizados(fila["nombreCompleto"].value);
                                            foreach (string nombre in nombreNormalizado.Split(' '))
                                            {
                                                if (!frecuenciaNombresAux.ContainsKey(nombre))
                                                {
                                                    frecuenciaNombresAux[nombre] = 0;
                                                }
                                                frecuenciaNombresAux[nombre]++;
                                                if (nombre.Length > 0)
                                                {
                                                    if (!frecuenciaNombresAux.ContainsKey(nombre[0].ToString()))
                                                    {
                                                        frecuenciaNombresAux[nombre[0].ToString()] = 0;
                                                    }
                                                    frecuenciaNombresAux[nombre[0].ToString()]++;
                                                }
                                            }
                                        }
                                        if (resultadoQuery.results.bindings.Count < limit)
                                        {
                                            break;
                                        }
                                    }
                                }
                                frecuenciaNombresAux = frecuenciaNombresAux.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                                foreach (string nombre in frecuenciaNombresAux.Keys)
                                {
                                    scoreNombresAux[nombre] = (float)frecuenciaNombresAux[nombre] / numPersonas;
                                }

                                int max = frecuenciaNombresAux.Where(x => x.Key.Length > 1).Select(x => x.Value).Max();
                                int min = frecuenciaNombresAux.Where(x => x.Key.Length > 1).Select(x => x.Value).Min();
                                //Asignamos un valor a cada frecuencia
                                int numFrecuencias = frecuenciaNombresAux.Where(x => x.Key.Length > 1).Select(x => x.Value).Distinct().Count();
                                int i = 0;
                                Dictionary<int, float> frecuenciaValor = new Dictionary<int, float>();
                                foreach (int frecuencia in frecuenciaNombresAux.Where(x => x.Key.Length > 1).Select(x => x.Value).Reverse().Distinct())
                                {
                                    var f = (float)i / numFrecuencias;
                                    frecuenciaValor.Add(frecuencia, f);
                                    i++;
                                }
                                foreach (string nombre in frecuenciaNombresAux.Keys)
                                {
                                    if (nombre.Length == 1)
                                    {
                                        scoreNombresCalculadoAux[nombre] = maximoScoreNombres;
                                    }
                                    else
                                    {
                                        float x = minimoScoreNombres + (maximoScoreNombres - minimoScoreNombres) * frecuenciaValor[frecuenciaNombresAux[nombre]];
                                        if (x > maximoScoreNombres)
                                        {
                                            x = maximoScoreNombres;
                                        }
                                        if (x < minimoScoreNombres)
                                        {
                                            x = minimoScoreNombres;
                                        }
                                        scoreNombresCalculadoAux[nombre] = x;
                                    }
                                }
                                scoreNombresCalculadoAux = scoreNombresCalculadoAux.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                                mFrecuenciaNombres = frecuenciaNombresAux;
                                mScoreNombres = scoreNombresAux;
                                mScoreNombresCalculado = scoreNombresCalculadoAux;


                                //Se recalcula cada hora
                                Thread.Sleep(3600000);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }).Start();
                    while (mFrecuenciaNombres == null)
                    {
                        Thread.Sleep(1000);
                    }
                }
                return mFrecuenciaNombres;
            }
        }

        private static Dictionary<string, float> ScoreNombres
        {
            get
            {
                var x = FrecuenciaNombres;
                return mScoreNombres;
            }
        }

        private static Dictionary<string, float> ScoreNombresCalculado
        {
            get
            {
                var x = FrecuenciaNombres;
                return mScoreNombresCalculado;
            }
        }

        public static Dictionary<string, string> SimilarityBBDD(List<DisambiguableEntity> pItems, List<DisambiguableEntity> pItemBBDD, float pUmbral = 0.8f)
        {
            Dictionary<string, Dictionary<string, float>> listaEquivalencias = new Dictionary<string, Dictionary<string, float>>();
            if (pItemBBDD != null && pItemBBDD.Count > 0)
            {
                // Diccionario Nombres Personas Desnormalizadas
                Dictionary<string, string> dicNomPersonasDesnormalizadas = new Dictionary<string, string>();
                // Diccionario Titulos Desnormalizadas
                Dictionary<string, string> dicTitulosDesnormalizados = new Dictionary<string, string>();

                Dictionary<DisambiguableEntity, List<DisambiguationData>> disambiguationDataItemsACargar = GetDisambiguationData(pItems);
                Dictionary<DisambiguableEntity, List<DisambiguationData>> disambiguationDataItemsBBDD = GetDisambiguationData(pItemBBDD);

                // Diccionario con tipo de item y su listado correspondiente de los datos a desambiguar.
                Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> itemsPorTipoToLoad = ObtenerItemsPorTipo(disambiguationDataItemsACargar);
                // Diccionario con tipo de item y su listado correspondiente de los datos a desambiguar.
                Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> itemsPorTipoBBDD = ObtenerItemsPorTipo(disambiguationDataItemsBBDD);

                // Realizamos las comprobacions para ver si el input es correcto
                RealizarComprobaciones(itemsPorTipoToLoad, itemsPorTipoBBDD);

                foreach (string tipo in itemsPorTipoToLoad.Keys)
                {
                    List<KeyValuePair<DisambiguableEntity, List<DisambiguationData>>> itemsPorTipoBBDDList = itemsPorTipoBBDD[tipo].ToList();
                    Dictionary<DisambiguableEntity, List<DisambiguationData>> itemsBBDD = null;
                    if (itemsPorTipoBBDD != null)
                    {
                        itemsBBDD = new Dictionary<DisambiguableEntity, List<DisambiguationData>>();
                        if (itemsPorTipoBBDD.ContainsKey(tipo))
                        {
                            itemsBBDD = itemsPorTipoBBDD[tipo];
                        }
                    }
                    foreach (var itemA in itemsPorTipoToLoad[tipo])
                    {
                        listaEquivalencias[itemA.Key.ID] = new Dictionary<string, float>();
                        for (int i = 0; i < itemsPorTipoBBDD[tipo].Count; i++)
                        {
                            // Algoritmo de similaridad.
                            float similarity = GetSimilarity(itemA.Value, itemsPorTipoBBDDList[i].Value, dicNomPersonasDesnormalizadas, dicTitulosDesnormalizados, null);
                            if (similarity >= pUmbral)
                            {
                                listaEquivalencias[itemA.Key.ID][itemsPorTipoBBDDList[i].Key.ID] = similarity;
                            }
                        }
                    }
                }
            }

            Dictionary<string, string> listaEquivalenciasFinal = new Dictionary<string, string>();
            HashSet<string> idBBDDSeleccionados = new HashSet<string>();
            foreach (string id in listaEquivalencias.Keys)
            {
                listaEquivalenciasFinal[id] = "";
                if (listaEquivalencias[id].Count > 1)
                {
                    listaEquivalencias[id] = listaEquivalencias[id].OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                }
                foreach (string id2 in listaEquivalencias[id].Keys)
                {
                    //Si existe en otro sitio con mas puntuación no se añade
                    bool existeEnOtroSitio = false;
                    foreach (string idAux in listaEquivalencias.Keys)
                    {
                        if (idAux != id)
                        {
                            foreach (string id2Aux in listaEquivalencias[idAux].Keys)
                            {
                                if (id2 == id2Aux && listaEquivalencias[idAux][id2Aux] > listaEquivalencias[id][id2])
                                {
                                    existeEnOtroSitio = true;
                                }
                            }
                        }
                    }
                    //Si esta añadido no se añade
                    if (!existeEnOtroSitio && !idBBDDSeleccionados.Contains(id2))
                    {
                        listaEquivalenciasFinal[id] = id2;
                        idBBDDSeleccionados.Add(id2);
                    }                   
                }
            }

            foreach (DisambiguableEntity entity in pItems)
            {
                if (!listaEquivalenciasFinal.ContainsKey(entity.ID))
                {
                    listaEquivalenciasFinal[entity.ID] = "";
                }
            }
            return listaEquivalenciasFinal;
        }

        /// <summary>
        /// Proceso de desambiguación.
        /// </summary>
        /// <param name="pItems">Lista de datos a desambiguar.</param>
        /// <returns>Lista de datos desambiguables.</returns>
        public static Dictionary<string, HashSet<string>> Disambiguate(List<DisambiguableEntity> pItems, List<DisambiguableEntity> pItemBBDD, bool pDisambiguateItems = true, float pUmbral = 0.8f)
        {
            //TODO tener en cuenta las probabilidades cuando se apunta a entidades (autores de docs) (test 2 docs con 2 autores)
            //TODO block
            //TODO tener en cuenta negativos

            //En esta variable se almacenarán todas las equivalencias encontradas con su peso
            Dictionary<string, Dictionary<string, float>> listaEquivalencias = new Dictionary<string, Dictionary<string, float>>();

            //En esta variable se almacenarán todas las entidades que nunca podrán ser equivalentes
            Dictionary<string, HashSet<string>> listaDistintos = new Dictionary<string, HashSet<string>>();
            foreach (DisambiguableEntity item in pItems)
            {
                if (item.distincts != null)
                {
                    //TODO mover a funcion
                    foreach (string distinct in item.distincts)
                    {
                        if (!listaDistintos.ContainsKey(distinct))
                        {
                            listaDistintos[item.ID] = new HashSet<string>();
                        }
                        if (!listaDistintos.ContainsKey(distinct))
                        {
                            listaDistintos[distinct] = new HashSet<string>();
                        }
                        listaDistintos[item.ID].Add(distinct);
                        listaDistintos[distinct].Add(item.ID);
                    }
                }
            }


            //Obtenemos las propiedades que nos permitirán la desambiguación.
            Dictionary<DisambiguableEntity, List<DisambiguationData>> disambiguationDataItemsACargar = GetDisambiguationData(pItems);
            Dictionary<DisambiguableEntity, List<DisambiguationData>> disambiguationDataItemsBBDD = GetDisambiguationData(pItemBBDD);

            //Creamos una variable en la que estarán los datos de desambiguación de las entidades a cargar
            //se irá modificando según se vaya realizando la desambiguación
            Dictionary<DisambiguableEntity, List<DisambiguationData>> disambiguationDataItemsACargarAux = new Dictionary<DisambiguableEntity, List<DisambiguationData>>(disambiguationDataItemsACargar);

            bool cambios = true;
            while (cambios)
            {
                cambios = false;
                if (pDisambiguateItems)
                {
                    // 1º Aplicamos la desambiguación con los items a cargar
                    bool cambiosItems = true;
                    while (cambiosItems)
                    {
                        Dictionary<string, Dictionary<string, float>> listaEquivalenciasItemsACargar = ApplyDisambiguation(disambiguationDataItemsACargarAux, null, listaDistintos);
                        cambiosItems = ProcesarEquivalencias(pItems, null, listaEquivalenciasItemsACargar, disambiguationDataItemsACargarAux, disambiguationDataItemsBBDD, listaEquivalencias, pUmbral);
                        if (cambiosItems)
                        {
                            cambios = true;
                        }
                    }


                }
                if (pItemBBDD != null && pItemBBDD.Count > 0)
                {
                    //2º Aplicamos la desambiguación con los items de BBDD
                    bool cambiosBBDD = true;
                    while (cambiosBBDD)
                    {
                        Dictionary<string, Dictionary<string, float>> listaEquivalenciasItemsACargar = ApplyDisambiguation(disambiguationDataItemsACargarAux, disambiguationDataItemsBBDD, listaDistintos);
                        cambiosBBDD = ProcesarEquivalencias(pItems, pItemBBDD, listaEquivalenciasItemsACargar, disambiguationDataItemsACargarAux, disambiguationDataItemsBBDD, listaEquivalencias, pUmbral);
                        if (cambiosBBDD)
                        {
                            cambios = true;
                        }
                    }
                }
            }

            //Preparamos el objeto a devolver
            Dictionary<string, HashSet<string>> listadoEquivalencias = new Dictionary<string, HashSet<string>>();
            //Comprobamos errores
            foreach (string id in listaEquivalencias.Keys)
            {
                List<string> itemsBBDD = listaEquivalencias[id].Keys.Where(x => !Guid.TryParse(x.Split('|')[1], out Guid auxB)).ToList();
                if (itemsBBDD.Count > 1)
                {
                    throw new Exception("Error, no puede un item apuntar a más de un ítem de BBDD");
                }
            }
            //Añadimos los de BBDD
            Dictionary<string, Tuple<string, float>> itemsToBBDD = new Dictionary<string, Tuple<string, float>>();
            foreach (string id in listaEquivalencias.Keys)
            {
                bool idBBDD = !Guid.TryParse(id.Split('|')[1], out Guid auxA);
                foreach (string id2 in listaEquivalencias[id].Keys)
                {
                    bool id2BBDD = !Guid.TryParse(id2.Split('|')[1], out Guid auxB);
                    if (idBBDD && id2BBDD)
                    {
                        throw new Exception("Error, no puede un item apuntar a más de un ítem de BBDD");
                    }
                    float similitud = listaEquivalencias[id][id2];
                    string idLoad = null;
                    string idDataBase = null;
                    if (idBBDD && !id2BBDD)
                    {
                        idLoad = id2;
                        idDataBase = id;
                    }
                    else if (!idBBDD && id2BBDD)
                    {
                        idLoad = id;
                        idDataBase = id2;
                    }
                    else
                    {
                        continue;
                    }
                    if (!itemsToBBDD.ContainsKey(idLoad))
                    {
                        itemsToBBDD[idLoad] = new Tuple<string, float>(idDataBase, similitud);
                    }
                    else if (itemsToBBDD[idLoad].Item2 < similitud)
                    {
                        itemsToBBDD[idLoad] = new Tuple<string, float>(idDataBase, similitud);
                    }
                }
            }
            foreach (string itemLoad in itemsToBBDD.Keys)
            {
                string itemBBDD = itemsToBBDD[itemLoad].Item1;
                if (!listadoEquivalencias.ContainsKey(itemBBDD))
                {
                    listadoEquivalencias.Add(itemBBDD, new HashSet<string>());
                }
                listadoEquivalencias[itemBBDD].Add(itemLoad);
            }

            Dictionary<string, HashSet<string>> itemsToLoad = new Dictionary<string, HashSet<string>>();
            //Añadimos los que no son de BBDD
            foreach (string id in listaEquivalencias.Keys)
            {
                HashSet<string> ids = new HashSet<string>();
                bool idBBDD = !Guid.TryParse(id.Split('|')[1], out Guid auxA);
                if (!idBBDD)
                {
                    ids.Add(id);
                }
                foreach (string id2 in listaEquivalencias[id].Keys)
                {
                    bool id2BBDD = !Guid.TryParse(id2.Split('|')[1], out Guid auxB);
                    if (!id2BBDD)
                    {
                        ids.Add(id2);
                    }
                }
                List<KeyValuePair<string, HashSet<string>>> x = itemsToLoad.Where(x => x.Value.Intersect(ids).Count() > 0).ToList();
                if (x.Count == 0)
                {
                    itemsToLoad[Guid.NewGuid().ToString()] = ids;
                }
                else if (x.Count == 1)
                {
                    x[0].Value.UnionWith(ids);
                }
                else if (x.Count > 1)
                {
                    //Si en alguno hay mas de uno eliminamos los que nos dan conflicto de lo que vamos a cargar
                    List<string> idsX = x.SelectMany(x => x.Value).ToList();
                    ids.ExceptWith(idsX);
                    itemsToLoad[Guid.NewGuid().ToString()] = ids;
                }
            }
            foreach (string itemLoad in itemsToLoad.Keys)
            {
                List<KeyValuePair<string, HashSet<string>>> x = listadoEquivalencias.Where(x => x.Value.Intersect(itemsToLoad[itemLoad]).Count() > 0).ToList();
                if (x.Count == 0)
                {
                    listadoEquivalencias.Add(Guid.NewGuid().ToString(), itemsToLoad[itemLoad]);
                }
                else if (x.Count == 1)
                {
                    x[0].Value.UnionWith(itemsToLoad[itemLoad]);
                }
                else if (x.Count > 1)
                {
                    //Si en alguno hay mas de uno eliminamos los que nos dan conflicto de lo que vamos a cargar
                    List<string> idsX = x.SelectMany(x => x.Value).ToList();
                    itemsToLoad[itemLoad].ExceptWith(idsX);
                    listadoEquivalencias[Guid.NewGuid().ToString()] = itemsToLoad[itemLoad];
                }
            }

            HashSet<string> itemsDevolver = new HashSet<string>(listadoEquivalencias.Values.SelectMany(x => x));
            foreach (DisambiguableEntity item in pItems)
            {
                if (!itemsDevolver.Contains(item.GetType().Name + "|" + item.ID))
                {
                    listadoEquivalencias.Add(Guid.NewGuid().ToString(), new HashSet<string>() { item.GetType().Name + "|" + item.ID });
                }
            }

            //Verificar que estan todos y no estan repetidos
            HashSet<string> idsResultantes = new HashSet<string>();
            foreach (string id in listadoEquivalencias.Keys)
            {
                foreach (string iditem in listadoEquivalencias[id])
                {
                    if (!idsResultantes.Add(iditem))
                    {
                        throw new Exception("Item repetido");
                    }
                }
            }
            if (idsResultantes.Count != pItems.Count)
            {
                throw new Exception("Error");
            }
            return listadoEquivalencias;
        }

        /// <summary>
        /// Procesa las equivalencias
        /// </summary>
        /// <param name="pItems">Items originales</param>
        /// <param name="pItemsBBDD">Items de BBDD</param>
        /// <param name="pListaEquivalenciasItemsACargar">Resultado de la desmbiguación</param>
        /// <param name="pDisambiguationDataItemsACargar">Items con los datos de desambiguación</param>
        /// <param name="pDisambiguationDataItemsACargarBBDD">Items con los datos de desambiguación de BBDD</param>
        /// <param name="pListaEquivalencias">Lista de equivalencias</param>
        /// <param name="pUmbral">Umbral</param>
        private static bool ProcesarEquivalencias(List<DisambiguableEntity> pItems, List<DisambiguableEntity> pItemsBBDD, Dictionary<string, Dictionary<string, float>> pListaEquivalenciasItemsACargar,
            Dictionary<DisambiguableEntity, List<DisambiguationData>> pDisambiguationDataItemsACargar, Dictionary<DisambiguableEntity, List<DisambiguationData>> pDisambiguationDataItemsACargarBBDD, Dictionary<string, Dictionary<string, float>> pListaEquivalencias, float pUmbral)
        {
            Dictionary<string, DisambiguableEntity> dicItems = new Dictionary<string, DisambiguableEntity>();
            foreach (DisambiguableEntity item in pItems)
            {
                dicItems.Add(item.GetType().Name + "|" + item.ID, item);
            }
            if (pItemsBBDD != null)
            {
                foreach (DisambiguableEntity item in pItemsBBDD)
                {
                    dicItems.Add(item.GetType().Name + "|" + item.ID, item);
                }
            }

            bool cambios = false;
            Dictionary<string, string> cambiosReferencias = new Dictionary<string, string>();
            //Obtenemos las equivalencias que superan el umbral
            foreach (string idA in pListaEquivalenciasItemsACargar.Keys)
            {
                string idAtype = idA.Split('|')[0];
                string idAidentifier = idA.Split('|')[1];
                foreach (string idB in pListaEquivalenciasItemsACargar[idA].Keys)
                {
                    string idBtype = idB.Split('|')[0];
                    string idBidentifier = idB.Split('|')[1];

                    if (pListaEquivalenciasItemsACargar[idA][idB] > pUmbral)
                    {
                        cambios = true;
                        //TODO priorizar BBDD ¿que pasa sin en los que vamos a cargar esta el mismo ID dos veces?

                        //Obtenemos ItemA
                        DisambiguableEntity itemA = dicItems[idAtype + "|" + idAidentifier];
                        //Obtenemos ItemB
                        DisambiguableEntity itemB = dicItems[idBtype + "|" + idBidentifier];

                        //TODO eliminar
                        if (itemA.GetType().Name == "DisambiguationPerson")
                        {
                            List<DisambiguationData> A = itemA.GetDisambiguationData();
                            List<DisambiguationData> B = itemB.GetDisambiguationData();
                            if (!string.IsNullOrEmpty(A.FirstOrDefault(x => x.property == "orcid").value) && !string.IsNullOrEmpty(B.FirstOrDefault(x => x.property == "orcid").value) && A.FirstOrDefault(x => x.property == "orcid").value != B.FirstOrDefault(x => x.property == "orcid").value)
                            {

                            }
                            if (
                                (!A.FirstOrDefault(x => x.property == "completeName").value.ToLower().Contains("skarmeta") || !B.FirstOrDefault(x => x.property == "completeName").value.ToLower().Contains("skarmeta"))
                                && A.FirstOrDefault(x => x.property == "completeName").value.ToLower().Replace("-", " ") != B.FirstOrDefault(x => x.property == "completeName").value.ToLower().Replace("-", " ")
                                && (string.IsNullOrEmpty(A.FirstOrDefault(x => x.property == "orcid").value) || string.IsNullOrEmpty(A.FirstOrDefault(x => x.property == "orcid").value))
                                )
                            {

                            }
                        }

                        bool itemABBDD = !Guid.TryParse(idAidentifier, out Guid aux);
                        bool itemBBBDD = !Guid.TryParse(idBidentifier, out Guid aux2);

                        //Si los dos son items de BBDD no hay que hacer nada
                        if (itemABBDD && itemBBBDD)
                        {
                            continue;
                        }

                        //Nos quedamos con el 'mejor'
                        DisambiguableEntity itemEliminar = null;
                        DisambiguableEntity itemBueno = null;
                        string idMalo = null;
                        string idBueno = null;
                        if (itemABBDD || itemBBBDD)
                        {
                            List<DisambiguationData> dataA = null;
                            List<DisambiguationData> dataB = null;
                            if (itemABBDD && pDisambiguationDataItemsACargar.ContainsKey(itemB))
                            {
                                dataA = pDisambiguationDataItemsACargarBBDD[itemA];
                                dataB = pDisambiguationDataItemsACargar[itemB];
                                itemEliminar = itemB;
                                itemBueno = itemA;
                                idMalo = idBidentifier;
                                idBueno = idAidentifier;
                            }
                            else if (itemBBBDD && pDisambiguationDataItemsACargar.ContainsKey(itemA))
                            {
                                dataB = pDisambiguationDataItemsACargarBBDD[itemB];
                                dataA = pDisambiguationDataItemsACargar[itemA];
                                itemEliminar = itemA;
                                itemBueno = itemB;
                                idMalo = idAidentifier;
                                idBueno = idBidentifier;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (pDisambiguationDataItemsACargar.ContainsKey(itemA) && pDisambiguationDataItemsACargar.ContainsKey(itemB))
                        {
                            List<DisambiguationData> dataA = pDisambiguationDataItemsACargar[itemA];
                            List<DisambiguationData> dataB = pDisambiguationDataItemsACargar[itemB];

                            int identifiersA = dataA.Where(x => x.config.type == DisambiguationDataConfigType.equalsIdentifiers).Count();
                            int identifiersB = dataB.Where(x => x.config.type == DisambiguationDataConfigType.equalsIdentifiers).Count();

                            int? nombreA = dataA.Where(x => x.config.type == DisambiguationDataConfigType.algoritmoNombres).FirstOrDefault()?.value.Length;
                            int? nombreB = dataB.Where(x => x.config.type == DisambiguationDataConfigType.algoritmoNombres).FirstOrDefault()?.value.Length;

                            int itemsA = dataA.Where(x => x.config.type == DisambiguationDataConfigType.equalsItem).Count();
                            int itemsB = dataB.Where(x => x.config.type == DisambiguationDataConfigType.equalsItem).Count();

                            if (identifiersA > identifiersB)
                            {
                                itemEliminar = itemB;
                                itemBueno = itemA;
                                idMalo = idBidentifier;
                                idBueno = idAidentifier;
                            }
                            else if (identifiersB > identifiersA)
                            {
                                itemEliminar = itemA;
                                itemBueno = itemB;
                                idMalo = idAidentifier;
                                idBueno = idBidentifier;
                            }
                            else if (nombreA > nombreB)
                            {
                                itemEliminar = itemB;
                                itemBueno = itemA;
                                idMalo = idBidentifier;
                                idBueno = idAidentifier;
                            }
                            else if (nombreB > nombreA)
                            {
                                itemEliminar = itemA;
                                itemBueno = itemB;
                                idMalo = idAidentifier;
                                idBueno = idBidentifier;
                            }
                            else if (itemsA > itemsB)
                            {
                                itemEliminar = itemB;
                                itemBueno = itemA;
                                idMalo = idBidentifier;
                                idBueno = idAidentifier;
                            }
                            else if (itemsB > itemsA)
                            {
                                itemEliminar = itemA;
                                itemBueno = itemB;
                                idMalo = idAidentifier;
                                idBueno = idBidentifier;
                            }
                            else
                            {
                                itemEliminar = itemB;
                                itemBueno = itemA;
                                idMalo = idBidentifier;
                                idBueno = idAidentifier;
                            }
                        }
                        else if (pDisambiguationDataItemsACargar.ContainsKey(itemA))
                        {
                            itemEliminar = itemB;
                            itemBueno = itemA;
                            idMalo = idBidentifier;
                            idBueno = idAidentifier;
                        }
                        else if (pDisambiguationDataItemsACargar.ContainsKey(itemB))
                        {
                            itemEliminar = itemA;
                            itemBueno = itemB;
                            idMalo = idAidentifier;
                            idBueno = idBidentifier;
                        }
                        else
                        {
                            continue;
                        }

                        if (!pListaEquivalencias.ContainsKey(idA))
                        {
                            pListaEquivalencias.Add(idA, new Dictionary<string, float>());
                        }
                        if (!pListaEquivalencias.ContainsKey(idB))
                        {
                            pListaEquivalencias.Add(idB, new Dictionary<string, float>());
                        }
                        pListaEquivalencias[idA][idB] = pListaEquivalenciasItemsACargar[idA][idB];
                        pListaEquivalencias[idB][idA] = pListaEquivalenciasItemsACargar[idB][idA];
                        //Agregamos el listado del eliminado en el bueno
                        List<DisambiguationData> datosBueno = null;
                        if (pDisambiguationDataItemsACargar.ContainsKey(itemBueno))
                        {
                            datosBueno = pDisambiguationDataItemsACargar[itemBueno];
                        }
                        else if (pDisambiguationDataItemsACargarBBDD.ContainsKey(itemBueno))
                        {
                            datosBueno = pDisambiguationDataItemsACargarBBDD[itemBueno];
                        }
                        foreach (DisambiguationData dataBueno in datosBueno)
                        {
                            if (pDisambiguationDataItemsACargar.ContainsKey(itemEliminar))
                            {
                                foreach (DisambiguationData dataMalo in pDisambiguationDataItemsACargar[itemEliminar])
                                {
                                    if (dataBueno.property == dataMalo.property && dataBueno.config.type == DisambiguationDataConfigType.equalsItemList)
                                    {
                                        dataBueno.values.UnionWith(dataMalo.values);
                                    }
                                    if (dataBueno.property == dataMalo.property && dataBueno.config.type == DisambiguationDataConfigType.equalsIdentifiers && !string.IsNullOrEmpty(dataMalo.value))
                                    {
                                        dataBueno.value = dataMalo.value;
                                    }
                                }
                            }
                        }
                        //Cambiamos referencias
                        cambiosReferencias[idMalo] = idBueno;

                        //Eliminamos de la lista de items el duplicado
                        pDisambiguationDataItemsACargar.Remove(itemEliminar);
                    }
                }
            }
            //Cambiamos referencias
            foreach (DisambiguableEntity item in pDisambiguationDataItemsACargar.Keys)
            {
                foreach (DisambiguationData data in pDisambiguationDataItemsACargar[item])
                {
                    foreach (string antiguo in cambiosReferencias.Keys)
                    {
                        if (data.value == antiguo)
                        {
                            data.value = cambiosReferencias[antiguo];
                        }
                        if (data.values != null && data.values.Contains(antiguo))
                        {
                            data.values.Remove(antiguo);
                            data.values.Add(cambiosReferencias[antiguo]);
                        }
                    }
                }
            }
            return cambios;
        }

        /// <summary>
        /// Obtiene y clasifica los datos a desambiguar.
        /// </summary>
        /// <param name="pItems"></param>
        /// <returns>Diccionario con el tipo de dato y la lista de datos a desambiguar.</returns>
        private static Dictionary<DisambiguableEntity, List<DisambiguationData>> GetDisambiguationData(List<DisambiguableEntity> pItems)
        {
            Dictionary<DisambiguableEntity, List<DisambiguationData>> disambiguationData = new Dictionary<DisambiguableEntity, List<DisambiguationData>>();
            if (pItems != null)
            {
                foreach (DisambiguableEntity item in pItems)
                {
                    disambiguationData[item] = item.GetDisambiguationData();
                }
            }
            return disambiguationData;
        }

        /// <summary>
        /// Aplica la desambiguación de los datos.
        /// </summary>
        /// <param name="pItemsToLoad">Diccionario con el tipo de dato y la lista de los datos a desambiguar.</param>
        /// <param name="pItemsBBDD">Diccionario con el tipo de dato y la lista de los datos de BBDD.</param>
        /// <param name="pListaDistintos">Diccionario con los items que no pueden ser iguales</param>
        /// <returns>Listado con las equivalencias y su peso.</returns>
        private static Dictionary<string, Dictionary<string, float>> ApplyDisambiguation(Dictionary<DisambiguableEntity, List<DisambiguationData>> pItemsToLoad, Dictionary<DisambiguableEntity, List<DisambiguationData>> pItemsBBDD, Dictionary<string, HashSet<string>> pListaDistintos)
        {
            //TODO los ids de bbdd no hay que compararlos con nada

            // Respuesta: Diccionario con los IDs equivalentes.
            Dictionary<string, Dictionary<string, float>> equivalences = new Dictionary<string, Dictionary<string, float>>();

            // Diccionario con tipo de item y su listado correspondiente de los datos a desambiguar.
            Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> itemsPorTipoToLoad = ObtenerItemsPorTipo(pItemsToLoad);
            // Diccionario con tipo de item y su listado correspondiente de los datos a desambiguar.
            Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> itemsPorTipoBBDD = ObtenerItemsPorTipo(pItemsBBDD);

            // Realizamos las comprobacions para ver si el input es correcto
            //TODO mover de sitio
            RealizarComprobaciones(itemsPorTipoToLoad, itemsPorTipoBBDD);

            for (int i = 0; i < 2; i++)
            {
                foreach (string tipo in itemsPorTipoToLoad.Keys)
                {
                    Dictionary<DisambiguableEntity, List<DisambiguationData>> itemsBBDD = null;
                    if (itemsPorTipoBBDD != null)
                    {
                        itemsBBDD = new Dictionary<DisambiguableEntity, List<DisambiguationData>>();
                        if (itemsPorTipoBBDD.ContainsKey(tipo))
                        {
                            itemsBBDD = itemsPorTipoBBDD[tipo];
                        }
                    }
                    //TODO mover para que lo usen todos los tipos
                    Dictionary<string, Dictionary<string, float>> equivalencesAyuda = null;
                    if (i == 1)
                    {
                        equivalencesAyuda = new Dictionary<string, Dictionary<string, float>>();
                        foreach (string key in equivalences.Keys)
                        {
                            equivalencesAyuda.Add(key.Split('|')[1], new Dictionary<string, float>());
                            foreach (string key2 in equivalences[key].Keys)
                            {
                                equivalencesAyuda[key.Split('|')[1]].Add(key2.Split('|')[1], equivalences[key][key2]);
                            }
                        }
                    }
                    Dictionary<string, Dictionary<string, float>> equivalencesAux = ProcesadoNormal(itemsPorTipoToLoad[tipo], itemsBBDD, tipo, equivalencesAyuda, pListaDistintos);
                    foreach (string key in equivalencesAux.Keys)
                    {
                        equivalences[key] = equivalencesAux[key];
                    }
                }
            }
            return equivalences;
        }

        ///// <summary>
        ///// Procesado especial de desambiguación (propiedades limitadas)
        ///// </summary>
        ///// <param name="pItemsToLoad">Items para desambiguar</param>
        ///// <param name="pItemsBBDD">Items para desambiguar de la BBDD</param>
        ///// <param name="pTipo">Tipo</param>
        ///// <returns></returns>
        //private static Dictionary<string, Dictionary<string, float>> ProcesadoEspecial(Dictionary<DisambiguableEntity, List<DisambiguationData>> pItemsToLoad, Dictionary<DisambiguableEntity, List<DisambiguationData>> pItemsBBDD, string pTipo)
        //{
        //    //Respuesta
        //    Dictionary<string, Dictionary<string, float>> equivalences = new Dictionary<string, Dictionary<string, float>>();

        //    //Propiedad - Valor - IDs
        //    Dictionary<string, Dictionary<string, HashSet<string>>> dicComparacionEqualsTitle = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        //    //Propiedad - ScorePositivo
        //    Dictionary<string, KeyValuePair<float, float>> dicComparacionEqualsTitleScore = new Dictionary<string, KeyValuePair<float, float>>();
        //    //ID - Propiedad - Valor
        //    Dictionary<string, Dictionary<string, string>> dicComparacionEqualsTitleValues = new Dictionary<string, Dictionary<string, string>>();

        //    //Propiedad - Valor - IDs
        //    Dictionary<string, Dictionary<string, HashSet<string>>> dicComparacionEqualsIdentifiers = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        //    //Propiedad - ScorePositivo/ScoreNegativo
        //    Dictionary<string, KeyValuePair<float, float>> dicComparacionEqualsIdentifiersScore = new Dictionary<string, KeyValuePair<float, float>>();
        //    //ID - Propiedad - Valor
        //    Dictionary<string, Dictionary<string, string>> dicComparacionEqualsIdentifiersValues = new Dictionary<string, Dictionary<string, string>>();

        //    //Propiedad - Valor - IDs
        //    Dictionary<string, Dictionary<string, HashSet<string>>> dicComparacionEqualsItem = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        //    //Propiedad - ScorePositivo/ScoreNegativo
        //    Dictionary<string, KeyValuePair<float, float>> dicComparacionEqualsItemScore = new Dictionary<string, KeyValuePair<float, float>>();
        //    //ID - Propiedad - Valor
        //    Dictionary<string, Dictionary<string, string>> dicComparacionEqualsItemValues = new Dictionary<string, Dictionary<string, string>>();

        //    HashSet<string> idsToCompare = new HashSet<string>();

        //    foreach (var itemA in pItemsToLoad)
        //    {
        //        idsToCompare.Add(itemA.Key.ID);
        //        for (int i = 0; i < itemA.Value.Count; i++)
        //        {
        //            switch (itemA.Value[i].config.@type)
        //            {
        //                case DisambiguationDataConfigType.equalsTitle:
        //                    ProcessItem(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, itemA, i, itemA.Value[i].config.@type);
        //                    break;
        //                case DisambiguationDataConfigType.equalsIdentifiers:
        //                    ProcessItem(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, itemA, i, itemA.Value[i].config.@type);
        //                    break;
        //                case DisambiguationDataConfigType.equalsItem:
        //                    ProcessItem(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, itemA, i, itemA.Value[i].config.@type);
        //                    break;
        //                default:
        //                    throw new Exception("No implementado");
        //            }
        //        }
        //    }

        //    bool compareBBDD = false;
        //    if (pItemsBBDD != null)
        //    {
        //        idsToCompare = new HashSet<string>();
        //        compareBBDD = true;
        //        foreach (var itemA in pItemsBBDD)
        //        {
        //            idsToCompare.Add(itemA.Key.ID);
        //            for (int i = 0; i < itemA.Value.Count; i++)
        //            {
        //                switch (itemA.Value[i].config.@type)
        //                {
        //                    case DisambiguationDataConfigType.equalsTitle:
        //                        ProcessItem(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, itemA, i, itemA.Value[i].config.@type);
        //                        break;
        //                    case DisambiguationDataConfigType.equalsIdentifiers:
        //                        ProcessItem(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, itemA, i, itemA.Value[i].config.@type);
        //                        break;
        //                    case DisambiguationDataConfigType.equalsItem:
        //                        ProcessItem(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, itemA, i, itemA.Value[i].config.@type);
        //                        break;
        //                    default:
        //                        throw new Exception("No implementado");
        //                }
        //            }
        //        }
        //    }

        //    int indice = 0;
        //    foreach (var itemA in pItemsToLoad)
        //    {
        //        indice++;
        //        string idA = itemA.Key.ID;
        //        if (!Guid.TryParse(idA, out Guid aux))
        //        {
        //            //Si es un ID de BBDD no jay que comparar
        //            continue;
        //        }

        //        //Obtenemos las equivalencias en función de los IDs
        //        Dictionary<string, float> dicEquivalenciasIdentifiers = new Dictionary<string, float>();
        //        CalculateEquivalencesPositives(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, dicEquivalenciasIdentifiers, idA, idsToCompare);
        //        CalculateEquivalencesNegatives(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, dicEquivalenciasIdentifiers, idA, idsToCompare);

        //        //Obtenemos las equivalencias de los títulos
        //        Dictionary<string, float> dicEquivalenciasTitle = new Dictionary<string, float>();
        //        CalculateEquivalencesPositives(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, dicEquivalenciasTitle, idA, idsToCompare);
        //        CalculateEquivalencesPositives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalenciasTitle, idA, idsToCompare);
        //        CalculateEquivalencesNegatives(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, dicEquivalenciasTitle, idA, idsToCompare);
        //        CalculateEquivalencesNegatives(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, dicEquivalenciasTitle, idA, idsToCompare);
        //        CalculateEquivalencesNegatives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalenciasTitle, idA, idsToCompare);

        //        //Obtenemos las equivalencias de los items
        //        Dictionary<string, float> dicEquivalenciasItem = new Dictionary<string, float>();
        //        CalculateEquivalencesPositives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalenciasItem, idA, idsToCompare);
        //        CalculateEquivalencesNegatives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalenciasItem, idA, idsToCompare);
        //        CalculateEquivalencesNegatives(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, dicEquivalenciasItem, idA, idsToCompare);
        //        CalculateEquivalencesNegatives(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, dicEquivalenciasItem, idA, idsToCompare);

        //        //Almacenamos las equivalencias en función de los IDs
        //        Dictionary<string, float> dicEquivalencias = new Dictionary<string, float>();
        //        foreach (string id in dicEquivalenciasIdentifiers.Keys)
        //        {
        //            if (dicEquivalenciasIdentifiers[id] > 0)
        //            {
        //                dicEquivalencias[id] = dicEquivalenciasIdentifiers[id];
        //            }
        //        }

        //        //Almacenamos las equivalencias en función de los Títulos
        //        foreach (string id in dicEquivalenciasTitle.Keys)
        //        {
        //            if (!dicEquivalencias.ContainsKey(id) && dicEquivalenciasTitle[id] > 0)
        //            {
        //                dicEquivalencias[id] = dicEquivalenciasTitle[id];
        //            }
        //        }

        //        //Almacenamos las equivalencias en función de los items
        //        foreach (string id in dicEquivalenciasItem.Keys)
        //        {
        //            if (!dicEquivalencias.ContainsKey(id) && dicEquivalenciasItem[id] > 0)
        //            {
        //                dicEquivalencias[id] = dicEquivalenciasItem[id];
        //            }
        //        }

        //        //Si hay mas de una equivalencia con BBDD sólo nos quedamos con la más alta
        //        int numBBDD = 0;
        //        float scoreBBDD = 0;
        //        string itemBBDD = null;
        //        foreach (string idB in dicEquivalencias.Keys)
        //        {
        //            if (!Guid.TryParse(idB, out Guid aux2))
        //            {
        //                numBBDD++;
        //            }
        //            if (dicEquivalencias[idB] > scoreBBDD)
        //            {
        //                itemBBDD = idB;
        //                scoreBBDD = dicEquivalencias[idB];
        //            }
        //        }
        //        foreach (string idB in dicEquivalencias.Keys)
        //        {
        //            if (!Guid.TryParse(idB, out Guid aux2))
        //            {
        //                if (idB == itemBBDD)
        //                {
        //                    AddSimilarityId(pTipo, equivalences, dicEquivalencias[idB], idA, idB);
        //                }
        //            }
        //            else
        //            {
        //                // Agregación de los IDs similares.
        //                AddSimilarityId(pTipo, equivalences, dicEquivalencias[idB], idA, idB);
        //            }
        //        }
        //    }
        //    return equivalences;
        //}

        /// <summary>
        /// Procesado especial de desambiguación (propiedades limitadas)
        /// </summary>
        /// <param name="pItemsToLoad">Items para desambiguar</param>
        /// <param name="pItemsBBDD">Items para desambiguar de la BBDD</param>
        /// <param name="pTipo">Tipo</param>
        /// <param name="pListaDistintos">Diccionario con la lista de los IDs que no puden ser iguales</param>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, float>> ProcesadoNormal(Dictionary<DisambiguableEntity, List<DisambiguationData>> pItemsToLoad, Dictionary<DisambiguableEntity, List<DisambiguationData>> pItemsBBDD, string pTipo, Dictionary<string, Dictionary<string, float>> pEquivalencesAux, Dictionary<string, HashSet<string>> pListaDistintos)
        {
            //Respuesta
            Dictionary<string, Dictionary<string, float>> equivalences = new Dictionary<string, Dictionary<string, float>>();

            #region Diccionarios auxiliares para la desmbiguación
            //TODO Diccionario Nombres Personas Desnormalizadas Mover afuera
            Dictionary<string, string> dicNomPersonasDesnormalizadas = new Dictionary<string, string>();
            //TODO Diccionario Titulos Desnormalizadas Mover afuera
            Dictionary<string, string> dicTitulosDesnormalizados = new Dictionary<string, string>();
            #endregion

            // Lista con los items del tipo actual
            List<KeyValuePair<DisambiguableEntity, List<DisambiguationData>>> listaItemsCompare = pItemsToLoad.ToList();
            bool compareBBDD = false;
            if (pItemsBBDD != null)
            {
                compareBBDD = true;
                listaItemsCompare = pItemsBBDD.ToList();
            }

            if (!compareBBDD)
            {
                int indice = 0;
                foreach (var itemA in pItemsToLoad)
                {
                    indice++;
                    for (int i = indice; i < listaItemsCompare.Count; i++)
                    {
                        string idA = itemA.Key.ID;
                        string idB = listaItemsCompare[i].Key.ID;
                        if (pListaDistintos.ContainsKey(idA) && pListaDistintos[idA].Contains(idB))
                        {
                            continue;
                        }
                        // Algoritmo de similaridad.
                        float similarity = GetSimilarity(itemA.Value, listaItemsCompare[i].Value, dicNomPersonasDesnormalizadas, dicTitulosDesnormalizados, pEquivalencesAux);

                        // Agregación de los IDs similares.
                        AddSimilarityId(pTipo, equivalences, similarity, idA, idB);
                    }
                }
            }

            if (compareBBDD)
            {
                foreach (var itemA in pItemsToLoad)
                {
                    //Si es un item de BBDD no hay que comparar
                    if (!Guid.TryParse(itemA.Key.ID, out Guid aux))
                    {
                        continue;
                    }
                    //Nos quedamos sólo con el que más se parezca
                    float similarityMax = 0;
                    string idA = itemA.Key.ID;
                    string idB = null;
                    //TODO pListaDistintos
                    for (int i = 0; i < listaItemsCompare.Count; i++)
                    {
                        // Algoritmo de similaridad.
                        float similarity = GetSimilarity(itemA.Value, listaItemsCompare[i].Value, dicNomPersonasDesnormalizadas, dicTitulosDesnormalizados, pEquivalencesAux);
                        if (similarity > similarityMax)
                        {
                            similarityMax = similarity;
                            idB = listaItemsCompare[i].Key.ID;
                        }
                    }
                    if (similarityMax > 0)
                    {
                        // Agregación de los IDs similares.
                        AddSimilarityId(pTipo, equivalences, similarityMax, idA, idB);
                    }
                }
            }

            return equivalences;
        }


        /// <summary>
        /// Obtiene in diccionario con los items separados por tipo
        /// </summary>
        /// <param name="pItems">Items</param>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> ObtenerItemsPorTipo(Dictionary<DisambiguableEntity, List<DisambiguationData>> pItems)
        {
            if (pItems != null)
            {
                Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> itemsPorTipo = new Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>>();
                foreach (var item in pItems)
                {
                    string name = item.Key.GetType().Name;
                    if (!itemsPorTipo.ContainsKey(name))
                    {
                        itemsPorTipo.Add(name, new Dictionary<DisambiguableEntity, List<DisambiguationData>>());
                    }
                    itemsPorTipo[name].Add(item.Key, item.Value);
                }
                return itemsPorTipo;
            }
            return null;
        }

        /// <summary>
        /// Raliza comprobaciones con los datos a desambiguar para verificar que la entrada es correcta
        /// </summary>
        /// <param name="pItemsDataToLoad">Datos de desambiguación</param>
        /// <param name="pItemsDataBBDD">Datos de desambiguación de BBDD</param>
        private static void RealizarComprobaciones(Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> pItemsDataToLoad, Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> pItemsDataBBDD)
        {
            List<Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>>> pItemsDataComprobar = new List<Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>>>();
            pItemsDataComprobar.Add(pItemsDataToLoad);
            pItemsDataComprobar.Add(pItemsDataBBDD);

            bool block = false;
            foreach (Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> itemsData in pItemsDataComprobar)
            {
                if (itemsData != null && itemsData.Count > 0)
                {
                    //Valores de propiedades
                    HashSet<string> ids = new HashSet<string>();
                    foreach (string tipo in itemsData.Keys)
                    {
                        if (itemsData[tipo].Count > 0)
                        {
                            foreach (DisambiguableEntity entity in itemsData[tipo].Keys)
                            {
                                entity.block = block;
                                if (block)
                                {
                                    //El ID no debe contener '|' y debe empezar por http
                                    if (entity.ID.Contains("|") || !entity.ID.StartsWith("http"))
                                    {
                                        throw new Exception("Los IDs de los items de BBDD no deben contener '|' y deben empezar por http");
                                    }

                                }
                                else
                                {
                                    //El ID debe ser un guid
                                    if (!Guid.TryParse(entity.ID, out Guid aux))
                                    {
                                        throw new Exception("Los IDs de los items de BBDD no deben contener '|' y deben empezar por http");
                                    }
                                }
                            }
                            List<DisambiguationData> data = itemsData[tipo].First().Value;
                            if (data.Select(x => x.property).Distinct().Count() != data.Count())
                            {
                                throw new Exception("En los items " + tipo + " hay propiedades repetidas");
                            }
                            if (data.Select(x => x.config.type).Contains(DisambiguationDataConfigType.algoritmoNombres) && data.Select(x => x.config.type).Contains(DisambiguationDataConfigType.equalsTitle))
                            {
                                throw new Exception("En los items " + tipo + " hay configurado algoritmoNombres y equalsTitle, no puedes estar los dos de forma simultánea");
                            }
                            foreach (KeyValuePair<DisambiguableEntity, List<DisambiguationData>> item in itemsData[tipo])
                            {
                                if (!ids.Add(item.Key.ID))
                                {
                                    throw new Exception("El id " + item.Key.ID + "está repetido");
                                }
                                if (string.IsNullOrEmpty(item.Key.ID))
                                {
                                    throw new Exception("Todas las entiades deben tener ID");
                                }
                                if (data.Count != item.Value.Count)
                                {
                                    throw new Exception("Todas las entiades del mismo tipo deben tener las mismas propiedades");
                                }
                                for (int i = 0; i < data.Count; i++)
                                {
                                    if (data[i].property != item.Value[i].property)
                                    {
                                        throw new Exception("En los items " + tipo + " hay propiedades diferentes");
                                    }
                                    if (data[i].config.score != item.Value[i].config.score)
                                    {
                                        throw new Exception("Todas las entiades del mismo tipo deben tener el mismo score");
                                    }
                                    if (data[i].config.scoreMinus != item.Value[i].config.scoreMinus)
                                    {
                                        throw new Exception("Todas las entiades del mismo tipo deben tener el mismo scoreMinus");
                                    }
                                    if (data[i].config.type != item.Value[i].config.type)
                                    {
                                        throw new Exception("Todas las entiades del mismo tipo deben tener el mismo type");
                                    }
                                    switch (item.Value[i].config.type)
                                    {
                                        case DisambiguationDataConfigType.algoritmoNombres:
                                            if (item.Value[i].value == null)
                                            {
                                                item.Value[i].value = "";
                                            }
                                            if (item.Value[i].config.score <= 0 && item.Value[i].config.score > 1)
                                            {
                                                throw new Exception("La propiedad score en 'algoritmoNombres' debe ser > 0 y <=1");
                                            }
                                            if (item.Value[i].config.scoreMinus != 0)
                                            {
                                                throw new Exception("La propiedad scoreMinus en 'algoritmoNombres' no hay que configurarla");
                                            }
                                            break;
                                        case DisambiguationDataConfigType.equalsIdentifiers:
                                            if (item.Value[i].value == null)
                                            {
                                                item.Value[i].value = "";
                                            }
                                            if (item.Value[i].config.score != 0)
                                            {
                                                throw new Exception("La propiedad score en 'equalsIdentifiers' no hay que configurarla");
                                            }
                                            if (item.Value[i].config.scoreMinus != 0)
                                            {
                                                throw new Exception("La propiedad scoreMinus en 'equalsIdentifiers' no hay que configurarla");
                                            }
                                            break;
                                        case DisambiguationDataConfigType.equalsItem:
                                            if (item.Value[i].value == null)
                                            {
                                                item.Value[i].value = "";
                                            }
                                            if (item.Value[i].config.score < 0 && item.Value[i].config.score > 1)
                                            {
                                                throw new Exception("La propiedad score en 'equalsItem' debe ser >= 0 y <=1");
                                            }
                                            if (item.Value[i].config.scoreMinus < 0 && item.Value[i].config.scoreMinus > 1)
                                            {
                                                throw new Exception("La propiedad scoreMinus en 'equalsItem' debe ser >= 0 y <=1");
                                            }
                                            break;
                                        case DisambiguationDataConfigType.equalsTitle:
                                            if (item.Value[i].value == null)
                                            {
                                                item.Value[i].value = "";
                                            }
                                            if (item.Value[i].config.score <= 0 && item.Value[i].config.score > 1)
                                            {
                                                throw new Exception("La propiedad score en 'equalsTitle' debe ser > 0 y <=1");
                                            }
                                            if (item.Value[i].config.scoreMinus != 0)
                                            {
                                                throw new Exception("La propiedad scoreMinus en 'equalsTitle' no hay que configurarla");
                                            }
                                            break;
                                        case DisambiguationDataConfigType.equalsItemList:
                                            if (item.Value[i].values == null)
                                            {
                                                item.Value[i].values = new HashSet<string>();
                                            }
                                            if (item.Value[i].config.score < 0 && item.Value[i].config.score > 1)
                                            {
                                                throw new Exception("La propiedad score en 'equalsItemList' debe ser >= 0 y <=1");
                                            }
                                            if (item.Value[i].config.scoreMinus != 0)
                                            {
                                                throw new Exception("La propiedad scoreMinus en 'equalsIdentifiers' no hay que configurarla");
                                            }
                                            break;
                                        default:
                                            throw new Exception("No implementado");
                                    }
                                }
                            }
                            if (!block && pItemsDataBBDD != null && pItemsDataBBDD.Count > 1)
                            {
                                foreach (KeyValuePair<DisambiguableEntity, List<DisambiguationData>> item in pItemsDataBBDD[tipo])
                                {
                                    if (data.Count != item.Value.Count)
                                    {
                                        throw new Exception("Todas las entiades del mismo tipo deben tener las mismas propiedades");
                                    }
                                    for (int i = 0; i < data.Count; i++)
                                    {
                                        if (data[i].property != item.Value[i].property)
                                        {
                                            throw new Exception("En los items " + tipo + " hay propiedades diferentes");
                                        }
                                        if (data[i].config.score != item.Value[i].config.score)
                                        {
                                            throw new Exception("Todas las entiades del mismo tipo deben tener el mismo score");
                                        }
                                        if (data[i].config.scoreMinus != item.Value[i].config.scoreMinus)
                                        {
                                            throw new Exception("Todas las entiades del mismo tipo deben tener el mismo scoreMinus");
                                        }
                                        if (data[i].config.type != item.Value[i].config.type)
                                        {
                                            throw new Exception("Todas las entiades del mismo tipo deben tener el mismo type");
                                        }
                                        switch (item.Value[i].config.type)
                                        {
                                            case DisambiguationDataConfigType.algoritmoNombres:
                                                if (item.Value[i].value == null)
                                                {
                                                    item.Value[i].value = "";
                                                }
                                                if (item.Value[i].config.score <= 0 && item.Value[i].config.score > 1)
                                                {
                                                    throw new Exception("La propiedad score en 'algoritmoNombres' debe ser > 0 y <=1");
                                                }
                                                if (item.Value[i].config.scoreMinus != 0)
                                                {
                                                    throw new Exception("La propiedad scoreMinus en 'algoritmoNombres' no hay que configurarla");
                                                }
                                                break;
                                            case DisambiguationDataConfigType.equalsIdentifiers:
                                                if (item.Value[i].value == null)
                                                {
                                                    item.Value[i].value = "";
                                                }
                                                if (item.Value[i].config.score != 0)
                                                {
                                                    throw new Exception("La propiedad score en 'equalsIdentifiers' no hay que configurarla");
                                                }
                                                if (item.Value[i].config.scoreMinus != 0)
                                                {
                                                    throw new Exception("La propiedad scoreMinus en 'equalsIdentifiers' no hay que configurarla");
                                                }
                                                break;
                                            case DisambiguationDataConfigType.equalsItem:
                                                if (item.Value[i].value == null)
                                                {
                                                    item.Value[i].value = "";
                                                }
                                                if (item.Value[i].config.score < 0 && item.Value[i].config.score > 1)
                                                {
                                                    throw new Exception("La propiedad score en 'equalsItem' debe ser >= 0 y <=1");
                                                }
                                                if (item.Value[i].config.scoreMinus < 0 && item.Value[i].config.scoreMinus > 1)
                                                {
                                                    throw new Exception("La propiedad scoreMinus en 'equalsItem' debe ser >= 0 y <=1");
                                                }
                                                break;
                                            case DisambiguationDataConfigType.equalsTitle:
                                                if (item.Value[i].value == null)
                                                {
                                                    item.Value[i].value = "";
                                                }
                                                if (item.Value[i].config.score <= 0 && item.Value[i].config.score > 1)
                                                {
                                                    throw new Exception("La propiedad score en 'equalsTitle' debe ser > 0 y <=1");
                                                }
                                                if (item.Value[i].config.scoreMinus != 0)
                                                {
                                                    throw new Exception("La propiedad scoreMinus en 'equalsTitle' no hay que configurarla");
                                                }
                                                break;
                                            case DisambiguationDataConfigType.equalsItemList:
                                                if (item.Value[i].values == null)
                                                {
                                                    item.Value[i].values = new HashSet<string>();
                                                }
                                                if (item.Value[i].config.score < 0 && item.Value[i].config.score > 1)
                                                {
                                                    throw new Exception("La propiedad score en 'equalsItemList' debe ser >= 0 y <=1");
                                                }
                                                if (item.Value[i].config.scoreMinus != 0)
                                                {
                                                    throw new Exception("La propiedad scoreMinus en 'equalsIdentifiers' no hay que configurarla");
                                                }
                                                break;
                                            default:
                                                throw new Exception("No implementado");
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                block = true;
            }
        }



        /// <summary>
        /// Obtiene la similitud entre dos datos del mismo tipo. 
        /// </summary>
        /// <param name="pDataA">Dato A</param>
        /// <param name="pDataB">Dato B</param>
        /// <returns>Número de similitud. A más cerca de 1, más similar es.</returns>
        /// <exception cref="Exception">Configuración no implementada.</exception>
        private static float GetSimilarity(List<DisambiguationData> pDataA, List<DisambiguationData> pDataB, Dictionary<string, string> pDicNomAutoresDesnormalizados, Dictionary<string, string> pDicTitulosDesnormalizados, Dictionary<string, Dictionary<string, float>> pEquivalencesAux)
        {
            float result = 0;
            #region Con identificadores con valor
            if (pDataA.Count > 0 && pDataA.Exists(x => x.config.type == DisambiguationDataConfigType.equalsIdentifiers))
            {
                bool existenIdentificadores = false;
                for (int i = 0; i < pDataA.Count; i++)
                {
                    DisambiguationData dataAAux = pDataA[i];
                    DisambiguationData dataBAux = pDataB[i];
                    if (dataAAux.config.type == DisambiguationDataConfigType.equalsIdentifiers)
                    {
                        if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value))
                        {
                            existenIdentificadores = true;
                            if (dataAAux.value.Equals(dataBAux.value))
                            {
                                result = 1;
                            }
                        }
                    }
                }

                if (existenIdentificadores && result > 0)
                {
                    for (int i = 0; i < pDataA.Count; i++)
                    {
                        DisambiguationData dataAAux = pDataA[i];
                        DisambiguationData dataBAux = pDataB[i];
                        if (dataAAux.config.type == DisambiguationDataConfigType.equalsIdentifiers)
                        {
                            if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value) && !dataAAux.value.Equals(dataBAux.value))
                            {
                                result = 0;
                            }
                        }
                    }
                }
                if (existenIdentificadores)
                {
                    return result;
                }
            }
            #endregion

            #region Con título configurado
            if (pDataA.Count > 0 && pDataA.Exists(x => x.config.type == DisambiguationDataConfigType.equalsTitle))
            {
                //Títulos
                for (int i = 0; i < pDataA.Count; i++)
                {
                    DisambiguationData dataAAux = pDataA[i];
                    DisambiguationData dataBAux = pDataB[i];
                    if (dataAAux.config.type == DisambiguationDataConfigType.equalsTitle)
                    {
                        if (GetTitleSimilarity(dataAAux.value, dataBAux.value, pDicTitulosDesnormalizados))
                        {
                            result += (1 - result) * dataAAux.config.score;
                        }
                    }
                }
                //Resto
                if (result > 0)
                {
                    //ItemA ItemB Prop
                    Dictionary<string, Dictionary<string, HashSet<string>>> dicEqualsItem = new Dictionary<string, Dictionary<string, HashSet<string>>>();
                    for (int i = 0; i < pDataA.Count; i++)
                    {
                        DisambiguationData dataAAux = pDataA[i];
                        DisambiguationData dataBAux = pDataB[i];

                        if (dataAAux.config.score > 0)
                        {
                            switch (dataAAux.config.type)
                            {
                                case DisambiguationDataConfigType.equalsIdentifiers:
                                case DisambiguationDataConfigType.equalsTitle:
                                    //No se comprueba
                                    break;
                                case DisambiguationDataConfigType.algoritmoNombres:
                                    throw new Exception("Si tiene título no debería tener nombres");
                                    break;
                                case DisambiguationDataConfigType.equalsItem:
                                    if (PesoEqualsItem(ref result, dataAAux.config.score, dataAAux.value, dataBAux.value, pEquivalencesAux))
                                    {
                                        if (!dicEqualsItem.ContainsKey(dataAAux.value))
                                        {
                                            dicEqualsItem.Add(dataAAux.value, new Dictionary<string, HashSet<string>>());
                                        }
                                        if (!dicEqualsItem[dataAAux.value].ContainsKey(dataBAux.value))
                                        {
                                            dicEqualsItem[dataAAux.value].Add(dataBAux.value, new HashSet<string>());
                                        }
                                        dicEqualsItem[dataAAux.value][dataBAux.value].Add(dataAAux.property);
                                    }
                                    break;
                                case DisambiguationDataConfigType.equalsItemList:
                                    result = PesoEqualsItemList(result, dataAAux.config.score, dataAAux.values, dataBAux.values, pEquivalencesAux);
                                    break;
                                default:
                                    throw new Exception("No está implementado.");
                            }
                        }
                    }

                    if (result > 0)
                    {
                        for (int i = 0; i < pDataA.Count; i++)
                        {
                            DisambiguationData dataAAux = pDataA[i];
                            DisambiguationData dataBAux = pDataB[i];

                            if (dataAAux.config.scoreMinus > 0)
                            {
                                switch (dataAAux.config.type)
                                {
                                    case DisambiguationDataConfigType.equalsIdentifiers:
                                    case DisambiguationDataConfigType.equalsTitle:
                                        //No se comprueba
                                        break;
                                    case DisambiguationDataConfigType.algoritmoNombres:
                                        throw new Exception("Si tiene título no debería tener nombres");
                                        break;
                                    case DisambiguationDataConfigType.equalsItem:
                                        if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value))
                                        {
                                            if (!(dicEqualsItem.ContainsKey(dataAAux.value) &&
                                                dicEqualsItem[dataAAux.value].ContainsKey(dataBAux.value) &&
                                               dicEqualsItem[dataAAux.value][dataBAux.value].Contains(dataAAux.property)))
                                            {
                                                result -= (1 - result) * dataAAux.config.scoreMinus;
                                            }
                                        }
                                        break;
                                    case DisambiguationDataConfigType.equalsItemList:
                                        throw new Exception("No hay negativo para equalsItemList");
                                        break;
                                    default:
                                        throw new Exception("No está implementado.");
                                }
                            }
                        }
                    }
                }

                return result;
            }
            #endregion

            #region Con nombre configurado
            if (pDataA.Count > 0 && pDataA.Exists(x => x.config.type == DisambiguationDataConfigType.algoritmoNombres))
            {
                //Nombres
                for (int i = 0; i < pDataA.Count; i++)
                {
                    DisambiguationData dataAAux = pDataA[i];
                    DisambiguationData dataBAux = pDataB[i];
                    if (dataAAux.config.type == DisambiguationDataConfigType.algoritmoNombres)
                    {
                        float nameSimilarity = GetNameSimilarity(dataAAux.value, dataBAux.value, pDicNomAutoresDesnormalizados);
                        result += (1 - result) * nameSimilarity * dataAAux.config.score;
                    }
                }
                //Resto
                if (result > 0)
                {
                    Dictionary<string, Dictionary<string, HashSet<string>>> dicEqualsItem = new Dictionary<string, Dictionary<string, HashSet<string>>>();
                    for (int i = 0; i < pDataA.Count; i++)
                    {
                        DisambiguationData dataAAux = pDataA[i];
                        DisambiguationData dataBAux = pDataB[i];

                        if (dataAAux.config.score > 0)
                        {
                            switch (dataAAux.config.type)
                            {
                                case DisambiguationDataConfigType.equalsIdentifiers:
                                case DisambiguationDataConfigType.algoritmoNombres:
                                    //No se comprueba
                                    break;
                                case DisambiguationDataConfigType.equalsTitle:
                                    throw new Exception("Si tiene nombre no debería tener titulo");
                                    break;
                                case DisambiguationDataConfigType.equalsItem:
                                    if (PesoEqualsItem(ref result, dataAAux.config.score, dataAAux.value, dataBAux.value, pEquivalencesAux))
                                    {
                                        if (!dicEqualsItem.ContainsKey(dataAAux.value))
                                        {
                                            dicEqualsItem.Add(dataAAux.value, new Dictionary<string, HashSet<string>>());
                                        }
                                        if (!dicEqualsItem[dataAAux.value].ContainsKey(dataBAux.value))
                                        {
                                            dicEqualsItem[dataAAux.value].Add(dataBAux.value, new HashSet<string>());
                                        }
                                        dicEqualsItem[dataAAux.value][dataBAux.value].Add(dataAAux.property);
                                    }
                                    break;
                                case DisambiguationDataConfigType.equalsItemList:
                                    result = PesoEqualsItemList(result, dataAAux.config.score, dataAAux.values, dataBAux.values, pEquivalencesAux);
                                    break;
                                default:
                                    throw new Exception("No está implementado.");
                            }
                        }
                    }

                    if (result > 0)
                    {
                        for (int i = 0; i < pDataA.Count; i++)
                        {
                            DisambiguationData dataAAux = pDataA[i];
                            DisambiguationData dataBAux = pDataB[i];

                            if (dataAAux.config.scoreMinus > 0)
                            {
                                switch (dataAAux.config.type)
                                {
                                    case DisambiguationDataConfigType.equalsIdentifiers:
                                    case DisambiguationDataConfigType.algoritmoNombres:
                                        //No se comprueba
                                        break;
                                    case DisambiguationDataConfigType.equalsTitle:
                                        throw new Exception("Si tiene nombre no debería tener titulo");
                                        break;
                                    case DisambiguationDataConfigType.equalsItem:
                                        if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value))
                                        {
                                            if (!(dicEqualsItem.ContainsKey(dataAAux.value) &&
                                                dicEqualsItem[dataAAux.value].ContainsKey(dataBAux.value) &&
                                               dicEqualsItem[dataAAux.value][dataBAux.value].Contains(dataAAux.property)))
                                            {
                                                result -= (1 - result) * dataAAux.config.scoreMinus;
                                            }
                                        }
                                        break;
                                    case DisambiguationDataConfigType.equalsItemList:
                                        throw new Exception("No hay negativo para equalsItemList");
                                        break;
                                    default:
                                        throw new Exception("No está implementado.");
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = 0;
                }

                return result;
            }
            #endregion

            #region resto
            Dictionary<string, Dictionary<string, HashSet<string>>> dicEqualsItemResto = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            for (int i = 0; i < pDataA.Count; i++)
            {
                DisambiguationData dataAAux = pDataA[i];
                DisambiguationData dataBAux = pDataB[i];

                if (dataAAux.config.score > 0)
                {
                    switch (dataAAux.config.type)
                    {
                        case DisambiguationDataConfigType.equalsTitle:
                        case DisambiguationDataConfigType.equalsIdentifiers:
                            //No se comprueba
                            break;
                        case DisambiguationDataConfigType.algoritmoNombres:
                            float similaridad = GetNameSimilarity(dataAAux.value, dataBAux.value, pDicNomAutoresDesnormalizados);
                            result += (1 - result) * similaridad * dataAAux.config.score;
                            break;
                        case DisambiguationDataConfigType.equalsItem:
                            if (PesoEqualsItem(ref result, dataAAux.config.score, dataAAux.value, dataBAux.value, pEquivalencesAux))
                            {
                                if (!dicEqualsItemResto.ContainsKey(dataAAux.value))
                                {
                                    dicEqualsItemResto.Add(dataAAux.value, new Dictionary<string, HashSet<string>>());
                                }
                                if (!dicEqualsItemResto[dataAAux.value].ContainsKey(dataBAux.value))
                                {
                                    dicEqualsItemResto[dataAAux.value].Add(dataBAux.value, new HashSet<string>());
                                }
                                dicEqualsItemResto[dataAAux.value][dataBAux.value].Add(dataAAux.property);
                            }
                            break;
                        default:
                            throw new Exception("No está implementado.");
                    }
                }
            }

            if (result > 0)
            {
                for (int i = 0; i < pDataA.Count; i++)
                {
                    DisambiguationData dataAAux = pDataA[i];
                    DisambiguationData dataBAux = pDataB[i];

                    if (dataAAux.config.scoreMinus > 0)
                    {
                        switch (dataAAux.config.type)
                        {
                            case DisambiguationDataConfigType.equalsIdentifiers:
                            case DisambiguationDataConfigType.equalsTitle:
                            case DisambiguationDataConfigType.algoritmoNombres:
                                //No se comprueba
                                break;
                            case DisambiguationDataConfigType.equalsItem:
                                if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value))
                                {
                                    if (!(dicEqualsItemResto.ContainsKey(dataAAux.value) &&
                                        dicEqualsItemResto[dataAAux.value].ContainsKey(dataBAux.value) &&
                                       dicEqualsItemResto[dataAAux.value][dataBAux.value].Contains(dataAAux.property)))
                                    {
                                        result -= (1 - result) * dataAAux.config.scoreMinus;
                                    }
                                }
                                break;
                            default:
                                throw new Exception("No está implementado.");
                        }
                    }
                }
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Procesar el peso para los EqualsItemList
        /// </summary>
        /// <param name="pPesoInicial">Peso inicial</param>
        /// <param name="pScore">Score de la propiedad</param>
        /// <param name="pValoresA">Valores A</param>
        /// <param name="pValoresB">Valores B</param>
        /// <param name="pEquivalencias">Equivalencias</param>
        /// <returns></returns>
        private static float PesoEqualsItemList(float pPesoInicial, float pScore, HashSet<string> pValoresA, HashSet<string> pValoresB, Dictionary<string, Dictionary<string, float>> pEquivalencias)
        {
            //TODO optimizar
            HashSet<string> equivalenciasAaux = new HashSet<string>();
            HashSet<string> equivalenciasBaux = new HashSet<string>();
            foreach (string id in pValoresA)
            {
                if (pEquivalencias != null && pEquivalencias.ContainsKey(id))
                {
                    equivalenciasAaux = new HashSet<string>() { id };
                    equivalenciasAaux.UnionWith(pEquivalencias[id].Keys);
                }
                else
                {
                    equivalenciasAaux = new HashSet<string>();
                }
                equivalenciasAaux.Add(id);
            }
            foreach (string id in pValoresB)
            {
                if (pEquivalencias != null && pEquivalencias.ContainsKey(id))
                {
                    equivalenciasBaux = new HashSet<string>() { id };
                    equivalenciasBaux.UnionWith(pEquivalencias[id].Keys);
                }
                else
                {
                    equivalenciasBaux = new HashSet<string>();
                }
                equivalenciasBaux.Add(id);
            }
            HashSet<string> bAsigandos = new HashSet<string>();
            Dictionary<string, Dictionary<string, float>> dicFinal = new Dictionary<string, Dictionary<string, float>>();
            foreach (string igual in equivalenciasAaux.Intersect(equivalenciasBaux))
            {
                foreach (string id in pValoresA)
                {
                    foreach (string id2 in pValoresB)
                    {
                        if (!bAsigandos.Contains(id2))
                        {
                            float score = 0;
                            if (id == igual && id2 == igual)
                            {
                                score = 1;
                            }
                            else if (pEquivalencias != null)
                            {
                                if (pEquivalencias.ContainsKey(id) && pEquivalencias[id].ContainsKey(id2))
                                {
                                    score = pEquivalencias[id][id2];
                                }
                                else if (pEquivalencias.ContainsKey(id2) && pEquivalencias[id2].ContainsKey(id))
                                {
                                    score = pEquivalencias[id2][id];
                                }
                            }
                            if (score > 0)
                            {
                                if (!dicFinal.ContainsKey(id))
                                {
                                    dicFinal[id] = new Dictionary<string, float>();
                                }
                                if (!dicFinal[id].ContainsKey(id2))
                                {
                                    dicFinal[id][id2] = 0;
                                    bAsigandos.Add(id2);
                                }
                                if (dicFinal[id][id2] < score)
                                {
                                    dicFinal[id][id2] = score;
                                }
                            }
                        }

                    }
                }
            }
            foreach (string id1 in dicFinal.Keys)
            {
                foreach (string id2 in dicFinal[id1].Keys)
                {
                    pPesoInicial += (1 - pPesoInicial) * dicFinal[id1][id2] * pScore;
                }
            }
            return pPesoInicial;
        }

        /// <summary>
        /// Procesar el peso para los EqualsItem
        /// </summary>
        /// <param name="pPesoInicial">Peso inicial</param>
        /// <param name="pScore">Score de la propiedad</param>
        /// <param name="pValorA">Valor A</param>
        /// <param name="pValorB">Valor B</param>
        /// <param name="pEquivalencias">Equivalencias</param>
        /// <returns></returns>
        private static bool PesoEqualsItem(ref float pPesoInicial, float pScore, string pValorA, string pValorB, Dictionary<string, Dictionary<string, float>> pEquivalencias)
        {
            bool cambio = false;
            if (!string.IsNullOrEmpty(pValorA) && !string.IsNullOrEmpty(pValorB))
            {
                float peso = 0;
                if (pValorA.Equals(pValorB))
                {
                    peso = 1;
                }
                else if (pEquivalencias!=null && pEquivalencias.ContainsKey(pValorA))
                {
                    foreach (string equivalence in pEquivalencias[pValorA].Keys)
                    {
                        if (equivalence == pValorB)
                        {
                            if (pEquivalencias[pValorA][equivalence] > peso)
                            {
                                peso = pEquivalencias[pValorA][equivalence];
                            }
                        }
                        if (pEquivalencias.ContainsKey(pValorB))
                        {
                            foreach (string equivalence2 in pEquivalencias[pValorB].Keys)
                            {
                                if (equivalence == equivalence2)
                                {
                                    if (pEquivalencias[pValorB][equivalence2] > peso)
                                    {
                                        peso = pEquivalencias[pValorB][equivalence2];
                                    }
                                }
                            }
                        }
                    }
                }
                else if (pEquivalencias != null && pEquivalencias.ContainsKey(pValorB) && !pEquivalencias.ContainsKey(pValorA))
                {
                    foreach (string equivalence2 in pEquivalencias[pValorB].Keys)
                    {
                        if (equivalence2 == pValorA)
                        {
                            if (pEquivalencias[pValorB][equivalence2] > peso)
                            {
                                peso = pEquivalencias[pValorB][equivalence2];
                            }
                        }
                    }
                }
                if (peso > 0)
                {
                    cambio = true;
                    pPesoInicial += (1 - pPesoInicial) * peso * pScore;
                }
            }
            return cambio;
        }


        /// <summary>
        /// Método para normalizar los títulos
        /// </summary>
        /// <param name="pTitle">Título</param>
        /// <returns></returns>
        private static string NormalizeTitle(string pTitle)
        {
            string normalizedString = pTitle.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char charin in normalizedString)
            {
                if (char.IsLetterOrDigit(charin))
                {
                    sb.Append(charin);
                }
            }
            normalizedString = sb.ToString().Normalize(NormalizationForm.FormD).ToLower();
            return normalizedString;
        }

        /// <summary>
        /// Agrega los IDs con su ID similar y su similaridad.
        /// </summary>
        /// <param name="pEquivalences"></param>
        /// <param name="pSimilaridad"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        private static void AddSimilarityId(string pTipo, Dictionary<string, Dictionary<string, float>> pEquivalences, float pSimilaridad, string pIdA, string pIdB)
        {
            if (pSimilaridad > 0) // TODO: Umbral.
            {
                if (!pEquivalences.ContainsKey($"{pTipo}|{pIdA}"))
                {
                    pEquivalences.Add(pTipo + "|" + pIdA, new Dictionary<string, float>());
                }
                if (!pEquivalences.ContainsKey($"{pTipo}|{pIdB}"))
                {
                    pEquivalences.Add(pTipo + "|" + pIdB, new Dictionary<string, float>());
                }
                pEquivalences[$"{pTipo}|{pIdA}"][$"{pTipo}|{pIdB}"] = pSimilaridad;
                pEquivalences[$"{pTipo}|{pIdB}"][$"{pTipo}|{pIdA}"] = pSimilaridad;
            }
        }

        public static float GetNameSimilarity(string pSource, string pTarget, Dictionary<string, string> pDicNomAutoresDesnormalizados)
        {
            pSource = ObtenerTextosNombresNormalizados(pSource, pDicNomAutoresDesnormalizados);
            pTarget = ObtenerTextosNombresNormalizados(pTarget, pDicNomAutoresDesnormalizados);

            //Almacenamos los scores de cada una de las palabras
            List<float> scores = new List<float>();

            string[] pFirmaNormalizadoSplit = pSource.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] pTargetNormalizadoSplit = pTarget.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            string[] source = pFirmaNormalizadoSplit;
            string[] target = pTargetNormalizadoSplit;

            int indexTarget = 0;
            bool coincidenciaNombreNoinicial = false;
            for (int i = 0; i < source.Length; i++)
            {
                //Similitud real
                float score = 0;
                string wordSource = source[i];
                bool wordSourceInicial = wordSource.Length == 1;
                //int desplazamiento = 0;
                for (int j = indexTarget; j < target.Length; j++)
                {
                    string wordTarget = target[j];
                    bool wordTargetInicial = wordTarget.Length == 1;
                    //Alguna de las dos es inicial
                    if (wordSourceInicial || wordTargetInicial)
                    {
                        float scoreWord = minimoScoreNombres;
                        if (ScoreNombresCalculado.ContainsKey(wordSource[0].ToString()))
                        {
                            scoreWord = ScoreNombresCalculado[wordSource[0].ToString()];
                        }
                        if (wordSource[0] == wordTarget[0])
                        {
                            if (wordSourceInicial != wordTargetInicial)
                            {
                                //Una de las dos es inicial                                
                                score = scoreWord;
                                indexTarget = j + 1;
                                break;
                            }
                            else
                            {
                                //Son las dos iniciales                            
                                score = scoreWord;
                                indexTarget = j + 1;
                                break;
                            }
                        }
                    }
                    //Ninguna de las dos es inicial
                    if (wordSource.Equals(wordTarget))
                    {
                        coincidenciaNombreNoinicial = true;
                        float scoreWord = minimoScoreNombres;
                        if (ScoreNombresCalculado.ContainsKey(wordTarget))
                        {
                            scoreWord = ScoreNombresCalculado[wordTarget];
                        }
                        score = scoreWord;
                        indexTarget = j + 1;
                        break;
                    }
                }
                if (score > 0)
                {
                    scores.Add(score);
                }
            }

            //Coincide algo que no sea inicial  
            if (coincidenciaNombreNoinicial && scores.Count > 0)
            {
                if (pSource.Contains("skarmeta") && pTarget.Contains("skarmeta"))
                {

                }
                int longMin = Math.Min(source.Length, target.Length);
                //Cuanto mas bajo mejor
                //Calculamos las coincidencias
                float temp = 1;
                foreach (float score in scores)
                {
                    temp = temp * score;
                }
                float scoreFinal = 1 - temp;
                //Si hay alguna cosa que no coincida restamos
                if (scores.Count < longMin)
                {
                    //Coincidencias
                    int coincidencias = scores.Count;
                    //No coincidencias
                    int noCoincidencias = longMin - scores.Count;
                    //Si hay sólo una no coincidencia aplicamos corrector
                    if (noCoincidencias < 2)
                    {
                        scoreFinal = scoreFinal / 2;
                    }
                    else
                    {
                        //Si hay mas de una no coincidencia devolcemos cero
                        return 0;
                    }
                }
                //Si solo hay una coincidencia restamos
                if (scores.Count == 1)
                {
                    scoreFinal = scoreFinal / 2;
                }
                //En función del nº de coincidencias aplicamos algo (si solo es un apalabra pasamos)
                return scoreFinal;
            }
            return 0;
        }

        public static bool GetTitleSimilarity(string pTituloA, string pTituloB, Dictionary<string, string> pDicTitulosDesnormalizados)
        {
            string tituloAAux = ObtenerTextosTitulosNormalizados(pTituloA, pDicTitulosDesnormalizados);
            string tituloBAux = ObtenerTextosTitulosNormalizados(pTituloB, pDicTitulosDesnormalizados);
            if (!string.IsNullOrEmpty(tituloAAux) && !string.IsNullOrEmpty(tituloBAux) && tituloAAux.Equals(tituloBAux))
            {
                return true;
            }
            return false;
        }

        private static string ObtenerTextosNombresNormalizados(string pText, Dictionary<string, string> pDicNomAutoresDesnormalizados = null)
        {
            // Comprobación si tenemos guardado el nombre.
            string textoAux = pText;
            if (pDicNomAutoresDesnormalizados != null && pDicNomAutoresDesnormalizados.ContainsKey(textoAux))
            {
                return pDicNomAutoresDesnormalizados[textoAux];
            }

            pText = pText.ToLower();
            pText = pText.Trim();
            if (pText.Contains(","))
            {
                pText = (pText.Substring(pText.IndexOf(",") + 1)).Trim() + " " + (pText.Substring(0, pText.IndexOf(","))).Trim();
            }
            pText = pText.Replace("-", " ");
            string textoNormalizado = pText.Normalize(NormalizationForm.FormD);
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^a-zA-Z ]");
            string textoSinAcentos = reg.Replace(textoNormalizado, "");
            while (textoSinAcentos.Contains(" del "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" del ", " ");
            }
            while (textoSinAcentos.Contains(" de "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" de ", " ");
            }
            while (textoSinAcentos.Contains(" la "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" la ", " ");
            }
            while (textoSinAcentos.Contains(" von "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" von ", " ");
            }
            while (textoSinAcentos.Contains(" al "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" al ", " ");
            }
            while (textoSinAcentos.Contains("  "))
            {
                textoSinAcentos = textoSinAcentos.Replace("  ", " ");
            }

            if (pDicNomAutoresDesnormalizados != null)
            {
                pDicNomAutoresDesnormalizados.Add(textoAux, textoSinAcentos.Trim());
            }

            return textoSinAcentos.Trim();
        }

        private static string ObtenerTextosTitulosNormalizados(string pText, Dictionary<string, string> pDicTitulosDesnormalizados)
        {
            // Comprobación si tenemos guardado el nombre.
            string textoAux = pText;
            if (pDicTitulosDesnormalizados.ContainsKey(textoAux))
            {
                return pDicTitulosDesnormalizados[textoAux];
            }
            textoAux = NormalizeTitle(pText);
            pDicTitulosDesnormalizados[pText] = textoAux;
            return textoAux.Trim();
        }

        private static HashSet<string> GetNGramas(string pText, int pNgramSize)
        {
            HashSet<string> ngramas = new HashSet<string>();
            int textLength = pText.Length;
            if (pNgramSize == 1)
            {
                for (int i = 0; i < textLength; i++)
                {
                    ngramas.Add(pText[i].ToString());
                }
                return ngramas;
            }

            HashSet<string> ngramasaux = new HashSet<string>();
            for (int i = 0; i < textLength; i++)
            {
                foreach (string ngram in ngramasaux.ToList())
                {
                    string ngamaux = ngram + pText[i];
                    if (ngamaux.Length == pNgramSize)
                    {
                        ngramas.Add(ngamaux);
                    }
                    else
                    {
                        ngramasaux.Add(ngamaux);
                    }
                    ngramasaux.Remove(ngram);
                }
                ngramasaux.Add(pText[i].ToString());
                if (i < pNgramSize)
                {
                    foreach (string ngrama in ngramasaux)
                    {
                        if (ngrama.Length == i + 1)
                        {
                            ngramas.Add(ngrama);
                        }
                    }
                }
            }
            for (int i = (textLength - pNgramSize) + 1; i < textLength; i++)
            {
                if (i >= pNgramSize)
                {
                    ngramas.Add(pText.Substring(i));
                }
            }
            return ngramas;
        }
    }
}
