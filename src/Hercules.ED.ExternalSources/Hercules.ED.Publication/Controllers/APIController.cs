using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;
using Newtonsoft.Json;
using PublicationAPI.Controllers;
using Serilog;

namespace PublicationConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("Publication/[action]")]
    public class APIController : ControllerBase
    {
        readonly ConfigService _Configuracion;

        public APIController(ConfigService pConfig)
        {
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
            Log.Information("Leyendo Configuración...");
            ROPublicationLogic PublicationObject = new ROPublicationLogic(_Configuracion);//,almacenamiento.metricas_scopus, almacenamiento.metricas_WoS);
            Log.Information("Obteniendo datos de publicación...");
            List<Publication> publication = PublicationObject.getPublications(orcid, date);
            return publication;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<Publication> GetRoPublication([FromQuery][Required] string pDoi, string pNombreCompletoAutor = null)
        {
            Log.Information("Leyendo Configuración...");
            ROPublicationLogic PublicationObject = new ROPublicationLogic(_Configuracion);//,almacenamiento.metricas_scopus, almacenamiento.metricas_WoS);
            Log.Information("Obteniendo datos de publicación...");
            List<Publication> publication = PublicationObject.getPublications("", pDoi: pDoi, pNombreCompletoAutor: pNombreCompletoAutor);
            return publication;
        }
    }
}

