using Newtonsoft.Json;
using OAI_PMH.Controllers;
using OAI_PMH.Models.SGI.GruposInvestigacion;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OAI_PMH.Services
{
    public class InvestigationGroup
    {
        public static Dictionary<string, DateTime> GetModifiedGrupos(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/modificados-ids?q=fechaModificacion=ge=\"" + from + "\""); // TODO: Revisar url petición.
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    string idMod = "Grupo_" + id.Replace("\"", "");
                    if (!idDictionary.ContainsKey(idMod))
                    {
                        idDictionary.Add(idMod, DateTime.UtcNow);
                    }
                }
            }
            return idDictionary;
        }

        public static Grupo GetGrupos(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split("_")[1];
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + identifier); // TODO: Revisar url petición.
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Grupo grupo = new Grupo();
            try
            {
                grupo = JsonConvert.DeserializeObject<Grupo>(response.Content);
                grupo.equipo = GetGrupoEquipo(identifier, pConfig);
                grupo.palabrasClave = GetGrupoPalabrasClave(identifier, pConfig);
            }
            catch (Exception e)
            {
                return null;
            }

            return grupo;
        }

        private static List<GrupoEquipo> GetGrupoEquipo(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<GrupoEquipo> grupoEquipo = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + id + "/miembrosequipo"); // TODO: Revisar url petición.
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            grupoEquipo = JsonConvert.DeserializeObject<List<GrupoEquipo>>(response.Content);
            return grupoEquipo;
        }

        private static List<string> GetGrupoPalabrasClave(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<string> palabras = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + id + "/palabrasclave"); // TODO: Revisar url petición.
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            palabras = JsonConvert.DeserializeObject<List<string>>(response.Content);
            return palabras;
        }
    }
}
