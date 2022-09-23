using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using System.Text;
using ExcelDataReader;
using SemanticScholarAPI.ROs.SemanticScholar.Models;
using SemanticScholarAPI.ROs.SemanticScholar.Controllers;

namespace SemanticScholarAPI.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("SemanticScholar/[action]")]
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
        ///     GET /scopus/GetROs?author_id=SCOPUS_ID&amp;year=2020
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
            ROSemanticScholarLogic SemanticScholarObject = new ROSemanticScholarLogic();
            Publication publication = SemanticScholarObject.getPublications(doi);
            return publication;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Tuple<Publication, List<PubReferencias>> GetReferences([FromQuery][Required] string pDoi)
        {
            ROSemanticScholarLogic SemanticScholarObject = new ROSemanticScholarLogic();
            return SemanticScholarObject.getReferencias(pDoi);
        }
    }
}

