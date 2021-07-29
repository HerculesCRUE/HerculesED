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
        public Author_maite GetROs([FromQuery][Required] string author_id)
        {
            ROScopusController ScopusObject = new ROScopusController("https://api.elsevier.com/", "75f4ab3fac56f42ac83cdeb7c98882ca");
            Author_maite author = ScopusObject.Author(author_id);

            //List<Publication> publications = ScopusObject.getAllPublication(user_orcid);
            var respublications = JsonConvert.SerializeObject(author);
            // Return the repository
            Console.Write(author);
            return author;
        }
        /// </remarks>
        /// <param name="doi">The user id in the application</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Invalid app</response> 
        /// <response code="500">Oops! Something went wrong</response> 
    
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Publication Publication([FromQuery][Required] string doi)
        {
            ROScopusController ScopusObject = new ROScopusController("https://api.elsevier.com/", "75f4ab3fac56f42ac83cdeb7c98882ca");
            Publication publication = ScopusObject.Publication(doi);

            //List<Publication> publications = ScopusObject.getAllPublication(user_orcid);
            var respublications = JsonConvert.SerializeObject(publication);
            // Return the repository
            
            return publication;
        }
    
     /// </remarks>
        /// <param name="name_2">The user id in the application</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Invalid app</response> 
        /// <response code="500">Oops! Something went wrong</response> 
    
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Publication Publication_2([FromQuery][Required] string name)
        {
            ROScopusController ScopusObject = new ROScopusController("https://api.elsevier.com/", "75f4ab3fac56f42ac83cdeb7c98882ca");
            Publication publication = ScopusObject.Publication_2(name);

            //List<Publication> publications = ScopusObject.getAllPublication(user_orcid);
            var respublications = JsonConvert.SerializeObject(publication);
            // Return the repository
            
            return publication;
        }
    }}

