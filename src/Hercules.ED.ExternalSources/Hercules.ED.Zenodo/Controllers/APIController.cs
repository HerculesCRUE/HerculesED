using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZenodoConnect.ROs.Zenodo.Controllers;
using Newtonsoft.Json;
using ZenodoAPI.Controllers;
using ZenodoAPI.Models;
using ZenodoAPI.Models.Data;
using System.Collections.Generic;

namespace ZenodoConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("Zenodo/[action]")]
    public class APIController : ControllerBase
    {
        readonly ConfigService _Configuracion;
        private Zenodo _Zenodo;
        private readonly ILogger<APIController> _logger;

        public APIController(ILogger<APIController> logger, ConfigService pConfig)
        {
            _Configuracion = pConfig;
            _Zenodo = new Zenodo(_Configuracion);
            _logger = logger;
        }
        /// <summary>
        /// Get all repositories from a specified user account and RO
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /scopus/GetROs?author_id=SCOPUS_ID&amp;year=2020
        /// </remarks>
        /// <param name="orcid">Orcid</param>
        /// <param date="date">Year-month-day</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Invalid app</response> 
        /// <response code="500">Oops! Something went wrong</response> 

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public string GetROs([FromQuery][Required] string ID)
        {
            ROZenodoLogic ZenodoObject = new ROZenodoLogic();//,"0grEw8zOOPjtlxyHLOQtUjTSSSx3FFrywNNb3YivsvpYZ4bIiCNCQBrbY7xh");
            //, "10e8a3a2417b7ae1d864b5558136c56b78ed3eb8");//"adf94bebeeba8c3042ad5193455740e2");
            string publication = ZenodoObject.getPublications(ID);
            return publication;
        }

        /// <summary>
        /// Obtiene los datos ofrecidos por el API de Zenodo.
        /// </summary>
        /// <param name="pOrcid">ORCID a consultar.</param>
        /// <returns>Datos.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<ResearchObject> GetROsByOrcid(string pOrcid)
        {
            return _Zenodo.getPublicationsOrcid(pOrcid);
        }

        /// <summary>
        /// Obtiene los datos necesarios para la carga en BBDD.
        /// </summary>
        /// <param name="pOrcid">ORCID a consultar.</param>
        /// <returns>Datos.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<OntologyRO> GetOntologyData(string pOrcid)
        {
            return _Zenodo.getOntologyData(pOrcid);
        }
    }
}

