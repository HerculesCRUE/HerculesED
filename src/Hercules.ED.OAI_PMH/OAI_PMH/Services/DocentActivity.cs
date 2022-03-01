using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAI_PMH.Models.SGI.ActividadDocente;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OAI_PMH.Services
{
    public class DocentActivity
    {
        // TODO: Revisar URL de petición junto a parametros.
        private static readonly string url = "https://sgi.demo.treelogic.com/api/sgicsp/";

        public static Dictionary<string, DateTime> GetModifiedTesis(string from)
        {
            string accessToken = Token.CheckToken();
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(url + "actividad-docente/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + "&q=tipoFormacion=\"030.040.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("Tesis_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }
        public static Tesis GetTesis(string id)
        {
            string accessToken = Token.CheckToken();
            string identifier = id.Split('_')[1];
            RestClient client = new(url + "actividad-docente/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            return JsonConvert.DeserializeObject<Tesis>(json.ToString());
        }
        public static Dictionary<string, DateTime> GetModifiedAcademicFormationProvided(string from)
        {
            string accessToken = Token.CheckToken();
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(url + "actividad-docente/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + "&q=tipoFormacion=\"030.010.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("FormacionImpartida_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }
        public static FormacionAcademicaImpartida GetAcademicFormationProvided(string id)
        {
            string accessToken = Token.CheckToken();
            string identifier = id.Split('_')[1];
            RestClient client = new(url + "actividad-docente/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            return JsonConvert.DeserializeObject<FormacionAcademicaImpartida>(json.ToString());
        }
        public static Dictionary<string, DateTime> GetModifiedSeminars(string from)
        {
            string accessToken = Token.CheckToken();
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(url + "actividad-docente/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + "&q=tipoFormacion=\"030.060.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("CursosSeminarios_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }
        public static SeminariosCursos GetSeminars(string id)
        {
            string accessToken = Token.CheckToken();
            string identifier = id.Split('_')[1];
            RestClient client = new(url + "actividad-docente/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            return JsonConvert.DeserializeObject<SeminariosCursos>(json.ToString());
        }
    }
}
