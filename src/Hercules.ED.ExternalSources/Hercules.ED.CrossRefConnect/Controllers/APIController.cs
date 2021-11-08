using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CrossRefConnect.ROs.CrossRef.Controllers;
using CrossRefConnect.ROs.CrossRef.Models;
using CrossRefConnect.ROs.CrossRef.Models.Inicial;
using Newtonsoft.Json;

namespace CrossRefConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("CrossRef/[action]")]
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
        /// <param name="doi">Orcid</param>
        /// <param date="date">Year-month-day</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Invalid app</response> 
        /// <response code="500">Oops! Something went wrong</response> 
        
        //public List<Publication> GetROs([FromQuery][Required] string user, [FromQuery][Required] string userToken, [FromQuery][Required] string appToken, [FromQuery] string consumerKey = null, [FromQuery] string consumerSecret = null)
        //{
            //if (string.IsNullOrEmpty(user))
           // {
           //     return null;
          //  } 
           // else if (string.IsNullOrEmpty(userToken))
           // {
            //    return null;
           // }
            // Get all publication from a user
            //ROScopusController ScopusObject = new ROScopusController("https://api.elsevier.com/", userToken);
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Publication GetROs([FromQuery][Required] string ID)
        {
            ROCrossRefController CrossRefObject = new ROCrossRefController("https://api.crossref.org/");//"adf94bebeeba8c3042ad5193455740e2");
            Publication publication = CrossRefObject.getPublications(ID);
            return publication;
        }
        
    
    }}

