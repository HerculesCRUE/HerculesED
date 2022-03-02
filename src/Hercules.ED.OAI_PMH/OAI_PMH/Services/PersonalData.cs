using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAI_PMH.Controllers;
using OAI_PMH.Models.SGI.PersonalData;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Services
{
    public class PersonalData
    {
        public static Dictionary<string, DateTime> GetModifiedPeople(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "personas/modificadas-ids?q=fechaModificacion=ge=\"" + from + "\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    string idPersona = "Persona_" + id.Replace("\"", "").Substring(0, id.Replace("\"", "").Length - 1);
                    if (!idDictionary.ContainsKey(idPersona))
                    {
                        idDictionary.Add(idPersona, DateTime.UtcNow);
                    }
                }
            }
            return idDictionary;
        }

        public static Persona GetPersona(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split('_')[1];
            Persona persona = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "personas/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            persona = JsonConvert.DeserializeObject<Persona>(json.ToString());
            persona.DatosPersonales = GetDatosPersonales(identifier, pConfig);
            persona.DatosContacto = GetDatosContacto(identifier, pConfig);
            persona.Vinculacion = GetVinculacion(identifier, pConfig);
            persona.DatosAcademicos = GetDatosAcademicos(identifier, pConfig);
            persona.Fotografia = GetFotografia(identifier, pConfig);
            persona.Sexenios = GetSexenios(identifier, pConfig);
            return persona;
        }

        private static DatosPersonales GetDatosPersonales(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            DatosPersonales datosPersonales = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "datos-personales/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosPersonales = JsonConvert.DeserializeObject<DatosPersonales>(response.Content);
            return datosPersonales;
        }

        private static DatosContacto GetDatosContacto(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            DatosContacto datosContacto = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "datos-contacto/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosContacto = JsonConvert.DeserializeObject<DatosContacto>(response.Content);
            return datosContacto;
        }

        private static Vinculacion GetVinculacion(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Vinculacion vinculacion = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "vinculaciones/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            vinculacion = JsonConvert.DeserializeObject<Vinculacion>(response.Content);
            return vinculacion;
        }

        private static DatosAcademicos GetDatosAcademicos(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            DatosAcademicos datosAcademicos = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "datos-academicos/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosAcademicos = JsonConvert.DeserializeObject<DatosAcademicos>(response.Content);
            return datosAcademicos;
        }
        private static Fotografia GetFotografia(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Fotografia fotografia = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "personas/" + id + "/fotografia");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            fotografia = JsonConvert.DeserializeObject<Fotografia>(response.Content);
            return fotografia;
        }

        private static Sexenio GetSexenios(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Sexenio sexenios = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "sexenios/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            sexenios = JsonConvert.DeserializeObject<Sexenio>(response.Content);
            return sexenios;
        }
    }
}
