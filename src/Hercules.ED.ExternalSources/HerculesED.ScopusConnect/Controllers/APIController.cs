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
    [Route("scopus/[action]")]
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
        ///     GET /scopus/GetROs?user=danijmj&amp;userToken=djtrdfjhdfg_dhretuhefhdfgjeru56jd5jemndskqqhgd&amp;appToken=djtrdfjhdfg_dhretuhefhdfgjeru56jd5jemndskqqhgd
        ///     GET /scopus/GetROs?user=githubuser&amp;userToken=djhrdfjhdfg_dhretuefhdfgdjeru56jd5jemndskqqhgd&amp;appToken=djhrdfjhdfg_dhretuefhdfgdjeru56jd5jemndskqqhgd
        /// </remarks>
        /// <param name="author_id">The user id in the application</param>
                /// <param year="year">The user id in the application</param>
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
        public List<Publication> GetROs([FromQuery][Required] string author_id,string year="1800")
        {
            ROScopusController ScopusObject = new ROScopusController("https://api.elsevier.com/", "adf94bebeeba8c3042ad5193455740e2");
            //Author_maite author = ScopusObject.Author(author_id);
            List<Publication> publication = ScopusObject.getPublications(author_id,year);
            //System.IO.StreamWriter outputFile = new System.IO.StreamWriter("ejemplo.txt");
            //outputFile.Write(publication.ToString());
            //outputFile.Close();

            //List<Publication> publications = ScopusObject.getAllPublication(user_orcid);
            //var respublications = JsonConvert.SerializeObject(author);
            // Return the repository
            //Console.Write(author);
            return publication;
        }
        
    
    }}

