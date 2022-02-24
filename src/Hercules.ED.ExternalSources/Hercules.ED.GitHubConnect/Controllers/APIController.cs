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
using GitHubAPI.Models.Data;

namespace GitHubAPI.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("github/[action]")]
    public class APIController : ControllerBase
    {
        private readonly ILogger<APIController> _logger;
        readonly ConfigService _Configuracion;

        public APIController(ILogger<APIController> logger, ConfigService pConfig)
        {
            _logger = logger;
            _Configuracion = pConfig;
        }

        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public List<repositorio_roh> GetROs([FromQuery][Required] string user, [FromQuery][Required] string userToken, [FromQuery][Required] string appToken, [FromQuery] string consumerKey = null, [FromQuery] string consumerSecret = null)
        //{


        //    if (string.IsNullOrEmpty(user))
        //    {
        //        return null;
        //    } 
        //    else if (string.IsNullOrEmpty(userToken))
        //    {
        //        return null;
        //    }  
        //    // Get all repositoriess from a user
        //    ROGitHubController gitHubObject = new ROGitHubController("https://api.github.com", "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");
        //    //ROGitHubController gitHubObject = new ROGitHubController("https://api.github.com", userToken);
        //    List<repositorio_roh> repositories = gitHubObject.getAllRepositories(user);
        //    var respositories = JsonConvert.SerializeObject(repositories);
        //    // Return the repository
        //    return repositories;
            
        //}
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<DataGitHub> GetData([FromQuery][Required] string pUser, [FromQuery][Required] string pToken)
        {
            GitHub github = new GitHub(_Configuracion);
            return github.getData(pUser, pToken);
        }
    }
}
