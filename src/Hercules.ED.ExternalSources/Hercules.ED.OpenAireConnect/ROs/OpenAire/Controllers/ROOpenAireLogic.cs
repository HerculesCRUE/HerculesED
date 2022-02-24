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
using OpenAireConnect.ROs.OpenAire.Models;
using OpenAireConnect.ROs.OpenAire.Models.Inicial;
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

namespace OpenAireConnect.ROs.OpenAire.Controllers
{
    public class ROOpenAireLogic
    {

        protected string baseUri { get; set; }


        protected Dictionary<string, string> headers = new Dictionary<string, string>();
        public ROOpenAireLogic(string baseUri)
        {
            this.baseUri = baseUri;

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
                    //request.Headers.TryAddWithoutValidation("X-ApiKey", bareer);
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
            ROOpenAireControllerJSON info = new ROOpenAireControllerJSON(this);
            List<Publication> sol = new List<Publication>();

            string h = "1";
            //TODO: Cambiar fecha de publicaci�n por fecha de modificaci�n.
            Uri url = new Uri($@"{baseUri}/search/publications?size={1000}&orcid={orcid}&format=json&fromDateAccepted={date}");
            //Uri url = new Uri($@"{baseUri}api/OpenAire/citing?databaseId=WOK&uniqueId=OpenAire:000624784700001&count={numItems}&firstRecord={(numItems * n) + 1}&publishTimeSpan=1500-01-01%2B3000-12-31"); //&publishTimeSpan={date}%2B3000-12-31
            //Uri url = new Uri($@"{baseUri}api/OpenAire/references?databaseId=WOK&uniqueId=OpenAire:000624784700001&count={numItems}&firstRecord={(numItems * n) + 1}"); //
            //Uri url = new Uri($@"{baseUri}api/OpenAire?databaseId=WOK&uniqueId=OpenAire:000270372400005"); //&publishTimeSpan={date}%2B3000-12-31

            string info_publication = httpCall(url.ToString(), "GET", headers).Result;

            try
            {
                Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
                int total = Int32.Parse(objInicial.response.header.total.Text);
                List<Publication> nuevas = info.getListPublicatio(objInicial);
                sol.AddRange(nuevas);
            }
            catch (Exception error)
            {
                throw error;
            }

            return sol;
        }

        
        /// <summary>
        /// Obtiene una publicaci�n mediante el DOI.
        /// </summary>
        /// <param name="pDoi">DOI de la publicaci�n a obtener.</param>
        /// <returns></returns>
        public Publication getPublicationDoi(string pDoi)
        {
            // Objeto publicaci�n.
            Publication publicacionFinal = null;

            try
            {
                // Clase.
                ROOpenAireControllerJSON info = new ROOpenAireControllerJSON(this);

                // Petici�n.

                Uri url = new Uri($@"{baseUri}/search/publications?doi={pDoi}&format=json");
                string result = httpCall(url.ToString(), "GET", headers).Result;
                // Obtenci�n de datos.
                if (!string.IsNullOrEmpty(result))
                {
                    Root objInicial = JsonConvert.DeserializeObject<Root>(result);
                    Result2 resultado = objInicial.response.results.result[0];
                    publicacionFinal = info.cambioDeModeloPublicacion(resultado, true);
                }
            }
            catch (Exception error)
            {
                return publicacionFinal;
            }

            return publicacionFinal;
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
