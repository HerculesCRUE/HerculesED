using Newtonsoft.Json;
using OAI_PMH.Controllers;
using OAI_PMH.Models.SGI.ProteccionIndustrialIntelectual;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OAI_PMH.Services
{
    public class Invention
    {
        public static Dictionary<string, DateTime> GetModifiedInvenciones(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig, pTokenGestor: false, pTokenPii: true);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseInvenciones() + "invenciones/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    string idMod = "Invencion_" + id.Replace("\"", "");
                    if (!idDictionary.ContainsKey(idMod))
                    {
                        idDictionary.Add(idMod, DateTime.UtcNow);
                    }
                }
            }
            return idDictionary;
        }

        public static Invencion GetInvenciones(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig, pTokenGestor: false, pTokenPii: true);
            string identifier = id.Replace("\"", "").Split('_')[1];
            RestClient client = new(pConfig.GetUrlBaseInvenciones() + "invenciones/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Invencion invencion = JsonConvert.DeserializeObject<Invencion>(response.Content);
            invencion.sectoresAplicacion = GetSectores(identifier, pConfig);
            invencion.invencionDocumentos = GetDocumentos(identifier, pConfig);
            invencion.gastos = GetGastos(identifier, pConfig);
            invencion.palabrasClave = GetPalabrasClaves(identifier, pConfig);
            invencion.inventores = GetInventores(identifier, pConfig);
            return invencion;
        }

        public static List<SectorAplicacion> GetSectores(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig, pTokenGestor: false, pTokenPii: true);
            string identifier = id.Replace("\"", ""); 
            RestClient client = new(pConfig.GetUrlBaseInvenciones() + "invenciones/" + identifier + "/sectoresaplicacion");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                return JsonConvert.DeserializeObject<List<SectorAplicacion>>(response.Content);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<InvencionDocumento> GetDocumentos(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig, pTokenGestor: false, pTokenPii: true);
            string identifier = id.Replace("\"", "");
            RestClient client = new(pConfig.GetUrlBaseInvenciones() + "invenciones/" + identifier + "/invenciondocumentos");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                return JsonConvert.DeserializeObject<List<InvencionDocumento>>(response.Content);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<InvencionGastos> GetGastos(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig, pTokenGestor: false, pTokenPii: true);
            string identifier = id.Replace("\"", "");
            RestClient client = new(pConfig.GetUrlBaseInvenciones() + "invenciones/" + identifier + "/gastos");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                return JsonConvert.DeserializeObject<List<InvencionGastos>>(response.Content);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<PalabraClave> GetPalabrasClaves(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig, pTokenGestor: false, pTokenPii: true);
            string identifier = id.Replace("\"", ""); 
            RestClient client = new(pConfig.GetUrlBaseInvenciones() + "invenciones/" + identifier + "/palabrasclave");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                return JsonConvert.DeserializeObject<List<PalabraClave>>(response.Content);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<Inventor> GetInventores(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig, pTokenGestor: false, pTokenPii: true);
            string identifier = id.Replace("\"", ""); 
            RestClient client = new(pConfig.GetUrlBaseInvenciones() + "inventores/" + identifier + "/invencion-inventores");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            try
            {
                return JsonConvert.DeserializeObject<List<Inventor>>(response.Content);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
