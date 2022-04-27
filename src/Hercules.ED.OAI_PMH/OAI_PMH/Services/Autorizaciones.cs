using Newtonsoft.Json;
using OAI_PMH.Controllers;
using OAI_PMH.Models.SGI.Autorizacion;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OAI_PMH.Services
{
    public class Autorizaciones
    {
        public static Dictionary<string, DateTime> GetModifiedAutorizaciones(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseAutorizacion() + "autorizaciones/modificadas-ids?q=fechaModificacion=ge=\"" + from + "\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            if (!string.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("AutorizacionProyecto_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }

        public static Autorizacion GetAutorizacion(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split("_")[1];
            RestClient client = new(pConfig.GetUrlBaseAutorizacion() + "autorizaciones/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                return JsonConvert.DeserializeObject<Autorizacion>(response.Content);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
