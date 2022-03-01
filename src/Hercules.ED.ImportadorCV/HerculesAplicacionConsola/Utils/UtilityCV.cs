using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace GuardadoCV.Models.Utils
{
    public class UtilityCV
    {
        /// <summary>
        /// Propiedad para marcar las entidades como públicas
        /// </summary>
        public static string PropertyIspublic { get { return "http://w3id.org/roh/isPublic"; } }

        /// <summary>
        /// Propiedad para comprobar si no es editable, tiene que tener en alguna propiedad
        /// de las claves algún valor de los valores
        /// </summary>
        public static Dictionary<string, List<string>> PropertyNotEditable = new Dictionary<string, List<string>>()
        {
            { "http://w3id.org/roh/crisIdentifier", new List<string>() },
            { "http://w3id.org/roh/isValidated", new List<string>(){ "true"} }
            //TODO estado de validacion
        };

        public static Dictionary<string, string> dicPrefix = new Dictionary<string, string>() {
            { "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
            {"rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
            {"foaf", "http://xmlns.com/foaf/0.1/" },
            {"vivo", "http://vivoweb.org/ontology/core#" },
            {"owl", "http://www.w3.org/2002/07/owl#" },
            {"bibo", "http://purl.org/ontology/bibo/" },
            {"roh", "http://w3id.org/roh/" },
            {"dct", "http://purl.org/dc/terms/" },
            {"xsd", "http://www.w3.org/2001/XMLSchema#" },
            {"obo", "http://purl.obolibrary.org/obo/" },
            {"vcard", "https://www.w3.org/2006/vcard/ns#" },
            {"dc", "http://purl.org/dc/elements/1.1/" },
            {"gn", "http://www.geonames.org/ontology#" }
        };

        
        /// <summary>
        /// Cambia la propiedad añadiendole elprefijo
        /// </summary>
        /// <param name="pProperty">Propiedad con la URL completa</param>
        /// <returns>Url con prefijo</returns>
        public static string AniadirPrefijo(string pProperty)
        {
            KeyValuePair<string, string> prefix = dicPrefix.First(x => pProperty.StartsWith(x.Value));
            return pProperty.Replace(prefix.Value, prefix.Key + ":");
        }

        public static string GetTextLang(string pLang, Dictionary<string, string> pValores)
        {
            if (pValores == null)
            {
                return "";
            }
            else if (pValores.ContainsKey(pLang))
            {
                return pValores[pLang];
            }
            else if (pValores.ContainsKey("es"))
            {
                return pValores["es"];
            }
            else if (pValores.Count > 0)
            {
                return pValores.Values.First();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Método para dividir listas
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pItems">Listado</param>
        /// <param name="pSize">Tamaño</param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> pItems, int pSize)
        {
            for (int i = 0; i < pItems.Count; i += pSize)
            {
                yield return pItems.GetRange(i, Math.Min(pSize, pItems.Count - i));
            }
        }



    }
}