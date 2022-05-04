using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ExportadorWebCV.Utils
{
    public class UtilityExportar
    {
        public static List<Tuple<string, string>> GetDatos(ResourceApi pResourceApi, string pCVID)
        {
            string select = $@"select distinct ?prop ?o";
            string where = $@"
where {{
    ?cv <http://w3id.org/roh/personalData> ?personalData . 
    ?personalData ?prop ?o
    FILTER(?cv =<{pCVID}>)
}}";

            List<Tuple<string, string>> listaResultado = new List<Tuple<string, string>>();

            SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            if (resultData.results.bindings.Count == 0)
            {
                return null;
            }
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                listaResultado.Add(new Tuple<string, string>(fila["prop"].value, fila["o"].value));
            }

            return listaResultado;
        }
        public static void AniadirItems(cvnRootResultBean cvn, List<CvnItemBean> listado)
        {
            if (cvn.cvnRootBean == null)
            {
                cvn.cvnRootBean = listado.ToArray();
            }
            else
            {
                cvn.cvnRootBean = cvn.cvnRootBean.Union(listado).ToArray();
            }
        }

        public static string EliminarRDF(string cadena)
        {
            if (string.IsNullOrEmpty(cadena))
            {
                return "";
            }
            return string.Join("|", cadena.Split("|").Select(x => x.Split("@@@")[0]));
        }

        public static bool Comprobar<T>(IEnumerable<T> enumeracion)
        {
            return enumeracion.Any();
        }

        public static void AddCvnItemBeanCvnString(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => EliminarRDF(x.prop).StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0)
            {
                itemBean.Items.Add(new CvnItemBeanCvnString()
                {
                    Code = code,
                    Value = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                        .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1]
                });
            }
        }

        public static void AddDireccion(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => EliminarRDF(x.prop).StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0)
            {
                itemBean.Items.Add(new CvnItemBeanCvnString()
                {
                    Code = code,
                    Value = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                        .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last().Split("_").Last()
                });
            }
        }

        public static void AddCvnItemBeanCvnFamilyNameBean(CvnItemBean itemBean, string section, List<string> property, string code, Entity entity)
        {
            if (property.Count != 2)
            {
                return;
            }
            CvnItemBeanCvnFamilyNameBean cvnFamilyNameBean = new CvnItemBeanCvnFamilyNameBean();
            cvnFamilyNameBean.Code = code;

            if (entity.properties.Where(x => EliminarRDF(x.prop).StartsWith(section)).Count() == 0)
            {
                return;
            }
            AddCvnItemBeanCvnFamilyNameBeanFirstFamilyName(cvnFamilyNameBean, property.ElementAt(0), entity);
            AddCvnItemBeanCvnFamilyNameBeanSecondFamilyName(cvnFamilyNameBean, property.ElementAt(1), entity);

            itemBean.Items.Add(cvnFamilyNameBean);
        }

        public static void AddCvnItemBeanCvnFamilyNameBeanFirstFamilyName(CvnItemBeanCvnFamilyNameBean familyNameBean, string prop, Entity entity)
        {
            if (entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(prop)).Count() == 0)
            {
                return;
            }
            familyNameBean.FirstFamilyName = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(prop))
                .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];
        }

        public static void AddCvnItemBeanCvnFamilyNameBeanSecondFamilyName(CvnItemBeanCvnFamilyNameBean familyNameBean, string prop, Entity entity)
        {
            if (entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(prop)).Count() == 0)
            {
                return;
            }
            familyNameBean.SecondFamilyName = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(prop))
                .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];
        }

        /// <summary>
        /// Inserta en <paramref name="entity"/> el con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// Debe estar Concatenado por "_", y se seleccionará el ultimo valor de la concatenación de "_"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCvnItemBeanNumericValue(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => EliminarRDF(x.prop).StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0)
            {
                itemBean.Items.Add(new CvnItemBeanCvnString()
                {
                    Code = code,
                    Value = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                        .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1].Split("_").Last()
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="value"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCvnItemBeanCvnRichText(CvnItemBean itemBean, string value, string code, [Optional] string secciones)
        {
            CvnItemBeanCvnRichText richText = new CvnItemBeanCvnRichText();
            richText.Code = code;
            richText.Value = value;

            itemBean.Items.Add(richText);
        }

        public static void AddCvnItemBeanCvnAuthorBean(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnAuthorBean authorBean = new CvnItemBeanCvnAuthorBean();
            authorBean.Code = code;

            itemBean.Items.Add(authorBean);
        }

        public static void AddCvnItemBeanCvnBoolean(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnBoolean cvnBoolean = new CvnItemBeanCvnBoolean();
            cvnBoolean.Code = code;

            itemBean.Items.Add(cvnBoolean);
        }

        public static void AddCvnItemBeanCvnCodeGroup(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnCodeGroup codeGroup = new CvnItemBeanCvnCodeGroup();
            codeGroup.Code = code;

            itemBean.Items.Add(codeGroup);
        }

        public static void AddCvnItemBeanCvnDouble(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnDouble cvnDouble = new CvnItemBeanCvnDouble();
            cvnDouble.Code = code;

            itemBean.Items.Add(cvnDouble);
        }

        public static void AddCvnItemBeanCvnDuration(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnDuration duration = new CvnItemBeanCvnDuration();
            duration.Code = code;

            itemBean.Items.Add(duration);
        }

        public static void AddCvnItemBeanCvnEntityBean(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnEntityBean entityBean = new CvnItemBeanCvnEntityBean();
            entityBean.Code = code;

            itemBean.Items.Add(entityBean);
        }

        public static void AddCvnItemBeanCvnPageBean(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnPageBean pageBean = new CvnItemBeanCvnPageBean();
            pageBean.Code = code;

            itemBean.Items.Add(pageBean);
        }

        public static void AddCvnItemBeanCvnTitleBean(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnTitleBean titleBean = new CvnItemBeanCvnTitleBean();
            titleBean.Code = code;

            itemBean.Items.Add(titleBean);
        }

        public static void AddCvnItemBeanCvnVolumeBean(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnVolumeBean volumeBean = new CvnItemBeanCvnVolumeBean();
            volumeBean.Code = code;

            itemBean.Items.Add(volumeBean);
        }

        public static void AddCvnItemBeanCvnPhoneBean(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            CvnItemBeanCvnPhoneBean phone = new CvnItemBeanCvnPhoneBean();
            phone.Code = code;

            phone.Extension = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property + "|http://w3id.org/roh/hasExtension"))) ?
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property + "|http://w3id.org/roh/hasExtension"))
                .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            phone.InternationalCode = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property + "|http://w3id.org/roh/hasInternationalCode"))) ?
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property + "|http://w3id.org/roh/hasInternationalCode"))
                    .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            phone.Number = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property + "|https://www.w3.org/2006/vcard/ns#hasValue"))) ?
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property + "|https://www.w3.org/2006/vcard/ns#hasValue"))
                .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;

            itemBean.Items.Add(phone);
        }

        /// <summary>
        /// Inserta en <paramref name="entity"/> el con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// Debe estar Concatenado por @@@, el valor del tipo debe encontrarse entre "/" y ";", los bytes de la imagen 
        /// deben estar despues de la primera ",".
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCvnItemBeanCvnPhotoBean(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => EliminarRDF(x.prop).StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0)
            {
                string datos = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                    .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];
                string tipoImagen = datos.Split(";")[0].Split("/")[1];
                string bytesImagen = datos.Split(";")[1].Split(",")[1];
                itemBean.Items.Add(new CvnItemBeanCvnPhotoBean()
                {
                    Code = code,
                    BytesInBase64 = bytesImagen,
                    MimeType = tipoImagen
                });
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entity"/> el con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// Debe tener formato de fecha GNOSS "yyyMMddHHmmSS" y estar concatenado por "@@@"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCvnItemBeanCvnDateDayMonthYear(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => EliminarRDF(x.prop).StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0)
            {
                string gnossDate = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                    .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];

                string anio = gnossDate.Substring(0, 4);
                string mes = gnossDate.Substring(4, 2);
                string dia = gnossDate.Substring(6, 2);
                string hora = gnossDate.Substring(8, 2);
                string minuto = gnossDate.Substring(10, 2);
                string segundos = gnossDate.Substring(12, 2);
                string fecha = anio + "-" + mes + "-" + dia + "T" + hora + ":" + minuto + ":" + segundos + "+01:00";

                DateTime fechaDateTime = DateTime.Parse(fecha);
                itemBean.Items.Add(new CvnItemBeanCvnDateDayMonthYear()
                {
                    Code = code,
                    Value = fechaDateTime
                });
            }
        }

        public static void AddCvnItemBeanCvnExternalPKBean(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => EliminarRDF(x.prop).StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0)
            {
                CvnItemBeanCvnExternalPKBean externalPKBean = new CvnItemBeanCvnExternalPKBean();
                externalPKBean.Code = code;

                if (property.Contains("http://w3id.org/roh/otherIds"))
                {
                    externalPKBean.Type = "OTHERS";
                    externalPKBean.Value = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                        .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last();
                    externalPKBean.Others = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith("http://purl.org/dc/elements/1.1/title"))
                        .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last();
                    itemBean.Items.Add(externalPKBean);
                    return;
                }

                //Añado el tipo si se corresponde con uno de los validos, sino salgo sin añadir
                switch (property)
                {
                    case "http://w3id.org/roh/ORCID":
                        externalPKBean.Type = "140";
                        break;
                    case "http://vivoweb.org/ontology/core#scopusId":
                        externalPKBean.Type = "150";
                        break;
                    case "http://vivoweb.org/ontology/core#researcherId":
                        externalPKBean.Type = "160";
                        break;
                    default:
                        return;
                }

                externalPKBean.Value = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                    .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last();

                itemBean.Items.Add(externalPKBean);
            }
        }

    }
}
