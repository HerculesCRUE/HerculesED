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
using Gnoss.ApiWrapper;

namespace OpenAireConnect.ROs.OpenAire.Controllers
{
    public class ROOpenAireLogic
    {
        private static string RUTA_OAUTH = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config";
        private static ResourceApi mResourceApi = null;

        private static ResourceApi ResourceApi
        {
            get
            {
                while (mResourceApi == null)
                {
                    try
                    {
                        mResourceApi = new ResourceApi(RUTA_OAUTH);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("No se ha podido iniciar ResourceApi");
                        Console.WriteLine($"Contenido OAuth: {File.ReadAllText(RUTA_OAUTH)}");
                        Thread.Sleep(10000);
                    }
                }
                return mResourceApi;
            }
        }

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
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");

                    if (headers != null && headers.Count > 0)
                    {
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
                        catch(Exception ex)
                        {
                            intentos--;
                            if (intentos == 0)
                            {
                                ResourceApi.Log.Error($@"[ERROR] {DateTime.Now} {ex.Message} {ex.StackTrace}");
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
            List<Publication> listaResultados = new List<Publication>();

            // Petición
            Uri url = new Uri($@"{baseUri}/search/publications?size={1000}&orcid={orcid}&format=json&fromDateAccepted={date}");
            
            string info_publication = httpCall(url.ToString(), "GET", headers).Result;

            try
            {
                Root objInicial = JsonConvert.DeserializeObject<Root>(info_publication);
                int total = Int32.Parse(objInicial.response.header.total.Text);
                List<Publication> nuevas = info.getListPublication(objInicial);
                listaResultados.AddRange(nuevas);
            }
            catch (Exception ex)
            {
                ResourceApi.Log.Error($@"[ERROR] {DateTime.Now} {ex.Message} {ex.StackTrace}");
            }

            return listaResultados;
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
                // Obtención de datos.
                if (!string.IsNullOrEmpty(result))
                {
                    Root objInicial = JsonConvert.DeserializeObject<Root>(result);
                    Result2 resultado = objInicial.response.results.result[0];
                    publicacionFinal = info.cambioDeModeloPublicacion(resultado);
                }
            }
            catch (Exception ex)
            {
                ResourceApi.Log.Error($@"[ERROR] {DateTime.Now} {ex.Message} {ex.StackTrace}");
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
