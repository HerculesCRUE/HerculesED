using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZenodoConnect.ROs.Zenodo.Controllers;
using Newtonsoft.Json;
namespace ZenodoConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("Zenodo/[action]")]
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
        
    
    }}

