using Gnoss.ApiWrapper;
using HerculesAplicacionConsola;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utils
{
    public static class Utility
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configOAuth\OAuthV3.config");

        /// <summary>
        /// Dada la ruta de un directorio devuelve si existe en el sistema
        /// </summary>
        /// <param name="path">Ruta del directorio</param>
        /// <returns>True si existe</returns>
        public static bool ExistePath(string path)
        {
            if (string.IsNullOrEmpty(path)) { return false; }
            return Directory.Exists(path);
        }

        /// <summary>
        /// Dada la ruta del archivo devuelve si existe
        /// </summary>
        /// <param name="rutaArchivo">Ruta del archivo</param>
        /// <returns>True si existe el archivo</returns>
        public static bool ExisteArchivo(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo)) { return false; }
            return (File.Exists(rutaArchivo));
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
        /// Dado un codigo y su longitud contando los puntos
        /// Devuelve si es formato del codigo es valido
        /// </summary>
        /// <param name="codigo">Codigo</param>
        /// <returns>True si el codigo es correcto</returns>
        public static bool CodigoCampoCorrecto(string codigo)
        {
            if (CodigoCorrecto(codigo))
            {
                return codigo.Count() == 15;
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
        /// <param name="cvn">cvnRootResultBean</param>
        /// <param name="codigo">Codigo</param>
        /// <returns>Lista de elementos de tipo <typeparamref name="T"/> de <paramref name="cvn"/></returns>
        public static List<T> GetListaElementosPorIDCampo<T>(this cvnRootResultBean cvn, string codigo) where T : CVNObject
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvn.cvnRootBean.SelectMany(x => x.Items).ToList();
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
        /// <typeparam name="T"></typeparam>
        /// <param name="cvn">cvnRootResultBean</param>
        /// <param name="codigo">Codigo</param>
        /// <returns> <typeparamref name="T"/> de <paramref name="cvn"/></returns>
        public static T GetElementoPorIDCampo<T>(this cvnRootResultBean cvn, string codigo) where T : CVNObject
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvn.cvnRootBean.SelectMany(x => x.Items)?.ToList();
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
        /// <param name="cvn"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnString que tenga ese codigo</returns>
        public static string GetStringPorIDCampo(this cvnRootResultBean cvn, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvn.cvnRootBean.SelectMany(x => x.Items)?.ToList();
                CvnItemBeanCvnString campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
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

            if (codigo.Count() != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return campo.Value;
            }
            return null;

        }

        /// <summary>
        /// Dado el codigo del campo, devuelve un string del elemento, 
        /// de tipo CvnItemBeanCvnDuration que tenga ese codigo.
        /// </summary>
        /// <param name="cvn"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnDuration que tenga ese codigo</returns>
        public static string GetDurationPorIDCampo(this cvnRootResultBean cvn, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvn.cvnRootBean.SelectMany(x => x.Items)?.ToList();
                CvnItemBeanCvnDuration campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDuration).Cast<CvnItemBeanCvnDuration>().FirstOrDefault();
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
        /// Dado el codigo del campo, devuelve un string del elemento, 
        /// de tipo CvnItemBeanCvnDuration que tenga ese codigo.
        /// </summary>
        /// <param name="cvnItemBean"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnDuration que tenga ese codigo</returns>
        public static string GetDurationPorIDCampo(this CvnItemBean cvnItemBean, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvnItemBean.Items?.ToList();
                CvnItemBeanCvnDuration campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDuration).Cast<CvnItemBeanCvnDuration>().FirstOrDefault();
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
        /// Dado el codigo del campo, devuelve un string del elemento, 
        /// de tipo CvnItemBeanCvnDuration que tenga ese codigo.
        /// </summary>
        /// <param name="listado"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>string del elemento de tipo CvnItemBeanCvnDuration que tenga ese codigo</returns>
        public static string GetDurationPorIDCampo(this List<CvnItemBean> listado, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            if (codigo.Count() != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnDuration campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDuration).Cast<CvnItemBeanCvnDuration>().FirstOrDefault();
            if (campo != null)
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

            if (codigo.Count() != 15) { return null; }
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

            if (codigo.Count() != 15) { return null; }
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
                    return aux.Substring(0, M - 1);
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

            if (codigo.Count() != 15) { return null; }
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

            if (codigo.Count() != 15) { return null; }
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
                if (T == -1)
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
                    return campo.Value.ToString(CultureInfo.InvariantCulture);
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
        /// Dado el codigo del campo, devuelve el valor DateTime del elemento, 
        /// de tipo CvnItemBeanCvnDateDayMonthYear que tenga ese codigo.
        /// </summary>
        /// <param name="cvn"></param>
        /// <param name="codigo">Codigo</param>
        /// <returns>DateTime del elemento de tipo CvnItemBeanCvnDateDayMonthYear que tenga ese codigo</returns>
        public static DateTime? GetDateTimePorIDCampo(this cvnRootResultBean cvn, string codigo)
        {
            try
            {
                if (!CodigoCampoCorrecto(codigo))
                {
                    throw new ArgumentException("Codigo de campo incorrecto" + codigo);
                }

                List<CVNObject> listadoCamposAux = cvn.cvnRootBean.SelectMany(x => x.Items)?.ToList();
                CvnItemBeanCvnDateDayMonthYear campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnDateDayMonthYear).Cast<CvnItemBeanCvnDateDayMonthYear>().FirstOrDefault();
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
            return null;

        }

        /// <summary>
        /// Devuelve la fecha en formato GNOSS (YYYYMMDD000000) como un string.
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
            return null;

        }

        /// <summary>
        /// Devuelve el genero,
        /// con formato mResourceApi.GraphsUrl + "items/gender_000"
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

            if (codigo.Count() != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/gender_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el pais como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_000"
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

            if (codigo.Count() != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/feature_PCLD_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el pais como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_000"
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
            if (codigo.Count() != 15) { return null; }

            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/feature_PCLD_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la region como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_ADM1_ES11"
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

            if (codigo.Count() != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/feature_ADM1_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la region como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_ADM1_ES11"
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
            if (codigo.Count() != 15) { return null; }

            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/feature_ADM1_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la provincia como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/feature_ADM2_000"
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

            if (codigo.Count() != 15) { return null; }
            if (listado == null) { return null; }
            IEnumerable<CVNObject> listadoCamposAux = listado.Where(x => x.Code.StartsWith(codigo.Substring(0, 11))).SelectMany(x => x.Items)?.ToList();
            CvnItemBeanCvnString campo = listadoCamposAux?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/feature_ADM2_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la organizacion como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/organizationtype_"
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

            if (codigo.Count() != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/organizationtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de evento como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/eventtype_"
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

            if (codigo.Count() != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/eventtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el nombre del EntityBean como respuesta
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

            if (codigo.Count() != 15) { return null; }
            CvnItemBeanCvnEntityBean campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnEntityBean).Cast<CvnItemBeanCvnEntityBean>().FirstOrDefault();
            if (campo != null)
            {
                return campo.Name;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el objetivo como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/items/staygoal_"
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

            if (codigo.Count() != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/staygoal_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la modalidad de la actividad como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/items/activitymodality_"
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

            if (codigo.Count() != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/activitymodality_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el sistema de actividad como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/items/accesssystemactivity_"
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

            if (codigo.Count() != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/accesssystemactivity_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la finalidad como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/items/grantaim_"
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

            if (codigo.Count() != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/grantaim_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve el tipo de relacion como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/items/organizationtype_"
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

            if (codigo.Count() != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/relationshiptype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// Devuelve la region geografica como respuesta,
        /// con formato mResourceApi.GraphsUrl + "items/geographicregion_000"
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

            if (codigo.Count() != 15) { return null; }
            CvnItemBeanCvnString campo = item.Items?.Where(x => x.Code.StartsWith(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/geographicregion_" + campo.Value;
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
        /// <returns></returns>
        public static string GetVolumenPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Count() != 15) { return null; }

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
        /// <returns></returns>
        public static string GetNumeroVolumenPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Count() != 15) { return null; }

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
        /// <returns></returns>
        public static string GetPaginaInicialPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Count() != 15) { return null; }

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
        /// <returns></returns>
        public static string GetPaginaFinalPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }
            if (codigo.Count() != 15) { return null; }

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

        /// <summary>
        /// Devuelve un string en formato de fecha de GNOSS
        /// YYYYMMDD000000
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>YYYYMMDD000000</returns>
        public static string DatetimeStringGNOSS(this CvnItemBeanCvnDateDayMonthYear dateTime)
        {
            string fechaString = dateTime.Value.ToString().Replace("-", "").Replace("T", "").Replace(":", "").Split("+")[0];
            string[] fechaAux = fechaString.Split("/");
            string anio = fechaAux[2].Split(" ")[0];

            fechaString = anio + fechaAux[1] + fechaAux[0] + "000000";
            return fechaString;
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
        /// feature_PCLD_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/feature_PCLD_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// feature_ADM1_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/feature_ADM1_" + campo.Value;
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
        /// organizationtype_
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
                if (campo != null)
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
        /// con codigo igual a <paramref name="codigo"/> como string
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetCvnBooleanCvnCodeGroup(this CvnItemBeanCvnCodeGroup item, string codigo)
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
        /// documentformat_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/documentformat_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// publicationtype_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/publicationtype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// contributiongradedocument_
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetGradoContribucionPorIDCampo(this CvnItemBean item, string codigo)
        {
            if (!CodigoCampoCorrecto(codigo))
            {
                throw new ArgumentException("Codigo de campo incorrecto" + codigo);
            }

            CvnItemBeanCvnString campo = item.Items.Where(x => x.Code.Equals(codigo) && x is CvnItemBeanCvnString).Cast<CvnItemBeanCvnString>().FirstOrDefault();
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/contributiongradedocument_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// colaborationtypegroup_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/colaborationtypegroup_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// industrialpropertytype_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/industrialpropertytype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// participationtypeactivity_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/participationtypeactivity_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// projectmodality_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/projectmodality_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// projecttype_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/projecttype_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// managementtypeactivity_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/managementtypeactivity_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// targetgroupprofile_
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
            if (campo != null)
            {
                return mResourceApi.GraphsUrl + "items/targetgroupprofile_" + campo.Value;
            }
            return null;
        }

        /// <summary>
        /// language_
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
        /// unesco_
        /// </summary>
        /// <param name="item"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static string GetCodUnescoIDCampo(this CvnItemBeanCvnString item)
        {
            return mResourceApi.GraphsUrl + "items/unesco_" + item.Value;
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
    }

}
