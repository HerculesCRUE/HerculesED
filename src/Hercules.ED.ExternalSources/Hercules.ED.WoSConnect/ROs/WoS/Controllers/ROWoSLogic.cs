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
        public Dictionary<string, Tuple<string,string,string,string,string,string>>  autores_orcid; //= LeerDatosExcel_autores(@"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hercules-ED_autores.xlsx");


        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROWoSLogic(string baseUri, string bareer, Dictionary<string, string> ds,  Dictionary<string, Tuple<string,string, string, string,string,string>>  autores_orcid)
        {
            this.baseUri = baseUri;
            this.bareer = bareer;
            this.ds = ds;//LeerDatosExcel(path);
            this.autores_orcid= autores_orcid;
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
        /// <param name="orcid">ORCID</param>
        /// <param name="date">Fecha de incicio</param>
        /// <returns></returns>
        public List<Publication> getPublications(string orcid, string date = "1500-01-01")
        {
            ROWoSControllerJSON info = new ROWoSControllerJSON(this);
            int n = 0;
            List<Publication> sol = new List<Publication>();
            int numItems = 100;
            bool continuar = true;
            while (continuar)
            {
                Uri url = new Uri($@"{baseUri}api/wos/?databaseId=WOK&usrQuery=AI=({orcid})&count={numItems}&firstRecord={(numItems * n) + 1}&publishTimeSpan={date}%2B3000-12-31");
                n++;
                string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
                List<Publication> nuevas = info.getListPublicatio(objInicial);
                sol.AddRange(nuevas);
                if (nuevas.Count == 0)
                {
                    continuar = false;
                }
            }

            Console.Write("Ids del diccionario\n");
            foreach(string i in this.autores_orcid.Keys){
                Console.Write(i);
                Console.Write(" ");
                Console.Write(this.autores_orcid[i]);
                Console.Write("\n");
            }
            return sol;
        }

       
    }
}
