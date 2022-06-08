using Hercules.ED.GraphicEngine.Config;
using Hercules.ED.GraphicEngine.Models;
using Hercules.ED.GraphicEngine.Models.Graficas;
using Hercules.ED.GraphicEngine.Models.Paginas;
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
        public GraficaBase GetGrafica(string pIdPagina, string pIdGrafica, string pFiltroFacetas, string pLang)
        {
            return Models.GraphicEngine.GetGrafica(pIdPagina, pIdGrafica, pFiltroFacetas, pLang);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<DataGraphicUser> GetGraficasUser(string pUserId)
        {
            return Models.GraphicEngine.GetGraficasUser(pUserId);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult GetCSVGrafica(string pIdPagina, string pIdGrafica, string pFiltroFacetas, string pLang)
        {
            return File(Models.GraphicEngine.GetGrafica(pIdPagina, pIdGrafica, pFiltroFacetas, pLang).GenerateCSV(), "application/CSV","graphic.csv");
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Faceta GetFaceta(string pIdPagina, string pIdFaceta, string pFiltroFacetas, string pLang)
        {
            return Models.GraphicEngine.GetFaceta(pIdPagina, pIdFaceta, pFiltroFacetas, pLang);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Pagina GetPaginaGrafica(string pIdPagina, string pLang)
        {
            return Models.GraphicEngine.GetPage(pIdPagina, pLang);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<Pagina> GetPaginasGraficas(string pLang)
        {
            return Models.GraphicEngine.GetPages(pLang);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<DataPageUser> GetPaginasUsuario(string pUserId)
        {
            return Models.GraphicEngine.GetPagesUser(pUserId);
        }
    }
}
