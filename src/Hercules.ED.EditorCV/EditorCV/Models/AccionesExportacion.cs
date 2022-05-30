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
        /// <param name="pCVID">Id del CV</param>
        /// <param name="result">archivo pdf a guardar</param>
        public static void AddFile(ConfigService _Configuracion, string pCVID, string lang, List<string> listaId)
        {
            Guid guidCortoCVID = mResourceApi.GetShortGuid(pCVID);

            //TODO eliminar
            goto Testing;
            //Añado GeneratedPDFFile sin el link al archivo
            string filePredicateTitle = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/title";
            string filePredicateFecha = "http://w3id.org/roh/generatedPDFFile|http://purl.org/dc/terms/issued";

            string idEntityAux = $"{mResourceApi.GraphsUrl}items/GeneratedPDFFile_" + guidCortoCVID.ToString() + "_" + Guid.NewGuid();

            //TODO
            string PDFFileTitle = "prueba.pdf";//resp.filename;
            string PDFFileFecha = DateTime.UtcNow.ToString("yyyyMMddHHmmss");


            List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
            TriplesToInclude trTitle = new TriplesToInclude(idEntityAux + "|" + PDFFileTitle, filePredicateTitle);
            listaTriples.Add(trTitle);
            TriplesToInclude trFecha = new TriplesToInclude(idEntityAux + "|" + PDFFileFecha, filePredicateFecha);
            listaTriples.Add(trFecha);

            var inserted = mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { guidCortoCVID, listaTriples } });

        Testing:
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
            string urlExportador = _Configuracion.GetUrlExportador();
            HttpResponseMessage response = client.PostAsync($"{urlExportador}", formContent).Result;
            response.EnsureSuccessStatusCode();
            byte[] result = response.Content.ReadAsByteArrayAsync().Result;

            //TODO
            goto TestingFin;

            string filePredicate = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/filePDF";
            //TODO
            string fileName = "http://gnoss.com/items/GeneratedPDFFile_44adc517-91fd-40fb-b90e-54f3e3c7c0a0_1fcb4fa6-7ba3-4849-9ba3-be58e1693be8|prueba.pdf";
            //string fileName = ""+resp.filename;
            List<byte[]> attachedFile = new List<byte[]>();
            attachedFile.Add(result);

            //TODO
            //Añado el fichero en virtuoso
            mResourceApi.AttachFileToResource(guidCortoCVID, filePredicate, fileName,
                new List<string>() { "filePDF" }, new List<short>() { 0 }, attachedFile);
        //new List<string>() { resp.filename }, new List<short>() { 0 }, new List<byte[]>() { resp.dataHandler });
        TestingFin:;
        }

        /// <summary>
        /// Devuelve todas las pestañas del CV de <paramref name="pCVId"/>
        /// </summary>
        /// <param name="pCVId"></param>
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
