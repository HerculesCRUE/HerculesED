
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerculesAplicacionConsola.Clases.Extendidas
{
    static class PersonalDataExtension
    {

        //private static string GetAttributeFrom<T>(object instance, string propertyName) where T : Attribute
        //{
        //    var attrType = typeof(RDFPropertyAttribute);
        //    var property = instance.GetType().GetProperty(propertyName);
        //    return ((RDFPropertyAttribute)property.GetCustomAttributes(attrType, false).First()).RDFProperty;
        //}
        //public static List<Dictionary<string, List<string>>> GetPropiedades(this PersonalData personalData)
        //{
        //    List<Dictionary<string, List<string>>> propiedadesLista = new List<Dictionary<string, List<string>>>();

        //    //AddNoNull(propiedadesLista,diccionario(personalData.GetFoaf_Gender(), stringToListString(personalData.Foaf_gender)));
        //    //AddNoNull(propiedadesLista,diccionario(personalData.GetSchema_nationality(), stringToListString(personalData.Schema_nationality)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoRoh_nie(), StringToListString(personalData.Roh_nie)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoVivo_researcherId(), StringToListString(personalData.Vivo_researcherId)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoVivo_scopusId(), StringToListString(personalData.Vivo_scopusId)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoFoaf_familyName(), StringToListString(personalData.Foaf_familyName)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoRoh_secondFamilyName(), StringToListString(personalData.Roh_secondFamilyName)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoVcard_email(), StringToListString(personalData.Vcard_email)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoFoaf_img(), StringToListString(personalData.Foaf_img)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoRoh_dni(), StringToListString(personalData.Roh_dni)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoRoh_ORCID(), StringToListString(personalData.Roh_ORCID)));
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoRoh_passport(), StringToListString(personalData.Roh_passport)));
        //    if (personalData.Vcard_birth_date.HasValue)
        //    {
        //        AddNoNull(propiedadesLista, Diccionario(personalData.AtributoVcard_birth_date(), StringToListString(DatetimeStringGNOSS(personalData.Vcard_birth_date.Value))));
        //    }
        //    AddNoNull(propiedadesLista, Diccionario(personalData.AtributoFoaf_firstName(), StringToListString(personalData.Foaf_firstName)));


        //    propiedadesLista.AddRange(personalData.Vcard_hasTelephone.GetPropiedades(personalData.AtributoVcard_hasTelephone()));
        //    propiedadesLista.AddRange(personalData.Roh_hasFax.GetPropiedades(personalData.AtributoRoh_hasFax()));
        //    propiedadesLista.AddRange(personalData.Roh_hasMobilePhone.GetPropiedades(personalData.AtributoRoh_hasMobilePhone()));
        //    propiedadesLista.AddRange(personalData.Roh_birthplace.GetPropiedades(personalData.AtributoRoh_birthplace()));
        //    propiedadesLista.AddRange(personalData.Vcard_address.GetPropiedades(personalData.AtributoVcard_address()));
        //    foreach (Document documento in personalData.Roh_otherIds)
        //    {
        //        propiedadesLista.AddRange(documento.GetPropiedades());
        //    }


        //    return propiedadesLista;
        //}


        //private static void AddNoNull(List<Dictionary<string, List<string>>> propiedadesLista, Dictionary<string, List<string>> diccionario)
        //{
        //    if (diccionario != null)
        //    {
        //        if (diccionario.Keys.Count > 0)
        //        {
        //            propiedadesLista.Add(diccionario);
        //        }
        //    }
        //}
        //private static string AtributoFoaf_gender(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Foaf_gender));
        //}
        //private static string AtributoSchema_nationality(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Schema_nationality));
        //}
        //private static string AtributoRoh_nie(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Roh_nie));
        //}
        //private static string AtributoVivo_researcherId(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Vivo_researcherId));
        //}
        //private static string AtributoVivo_scopusId(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Vivo_scopusId));
        //}
        //private static string AtributoFoaf_familyName(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Foaf_familyName));
        //}
        //private static string AtributoRoh_secondFamilyName(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Roh_secondFamilyName));
        //}
        //private static string AtributoVcard_email(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Vcard_email));
        //}
        //private static string AtributoFoaf_img(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Foaf_img));
        //}
        //private static string AtributoRoh_dni(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Roh_dni));
        //}
        //private static string AtributoRoh_ORCID(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Roh_ORCID));
        //}
        //private static string AtributoRoh_passport(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Roh_passport));
        //}
        //private static string AtributoVcard_birth_date(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Vcard_birth_date));
        //}
        //private static string AtributoFoaf_firstName(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Foaf_firstName));
        //}
        //private static string AtributoVcard_hasTelephone(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Vcard_hasTelephone));
        //}
        //private static string AtributoRoh_hasFax(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Roh_hasFax));
        //}
        //private static string AtributoRoh_hasMobilePhone(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Roh_hasMobilePhone));
        //}
        //private static string AtributoRoh_birthplace(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Roh_birthplace));
        //}
        //private static string AtributoVcard_address(this PersonalData personalData)
        //{
        //    return GetAttributeFrom<RDFPropertyAttribute>(personalData, nameof(personalData.Vcard_address));
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
        public static string DatetimeStringGNOSS(DateTime dateTime)
        {
            string fechaString = "";
            fechaString = dateTime.ToString().Replace("-", "").Replace("T", "").Replace(":", "").Split("+")[0];
            string[] fechaAux = fechaString.Split("/");
            string anio = fechaAux[2].Split(" ")[0];

            fechaString = anio + fechaAux[1] + fechaAux[0] + "000000";
            return fechaString;
        }

    }
}
