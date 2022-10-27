using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EditorCV.Models
{
    public abstract class AccionesEnvio
    {
        #region --- Constantes   
        protected static string RUTA_OAUTH = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config";
        protected static ResourceApi mResourceApi = new ResourceApi(RUTA_OAUTH);
        protected static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Models{Path.DirectorySeparatorChar}Utils{Path.DirectorySeparatorChar}prefijos.json";
        protected static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));
        #endregion

        /// <summary>
        /// Obtención del token.
        /// </summary>
        /// <returns></returns>
        protected string GetTokenCSP(ConfigService pConfig)
        {
            Uri url = new Uri(pConfig.GetUrlToken());
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("username", pConfig.GetUsernameEsbCsp()),
                new KeyValuePair<string, string>("password", pConfig.GetPasswordEsbCsp()),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            string result = httpCall(url.ToString(), "POST", content).Result;
            var json = JObject.Parse(result);

            return json["access_token"].ToString();
        }

        /// <summary>
        /// Obtención del token.
        /// </summary>
        /// <returns></returns>
        protected string GetTokenPRC(ConfigService pConfig)
        {
            Uri url = new Uri(pConfig.GetUrlToken());
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("username", pConfig.GetUsernameEsbPrc()),
                new KeyValuePair<string, string>("password", pConfig.GetPasswordEsbPrc()),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            string result = httpCall(url.ToString(), "POST", content).Result;
            var json = JObject.Parse(result);

            return json["access_token"].ToString();
        }

        /// <summary>
        /// Llamada para la obtención del token.
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pMethod"></param>
        /// <param name="pBody"></param>
        /// <returns></returns>
        protected async Task<string> httpCall(string pUrl, string pMethod, FormUrlEncodedContent pBody)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(pMethod), pUrl))
                {
                    request.Content = pBody;

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
                return string.Empty;
            }
        }

        /// <summary>
        /// Inserta un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        protected void Insercion(Guid pGuid, string pPropiedad, string pValorNuevo)
        {
            Insercion(pGuid, pPropiedad, new List<string>() { pValorNuevo });
        }

        /// <summary>
        /// Inserta un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        protected void Insercion(Guid pGuid, string pPropiedad, List<string> pValorNuevo)
        {
            Dictionary<Guid, List<TriplesToInclude>> dicInsercion = new Dictionary<Guid, List<TriplesToInclude>>();
            List<TriplesToInclude> listaTriplesInsercion = new List<TriplesToInclude>();
            foreach (string item in pValorNuevo)
            {
                TriplesToInclude triple = new TriplesToInclude();
                triple.Predicate = pPropiedad;
                triple.NewValue = item;
                listaTriplesInsercion.Add(triple);
            }
            dicInsercion.Add(pGuid, listaTriplesInsercion);
            mResourceApi.InsertPropertiesLoadedResources(dicInsercion);
        }

        /// <summary>
        /// Modifica un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        /// <param name="pValorAntiguo"></param>
        protected void Modificacion(Guid pGuid, string pPropiedad, string pValorNuevo, string pValorAntiguo)
        {
            Modificacion(pGuid, pPropiedad, new List<string>() { pValorNuevo }, new List<string>() { pValorAntiguo };
        }

        /// <summary>
        /// Modifica un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        /// <param name="pValorAntiguo"></param>
        protected void Modificacion(Guid pGuid, string pPropiedad, List<string> pValorNuevo, List<string> pValorAntiguo)
        {
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            for (int i = 0; i < pValorNuevo.Count; i++)
            {
                TriplesToModify triple = new TriplesToModify();
                triple.Predicate = pPropiedad;
                triple.NewValue = pValorNuevo[i];
                triple.OldValue = pValorAntiguo[i];
                listaTriplesModificacion.Add(triple);
            }
            dicModificacion.Add(pGuid, listaTriplesModificacion);
            mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }
    }
}
