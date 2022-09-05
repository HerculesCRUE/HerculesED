using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.LoadCVs.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hercules.ED.LoadCVs
{
    internal class Program
    {
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/OAuthV3.config");

        private static ConfigService _Config;

        static void Main(string[] args)
        {
            _Config = new ConfigService();

            string directorioPendientes = $@"{_Config.GetRutaDatos()}\pending\";
            string directorioProcesados = $@"{_Config.GetRutaDatos()}\processed\";

            if (!Directory.Exists(directorioPendientes))
            {
                Directory.CreateDirectory(directorioPendientes);
            }
            if (!Directory.Exists(directorioProcesados))
            {
                Directory.CreateDirectory(directorioProcesados);
            }

            while (true)
            {
                foreach (string fichero in Directory.EnumerateFiles(directorioPendientes))
                {
                    // Obtención del ORCID.
                    string orcid = Path.GetFileName(fichero).Split(".").First();

                    // Obtención del ID GNOSS del CV.
                    string idCV = GetUserCV(orcid);

                    MultipartFormDataContent multipartFormData = new MultipartFormDataContent();
                    multipartFormData.Add(new ByteArrayContent(File.ReadAllBytes(fichero)), "File", Path.GetFileName(fichero));

                    string urlImportar = _Config.GetRutaImportador();

                    //Petición al exportador para conseguir el archivo PDF
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("pCVID", idCV);
                    client.Timeout = new TimeSpan(1, 15, 0);

                    HttpResponseMessage response = client.PostAsync($"{urlImportar}", multipartFormData).Result;
                    response.EnsureSuccessStatusCode();
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception(response.StatusCode.ToString() + " " + response.Content);
                    }

                    File.Copy(fichero, directorioProcesados + orcid + ".pdf");
                    File.Delete(fichero);
                }

                Thread.Sleep(1000);
            }
        }

        private static string GetUserCV(string pOrcid)
        {
            string select = $@"SELECT ?cv FROM <{mResourceApi.GraphsUrl}person.owl> FROM <{mResourceApi.GraphsUrl}curriculumvitae.owl> ";
            string where = $@"WHERE {{
                                ?persona a <http://xmlns.com/foaf/0.1/Person>. 
                                ?persona <http://w3id.org/roh/ORCID> '{pOrcid}'. 
                                ?cv a <http://w3id.org/roh/CV>. 
                                ?cv <http://w3id.org/roh/cvOf> ?persona. 
                                }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["cv"].value;
                }
            }

            return "";
        }
    }
}
