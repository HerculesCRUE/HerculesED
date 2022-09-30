using GitHubAPI.Controllers;
using GitHubAPI.Models.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubAPI.ROs.Codes.Controllers
{
    public class GitHub
    {
        // Configuración.
        readonly ConfigService _Configuracion;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        public GitHub(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Contrucción de la cabecera de envío.
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pToken"></param>
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
                    request.Headers.Add("Accept", "application/vnd.github.v3+json");
                    request.Headers.Add("Connection", "keep-alive");
                    request.Headers.Add("Authorization", pToken);
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1521.3 Safari/537.36");

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
        /// Construye el objeto con todos los datos necesarios obtenidos.
        /// </summary>
        /// <param name="pUser">Nombre del repositorio.</param>
        /// <param name="pToken">Token.</param>
        /// <returns></returns>
        public List<DataGitHub> getData(string pUser, string pToken)
        {
            // Petición.
            Uri url = new Uri($@"{_Configuracion.GetUrlBase()}/users/{pUser}/repos");
            string result = httpCall(url.ToString(), pToken, pMethod: "GET").Result;
            List<Repositories> listaRepositorios = new List<Repositories>();

            try
            {
                listaRepositorios = JsonConvert.DeserializeObject<List<Repositories>>(result);
            }
            catch (Exception)
            {
                return null;
            }

            // Lista de Datos.
            if (listaRepositorios != null && listaRepositorios.Any())
            {
                List<DataGitHub> listaDatos = new List<DataGitHub>();

                foreach (Repositories repositorio in listaRepositorios)
                {
                    DataGitHub data = new DataGitHub();
                    data.tipo = "codigo";
                    if (repositorio.id != null)
                    {
                        data.id = repositorio.id;
                    }
                    if (!string.IsNullOrEmpty(repositorio.name))
                    {
                        data.titulo = repositorio.name;
                    }
                    if (!string.IsNullOrEmpty(repositorio.description))
                    {
                        data.descripcion = repositorio.description;
                    }
                    if (!string.IsNullOrEmpty(repositorio.html_url))
                    {
                        data.url = repositorio.html_url;
                    }
                    if (repositorio.created_at != null)
                    {
                        data.fechaCreacion = repositorio.created_at.ToString();
                    }
                    if (repositorio.updated_at != null)
                    {
                        data.fechaActualizacion = repositorio.updated_at.ToString();
                    }
                    if (repositorio.license != null && !string.IsNullOrEmpty(repositorio.license.spdx_id))
                    {
                        data.licencia = repositorio.license.spdx_id;
                    }
                    if (repositorio.forks_count != null)
                    {
                        data.numForks = repositorio.forks_count;
                    }
                    if (repositorio.open_issues_count != null)
                    {
                        data.numIssues = repositorio.open_issues_count;
                    }
                    if (repositorio.topics != null && repositorio.topics.Any())
                    {
                        data.etiquetas = repositorio.topics;
                    }
                    if (!string.IsNullOrEmpty(repositorio.contributors_url))
                    {
                        data.listaAutores = getContributors(repositorio.contributors_url, pToken);
                    }
                    if (!string.IsNullOrEmpty(repositorio.languages_url))
                    {
                        data.lenguajes = getLenguajesProg(repositorio.languages_url, pToken);
                    }
                    if (!string.IsNullOrEmpty(repositorio.releases_url))
                    {
                        data.numReleases = getNumReleases(repositorio.releases_url, pToken);
                    }

                    // Enriquecimiento
                    string dataEnriquecimientoSinPdf = JsonConvert.SerializeObject(obtenerObjEnriquecimiento(data));
                    data.etiquetasEnriquecidas = getDescriptores(dataEnriquecimientoSinPdf, "specific");
                    if (data.etiquetasEnriquecidas != null && !data.etiquetasEnriquecidas.Any())
                    {
                        data.etiquetasEnriquecidas = null;
                    }

                    data.categoriasEnriquecidas = getDescriptores(dataEnriquecimientoSinPdf, "thematic");
                    if (data.categoriasEnriquecidas != null && !data.categoriasEnriquecidas.Any())
                    {
                        data.categoriasEnriquecidas = null;
                    }

                    if (!string.IsNullOrEmpty(data.titulo))
                    {
                        listaDatos.Add(data);
                    }
                }

                return listaDatos;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene las personas participantes en el proyecto.
        /// </summary>
        /// <param name="pUrl">URL a consultar.</param>
        /// <param name="pToken">Token.</param>
        /// <returns></returns>
        public List<string> getContributors(string pUrl, string pToken)
        {
            // Petición.
            string result = httpCall(pUrl, pToken, pMethod: "GET").Result;
            List<Contributors> listaContributors = new List<Contributors>();

            try
            {
                listaContributors = JsonConvert.DeserializeObject<List<Contributors>>(result);
            }
            catch (Exception)
            {
                return null;
            }

            if (listaContributors != null && listaContributors.Any())
            {
                List<string> nombres = new List<string>();
                foreach (Contributors miembro in listaContributors)
                {
                    if (!string.IsNullOrEmpty(miembro.login))
                    {
                        nombres.Add(miembro.login);
                    }
                }
                return nombres;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene un diccionario con el lenguaje de programación y su porcentaje de uso en el proyecto.
        /// </summary>
        /// <param name="pUrl">URL a consultar.</param>
        /// <param name="pToken">Token.</param>
        /// <returns></returns>
        public Dictionary<string, float> getLenguajesProg(string pUrl, string pToken)
        {
            // Petición.
            string result = httpCall(pUrl, pToken, pMethod: "GET").Result;
            Dictionary<string, string> dicLenguajes = new Dictionary<string, string>();

            try
            {
                dicLenguajes = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            }
            catch (Exception)
            {
                return null;
            }

            if (dicLenguajes != null && dicLenguajes.Any())
            {
                Dictionary<string, float> dicLenguajesCalculados = new Dictionary<string, float>();

                long numTotal = 0;
                foreach (KeyValuePair<string, string> item in dicLenguajes)
                {
                    numTotal += Int32.Parse(item.Value);
                }
                foreach (KeyValuePair<string, string> item in dicLenguajes)
                {
                    float porcentaje = (Int32.Parse(item.Value) * 100) / (float)numTotal;
                    porcentaje = (float)Math.Round(porcentaje * 100f) / 100f;
                    dicLenguajesCalculados.Add(item.Key, porcentaje);
                }

                return dicLenguajesCalculados;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene el número de Releases de un repositorio.
        /// </summary>
        /// <param name="pUrl">URL a consultar.</param>
        /// <param name="pToken">Token.</param>
        /// <returns></returns>
        public int getNumReleases(string pUrl, string pToken)
        {
            // Petición.
            string result = httpCall(pUrl.Substring(0, pUrl.IndexOf("{")), pToken, pMethod: "GET").Result;
            List<Releases> listaReleases = new List<Releases>();

            try
            {
                listaReleases = JsonConvert.DeserializeObject<List<Releases>>(result);
            }
            catch (Exception e)
            {
                return 0;
            }

            if (listaReleases != null && listaReleases.Any())
            {
                return listaReleases.Count();
            }
            else
            {
                return 0;
            }
        }

        public ObjEnriquecimientoConPdf obtenerObjEnriquecimientoPdf(DataGitHub pRo)
        {
            if (!string.IsNullOrEmpty(pRo.titulo) && !string.IsNullOrEmpty(pRo.descripcion))
            {
                ObjEnriquecimientoConPdf objEnriquecimiento = new ObjEnriquecimientoConPdf();
                objEnriquecimiento.rotype = "papers";
                objEnriquecimiento.title = pRo.titulo;
                objEnriquecimiento.abstract_ = pRo.descripcion;

                return objEnriquecimiento;
            }
            else
            {
                return null;
            }
        }

        public ObjEnriquecimientoSinPdf obtenerObjEnriquecimiento(DataGitHub pRo)
        {
            if (!string.IsNullOrEmpty(pRo.titulo) && !string.IsNullOrEmpty(pRo.descripcion))
            {
                ObjEnriquecimientoSinPdf objEnriquecimiento = new ObjEnriquecimientoSinPdf();
                objEnriquecimiento.rotype = "papers";
                objEnriquecimiento.title = pRo.titulo;
                objEnriquecimiento.abstract_ = pRo.descripcion;

                return objEnriquecimiento;
            }
            else
            {
                return null;
            }
        }

        public List<string> getDescriptores(string pDataEnriquecimiento, string pTipo)
        {
            // Petición.
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            string result = string.Empty;
            var contentData = new StringContent(pDataEnriquecimiento, System.Text.Encoding.UTF8, "application/json");

            int intentos = 3;
            while (true)
            {
                try
                {
                    response = client.PostAsync($@"{_Configuracion.GetUrlBaseEnriquecimiento()}/{pTipo}", contentData).Result;
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

            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
            }

            if (!string.IsNullOrEmpty(result))
            {
                Topics_enriquecidos data = null;
                try
                {
                    data = JsonConvert.DeserializeObject<Topics_enriquecidos>(result);
                }
                catch (Exception)
                {
                    return null;
                }

                if (data != null)
                {
                    HashSet<string> listaTopics = new HashSet<string>();
                    foreach (Knowledge_enriquecidos item in data.topics)
                    {
                        listaTopics.Add(item.word);
                    }
                    return listaTopics.ToList();
                }
            }

            return null;
        }
    }
}
