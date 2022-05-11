using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using ImportadorWebCV;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ExportadorWebCV.Utils
{
    public class UtilityExportar
    {
        public static List<Tuple<string,string>> GetListadoEntidadesCV(ResourceApi pResourceApi, List<string> propiedadesItem, string pCVID) 
        {
            //Compruebo que no es nulo y que tiene 1 o más valores
            if (propiedadesItem == null) { return null; }
            if (propiedadesItem.Count != 4) { return null; }

            string select = $@"select distinct *";
            string where = $@"where {{?cv <{propiedadesItem[0]}> ?prop1 . 
        ?prop1 <{propiedadesItem[1]}> ?prop2 .
        OPTIONAL{{?prop2 <{propiedadesItem[2]}> ?itemCV }}
        ?prop2 <{propiedadesItem[3]}> ?item .
    FILTER(?cv=<{pCVID}>) 
}}";


            List<Tuple<string, string>> listaResultado = new List<Tuple<string, string>>();

            SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            if (resultData.results.bindings.Count == 0)
            {
                return null;
            }
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (fila.ContainsKey("itemCV"))
                {
                    listaResultado.Add(new Tuple<string, string>(fila["item"].value, fila["itemCV"].value));
                }
                else
                {
                    listaResultado.Add(new Tuple<string, string>(fila["item"].value, ""));
                }
            }

            return listaResultado;
        }
        public static List<string> GetListadoEntidades(ResourceApi pResourceApi, List<string> propiedadesItem, string pCVID)
        {
            //Compruebo que no es nulo y que tiene 1 o más valores
            if (propiedadesItem == null) { return null; }
            if (propiedadesItem.Count == 0) { return null; }

            string select = $@"select distinct *";
            string where = "where {";

            if (propiedadesItem.Count == 1)
            {
                where += $@" ?cv <{propiedadesItem[0]}> ?item ";
            }
            if (propiedadesItem.Count > 1)
            {
                where += $@" ?cv <{propiedadesItem[0]}> ?prop1 . ";
                for (int i = 1; i < propiedadesItem.Count - 1; i++)
                {
                    where += $@" ?prop{i} <{propiedadesItem[i]}> ?prop{i + 1} . ";
                }
                where += $@" ?prop{propiedadesItem.Count - 1} <{propiedadesItem[propiedadesItem.Count - 1]}> ?item ";
            }
            where += $"FILTER(?cv=<{pCVID}>) }}";


            List<string> listaResultado = new List<string>();

            SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            if (resultData.results.bindings.Count == 0)
            {
                return null;
            }
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                listaResultado.Add(fila["item"].value);
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

        /// <summary>
        /// True si la enumeracion contiene algun elemento
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeracion"></param>
        /// <returns></returns>
        public static bool Comprobar<T>(IEnumerable<T> enumeracion)
        {
            return enumeracion.Any();
        }

        public static void AddCvnItemBeanCvnString(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

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

        public static void AddCvnItemBeanCvnStringSimple(CvnItemBean itemBean, string code, string value)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            itemBean.Items.Add(new CvnItemBeanCvnString()
            {
                Code = code,
                Value = value
            });
        }

        /// <summary>
        /// Inserta las palabras clave de propiedad <paramref name="property"/> en <paramref name="itemBean"/>.
        /// La palabra clave se seleccionará del ultimo valor al separar por "_"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCvnItemBeanCvnKeyword(CvnItemBean itemBean, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            List<string> listaPalabrasClave = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(property))) ?
                    entity.properties.Where(x => EliminarRDF(x.prop).Equals(property)).Select(x => x.values).FirstOrDefault()
                    : null;
            if (listaPalabrasClave == null)
            {
                return;
            }

            Dictionary<string, string> codigos = GetHijosListadoPalabrasClave(listaPalabrasClave);

            foreach (KeyValuePair<string, string> keyValue in codigos)
            {
                AddCvnItemBeanCvnStringSimple(itemBean, code, keyValue.Value);
            }
        }

        public static Dictionary<string, string> GetHijosListadoPalabrasClave(List<string> listaPalabrasClave)
        {
            Dictionary<string, string> codigos = new Dictionary<string, string>();

            foreach (string palabraClave in listaPalabrasClave)
            {
                string key = palabraClave.Split("@@@").First();
                string value = palabraClave.Split("_").Last();

                if (codigos.ContainsKey(key))
                {
                    string mayor;
                    if (double.Parse(value) > double.Parse(codigos[key]))
                    {
                        mayor = value;
                    }
                    else
                    {
                        mayor = codigos[key];
                    }
                    codigos[key] = mayor;
                }
                else
                {
                    codigos.Add(key, value);
                }
            }
            return codigos;
        }

        public static void AddCvnItemBeanCvnString(CvnItemBean itemBean, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            if (entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0)
            {
                itemBean.Items.Add(new CvnItemBeanCvnString()
                {
                    Code = code,
                    Value = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                        .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last()
                });
            }
        }

        public static void AddDireccion(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

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
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

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
        /// Inserta en <paramref name="entity"/> con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// Debe estar Concatenado por "_", y se seleccionará el ultimo valor de la concatenación de "_"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCvnItemBeanNumericValue(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

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
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnRichText richText = new CvnItemBeanCvnRichText();
            richText.Code = code;
            richText.Value = value;

            itemBean.Items.Add(richText);
        }

        public static void AddCvnItemBeanCvnAuthorBean(CvnItemBean itemBean, Dictionary<string, string> properties, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnAuthorBean authorBean = new CvnItemBeanCvnAuthorBean();
            authorBean.Code = code;

            CvnItemBeanCvnAuthorBeanCvnFamilyNameBean familyNameBean = new CvnItemBeanCvnAuthorBeanCvnFamilyNameBean();
            familyNameBean.Code = code;

            familyNameBean.FirstFamilyName = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["PrimerApellido"])))
                ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["PrimerApellido"])).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last()
                : null;
            familyNameBean.SecondFamilyName = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["SegundoApellido"])))
                ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["SegundoApellido"])).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last()
                : null;
            if (!string.IsNullOrEmpty(familyNameBean.FirstFamilyName) && !string.IsNullOrEmpty(familyNameBean.SecondFamilyName))
            {
                authorBean.CvnFamilyNameBean = familyNameBean;
            }
            authorBean.GivenName = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["Nombre"])))
                ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["Nombre"])).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last()
                : null;
            authorBean.Signature = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["Firma"])))
                ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["Firma"])).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last()
                : null;

            itemBean.Items.Add(authorBean);
        }

        public static void AddCvnItemBeanCvnAuthorBeanList(CvnItemBean itemBean, Dictionary<string, string> properties, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            Dictionary<Tuple<string, string>, string> diccionarioCodirectores = new Dictionary<Tuple<string, string>, string>();

            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["PrimerApellido"]))))
            {
                List<string> listaPrimerApellido = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["PrimerApellido"])).Select(x => x.values).FirstOrDefault();
                ListarCodirectores(listaPrimerApellido, diccionarioCodirectores, "PrimerApellido");
            }
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["SegundoApellido"]))))
            {
                List<string> listaSegundoApellido = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["SegundoApellido"])).Select(x => x.values).FirstOrDefault();
                ListarCodirectores(listaSegundoApellido, diccionarioCodirectores, "SegundoApellido");
            }
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["Nombre"]))))
            {
                List<string> listaNombre = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["Nombre"])).Select(x => x.values).FirstOrDefault();
                ListarCodirectores(listaNombre, diccionarioCodirectores, "Nombre");
            }
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["Firma"]))))
            {
                List<string> listaFirma = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(properties["Firma"])).Select(x => x.values).FirstOrDefault();
                ListarCodirectores(listaFirma, diccionarioCodirectores, "Firma");
            }

            List<IGrouping<string, KeyValuePair<Tuple<string, string>, string>>> listadoCodirectores = diccionarioCodirectores
                .GroupBy(x => x.Key.Item1).ToList();
            for (int i = 0; i < listadoCodirectores.Count; i++)
            {
                var keyValuePair = listadoCodirectores.ElementAt(i).Select(x => new KeyValuePair<Tuple<string, string>, string>(x.Key, x.Value));

                CvnItemBeanCvnAuthorBean authorBean = new CvnItemBeanCvnAuthorBean();
                authorBean.Code = code;

                CvnItemBeanCvnAuthorBeanCvnFamilyNameBean familyNameBean = new CvnItemBeanCvnAuthorBeanCvnFamilyNameBean();
                familyNameBean.Code = code;

                if (keyValuePair.Any())
                {
                    familyNameBean.FirstFamilyName = keyValuePair.Where(x => x.Key.Item2.Equals("PrimerApellido")).Select(x => x.Value).FirstOrDefault();
                    familyNameBean.SecondFamilyName = keyValuePair.Where(x => x.Key.Item2.Equals("SegundoApellido")).Select(x => x.Value).FirstOrDefault();
                    authorBean.GivenName = keyValuePair.Where(x => x.Key.Item2.Equals("Nombre")).Select(x => x.Value).FirstOrDefault();
                    authorBean.Signature = keyValuePair.Where(x => x.Key.Item2.Equals("Firma")).Select(x => x.Value).FirstOrDefault();
                }
                //Si no hay valores no añado
                if (!string.IsNullOrEmpty(familyNameBean.FirstFamilyName) && !string.IsNullOrEmpty(familyNameBean.SecondFamilyName))
                {
                    authorBean.CvnFamilyNameBean = familyNameBean;
                }

                itemBean.Items.Add(authorBean);
            }
        }


        private static void ListarCodirectores(List<string> listado, Dictionary<Tuple<string, string>, string> diccionario, string nombrePropiedad)
        {
            foreach (string dato in listado)
            {
                string identificador = dato.Split("@@@").First().Split("_").Last();
                string valor = dato.Split("@@@").Last();
                if (!string.IsNullOrEmpty(identificador) && !string.IsNullOrEmpty(valor))
                {
                    diccionario.TryAdd(new Tuple<string, string>(identificador, nombrePropiedad), valor);
                }
            }
        }

        public static void AddCvnItemBeanCvnBoolean(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnBoolean cvnBoolean = new CvnItemBeanCvnBoolean();
            cvnBoolean.Code = code;

            //Añado si se inserta valor
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))))
            {
                cvnBoolean.Value = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                        .Select(x => x.values).FirstOrDefault().FirstOrDefault().ToLower().Equals("true") ? true : false;

                itemBean.Items.Add(cvnBoolean);
            }
        }

        public static void AddCvnItemBeanCvnCodeGroup(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnCodeGroup codeGroup = new CvnItemBeanCvnCodeGroup();
            codeGroup.Code = code;

            itemBean.Items.Add(codeGroup);
        }

        public static void AddCvnItemBeanCvnDouble(CvnItemBean itemBean, string code, string value, [Optional] string secciones)
        {
            CvnItemBeanCvnDouble cvnDouble = new CvnItemBeanCvnDouble();
            cvnDouble.Code = code;
            if (value.Contains("."))
            {
                value = value.Replace(".", ",");
            }
            cvnDouble.Value = Convert.ToDecimal(value);

            itemBean.Items.Add(cvnDouble);
        }

        public static void AddCvnItemBeanCvnDuration(CvnItemBean itemBean, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnDuration duration = new CvnItemBeanCvnDuration();
            duration.Code = code;
            string duracion = "P";
            if (entity.properties.Where(x => EliminarRDF(x.prop).Contains("http://w3id.org/roh/duration")).Any())
            {
                string anio = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith("http://w3id.org/roh/durationYears")).Count() != 0 ?
                    entity.properties.Where(x => EliminarRDF(x.prop).EndsWith("http://w3id.org/roh/durationYears")).Select(x => x.values)?.FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(anio))
                {
                    duracion += anio + "Y";
                }

                string mes = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith("http://w3id.org/roh/durationMonths")).Count() != 0
                    ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith("http://w3id.org/roh/durationMonths")).Select(x => x.values)?.FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(mes))
                {
                    duracion += mes + "M";
                }

                string dia = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith("http://w3id.org/roh/durationDays")).Count() != 0 ?
                    entity.properties.Where(x => EliminarRDF(x.prop).EndsWith("http://w3id.org/roh/durationDays")).Select(x => x.values)?.FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(dia))
                {
                    duracion += dia + "D";
                }
            }

            //si no hay datos insertados pongo la cadena a vacia
            if (duracion.Equals("P"))
            {
                duracion = "";
            }
            duration.Value = !string.IsNullOrEmpty(duracion) ? duracion : null;

            //Añado si no es nulo
            if (duration.Value != null)
            {
                itemBean.Items.Add(duration);
            }
        }

        public static void AddCvnItemBeanCvnDurationHours(CvnItemBean itemBean, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnDuration duration = new CvnItemBeanCvnDuration();
            duration.Code = code;
            string duracion = "PT";

            string horas = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith("http://w3id.org/roh/durationHours")).Count() != 0 ?
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith("http://w3id.org/roh/durationHours")).Select(x => x.values)?.FirstOrDefault().FirstOrDefault()
                : null;
            if (!string.IsNullOrEmpty(horas))
            {
                duracion += horas + "H";
            }


            //si no hay datos insertados pongo la cadena a vacia
            if (duracion.Equals("PT"))
            {
                duracion = "";
            }
            duration.Value = !string.IsNullOrEmpty(duracion) ? duracion : null;

            //Añado si no es nulo
            if (duration.Value != null)
            {
                itemBean.Items.Add(duration);
            }
        }

        public static void AddCvnItemBeanCvnEntityBean(CvnItemBean itemBean, string propertyName, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnEntityBean entityBean = new CvnItemBeanCvnEntityBean();
            entityBean.Code = code;

            //Añado si se inserta valor
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyName))))
            {
                entityBean.Name = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyName)).Select(x => x.values).FirstOrDefault().FirstOrDefault();

                itemBean.Items.Add(entityBean);
            }
        }

        public static void AddCvnItemBeanCvnPageBean(CvnItemBean itemBean, Dictionary<string, string> propPagIniPagFin, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnPageBean pageBean = new CvnItemBeanCvnPageBean();
            pageBean.Code = code;
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(propPagIniPagFin["PaginaInicial"]))))
            {
                pageBean.InitialPage = entity.properties.Where(x => EliminarRDF(x.prop).Equals(propPagIniPagFin["PaginaInicial"]))
                            .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last();
            }
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(propPagIniPagFin["PaginaFinal"]))))
            {
                pageBean.FinalPage = entity.properties.Where(x => EliminarRDF(x.prop).Equals(propPagIniPagFin["PaginaFinal"]))
                            .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last();
            }

            if (!string.IsNullOrEmpty(pageBean.InitialPage) || !string.IsNullOrEmpty(pageBean.FinalPage))
            {
                itemBean.Items.Add(pageBean);
            }
        }

        public static void AddLanguage(CvnItemBean itemBean, string propertyIdentification, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            if (Comprobar(entity.properties.Where(x => x.prop.Equals("http://w3id.org/roh/languageOfTheCertificate"))))
            {
                CultureInfo cultureInfo = new CultureInfo(entity.properties.Where(x => x.prop.Equals("http://w3id.org/roh/languageOfTheCertificate")).First()
                    .values.First().Split("_").Last());

                CvnItemBeanCvnTitleBean titleBean = new CvnItemBeanCvnTitleBean();
                titleBean.Code = code;
                titleBean.Name = cultureInfo.EnglishName;
                titleBean.Identification = cultureInfo.Name;
            }
        }

        public static void AddCvnItemBeanCvnTitleBean(CvnItemBean itemBean, string propertyName, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnTitleBean titleBean = new CvnItemBeanCvnTitleBean();
            titleBean.Code = code;
            titleBean.Name = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyName)))
                ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyName)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                : null;


            //Añado si se inserta valor
            if (!string.IsNullOrEmpty(titleBean.Name) || !string.IsNullOrEmpty(titleBean.Identification))
            {
                itemBean.Items.Add(titleBean);
            }
        }

        public static void AddCvnItemBeanCvnTitleBean(CvnItemBean itemBean, string propertyIdentification, string propertyName, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnTitleBean titleBean = new CvnItemBeanCvnTitleBean();
            titleBean.Code = code;
            titleBean.Identification = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyIdentification)))
                ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyIdentification)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last()
                : null;
            titleBean.Name = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyName)))
                ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyName)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                : null;


            //Añado si se inserta valor
            if (!string.IsNullOrEmpty(titleBean.Name) || !string.IsNullOrEmpty(titleBean.Identification))
            {
                itemBean.Items.Add(titleBean);
            }
        }

        public static void AddCvnItemBeanCvnVolumeBean(CvnItemBean itemBean, Dictionary<string, string> propVolNum, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnVolumeBean volumeBean = new CvnItemBeanCvnVolumeBean();
            volumeBean.Code = code;
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(propVolNum["Numero"]))))
            {
                volumeBean.Number = entity.properties.Where(x => EliminarRDF(x.prop).Equals(propVolNum["Numero"]))
                            .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last();
            }
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(propVolNum["Volumen"]))))
            {
                volumeBean.Volume = entity.properties.Where(x => EliminarRDF(x.prop).Equals(propVolNum["Volumen"]))
                            .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last();
            }

            if (!string.IsNullOrEmpty(volumeBean.Number) || !string.IsNullOrEmpty(volumeBean.Volume))
            {
                itemBean.Items.Add(volumeBean);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entity"/> el PhoneBean con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// con las propiedades:
        ///  "http://w3id.org/roh/hasExtension" -> Extension,
        ///  "http://w3id.org/roh/hasInternationalCode" -> InternationalCode,
        ///  "https://www.w3.org/2006/vcard/ns#hasValue" -> Number.
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
        public static void AddCvnItemBeanCvnPhoneBean(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

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

            if (!string.IsNullOrEmpty(phone.Extension) || !string.IsNullOrEmpty(phone.InternationalCode)
                || !string.IsNullOrEmpty(phone.Number))
            {
                itemBean.Items.Add(phone);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entity"/> el PhotoBean con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
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
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

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
        /// Inserta en <paramref name="entity"/> el DateDayMonthYear con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// Debe tener formato de fecha GNOSS "yyyMMddHHmmSS" y estar concatenado por "@@@"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCvnItemBeanCvnDateDayMonthYear(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

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

        public static void AddCvnItemBeanCvnDateDayMonthYear(CvnItemBean itemBean, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            if (entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0)
            {
                string gnossDate = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                    .Select(x => x.values).FirstOrDefault().FirstOrDefault();

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

        public static void AddCvnItemBeanCvnExternalPKBeanOthers(CvnItemBean itemBean, Dictionary<string, string> propertyList, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            List<string> listaNombres = new List<string>();
            List<string> listaIdentificadores = new List<string>();

            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyList["Nombre"]))))
            {
                listaNombres = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyList["Nombre"])).Select(x => x.values).FirstOrDefault();
            }

            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyList["ID"]))))
            {
                listaIdentificadores = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyList["ID"])).Select(x => x.values).FirstOrDefault();
            }

            Dictionary<string, Tuple<string, string>> diccionario = new Dictionary<string, Tuple<string, string>>();
            foreach (string nombre in listaNombres)
            {
                string id = nombre.Split("@@@").First();
                string value = nombre.Split("@@@").Last();
                diccionario.Add(id, new Tuple<string, string>(value, ""));
            }
            foreach (string identificador in listaIdentificadores)
            {
                string id = identificador.Split("@@@").First();
                string value = identificador.Split("@@@").Last();
                if (diccionario.ContainsKey(identificador.Split("@@@").First()))
                {
                    diccionario[id] = new Tuple<string, string>(diccionario[id].Item1, value);
                }
            }

            foreach (KeyValuePair<string, Tuple<string, string>> keyValue in diccionario)
            {
                CvnItemBeanCvnExternalPKBean externalPKBean = new CvnItemBeanCvnExternalPKBean();
                externalPKBean.Code = code;
                externalPKBean.Type = "OTHERS";
                externalPKBean.Value = !string.IsNullOrEmpty(keyValue.Value.Item1) ? keyValue.Value.Item1 : null;
                externalPKBean.Others = !string.IsNullOrEmpty(keyValue.Value.Item2) ? keyValue.Value.Item2 : null;

                itemBean.Items.Add(externalPKBean);
            }
        }

        public static void AddCvnItemBeanCvnExternalPKBean(CvnItemBean itemBean, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            if (entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)) == null
                || entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() == 0)
            {
                return;
            }

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
                case "http://w3id.org/roh/legalDeposit":
                    externalPKBean.Type = "030";
                    break;
                case "http://w3id.org/roh/isbn":
                    externalPKBean.Type = "020";
                    break;
                case "http://purl.org/ontology/bibo/issn":
                    externalPKBean.Type = "010";
                    break;
                case "http://purl.org/ontology/bibo/doi":
                    externalPKBean.Type = "040";
                    break;
                case "http://purl.org/ontology/bibo/handle":
                    externalPKBean.Type = "120";
                    break;
                case "http://purl.org/ontology/bibo/pmid":
                    externalPKBean.Type = "130";
                    break;
                default:
                    return;
            }

            externalPKBean.Value = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last();

            itemBean.Items.Add(externalPKBean);

        }

        public static void AddCvnItemBeanCvnExternalPKBean(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

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
