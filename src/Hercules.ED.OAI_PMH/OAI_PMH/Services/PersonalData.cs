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

            #region --- Personal Data
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
            #endregion

            #region --- Formación Académica
            Dictionary<string, DateTime> dicFormacionAcademica = AcademicFormation.GetModifiedCiclos(from, pConfig);
            foreach(KeyValuePair<string, DateTime> item in dicFormacionAcademica)
            {
                string idPersona = "Persona_" + item.Key.Split("_")[1];
                if (!idDictionary.ContainsKey(idPersona))
                {
                    idDictionary.Add(idPersona, DateTime.UtcNow);
                }
                else
                {
                    if (DateTime.Compare(item.Value, idDictionary[idPersona]) == 1)
                    {
                        idDictionary[idPersona] = item.Value;
                    }
                }
            }

            dicFormacionAcademica = AcademicFormation.GetModifiedDoctorados(from, pConfig, accessToken);
            foreach (KeyValuePair<string, DateTime> item in dicFormacionAcademica)
            {
                string idPersona = "Persona_" + item.Key.Split("_")[1];
                if (!idDictionary.ContainsKey(idPersona))
                {
                    idDictionary.Add(idPersona, DateTime.UtcNow);
                }
                else
                {
                    if (DateTime.Compare(item.Value, idDictionary[idPersona]) == 1)
                    {
                        idDictionary[idPersona] = item.Value;
                    }
                }
            }

            dicFormacionAcademica = AcademicFormation.GetModifiedPosgrado(from, pConfig, accessToken);
            foreach (KeyValuePair<string, DateTime> item in dicFormacionAcademica)
            {
                string idPersona = "Persona_" + item.Key.Split("_")[1];
                if (!idDictionary.ContainsKey(idPersona))
                {
                    idDictionary.Add(idPersona, DateTime.UtcNow);
                }
                else
                {
                    if (DateTime.Compare(item.Value, idDictionary[idPersona]) == 1)
                    {
                        idDictionary[idPersona] = item.Value;
                    }
                }
            }

            dicFormacionAcademica = AcademicFormation.GetModifiedEspecializada(from, pConfig, accessToken);
            foreach (KeyValuePair<string, DateTime> item in dicFormacionAcademica)
            {
                string idPersona = "Persona_" + item.Key.Split("_")[1];
                if (!idDictionary.ContainsKey(idPersona))
                {
                    idDictionary.Add(idPersona, DateTime.UtcNow);
                }
                else
                {
                    if (DateTime.Compare(item.Value, idDictionary[idPersona]) == 1)
                    {
                        idDictionary[idPersona] = item.Value;
                    }
                }
            }
            #endregion

            #region --- Actividad Docente
            Dictionary<string, DateTime> dicActividadDocente = DocentActivity.GetModifiedTesis(from, pConfig, accessToken);
            foreach (KeyValuePair<string, DateTime> item in dicActividadDocente)
            {
                string idPersona = "Persona_" + item.Key.Split("_")[1];
                if (!idDictionary.ContainsKey(idPersona))
                {
                    idDictionary.Add(idPersona, DateTime.UtcNow);
                }
                else
                {
                    if (DateTime.Compare(item.Value, idDictionary[idPersona]) == 1)
                    {
                        idDictionary[idPersona] = item.Value;
                    }
                }
            }

            dicActividadDocente = DocentActivity.GetModifiedAcademicFormationProvided(from, pConfig, accessToken);
            foreach (KeyValuePair<string, DateTime> item in dicActividadDocente)
            {
                string idPersona = "Persona_" + item.Key.Split("_")[1];
                if (!idDictionary.ContainsKey(idPersona))
                {
                    idDictionary.Add(idPersona, DateTime.UtcNow);
                }
                else
                {
                    if (DateTime.Compare(item.Value, idDictionary[idPersona]) == 1)
                    {
                        idDictionary[idPersona] = item.Value;
                    }
                }
            }

            dicActividadDocente = DocentActivity.GetModifiedSeminars(from, pConfig, accessToken);
            foreach (KeyValuePair<string, DateTime> item in dicActividadDocente)
            {
                string idPersona = "Persona_" + item.Key.Split("_")[1];
                if (!idDictionary.ContainsKey(idPersona))
                {
                    idDictionary.Add(idPersona, DateTime.UtcNow);
                }
                else
                {
                    if (DateTime.Compare(item.Value, idDictionary[idPersona]) == 1)
                    {
                        idDictionary[idPersona] = item.Value;
                    }
                }
            }
            #endregion

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
            if(string.IsNullOrEmpty(response.Content))
            {
                return null;
            }
            var json = JObject.Parse(response.Content);
            persona = JsonConvert.DeserializeObject<Persona>(json.ToString());
            persona.DatosPersonales = GetDatosPersonales(identifier, pConfig, accessToken);
            persona.DatosContacto = GetDatosContacto(identifier, pConfig, accessToken);
            persona.Vinculacion = GetVinculacion(identifier, pConfig, accessToken);
            persona.DatosAcademicos = GetDatosAcademicos(identifier, pConfig, accessToken);
            persona.Fotografia = GetFotografia(identifier, pConfig, accessToken);
            persona.Sexenios = GetSexenios(identifier, pConfig, accessToken);
            persona.FormacionAcademicaImpartida = DocentActivity.GetAcademicFormationProvided(identifier, pConfig, accessToken);
            persona.SeminariosCursos = DocentActivity.GetSeminars(identifier, pConfig, accessToken);
            persona.Tesis = DocentActivity.GetTesis(identifier, pConfig, accessToken);
            persona.Ciclos = AcademicFormation.GetFormacionAcademicaCiclos(identifier, pConfig, accessToken);
            persona.Doctorados = AcademicFormation.GetFormacionAcademicaDoctorados(identifier, pConfig, accessToken);
            persona.FormacionEspecializada = AcademicFormation.GetFormacionAcademicaEspecializada(identifier, pConfig, accessToken);
            persona.Posgrado = AcademicFormation.GetFormacionAcademicaPosgrado(identifier, pConfig, accessToken);
            return persona;
        }

        private static DatosPersonales GetDatosPersonales(string id, ConfigService pConfig, string accessToken)
        {
            DatosPersonales datosPersonales = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "datos-personales/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosPersonales = JsonConvert.DeserializeObject<DatosPersonales>(response.Content);
            return datosPersonales;
        }

        private static DatosContacto GetDatosContacto(string id, ConfigService pConfig, string accessToken)
        {
            DatosContacto datosContacto = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "datos-contacto/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosContacto = JsonConvert.DeserializeObject<DatosContacto>(response.Content);
            return datosContacto;
        }

        private static Vinculacion GetVinculacion(string id, ConfigService pConfig, string accessToken)
        {
            Vinculacion vinculacion = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "vinculaciones/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                vinculacion = JsonConvert.DeserializeObject<Vinculacion>(response.Content);
            }
            catch
            {
                return null;
            }
            return vinculacion;
        }

        private static VinculacionCategoriaProfesional GetVinculacionCategoriaProfesional(string id, ConfigService pConfig, string accessToken)
        {
            VinculacionCategoriaProfesional vinculacion = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "vinculaciones/persona/" + id + "/vinculaciones-categorias-profesionales");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            vinculacion = JsonConvert.DeserializeObject<VinculacionCategoriaProfesional>(response.Content);
            return vinculacion;
        }

        private static DatosAcademicos GetDatosAcademicos(string id, ConfigService pConfig, string accessToken)
        {
            DatosAcademicos datosAcademicos = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "datos-academicos/persona/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosAcademicos = JsonConvert.DeserializeObject<DatosAcademicos>(response.Content);
            return datosAcademicos;
        }

        private static Fotografia GetFotografia(string id, ConfigService pConfig, string accessToken)
        {
            Fotografia fotografia = new();
            RestClient client = new(pConfig.GetUrlBasePersona() + "personas/" + id + "/fotografia");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            fotografia = JsonConvert.DeserializeObject<Fotografia>(response.Content);
            return fotografia;
        }

        private static Sexenio GetSexenios(string id, ConfigService pConfig, string accessToken)
        {
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
