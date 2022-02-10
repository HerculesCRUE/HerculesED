using FigShareAPI.Models;
using FigShareAPI.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FigShareAPI.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("FigShare/[action]")]
    public class APIController : ControllerBase
    {
        private readonly ILogger<APIController> _logger;
        readonly ConfigService _Configuracion;

        public APIController(ILogger<APIController> logger, ConfigService pConfig)
        {
            _logger = logger;
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Permite obtener la lista de identficadores de los artículos de un investigador.
        /// </summary>
        /// <returns>Lista de identificadores.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<int> GetIdentifiers()
        {
            FigShare figShare = new FigShare(_Configuracion);
            return figShare.getIdentifiers();
        }

        /// <summary>
        /// Permite obtener la información de los artículos de un investigador.
        /// </summary>
        /// <returns>Lista de articulos.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<Article> GetData()
        {
            List<int> identificadores = GetIdentifiers();

            FigShare figShare = new FigShare(_Configuracion);
            return figShare.getData(identificadores);
        }

        /// <summary>
        /// Permite obtener la lista de objetos RO con los datos necesarios de los artículos.
        /// </summary>
        /// <returns>Lista de ROs.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<RO> GetROs()
        {
            List<int> identificadores = GetIdentifiers();
            List<Article> articulos = GetData();

            FigShare figShare = new FigShare(_Configuracion);
            return figShare.getROs(articulos);
        }
    }
}
