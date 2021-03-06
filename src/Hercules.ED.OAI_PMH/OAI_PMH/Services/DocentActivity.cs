using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAI_PMH.Controllers;
using OAI_PMH.Models.SGI.ActividadDocente;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OAI_PMH.Services
{
    public class DocentActivity
    {
        public static Dictionary<string, DateTime> GetModifiedTesis(string from, ConfigService pConfig, string accessToken)
        {
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseActividadDocente() + "actividad-docente/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + ";tipoActividad=\"030.040.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    string idMod = "Tesis_" + id.Replace("\"", "");
                    if (!idDictionary.ContainsKey(idMod))
                    {
                        idDictionary.Add(idMod, DateTime.UtcNow);
                    }
                }
            }
            return idDictionary;
        }
        public static List<Tesis> GetTesis(string id, ConfigService pConfig, string accessToken)
        {
            string identifier = id.Replace("\"", "");
            RestClient client = new(pConfig.GetUrlBaseActividadDocente() + "actividad-docente/" + identifier + "?tipoActividad=\"030.040.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                return JsonConvert.DeserializeObject<List<Tesis>>(response.Content);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static Dictionary<string, DateTime> GetModifiedAcademicFormationProvided(string from, ConfigService pConfig, string accessToken)
        {
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseActividadDocente() + "actividad-docente/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + ";tipoActividad=\"030.010.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    string idMod = "FormacionImpartida_" + id.Replace("\"", "");
                    if (!idDictionary.ContainsKey(idMod))
                    {
                        idDictionary.Add(idMod, DateTime.UtcNow);
                    }
                }
            }
            return idDictionary;
        }
        public static List<FormacionAcademicaImpartida> GetAcademicFormationProvided(string id, ConfigService pConfig, string accessToken)
        {
            string identifier = id.Replace("\"", "");
            RestClient client = new(pConfig.GetUrlBaseActividadDocente() + "actividad-docente/" + identifier + "?tipoActividad=\"030.010.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                return JsonConvert.DeserializeObject<List<FormacionAcademicaImpartida>>(response.Content);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static Dictionary<string, DateTime> GetModifiedSeminars(string from, ConfigService pConfig, string accessToken)
        {
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseActividadDocente() + "actividad-docente/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"" + ";tipoActividad=\"030.060.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    string idMod = "CursosSeminarios_" + id.Replace("\"", "");
                    if (!idDictionary.ContainsKey(idMod))
                    {
                        idDictionary.Add(idMod, DateTime.UtcNow);
                    }
                }
            }
            return idDictionary;
        }
        public static List<SeminariosCursos> GetSeminars(string id, ConfigService pConfig, string accessToken)
        {
            string identifier = id.Replace("\"", "");
            RestClient client = new(pConfig.GetUrlBaseActividadDocente() + "actividad-docente/" + identifier + "?tipoActividad=\"030.060.000.000\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                return JsonConvert.DeserializeObject<List<SeminariosCursos>>(response.Content);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
