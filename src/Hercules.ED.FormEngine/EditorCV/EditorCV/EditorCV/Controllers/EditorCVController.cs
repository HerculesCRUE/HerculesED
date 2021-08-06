using EditorCV.Models;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Helpers;
using Gnoss.ApiWrapper.OAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EditorCV.Controllers.Properties;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace EditorCV.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class EditorCVController : ControllerBase
    {
        private static ResourceApi resourceApi = new ResourceApi("Config/ConfigOauth/OauthV3.config", null, new LogHelperFile("c:\\logs\\", "logCV"));

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public JsonStructure Post([FromBody][Required] DataJsonStructure data)
        {
            // var txt = JsonConvert.DeserializeObject<JsonStructure>(data);
            return data.data;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public JsonStructure Get(string id, string rdftype)
        {
            // El parámetro ID sería el identificador de la entidad a recuperar (no es obligatorio si se quiere crear una nueva entidad)
            // El parámetro rdftype sería el rdftype de la entidad a recuperar (obligatorio)
            // El parámetro tipoplant sirve para determinar el tipo de plantilla a recuperar
            // Este método debería leer el fichero json de configuración y realizar consultas a la API para recuperar los datos de la entidad.

            id = "http://gnoss.com/items/Document_6c5ad967-1775-4960-896b-932a2e407d97_dfd9016f-a9b5-4023-9b22-85c760f187d1";
            // id = "http://gnoss.com/items/DocumentCategoryPath_6c5ad967-1775-4960-896b-932a2e407d97_e361a558-3769-47aa-bd20-accd6ae01647";
            rdftype = "http://purl.org/roh/mirror/bibo#Document";
            // string onto = "asiodocument";
            //Gnoss.ApiWrapper.ApiModel.
            //OAuthInfo oAuthInfo=new OAuthInfo()

            var lItems = new LoadItems2(id);

            return lItems.GetMainEntity();
            // return null;
        }


    }
}
