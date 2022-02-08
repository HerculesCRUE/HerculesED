using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.DisambiguationEngine.Models
{
    public static class Disambiguation
    {
        /// <summary>
        /// Proceso de desambiguación.
        /// </summary>
        /// <param name="pItems">Lista de datos a desambiguar.</param>
        /// <returns>Lista de datos desambiguables.</returns>
        public static Dictionary<string, HashSet<string>> Disambiguate(List<DisambiguableEntity> pItems, List<DisambiguableEntity> pItemBBDD,bool pDisambiguateItems=true, float pUmbral = 0.8f)
        {
            //Respuesta
            Dictionary<string, Dictionary<string, float>> listaEquivalencias = new Dictionary<string, Dictionary<string, float>>();

            // Obtenemos las propiedades que nos permitirán la desambiguación.
            Dictionary<DisambiguableEntity, List<DisambiguationData>> disambiguationDataItemsACargar = GetDisambiguationData(pItems);

            //Creamos una variable que se irá modificando cuando se vaya realizando la desambiguación
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
                        Dictionary<string, Dictionary<string, float>> listaEquivalenciasItemsACargar = ApplyDisambiguation(disambiguationDataItemsACargarAux);
                        cambiosItems = ProcesarEquivalencias(pItems, listaEquivalenciasItemsACargar, disambiguationDataItemsACargarAux, listaEquivalencias, pUmbral);
                        if(cambiosItems)
                        {
                            cambios = true;
                        }
                    }

                    //2º Aplicamos la desambiguación con los items de BBDD
                }
            }

            //Preparamos el objeto a devolver
            Dictionary<string, HashSet<string>> listadoEquivalencias = new Dictionary<string, HashSet<string>>();
            foreach (string id in listaEquivalencias.Keys)
            {
                string idAux = Guid.NewGuid().ToString();
                KeyValuePair<string, HashSet<string>> item = listadoEquivalencias.FirstOrDefault(x => x.Value.Contains(id));
                if (item.Value == null)
                {
                    listadoEquivalencias[idAux] = new HashSet<string>(listaEquivalencias[id].Keys.ToList());
                    listadoEquivalencias[idAux].Add(id);
                }
                else
                {
                    item.Value.UnionWith(listaEquivalencias[id].Keys.ToList());
                    item.Value.Add(id);
                }
            }

            HashSet<string> itemsDevolver = new HashSet<string>(listadoEquivalencias.Values.SelectMany(x => x));
            foreach (DisambiguableEntity item in pItems)
            {
                if (!itemsDevolver.Contains(item.GetType().Name + "_" + item.ID))
                {
                    listadoEquivalencias.Add(Guid.NewGuid().ToString(), new HashSet<string>() { item.GetType().Name + "_" + item.ID });
                }
            }

            ////TODO eliminar test
            //HashSet<string> skarmeta = new HashSet<string>();
            //foreach (string id in listadoEquivalencias.Keys)
            //{
            //    HashSet<string> idsIguales = listadoEquivalencias[id];
            //    foreach(string idigual in idsIguales)
            //    {
            //        DisambiguableEntity entity = pItems.First(x => x.ID == idigual.Split('_')[1]);
            //        if(entity is DisambiguationPerson)
            //        {
            //            if(((DisambiguationPerson)entity).completeName.ToLower().Contains("skarmeta"))
            //            {
            //                if(skarmeta.Add(id))
            //                {

            //                }
            //            }
            //        }
            //    }
            //}


            return listadoEquivalencias;
        }

        /// <summary>
        /// Procesa las equivalencias
        /// </summary>
        /// <param name="pItems">Items originales</param>
        /// <param name="pListaEquivalenciasItemsACargar">Resultado de la desmbiguación</param>
        /// <param name="pDisambiguationDataItemsACargar">Items con los datos de desambiguación</param>
        /// <param name="pListaEquivalencias">Lista de equivalencias</param>
        /// <param name="pUmbral">Umbral</param>
        private static bool ProcesarEquivalencias(List<DisambiguableEntity> pItems, Dictionary<string, Dictionary<string, float>> pListaEquivalenciasItemsACargar,
            Dictionary<DisambiguableEntity, List<DisambiguationData>> pDisambiguationDataItemsACargar, Dictionary<string, Dictionary<string, float>> pListaEquivalencias, float pUmbral)
        {
            bool cambios = false;
            //Obtenemos las equivalencias que superan el umbral
            foreach (string idA in pListaEquivalenciasItemsACargar.Keys)
            {
                string idAtype = idA.Split('_')[0];
                string idAidentifier = idA.Split('_')[1];
                foreach (string idB in pListaEquivalenciasItemsACargar[idA].Keys)
                {
                    string idBtype = idB.Split('_')[0];
                    string idBidentifier = idB.Split('_')[1];

                    if (pListaEquivalenciasItemsACargar[idA][idB] > pUmbral)
                    {
                        cambios = true;
                        //Obtenemos ItemA
                        DisambiguableEntity itemA = pItems.First(x => x.ID == idAidentifier && x.GetType().Name == idAtype);
                        //Obtenemos ItemB
                        DisambiguableEntity itemB = pItems.First(x => x.ID == idBidentifier && x.GetType().Name == idBtype);

                        //Nos quedamos con el 'mejor'
                        DisambiguableEntity itemEliminar = null;
                        DisambiguableEntity itemBueno = null;
                        string idMalo = null;
                        string idBueno = null;
                        if (pDisambiguationDataItemsACargar.ContainsKey(itemA) && pDisambiguationDataItemsACargar.ContainsKey(itemB))
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
                        foreach (DisambiguationData dataBueno in pDisambiguationDataItemsACargar[itemBueno])
                        {
                            if (pDisambiguationDataItemsACargar.ContainsKey(itemEliminar))
                            {
                                foreach (DisambiguationData dataMalo in pDisambiguationDataItemsACargar[itemEliminar])
                                {
                                    if (dataBueno.property == dataMalo.property && dataBueno.config.type == DisambiguationDataConfigType.equalsItemList)
                                    {
                                        dataBueno.values.UnionWith(dataMalo.values);
                                    }
                                }
                            }
                        }
                        //Cambiamos referencias
                        foreach (DisambiguableEntity item in pDisambiguationDataItemsACargar.Keys)
                        {
                            foreach (DisambiguationData data in pDisambiguationDataItemsACargar[item])
                            {
                                if (data.value == idMalo)
                                {
                                    data.value = idBueno;
                                }
                                if (data.values != null && data.values.Contains(idMalo))
                                {
                                    data.values.Remove(idMalo);
                                    data.values.Add(idBueno);
                                }
                            }
                        }
                        //Eliminamos de la lista de items el duplicado
                        pDisambiguationDataItemsACargar.Remove(itemEliminar);
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
            foreach (DisambiguableEntity item in pItems)
            {
                disambiguationData[item] = item.GetDisambiguationData();
            }
            return disambiguationData;
        }

        /// <summary>
        /// Aplica la desambiguación de los datos.
        /// </summary>
        /// <param name="pItems">Diccionario con el tipo de dato y la lista de los datos a desambiguar.</param>
        /// <returns>Listado con las equivalencias y su peso.</returns>
        private static Dictionary<string, Dictionary<string, float>> ApplyDisambiguation(Dictionary<DisambiguableEntity, List<DisambiguationData>> pItems)
        {
            // Respuesta: Diccionario con los IDs equivalentes.
            Dictionary<string, Dictionary<string, float>> equivalences = new Dictionary<string, Dictionary<string, float>>();

            // Diccionario con tipo de item y su listado correspondiente.
            Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> itemsPorTipo = ObtenerItemsPorTipo(pItems);

            // Realizamos las comprobacions para ver si el input es correcto
            RealizarComprobaciones(itemsPorTipo);

            foreach (string tipo in itemsPorTipo.Keys)
            {
                //Listado con los tipos aptos para el procesado especial
                List<DisambiguationDataConfigType> tiposEspeciales = new List<DisambiguationDataConfigType>();
                tiposEspeciales.Add(DisambiguationDataConfigType.equalsTitle);
                tiposEspeciales.Add(DisambiguationDataConfigType.equalsItem);
                tiposEspeciales.Add(DisambiguationDataConfigType.equalsIdentifiers);

                //Indica si se va a realizar un procesado especial (limitado en cuanto a propiedades pero más rápido)
                bool procesadoEspecial = !itemsPorTipo[tipo].First().Value.Exists(x => !tiposEspeciales.Contains(x.config.type));
                if (procesadoEspecial)
                {
                    equivalences = ProcesadoEspecial(itemsPorTipo[tipo], tipo);
                }
                else
                {
                    equivalences = ProcesadoNormal(itemsPorTipo[tipo], tipo);
                }
            }
            return equivalences;
        }

        /// <summary>
        /// Procesado especial de desambiguación (propiedades limitadas)
        /// </summary>
        /// <param name="pItems">Items para desambiguar</param>
        /// <param name="pTipo">Tipo</param>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, float>> ProcesadoEspecial(Dictionary<DisambiguableEntity, List<DisambiguationData>> pItems, string pTipo)
        {
            //Respuesta
            Dictionary<string, Dictionary<string, float>> equivalences = new Dictionary<string, Dictionary<string, float>>();

            //Propiedad - Valor - IDs
            Dictionary<string, Dictionary<string, HashSet<string>>> dicComparacionEqualsTitle = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            //Propiedad - ScorePositivo
            Dictionary<string, KeyValuePair<float, float>> dicComparacionEqualsTitleScore = new Dictionary<string, KeyValuePair<float, float>>();
            //ID - Propiedad - Valor
            Dictionary<string, Dictionary<string, string>> dicComparacionEqualsTitleValues = new Dictionary<string, Dictionary<string, string>>();

            //Propiedad - Valor - IDs
            Dictionary<string, Dictionary<string, HashSet<string>>> dicComparacionEqualsIdentifiers = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            //Propiedad - ScorePositivo/ScoreNegativo
            Dictionary<string, KeyValuePair<float, float>> dicComparacionEqualsIdentifiersScore = new Dictionary<string, KeyValuePair<float, float>>();
            //ID - Propiedad - Valor
            Dictionary<string, Dictionary<string, string>> dicComparacionEqualsIdentifiersValues = new Dictionary<string, Dictionary<string, string>>();

            //Propiedad - Valor - IDs
            Dictionary<string, Dictionary<string, HashSet<string>>> dicComparacionEqualsItem = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            //Propiedad - ScorePositivo/ScoreNegativo
            Dictionary<string, KeyValuePair<float, float>> dicComparacionEqualsItemScore = new Dictionary<string, KeyValuePair<float, float>>();
            //ID - Propiedad - Valor
            Dictionary<string, Dictionary<string, string>> dicComparacionEqualsItemValues = new Dictionary<string, Dictionary<string, string>>();

            foreach (var itemA in pItems)
            {
                for (int i = 0; i < itemA.Value.Count; i++)
                {
                    switch (itemA.Value[i].config.@type)
                    {
                        case DisambiguationDataConfigType.equalsTitle:
                            ProcessItem(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, itemA, i, itemA.Value[i].config.@type);
                            break;
                        case DisambiguationDataConfigType.equalsIdentifiers:
                            ProcessItem(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, itemA, i, itemA.Value[i].config.@type);
                            break;
                        case DisambiguationDataConfigType.equalsItem:
                            ProcessItem(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, itemA, i, itemA.Value[i].config.@type);
                            break;
                        default:
                            throw new Exception("No implementado");
                    }
                }
            }

            int indice = 0;
            foreach (var itemA in pItems)
            {
                indice++;
                string idA = itemA.Key.ID;

                //Obtenemos las equivalencias en función de los IDs
                Dictionary<string, float> dicEquivalenciasIdentifiers = new Dictionary<string, float>();
                CalculateEquivalencesPositives(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, dicEquivalenciasIdentifiers, idA);
                CalculateEquivalencesNegatives(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, dicEquivalenciasIdentifiers, idA);

                //Obtenemos las equivalencias de los títulos
                Dictionary<string, float> dicEquivalenciasTitle = new Dictionary<string, float>();
                CalculateEquivalencesPositives(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, dicEquivalenciasTitle, idA);
                CalculateEquivalencesPositives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalenciasTitle, idA);
                CalculateEquivalencesNegatives(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, dicEquivalenciasTitle, idA);
                CalculateEquivalencesNegatives(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, dicEquivalenciasTitle, idA);
                CalculateEquivalencesNegatives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalenciasTitle, idA);

                //Obtenemos las equivalencias de los items
                Dictionary<string, float> dicEquivalenciasItem = new Dictionary<string, float>();
                CalculateEquivalencesPositives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalenciasItem, idA);
                CalculateEquivalencesNegatives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalenciasItem, idA);
                CalculateEquivalencesNegatives(dicComparacionEqualsIdentifiers, dicComparacionEqualsIdentifiersValues, dicComparacionEqualsIdentifiersScore, dicEquivalenciasItem, idA);
                CalculateEquivalencesNegatives(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, dicEquivalenciasItem, idA);

                //Almacenamos las equivalencias en función de los IDs
                Dictionary<string, float> dicEquivalencias = new Dictionary<string, float>();
                foreach (string id in dicEquivalenciasIdentifiers.Keys)
                {
                    if (dicEquivalenciasIdentifiers[id] > 0)
                    {
                        dicEquivalencias[id] = dicEquivalenciasIdentifiers[id];
                    }
                }

                //Almacenamos las equivalencias en función de los Títulos
                foreach (string id in dicEquivalenciasTitle.Keys)
                {
                    if (!dicEquivalencias.ContainsKey(id) && dicEquivalenciasTitle[id] > 0)
                    {
                        dicEquivalencias[id] = dicEquivalenciasTitle[id];
                    }
                }

                //Almacenamos las equivalencias en función de los items
                foreach (string id in dicEquivalenciasItem.Keys)
                {
                    if (!dicEquivalencias.ContainsKey(id) && dicEquivalenciasItem[id] > 0)
                    {
                        dicEquivalencias[id] = dicEquivalenciasItem[id];
                    }
                }

                foreach (string idB in dicEquivalencias.Keys)
                {
                    // Agregación de los IDs similares.
                    AddSimilarityId(pTipo, equivalences, dicEquivalencias[idB], idA, idB);
                }
            }
            return equivalences;
        }

        /// <summary>
        /// Procesado especial de desambiguación (propiedades limitadas)
        /// </summary>
        /// <param name="pItems">Items para desambiguar</param>
        /// <param name="pTipo">Tipo</param>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, float>> ProcesadoNormal(Dictionary<DisambiguableEntity, List<DisambiguationData>> pItems, string pTipo)
        {
            //Respuesta
            Dictionary<string, Dictionary<string, float>> equivalences = new Dictionary<string, Dictionary<string, float>>();

            #region Diccionarios auxiliares para la desmbiguación
            // Diccionario Nombres Personas Desnormalizadas
            Dictionary<string, string> dicNomPersonasDesnormalizadas = new Dictionary<string, string>();
            // Diccionario Titulos Desnormalizadas
            Dictionary<string, string> dicTitulosDesnormalizados = new Dictionary<string, string>();
            #endregion

            // Lista con los items del tipo actual
            List<KeyValuePair<DisambiguableEntity, List<DisambiguationData>>> listaItemsTipo = pItems.ToList();

            int indice = 0;
            foreach (var itemA in pItems)
            {
                indice++;
                for (int i = indice; i < pItems.Count; i++)
                {
                    string idA = itemA.Key.ID;
                    string idB = listaItemsTipo[i].Key.ID;

                    // Algoritmo de similaridad.
                    float similarity = GetSimilarity(itemA.Value, listaItemsTipo[i].Value, dicNomPersonasDesnormalizadas, dicTitulosDesnormalizados);

                    // Agregación de los IDs similares.
                    AddSimilarityId(pTipo, equivalences, similarity, idA, idB);
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

        /// <summary>
        /// Raliza comprobaciones con los datos a desambiguar para verificar que la entrada es correcta
        /// </summary>
        /// <param name="pItemsData">Datos de desambiguación</param>
        private static void RealizarComprobaciones(Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> pItemsData)
        {
            //Valores de propiedades
            HashSet<string> ids = new HashSet<string>();
            foreach (string tipo in pItemsData.Keys)
            {
                if (pItemsData[tipo].Count > 0)
                {
                    List<DisambiguationData> data = pItemsData[tipo].First().Value;
                    if (data.Select(x => x.property).Distinct().Count() != data.Count())
                    {
                        throw new Exception("En los items " + tipo + " hay propiedades repetidas");
                    }
                    if (data.Select(x => x.config.type).Contains(DisambiguationDataConfigType.algoritmoNombres) && data.Select(x => x.config.type).Contains(DisambiguationDataConfigType.equalsTitle))
                    {
                        throw new Exception("En los items " + tipo + " hay configurado algoritmoNombres y equalsTitle, no puedes estar los dos de forma simultánea");
                    }
                    foreach (KeyValuePair<DisambiguableEntity, List<DisambiguationData>> item in pItemsData[tipo])
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
                                    if(item.Value[i].value==null)
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



        /// <summary>
        /// Obtiene la similitud entre dos datos del mismo tipo. 
        /// </summary>
        /// <param name="pDataA">Dato A</param>
        /// <param name="pDataB">Dato B</param>
        /// <returns>Número de similitud. A más cerca de 1, más similar es.</returns>
        /// <exception cref="Exception">Configuración no implementada.</exception>
        private static float GetSimilarity(List<DisambiguationData> pDataA, List<DisambiguationData> pDataB, Dictionary<string, string> pDicNomAutoresDesnormalizados, Dictionary<string, string> pDicTitulosDesnormalizados)
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
                                    {
                                        if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value) && dataAAux.value.Equals(dataBAux.value))
                                        {
                                            result += (1 - result) * dataAAux.config.score;
                                        }
                                    }
                                    break;
                                case DisambiguationDataConfigType.equalsItemList:
                                    {
                                        int numIguales = dataAAux.values.Intersect(dataBAux.values).Count();
                                        for (int j = 0; j < numIguales; j++)
                                        {
                                            result += (1 - result) * dataAAux.config.score;
                                        }
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
                                        //No se comprueba
                                        break;
                                    case DisambiguationDataConfigType.algoritmoNombres:
                                        throw new Exception("Si tiene título no debería tener nombres");
                                        break;
                                    case DisambiguationDataConfigType.equalsItem:
                                        {
                                            if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value) && !dataAAux.value.Equals(dataBAux.value))
                                            {
                                                result -= (1 - result) * dataAAux.config.score;
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
                        result += (1 - result) * GetNameSimilarity(dataAAux.value, dataBAux.value, pDicNomAutoresDesnormalizados) * dataAAux.config.score;
                    }
                }
                //Resto
                //TODO cambiarm tiene que haber u minimo en el titulo
                if (result > 0.8f)
                {
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
                                    {
                                        if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value) && dataAAux.value.Equals(dataBAux.value))
                                        {
                                            result += (1 - result) * dataAAux.config.score;
                                        }
                                    }
                                    break;
                                case DisambiguationDataConfigType.equalsItemList:
                                    {
                                        int numIguales = dataAAux.values.Intersect(dataBAux.values).Count();
                                        for (int j = 0; j < numIguales; j++)
                                        {
                                            result += (1 - result) * dataAAux.config.score;
                                        }
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
                                    case DisambiguationDataConfigType.algoritmoNombres:
                                        //No se comprueba
                                        break;
                                    case DisambiguationDataConfigType.equalsTitle:
                                        throw new Exception("Si tiene nombre no debería tener titulo");
                                        break;
                                    case DisambiguationDataConfigType.equalsItem:
                                        {
                                            if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value) && !dataAAux.value.Equals(dataBAux.value))
                                            {
                                                result -= (1 - result) * dataAAux.config.score;
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

            #region resto
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
                            {
                                float similaridad = GetNameSimilarity(dataAAux.value, dataBAux.value, pDicNomAutoresDesnormalizados);
                                result += (1 - result) * similaridad * dataAAux.config.score;
                            }
                            break;
                        case DisambiguationDataConfigType.equalsItem:
                            {
                                if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value) && dataAAux.value.Equals(dataBAux.value))
                                {
                                    result += (1 - result) * dataAAux.config.score;
                                }
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
                                {
                                    if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value) && !dataAAux.value.Equals(dataBAux.value))
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
        /// Obtiene el valor de los diversos datos a procesar.
        /// </summary>
        /// <param name="pDicComparacionItem"></param>
        /// <param name="pDicComparacionValor"></param>
        /// <param name="pDicComparacionScore"></param>
        /// <param name="pData"></param>
        /// <param name="pIndice"></param>
        /// <param name="pTitle"></param>
        private static void ProcessItem(Dictionary<string, Dictionary<string, HashSet<string>>> pDicComparacionItem, Dictionary<string, Dictionary<string, string>> pDicComparacionValor, Dictionary<string, KeyValuePair<float, float>> pDicComparacionScore, KeyValuePair<DisambiguableEntity, List<DisambiguationData>> pData, int pIndice, DisambiguationDataConfigType pType)
        {
            string valor = String.Empty;
            float score = 0;
            float scoreMinus = 0;
            switch (pType)
            {
                case DisambiguationDataConfigType.equalsTitle:
                    valor = NormalizeTitle(pData.Value[pIndice].value);
                    score = pData.Value[pIndice].config.score;
                    scoreMinus = 1;
                    break;
                case DisambiguationDataConfigType.equalsIdentifiers:
                    valor = pData.Value[pIndice].value.ToLower();
                    score = 1;
                    scoreMinus = 1;
                    break;
                case DisambiguationDataConfigType.equalsItem:
                    valor = pData.Value[pIndice].value;
                    score = pData.Value[pIndice].config.score;
                    scoreMinus = pData.Value[pIndice].config.scoreMinus;
                    break;
                default:
                    throw new Exception("implementar");
            }

            if (!pDicComparacionItem.ContainsKey(pData.Value[pIndice].property))
            {
                pDicComparacionItem.Add(pData.Value[pIndice].property, new Dictionary<string, HashSet<string>>());
            }
            if (!pDicComparacionValor.ContainsKey(pData.Key.ID))
            {
                pDicComparacionValor.Add(pData.Key.ID, new Dictionary<string, string>());
            }
            if (!pDicComparacionItem[pData.Value[pIndice].property].ContainsKey(valor))
            {
                pDicComparacionItem[pData.Value[pIndice].property].Add(valor, new HashSet<string>());
            }
            if (!string.IsNullOrEmpty(valor))
            {
                if (!pDicComparacionValor[pData.Key.ID].ContainsKey(valor))
                {
                    pDicComparacionValor[pData.Key.ID].Add(pData.Value[pIndice].property, valor);
                }
            }

            pDicComparacionItem[pData.Value[pIndice].property][valor].Add(pData.Key.ID);
            pDicComparacionScore[pData.Value[pIndice].property] = new KeyValuePair<float, float>(score, scoreMinus);
        }

        /// <summary>
        /// Suma el valor postivo de la similitud entre una lista de items.
        /// </summary>
        /// <param name="pDicComparacionItem"></param>
        /// <param name="pDicComparacionValor"></param>
        /// <param name="pDicComparacionScore"></param>
        /// <param name="pDicEquivalencias"></param>
        /// <param name="pId"></param>
        private static void CalculateEquivalencesPositives(Dictionary<string, Dictionary<string, HashSet<string>>> pDicComparacionItem, Dictionary<string, Dictionary<string, string>> pDicComparacionValor, Dictionary<string, KeyValuePair<float, float>> pDicComparacionScore, Dictionary<string, float> pDicEquivalencias, string pId)
        {
            if (pDicComparacionValor.ContainsKey(pId))
            {
                foreach (string prop in pDicComparacionValor[pId].Keys)
                {
                    if (pDicComparacionScore[prop].Key > 0)
                    {
                        foreach (string id in pDicComparacionItem[prop][pDicComparacionValor[pId][prop]])
                        {
                            if (pDicComparacionValor[pId].ContainsKey(prop) && pDicComparacionValor[id].ContainsKey(prop)
                                && !string.IsNullOrEmpty(pDicComparacionValor[pId][prop]) && !string.IsNullOrEmpty(pDicComparacionValor[id][prop]))
                            {
                                if (id != pId)
                                {
                                    if (!pDicEquivalencias.ContainsKey(id))
                                    {
                                        pDicEquivalencias[id] = 0.0f;
                                    }
                                    pDicEquivalencias[id] += (1 - pDicEquivalencias[id]) * pDicComparacionScore[prop].Key;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resta el valor negativo de la similitud entre una lista de items.
        /// </summary>
        /// <param name="pDicComparacionItem"></param>
        /// <param name="pDicComparacionValor"></param>
        /// <param name="pDicComparacionScore"></param>
        /// <param name="pDicEquivalencias"></param>
        /// <param name="pId"></param>
        private static void CalculateEquivalencesNegatives(Dictionary<string, Dictionary<string, HashSet<string>>> pDicComparacionItem, Dictionary<string, Dictionary<string, string>> pDicComparacionValor, Dictionary<string, KeyValuePair<float, float>> pDicComparacionScore, Dictionary<string, float> pDicEquivalencias, string pId)
        {
            if (pDicComparacionValor.ContainsKey(pId))
            {
                foreach (string id in pDicEquivalencias.Keys)
                {
                    foreach (string prop in pDicComparacionValor[pId].Keys)
                    {
                        if (pDicComparacionScore[prop].Value > 0)
                        {
                            if (pDicComparacionValor[pId].ContainsKey(prop) && pDicComparacionValor[id].ContainsKey(prop)
                                && !string.IsNullOrEmpty(pDicComparacionValor[pId][prop]) && !string.IsNullOrEmpty(pDicComparacionValor[id][prop]))
                            {
                                if (!pDicComparacionItem[prop][pDicComparacionValor[pId][prop]].Contains(id))
                                {
                                    pDicEquivalencias[id] -= pDicEquivalencias[id] * pDicComparacionScore[prop].Value;
                                }
                            }
                        }
                    }
                }
            }
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
                if (!pEquivalences.ContainsKey($"{pTipo}_{pIdA}"))
                {
                    pEquivalences.Add(pTipo + "_" + pIdA, new Dictionary<string, float>());
                }
                if (!pEquivalences.ContainsKey($"{pTipo}_{pIdB}"))
                {
                    pEquivalences.Add(pTipo + "_" + pIdB, new Dictionary<string, float>());
                }
                pEquivalences[$"{pTipo}_{pIdA}"][$"{pTipo}_{pIdB}"] = pSimilaridad;
                pEquivalences[$"{pTipo}_{pIdB}"][$"{pTipo}_{pIdA}"] = pSimilaridad;
            }
        }

        public static float GetNameSimilarity(string pFirma, string pTarget, Dictionary<string, string> pDicNomAutoresDesnormalizados)
        {
            pFirma = ObtenerTextosFirmasNormalizadas(pFirma, pDicNomAutoresDesnormalizados);
            pTarget = ObtenerTextosFirmasNormalizadas(pTarget, pDicNomAutoresDesnormalizados);

            //Almacenamos los scores de cada una de las palabras
            List<float> scores = new List<float>();

            string[] pFirmaNormalizadoSplit = pFirma.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] pTargetNormalizadoSplit = pTarget.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            string[] source = pFirmaNormalizadoSplit;
            string[] target = pTargetNormalizadoSplit;

            int indexTarget = 0;
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
                        if (wordSourceInicial != wordTargetInicial)
                        {
                            //No son las dos iniciales
                            if (wordSource[0] == wordTarget[0])
                            {
                                score = 0.5f;
                                indexTarget = j + 1;
                                //desplazamiento = Math.Abs(j - i);
                                break;
                            }
                        }
                        else
                        {
                            //Son las dos iniciales
                            score = 0.75f;
                            indexTarget = j + 1;
                            //desplazamiento = Math.Abs(j - i);
                            break;
                        }
                    }
                    float scoreSingleName = CompareSingleName(wordSource, wordTarget);
                    if (scoreSingleName > 0)
                    {
                        score = scoreSingleName;
                        indexTarget = j + 1;
                        break;
                    }
                }
                scores.Add(score);
            }
            if (scores.Count > 0)
            {
                return scores.Sum() / source.Length;
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

        private static string ObtenerTextosFirmasNormalizadas(string pText, Dictionary<string, string> pDicNomAutoresDesnormalizados)
        {
            // Comprobación si tenemos guardado el nombre.
            string textoAux = pText;
            if (pDicNomAutoresDesnormalizados.ContainsKey(textoAux))
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
            while (textoSinAcentos.Contains("  "))
            {
                textoSinAcentos = textoSinAcentos.Replace("  ", " ");
            }

            pDicNomAutoresDesnormalizados.Add(textoAux, textoSinAcentos.Trim());

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

        private static float CompareSingleName(string pNameA, string pNameB)
        {
            //TODO
            if (pNameA == pNameB)
            {
                return 1;
            }
            return 0;
            //HashSet<string> ngramsNameA = GetNGramas(pNameA, 2);
            //HashSet<string> ngramsNameB = GetNGramas(pNameB, 2);
            //float tokens_comunes = ngramsNameA.Intersect(ngramsNameB).Count();
            //float union_tokens = ngramsNameA.Union(ngramsNameB).Count();
            //float coeficiente_jackard = tokens_comunes / union_tokens;
            //return coeficiente_jackard;
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
