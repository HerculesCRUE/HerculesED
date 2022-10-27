using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.ImportExportCV.Sincro.Secciones;
using Hercules.ED.LoadCV.Config;
using Hercules.ED.LoadCV.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hercules.ED.LoadCV
{
    public static class CargaCV
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");

        /// <summary>
        /// Recorremos los ficheros.
        /// Comprobamos si existe la persona del DNI (nombre del fichero) y si esta es investigadora.
        /// Se hace una llamada para pedir el ORCID del archivo.
        /// Si existe el ORCID en la persona se actualiza, sino se añade el ORCID. 
        /// </summary>
        internal static void CargaORCID(ConfigService _Configuracion)
        {
            try
            {
                List<string> listadoArchivos = Directory.GetFiles(_Configuracion.GetRutaCarpeta()).ToList();
                foreach (string archivo in listadoArchivos)
                {
                    try
                    {
                        string nombreArchivo = archivo.Split("\\").Last();
                        //Si no existe el investigador continuo con el siguiente.
                        if (!ExisteInvestigadorActivo(nombreArchivo.Split(".").First()))
                        {
                            continue;
                        }

                        string urlEstado = _Configuracion.GetUrlImportadorExportador() + "/ObtenerORCID";

                        MultipartFormDataContent multipartFormData = new MultipartFormDataContent();

                        byte[] text = File.ReadAllBytes(_Configuracion.GetRutaCarpeta() + Path.DirectorySeparatorChar + nombreArchivo);
                        multipartFormData.Add(new ByteArrayContent(text), "File", nombreArchivo);

                        HttpClient httpClient = new HttpClient();
                        HttpResponseMessage responseMessage = httpClient.PostAsync($"{urlEstado}", multipartFormData).Result;

                        if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            mResourceApi.Log.Error(responseMessage.Content.ReadAsStringAsync().Result);
                            continue;
                        }

                        string ORCID = responseMessage.Content.ReadAsStringAsync().Result;
                        if (!ComprobarORCID(ORCID))
                        {
                            mResourceApi.Log.Info("Archivo: " + nombreArchivo + ", Resultado: Formato de ORCID invalido");
                            continue;
                        }

                        Dictionary<string, string> dicPersonaORCID = InvestigadorConORCID(nombreArchivo.Split(".").First());
                        string idPersona = dicPersonaORCID.First().Key;

                        //Si el investigador NO tiene ORCID lo inserto
                        if (string.IsNullOrEmpty(dicPersonaORCID[idPersona]))
                        {
                            SincroORCID.InsertaORCIDPersona(idPersona, ORCID, mResourceApi);

                            mResourceApi.Log.Info("Archivo: " + nombreArchivo + ", Resultado: Se ha insertado el ORCID");
                        }
                        //Si el investigador tiene ORCID se actualiza
                        else
                        {
                            SincroORCID.ActualizaORCIDPersona(idPersona, dicPersonaORCID[idPersona], ORCID, mResourceApi);

                            mResourceApi.Log.Info("Archivo: " + nombreArchivo + ", Resultado: Se ha actualizado el ORCID");
                        }

                    }
                    catch (Exception ex)
                    {
                        mResourceApi.Log.Error(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Recorremos los ficheros.
        /// Comprobamos, por cada uno, si existe la persona (nombre del archivo) con ese DNI en base de datos, es investigadora y tiene CV.
        /// Se llama al servicio de importación de CV, pasandole el archivo y el identificador de CV.
        /// </summary>
        internal static void CargarCV(ConfigService _Configuracion)
        {
            try
            {
                List<string> listadoArchivos = Directory.GetFiles(_Configuracion.GetRutaCarpeta()).ToList();
                foreach (string archivo in listadoArchivos)
                {
                    try
                    {
                        string nombreArchivo = archivo.Split("\\").Last();
                        string CVID = CVInvestigadorActivo(nombreArchivo.Split(".").First());

                        //Si no existe el CV de la persona continuo con el siguiente archivo.
                        if (string.IsNullOrEmpty(CVID))
                        {
                            continue;
                        }
                        string urlEstado = _Configuracion.GetUrlImportadorExportador() + "/Importar";

                        MultipartFormDataContent multipartFormData = new MultipartFormDataContent();
                        multipartFormData.Add(new StringContent(CVID), "pCVID");

                        MemoryStream ms = new MemoryStream();
                        byte[] text = File.ReadAllBytes(_Configuracion.GetRutaCarpeta() + Path.DirectorySeparatorChar + nombreArchivo);
                        multipartFormData.Add(new ByteArrayContent(text), "File", nombreArchivo);

                        HttpClient httpClient = new HttpClient();
                        httpClient.Timeout = new TimeSpan(1, 15, 0);

                        HttpResponseMessage responseMessage = httpClient.PostAsync($"{urlEstado}", multipartFormData).Result;
                        mResourceApi.Log.Info("Archivo: " + nombreArchivo + ", Resultado: " + responseMessage.StatusCode);

                    }
                    catch (Exception ex)
                    {
                        mResourceApi.Log.Error(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Devuelve true si el formato de <paramref name="ORCID"/> es correcto.
        /// </summary>
        /// <param name="ORCID">ORCID</param>
        /// <returns>True si el formato es correcto para un ORCID</returns>
        public static bool ComprobarORCID(string ORCID)
        {
            //Compruebo que no sea nulo
            if (string.IsNullOrEmpty(ORCID))
            {
                return false;
            }

            //Compruebo si tiene formato DNI
            if (Regex.IsMatch(ORCID, "\\d{4}-\\d{4}-\\d{4}-\\d{4}"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Dado un crisIdentifier, indica si está bien formado
        /// </summary>
        /// <param name="crisID">crisIdentifier</param>
        /// <returns>True si tiene formato valido</returns>
        private static bool ValidarCrisID(string crisID)
        {
            //Compruebo que no sea nulo
            if (string.IsNullOrEmpty(crisID))
            {
                return false;
            }

            //Compruebo si tiene formato DNI
            if (Regex.IsMatch(crisID, "\\d{8}[TRWAGMYFPDXBNJZSQVHLCKE]"))
            {
                return true;
            }

            //Compruebo si tiene formato NIE
            if (Regex.IsMatch(crisID, "[XYZ]\\d{7}[TRWAGMYFPDXBNJZSQVHLCKE]"))
            {
                return true;
            }

            //Compruebo si tiene formato Pasaporte
            if (Regex.IsMatch(crisID, "\\w{3}\\d{6}\\w*"))
            {
                return true;
            }


            return false;
        }

        /// <summary>
        /// Dado el crisIdentifier devuelve el id de la persona en BBDD y el orcid en caso de que exista
        /// </summary>
        /// <param name="crisIdentifier">crisIdentifier de la persona</param>
        /// <returns>idPersona y ORCID</returns>
        private static Dictionary<string, string> InvestigadorConORCID(string crisIdentifier)
        {
            Dictionary<string, string> dicPersonaORCID = new Dictionary<string, string>();
            string select = "SELECT distinct ?person ?orcid";
            string where = $@"WHERE{{
                                ?person a <http://xmlns.com/foaf/0.1/Person> .
                                ?person <http://w3id.org/roh/crisIdentifier> '{crisIdentifier.Substring(0, crisIdentifier.Length - 1)}' .
                                ?person <http://w3id.org/roh/isActive> 'true' .
                                OPTIONAL{{ ?person <http://w3id.org/roh/ORCID> ?orcid }}
                            }}";
            SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string>() { "person", "curriculumvitae" });
            foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
            {
                if (fila.ContainsKey("person") && fila.ContainsKey("orcid"))
                {
                    dicPersonaORCID.Add(fila["person"].value, fila["orcid"].value);
                    return dicPersonaORCID;
                }
                if (fila.ContainsKey("person"))
                {
                    dicPersonaORCID.Add(fila["person"].value, "");
                    return dicPersonaORCID;
                }
            }

            return dicPersonaORCID;
        }

        /// <summary>
        /// Dado el crisIdentifier devuleve si existe el investigador en BBDD, y está activo
        /// </summary>
        /// <param name="crisIdentifier">crisIdentifier de la persona</param>
        /// <returns>true si el investigador existe en BBDD</returns>
        private static bool ExisteInvestigadorActivo(string crisIdentifier)
        {
            if (!ValidarCrisID(crisIdentifier))
            {
                return false;
            }

            string select = "SELECT distinct ?person";
            string where = $@"WHERE{{
                                ?person a <http://xmlns.com/foaf/0.1/Person> .
                                ?person <http://w3id.org/roh/crisIdentifier> '{crisIdentifier.Substring(0, crisIdentifier.Length - 1)}' .
                                ?person <http://w3id.org/roh/isActive> 'true' .
                            }}";
            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Dado el crisIdentifier devuelve el identificador del CV.
        /// </summary>
        /// <param name="crisIdentifier">crisIdentifier de la persona</param>
        /// <returns>Devuelve el identificador del CV</returns>
        private static string CVInvestigadorActivo(string crisIdentifier)
        {
            if (!ValidarCrisID(crisIdentifier))
            {
                return "";
            }

            string select = "SELECT distinct ?cv ?person";
            string where = $@"WHERE{{
                                ?cv <http://w3id.org/roh/cvOf> ?person .
                                ?person a <http://xmlns.com/foaf/0.1/Person> .
                                ?person <http://w3id.org/roh/crisIdentifier> '{crisIdentifier.Substring(0, crisIdentifier.Length - 1)}' .
                                ?person <http://w3id.org/roh/isActive> 'true' .
                            }}";
            SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string>() { "person", "curriculumvitae" });
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                return resultadoQuery.results.bindings.First()["cv"].value;
            }
            return "";
        }

    }
}
