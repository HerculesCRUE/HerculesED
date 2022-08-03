using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScopusConnect.ROs.Scopus.Controllers;
using ScopusConnect.ROs.Scopus.Models;
using Newtonsoft.Json;
namespace ScopusConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("Scopus/[action]")]
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
        /// <param orcid="orcid">Orcid</param>
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
        public List<Publication> GetROs([FromQuery][Required] string orcid, string date = "1500-01-01")
        {
            ROScopusLogic ScopusObject = new ROScopusLogic();//"75f4ab3fac56f42ac83cdeb7c98882ca");//"adf94bebeeba8c3042ad5193455740e2");
            List<Publication> publication = ScopusObject.getPublications(orcid, date);
            return publication;
        }

        /// <summary>
        /// Permite obtener una publicación mediante su DOI.
        /// </summary>
        /// <param name="pDoi">Identificador DOI de la publicación a buscar.</param>
        /// <returns>Objeto con los datos recuperados.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]        
        public Publication GetPublicationByDOI([FromQuery][Required] string pDoi)
        {
            ROScopusLogic ScopusObject = new ROScopusLogic();
            Publication publication = ScopusObject.getPublicationDoi(pDoi);
            return publication;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public float GetIndiceH([FromQuery][Required] string pOrcid)
        {
            ROScopusLogic ScopusObject = new ROScopusLogic();
            return ScopusObject.getHIndex(pOrcid);
        }
    }
}

