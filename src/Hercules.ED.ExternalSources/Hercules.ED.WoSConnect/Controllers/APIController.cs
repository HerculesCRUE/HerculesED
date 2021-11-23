using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WoSConnect.ROs.WoS.Controllers;
using WoSConnect.ROs.WoS.Models;
using WoSConnect.ROs.WoS.Models.Inicial;
using Newtonsoft.Json;

namespace WoSConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("WoS/[action]")]
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
        ///     GET /WoS/GetROs?orcid=--
        ///     GET http://localhost:5000/WoS/GetROs?orcid=
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
        public List<Publication> GetROs([FromQuery][Required] string orcid,string date ="1500-01-01")
        {
            ROWoSController WoSObject = new ROWoSController("https://wos-api.clarivate.com/", "10e8a3a2417b7ae1d864b5558136c56b78ed3eb8");//"adf94bebeeba8c3042ad5193455740e2");
            List<Publication> publication = WoSObject.getPublications(orcid,date);
            return publication;
        }
        
    
    }}

