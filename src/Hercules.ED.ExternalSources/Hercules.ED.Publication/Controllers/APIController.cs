using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;
using PublicationConnect.Controllers;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;
using Newtonsoft.Json;
using PublicationConnect.Controllers.autores;
using PublicationAPI.Controllers;

namespace PublicationConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("Publication/[action]")]
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
        /// Get all repositories from a specified user account and RO
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /scopus/GetROs?orcid=XXXX-XXXX-XXXX-XXXX&amp;date=year-month-day
        /// </remarks>
        /// <param orcid="orcid">Orcid</param>
        /// <param year-month-day="year-month-day">Year-month-day</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Invalid app</response> 
        /// <response code="500">Oops! Something went wrong</response> 

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<Publication> GetROs([FromQuery][Required] string orcid, string date = "1500-01-01")
        {
            almacenamiento_autores almacenamiento = new almacenamiento_autores();

            ROPublicationLogic PublicationObject = new ROPublicationLogic("", _Configuracion,almacenamiento.metricas_scopus);

            List<Publication> publication = PublicationObject.getPublications(orcid, date);
            //almacenamiento.unificar_personas();
            //publication = almacenamiento.poner_usuarios(publication);
            //almacenamiento.guardar_info_autores();
            return publication;
            //  return null;
        }

    }
}

