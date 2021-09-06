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
    [Route("[controller]/[action]")]
    public class EditorCVController : ControllerBase
    {
        private static ResourceApi resourceApi = new ResourceApi("Config/ConfigOauth/OauthV3.config", null, new LogHelperFile("c:\\logs\\", "logCV"));

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public JsonStructure Save([FromBody][Required] DataJsonStructure data)
        {
            // var txt = JsonConvert.DeserializeObject<JsonStructure>(data);
            return data.data;
        }



        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public JsonStructure GetSection([FromQuery][Required] string id, [FromQuery][Required] string section, [FromQuery][Required] string idSection, [FromBody][Required] DataJsonStructure data)
        {
            LoadItems lItems = null;

            if (id != null)
            {
                lItems = new LoadItems(id);

                if (section != null && data != null)
                {
                    lItems.AddNewJson(data);
                    return lItems.GetSection(idSection, section);
                }
                else
                {
                    return lItems.GetMainEntity();
                }
            }


            return new JsonStructure();
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public JsonStructure LoadMain(string id)
        {
            LoadItems lItems = null;
            // El parámetro ID sería el identificador de la entidad a recuperar (no es obligatorio si se quiere crear una nueva entidad)
            // El parámetro rdftype sería el rdftype de la entidad a recuperar (obligatorio)
            // El parámetro tipoplant sirve para determinar el tipo de plantilla a recuperar
            // Este método debería leer el fichero json de configuración y realizar consultas a la API para recuperar los datos de la entidad.

            // id = "http://gnoss.com/items/Document_6c5ad967-1775-4960-896b-932a2e407d97_dfd9016f-a9b5-4023-9b22-85c760f187d1";
            // id = "http://gnoss.com/items/CV_1fca886e-da0b-770e-1171-963e7ca03db8_2eb3851b-5489-47b2-b541-f99b37d83922";
            // rdftype = "http://purl.org/roh/mirror/bibo#Document";
            // string onto = "asiodocument";
            //Gnoss.ApiWrapper.ApiModel.
            //OAuthInfo oAuthInfo=new OAuthInfo()
            if (id != null)
            {
                lItems = new LoadItems(id);
                lItems.DeserialiceTemplate();
                return lItems.GetMainEntity();
                
            }


            return new JsonStructure();
        }

    }
}
