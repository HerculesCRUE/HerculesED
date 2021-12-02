using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using WoSConnect.ROs.WoS.Models;
using WoSConnect.ROs.WoS.Models.Inicial;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using System.IO;

namespace WoSConnect.ROs.WoS.Controllers
{
    public class ROWoSLogic : WoSInterface
    {
        protected string bareer;
        protected string baseUri { get; set; }

        public  Dictionary<string, string>  ds; //= LeerDatosExcel(@"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hércules-ED_Taxonomías_v1.2.xlsx");



//         private static Dictionary<string, Tuple<string, string, string, string>> LeerDatosExcel(string pRuta)

//         {
//             Console.Write("ESTOY LEYENDO 2?????????????????????????????????????????????????????");

//             // Lectura del Excel.

//             DataSet ds = new DataSet();

//             Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

//             using (var stream = File.Open(pRuta, FileMode.Open, FileAccess.Read))

//             {

//                 using (var reader = ExcelReaderFactory.CreateReader(stream))

//                 {

//                     ds = reader.AsDataSet(new ExcelDataSetConfiguration()

//                     {

//                         ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()

//                         {

//                             UseHeaderRow = true,

//                         }

//                     });

//                 }

//             }
//             Dictionary<string, Tuple<string, string, string, string>> name = new Dictionary<string, Tuple<string, string, string, string>>();

//             //List<Tuple<string, string, string, string, string>> listaDatos = new List<Tuple<string, string, string, string, string>>();

//             foreach (DataRow fila in ds.Tables["Hércules-KA-taxonomy (clean)"].Rows)
//             {
//                 Tuple<string, string, string, string> tupla =
// new Tuple<string, string, string, string>(fila["Level 0"].ToString(), fila["Level 1"].ToString(), fila["Level 2"].ToString(), fila["Level 3"].ToString());

//                 name[fila["WoS-JCR code"].ToString()] = tupla;

//                 //                 listaDatos.Add(tupla);

//             }

//             return name;

//         }



        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROWoSLogic(string baseUri, string bareer, Dictionary<string, string> ds)
        {
            this.baseUri = baseUri;
            this.bareer = bareer;
            this.ds = ds;//LeerDatosExcel(path);
            //$@"C:\GNOSS\Proyectos\HerculesMA\src\Hercules.MA.Load\Hercules.MA.Load\Dataset\Hércules-ED_Taxonomías_v1.1.xlsx");

        }

        /// <summary>
        /// A Http calls function
        /// </summary>
        /// <param name="url">the http call url</param>
        /// <param name="method">Crud method for the call</param>
        /// <param name="headers">The headers for the call</param>
        /// <returns></returns>
        protected async Task<string> httpCall(string url, string method = "GET", Dictionary<string, string> headers = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(method), url))
                {
                    request.Headers.TryAddWithoutValidation("X-ApiKey", bareer);
                    //request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");

                    if (headers != null && headers.Count > 0)
                    {
                        // if (headers.ContainsKey("Authorization"))
                        // {
                        //     request.Headers.TryAddWithoutValidation("Authorization", headers["Authorization"]);
                        // }
                        foreach (var item in headers)
                        {
                            request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    try
                    {
                        response = await httpClient.SendAsync(request);
                    }
                    catch (System.Exception)
                    {
                        throw new Exception("Error in the http call");
                    }
                }
            }
            if (response.Content != null)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "";
            }

        }

        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="ID"></param>
        /// <param date="Year-Moth-day"></param>

        /// <returns></returns>
        public List<Publication> getPublications(string name, string date = "1500-01-01", string uri = "api/wos/?databaseId=WOK&usrQuery=AI=({0})&count=100&firstRecord={1}&publishTimeSpan={2}%2B2022-12-31")
        {
            ROWoSControllerJSON info = new ROWoSControllerJSON(this);
            int n = 0;
            List<Publication> sol = new List<Publication>();
            int result = 1;
            int cardinalidad = 1;
            while (cardinalidad >= result)
            {
                Uri url = new Uri(baseUri + string.Format(uri, name, result.ToString(), date));
                n++;
                result = 100 * n;
                string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                //Console.Write(info_publication);
                Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
                cardinalidad = objInicial.Data.Records.records.REC.Count();
                List<Publication> nuevas = info.getListPublicatio(objInicial);
                sol.AddRange(nuevas);
            }

            return sol;
        }
    }
}
