using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAI_PMH.Models.SGI.Organization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Services
{
    class Organization
    {
        private static readonly string url = "http://sgi.ic.corp.treelogic.com/api/sgemp/";

        public static Dictionary<string, DateTime> GetModifiedOrganizations(string from)
        {
            string accessToken = Token.CheckToken();
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(url + "empresas/modificadas-ids?q=fechaModificacion=ge=\"" + from + "\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("Organizacion_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }

        public static Empresa GetEmpresa(string id)
        {
            string accessToken = Token.CheckToken();
            string identifier = id.Split('_')[1];
            Empresa empresa = new();
            RestClient client = new(url + "empresas/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            empresa = JsonConvert.DeserializeObject<Empresa>(json.ToString());
            empresa.DatosContacto = GetDatosContacto(identifier);
            empresa.DatosTipoEmpresa = GetDatosTipoEmpresa(identifier);
            return empresa;
        }

        private static DatosContacto GetDatosContacto(string id)
        {
            string accessToken = Token.CheckToken();
            DatosContacto datosContacto = new();
            RestClient client = new(url + "datos-contacto/empresa/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosContacto = JsonConvert.DeserializeObject<DatosContacto>(response.Content);
            return datosContacto;
        }

        private static DatosTipoEmpresa GetDatosTipoEmpresa(string id)
        {
            string accessToken = Token.CheckToken();
            DatosTipoEmpresa datosTipoEmpresa = new();
            RestClient client = new(url + "datos-tipo-empresa/empresa/" + id);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            datosTipoEmpresa = JsonConvert.DeserializeObject<DatosTipoEmpresa>(response.Content);
            return datosTipoEmpresa;
        }
    }
}
