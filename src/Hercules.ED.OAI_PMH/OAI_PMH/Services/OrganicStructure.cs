using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Services
{
    public class OrganicStructure
    {
        private static readonly string url = "http://sgi.ic.corp.treelogic.com/api/sgo/";

        public static Dictionary<string, DateTime> GetAreasConocimiento(string parentId)
        {
            string accessToken = Token.CheckToken();
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(url + "empresas/modificadas-ids?q=padreId=na=\"" + parentId + "\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            idList = response.Content[1..^1].Split(',').ToList();

            foreach (string id in idList)
            {
                idDictionary.Add("Organizacion_" + id, DateTime.UtcNow);
            }
            return idDictionary;
        }
    }
}
