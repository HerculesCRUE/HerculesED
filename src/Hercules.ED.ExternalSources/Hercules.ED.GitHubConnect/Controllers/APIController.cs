using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GitHubAPI.ROs.Codes.Controllers;
using GitHubAPI.ROs.Codes.Models;
using GitHubAPI.ROs.Codes.Models.Inicial;

using Newtonsoft.Json;

namespace GitHubAPI.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("github/[action]")]
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
        ///     GET /github/GetROs?user=danijmj&amp;userToken=djtrdfjhdfg_dhretuhefhdfgjeru56jd5jemndskqqhgd&amp;appToken=djtrdfjhdfg_dhretuhefhdfgjeru56jd5jemndskqqhgd
        ///     GET /github/GetROs?user=githubuser&amp;userToken=djhrdfjhdfg_dhretuefhdfgdjeru56jd5jemndskqqhgd&amp;appToken=djhrdfjhdfg_dhretuefhdfgdjeru56jd5jemndskqqhgd
        ///
        /// </remarks>
        /// <param name="user">The user id in the application</param>
        /// <param name="userToken">Token for the user.</param>
        /// <param name="appToken">app token to connect.</param>
        /// <param name="consumerKey">Consumer key.</param>
        /// <param name="consumerSecret">Consumer secret.</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Invalid app</response> 
        /// <response code="500">Oops! Something went wrong</response> 
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<repositorio_roh> GetROs([FromQuery][Required] string user, [FromQuery][Required] string userToken, [FromQuery][Required] string appToken, [FromQuery] string consumerKey = null, [FromQuery] string consumerSecret = null)
        {


            if (string.IsNullOrEmpty(user))
            {
                return null;
            } 
            else if (string.IsNullOrEmpty(userToken))
            {
                return null;
            }  
            // Get all repositoriess from a user
            ROGitHubController gitHubObject = new ROGitHubController("https://api.github.com", "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");
            //ROGitHubController gitHubObject = new ROGitHubController("https://api.github.com", userToken);
            List<repositorio_roh> repositories = gitHubObject.getAllRepositories(user);
            var respositories = JsonConvert.SerializeObject(repositories);
            // Return the repository
            return repositories;
            
        }


    }

}
