using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static Models.Entity;

namespace Utils
{
    public class UtilitySecciones
    {
        private static Dictionary<string, string> mListaRevistas = new Dictionary<string, string>();
        private static Dictionary<string, string> mListaPalabrasClave = new Dictionary<string, string>();

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

            while (true)
            {
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

                //Si tengo más de 10.000 resultados repito la consulta
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                ids.UnionWith(resultData.results.bindings.Select(x => x["item"].value));

                offsetInt += limit;
                if (resultData.results.bindings.Count < limit)
                {
                    break;
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

            int offsetInt = 0;
            int limit = 10000;

            if (mListaRevistas.Count == 0)
            {
                while (true)
                {
                    //Si tengo más de 10.000 resultados repito la consulta, sino salgo del bucle
                    string select = $@"SELECT distinct ?identificador ?nombreRevista {{ select *";
                    string where = $@"where {{
                                ?identificador <http://w3id.org/roh/title> ?nombreRevista .
                             }} ORDER BY ?nombreRevista
                        }} LIMIT {limit} OFFSET {offsetInt} ";
                    SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "maindocument");
                    for (int i = 0; i < resultData.results.bindings.Count; i++)
                    {
                        mListaRevistas[resultData.results.bindings.Select(x => x["nombreRevista"].value).ElementAt(i).ToLower()] = resultData.results.bindings.Select(x => x["identificador"].value).ElementAt(i);
                    }
                    offsetInt += limit;
                    if (resultData.results.bindings.Count < limit)
                    {
                        break;
                    }
                }
            }
            if (mListaRevistas.ContainsKey(nombreRevista.ToLower()))
            {
                return mListaRevistas.Where(x => x.Key.Equals(nombreRevista.ToLower())).Select(x => x.Value).FirstOrDefault();
            }
            else
            {
                return null;
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
        public static void AniadirEntidad(ResourceApi mResourceApi, string nombreEntidad, string propiedadNombreEntidad, string propiedadEntidad, Entity entidadAux, [Optional] string aux)
        {
            if (mResourceApi == null || string.IsNullOrEmpty(nombreEntidad) ||
                string.IsNullOrEmpty(propiedadEntidad) || string.IsNullOrEmpty(propiedadEntidad))
            { return; }

            Property propertyNombre = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadNombreEntidad);
            Property propertyEntidad = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadEntidad);

            string entidadN = GetOrganizacionPorNombre(mResourceApi, nombreEntidad);
            if (!string.IsNullOrEmpty(aux))
            {
                nombreEntidad = aux + nombreEntidad;
                entidadN = aux + entidadN;
            }

            if (!string.IsNullOrEmpty(entidadN))
            {
                CheckProperty(propertyNombre, entidadAux, nombreEntidad, propiedadNombreEntidad);
                CheckProperty(propertyEntidad, entidadAux, entidadN, propiedadEntidad);
            }
            else
            {
                CheckProperty(propertyNombre, entidadAux, nombreEntidad, propiedadNombreEntidad);
                CheckProperty(propertyEntidad, entidadAux, "", propiedadEntidad);
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
        /// Diccionario con el tesauro de todas las palabras clave, 
        /// con clave el identificador de la palabra clave y valor el identificador del padre o null en caso de ser el ultimo nodo.
        /// </summary>
        /// <param name="mResourceApi"></param>
        /// <returns></returns>
        public static Dictionary<string, string> PalabrasClave(ResourceApi mResourceApi)
        {
            if (mListaPalabrasClave.Count > 0) { return mListaPalabrasClave; }

            string select = "select ?idS ?idPadre ";
            string where = @$"where {{
                                ?s a <http://www.w3.org/2008/05/skos#Concept>.
                                ?s <http://purl.org/dc/elements/1.1/source> 'tesauro_cvn'.
                                ?s <http://purl.org/dc/elements/1.1/identifier> ?idS.
                                OPTIONAL {{ ?s <http://www.w3.org/2008/05/skos#broader> ?padre. ?padre <http://purl.org/dc/elements/1.1/identifier> ?idPadre. }}
                            }} ORDER BY ?padre ?s ";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "taxonomy");
            if (sparqlObject.results.bindings.Count > 0)
            {
                for (int i = 0; i < sparqlObject.results.bindings.Count; i++)
                {
                    mListaPalabrasClave[sparqlObject.results.bindings.Select(x => x["idS"].value).ElementAt(i)] = sparqlObject.results.bindings.ElementAt(i).ContainsKey("idPadre") ?
                        sparqlObject.results.bindings.Select(x => x["idPadre"].value).ElementAt(i) : null;
                }
            }

            return mListaPalabrasClave;
        }

        /// <summary>
        /// Obtiene el identificador y nick de la persona con CV <paramref name="CVID"/>
        /// </summary>
        /// <param name="resourceApi"></param>
        /// <param name="CVID"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ObtenerIdPersona(ResourceApi resourceApi, string CVID)
        {
            Dictionary<string, string> resultado = new Dictionary<string, string>();
            string select = "SELECT distinct ?idPersona ?nickPersona";
            string where = $@"where {{
                                <{CVID}> <http://w3id.org/roh/cvOf> ?idPersona . 
                                OPTIONAL{{ ?idPersona <http://xmlns.com/foaf/0.1/nick> ?nickPersona }}
                            }}";

            SparqlObject sparqlObject = resourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            if (sparqlObject.results.bindings.Count > 0)
            {
                string nick = sparqlObject.results.bindings.Any(x => x.ContainsKey("nickPersona")) ? sparqlObject.results.bindings.Select(x => x["nickPersona"].value)?.FirstOrDefault() : null;
                resultado.Add(sparqlObject.results.bindings.Select(x => x["idPersona"].value).FirstOrDefault(), nick);
            }
            return resultado;
        }

        /// <summary>
        /// Inserta los valores de los autores en cada una de sus propiedades.
        /// </summary>
        /// <param name="listaAutores">listaAutores</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propiedadAutorFirma">propiedadAutorFirma</param>
        /// <param name="propiedadAutorOrden">propiedadAutorOrden</param>
        /// <param name="propiedadAutorNombre">propiedadAutorNombre</param>
        /// <param name="propiedadAutorPrimerApellido">propiedadAutorPrimerApellido</param>
        /// <param name="propiedadAutorSegundoApellido">propiedadAutorSegundoApellido</param>
        public static void InsertaAutorProperties(List<CvnItemBeanCvnAuthorBean> listaAutores, Entity entidadAux, string propiedadAutorFirma, string propiedadAutorOrden,
            string propiedadAutorNombre, string propiedadAutorPrimerApellido, string propiedadAutorSegundoApellido)
        {
            //No hago nada si no se pasa la propiedad.
            if (string.IsNullOrEmpty(propiedadAutorFirma) || string.IsNullOrEmpty(propiedadAutorOrden) ||
                string.IsNullOrEmpty(propiedadAutorNombre) || string.IsNullOrEmpty(propiedadAutorPrimerApellido) ||
                string.IsNullOrEmpty(propiedadAutorSegundoApellido))
            { return; }

            foreach (CvnItemBeanCvnAuthorBean autor in listaAutores)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                Property propertyFirma = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorFirma);
                Property propertyOrden = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorOrden);
                Property propertyNombre = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorNombre);
                Property propertyPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorPrimerApellido);
                Property propertySegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorSegundoApellido);

                string valorFirma = StringGNOSSID(entityPartAux, autor.GetFirmaAutor());
                string valorOrden = StringGNOSSID(entityPartAux, autor.GetOrdenAutor());
                string valorNombre = StringGNOSSID(entityPartAux, autor.GetNombreAutor());
                string valorPrimerApellido = StringGNOSSID(entityPartAux, autor.GetPrimerApellidoAutor());
                string valorSegundoApellido = StringGNOSSID(entityPartAux, autor.GetSegundoApellidoAutor());

                //Si no tiene ningun valor no lo inserto
                if (string.IsNullOrEmpty(valorFirma) || string.IsNullOrEmpty(valorOrden) ||
                    string.IsNullOrEmpty(valorNombre) || string.IsNullOrEmpty(valorPrimerApellido) ||
                    string.IsNullOrEmpty(valorSegundoApellido)) { continue; }

                CheckProperty(propertyFirma, entidadAux, valorFirma, propiedadAutorFirma);
                CheckProperty(propertyOrden, entidadAux, valorOrden, propiedadAutorOrden);
                CheckProperty(propertyNombre, entidadAux, valorNombre, propiedadAutorNombre);
                CheckProperty(propertyPrimerApellido, entidadAux, valorPrimerApellido, propiedadAutorPrimerApellido);
                CheckProperty(propertySegundoApellido, entidadAux, valorSegundoApellido, propiedadAutorSegundoApellido);
            }
        }

        /// <summary>
        /// Inserta los Identificadores de publicación dependiendo del tipo
        /// 120-Handle, 040-DOI, 130-PMID, OTHERS-otros identificadores.
        /// </summary>
        /// <param name="listadoIDs">listadoIDs</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propIdHandle">propIdHandle</param>
        /// <param name="propIdDOI">propIdDOI</param>
        /// <param name="propIdPMID">propIdPMID</param>
        /// <param name="propIdOtroPub">propIdOtroPub</param>
        /// <param name="nombreOtroPub">nombreOtroPub</param>
        public static void InsertaTiposIDPublicacion(List<CvnItemBeanCvnExternalPKBean> listadoIDs, Entity entidadAux,
            string propIdHandle, string propIdDOI, string propIdPMID, string propIdOtroPub, string nombreOtroPub)
        {
            //Si alguna propiedad es nula no hago nada
            if (string.IsNullOrEmpty(propIdHandle) && string.IsNullOrEmpty(propIdHandle)
                && string.IsNullOrEmpty(propIdPMID) && string.IsNullOrEmpty(propIdOtroPub)
                && string.IsNullOrEmpty(nombreOtroPub))
            { return; }

            foreach (CvnItemBeanCvnExternalPKBean identificador in listadoIDs)
            {
                switch (identificador.Type)
                {
                    case "120":
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(propIdHandle, identificador.Value)
                        ));
                        break;
                    case "040":
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(propIdDOI, identificador.Value)
                        ));
                        break;
                    case "130":
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(propIdPMID, identificador.Value)
                        ));
                        break;
                    case "OTHERS":
                        Property IDOtro = entidadAux.properties.FirstOrDefault(x => x.prop == propIdOtroPub);
                        Property NombreOtro = entidadAux.properties.FirstOrDefault(x => x.prop == nombreOtroPub);

                        string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                        string valorID = StringGNOSSID(entityPartAux, identificador.Value); ;
                        CheckProperty(IDOtro, entidadAux, valorID, propIdOtroPub);

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
        /// Inserta en <paramref name="entidadAux"/> los valores de los ISSN.
        /// </summary>
        /// <param name="listadoISBN"></param>
        /// <param name="entidadAux"></param>
        /// <param name="propiedadISSN"></param>
        public static void InsertaISSN(List<CvnItemBeanCvnExternalPKBean> listadoISBN, Entity entidadAux, string propiedadISSN)
        {
            //No hago nada si no se pasa la propiedad.
            if (string.IsNullOrEmpty(propiedadISSN))
            { return; }

            foreach (CvnItemBeanCvnExternalPKBean issn in listadoISBN)
            {
                //Si no hay type, ignoro el valor
                if (string.IsNullOrEmpty(issn.Type)) { continue; }

                //Si es ISSN (010)
                if (issn.Type.Equals("010"))
                {
                    entidadAux.properties.AddRange(AddProperty(
                        new Property(propiedadISSN, issn.Value)
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
