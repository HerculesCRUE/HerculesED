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
        public static Dictionary<string, Dictionary<string, float>> Disambiguate(List<DisambiguableEntity> pItems/*, List<DisambiguableEntity> pItemBBDD*/)
        {
            // --- 1º Aplica la desambiguación con los items a cargar

            // Obtenemos las propiedades que nos permitirán la desambiguación.
            Dictionary<DisambiguableEntity, List<DisambiguationData>> disambiguationData = GetDisambiguationData(pItems);

            // Aplicamos la desambiguación.
            Dictionary<string, Dictionary<string, float>> listaEquivalencias = ApplyDisambiguation(disambiguationData);

            // TODO: Devolver cambios.
            return listaEquivalencias;
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
        /// <returns>Listado de los datos desambiguados.</returns>
        private static Dictionary<string, Dictionary<string, float>> ApplyDisambiguation(Dictionary<DisambiguableEntity, List<DisambiguationData>> pItems, float pUmbral = 0.8f)
        {
            // Diccionario Nombres Autores Desnormalizados
            Dictionary<string, string> dicNomAutoresDesnormalizados = new Dictionary<string, string>();

            // Diccionario con los IDs equivalentes.
            Dictionary<string, Dictionary<string, float>> equivalences = new Dictionary<string, Dictionary<string, float>>();

            // Objeto equivalente.
            Dictionary<DisambiguableEntity, List<string>> equivalenceObjects = new Dictionary<DisambiguableEntity, List<string>>();

            // Lista de objetos desambiguados.
            Dictionary<string, List<DisambiguableEntity>> disambiguatedObjects = new Dictionary<string, List<DisambiguableEntity>>();

            // Diccionario con tipo de item y su listado correspondiente.
            Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>> itemsPorTipo = new Dictionary<string, Dictionary<DisambiguableEntity, List<DisambiguationData>>>();

            foreach (var item in pItems)
            {
                string name = item.Key.GetType().Name;
                if (!itemsPorTipo.ContainsKey(name))
                {
                    itemsPorTipo.Add(name, new Dictionary<DisambiguableEntity, List<DisambiguationData>>());
                    disambiguatedObjects.Add(name, new List<DisambiguableEntity>());
                }
                itemsPorTipo[name].Add(item.Key, item.Value);
            }

            foreach (string tipo in itemsPorTipo.Keys)
            {
                //Propiedad - Valor - IDs
                Dictionary<string, Dictionary<string, HashSet<string>>> dicComparacionEqualsTitle = new Dictionary<string, Dictionary<string, HashSet<string>>>();
                //Propiedad - ScorePositivo/ScoreNegativo
                Dictionary<string, KeyValuePair<float, float>> dicComparacionEqualsTitleScore = new Dictionary<string, KeyValuePair<float, float>>();
                //ID - Propiedad - Valor
                Dictionary<string, Dictionary<string, string>> dicComparacionEqualsTitleValues = new Dictionary<string, Dictionary<string, string>>();

                //Propiedad - Valor - IDs
                Dictionary<string, Dictionary<string, HashSet<string>>> dicComparacionEqualsItem = new Dictionary<string, Dictionary<string, HashSet<string>>>();
                //Propiedad - ScorePositivo/ScoreNegativo
                Dictionary<string, KeyValuePair<float, float>> dicComparacionEqualsItemScore = new Dictionary<string, KeyValuePair<float, float>>();
                //ID - Propiedad - Valor
                Dictionary<string, Dictionary<string, string>> dicComparacionEqualsItemValues = new Dictionary<string, Dictionary<string, string>>();

                bool procesadoEspecial = false;
                List<DisambiguationDataConfigType> tiposEspeciales = new List<DisambiguationDataConfigType>();
                tiposEspeciales.Add(DisambiguationDataConfigType.equalsTitle);
                tiposEspeciales.Add(DisambiguationDataConfigType.equalsIdentifiers);

                if (!itemsPorTipo[tipo].First().Value.Exists(x => !tiposEspeciales.Contains(x.config.type)))
                {
                    procesadoEspecial = true;
                    foreach (var itemA in itemsPorTipo[tipo])
                    {
                        for (int i = 0; i < itemA.Value.Count; i++)
                        {
                            if (itemA.Value[i].config.@type == DisambiguationDataConfigType.equalsTitle)
                            {
                                ProcessItem(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, itemA, i, itemA.Value[i].config.@type);
                            }
                            else if (itemA.Value[i].config.@type == DisambiguationDataConfigType.equalsIdentifiers)
                            {
                                ProcessItem(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, itemA, i, itemA.Value[i].config.@type);
                            }
                        }
                    }
                }

                int indice = 0;
                List<KeyValuePair<DisambiguableEntity, List<DisambiguationData>>> listaItemsTipo = itemsPorTipo[tipo].ToList();

                foreach (var itemA in itemsPorTipo[tipo])
                {
                    indice++;

                    if (!procesadoEspecial)
                    {
                        for (int i = indice; i < itemsPorTipo[tipo].Count; i++)
                        {
                            string idA = itemA.Key.ID;
                            string idB = listaItemsTipo[i].Key.ID;

                            // Algoritmo de similaridad.
                            float similarity = GetSimilarity(itemA.Value, listaItemsTipo[i].Value, dicNomAutoresDesnormalizados);

                            // Agregación de los IDs similares.
                            AddSimilarityId(tipo, equivalences, similarity, idA, idB);
                        }
                    }
                    else
                    {
                        string idA = itemA.Key.ID;
                        Dictionary<string, float> dicEquivalencias = new Dictionary<string, float>();

                        // Sumar equivalencias positivas.
                        CalculateEquivalencesPositives(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, dicEquivalencias, idA);
                        CalculateEquivalencesPositives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalencias, idA);

                        // Restar equivalencias negativas.
                        CalculateEquivalencesNegatives(dicComparacionEqualsTitle, dicComparacionEqualsTitleValues, dicComparacionEqualsTitleScore, dicEquivalencias, idA);
                        CalculateEquivalencesNegatives(dicComparacionEqualsItem, dicComparacionEqualsItemValues, dicComparacionEqualsItemScore, dicEquivalencias, idA);

                        foreach (string idB in dicEquivalencias.Keys)
                        {
                            // Agregación de los IDs similares.
                            AddSimilarityId(tipo, equivalences, dicEquivalencias[idB], idA, idB);
                        }
                    }
                }
            }

            return equivalences;
        }

        /// <summary>
        /// Obtiene la similitud entre dos datos del mismo tipo. 
        /// </summary>
        /// <param name="pDataA">Dato A</param>
        /// <param name="pDataB">Dato B</param>
        /// <returns>Número de similitud. A más cerca de 1, más similar es.</returns>
        /// <exception cref="Exception">Configuración no implementada.</exception>
        private static float GetSimilarity(List<DisambiguationData> pDataA, List<DisambiguationData> pDataB, Dictionary<string, string> pDicNomAutoresDesnormalizados)
        {
            float result = 0;

            for (int i = 0; i < pDataA.Count; i++)
            {
                DisambiguationData dataAAux = pDataA[i];
                DisambiguationData dataBAux = pDataB[i];

                if (dataAAux.config.score > 0)
                {
                    switch (dataAAux.config.type)
                    {
                        case DisambiguationDataConfigType.equalsIdentifiers:
                            if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value) && dataAAux.value.Equals(dataBAux.value))
                            {
                                result += (1 - result) * dataAAux.config.score;
                            }
                            break;

                        case DisambiguationDataConfigType.equalsTitle:
                            if (!NormalizeTitle(dataAAux.value).Equals(NormalizeTitle(dataBAux.value)))
                            {
                                result += (1 - result) * dataAAux.config.score;
                            }
                            break;

                        case DisambiguationDataConfigType.algoritmoNombres:
                            float similaridad = GetNameSimilarity(dataAAux.value, dataBAux.value, pDicNomAutoresDesnormalizados);
                            result += (1 - result) * similaridad * dataAAux.config.score;
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
                                if (!string.IsNullOrEmpty(dataAAux.value) && !string.IsNullOrEmpty(dataBAux.value) && !dataAAux.value.Equals(dataBAux.value))
                                {
                                    result -= result * dataAAux.config.scoreMinus;
                                }
                                break;

                            case DisambiguationDataConfigType.equalsTitle:
                                if (!NormalizeTitle( dataAAux.value).Equals(NormalizeTitle(dataBAux.value)))
                                {
                                    result -= result * dataAAux.config.scoreMinus;
                                }
                                break;

                            case DisambiguationDataConfigType.algoritmoNombres:
                                float similaridad = GetNameSimilarity(dataAAux.value, dataBAux.value, pDicNomAutoresDesnormalizados);
                                result -= (1 - result) * similaridad * dataAAux.config.score;
                                break;

                            default:
                                throw new Exception("No está implementado.");
                        }
                    }
                }
            }

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
            normalizedString = sb.ToString().Normalize(NormalizationForm.FormD);
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

            switch(pType)
            {
                case DisambiguationDataConfigType.equalsTitle:
                    valor = NormalizeTitle(pData.Value[pIndice].value);
                    break;
                case DisambiguationDataConfigType.equalsIdentifiers:
                    valor = pData.Value[pIndice].value.ToLower();
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
            pDicComparacionScore[pData.Value[pIndice].property] = new KeyValuePair<float, float>(pData.Value[pIndice].config.score, pData.Value[pIndice].config.scoreMinus);
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
                if (!pEquivalences.ContainsKey(pTipo + "_" + pIdA))
                {
                    pEquivalences.Add(pTipo + "_" + pIdA, new Dictionary<string, float>());
                }
                if (!pEquivalences.ContainsKey(pTipo + "_" + pIdB))
                {
                    pEquivalences.Add(pTipo + "_" + pIdB, new Dictionary<string, float>());
                }
                pEquivalences[pTipo + "_" + pIdA][pTipo + "_" + pIdB] = pSimilaridad;
                pEquivalences[pTipo + "_" + pIdB][pTipo + "_" + pIdA] = pSimilaridad;
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

        private static float CompareSingleName(string pNameA, string pNameB)
        {
            HashSet<string> ngramsNameA = GetNGramas(pNameA, 2);
            HashSet<string> ngramsNameB = GetNGramas(pNameB, 2);
            float tokens_comunes = ngramsNameA.Intersect(ngramsNameB).Count();
            float union_tokens = ngramsNameA.Union(ngramsNameB).Count();
            float coeficiente_jackard = tokens_comunes / union_tokens;
            return coeficiente_jackard;
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

        private static DisambiguableEntity CreateDisambiguationObject(string pType, List<DisambiguationData> pDataA, List<DisambiguationData> pDataB)
        {
            switch (pType)
            {
                case "DisambiguationPublication":
                    DisambiguationPublication disambiguatedPublication = new DisambiguationPublication();
                    disambiguatedPublication.ID = Guid.NewGuid().ToString();
                    for (int j = 0; j < pDataA.Count; j++)
                    {
                        PropertyInfo prop = disambiguatedPublication.GetType().GetProperty(pDataA[j].@property, BindingFlags.Public | BindingFlags.Instance);

                        if (!string.IsNullOrEmpty(pDataA[j].value))
                        {
                            prop.SetValue(disambiguatedPublication, pDataA[j].value, null);
                        }
                        else if (!string.IsNullOrEmpty(pDataB[j].value))
                        {
                            prop.SetValue(disambiguatedPublication, pDataB[j].value, null);
                        }
                    }
                    return disambiguatedPublication;

                case "DisambiguationPerson":
                    DisambiguationPerson disambiguatedPerson = new DisambiguationPerson();
                    disambiguatedPerson.ID = Guid.NewGuid().ToString();
                    for (int j = 0; j < pDataA.Count; j++)
                    {
                        PropertyInfo prop = disambiguatedPerson.GetType().GetProperty(pDataA[j].@property, BindingFlags.Public | BindingFlags.Instance);

                        if (!string.IsNullOrEmpty(pDataA[j].value))
                        {
                            prop.SetValue(disambiguatedPerson, pDataA[j].value, null);
                        }
                        else if (!string.IsNullOrEmpty(pDataB[j].value))
                        {
                            prop.SetValue(disambiguatedPerson, pDataB[j].value, null);
                        }
                    }
                    return disambiguatedPerson;
                default:
                    throw new Exception("Tipo de objeto no implementado");
            }
        }
    }
}
