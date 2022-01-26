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
using System.Threading;

namespace WoSConnect.ROs.WoS.Controllers
{
    public class ROWoSLogic
    {
        protected string bareer;
        protected string baseUri { get; set; }

        public Dictionary<string, string> ds;


        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROWoSLogic(string baseUri, string bareer, Dictionary<string, string> ds)
        {
            this.baseUri = baseUri;
            this.bareer = bareer;
            this.ds = ds;

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

                    int intentos = 3;
                    while (true)
                    {
                        try
                        {
                            response = await httpClient.SendAsync(request);
                            break;
                        }
                        catch
                        {
                            intentos--;
                            if (intentos == 0)
                            {
                                throw;
                            }
                            else
                            {
                                Thread.Sleep(1000);
                            }
                        }
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
                //TODO: Cambiar fecha de publicación por fecha de modificación.
                Uri url = new Uri($@"{baseUri}api/wos/?databaseId=WOK&usrQuery=AI=({orcid})&count={numItems}&firstRecord={(numItems * n) + 1}&publishTimeSpan={date}%2B3000-12-31");
                //Uri url = new Uri($@"{baseUri}api/wos/citing?databaseId=WOK&uniqueId=WOS:000624784700001&count={numItems}&firstRecord={(numItems * n) + 1}&publishTimeSpan=1500-01-01%2B3000-12-31"); //&publishTimeSpan={date}%2B3000-12-31
                //Uri url = new Uri($@"{baseUri}api/wos/references?databaseId=WOK&uniqueId=WOS:000624784700001&count={numItems}&firstRecord={(numItems * n) + 1}"); //
                //Uri url = new Uri($@"{baseUri}api/wos?databaseId=WOK&uniqueId=WOS:000270372400005"); //&publishTimeSpan={date}%2B3000-12-31
                n++;
                string info_publication = httpCall(url.ToString(), "GET", headers).Result;
                try
                {
                    Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
                    List<Publication> nuevas = info.getListPublicatio(objInicial);
                    sol.AddRange(nuevas);
                    if (nuevas.Count == 0)
                    {
                        continuar = false;
                    }
                }
                catch (Exception error)
                {
                    throw error;
                }
            }
            return sol;
        }

        /// <summary>
        /// Obtiene una publicación mediante el ID de WoS.
        /// </summary>
        /// <param name="pIdWos">ID de la publicación a obtener.</param>
        /// <returns></returns>
        public Publication getPublicationWos(string pIdWos)
        {
            // Objeto publicación.
            Publication publicacionFinal = null;

            try
            {
                // Clase.
                ROWoSControllerJSON info = new ROWoSControllerJSON(this);

                // Petición.
                Uri url = new Uri($@"{baseUri}api/wos/id/WOS:{pIdWos}?databaseId=WOK&count=1&firstRecord=1");
                string result = httpCall(url.ToString(), "GET", headers).Result;

                // Obtención de datos.
                if (!string.IsNullOrEmpty(result) && !result.Contains("\"RecordsFound\":0"))
                {
                    Root objInicial = JsonConvert.DeserializeObject<Root>(result);
                    PublicacionInicial publicacionInicial = objInicial.Data.Records.records.REC[0];
                    publicacionFinal = info.cambioDeModeloPublicacion(publicacionInicial, true);
                }
            }
            catch (Exception error)
            {
                return publicacionFinal;
            }

            return publicacionFinal;
        }

        /// <summary>
        /// Obtiene una publicación mediante el DOI.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación a obtener.</param>
        /// <returns></returns>
        public Publication getPublicationDoi(string pDoi)
        {
            // Objeto publicación.
            Publication publicacionFinal = null;

            try
            {
                // Clase.
                ROWoSControllerJSON info = new ROWoSControllerJSON(this);

                // Petición.
                Uri url = new Uri($@"{baseUri}api/wos/?databaseId=WOK&usrQuery=DO=({pDoi})&count=1&firstRecord=1");
                string result = httpCall(url.ToString(), "GET", headers).Result;

                // Obtención de datos.
                if (!string.IsNullOrEmpty(result) && !result.Contains("\"RecordsFound\":0"))
                {
                    Root objInicial = JsonConvert.DeserializeObject<Root>(result);
                    PublicacionInicial publicacionInicial = objInicial.Data.Records.records.REC[0];
                    publicacionFinal = info.cambioDeModeloPublicacion(publicacionInicial, true);
                }
            }
            catch (Exception error)
            {
                return publicacionFinal;
            }

            return publicacionFinal;
        }

        /// <summary>
        /// Obtiene las publicaciones que son citadas mediante el ID de WOS.
        /// </summary>
        /// <param name="pIdWos">ID de WoS.</param>
        /// <param name="pFecha">Fecha de obtención.</param>
        /// <returns></returns>
        public List<Publication> getCitingByWosId(string pIdWos, string pFecha = "1500-01-01")
        {
            // Objeto publicación.
            List<Publication> listaPublicaciones = new List<Publication>();
            int numIncremental = 0;
            int numCitas = 100;

            try
            {
                while (true)
                {
                    // Clase.
                    ROWoSControllerJSON info = new ROWoSControllerJSON(this);

                    // Petición.
                    string result = string.Empty;
                    Uri url = new Uri($@"{baseUri}api/wos/citing?databaseId=WOK&uniqueId=WOS:{pIdWos}&count={numCitas}&firstRecord={(numCitas * numIncremental) + 1}&publishTimeSpan={pFecha}%2B3000-12-31");
                    result = httpCall(url.ToString(), "GET", headers).Result;
                    Thread.Sleep(500); // Restricción de peticiones del API de WOS (2 peticiones por segundo)

                    // Obtención de datos.
                    if (!result.Contains("Server.invalidInput") && (!string.IsNullOrEmpty(result) && !result.Contains("\"RecordsFound\":0")))
                    {
                        numIncremental++;
                        Root objInicial = new Root();

                        try
                        {                            
                            objInicial = JsonConvert.DeserializeObject<Root>(result);
                        }
                        catch (Exception error)
                        {
                            continue; // Si no puede parsear el objeto, que pase al siguiente.
                        }

                        foreach (PublicacionInicial item in objInicial.Data.Records.records.REC)
                        {
                            Publication publicacion = info.getPublicacionCita(item);
                            listaPublicaciones.Add(publicacion);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception error)
            {
                return listaPublicaciones;
            }

            return listaPublicaciones;
        }
    }

    public class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }
            return new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
