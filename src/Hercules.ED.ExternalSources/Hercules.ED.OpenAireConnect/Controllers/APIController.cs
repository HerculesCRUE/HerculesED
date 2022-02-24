using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenAireConnect.ROs.OpenAire.Controllers;
using OpenAireConnect.ROs.OpenAire.Models;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using OpenAireAPI.Controllers;

namespace OpenAireConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("OpenAire/[action]")]
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
        ///     GET /OpenAire/GetROs?orcid=--
        ///     GET http://localhost:5000/OpenAire/GetROs?orcid=
        /// </remarks>
        /// <param orcid="orcid">Orcid</param>
        /// <param date="Year-Month-Day">Orcid</param>
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
            ROOpenAireController OpenAireObject = new ROOpenAireController(_Configuracion.GetUrlOpenAire());
            List<Publication> publication = OpenAireObject.getPublications(orcid, date);
            return publication;
        }


        /// <summary>
        /// Permite obtener la información de una publicación mediante el identificador de OpenAire.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación.</param>
        /// <returns>Objeto con los datos recuperados.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Publication GetRoByDoi([FromQuery][Required] string pDoi)
        {
            ROOpenAireController OpenAireObject = new ROOpenAireController(_Configuracion.GetUrlOpenAire());
            Publication publication = OpenAireObject.getPublicationDoi(pDoi);
            return publication;
        }
    }
}

