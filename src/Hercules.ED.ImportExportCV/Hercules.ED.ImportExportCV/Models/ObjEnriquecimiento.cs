using Hercules.ED.ImportExportCV.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hercules.ED.ImportExportCV.Models
{
    public class ObjEnriquecimiento
    {
        public string rotype { get; set; }
        public string title { get; set; }
        [JsonProperty("abstract")]
        public string abstract_ { get; set; }
        public ObjEnriquecimiento(string tituloPublicacion)
        {
            this.abstract_ = "";
            this.rotype = "papers";
            this.title = tituloPublicacion;
        }
        public ObjEnriquecimiento(string tituloPublicacion, string @abstract)
        {
            this.abstract_ = @abstract;
            this.rotype = "papers";
            this.title = tituloPublicacion;
        }

        public class Topics_enriquecidos
        {
            public string pdf_url { get; set; }
            public string rotype { get; set; }
            public List<Knowledge_enriquecidos> topics { get; set; }
        }

        public class Knowledge_enriquecidos
        {
            public string word { get; set; }
            public string porcentaje { get; set; }
        }

        public Dictionary<string, string> getDescriptores(ConfigService _Configuracion, ObjEnriquecimiento objEnriquecimiento, string pTipo)
        {
            string pDataEnriquecimiento = JsonConvert.SerializeObject(objEnriquecimiento);
            // Petición.
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            string result = string.Empty;
            var contentData = new StringContent(pDataEnriquecimiento, System.Text.Encoding.UTF8, "application/json");

            int intentos = 10;
            while (true)
            {
                try
                {
                    response = client.PostAsync($@"{_Configuracion.GetUrlEnriquecimiento()}/{pTipo}", contentData).Result;
                    break;
                }
                catch
                {
                    intentos--;
                    if (intentos == 0)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(intentos * 1000);
                    }
                }
            }

            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
            }

            if (string.IsNullOrEmpty(result))
            {
                Topics_enriquecidos data = null;
                try
                {
                    data = JsonConvert.DeserializeObject<Topics_enriquecidos>(result);
                }
                catch (Exception)
                {
                    return null;
                }

                if (data != null)
                {
                    Dictionary<string, string> dicTopics = new Dictionary<string, string>();
                    foreach (Knowledge_enriquecidos item in data.topics)
                    {
                        if (!dicTopics.ContainsKey(item.word))
                        {
                            dicTopics.Add(item.word, item.porcentaje);
                        }
                    }
                    return dicTopics;
                }
            }

            return null;
        }
    }
}
