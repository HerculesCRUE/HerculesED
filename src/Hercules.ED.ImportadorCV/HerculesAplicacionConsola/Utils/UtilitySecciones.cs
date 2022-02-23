using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Models.Entity;

namespace HerculesAplicacionConsola.Utils
{
    public class UtilitySecciones
    {
        public static HashSet<string> GetIDS(ResourceApi pResourceApi, string pCVID, List<string> propiedadesItem)
        {
            HashSet<string> ids = new HashSet<string>();
            int offsetInt = 0;
            int limit = 10000;
            string select = $@"SELECT distinct ?item {{ select *";
            string where = $@"where {{";

            //Compruebo que no es nulo y que tiene 1 o más valores
            if (propiedadesItem == null) { return null; }
            if (propiedadesItem.Count == 0) { return null; }

            if (propiedadesItem.Count == 1)
            {
                where += $@" ?s <{propiedadesItem[0]}> ?item ";
            }
            if (propiedadesItem.Count > 1)
            {
                where += $@" ?s <{propiedadesItem[0]}> ?prop1 . ";
                for (int i = 1; i < propiedadesItem.Count - 1; i++)
                {
                    where += $@" ?prop{i} <{propiedadesItem[i]}> ?prop{i + 1} . ";
                }
                where += $@" ?prop{propiedadesItem.Count - 1} <{propiedadesItem[propiedadesItem.Count - 1]}> ?item ";
            }


            //Si tengo más de 10.000 resultados repito la consulta, sino salgo del bucle
            while (true)
            {
                //where += $@"where {{?s <{propiedadesItem[0]}> ?activity .
                //                        ?activity <{propiedadesItem[1]}> ?rom .
                //                        ?rom <{propiedadesItem[2]}> ?item .
                //                        FILTER(?s = <{pCVID}>)
                //                    }} ORDER BY ?item
                //                }} LIMIT {limit} OFFSET {offsetInt}";
                where += $@" FILTER(?s = <{pCVID}>)
                            }} ORDER BY ?item
                        }}LIMIT {limit} OFFSET {offsetInt} ";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                ids.UnionWith(resultData.results.bindings.Select(x => x["item"].value));
                if (resultData.results.bindings.Count < limit)
                {
                    break;
                }
                else
                {
                    offsetInt += 10000;
                }
            }

            return ids;
        }
        public static string GetNombreRevista(ResourceApi pResourceApi, string nombreRevista)
        {
            string select = $@"SELECT distinct ?identificador";
            string where = "where {?identificador <http://w3id.org/roh/title> \"" + nombreRevista + "\"}";

            SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "maindocument");
            if (resultData.results.bindings.Count == 0)
            {
                return null;
            }
            else
            {
                return resultData.results.bindings.Select(x => x["identificador"].value).FirstOrDefault();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pResourceApi"></param>
        /// <param name="busqueda"></param>
        /// <returns></returns>
        public static List<string> GetBusquedaAutor(ResourceApi pResourceApi, string busqueda)
        {
            string select = $@"select distinct ?busqueda";
            string where = $@"where {{
                               graph ?g{{
                                    ?s <http://w3id.org/roh/publicAuthorList> ?listaAuthor . 
                                    ?listaAuthor <http://gnoss/search> ?busqueda . 
                                    filter contains(?busqueda, ""{busqueda}"")
                                }} 
                             }}";

            /*
             select distinct ?nombre ?listaAuthor
             where {{ graph ?g{{
                    ?s<http://w3id.org/roh/publicAuthorList> ?listaAuthor . 
                    ?listaAuthor <http://xmlns.com/foaf/0.1/firstName> ?nombre .
                    ?listaAuthor <http://xmlns.com/foaf/0.1/lastName> ?apellidos .
                    filter contains(lcase(?nombre), ""antonio"") .
                    filter contains(lcase(?apellidos), ""fernando skarmeta gomez"") .
                             }}
                   }}
             */
            CvnItemBeanCvnAuthorBean autor = new CvnItemBeanCvnAuthorBean();
            autor.CvnFamilyNameBean.FirstFamilyName = "";
            autor.CvnFamilyNameBean.SecondFamilyName = "";
            autor.GivenName = "";

            SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "document");
            if (resultData.results.bindings.Count == 0)
            {
                return null;
            }
            else
            {
                List<string> listaBusqueda = new List<string>();

                listaBusqueda.AddRange(resultData.results.bindings.Select(x => x["busqueda"].value));

                return listaBusqueda;
                //return resultData.results.bindings.Select(x => x["busqueda"].value).FirstOrDefault();
            }
        }

        public static string GetOrganizacionPorNombre(ResourceApi pResourceApi, string nombreOrganizacion)
        {
            string select = $@"select distinct ?nombre ?organizacion";
            string where = $@"where {{ 
                                ?organizacion <http://w3id.org/roh/title> ?nombre  
                                FILTER CONTAINS (?nombre, ""{nombreOrganizacion}"")
                            }}";

            SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "organization");
            if (resultData.results.bindings.Count == 0)
            {
                return null;
            }

            return resultData.results.bindings.Select(x => x["organizacion"].value).FirstOrDefault();
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

        /// <summary>
        /// Añade en la lista de propiedades de la entidad las propiedades en las 
        /// que los valores no son nulos, en caso de que los valores sean nulos se omite
        /// dicha propiedad.
        /// </summary>
        /// <param name="list"></param>
        public static List<Property> AddProperty(params Property[] list)
        {
            List<Property> listado = new List<Property>();
            for (int i = 0; i < list.Length; i++)
            {
                if (!string.IsNullOrEmpty(list[i].values[0]))
                {
                    listado.Add(list[i]);
                }
            }
            return listado;
        }

        public static void CheckProperty(Property property, Entity entidadAux, string valorXML, string propiedadXML)
        {
            if (valorXML == null) { return; }

            if (property != null)
            {
                property.values.Add(valorXML);
            }
            else
            {
                entidadAux.properties.AddRange(AddProperty(
                    new Property(propiedadXML, valorXML)
                ));
            }
        }

    }
}
