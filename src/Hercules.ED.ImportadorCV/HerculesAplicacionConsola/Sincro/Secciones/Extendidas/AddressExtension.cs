
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerculesAplicacionConsola.Clases.Extendidas
{
    static class AddressExtension
    {

        //public static List<Dictionary<string, List<string>>> GetPropiedades(this Address address, string propiedadInicio)
        //{
        //    List<Dictionary<string, List<string>>> propiedadesLista = new List<Dictionary<string, List<string>>>();

        //    //string vcard_hasCountryName = address.GetPropiedad(address.GetVcard_hasCountryName(), propiedadInicio);
        //    //string roh_hasProvince = address.GetPropiedad(address.GetRoh_hasProvince(), propiedadInicio);
        //    //string vcard_hasRegion = address.GetPropiedad(address.GetVcard_hasRegion(), propiedadInicio);
        //    string vcard_postal_code = address.GetPropiedad(address.GetVcard_postal_code(), propiedadInicio);
        //    string vcard_extended_address = address.GetPropiedad(address.GetVcard_extended_address(), propiedadInicio);
        //    string vcard_street_address = address.GetPropiedad(address.GetVcard_street_address(), propiedadInicio);
        //    string vcard_locality = address.GetPropiedad(address.GetVcard_locality(), propiedadInicio);

        //    if (propiedadInicio.Equals("http://w3id.org/roh/birthplace"))
        //    {
        //        //AddNoNull(propiedadesLista, diccionario(vcard_hasCountryName, stringToListString(address.idGNOSSSecondary,address.IdVcard_hasCountryName)));
        //        //AddNoNull(propiedadesLista, diccionario(roh_hasProvince, stringToListString(address.idGNOSSSecondary,address.IdRoh_hasProvince)));
        //        //AddNoNull(propiedadesLista, diccionario(vcard_hasRegion, stringToListString(address.idGNOSSSecondary,address.IdVcard_hasRegion)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_postal_code, StringToListString(address.idGNOSSSecondary, address.Vcard_postal_code)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_extended_address, StringToListString(address.idGNOSSSecondary, address.Vcard_extended_address)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_street_address, StringToListString(address.idGNOSSSecondary, address.Vcard_street_address)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_locality, StringToListString(address.idGNOSSSecondary, address.Vcard_locality)));
        //    }
        //    else
        //    {
        //        //AddNoNull(propiedadesLista, diccionario(vcard_hasCountryName, stringToListString(address.GNOSSID,address.IdVcard_hasCountryName)));
        //        //AddNoNull(propiedadesLista, diccionario(roh_hasProvince, stringToListString(address.GNOSSID,address.IdRoh_hasProvince)));
        //        //AddNoNull(propiedadesLista, diccionario(vcard_hasRegion, stringToListString(address.GNOSSID,address.IdVcard_hasRegion)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_postal_code, StringToListString(address.GNOSSID, address.Vcard_postal_code)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_extended_address, StringToListString(address.GNOSSID, address.Vcard_extended_address)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_street_address, StringToListString(address.GNOSSID, address.Vcard_street_address)));
        //        AddNoNull(propiedadesLista, Diccionario(vcard_locality, StringToListString(address.GNOSSID, address.Vcard_locality)));
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
        //private static string GetPropiedad(this Address address, string propiedadFinal, string propiedadInicio)
        //{
        //    string propiedad = "";
        //    if (!string.IsNullOrEmpty(propiedadInicio))
        //    {
        //        propiedad = propiedadInicio + "@@@" + address.RdfType + "|" + propiedadFinal;
        //    }
        //    else
        //    {
        //        propiedad = propiedadFinal;
        //    }
        //    return propiedad;
        //}

        //private static string GetVcard_hasCountryName(this Address address)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(address, nameof(address.Vcard_hasCountryName));
        //}

        //private static string GetRoh_hasProvince(this Address address)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(address, nameof(address.Roh_hasProvince));
        //}

        //private static string GetVcard_hasRegion(this Address address)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(address, nameof(address.Vcard_hasRegion));
        //}

        //private static string GetVcard_postal_code(this Address address)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(address, nameof(address.Vcard_postal_code));
        //}

        //private static string GetVcard_extended_address(this Address address)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(address, nameof(address.Vcard_extended_address));
        //}

        //private static string GetVcard_street_address(this Address address)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(address, nameof(address.Vcard_street_address));
        //}

        //private static string GetVcard_locality(this Address address)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(address, nameof(address.Vcard_locality));
        //}

        public static Dictionary<string, List<string>> Diccionario(string cadena, List<string> listado)
        {
            Dictionary<string, List<string>> diccionario = new Dictionary<string, List<string>>();
            if (string.IsNullOrEmpty(cadena) || listado.Count < 1)
            {
                return null;
            }
            diccionario.Add(cadena, listado);
            return diccionario;
        }
        public static List<string> StringToListString(string GNOSSID, string cadena)
        {
            List<string> listado = new List<string>();
            if (!string.IsNullOrEmpty(cadena))
            {
                listado.Add(GNOSSID + "@@@" + cadena);
            }
            return listado;
        }
    }
}
