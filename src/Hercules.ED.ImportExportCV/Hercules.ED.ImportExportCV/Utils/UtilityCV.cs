using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utils
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
        private static Dictionary<string, List<string>> PropertyNotEditable = new ()
        {
            { "http://w3id.org/roh/crisIdentifier", new List<string>() },
            { "http://w3id.org/roh/isValidated", new List<string>(){ "true"} },
            { "http://w3id.org/roh/validationStatusPRC", new List<string>(){ "pendiente", "validado" } }
        };

        public static Dictionary<string, List<string>> GetPropertyNotEditable()
        {
            return PropertyNotEditable;
        }

        private static Dictionary<string, string> dicPrefix = new () {
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

        public static Dictionary<string, string> GetDicPrefix()
        {
            return dicPrefix;
        }


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

        /// <summary>
        /// Devuelve el identificador numerico apartir del tipo de documento(SAD1, SAD2, SAD3).
        /// </summary>
        /// <param name="tipoDocumento">Tipo de documento</param>
        /// <returns>Identificador del FECYT</returns>
        public static string IdentificadorFECYT(string tipoDocumento)
        {
            if (string.IsNullOrEmpty(tipoDocumento))
            {
                return null;
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD1"))
            {
                return "060.010.010.000";
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD2"))
            {
                return "060.010.020.000";
            }
            if (tipoDocumento.Equals("http://gnoss.com/items/scientificactivitydocument_SAD3"))
            {
                return "060.010.030.000";
            }
            return null;
        }
                
    }
}