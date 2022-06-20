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
        public List<DataGraphicUser> GetGraficasUser(string pPageId)
        {
            return Models.GraphicEngine.GetGraficasUserByPageId(pPageId);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void CrearPagina(string pUserId, string pTitulo)
        {
            Models.GraphicEngine.CrearPaginaUsuario(pUserId, pTitulo);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult GetCSVGrafica(string pIdPagina, string pIdGrafica, string pFiltroFacetas, string pLang, string pTitulo = "Gráfica")
        {
            return File(Models.GraphicEngine.GetGrafica(pIdPagina, pIdGrafica, pFiltroFacetas, pLang).GenerateCSV(), "application/CSV", pTitulo + ".csv");
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Faceta GetFaceta(string pIdPagina, string pIdFaceta, string pFiltroFacetas, string pLang,bool pGetAll=false)
        {
            return Models.GraphicEngine.GetFaceta(pIdPagina, pIdFaceta, pFiltroFacetas, pLang, pGetAll);
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void GuardarGrafica(string pTitulo, string pAnchura, string pIdPaginaGrafica, string pIdGrafica, string pFiltros, string pUserId, string pIdRecursoPagina = null, string pTituloPagina = null)
        {
            Models.GraphicEngine.GuardarGrafica(pTitulo, pAnchura, pIdPaginaGrafica, pIdGrafica, pFiltros, pUserId, pIdRecursoPagina, pTituloPagina);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void BorrarGrafica(string pUserId, string pPageID, string pGraphicID)
        {
            Models.GraphicEngine.BorrarGrafica(pUserId, pPageID, pGraphicID);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void BorrarPagina(string pUserId, string pPageID)
        {
            Models.GraphicEngine.BorrarPagina(pUserId, pPageID);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void EditarNombrePagina(string pUserId, string pPageID, string pNewTitle, string pOldTitle)
        {
            Models.GraphicEngine.EditarNombrePagina(pUserId, pPageID, pNewTitle, pOldTitle);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void EditarOrdenPagina(string pUserId, string pPageID, int pNewOrder, int pOldOrder)
        {
            Models.GraphicEngine.EditarOrdenPagina(pUserId, pPageID, pNewOrder, pOldOrder);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void EditarNombreGrafica(string pUserId, string pPageID, string pGraphicID, string pNewTitle, string pOldTitle)
        {
            Models.GraphicEngine.EditarNombreGrafica(pUserId, pPageID, pGraphicID, pNewTitle, pOldTitle);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void EditarOrdenGrafica(string pUserId, string pPageID, string pGraphicID, int pNewOrder, int pOldOrder)
        {
            Models.GraphicEngine.EditarOrdenGrafica(pUserId, pPageID, pGraphicID, pNewOrder, pOldOrder);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void EditarAnchuraGrafica(string pUserId, string pPageID, string pGraphicID, int pNewWidth, int pOldWidth)
        {
            Models.GraphicEngine.EditarAnchuraGrafica(pUserId, pPageID, pGraphicID, pNewWidth, pOldWidth);
        }
    }
}
