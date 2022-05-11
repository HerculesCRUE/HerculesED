using Hercules.ED.GraphicEngine.Config;
using Hercules.ED.GraphicEngine.Models;
using Hercules.ED.GraphicEngine.Models.Graficas;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Hercules.ED.GraphicEngine.Controllers
{
    [ApiController]
    [Route("[action]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class GraphicControllers : ControllerBase
    {
        readonly ConfigService _Configuracion;

        public GraphicControllers(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public GraficaBase GetGrafica(string pIdPagina, string pIdGrafica, string pFiltros,string pLang)
        {
            return Models.GraphicEngine.GetGrafica(pIdPagina, pIdGrafica, pFiltros,pLang);
        }
    }
}
