using Hercules.ED.GraphicEngine.Config;
using Hercules.ED.GraphicEngine.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hercules.ED.GraphicEngine.Controllers
{
    public class FacetaControllers
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
            public Faceta GetFaceta(string pIdPagina, string pIdFaceta, string pFiltros, string pLang)
            {
                return Models.GraphicEngine.GetFaceta(pIdPagina, pIdFaceta, pFiltros, pLang);
            }
        }
    }
}
