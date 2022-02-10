using FigShareAPI.Controllers;
using FigShareAPI.Models.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FigShareAPI.Models
{
    public class FigShare
    {
        // Configuración.
        readonly ConfigService _Configuracion;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        public FigShare(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Contrucción de la cabecera de envío.
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pMethod"></param>
        /// <param name="pHeaders"></param>
        /// <returns></returns>
        protected async Task<string> httpCall(string pUrl, string pToken, string pMethod = "GET", Dictionary<string, string> pHeaders = null)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(pMethod), pUrl))
                {
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("Host", "api.figshare.com");
                    request.Headers.Add("Authorization", pToken);

                    if (pHeaders != null && pHeaders.Count > 0)
                    {
                        foreach (var item in pHeaders)
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
        /// Obtiene la lista de IDs de los ROs que son públicos.
        /// </summary>
        /// <returns>Lista de identificadores.</returns>
        public List<int> getIdentifiers(string pToken)
        {
            // Petición.
            Uri url = new Uri($@"{_Configuracion.GetUrlBase()}/account/articles");
            string result = httpCall(url.ToString(), pToken, pMethod: "GET").Result;
            List<ArticleScheme> listaArticulos = JsonConvert.DeserializeObject<List<ArticleScheme>>(result);

            // Obtención de IDs de los ROs que son PÚBLICOS.
            List<int> listaIdentificadores = new List<int>();
            foreach (ArticleScheme articulo in listaArticulos)
            {
                if (articulo.published_date != null)
                {
                    listaIdentificadores.Add(articulo.id);
                }
            }

            return listaIdentificadores;
        }

        /// <summary>
        /// Obtiene los datos más detallados de los IDs de la lista.
        /// </summary>
        /// <param name="pListaIdentificadores">Lista de IDs a obtener los datos.</param>
        /// <returns>Lista de datos.</returns>
        public List<Article> getData(List<int> pListaIdentificadores, string pToken)
        {
            List<Article> listaArticulos = new List<Article>();

            foreach (int id in pListaIdentificadores)
            {
                // Petición.
                Uri url = new Uri($@"{_Configuracion.GetUrlBase()}/account/articles/{id}");
                string result = httpCall(url.ToString(), pToken, pMethod: "GET").Result;
                listaArticulos.Add(JsonConvert.DeserializeObject<Article>(result));
            }

            return listaArticulos;
        }

        /// <summary>
        /// Obtiene los datos necesarios para los RO en EDMA.
        /// </summary>
        /// <param name="pListaArticulos">Lista de articulos con los datos completos.</param>
        /// <returns>Lista con los datos necesarios.</returns>
        public List<RO> getROs(List<Article> pListaArticulos)
        {
            List<RO> listaROs = new List<RO>();
            foreach (Article articulo in pListaArticulos)
            {
                RO researchObject = new RO();
                if (articulo.id != null)
                {
                    researchObject.id = articulo.id;
                }
                if (!string.IsNullOrEmpty(articulo.defined_type_name))
                {
                    researchObject.tipo = articulo.defined_type_name;
                }
                if (!string.IsNullOrEmpty(articulo.title))
                {
                    researchObject.titulo = articulo.title;
                }
                if (!string.IsNullOrEmpty(articulo.description))
                {
                    researchObject.descripcion = articulo.description;
                }
                if (!string.IsNullOrEmpty(articulo.figshare_url))
                {
                    researchObject.url = articulo.figshare_url;
                }
                if (articulo.published_date != null)
                {
                    researchObject.fechaPublicacion = articulo.published_date.ToString();
                }
                if (!string.IsNullOrEmpty(articulo.doi))
                {
                    researchObject.doi = articulo.doi;
                }
                if (articulo.tags != null && articulo.tags.Any())
                {
                    researchObject.etiquetas = articulo.tags;
                }
                if (articulo.authors != null && articulo.authors.Any())
                {
                    researchObject.autores = new List<Person>();
                    foreach (Author autor in articulo.authors)
                    {
                        Person person = new Person();
                        person.id = autor.id;
                        person.nombreCompleto = autor.full_name;
                        person.orcid = autor.orcid_id;
                        researchObject.autores.Add(person);
                    }
                }
                if (articulo.license != null && !string.IsNullOrEmpty(articulo.license.name))
                {
                    researchObject.licencia = articulo.license.name;
                }
                listaROs.Add(researchObject);
            }
            if (listaROs.Any())
            {
                return listaROs;
            }
            else
            {
                return null;
            }
        }
    }
}
