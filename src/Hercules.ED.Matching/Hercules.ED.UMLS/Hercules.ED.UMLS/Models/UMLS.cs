using Hercules.ED.UMLS.Controllers;
using Hercules.ED.UMLS.Middlewares;
using Hercules.ED.UMLS.Models.Data;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hercules.ED.UMLS.Models
{
    public class UMLS
    {
        // Configuración.
        readonly ConfigService _Configuracion;

        // Logs.
        private FileLogger _FileLogger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        public UMLS(ConfigService pConfig)
        {
            _Configuracion = pConfig;
            _FileLogger = new FileLogger(pConfig);
        }

        protected async Task<string> httpCall(string pUrl, string pMethod, Dictionary<string, string> pHeader = null, FormUrlEncodedContent pBody = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(pMethod), pUrl))
                {
                    if (pMethod == "GET")
                    {
                        if (pHeader != null && pHeader.Any())
                        {
                            foreach (KeyValuePair<string, string> item in pHeader)
                            {
                                request.Headers.Add(item.Key, item.Value);
                            }
                        }
                    }
                    else if (pMethod == "POST")
                    {
                        request.Content = pBody;
                    }

                    int intentos = 10;
                    while (true)
                    {
                        try
                        {
                            response = await httpClient.SendAsync(request);
                            break;
                        }
                        catch (Exception error)
                        {
                            intentos--;
                            if (intentos == 0)
                            {
                                _FileLogger.Log(pUrl, error.ToString());
                                throw;
                            }
                            else
                            {
                                Thread.Sleep(5000);
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
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtiene el "Ticket-Granting Ticket". El TGT tiene una validez de 8 horas.
        /// </summary>
        /// <returns>TGT.</returns>
        public string GetTGT()
        {
            // Petición.
            Uri url = new Uri(_Configuracion.GetUrlTGT());
            FormUrlEncodedContent body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("apikey", _Configuracion.GetApiKey())
            });
            string result = httpCall(url.ToString(), "POST", pBody: body).Result;

            // Obtención del dato del HTML de respuesta.
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result);
            string data = doc.DocumentNode.SelectSingleNode("//form").GetAttributeValue("action", "");

            // TGT.
            return data.Substring(data.LastIndexOf('/') + 1);
        }

        /// <summary>
        /// Obtiene el "Service Ticket". El ST expira a los 5 minutos o por cada petición usada.
        /// </summary>
        /// <param name="pTGT"></param>
        /// <returns>ST.</returns>
        public string GetTicket(string pTGT)
        {
            // Petición.
            Uri url = new Uri($@"{_Configuracion.GetUrlTicket()}/{pTGT}");
            FormUrlEncodedContent body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("service", "http://umlsks.nlm.nih.gov")
            });

            // Ticket.
            return httpCall(url.ToString(), "POST", pBody: body).Result;
        }

        /// <summary>
        /// Mediante un ID MESH, obtiene la equivalencia al ID SNOMED.
        /// </summary>
        /// <param name="pName">Nombre del término a obtener.</param>
        /// <param name="pMeshId">ID MESH.</param>
        /// <param name="pST">Service Ticket.</param>
        /// <returns>ID SNOMED.</returns>
        public string GetSnomedId(string pName, string pMeshId, string pST, Data.Data pData)
        {
            // Mensaje de error.
            string msg = string.Empty;

            // Petición.
            Uri url = new Uri($@"{_Configuracion.GetUrlSNOMED()}/{pMeshId}?targetSource=SNOMEDCT_US&ticket={pST}");
            string result = httpCall(url.ToString(), "GET").Result;

            // Obtención del dato del JSON de respuesta.
            try
            {     
                CrosswalkObj data = JsonConvert.DeserializeObject<CrosswalkObj>(result);

                // ID SNOMED.
                string snomedId = string.Empty;
                foreach (Result item in data.result)
                {
                    if(item.name.ToLower().Trim() == pName.ToLower().Trim())
                    {
                        pData.snomedTerm = item;
                        snomedId = item.ui;
                        break;
                    }
                }

                return snomedId;
            }
            catch (Exception error)
            {
                if (result.StartsWith("<!doctype html>"))
                {
                    msg = "HTTP Status 500 – Internal Server Error";
                }
                else
                {
                    msg = "No results containing all your search terms were found";
                }

                _FileLogger.Log(msg, error.ToString());
                Thread.Sleep(5000);
                return null;
            }
        }

        /// <summary>
        /// Obtiene todas las relaciones que tiene el término buscado.
        /// </summary>
        /// <param name="pSnomedId">ID SNOMED.</param>
        /// <param name="pTGT">TGT.</param>
        /// <returns>Lista de relaciones del termino buscado.</returns>
        public List<ResultRelations> GetRelaciones(string pSnomedId, string pTGT)
        {
            List<ResultRelations> listaResultados = new List<ResultRelations>();

            // Paginacion.
            int numPagina = 1;
            int numPaginasTotal = int.MaxValue;

            // Petición.
            while (numPagina <= numPaginasTotal)
            {
                // Obtención del Service Ticket.
                string serviceTicket = GetTicket(pTGT);

                // Petición.
                Uri url = new Uri($@"{_Configuracion.GetUrlRelaciones()}/{pSnomedId}/relations?pageNumber={numPagina}&ticket={serviceTicket}");
                string result = httpCall(url.ToString(), "GET").Result;

                // Obtención del dato del JSON de respuesta.
                RelationsObj data = JsonConvert.DeserializeObject<RelationsObj>(result);
                listaResultados.AddRange(data.result);

                // Obtención de número de paginas total y actual.
                numPagina++;
                numPaginasTotal = data.pageCount;
            }

            // Datos.
            return listaResultados;
        }
    }
}
