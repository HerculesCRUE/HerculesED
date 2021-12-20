using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SemanticScholarConnect.ROs.SemanticScholar.Controllers;
using SemanticScholarConnect.ROs.SemanticScholar.Models;
using SemanticScholarConnect.ROs.SemanticScholar.Models.Inicial;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using System.Text;
using ExcelDataReader;


namespace SemanticScholarConnect.Controllers
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
            ROSemanticScholarLogic SemanticScholarObject = new ROSemanticScholarLogic();//, "10e8a3a2417b7ae1d864b5558136c56b78ed3eb8");//"adf94bebeeba8c3042ad5193455740e2");
            Publication publication = SemanticScholarObject.getPublications(doi);
            return publication;
        }


    }
}

