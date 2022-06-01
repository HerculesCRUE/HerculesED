using EditorCV.Models.API;
using EditorCV.Models.API.Input;
using EditorCV.Models.API.Response;
using EditorCV.Models.Utils;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace EditorCV.Models
{
    public class AccionesExportacion
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        /// <summary>
        /// Añade el archivo enviado como array de bytes.
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="nombreCV"></param>
        /// <param name="pCVID"></param>
        /// <param name="lang"></param>
        /// <param name="listaId"></param>
        public static void AddFile(ConfigService _Configuracion, string pCVID, string nombreCV, string lang, List<string> listaId)
        {
            Guid guidCortoCVID = mResourceApi.GetShortGuid(pCVID);

            //Añado GeneratedPDFFile sin el link al archivo
            string filePredicateTitle = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/title";
            string filePredicateFecha = "http://w3id.org/roh/generatedPDFFile|http://purl.org/dc/terms/issued";
            string filePredicateEstado = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/status";

            string idEntityAux = $"{mResourceApi.GraphsUrl}items/GeneratedPDFFile_" + guidCortoCVID.ToString() + "_" + Guid.NewGuid();

            string PDFFilePDF = "CV_filePDF" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".pdf";
            string PDFFileFecha = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string PDFFileEstado = "pendiente";

            List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
            TriplesToInclude trTitle = new TriplesToInclude(idEntityAux + "|" + nombreCV, filePredicateTitle);
            listaTriples.Add(trTitle);
            TriplesToInclude trFecha = new TriplesToInclude(idEntityAux + "|" + PDFFileFecha, filePredicateFecha);
            listaTriples.Add(trFecha);
            TriplesToInclude trEstado = new TriplesToInclude(idEntityAux + "|" + PDFFileEstado, filePredicateEstado);
            listaTriples.Add(trEstado);

            var inserted = mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { guidCortoCVID, listaTriples } });
            string urlExportador = _Configuracion.GetUrlExportador();
            throw new Exception(urlExportador);
            Thread thread = new Thread(() => AddPDFFile(urlExportador, pCVID, lang, listaId, idEntityAux, PDFFilePDF, guidCortoCVID, filePredicateEstado));
            thread.Start();
        }

        /// <summary>
        /// Adjunto el fichero y modifico los triples de <paramref name="idEntityAux"/> para referenciar el archivo y 
        /// modificar el estado a "procesado". En caso de error durante el proceso cambio el estado a "error".
        /// </summary>
        /// <param name="urlExportador"></param>
        /// <param name="pCVID">Identificador del CV</param>
        /// <param name="lang">Lenguaje del CV</param>
        /// <param name="listaId">listado de identificadores</param>
        /// <param name="idEntityAux">Identificador de la entidad auxiliar a modificar</param>
        /// <param name="PDFFilePDF">nombre del fichero</param>
        /// <param name="guidCortoCVID">GUID corto del CV</param>
        /// <param name="filePredicateEstado">Predicado estado de la entidad</param>
        static void AddPDFFile(string urlExportador, string pCVID, string lang, List<string> listaId,
            string idEntityAux, string PDFFilePDF, Guid guidCortoCVID, string filePredicateEstado)
        {
            try
            {
                //Petición al exportador
                List<KeyValuePair<string, string>> parametros = new List<KeyValuePair<string, string>>();
                parametros.Add(new KeyValuePair<string, string>("pCVID", pCVID));
                parametros.Add(new KeyValuePair<string, string>("lang", lang));
                foreach (string id in listaId)
                {
                    parametros.Add(new KeyValuePair<string, string>("listaId", id));
                }
                FormUrlEncodedContent formContent = new FormUrlEncodedContent(parametros);

                //Petición al exportador para conseguir el archivo PDF
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(1, 15, 0);
                HttpResponseMessage response = client.PostAsync($"{urlExportador}", formContent).Result;
                response.EnsureSuccessStatusCode();
                byte[] result = response.Content.ReadAsByteArrayAsync().Result;

                //Inserto el archivo
                string filePredicate = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/filePDF";

                string fileName = idEntityAux + "|" + PDFFilePDF;
                List<byte[]> attachedFile = new List<byte[]>();
                attachedFile.Add(result);

                //Añado el fichero en virtuoso
                mResourceApi.AttachFileToResource(guidCortoCVID, filePredicate, fileName,
                    new List<string>() { PDFFilePDF }, new List<short>() { 0 }, attachedFile);

                //Cambio el estado a "procesado"
                string PDFFileEstado = "procesado";
                Dictionary<Guid, List<TriplesToModify>> triplesModificar = new Dictionary<Guid, List<TriplesToModify>>();
                triplesModificar[mResourceApi.GetShortGuid(pCVID)] = new List<TriplesToModify>()
                {
                    new TriplesToModify(idEntityAux + "|" + PDFFileEstado, idEntityAux + "|pendiente", filePredicateEstado)
                };
                mResourceApi.ModifyPropertiesLoadedResources(triplesModificar);

            }
            catch (Exception e)
            {
                //Cambio el estado a "error"
                string PDFFileEstado = "error";
                Dictionary<Guid, List<TriplesToModify>> triplesModificar = new Dictionary<Guid, List<TriplesToModify>>();
                triplesModificar[mResourceApi.GetShortGuid(pCVID)] = new List<TriplesToModify>()
                {
                    new TriplesToModify(idEntityAux + "|" + PDFFileEstado, idEntityAux + "|pendiente", filePredicateEstado)
                };
                mResourceApi.ModifyPropertiesLoadedResources(triplesModificar);
                mResourceApi.Log.Error("Error: " + e.Message + ". Traza:" + e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Devuelve el listado de ficheros PDF guardados.
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <returns></returns>
        public static List<FilePDF> GetListPDFFile(string pCVId)
        {
            List<FilePDF> listadoArchivos = new List<FilePDF>();
            string select = "SELECT ?titulo ?fecha ?estado ?fichero";
            string where = $@"WHERE{{
    <{pCVId}> <http://w3id.org/roh/generatedPDFFile> ?pdfFile .
    ?pdfFile <http://w3id.org/roh/title> ?titulo.
    OPTIONAL{{ ?pdfFile <http://purl.org/dc/terms/issued> ?fecha }}
    OPTIONAL{{ ?pdfFile <http://w3id.org/roh/status> ?estado }}
    OPTIONAL{{ ?pdfFile <http://w3id.org/roh/filePDF> ?fichero }}
}}group by ?pdfFile order by ?fecha";

            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (!fila.ContainsKey("titulo"))
                {
                    continue;
                }

                FilePDF file = new FilePDF();
                file.titulo = fila["titulo"].value;
                file.fecha = "";
                file.estado = "";
                file.fichero = "";
                if (fila.ContainsKey("fecha"))
                {
                    file.fecha = fila["fecha"].value;
                }
                if (fila.ContainsKey("estado"))
                {
                    file.estado = fila["estado"].value;
                }
                if (fila.ContainsKey("fichero"))
                {
                    string uri = "http://edma.gnoss.com/download-file?doc=" + mResourceApi.GetShortGuid(pCVId) + "&ext=.pdf&archivoAdjuntoSem="
                        + fila["fichero"].value.Split(".").First()
                        + "&ontologiaAdjuntoSem=88129721-ecf9-4ea3-afc6-db253f1cb480&ID=15ff250b-510d-4a08-b4a8-ac7526fbc53b&proy=b836078b-78a0-4939-b809-3f2ccf4e5c01&dscr=true";
                    file.fichero = uri;
                }

                listadoArchivos.Add(file);
            }

            return listadoArchivos;
        }

        /// <summary>
        /// Devuelve todas las pestañas del CV de <paramref name="pCVId"/>
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <returns></returns>
        public static ConcurrentDictionary<string, string> GetAllTabs(string pCVId)
        {
            ConcurrentDictionary<string, string> dicIds = new ConcurrentDictionary<string, string>();
            string select = "SELECT *";
            string where = $@"WHERE{{
    <{pCVId}> ?p ?o .
}}";

            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (!fila.ContainsKey("p") || !fila.ContainsKey("o"))
                {
                    continue;
                }
                if (!IsValidTab(fila["p"].value))
                {
                    continue;
                }

                string property = fila["p"].value.Split("/").Last();
                string uri = fila["p"].value.Split(property).First();
                dicIds.TryAdd(FirstLetterUpper(uri, property), fila["o"].value);
            }

            return dicIds;
        }

        /// <summary>
        /// Devuelve true si <paramref name="tab"/> se encuentra en:
        /// "http://w3id.org/roh/personalData",
        /// "http://w3id.org/roh/scientificExperience",
        /// "http://w3id.org/roh/scientificActivity",
        /// "http://w3id.org/roh/teachingExperience",
        /// "http://w3id.org/roh/qualifications",
        /// "http://w3id.org/roh/professionalSituation",
        /// "http://w3id.org/roh/freeTextSummary"
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        private static bool IsValidTab(string tab)
        {
            List<string> validTabs = new List<string>()
            {
                "http://w3id.org/roh/personalData",
                "http://w3id.org/roh/scientificExperience",
                "http://w3id.org/roh/scientificActivity",
                "http://w3id.org/roh/teachingExperience",
                "http://w3id.org/roh/qualifications",
                "http://w3id.org/roh/professionalSituation",
                "http://w3id.org/roh/freeTextSummary"
            };
            return validTabs.Contains(tab);
        }

        /// <summary>
        /// Cambia la 1º letra de <paramref name="property"/> a mayuscula y la concatena con <paramref name="uri"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static string FirstLetterUpper(string uri, string property)
        {
            if (property.Length == 0 || property.Length == 1)
            {
                return "";
            }
            string upper = property.Substring(0, 1).ToUpper();
            string substring = property.Substring(1, property.Length - 1);
            return uri + upper + substring;
        }
    }
}
