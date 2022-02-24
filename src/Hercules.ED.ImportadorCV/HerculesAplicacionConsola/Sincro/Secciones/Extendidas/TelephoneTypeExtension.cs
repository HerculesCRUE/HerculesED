using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerculesAplicacionConsola.Clases.Extendidas
{
    static class TelephoneTypeExtension
    {
        //public static List<Dictionary<string, List<string>>> GetPropiedades(this TelephoneType telephoneType, string propiedadInicio)
        //{
        //    List<Dictionary<string, List<string>>> propiedadesLista = new List<Dictionary<string, List<string>>>();

        //    string roh_hasExtension = telephoneType.GetPropiedad(telephoneType.GetRoh_HasExtension(), propiedadInicio);
        //    string roh_hasInternationalCode = telephoneType.GetPropiedad(telephoneType.GetRoh_hasInternationalCode(), propiedadInicio);
        //    string vcard_hasValue = telephoneType.GetPropiedad(telephoneType.GetVcard_hasValue(), propiedadInicio);


        //    if (propiedadInicio.Equals("https://www.w3.org/2006/vcard/ns#hasTelephone"))
        //    {
        //        AddNoNull(propiedadesLista, Diccionario(roh_hasExtension, StringToListString(telephoneType.GNOSSID, telephoneType.Roh_hasExtension)));
        //        AddNoNull(propiedadesLista, Diccionario(roh_hasInternationalCode, StringToListString(telephoneType.GNOSSID, telephoneType.Roh_hasInternationalCode)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_hasValue, StringToListString(telephoneType.GNOSSID, telephoneType.Vcard_hasValue)));
        //    }
        //    else
        //    {
        //        AddNoNull(propiedadesLista, Diccionario(roh_hasExtension, StringToListString(telephoneType.idGNOSSSecondary, telephoneType.Roh_hasExtension)));
        //        AddNoNull(propiedadesLista, Diccionario(roh_hasInternationalCode, StringToListString(telephoneType.idGNOSSSecondary, telephoneType.Roh_hasInternationalCode)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_hasValue, StringToListString(telephoneType.idGNOSSSecondary, telephoneType.Vcard_hasValue)));
        //    }


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
        //private static string GetPropiedad(this TelephoneType telephoneType, string propiedadFinal, string propiedadInicio)
        //{
        //    string propiedad = "";
        //    if (!string.IsNullOrEmpty(propiedadInicio))
        //    {
        //        propiedad = propiedadInicio + "@@@" + telephoneType.RdfType + "|" + propiedadFinal;
        //    }
        //    else
        //    {
        //        propiedad = propiedadFinal;
        //    }
        //    return propiedad;
        //}

        //private static string GetRoh_HasExtension(this TelephoneType telephoneType)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(telephoneType, nameof(telephoneType.Roh_hasExtension));
        //}

        //private static string GetRoh_hasInternationalCode(this TelephoneType telephoneType)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(telephoneType, nameof(telephoneType.Roh_hasInternationalCode));
        //}

        //private static string GetVcard_hasValue(this TelephoneType telephoneType)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(telephoneType, nameof(telephoneType.Vcard_hasValue));
        //}

        private static Dictionary<string, List<string>> Diccionario(string cadena, List<string> listado)
        {
            Dictionary<string, List<string>> diccionario = new Dictionary<string, List<string>>();
            if (string.IsNullOrEmpty(cadena) || listado.Count < 1)
            {
                return null;
            }
            diccionario.Add(cadena, listado);
            return diccionario;
        }

        private static List<string> StringToListString(string GNOSSID, string cadena)
        {
            List<string> listado = new List<string>();
            if (!string.IsNullOrEmpty(cadena))
            {
                listado.Add(GNOSSID +"@@@"+ cadena);
            }
            return listado;
        }
    }
}
