
using HerculesAplicacionConsola.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HerculesAplicacionConsola.Clases.Extendidas
{
    static class DocumentExtension
    {
        //public static List<Dictionary<string, List<string>>> GetPropiedades(this Document document)
        //{
        //    List<Dictionary<string, List<string>>> propiedadesLista = new List<Dictionary<string, List<string>>>();

        //    //AddNoNull(propiedadesLista, diccionario(foaf_topic, stringToListString(document.Foaf_topic)));
        //    //AddNoNull(propiedadesLista, diccionario(dc_title, stringToListString(document.Dc_title)));

        //    return propiedadesLista;
        //}
        //private static string GetAttributeFrom<T>(object instance, string propertyName) where T : Attribute
        //{
        //    var attrType = typeof(RDFPropertyAttribute);
        //    var property = instance.GetType().GetProperty(propertyName);
        //    return ((RDFPropertyAttribute)property.GetCustomAttributes(attrType, false).First()).RDFProperty;
        //}

        private static void AddNoNull(List<Dictionary<string, List<string>>> propiedadesLista, Dictionary<string, List<string>> diccionario)
        {
            if (diccionario != null)
            {
                if (diccionario.Keys.Count > 0)
                {
                    propiedadesLista.Add(diccionario);
                }
            }
        }
        //private static string GetPropiedad(this Document document, string propiedadFinal, string propiedadInicio)
        //{
        //    string propiedad = "";
        //    if (!string.IsNullOrEmpty(propiedadInicio))
        //    {
        //        propiedad = propiedadInicio + "@@@" + document.RdfType + "|" + propiedadFinal;
        //    }
        //    else
        //    {
        //        propiedad = propiedadFinal;
        //    }
        //    return propiedad;
        //}

        //private static string GetFoaf_topic(this Document document)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(document, nameof(document.Foaf_topic));
        //}

        //private static string GetDc_title(this Document document)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(document, nameof(document.Dc_title));
        //}

        public static Dictionary<string, List<string>> Diccionario(string cadena, List<string> listado)
        {
            Dictionary<string, List<string>> diccionario = new Dictionary<string, List<string>>();
            if (listado.Count() > 0)
            {
                diccionario.Add(cadena, listado);
            }
            return diccionario;
        }
        public static List<string> StringToListString(string cadena)
        {
            List<string> listado = new List<string>();
            if (!string.IsNullOrEmpty(cadena))
            {
                listado.Add(cadena);
            }
            return listado;
        }
    }
}
