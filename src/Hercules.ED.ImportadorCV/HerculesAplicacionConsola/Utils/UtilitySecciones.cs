using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.DisambiguationEngine.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using static Models.Entity;

namespace HerculesAplicacionConsola.Utils
{
    public class UtilitySecciones
    {
        /// <summary>
        /// Devuelve los identificadores devueltos en la consulta.
        /// </summary>
        /// <param name="pResourceApi">pResourceApi</param>
        /// <param name="pCVID">pCVID</param>
        /// <param name="propiedadesItem">propiedadesItem</param>
        /// <returns>HashSet<string></returns>
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
            where += $@" FILTER(?s = <{pCVID}>)
                            }} ORDER BY ?item
                        }}LIMIT {limit} OFFSET {offsetInt} ";

            //Si tengo más de 10.000 resultados repito la consulta, sino salgo del bucle
            while (true)
            {
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

        /// <summary>
        /// Devuelve el identificador a <paramref name="nombreRevista"/>
        /// en caso de que se encuentre en la consulta, nulo en caso contrario.
        /// </summary>
        /// <param name="pResourceApi">pResourceApi</param>
        /// <param name="nombreRevista">nombreRevista</param>
        /// <returns>string</returns>
        public static string GetNombreRevista(ResourceApi pResourceApi, string nombreRevista)
        {
            //Si el nombre de la revista es nulo o vacio
            if (string.IsNullOrEmpty(nombreRevista)) { return null; }

            string select = $@"SELECT distinct ?identificador";
            string where = $@"where {{
                                ?identificador <http://w3id.org/roh/title> ?nombreRevista .
                                FILTER (lcase(?nombreRevista) = ""{nombreRevista.ToLower()}"")
                            }} LIMIT 1";

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

        //TODO
        public static List<string> GetBusquedaAutor(ResourceApi pResourceApi, string busqueda)
        {
            string select = $@"select distinct ?busqueda";
            string where = $@"where {{
                               graph ?g{{
                                    ?s <http://w3id.org/roh/publicAuthorList> ?listaAuthor . 
                                    ?listaAuthor <http://gnoss/search> ?busqueda . 
                                    filter contains(?busqueda, ""{busqueda.ToLower()}"")
                                }} 
                             }}";

            /*
             select distinct ?nombre ?listaAuthor
             where {{ graph ?g{{
                    ?s<http://w3id.org/roh/publicAuthorList> ?listaAuthor . 
                    ?listaAuthor <http://xmlns.com/foaf/0.1/firstName> ?nombre .
                    ?listaAuthor <http://xmlns.com/foaf/0.1/lastName> ?apellidos .
                    filter contains(lcase(?nombre), ""nombre"") .
                    filter contains(lcase(?apellidos), ""apellidos"") .
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

        /// <summary>
        /// Devuelve la referencia a <paramref name="nombreOrganizacion"/>
        /// en caso de que se encuentre en la consulta, nulo en caso contrario.
        /// </summary>
        /// <param name="pResourceApi">pResourceApi</param>
        /// <param name="nombreOrganizacion">nombreOrganizacion</param>
        /// <returns>string</returns>
        public static string GetOrganizacionPorNombre(ResourceApi pResourceApi, string nombreOrganizacion)
        {
            string select = $@"select distinct ?nombre ?organizacion";
            string where = $@"where {{ 
                                ?organizacion <http://w3id.org/roh/title> ?nombre  
                                FILTER(ucase(?nombre)=""{nombreOrganizacion.ToUpper()}"")
                            }}  LIMIT 1";

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

        /// <summary>
        /// Añade la referencia a la entidad <paramref name="propiedadNombreEntidad"/> si esta se encuentra en BBDD.
        /// </summary>
        /// <param name="mResourceApi"></param>
        /// <param name="nombreEntidad"></param>
        /// <param name="propiedadNombreEntidad"></param>
        /// <param name="propiedadEntidad"></param>
        /// <param name="entidadAux"></param>
        public static void AniadirEntidad(ResourceApi mResourceApi, string nombreEntidad, string propiedadNombreEntidad, string propiedadEntidad, Entity entidadAux)
        {
            if (mResourceApi == null || string.IsNullOrEmpty(nombreEntidad) ||
                string.IsNullOrEmpty(propiedadEntidad) || string.IsNullOrEmpty(propiedadEntidad))
            { return; }

            string entidadN = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, nombreEntidad);
            if (!string.IsNullOrEmpty(entidadN))
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(propiedadNombreEntidad, nombreEntidad),
                    new Property(propiedadEntidad, entidadN)
                ));
            }
            else
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                       new Property(propiedadNombreEntidad, nombreEntidad)
                ));
                entidadAux.properties.Add(new Property(propiedadEntidad, ""));
            }
        }

        /// <summary>
        /// Añade en <paramref name="entidadAux"/> el <paramref name="valorXML"/> en caso de que no sea nulo.
        /// Si <paramref name="property"/> no es valor nulo, se añade el <paramref name="valorXML"/> en la propiedad,
        /// sino se crea una <paramref name="propiedadXML"/> en <paramref name="entidadAux"/>
        /// </summary>
        /// <param name="property">property</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="valorXML">valorXML</param>
        /// <param name="propiedadXML">propiedadXML</param>
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

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de códigos UNESCO de <paramref name="listaCodUnesco"/>,
        /// en <paramref name="propiedadCodUnesco"/>.
        /// </summary>
        /// <param name="listaCodUnesco">listaCodigosUnesco</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propiedadCodUnesco">propiedadCodigoUnesco</param>
        public static void CodigosUnesco(List<CvnItemBeanCvnString> listaCodUnesco, Entity entidadAux, string propiedadCodUnesco)
        {
            //No hago nada si no se pasa la propiedad.
            if (string.IsNullOrEmpty(propiedadCodUnesco))
            { return; }

            foreach (CvnItemBeanCvnString codigo in listaCodUnesco)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnesco = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadCodUnesco);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    CheckProperty(propertyCodUnesco, entidadAux, valorCodigo, propiedadCodUnesco);
                }
            }
        }

        /// <summary>
        /// Inserta los Identificadores de publicación dependiendo del tipo
        /// 120-Handle, 040-DOI, 130-PMID, OTHERS-otros identificadores.
        /// </summary>
        /// <param name="listadoIDs">listadoIDs</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="idHandle">idHandle</param>
        /// <param name="idDOI">idDOI</param>
        /// <param name="idPMID">idPMID</param>
        /// <param name="idOtroPub">idOtroPub</param>
        /// <param name="nombreOtroPub">nombreOtroPub</param>
        public static void InsertaTiposIDPublicacion(List<CvnItemBeanCvnExternalPKBean> listadoIDs, Entity entidadAux,
            string idHandle, string idDOI, string idPMID, string idOtroPub, string nombreOtroPub)
        {
            //Si alguna propiedad es nula no hago nada
            if (string.IsNullOrEmpty(idHandle) && string.IsNullOrEmpty(idHandle)
                && string.IsNullOrEmpty(idPMID) && string.IsNullOrEmpty(idOtroPub)
                && string.IsNullOrEmpty(nombreOtroPub))
            { return; }

            foreach (CvnItemBeanCvnExternalPKBean identificador in listadoIDs)
            {
                switch (identificador.Type)
                {
                    case "120":
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(idHandle, identificador.Value)
                        ));
                        break;
                    case "040":
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(idDOI, identificador.Value)
                        ));
                        break;
                    case "130":
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(idPMID, identificador.Value)
                        ));
                        break;
                    case "OTHERS":
                        Property IDOtro = entidadAux.properties.FirstOrDefault(x => x.prop == idOtroPub);
                        Property NombreOtro = entidadAux.properties.FirstOrDefault(x => x.prop == nombreOtroPub);

                        string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                        string valorID = StringGNOSSID(entityPartAux, identificador.Value); ;
                        CheckProperty(IDOtro, entidadAux, valorID, idOtroPub);

                        string valorNombre = StringGNOSSID(entityPartAux, identificador.Others);
                        CheckProperty(IDOtro, entidadAux, valorNombre, nombreOtroPub);
                        break;
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de los ISBN.
        /// </summary>
        /// <param name="listadoISBN">listadoISBN</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propiedadISBN">propiedadISBN</param>
        public static void InsertaISBN(List<CvnItemBeanCvnExternalPKBean> listadoISBN, Entity entidadAux, string propiedadISBN)
        {
            //No hago nada si no se pasa la propiedad.
            if (string.IsNullOrEmpty(propiedadISBN))
            { return; }

            foreach (CvnItemBeanCvnExternalPKBean isbn in listadoISBN)
            {
                //Si no hay type, ignoro el valor
                if (string.IsNullOrEmpty(isbn.Type)) { continue; }

                //Si es ISBN (020)
                if (isbn.Type.Equals("020"))
                {
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(propiedadISBN, isbn.Value)
                    ));
                }

            }
        }

        /// <summary>
        /// Añade los PhoneBean de <paramref name="listado"/> en <paramref name="entidadAux"/>.
        /// </summary>
        /// <param name="listado">listado de numeros telefonicos</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propiedadNumero">propiedadNumero</param>
        /// <param name="propiedadCodInternacional">propiedadCodInternacional</param>
        /// <param name="propiedadExtension">propiedadExtension</param>
        public static void InsertarListadoTelefonos(List<CvnItemBeanCvnPhoneBean> listado, Entity entidadAux, string propiedadNumero, string propiedadCodInternacional, string propiedadExtension)
        {
            //No hago nada si no se pasan las propiedades de cada parametro.
            if (string.IsNullOrEmpty(propiedadNumero) && string.IsNullOrEmpty(propiedadExtension)
                && string.IsNullOrEmpty(propiedadCodInternacional))
            { return; }

            foreach (CvnItemBeanCvnPhoneBean telefono in listado)
            {
                //No añado si no hay numero de telefono.
                if (string.IsNullOrEmpty(telefono.Number)) { continue; }

                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                //Añado Numero
                Property propertyNumero = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadNumero);

                string valorNumero = StringGNOSSID(entityPartAux, telefono.Number);

                CheckProperty(propertyNumero, entidadAux, valorNumero, propiedadNumero);

                //Añado Codigo Internacional
                Property propertyCodInternacional = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadCodInternacional);

                string valorCodInternacional = StringGNOSSID(entityPartAux, telefono.InternationalCode);

                CheckProperty(propertyCodInternacional, entidadAux, valorCodInternacional, propiedadCodInternacional);

                //Añado Extension
                Property propertyExtension = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadExtension);

                string valorExtension = StringGNOSSID(entityPartAux, telefono.Extension);

                CheckProperty(propertyExtension, entidadAux, valorExtension, propiedadExtension);
            }
        }

        /// <summary>
        /// Dada una cadena de GUID concatenados y finalizando en "|" y un string en caso de que 
        /// el string no sea nulo los concatena, sino devuelve null.
        /// </summary>
        /// <param name="entityAux">GUID concatenado con "|"</param>
        /// <param name="valor">Valor del parametro</param>
        /// <returns>String de concatenar los parametros, o nulo si el valor es vacio</returns>
        public static string StringGNOSSID(string entityAux, string valor)
        {
            if (!string.IsNullOrEmpty(valor))
            {
                return entityAux + valor;
            }
            return null;
        }

    }
}
