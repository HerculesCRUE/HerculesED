using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.DisambiguationEngine.Models;
using Hercules.ED.ImportExportCV.Models;
using Hercules.ED.ImportExportCV.Models.FuentesExternas;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using static Models.Entity;

namespace Utils
{
    public static class Utility
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        public static readonly int splitListNum = 500;

        /// <summary>
        /// Devuelve la persona a partir del CV
        /// </summary>
        /// <param name="pCVID"></param>
        /// <returns></returns>
        public static string PersonaCV(string pCVID)
        {
            string select = $@"select distinct ?person ";
            string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person a <http://xmlns.com/foaf/0.1/Person> .
                                    FILTER(?s=<{pCVID}>)
                                }}";
            SparqlObject resultData = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string> { "curriculumvitae", "person"});
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                return fila["person"].value;
            }
            return null;
        }
        
        public static string GetNombreCompletoPersonaCV(string pCVID)
        {
            string select = $@"select distinct ?name ";
            string where = $@" where {{
                                ?cv a <http://w3id.org/roh/CV> .
                                ?cv ?cvOf ?person .
                                ?person a <http://xmlns.com/foaf/0.1/Person> .
                                ?person <http://xmlns.com/foaf/0.1/name> ?name . 
                                FILTER(?cv=<{pCVID}>)
                            }}";
            SparqlObject resultData = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string> { "curriculumvitae", "person" });
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                return fila["name"].value;
            }
            return null;
        }

        /// <summary>
        /// Dada una lista de personas, devuelve las organizaciones correspondientes a cada persona.
        /// </summary>
        /// <param name="personas"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> DatosOrganizacionPersona(List<string> personas)
        {
            Dictionary<string, HashSet<string>> organizaciones = new Dictionary<string, HashSet<string>>();
            string select = $@"select distinct ?person ?organization
                                from <{mResourceApi.GraphsUrl}person.owl> 
                                from <{mResourceApi.GraphsUrl}group.owl> 
                                ";
            string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?person <http://w3id.org/roh/hasRole> ?organization .
                                    FILTER(?person in (<{string.Join(">,<", personas)}>))
                                }} ";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                //Organizaciones
                if (fila.ContainsKey("organization"))
                {
                    if (organizaciones.ContainsKey(fila["person"].value))
                    {
                        organizaciones[fila["person"].value].Append(fila["organization"].value);
                    }
                    else
                    {
                        organizaciones.Add(fila["person"].value, new HashSet<string>() { fila["organization"].value });
                    }
                }
            }
            return organizaciones;
        }

        /// <summary>
        /// Dada una lista de personas, devuelve los departamentos correspondientes a cada persona.
        /// </summary>
        /// <param name="personas"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> DatosDepartamentoPersona(List<string> personas)
        {
            Dictionary<string, HashSet<string>> departamentos = new Dictionary<string, HashSet<string>>();
            string select = $@"select distinct ?person ?departament
                                from <{mResourceApi.GraphsUrl}person.owl> 
                                from <{mResourceApi.GraphsUrl}group.owl> 
                                ";
            string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?person <http://vivoweb.org/ontology/core#departmentOrSchool> ?departament .
                                    FILTER(?person in (<{string.Join(">,<", personas)}>))
                                }} ";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                //Departamentos
                if (fila.ContainsKey("departament"))
                {
                    if (departamentos.ContainsKey(fila["person"].value))
                    {
                        departamentos[fila["person"].value].Append(fila["departament"].value);
                    }
                    else
                    {
                        departamentos.Add(fila["person"].value, new HashSet<string>() { fila["departament"].value });
                    }
                }
            }
            return departamentos;
        }

        /// <summary>
        /// Dada una lista de personas, devuelve los proyectos correspondientes a cada persona.
        /// </summary>
        /// <param name="personas"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> DatosProyectoPersona(List<string> personas)
        {
            Dictionary<string, HashSet<string>> proyectos = new Dictionary<string, HashSet<string>>();
            string select = $@"select distinct ?person ?project
                                from <{mResourceApi.GraphsUrl}person.owl> 
                                from <{mResourceApi.GraphsUrl}project.owl> 
                                ";
            string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?project a <http://vivoweb.org/ontology/core#Project>.
                                    ?project ?propRol ?rol .
                                    FILTER(?propRol in (<http://w3id.org/roh/researchers>,<http://w3id.org/roh/mainResearchers>))
                                    ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person .
                                    FILTER(?person in (<{string.Join(">,<", personas)}>))
                                }} ";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                //Proyectos
                if (fila.ContainsKey("project"))
                {
                    if (proyectos.ContainsKey(fila["person"].value))
                    {
                        proyectos[fila["person"].value].Append(fila["project"].value);
                    }
                    else
                    {
                        proyectos.Add(fila["person"].value, new HashSet<string>() { fila["project"].value });
                    }
                }
            }
            return proyectos;
        }

        /// <summary>
        /// Dada una lista de personas, devuelve los grupos correspondientes a cada persona.
        /// </summary>
        /// <param name="personas"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> DatosGrupoPersona(List<string> personas)
        {
            Dictionary<string, HashSet<string>> grupos = new Dictionary<string, HashSet<string>>();
            string select = $@"select distinct ?person ?group
                                from <{mResourceApi.GraphsUrl}person.owl> 
                                from <{mResourceApi.GraphsUrl}group.owl> 
                                ";
            string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.
                                    ?group a <http://xmlns.com/foaf/0.1/Group>.
                                    ?group ?propRol ?rol .
                                    FILTER(?propRol in (<http://w3id.org/roh/researchers>,<http://w3id.org/roh/mainResearchers>))
                                    ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person .
                                    FILTER(?person in (<{string.Join(">,<", personas)}>))
                                }} ";
            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                //Grupos
                if (fila.ContainsKey("group"))
                {
                    if (grupos.ContainsKey(fila["person"].value))
                    {
                        grupos[fila["person"].value].Append(fila["group"].value);
                    }
                    else
                    {
                        grupos.Add(fila["person"].value, new HashSet<string>() { fila["group"].value });
                    }
                }
            }
            return grupos;
        }

        /// <summary>
        /// Dado un codigo devuelve si el formato es valido
        /// </summary>
        /// <param name="codigo">Codigo</param>
        /// <returns>True si el formato es valido</returns>
        public static bool CodigoCorrecto(string codigo)
        {
            if (string.IsNullOrEmpty(codigo)) { return false; }
            return Regex.Match(codigo, "^\\d{3}(\\.\\d{3}){0,3}$").Success;
        }

        /// <summary>
        /// Dado un codigo devuelve si el formato es invalido
        /// </summary>
        /// <param name="codigo">Codigo</param>
        /// <returns>True si el formato es invalido</returns>
        public static bool CodigoIncorrecto(string codigo)
        {
            return !CodigoCorrecto(codigo);
        }

        /// <summary>
        /// Dado un codigo y su longitud contando los puntos
        /// Devuelve si es formato del codigo es valido
        /// </summary>
        /// <param name="codigo">Codigo</param>
        /// <returns>True si el codigo es correcto</returns>
        public static bool CodigoCampoCorrecto(string codigo)
        {
            if (CodigoCorrecto(codigo))
            {
                return codigo.Length == 15;
            }

            return false;
        }

        /// <summary>
        /// Dado el listado y los digitos del apartado, subapartado, bloque.
        /// Devuelve el listado de los campos con codigo empezado por los digitos pasados como parametro.
        /// </summary>
        /// <param name="cvn">cvnRootResultBean</param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Listado de bloque</returns>
        public static List<CVNObject> ListadoBloque(cvnRootResultBean cvn, string codigo)
        {
            List<CVNObject> listadoCampos = new List<CVNObject>();

            if (!Regex.Match(codigo, "^\\d{3}(\\.\\d{3}){0,3}$").Success)
            {
                return listadoCampos;
            }

            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                if (c.Code.StartsWith(codigo))
                {
                    foreach (CVNObject o in c.Items)
                    {
                        listadoCampos.Add(o);
                    }
                }
            }

            return listadoCampos;
        }

        /// <summary>
        /// Dado el listado y los tres digitos del apartado.
        /// Devuelve si existe algun apartado empezado por esos digitos en el listado.
        /// </summary>
        /// <param name="cvn">cvnRootResultBean</param>
        /// <param name="codigo">Codigo</param>
        /// <returns>True si existe ese apartado</returns>
        public static bool ExisteIdentificadorApartado(cvnRootResultBean cvn, string codigo)
        {
            if (codigo.Length != 3)
            {
                return false;
            }
            if (!Regex.Match(codigo, "\\d{3}").Success)
            {
                return false;
            }

            if (!cvn.cvnRootBean.Any())
            {
                return false;
            }
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                if (c.Code.StartsWith(codigo))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Dado el listado y los digitos del apartado y subapartado unidos con un punto.
        /// Devuelve si existe algun subapartado empezado con esos digitos en el listado.
        /// </summary>
        /// <param name="cvn">cvnRootResultBean</param>
        /// <param name="codigo">Codigo</param>
        /// <returns>True si existe ese subapartado</returns>
        public static bool ExisteIdentificadorSubapartado(cvnRootResultBean cvn, string codigo)
        {
            if (codigo.Length != 7)
            {
                return false;
            }
            if (!Regex.Match(codigo, "\\d{3}\\.\\d{3}").Success)
            {
                return false;
            }

            if (!cvn.cvnRootBean.Any())
            {
                return false;
            }
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                if (c.Code.StartsWith(codigo))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Dado el listado y los digitos del apartado, subapartado y bloque unidos cada uno con un punto.
        /// Devuelve si existe algun bloque empezado con esos digitos en el listado.
        /// </summary>
        /// <param name="cvn">cvnRootResultBean</param>
        /// <param name="codigo">Codigo</param>
        /// <returns>True si existe ese bloque</returns>
        public static bool ExisteIdentificadorBloque(cvnRootResultBean cvn, string codigo)
        {
            if (codigo.Length != 11)
            {
                return false;
            }
            if (!Regex.Match(codigo, "\\d{3}\\.\\d{3}\\.\\d{3}").Success)
            {
                return false;
            }

            if (!cvn.cvnRootBean.Any())
            {
                return false;
            }
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                if (c.Code.StartsWith(codigo))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Dado el codigo de apartado, subapartado o bloque devuelve los campos que empiecen por el 
        /// codigo.
        /// </summary>
        /// <param name="cvn">cvnRootResultBean</param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Lista de los elementos del bloque</returns>
        public static List<CvnItemBean> GetListadoBloque(this cvnRootResultBean cvn, string codigo)
        {
            try
            {
                if (!CodigoCorrecto(codigo)) { throw new ArgumentException("Codigo de campo incorrecto" + codigo); }
                List<CvnItemBean> listadoCampos = cvn.cvnRootBean.Where(x => x.Code.StartsWith(codigo))?.ToList();
                return listadoCampos;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un listado con todos los elementos que tengan ese codigo.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cvnItemBean">cvnItemBean</param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Lista de elementos de tipo <typeparamref name="T"/> de <paramref name="cvnItemBean"/></returns>
        public static List<T> GetListaElementosPorIDCampo<T>(this CvnItemBean cvnItemBean, string codigo) where T : CVNObject
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvnItemBean.Items?.ToList();
                List<T> listadoCampos = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is T).Cast<T>()?.ToList();
                return listadoCampos;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un listado con todos los elementos que tengan ese codigo.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listado">Listado de CvnItemBean</param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Lista de elementos de tipo <typeparamref name="T"/> de <paramref name="listado"/></returns>
        public static List<T> GetListaElementosPorIDCampo<T>(this List<CvnItemBean> listado, string codigo) where T : CVNObject
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
                List<T> listadoCampos = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is T).Cast<T>()?.ToList();
                return listadoCampos;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un elemento
        /// de tipo CvnItemBeanCvnString que tenga ese codigo.
        /// </summary>
        /// <param name="cvnItemBean"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns><typeparamref name="T"/> de <paramref name="cvnItemBean"/></returns>
        public static T GetElementoPorIDCampo<T>(this CvnItemBean cvnItemBean, string codigo) where T : CVNObject
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvnItemBean.Items?.ToList();
                T campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is T).Cast<T>().FirstOrDefault();
                return campo;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un elemento
        /// de tipo CvnItemBeanCvnString que tenga ese codigo.
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns><typeparamref name="T"/> de <paramref name="codigo"/></returns>
        public static T GetElementoPorIDCampo<T>(this List<CvnItemBean> listado, string codigo) where T : CVNObject
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
                T campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is T).Cast<T>().FirstOrDefault();
                return campo;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un string del elemento
        /// de tipo CvnItemBeanCvnString que tenga ese codigo.
        /// </summary>
        /// <param name="cvnItemBean"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnString que tenga ese codigo</returns>
        public static string GetStringPorIDCampo(this CvnItemBean cvnItemBean, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvnItemBean.Items?.ToList();
                CvnItemBeanCvnString campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
                if (campo != null && !string.IsNullOrEmpty(campo.Value.Trim()))
                {
                    return campo.Value;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un string del elemento
        /// de tipo CvnItemBeanCvnString que tenga ese codigo.
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnString que tenga ese codigo</returns>
        public static string GetStringPorIDCampo(this List<CvnItemBean> listado, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value.Trim()))
            {
                return campo.Value;
            }
            return null;

        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un string del anio, 
        /// de tipo CvnItemBeanCvnDuration que tenga ese codigo.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnDuration que tenga ese codigo</returns>
        public static string GetDurationAnioPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnDuration campo = item.Items.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDuration).Cast<CvnItemBeanCvnDuration>().FirstOrDefault();
            if (campo != null)
            {
                string aux = campo.Value?.Replace("P", "");
                int Y = aux.IndexOf("Y");
                if (Y == -1)
                {
                    return null;
                }

                return aux.Substring(0, Y);
            }
            return null;
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un string del mes, 
        /// de tipo CvnItemBeanCvnDuration que tenga ese codigo.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnDuration que tenga ese codigo</returns>
        public static string GetDurationMesPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnDuration campo = item.Items.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDuration).Cast<CvnItemBeanCvnDuration>().FirstOrDefault();
            if (campo != null)
            {
                string aux = campo.Value?.Replace("P", "");
                int Y = aux.IndexOf("Y");
                int M = aux.IndexOf("M");
                if (M == -1)
                {
                    return null;
                }
                if (Y == -1)
                {
                    return aux.Substring(0, M);
                }
                return aux.Substring(Y + 1, M - Y - 1);
            }
            return null;
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un string del dia, 
        /// de tipo CvnItemBeanCvnDuration que tenga ese codigo.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnDuration que tenga ese codigo</returns>
        public static string GetDurationDiaPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnDuration campo = item.Items.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDuration).Cast<CvnItemBeanCvnDuration>().FirstOrDefault();
            if (campo != null)
            {
                string aux = campo.Value?.Replace("P", "");
                int Y = aux.IndexOf("Y");
                int M = aux.IndexOf("M");
                int D = aux.IndexOf("D");
                if (D == -1)
                {
                    return null;
                }
                if (M == -1 && Y == -1)
                {
                    return aux.Substring(0, D);
                }
                if (M == -1)
                {
                    return aux.Substring(Y + 1, D - Y - 1);
                }
                return aux.Substring(M + 1, D - M - 1);
            }
            return null;
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un string de las horas, 
        /// de tipo CvnItemBeanCvnDuration que tenga ese codigo.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnDuration que tenga ese codigo</returns>
        public static string GetDurationHorasPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnDuration campo = item.Items.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDuration).Cast<CvnItemBeanCvnDuration>().FirstOrDefault();
            if (campo != null)
            {
                string aux = campo.Value?.Replace("P", "");
                int T = aux.IndexOf("T");
                int H = aux.IndexOf("H");
                if (H == -1)
                {
                    return null;
                }
                if (T == 0)
                {
                    return aux.Substring(T + 1, H - T - 1);
                }
            }
            return null;
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve el valor booleano del elemento, 
        /// de tipo CvnItemBeanCvnBoolean que tenga ese codigo.
        /// </summary>
        /// <param name="cvnItemBean"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnBoolean que tenga ese codigo</returns>
        public static string GetStringBooleanPorIDCampo(this CvnItemBean cvnItemBean, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvnItemBean.Items?.ToList();
                CvnItemBeanCvnBoolean campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnBoolean).Cast<CvnItemBeanCvnBoolean>().FirstOrDefault();
                if (campo != null)
                {
                    return campo.Value.ToString()?.ToLower();
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Obtiene un diccionario con las firma, como clave, y las posibles personas asociadas a la misma encontradas en BBDD de <paramref name="listadoFirma"/>.
        /// </summary>
        /// <param name="pResourceApi"></param>
        /// <param name="listadoFirma">Listado de firmas</param>
        /// <returns></returns>
        public static Dictionary<string, List<Persona>> ObtenerPersonasFirma(ResourceApi pResourceApi, List<string> listadoFirma)
        {
            Dictionary<string, List<Persona>> diccionarioPersonasFirma = new Dictionary<string, List<Persona>>();
            string nameInput = "";

            string selectOut = "select distinct ?personID ?name ?num ?nameInput ";
            string whereOut = $@"where{{
                                    ?personID <http://xmlns.com/foaf/0.1/name> ?name.
                                    {{";

            foreach (string firma in listadoFirma)
            {
                string texto = Disambiguation.ObtenerTextosNombresNormalizados(firma);
                string[] wordsTexto = texto.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (wordsTexto.Length > 0)
                {
                    #region Buscamos en nombres
                    {
                        List<string> unions = new List<string>();
                        List<string> unionsOut = new List<string>();
                        foreach (string wordOut in wordsTexto)
                        {
                            List<string> words = new List<string>();
                            if (wordOut.Length == 2)
                            {
                                words.Add(wordOut[0].ToString());
                                words.Add(wordOut[1].ToString());
                            }
                            else
                            {
                                words.Add(wordOut);
                            }

                            foreach (string word in words)
                            {
                                int score = 1;
                                if (word.Length > 1)
                                {
                                    score = 5;
                                }
                                if (score == 1)
                                {
                                    StringBuilder sbUnion = new StringBuilder();
                                    sbUnion.AppendLine("				?personID <http://xmlns.com/foaf/0.1/name> ?name.");
                                    sbUnion.AppendLine($@"				{{  FILTER(lcase(?name) like'{word}%').}} UNION  {{  FILTER(lcase(?name) like'% {word}%').}}  BIND({score} as ?num)  ");
                                    unions.Add(sbUnion.ToString());
                                }
                                else
                                {
                                    StringBuilder sbUnion = new StringBuilder();
                                    sbUnion.AppendLine("				?personID <http://xmlns.com/foaf/0.1/name> ?name.");
                                    sbUnion.AppendLine($@"				{FilterWordComplete(word, "name")} BIND({score} as ?num) ");
                                    //sbUnion.AppendLine($@"				?name bif:contains ""'{word}'"" BIND({score} as ?num) ");
                                    unions.Add(sbUnion.ToString());
                                }
                            }
                        }

                        string select = $@" select distinct ?personID sum(?num) as ?num ?nameInput ";
                        string where = $@" where
                                        {{
                                            ?personID a <http://xmlns.com/foaf/0.1/Person>.
                                            {{{string.Join("}UNION{", unions)}}}           
                                            BIND(""{texto}"" as ?nameInput)
                                        }}order by desc (?num) limit 50
                                     ";
                        string consultaInterna = select + where;
                        whereOut += consultaInterna + "}UNION{";
                    }
                    #endregion
                }
            }
            whereOut = whereOut.Remove(whereOut.Length - 6, 6);
            whereOut += " }order by desc (?nameInput) desc ( ?num)";

            SparqlObject sparqlObject = pResourceApi.VirtuosoQuery(selectOut, whereOut, "person");

            foreach (string firma in listadoFirma)
            {
                HashSet<int> scores = new HashSet<int>();
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings.Where(x => x["nameInput"].value == firma))
                {
                    nameInput = fila["nameInput"].value;
                    if (!diccionarioPersonasFirma.ContainsKey(nameInput))
                    {
                        diccionarioPersonasFirma[nameInput] = new List<Persona>();
                    }

                    string personID = fila["personID"].value;
                    string name = fila["name"].value;
                    int score = int.Parse(fila["num"].value);
                    scores.Add(score);
                    if (scores.Count > 2)
                    {
                        break;
                    }
                    Persona persona = new Persona
                    {
                        nombreCompleto = name,
                        personid = personID
                    };
                    diccionarioPersonasFirma[nameInput].Add(persona);
                }
            }

            return diccionarioPersonasFirma;
        }

        /// <summary>
        /// Cambia las letras acentuadas a, e, i, o, u por las mismas sin el signo de puntuación, la letra ñ por n y la letra ç por c.
        /// </summary>
        /// <param name="pWord"></param>
        /// <param name="pVar"></param>
        /// <returns></returns>
        public static string FilterWordComplete(string pWord, string pVar)
        {
            Dictionary<string, string> listaReemplazos = new Dictionary<string, string>();
            listaReemplazos["a"] = "aáàä";
            listaReemplazos["e"] = "eéèë";
            listaReemplazos["i"] = "iíìï";
            listaReemplazos["o"] = "oóòö";
            listaReemplazos["u"] = "uúùü";
            listaReemplazos["n"] = "nñ";
            listaReemplazos["c"] = "cç";
            foreach (string caracter in listaReemplazos.Keys)
            {
                pWord = pWord.Replace(caracter, $"[{listaReemplazos[caracter]}]");
            }
            string filter = @$"FILTER ( regex(?{pVar},""(^| ){pWord}($| )"", ""i""))";
            return filter;
        }

        /// <summary>
        /// Dado el codigo del campo, devuelve el valor del elemento, 
        /// de tipo CvnItemBeanCvnDouble que tenga ese codigo.
        /// En formato 
        /// </summary>
        /// <param name="cvnItemBean"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnDouble que tenga ese codigo</returns>
        public static string GetStringDoublePorIDCampo(this CvnItemBean cvnItemBean, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvnItemBean.Items?.ToList();
                CvnItemBeanCvnDouble campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDouble).Cast<CvnItemBeanCvnDouble>().FirstOrDefault();
                if (campo != null)
                {
                    return campo.Value.ToString();
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Devuelve la fecha en formato GNOSS (YYYYMMDD000000) como un string.
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string de la fecha en formato GNOSS</returns>
        public static string GetStringDatetimePorIDCampo(this List<CvnItemBean> listado, string codigo)
        {
            if (listado == null) { return null; }
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            List<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnDateDayMonthYear campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDateDayMonthYear).Cast<CvnItemBeanCvnDateDayMonthYear>().FirstOrDefault();
            if (campo != null)
            {
                return campo.DatetimeStringGNOSS();
            }
            CvnItemBeanCvnDateMonthYear campoMesAnio = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDateMonthYear).Cast<CvnItemBeanCvnDateMonthYear>().FirstOrDefault();
            if (campoMesAnio != null)
            {
                return campoMesAnio.DatetimeStringGNOSS();
            }
            CvnItemBeanCvnDateYear campoDiaAnio = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDateYear).Cast<CvnItemBeanCvnDateYear>().FirstOrDefault();
            if (campoDiaAnio != null)
            {
                return campoDiaAnio.DatetimeStringGNOSS();
            }

            return null;
        }

        /// <summary>
        /// Devuelve la fecha en formato GNOSS (YYYYMM01000000) como un string.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string de la fecha en formato GNOSS</returns>
        public static string GetStringDatetimePorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnDateDayMonthYear campo = item.Items.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDateDayMonthYear).Cast<CvnItemBeanCvnDateDayMonthYear>().FirstOrDefault();
            if (campo != null)
            {
                return campo.DatetimeStringGNOSS();
            }
            CvnItemBeanCvnDateMonthYear campoMesAnio = item.Items.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDateMonthYear).Cast<CvnItemBeanCvnDateMonthYear>().FirstOrDefault();
            if (campoMesAnio != null)
            {
                return campoMesAnio.DatetimeStringGNOSS();
            }
            CvnItemBeanCvnDateYear campoDiaAnio = item.Items.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDateYear).Cast<CvnItemBeanCvnDateYear>().FirstOrDefault();
            if (campoDiaAnio != null)
            {
                return campoDiaAnio.DatetimeStringGNOSS();
            }

            return null;
        }

        /// <summary>
        /// Devuelve el genero,
        /// con formato mResourceApi.GraphsUrl + "items/gender_" + valor
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Genero</returns>
        public static string GetGeneroPorIDCampo(this List<CvnItemBean> listado, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/gender_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el pais como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_PCLD_" + valor
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Pais</returns>
        public static string GetPaisPorIDCampo(this List<CvnItemBean> listado, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/feature_PCLD_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el pais como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_PCLD_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Pais</returns>
        public static string GetPaisPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Length != 15) { return null; }

            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/feature_PCLD_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la region como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_ADM1_" + valor
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Region</returns>
        public static string GetRegionPorIDCampo(this List<CvnItemBean> listado, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/feature_ADM1_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la region como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_ADM1_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Region</returns>
        public static string GetRegionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Length != 15) { return null; }

            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/feature_ADM1_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la region como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/resulttype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Region</returns>
        public static string GetTipoResultadoIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Length != 15) { return null; }

            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/resulttype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la provincia como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_ADM2_" + valor
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Provincia</returns>
        public static string GetProvinciaPorIDCampo(this List<CvnItemBean> listado, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/feature_ADM2_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la organizacion como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/organizationtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Organizacion</returns>
        public static string GetOrganizacionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/organizationtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de programa de tutorización como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/tutorshipsprogramtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Organizacion</returns>
        public static string GetTipoProgramaTutorizacionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/tutorshipsprogramtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de evento como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/seminareventtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>OrganizaTipoEventocion</returns>
        public static string GetTipoEventoSeminarioPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/seminareventtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de evento como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/eventtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>OrganizaTipoEventocion</returns>
        public static string GetTipoEventoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/eventtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de evento como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/eventinscriptiontype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>OrganizaTipoEventocion</returns>
        public static string GetTipoInscripcionEventoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/eventinscriptiontype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de evento como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/seminarinscriptiontype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>OrganizaTipoEventocion</returns>
        public static string GetTipoInscripcionSeminarioPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/seminarinscriptiontype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de intervención como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/eventinscriptiontype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoIntervencionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/eventinscriptiontype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la firma del del <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetFirmaAutor(this CvnItemBeanCvnAuthorBean item)
        {
            if (item == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(item.Signature))
            {
                return item.Signature;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el orden del del <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetOrdenAutor(this CvnItemBeanCvnAuthorBean item)
        {
            if (item == null)
            {
                return null;
            }
            return item.SignatureOrder.ToString();
        }

        /// <summary>
        /// Devuelve el nombre del <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetNombreAutor(this CvnItemBeanCvnAuthorBean item)
        {
            if (item == null) { return null; }
            if (!string.IsNullOrEmpty(item.GivenName))
            {
                return item.GivenName;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el primer apellido del <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetPrimerApellidoAutor(this CvnItemBeanCvnAuthorBean item)
        {
            if (item == null) { return null; }
            if (item.CvnFamilyNameBean != null)
            {
                return item.CvnFamilyNameBean.FirstFamilyName;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el segundo apellido del <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetSegundoApellidoAutor(this CvnItemBeanCvnAuthorBean item)
        {
            if (item == null) { return null; }
            if (item.CvnFamilyNameBean != null)
            {
                return item.CvnFamilyNameBean.SecondFamilyName;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el nombre del EntityBean con código <paramref name="codigo"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Name</returns>
        public static string GetNameEntityBeanPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnEntityBean campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnEntityBean).Cast<CvnItemBeanCvnEntityBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.Name;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el nombre del TitleBean con código <paramref name="codigo"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetNameTitleBeanPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnTitleBean campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnTitleBean).Cast<CvnItemBeanCvnTitleBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.Name;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el identificador del TitleBean con código <paramref name="codigo"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetIdentificationTitleBeanPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnTitleBean campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnTitleBean).Cast<CvnItemBeanCvnTitleBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.Identification;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el objetivo como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/staygoal_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Name</returns>
        public static string GetObjetivoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/staygoal_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la modalidad de la actividad como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/activitymodality_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>ModalidadActividad</returns>
        public static string GetModalidadActividadPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/activitymodality_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la referencia del programa de doctorado como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/doctoralprogramtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string ReferenciaProgramaDoctorado(this CvnItemBeanCvnTitleBean item)
        {
            if (!string.IsNullOrEmpty(item.Identification))
            {
                return mResourceApi.GraphsUrl + "items/doctoralprogramtype_" + item.Identification;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el sistema de actividad como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/accesssystemactivity_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>SistemaActividad</returns>
        public static string GetSistemaActividadPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/accesssystemactivity_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la finalidad como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/grantaim_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Finalidad</returns>
        public static string GetFinalidadPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/grantaim_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de relacion como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/relationshiptype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>TipoRelacion</returns>
        public static string GetRelacionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/relationshiptype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la region geografica como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/geographicregion_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>RegionGeografica</returns>
        public static string GetGeographicRegionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/geographicregion_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de convoatoria como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/calltype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoConvocatoriaPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/calltype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de evaluación como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/evaluationtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoEvaluacionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/evaluationtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve los creditos ECTS como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/hourscreditsectstype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetHorasCreditosECTSPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/hourscreditsectstype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de curso como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/coursetype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoCursoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/coursetype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de modalidad de docencia como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/modalityteachingtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoDocenciaModalidadPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/modalityteachingtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de programa como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/programtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoProgramaPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/programtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de oficialidad de la docencia como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/teachingtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoDocenciaOficialidadPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Length != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/teachingtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el valor de la fotografía digital,
        /// en formato imagen en base64
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Imagen en base64</returns>
        public static string GetImagenPorIDCampo(this List<CvnItemBean> listado, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
                CvnItemBeanCvnPhotoBean campo = listadoCamposAux?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnPhotoBean).Cast<CvnItemBeanCvnPhotoBean>().FirstOrDefault();
                if (campo != null)
                {
                    string imagenString = $@"data:image/{campo.MimeType};base64,{campo.BytesInBase64}";
                    return imagenString;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Devuelve el Volume del CvnItemBeanCvnVolumeBean con codigo igual a <paramref name="codigo"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns>Volumen del CvnItemBeanCvnVolumeBean</returns>
        public static string GetVolumenPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Length != 15) { return null; }

            CvnItemBeanCvnVolumeBean campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnVolumeBean).Cast<CvnItemBeanCvnVolumeBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.Volume;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el Number del CvnItemBeanCvnVolumeBean con codigo igual a <paramref name="codigo"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns>Number del CvnItemBeanCvnVolumeBean</returns>
        public static string GetNumeroVolumenPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Length != 15) { return null; }

            CvnItemBeanCvnVolumeBean campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnVolumeBean).Cast<CvnItemBeanCvnVolumeBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.Number;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la página inicial del CvnItemBeanCvnPageBean
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns>Página inicial del CvnItemBeanCvnPageBean</returns>
        public static string GetPaginaInicialPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Length != 15) { return null; }

            CvnItemBeanCvnPageBean campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnPageBean).Cast<CvnItemBeanCvnPageBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.InitialPage;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la página final del CvnItemBeanCvnPageBean
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns>Página final del CvnItemBeanCvnPageBean</returns>
        public static string GetPaginaFinalPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Length != 15) { return null; }

            CvnItemBeanCvnPageBean campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnPageBean).Cast<CvnItemBeanCvnPageBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.FinalPage;
            }
            return null;
        }

        /// <summary>
        /// Dado un listado de identificadores (CvnItemBeanCvnExternalPKBean)
        /// devuelve el valor de ORCID. 
        /// </summary>
        /// <param name="listado"></param>
        /// <returns>ORCID</returns>
        public static string GetORCID(this List<CvnItemBeanCvnExternalPKBean> listado)
        {
            return listado.Where(x => x.Type.Equals("140")).FirstOrDefault()?.Value;
        }

        /// <summary>
        /// Dado un listado de identificadores (CvnItemBeanCvnExternalPKBean)
        /// devuelve el valor de Scopus
        /// </summary>
        /// <param name="listado"></param>
        /// <returns>Scopus</returns>
        public static string GetScopus(this List<CvnItemBeanCvnExternalPKBean> listado)
        {
            return listado.Where(x => x.Type.Equals("150")).FirstOrDefault()?.Value;
        }

        /// <summary>
        /// Dado un listado de identificadores (CvnItemBeanCvnExternalPKBean)
        /// devuelve el valor de ResearcherID
        /// </summary>
        /// <param name="listado"></param>
        /// <returns>ResearcherID</returns>
        public static string GetResearcherID(this List<CvnItemBeanCvnExternalPKBean> listado)
        {
            return listado.Where(x => x.Type.Equals("160")).FirstOrDefault()?.Value;
        }

        public static Person GetNombrePersonaCV(string pCVID)
        {
            Person persona = new Person();
            try
            {
                string select = $@"select distinct ?person ?nombre ?orcid from <http://gnoss.com/person.owl>";
                string where = $@" where {{
                                    ?s <http://w3id.org/roh/cvOf> ?person .
                                    ?person <http://w3id.org/roh/ORCID> ?orcid .
                                    ?person <http://xmlns.com/foaf/0.1/name> ?nombre .
                                    FILTER(?s=<{pCVID}>)
                                }}";
                SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    persona.name = new Name();
                    persona.name.nombre_completo = new List<string>() { fila["nombre"].value };
                    persona.ORCID = fila["orcid"].value;
                }
                return persona;
            }
            catch (Exception e)
            {
                mResourceApi.Log.Error(e.Message);
            }
            return persona;
        }

        public static string DatetimeFE(string dateTime)
        {
            if (string.IsNullOrEmpty(dateTime))
            {
                return null;
            }

            try
            {
                //Creo un datetime, en formato UTC, sin especificar el Kind y le indico que lo convierta a horario de España.
                int anio = int.Parse(dateTime.Split("-").ElementAt(0));
                int mes = int.Parse(dateTime.Split("-").ElementAt(1));
                int dia = int.Parse(dateTime.Split("-").ElementAt(2));

                DateTime datetimeAux = new DateTime(anio, mes, dia);

                DateTime dateTime2 = new DateTime(datetimeAux.Ticks, DateTimeKind.Unspecified);

                if (TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id.Contains("Europe/Madrid")))
                {
                    dateTime2 = TimeZoneInfo.ConvertTime(dateTime2, TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid"));
                }


                return dateTime2.ToString("yyyyMMdd000000");
            }
            catch (Exception e)
            {
                mResourceApi.Log.Error("Error en el formato de fecha" + e.Message + " " + e.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Devuelve un string en formato de fecha de GNOSS
        /// YYYYMMDD000000
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>YYYYMMDD000000</returns>
        public static string DatetimeStringGNOSS(this CvnItemBeanCvnDateDayMonthYear dateTime)
        {
            try
            {
                //Creo un datetime, en formato UTC, sin especificar el Kind y le indico que lo convierta a horario de España.
                DateTime dateTime2 = new DateTime(dateTime.Value.Ticks, DateTimeKind.Unspecified);

                if (TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id.Contains("Europe/Madrid")))
                {
                    dateTime2 = TimeZoneInfo.ConvertTime(dateTime2, TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid"));
                }


                return dateTime2.ToString("yyyyMMdd000000");
            }
            catch (Exception e)
            {
                mResourceApi.Log.Error("Error en el formato de fecha" + e.Message + " " + e.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Devuelve un string en formato de fecha de GNOSS
        /// YYYYMMDD000000
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>YYYYMMDD000000</returns>
        public static string DatetimeStringGNOSS(this CvnItemBeanCvnDateMonthYear dateTime)
        {
            //Creo un datetime, en formato UTC, sin especificar el Kind y le indico que lo convierta a horario de España.
            DateTime dateTime2 = new DateTime(dateTime.Value.Ticks, DateTimeKind.Unspecified);

            if (TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id.Contains("Europe/Madrid")))
            {
                dateTime2 = TimeZoneInfo.ConvertTime(dateTime2, TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid"));
            }

            return dateTime2.ToString("yyyyMMdd000000");
        }

        /// <summary>
        /// Devuelve un string en formato de fecha de GNOSS
        /// YYYYMMDD000000
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>YYYYMMDD000000</returns>
        public static string DatetimeStringGNOSS(this CvnItemBeanCvnDateYear dateTime)
        {
            //Creo un datetime, en formato UTC, sin especificar el Kind y le indico que lo convierta a horario de España.
            DateTime dateTime2 = new DateTime(dateTime.Value.Ticks, DateTimeKind.Unspecified);

            if (TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id.Contains("Europe/Madrid")))
            {
                dateTime2 = TimeZoneInfo.ConvertTime(dateTime2, TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid"));
            }


            return dateTime2.ToString("yyyyMMdd000000");
        }

        /// <summary>
        /// Devuelve el valor del CvnString con codigo igual a <paramref name="codigo"/>
        /// </summary>
        /// <param name="codeGroup"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetStringCvnCodeGroup(this CvnItemBeanCvnCodeGroup codeGroup, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                CvnItemBeanCvnCodeGroupCvnString campo = codeGroup.CvnString?.Where(x => x.Code.Equals(codigo)).FirstOrDefault();
                if (campo != null)
                {
                    return campo.Value;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Devuelve el pais, de un CvnItemBeanCvnCodeGroup, como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_PCLD_" + valor
        /// </summary>
        /// <param name="codeGroup"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetPaisPorIDCampo(this CvnItemBeanCvnCodeGroup codeGroup, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnCodeGroupCvnString campo = codeGroup.CvnString?.Where(x => x.Code.Equals(codigo)).FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/feature_PCLD_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la región como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_ADM1_" + valor
        /// </summary>
        /// <param name="codeGroup"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetRegionPorIDCampo(this CvnItemBeanCvnCodeGroup codeGroup, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnCodeGroupCvnString campo = codeGroup.CvnString?.Where(x => x.Code.Equals(codigo)).FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/feature_ADM1_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de grado universitario como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/universitydegreetype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoGradoUniversitarioPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/universitydegreetype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la nota media como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/qualificationtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetNotaMediaPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/qualificationtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de premio como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/prizetype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetPremioPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/prizetype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelvel la palabra clave como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/tesauro_cvn_" + valor
        /// </summary>
        /// <param name="mResourceApi"></param>
        /// <param name="palabra"></param>
        /// <returns></returns>
        public static string ObtenerPalabraClave(ResourceApi mResourceApi, string palabra)
        {
            if (palabra == null) { return null; }

            return mResourceApi.GraphsUrl + "items/tesauro_cvn_" + palabra;
        }

        /// <summary>
        /// Devuelvel el tipo de formación como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/formationtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoFormacion(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/formationtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelvel el tipo de formación de actividad como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/formationactivitytype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoFormacionActividad(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/formationactivitytype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el Name perteneciente al CvnEntityBean con codigo igual a <paramref name="codigo"/>
        /// </summary>
        /// <param name="codeGroup"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetNameEntityBeanCvnCodeGroup(this CvnItemBeanCvnCodeGroup codeGroup, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }
                if (codeGroup.CvnEntityBean == null) { return null; }

                if ((bool)codeGroup.CvnEntityBean?.Code.Equals(codigo))
                {
                    return codeGroup.CvnEntityBean.Name;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Devuelvel el tipo de organización, que es parte de un CvnItemBeanCvnCodeGroup, como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/organizationtype_" + valor
        /// </summary>
        /// <param name="codeGroup"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetOrganizationCvnCodeGroup(this CvnItemBeanCvnCodeGroup codeGroup, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                CvnItemBeanCvnCodeGroupCvnString campo = codeGroup.CvnString?.Where(x => x.Code.Equals(codigo)).FirstOrDefault();
                if (campo != null && !string.IsNullOrEmpty(campo.Value))
                {
                    return mResourceApi.GraphsUrl + "items/organizationtype_" + campo.Value;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Devuelve el valor CvnBoolean del CvnItemBeanCvnCodeGroup 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetCvnBooleanCvnCodeGroup(this CvnItemBeanCvnCodeGroup item)
        {
            return item.CvnBoolean?.Value.ToString();
        }

        /// <summary>
        /// Devuelve el valor CvnDouble del CvnItemBeanCvnCodeGroup 
        /// con codigo igual a <paramref name="codigo"/> como string
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetCvnDoubleCvnCodeGroup(this CvnItemBeanCvnCodeGroup item, string codigo)
        {
            return item.CvnDouble?.Where(x => x.Code.Equals(codigo))?.Select(x => x.Value)?.FirstOrDefault().ToString();
        }

        /// <summary>
        /// Devuelve el Type del CvnItemBeanCvnExternalPKBean con codigo igual a <paramref name="codigo"/>
        /// </summary>
        /// <param name="cvnItemBean"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTypeCvnExternalPKBean(this CvnItemBean cvnItemBean, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvnItemBean.Items?.ToList();
                CvnItemBeanCvnExternalPKBean campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnExternalPKBean).Cast<CvnItemBeanCvnExternalPKBean>().FirstOrDefault();
                if (campo != null)
                {
                    return campo.Type;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Devuelve el Others del CvnItemBeanCvnExternalPKBean con codigo igual a <paramref name="codigo"/>
        /// </summary>
        /// <param name="cvnItemBean"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetOthersCvnExternalPKBean(this CvnItemBean cvnItemBean, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvnItemBean.Items?.ToList();
                CvnItemBeanCvnExternalPKBean campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnExternalPKBean).Cast<CvnItemBeanCvnExternalPKBean>()?.FirstOrDefault();
                if (campo != null)
                {
                    return campo.Others;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Devuelve el Value del CvnItemBeanCvnExternalPKBean con codigo igual a <paramref name="codigo"/>
        /// </summary>
        /// <param name="cvnItemBean"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetValueCvnExternalPKBean(this CvnItemBean cvnItemBean, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvnItemBean.Items?.ToList();
                CvnItemBeanCvnExternalPKBean campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnExternalPKBean).Cast<CvnItemBeanCvnExternalPKBean>()?.FirstOrDefault();
                if (campo != null)
                {
                    return campo.Value;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine("ArgumentException: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Devuelve el tipo de soporte como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/documentformat_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetFormatoDocumentoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/documentformat_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de soporte como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/supporttype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoSoportePorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/supporttype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de publicación como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/publicationtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoPublicacionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/publicationtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de modalidad de contrato como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/contractmodality_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetModalidadContrato(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/contractmodality_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de regimen de dedicación como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/dedicationregime_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetRegimenDedicacion(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/dedicationregime_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el codigo UNESCO como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/unesco_" + valor
        /// </summary>
        /// <param name="codigolista">codigolista</param>
        /// <returns></returns>
        public static string GetCodUnescoIDCampo(string codigolista)
        {
            return mResourceApi.GraphsUrl + "items/unesco_" + codigolista;
        }

        /// <summary>
        /// Devuelve el ambito de gratión como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/scopemanagementactivity_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetAmbitoGestion(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/scopemanagementactivity_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el grado de contribución del documento como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/contributiongradedocument_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetGradoContribucionDocumentoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/contributiongradedocument_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el grado de contribución del documento como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/contributiongradeproyect_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetGradoContribucionProyectoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/contributiongradeproject_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de colaboración en un grupo como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/colaborationtypegroup_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoColaboracionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/colaborationtypegroup_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de propiedad industrial como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/industrialpropertytype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoPropiedadIndustrialPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/industrialpropertytype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de participación de actividad como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/participationtypeactivity_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoParticipacionActividadPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/participationtypeactivity_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de participación como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/participationtypedocument_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoParticipacionDocumentoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/participationtypedocument_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de participación como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/participationtypeproject_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoParticipacionProyectoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/participationtypeproject_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de duración laboral como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/laboraldurationtype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoDuracionLaboralPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/laboraldurationtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la modalidad del proyecto como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/projectmodality_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetModalidadProyectoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/projectmodality_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de proyecto como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/projecttype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoProyectoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/projecttype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de proyecto como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/projectcharactertype_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoProyectoCharacterPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/projectcharactertype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de tipología de gestión como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/managementtypeactivity_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoTipologiaGestionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/managementtypeactivity_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de perfil de grupo como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/targetgroupprofile_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTipoPerfilGrupoPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/targetgroupprofile_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el Indice H  como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/hindexsource_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetIndiceH(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/hindexsource_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el idioma como respuesta, Identification de un CvnItemBeanCvnTitleBean,
        /// con formato mResourceApi.GraphsUrl + "items/language_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetTraduccion(this CvnItemBeanCvnTitleBean item)
        {
            if (!string.IsNullOrEmpty(item.Identification))
            {
                return mResourceApi.GraphsUrl + "items/language_" + item.Identification;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la traducción como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/language_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetTraduccion(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null && !string.IsNullOrEmpty(campo.Value))
            {
                return mResourceApi.GraphsUrl + "items/language_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el nivel del idioma como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/languagelevel_" + valor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetNivelLenguaje(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return mResourceApi.GraphsUrl + "items/languagelevel_" + value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el código unesco como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/unesco_" + valor
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetCodUnescoIDCampo(this CvnItemBeanCvnString item)
        {
            return mResourceApi.GraphsUrl + "items/unesco_" + item.Value;
        }

        /// <summary>
        /// Devuelve el codigo internacional del PhoneBean
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetCodInternacional(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnPhoneBean campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnPhoneBean).Cast<CvnItemBeanCvnPhoneBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.InternationalCode;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el nº de telefono del PhoneBean
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetNumeroTelefono(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnPhoneBean campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnPhoneBean).Cast<CvnItemBeanCvnPhoneBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.Number;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la extension del PhoneBean
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetExtensionTelefono(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnPhoneBean campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnPhoneBean).Cast<CvnItemBeanCvnPhoneBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.Extension;
            }
            return null;
        }

        /// <summary>
        /// Devuelve un listado con los valores de los codigos UNESCO.
        /// </summary>
        /// <param name="item">item</param>
        /// <returns>List<string></returns>
        public static List<string> GetPadresCodUnesco(this CvnItemBeanCvnString item)
        {
            if (item.Value.Length != 6) { return null; }

            List<string> listadoCodigos = new List<string>();
            string codigo = item.Value;

            if (Regex.Match(codigo, "^\\d{2}0000$").Success)
            {
                listadoCodigos.Add(codigo);
                return listadoCodigos;
            }
            else if (Regex.Match(codigo, "^\\d{4}00$").Success)
            {
                listadoCodigos.Add(codigo);
                string codigo0000 = codigo.Substring(0, 2) + "0000";
                listadoCodigos.Add(codigo0000);
                return listadoCodigos;
            }
            else
            {
                listadoCodigos.Add(codigo);
                string codigo00 = codigo.Substring(0, 4) + "00";
                string codigo0000 = codigo.Substring(0, 2) + "0000";
                listadoCodigos.Add(codigo00);
                listadoCodigos.Add(codigo0000);
                return listadoCodigos;
            }
        }

        /// <summary>
        /// Devuelve un listado con los valores del tesauro de las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<string> GetPadresPalabrasClave(this CvnItemBeanCvnString item)
        {
            if (item.Value.Length != 24) { return null; }

            Dictionary<string, string> palabrasClave = UtilitySecciones.PalabrasClave(mResourceApi);
            List<string> listadoCodigos = new List<string>();
            string codigo = item.Value;

            string padre = palabrasClave[codigo];
            while (padre != null)
            {
                listadoCodigos.Add(codigo);
                codigo = padre;
                padre = palabrasClave[codigo];
            }
            listadoCodigos.Add(codigo);

            return listadoCodigos;
        }

        /// <summary>
        /// Añade la referencia a la entidad <paramref name="propiedadNombreTitulacion"/> si esta se encuentra en BBDD.
        /// </summary>
        /// <param name="mResourceApi"></param>
        /// <param name="nombreTitulacion"></param>
        /// <param name="propiedadNombreTitulacion"></param>
        /// <param name="propiedadTitulacion"></param>
        /// <param name="entidadAux"></param>
        public static void AniadirTitulacion(ResourceApi mResourceApi, CvnItemBeanCvnTitleBean titulacion, string propiedadNombreTitulacion, string propiedadTitulacion, Entity entidadAux)
        {
            if (mResourceApi == null || titulacion == null ||
                   string.IsNullOrEmpty(propiedadTitulacion) || string.IsNullOrEmpty(propiedadTitulacion))
            { return; }

            if (!string.IsNullOrEmpty(titulacion.Identification))
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(propiedadNombreTitulacion, titulacion.Name),
                    new Property(propiedadTitulacion, mResourceApi.GraphsUrl + "items/degreetype_" + titulacion.Identification)
                ));
            }
            else
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                       new Property(propiedadNombreTitulacion, titulacion.Name)
                ));
                entidadAux.properties.Add(new Property(propiedadTitulacion, ""));
            }
        }


    }
}
