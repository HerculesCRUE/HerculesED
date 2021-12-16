using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using PublicationConnect.ROs.Publications;
using PublicationConnect.ROs.Publications.Models;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using PublicationConnect.Controllers;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;
//using Newtonsoft.Json.Linq.JObject;
using System.IO;
using System.Text;
namespace usuarios
{
    public class Usuarios
    {
        


     //    
       public float GetNameSimilarity(string pFirma, string pTarget)
        {
            pFirma = ObtenerTextosFirmasNormalizadas(pFirma);
            pTarget = ObtenerTextosFirmasNormalizadas(pTarget);

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

        private string ObtenerTextosFirmasNormalizadas(string pText)
        {
            pText=pText.ToLower();
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

            return textoSinAcentos.Trim();
        }

        private float CompareSingleName(string pNameA, string pNameB)
        {
            HashSet<string> ngramsNameA = GetNGramas(pNameA, 2);
            HashSet<string> ngramsNameB = GetNGramas(pNameB, 2);
            float tokens_comunes = ngramsNameA.Intersect(ngramsNameB).Count();
            float union_tokens = ngramsNameA.Union(ngramsNameB).Count();
            float coeficiente_jackard = tokens_comunes / union_tokens;
            return coeficiente_jackard;
        }

        private HashSet<string> GetNGramas(string pText, int pNgramSize)
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