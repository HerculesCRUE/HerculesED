using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private static readonly string url = "http://sgi.ic.corp.treelogic.com/api/sgp/";

        public static Dictionary<string, DateTime> GetModifiedPeople(string from)
        {
            string accessToken = Token.CheckToken();
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(url + "personas/modificadas-ids?q=fechaModificacion=ge=\"" + from + "\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("Persona_" + id.Substring(1, 8), DateTime.UtcNow);
                }
            }
            return idDictionary;
        }

        public static Persona GetPersona(string id)
        {
            string accessToken = Token.CheckToken();
            string identifier = id.Split('_')[1];
            Persona persona = new();
            RestClient client = new(url + "personas/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            persona = JsonConvert.DeserializeObject<Persona>(json.ToString());
            persona.DatosPersonales = GetDatosPersonales(identifier);
            persona.DatosContacto = GetDatosContacto(identifier);
            persona.Vinculacion = GetVinculacion(identifier);
            persona.DatosAcademicos = GetDatosAcademicos(identifier);
            persona.Fotografia = GetFotografia(identifier);
            persona.Sexenios = GetSexenios(identifier);
            return persona;
        }

        private static DatosPersonales GetDatosPersonales(string id)
        {
            string accessToken = Token.CheckToken();
            DatosPersonales datosPersonales = new();
            RestClient client = new(url + "datos-personales/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosPersonales = JsonConvert.DeserializeObject<DatosPersonales>(response.Content);
            return datosPersonales;
        }

        private static DatosContacto GetDatosContacto(string id)
        {
            string accessToken = Token.CheckToken();
            DatosContacto datosContacto = new();
            RestClient client = new(url + "datos-contacto/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosContacto = JsonConvert.DeserializeObject<DatosContacto>(response.Content);
            return datosContacto;
        }

        private static Vinculacion GetVinculacion(string id)
        {
            string accessToken = Token.CheckToken();
            Vinculacion vinculacion = new();
            RestClient client = new(url + "vinculaciones/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            vinculacion = JsonConvert.DeserializeObject<Vinculacion>(response.Content);
            return vinculacion;
        }

        private static DatosAcademicos GetDatosAcademicos(string id)
        {
            string accessToken = Token.CheckToken();
            DatosAcademicos datosAcademicos = new();
            RestClient client = new(url + "datos-academicos/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosAcademicos = JsonConvert.DeserializeObject<DatosAcademicos>(response.Content);
            return datosAcademicos;
        }
        private static Fotografia GetFotografia(string id)
        {
            string accessToken = Token.CheckToken();
            Fotografia fotografia = new();
            RestClient client = new(url + "personas/" + id + "fotografia");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            fotografia = JsonConvert.DeserializeObject<Fotografia>(response.Content);
            return fotografia;
        }

        private static List<Sexenio> GetSexenios(string id)
        {
            string accessToken = Token.CheckToken();
            List<Sexenio> sexenios = new();
            RestClient client = new(url + "sexenios/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            sexenios = JsonConvert.DeserializeObject<List<Sexenio>>(response.Content);
            return sexenios;
        }
    }
}
