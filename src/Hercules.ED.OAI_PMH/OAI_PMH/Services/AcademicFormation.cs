using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAI_PMH.Controllers;
using OAI_PMH.Models.SGI.FormacionAcademica;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OAI_PMH.Services
{
    public class AcademicFormation
    {
        public static Dictionary<string, DateTime> GetModifiedCiclos(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + "&q=tipoFormacion=\"020.010.010.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("FormacionAcademica-Ciclos_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }
        public static Ciclos GetFormacionAcademicaCiclos(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split('_')[1];
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            return JsonConvert.DeserializeObject<Ciclos>(json.ToString());
        }
        public static Dictionary<string, DateTime> GetModifiedDoctorados(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + "&q=tipoFormacion=\"020.010.020.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("FormacionAcademica-Doctorados_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }
        public static Doctorados GetFormacionAcademicaDoctorados(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split('_')[1];
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            return JsonConvert.DeserializeObject<Doctorados>(json.ToString());
        }
        public static Dictionary<string, DateTime> GetModifiedPosgrado(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + "&q=tipoFormacion=\"020.010.030.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("FormacionAcademica-Posgrado_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }
        public static Posgrado GetFormacionAcademicaPosgrado(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split('_')[1];
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            return JsonConvert.DeserializeObject<Posgrado>(json.ToString());
        }
        public static Dictionary<string, DateTime> GetModifiedEspecializada(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + "&q=tipoFormacion=\"020.020.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("FormacionAcademica-Especializada_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }
        public static FormacionEspecializada GetFormacionAcademicaEspecializada(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split('_')[1];
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            return JsonConvert.DeserializeObject<FormacionEspecializada>(json.ToString());
        }
        public static Dictionary<string, DateTime> GetModifiedIdiomas(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + "&q=tipoFormacion=\"020.060.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("FormacionAcademica-Idiomas_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }
        public static ConocimientoIdiomas GetFormacionAcademicaIdiomas(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split('_')[1];
            string type = id.Split('_')[2];
            RestClient client = new(pConfig.GetUrlBaseFormacionAcademica() + "formacion/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            return JsonConvert.DeserializeObject<ConocimientoIdiomas>(json.ToString());
        }
    }
}
