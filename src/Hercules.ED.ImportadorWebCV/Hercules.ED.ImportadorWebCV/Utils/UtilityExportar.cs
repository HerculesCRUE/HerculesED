﻿using Gnoss.ApiWrapper;
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
using System.Text;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ExportadorWebCV.Utils
{
    public class UtilityExportar
    {

        /// <summary>
        /// Devuelve una lista de tuplas con la persona, nombre, apellidos.
        /// </summary>
        /// <param name="pResourceApi"></param>
        /// <param name="dicPersonas">Key:BFO, Value:Person</param>
        /// <returns></returns>
        public static List<Tuple<string, string, string>> GetListadoAutores(ResourceApi pResourceApi, Dictionary<string, string> dicPersonas)
        {
            if (dicPersonas.Count() == 0)
            {
                return new List<Tuple<string, string, string>>();
            }
            string select = $@"select distinct *";
            string where = $@"where {{ 
    ?person a <http://xmlns.com/foaf/0.1/Person>
    OPTIONAL{{?person <http://xmlns.com/foaf/0.1/firstName> ?nombre}}
    OPTIONAL{{?person <http://xmlns.com/foaf/0.1/lastName> ?apellidos }}
    FILTER(
        ?person in (<{string.Join(">,<", dicPersonas.Keys)}>)
    ) 
}}";
            List<Tuple<string, string, string>> listaResultado = new List<Tuple<string, string, string>>();

            SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "person");
            if (resultData.results.bindings.Count == 0)
            {
                return null;
            }

            //Añado si como minimo tiene nombre
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (!fila.ContainsKey("person"))
                {
                    continue;
                }
                if (fila.ContainsKey("nombre") && fila.ContainsKey("apellidos"))
                {
                    listaResultado.Add(new Tuple<string, string, string>(dicPersonas[fila["person"].value], fila["nombre"].value, fila["apellidos"].value));
                }
                else if (fila.ContainsKey("nombre"))
                {
                    listaResultado.Add(new Tuple<string, string, string>("", fila["nombre"].value, ""));
                }
            }

            return listaResultado;
        }

        /// <summary>
        /// Devuelve una lista de tuplas con las entidades del CV.
        /// </summary>
        /// <param name="pResourceApi"></param>
        /// <param name="propiedadesItem"></param>
        /// <param name="pCVID"></param>
        /// <returns></returns>
        public static List<Tuple<string, string>> GetListadoEntidadesCV(ResourceApi pResourceApi, List<string> propiedadesItem, string pCVID)
        {
            //Compruebo que no es nulo y que tiene 1 o más valores
            if (propiedadesItem == null)
            {
                return null;
            }
            if (propiedadesItem.Count != 4)
            {
                return null;
            }

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

        /// <summary>
        /// Devuelve las entidades con propiedad/es <paramref name="propiedadesItem"/> del CV con valor <paramref name="pCVID"/>.
        /// </summary>
        /// <param name="pResourceApi"></param>
        /// <param name="propiedadesItem"></param>
        /// <param name="pCVID"></param>
        /// <returns></returns>
        public static List<string> GetListadoEntidades(ResourceApi pResourceApi, List<string> propiedadesItem, string pCVID)
        {
            //Compruebo que no es nulo y que tiene 1 o más valores
            if (propiedadesItem == null)
            {
                return null;
            }
            if (propiedadesItem.Count == 0)
            {
                return null;
            }

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

        /// <summary>
        /// Añade en cvnRootBean de <paramref name="cvn"/> los valores de <paramref name="listado"/>
        /// </summary>
        /// <param name="cvn"></param>
        /// <param name="listado"></param>
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

        /// <summary>
        /// Elimina las propiedades RDF intermedias de la cadena concatenando por "|" los valores restantes.
        /// </summary>
        /// <param name="cadena"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnItemBeanCvnString con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
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
                        .Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n")
                });
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnItemBeanCvnString con codigo <paramref name="code"/> y valor <paramref name="value"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="code"></param>
        /// <param name="value"></param>
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
                Value = value.Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n")
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

        /// <summary>
        /// Devuelve los hijos del listado de palabras clave, eliminando a los padres.
        /// </summary>
        /// <param name="listaPalabrasClave"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetHijosListadoPalabrasClave(List<string> listaPalabrasClave)
        {
            List<decimal> ld = new List<decimal>();

            Dictionary<string, string> codigos = new Dictionary<string, string>();
            foreach (string palabraClave in listaPalabrasClave)
            {
                string key = palabraClave.Split("@@@").First();
                decimal value = decimal.Parse(palabraClave.Split("_").Last());
                string valueString = palabraClave.Split("_").Last();

                ld.Add(value);
                if (codigos.ContainsKey(key))
                {
                    if (value.CompareTo(decimal.Parse(codigos[key])) <= 0)
                    {
                        continue;
                    }
                    codigos[key] = valueString;
                }
                else
                {
                    codigos.Add(key, valueString);
                }
            }
            return codigos;
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnItemBeanCvnString con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
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
                        .Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n")
                });
            }
        }

        /// <summary>
        /// Añade un listado de <paramref name="itemBean"/> un CvnItemBeanCvnString con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCvnItemBeanCvnStringList(CvnItemBean itemBean, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }
            List<string> listaStrings = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)))
                ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Select(x => x.values).FirstOrDefault()
                : null;

            if (listaStrings == null)
            {
                return;
            }

            foreach (string stringValue in listaStrings)
            {
                CvnItemBeanCvnString cvnString = new CvnItemBeanCvnString();
                cvnString.Code = code;
                cvnString.Value = stringValue.Split("_").Last().Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n");

                itemBean.Items.Add(cvnString);
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnItemBeanCvnString con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// dentro de entity.properties_cv
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCvnItemBeanCvnString_cv(CvnItemBean itemBean, string property, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            if (entity.properties_cv == null)
            {
                return;
            }

            if (entity.properties_cv.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0)
            {
                itemBean.Items.Add(new CvnItemBeanCvnString()
                {
                    Code = code,
                    Value = entity.properties_cv.Where(x => EliminarRDF(x.prop).EndsWith(property))
                        .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("_").Last()
                        .Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n")
                });
            }
        }

        /// <summary>
        /// Añade un objeto CvnItemBeanCvnString con formato de una direccion
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
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
                        .Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n")
                });
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnFamilyNameBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
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

        private static void AddCvnItemBeanCvnFamilyNameBeanFirstFamilyName(CvnItemBeanCvnFamilyNameBean familyNameBean, string prop, Entity entity)
        {
            if (entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(prop)).Count() == 0)
            {
                return;
            }
            familyNameBean.FirstFamilyName = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(prop))
                .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];
        }

        private static void AddCvnItemBeanCvnFamilyNameBeanSecondFamilyName(CvnItemBeanCvnFamilyNameBean familyNameBean, string prop, Entity entity)
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
                        .Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n")
                });
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnRichText con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="value"></param>
        /// <param name="code"></param>
        /// <param name="secciones"></param>
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

        /// <summary>
        /// Añade un CvnItemBeanCvnAuthorBeanCvnFamilyNameBean en <paramref name="itemBean"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="properties"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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

            if (!string.IsNullOrEmpty(authorBean.GivenName)
                || !string.IsNullOrEmpty(authorBean.Signature))
            {
                itemBean.Items.Add(authorBean);
            }
        }

        /// <summary>
        /// Devuelve un lista de tuplas con persona, nombre, apellidos.
        /// </summary>
        /// <param name="propiedad"></param>
        /// <param name="entity"></param>
        /// <param name="resourceApi"></param>
        /// <returns></returns>
        public static List<Tuple<string, string, string>> GetNombreApellidoAutor(string propiedad, Entity entity, ResourceApi resourceApi)
        {
            List<Tuple<string, string, string>> autorNombreApellido = new List<Tuple<string, string, string>>();
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(EliminarRDF(propiedad))))
                &&
                Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(EliminarRDF(propiedad))).Select(x => x.values).First())
            )
            {
                List<string[]> listadoPersonas = entity.properties.Where(x => EliminarRDF(x.prop).Equals(EliminarRDF(propiedad)))
                .Select(x => x.values).First().Select(x => x.Split("@@@")).ToList();
                Dictionary<string, string> dicPersonas = listadoPersonas.ToDictionary(x => x.ElementAt(1), x => x.ElementAt(0));

                autorNombreApellido = GetListadoAutores(resourceApi, dicPersonas);
            }

            return autorNombreApellido;
        }

        /// <summary>
        /// Devuelve un diccionario con el BFO y las firmas de los autores
        /// </summary>
        /// <param name="propiedad"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFirmasAutores(string propiedad, Entity entity)
        {
            Dictionary<string, string> dicFirmas = new Dictionary<string, string>();
            if (Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(EliminarRDF(propiedad))))
                    &&
                    Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(EliminarRDF(propiedad))).Select(x => x.values).First())
                )
            {
                List<string[]> listadoFirmas = entity.properties.Where(x => EliminarRDF(x.prop).Equals(EliminarRDF(propiedad)))
                    .Select(x => x.values).First().Select(x => x.Split("@@@")).ToList();

                dicFirmas = listadoFirmas.ToDictionary(x => x.ElementAt(0), x => x.ElementAt(1));
            }
            return dicFirmas;
        }

        /// <summary>
        /// Añade un CvnItemBeanCvnAuthorBeanCvnFamilyNameBean en <paramref name="itemBean"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="autorNombreApellido"></param>
        /// <param name="dicFirmas"></param>
        /// <param name="code"></param>
        /// <param name="secciones"></param>
        public static void AddCvnItemBeanCvnAuthorBeanListSimple(CvnItemBean itemBean, List<Tuple<string, string, string>> autorNombreApellido,
            Dictionary<string, string> dicFirmas, string code, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }
            foreach (Tuple<string, string, string> autores in autorNombreApellido)
            {
                CvnItemBeanCvnAuthorBean cvnAuthorBean = new CvnItemBeanCvnAuthorBean();
                cvnAuthorBean.Code = code;
                cvnAuthorBean.GivenName = autores.Item2;
                cvnAuthorBean.CvnFamilyNameBean = new CvnItemBeanCvnAuthorBeanCvnFamilyNameBean()
                {
                    Code = code,
                    FirstFamilyName = autores.Item3
                };
                if (dicFirmas.ContainsKey(autores.Item1))
                {
                    cvnAuthorBean.Signature = dicFirmas[autores.Item1];
                }
                itemBean.Items.Add(cvnAuthorBean);
            }
        }

        /// <summary>
        /// Añade un listado de autores, CvnItemBeanCvnAuthorBeanCvnFamilyNameBean, en <paramref name="itemBean"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="properties"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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

                if (!string.IsNullOrEmpty(authorBean.GivenName)
                || !string.IsNullOrEmpty(authorBean.Signature))
                {
                    itemBean.Items.Add(authorBean);
                }
            }
        }

        /// <summary>
        /// Añade en <paramref name="diccionario"/> los valores de Key:(identificador, nombrePropiedad) y Value:valor
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="diccionario"></param>
        /// <param name="nombrePropiedad"></param>
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnBoolean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnBoolean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// en entity.properties_cv
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
        public static void AddCvnItemBeanCvnBoolean_cv(CvnItemBean itemBean, string property, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            CvnItemBeanCvnBoolean cvnBoolean = new CvnItemBeanCvnBoolean();
            cvnBoolean.Code = code;

            //Añado si se inserta valor
            if (Comprobar(entity.properties_cv.Where(x => EliminarRDF(x.prop).EndsWith(property))))
            {
                cvnBoolean.Value = entity.properties_cv.Where(x => EliminarRDF(x.prop).EndsWith(property))
                        .Select(x => x.values).FirstOrDefault().FirstOrDefault().ToLower().Equals("true") ? true : false;

                itemBean.Items.Add(cvnBoolean);
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> los valores CodeGroup.
        /// Tipos de dato: "String", "Double","Boolean","EntityBean","TitleBean".
        /// </summary>
        /// <param name="itemBean">iteamBean</param>
        /// <param name="dicCodigos">Tupla de <TipoDato, Codigo, Propiedad> </param>
        /// <param name="code">Codigo del CodeGroup</param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
        public static void AddCvnItemBeanCvnCodeGroup(CvnItemBean itemBean, List<Tuple<string, string, string>> dicCodigos, string code, Entity entity,
            [Optional] string othersType, [Optional] string othersOthers, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            //Tipodato, Codigo, Propiedad, ID, Valor
            List<Tuple<string, string, string, string, string>> listadoTuplas = new List<Tuple<string, string, string, string, string>>();
            foreach (Tuple<string, string, string> tuple in dicCodigos)
            {
                if (!Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).Equals(tuple.Item3))))
                {
                    continue;
                }
                for (int i = 0; i < entity.properties.Where(x => EliminarRDF(x.prop).Equals(tuple.Item3)).Select(x => x.values).FirstOrDefault().Count(); i++)
                {
                    listadoTuplas.Add(new Tuple<string, string, string, string, string>(tuple.Item1, tuple.Item2, tuple.Item3,
                        entity.properties.Where(x => EliminarRDF(x.prop).Equals(tuple.Item3)).Select(x => x.values).FirstOrDefault().ElementAt(i).Split("@@@").FirstOrDefault(),
                        entity.properties.Where(x => EliminarRDF(x.prop).Equals(tuple.Item3)).Select(x => x.values).FirstOrDefault().ElementAt(i).Split("_").Last()
                        )
                    );
                }
            }

            //Si se envia opción de otros. Elimino la ocurrencia del tipo "OTHERS" y solo mantengo el valor especificado en el tipo otros.
            if (listadoTuplas.Any(x => x.Item3.Equals(othersOthers)))
            {
                List<string> idOthers = listadoTuplas.Where(x => x.Item3.Equals(othersOthers)).Select(x => x.Item4).ToList();
                foreach (string identificador in idOthers)
                {
                    listadoTuplas.RemoveAll(x => x.Item4.Equals(identificador) && x.Item3.Equals(othersType));
                }
            }

            List<IGrouping<string, Tuple<string, string, string, string, string>>> listado = listadoTuplas.GroupBy(x => x.Item4).ToList();
            for (int i = 0; i < listado.Count; i++)
            {
                //Inicializacion de valores
                CvnItemBeanCvnCodeGroup codeGroup = new CvnItemBeanCvnCodeGroup();
                codeGroup.Code = code;

                List<CvnItemBeanCvnCodeGroupCvnString> listadoStrings = new List<CvnItemBeanCvnCodeGroupCvnString>();
                List<CvnItemBeanCvnCodeGroupCvnDouble> listadoDouble = new List<CvnItemBeanCvnCodeGroupCvnDouble>();

                //Tipodato, Codigo, Propiedad, Valor
                var tupla = listado.ElementAt(i).Select(x => new Tuple<string, string, string, string>(x.Item1, x.Item2, x.Item3, x.Item5));
                for (int j = 0; j < tupla.Count(); j++)
                {
                    if (tupla.ElementAt(j).Item1.Equals("String"))
                    {
                        CvnItemBeanCvnCodeGroupCvnString cvnString = new CvnItemBeanCvnCodeGroupCvnString();
                        cvnString.Code = tupla.ElementAt(j).Item2;
                        cvnString.Value = tupla.ElementAt(j).Item4.Split("@@@").Last()
                            .Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n");

                        listadoStrings.Add(cvnString);
                        continue;
                    }
                    if (tupla.ElementAt(j).Item1.Equals("Double"))
                    {
                        CvnItemBeanCvnCodeGroupCvnDouble cvnDouble = new CvnItemBeanCvnCodeGroupCvnDouble();
                        cvnDouble.Code = tupla.ElementAt(j).Item2;
                        cvnDouble.Value = int.Parse(tupla.ElementAt(j).Item4.Split("@@@").Last());

                        listadoDouble.Add(cvnDouble);
                        continue;
                    }
                    if (tupla.ElementAt(j).Item1.Equals("Boolean"))
                    {
                        CvnItemBeanCvnCodeGroupCvnBoolean cvnBoolean = new CvnItemBeanCvnCodeGroupCvnBoolean();
                        cvnBoolean.Code = tupla.ElementAt(j).Item2;
                        cvnBoolean.Value = tupla.ElementAt(j).Item4.Split("@@@").Last().ToLower().Equals("true") ? true : false;


                        codeGroup.CvnBoolean = new CvnItemBeanCvnCodeGroupCvnBoolean();
                        codeGroup.CvnBoolean = cvnBoolean;
                        continue;
                    }
                    if (tupla.ElementAt(j).Item1.Equals("EntityBean"))
                    {
                        CvnItemBeanCvnCodeGroupCvnEntityBean cvnEntityBean = new CvnItemBeanCvnCodeGroupCvnEntityBean();
                        cvnEntityBean.Code = tupla.ElementAt(j).Item2;
                        cvnEntityBean.Name = tupla.ElementAt(j).Item4.Split("@@@").Last();


                        codeGroup.CvnEntityBean = new CvnItemBeanCvnCodeGroupCvnEntityBean();
                        codeGroup.CvnEntityBean = cvnEntityBean;
                        continue;
                    }
                    if (tupla.ElementAt(j).Item1.Equals("TitleBean"))
                    {
                        CvnItemBeanCvnCodeGroupCvnTitleBean cvnTitleBean = new CvnItemBeanCvnCodeGroupCvnTitleBean();
                        cvnTitleBean.Code = tupla.ElementAt(j).Item2;
                        cvnTitleBean.Name = tupla.ElementAt(j).Item4.Split("@@@").Last();


                        codeGroup.CvnTitleBean = new CvnItemBeanCvnCodeGroupCvnTitleBean();
                        codeGroup.CvnTitleBean = cvnTitleBean;
                        continue;
                    }
                }

                if (listadoStrings.Count > 0)
                {
                    codeGroup.CvnString = new CvnItemBeanCvnCodeGroupCvnString[10];
                    codeGroup.CvnString = listadoStrings.ToArray();
                }
                if (listadoDouble.Count > 0)
                {
                    codeGroup.CvnDouble = new CvnItemBeanCvnCodeGroupCvnDouble[10];
                    codeGroup.CvnDouble = listadoDouble.ToArray();
                }


                itemBean.Items.Add(codeGroup);
            }
        }

        /// <summary>
        /// Añade citas al objeto <paramref name="itemBean"/>.
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="dicCodigos">1º elemento Double con la propiedad
        /// 2º elemento String con "WOS/SCOPUS/INRECS/OTHERS"</param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        public static void AddCitas(CvnItemBean itemBean, List<Tuple<string, string, string>> dicCodigos, string code, Entity entity)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            // Si el tamaño del listado no es 2(WOS,SCOPUS,INRECS) o 3 (SCHOLAR) no hago nada
            if (dicCodigos.Count > 3 && dicCodigos.Count < 2)
            {
                return;
            }

            //Inicializacion de valores
            CvnItemBeanCvnCodeGroup codeGroup = new CvnItemBeanCvnCodeGroup();
            codeGroup.Code = code;

            List<CvnItemBeanCvnCodeGroupCvnString> listadoStrings = new List<CvnItemBeanCvnCodeGroupCvnString>();
            List<CvnItemBeanCvnCodeGroupCvnDouble> listadoDouble = new List<CvnItemBeanCvnCodeGroupCvnDouble>();

            if (dicCodigos.ElementAt(1).Item3.Equals("WOS") && Comprobar(entity.properties.Where(x => x.prop.Equals(dicCodigos.ElementAt(0).Item3))))
            {
                //Añado nº de citas
                CvnItemBeanCvnCodeGroupCvnDouble cvnDouble = new CvnItemBeanCvnCodeGroupCvnDouble();
                cvnDouble.Code = dicCodigos.ElementAt(0).Item2;
                cvnDouble.Value = int.Parse(entity.properties.Where(x => x.prop.Equals(dicCodigos.ElementAt(0).Item3)).Select(x => x.values).FirstOrDefault().FirstOrDefault());
                listadoDouble.Add(cvnDouble);

                //Añado Tipo
                CvnItemBeanCvnCodeGroupCvnString cvnString = new CvnItemBeanCvnCodeGroupCvnString();
                cvnString.Code = dicCodigos.ElementAt(1).Item2;
                cvnString.Value = "000";
                listadoStrings.Add(cvnString);
            }
            else if (dicCodigos.ElementAt(1).Item3.Equals("SCOPUS") && Comprobar(entity.properties.Where(x => x.prop.Equals(dicCodigos.ElementAt(0).Item3))))
            {
                //Añado nº de citas
                CvnItemBeanCvnCodeGroupCvnDouble cvnDouble = new CvnItemBeanCvnCodeGroupCvnDouble();
                cvnDouble.Code = dicCodigos.ElementAt(0).Item2;
                cvnDouble.Value = int.Parse(entity.properties.Where(x => x.prop.Equals(dicCodigos.ElementAt(0).Item3)).Select(x => x.values).FirstOrDefault().FirstOrDefault());
                listadoDouble.Add(cvnDouble);

                //Añado Tipo
                CvnItemBeanCvnCodeGroupCvnString cvnString = new CvnItemBeanCvnCodeGroupCvnString();
                cvnString.Code = dicCodigos.ElementAt(1).Item2;
                cvnString.Value = "010";

                listadoStrings.Add(cvnString);
            }
            else if (dicCodigos.ElementAt(1).Item3.Equals("INRECS") && Comprobar(entity.properties.Where(x => x.prop.Equals(dicCodigos.ElementAt(0).Item3))))
            {
                //Añado nº de citas
                CvnItemBeanCvnCodeGroupCvnDouble cvnDouble = new CvnItemBeanCvnCodeGroupCvnDouble();
                cvnDouble.Code = dicCodigos.ElementAt(0).Item2;
                cvnDouble.Value = int.Parse(entity.properties.Where(x => x.prop.Equals(dicCodigos.ElementAt(0).Item3)).Select(x => x.values).FirstOrDefault().FirstOrDefault());
                listadoDouble.Add(cvnDouble);

                //Añado Tipo
                CvnItemBeanCvnCodeGroupCvnString cvnString = new CvnItemBeanCvnCodeGroupCvnString();
                cvnString.Code = dicCodigos.ElementAt(1).Item2;
                cvnString.Value = "020";

                listadoStrings.Add(cvnString);
            }
            else if (dicCodigos.ElementAt(1).Item3.Equals("SCHOLAR") && Comprobar(entity.properties.Where(x => x.prop.Equals(dicCodigos.ElementAt(0).Item3))) && dicCodigos.Count == 3)
            {
                //Añado nº de citas
                CvnItemBeanCvnCodeGroupCvnDouble cvnDouble = new CvnItemBeanCvnCodeGroupCvnDouble();
                cvnDouble.Code = dicCodigos.ElementAt(0).Item2;
                cvnDouble.Value = int.Parse(entity.properties.Where(x => x.prop.Equals(dicCodigos.ElementAt(0).Item3)).Select(x => x.values).FirstOrDefault().FirstOrDefault());
                listadoDouble.Add(cvnDouble);

                //Añado Tipo
                CvnItemBeanCvnCodeGroupCvnString cvnString = new CvnItemBeanCvnCodeGroupCvnString();
                cvnString.Code = dicCodigos.ElementAt(1).Item2;
                codeGroup.CvnString.Append(cvnString);

                //Añado nombre otros
                CvnItemBeanCvnCodeGroupCvnString cvnStringOthers = new CvnItemBeanCvnCodeGroupCvnString();
                cvnStringOthers.Code = dicCodigos.ElementAt(2).Item2;
                cvnStringOthers.Value = "OTHERS";

                //Añado nombre otros
                CvnItemBeanCvnCodeGroupCvnString cvnStringOthersNombre = new CvnItemBeanCvnCodeGroupCvnString();
                cvnStringOthersNombre.Code = dicCodigos.ElementAt(3).Item2;
                cvnStringOthersNombre.Value = entity.properties.Where(x => x.prop.Equals(dicCodigos.ElementAt(0).Item3)).Select(x => x.values).FirstOrDefault().FirstOrDefault();

                listadoStrings.Add(cvnString);
            }


            if (listadoStrings.Count > 0)
            {
                codeGroup.CvnString = new CvnItemBeanCvnCodeGroupCvnString[10];
                codeGroup.CvnString = listadoStrings.ToArray();
            }
            if (listadoDouble.Count > 0)
            {
                codeGroup.CvnDouble = new CvnItemBeanCvnCodeGroupCvnDouble[10];
                codeGroup.CvnDouble = listadoDouble.ToArray();
            }

            if (codeGroup.CvnString != null && codeGroup.CvnString.Length != 0)
            {
                itemBean.Items.Add(codeGroup);
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnDouble con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="code"></param>
        /// <param name="value"></param>
        /// <param name="secciones"></param>
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnDuration con codigo <paramref name="code"/> si existe algun valor con propiedades
        /// "http://w3id.org/roh/durationYears", "http://w3id.org/roh/durationMonths" y "http://w3id.org/roh/durationDays"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnDuration con codigo <paramref name="code"/> si existe algun valor con propiedad "http://w3id.org/roh/durationHours"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnEntityBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="propertyName"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="propertyName"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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
                entityBean.Name = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyName))
                    .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last();

                itemBean.Items.Add(entityBean);
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un listado de CvnEntityBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="propertyName"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="propertyName"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
        public static void AddCvnItemBeanCvnEntityBeanList(CvnItemBean itemBean, string propertyName, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }
            List<string> listaEntityBeanNombres = Comprobar(entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyName)))
                ? entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(propertyName)).Select(x => x.values).FirstOrDefault()
                : null;

            if (listaEntityBeanNombres == null)
            {
                return;
            }

            foreach (string entityBeanNombre in listaEntityBeanNombres)
            {
                CvnItemBeanCvnEntityBean entityBean = new CvnItemBeanCvnEntityBean();
                entityBean.Code = code;
                entityBean.Name = entityBeanNombre.Split("@@@").Last();

                itemBean.Items.Add(entityBean);
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnPageBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="propPagIniPagFin"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="propPagIniPagFin"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnTitleBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="propertyIdentification"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="propertyIdentification"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
        public static void AddLanguage(CvnItemBean itemBean, string propertyIdentification, string code, Entity entity, [Optional] string secciones)
        {
            //Compruebo si el codigo pasado está bien formado
            if (Utility.CodigoIncorrecto(code))
            {
                return;
            }

            if (Comprobar(entity.properties.Where(x => x.prop.Equals(propertyIdentification))))
            {
                string IdNombre = entity.properties.Where(x => x.prop.Equals(propertyIdentification)).First().values.First().Split("_").Last();
                if (!UtilitySecciones.Lenguajes.Where(x => x.Item2.Equals(IdNombre)).Any())
                {
                    return;
                }
                string nombre = UtilitySecciones.Lenguajes.Where(x => x.Item2.Equals(IdNombre)).Select(x => x.Item1).FirstOrDefault();

                CvnItemBeanCvnTitleBean titleBean = new CvnItemBeanCvnTitleBean();
                titleBean.Code = code;
                titleBean.Name = nombre;
                titleBean.Identification = IdNombre;

                //Añado si se inserta valor
                if (!string.IsNullOrEmpty(titleBean.Name) || !string.IsNullOrEmpty(titleBean.Identification))
                {
                    itemBean.Items.Add(titleBean);
                }
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnTitleBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="propertyName"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="propertyName"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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
            if (!string.IsNullOrEmpty(titleBean.Name))
            {
                itemBean.Items.Add(titleBean);
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnTitleBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="propertyName"/>.
        /// Con Identification: <paramref name="propertyIdentification"/> y Name: <paramref name="propertyName"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="propertyIdentification"></param>
        /// <param name="propertyName"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnvolumeBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="propVolNum"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="propVolNum"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        /// <param name="secciones"></param>
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
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Count() > 0 &&
                entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property)).Select(x => x.values).Where(x => x.Count() == 1).Any())
            {
                string gnossDate = entity.properties.Where(x => EliminarRDF(x.prop).EndsWith(property))
                    .Select(x => x.values).Where(x => x.Count() == 1).FirstOrDefault().FirstOrDefault().Split("@@@").LastOrDefault();

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

        /// <summary>
        /// Inserta en <paramref name="entity"/> el DateDayMonthYear con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// Debe tener formato de fecha GNOSS "yyyMMddHHmmSS"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnExternalPKBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="propertyList"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="propertyList"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
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
                externalPKBean.Others = !string.IsNullOrEmpty(keyValue.Value.Item1) ? keyValue.Value.Item1 : null;
                externalPKBean.Value = !string.IsNullOrEmpty(keyValue.Value.Item2) ? keyValue.Value.Item2 : null;

                itemBean.Items.Add(externalPKBean);
            }
        }

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnExternalPKBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
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
                case "http://w3id.org/roh/projectCode":
                    externalPKBean.Type = "000";
                    break;
                case "http://purl.org/ontology/bibo/issn":
                    externalPKBean.Type = "010";
                    break;
                case "http://w3id.org/roh/isbn":
                    externalPKBean.Type = "020";
                    break;
                case "http://w3id.org/roh/legalDeposit":
                    externalPKBean.Type = "030";
                    break;
                case "http://purl.org/ontology/bibo/doi":
                    externalPKBean.Type = "040";
                    break;
                case "http://w3id.org/roh/applicationNumber":
                    externalPKBean.Type = "060";
                    break;
                case "http://w3id.org/roh/referenceCode":
                case "http://w3id.org/roh/patentNumber":
                    externalPKBean.Type = "070";
                    break;
                case "http://w3id.org/roh/normalizedCode":
                    externalPKBean.Type = "110";
                    break;
                case "http://purl.org/ontology/bibo/handle":
                    externalPKBean.Type = "120";
                    break;
                case "http://purl.org/ontology/bibo/pmid":
                    externalPKBean.Type = "130";
                    break;
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

        /// <summary>
        /// Añade en <paramref name="itemBean"/> un CvnExternalPKBean con codigo <paramref name="code"/> si existe algun valor con propiedad <paramref name="property"/>
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
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
