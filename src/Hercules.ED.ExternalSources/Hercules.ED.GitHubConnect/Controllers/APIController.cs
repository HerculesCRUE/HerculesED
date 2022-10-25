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
        readonly ConfigService _Configuracion;

        public APIController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }               
        
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
