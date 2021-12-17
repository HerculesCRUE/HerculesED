using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenCitationsConnect.ROs.OpenCitations.Controllers;
using OpenCitationsConnect.ROs.OpenCitations.Models;
using OpenCitationsConnect.ROs.OpenCitations.Models.Inicial;
using Newtonsoft.Json;

namespace OpenCitationsConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("OpenCitations/[action]")]
    public class APIController : ControllerBase
    {
        private readonly ILogger<APIController> _logger;
        public APIController(ILogger<APIController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Get all repositories from a specified user account and RO
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /OpenCitations/GetROs?doi=&amp;year=2020
        /// </remarks>
        /// <param doi="doi">Orcid</param>
        /// <param date="date">Year-month-day</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Invalid app</response> 
        /// <response code="500">Oops! Something went wrong</response> 
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Publication GetROs([FromQuery][Required] string doi)
        {
            ROOpenCitationsController OpenCitationsObject = new ROOpenCitationsController("https://w3id.org/oc/index/api/v1");
            Publication publication = OpenCitationsObject.getPublications(doi);
            return publication;
        }
        
    
    }}

