using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace Hercules.ED.UpdateKeywords
{
    public class UtilKeywords
    {
        private ResourceApi mResourceApi;
        private CommunityApi mCommunityApi;
        private Guid mIdComunidad;
        private static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/prefijos.json";
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));

        public UtilKeywords(ResourceApi pResourceApi, CommunityApi pCommunityApi)
        {
            this.mResourceApi = pResourceApi;
            this.mCommunityApi = pCommunityApi;
            this.mIdComunidad = mCommunityApi.GetCommunityId();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetDocument()
        {
            List<string> listaDocumentos = new List<string>();
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?s ");
            where.Append("WHERE { ");
            where.Append("?s a 'document'. ");
            where.Append("?s roh:getKeyWords 'true'. ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mIdComunidad);
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("s") && !string.IsNullOrEmpty(fila["s"].value))
                    {
                        listaDocumentos.Add(fila["s"].value);
                    }
                }
            }

            return listaDocumentos;
        }

        /// <summary>
        /// Obtiene el ID y Valor de la consulta a Mesh.
        /// </summary>
        /// <param name="pTopicalDescriptor">Tópico a consultar.</param>
        /// <returns>Diccionario resultante.</returns>
        public Dictionary<string, string> SelectDataMesh(string pTopicalDescriptor)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();

            // Endpoint.
            string urlConsulta = "https://id.nlm.nih.gov/mesh/sparql";

            // Consulta.
            string consulta = $@"
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
                PREFIX owl: <http://www.w3.org/2002/07/owl#>
                PREFIX meshv: <http://id.nlm.nih.gov/mesh/vocab#>
                PREFIX mesh: <http://id.nlm.nih.gov/mesh/>
                PREFIX mesh2022: <http://id.nlm.nih.gov/mesh/2022/>
                PREFIX mesh2021: <http://id.nlm.nih.gov/mesh/2021/>
                PREFIX mesh2020: <http://id.nlm.nih.gov/mesh/2020/>

                SELECT ?d ?label
                FROM <http://id.nlm.nih.gov/mesh>
                WHERE {{
                    ?d a meshv:TopicalDescriptor.
                    ?d rdfs:label ?label.
                    FILTER(REGEX(?label, '{pTopicalDescriptor}', 'i'))
                }}
                ORDER BY ?d
            ";

            // Tipo de salida.
            string format = "JSON";

            // Petición.
            WebClient webClient = new WebClient();
            SparqlObject resultadoQuery = JsonConvert.DeserializeObject<SparqlObject>(webClient.DownloadString($@"{urlConsulta}?query={HttpUtility.UrlEncode(consulta)}&format={format}"));

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Any())
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    string id = string.Empty;
                    string label = string.Empty;

                    if (fila.ContainsKey("d") && !string.IsNullOrEmpty(fila["d"].value))
                    {
                        id = fila["d"].value;
                        id = id.Substring(id.LastIndexOf("/") + 1);
                    }

                    if (fila.ContainsKey("label") && !string.IsNullOrEmpty(fila["label"].value))
                    {
                        label = fila["label"].value;
                    }

                    if(!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(label))
                    {
                        if (!dicResultados.ContainsKey(id))
                        {
                            dicResultados.Add(id, label);
                        }
                    }
                }
            }

            return dicResultados;
        }
    }
}
