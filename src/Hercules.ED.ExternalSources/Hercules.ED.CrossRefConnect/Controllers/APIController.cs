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
using System.Data;
using System.IO;
using ClosedXML.Excel;
using System.Text;
using ExcelDataReader;
using CrossRefAPI.ROs.CrossRef.Models;

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
        /// Obiene los datos de una publicación
        /// </summary>
        /// <param name="DOI">El identificador DOI de la publicación a obtener sus datos</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<PubReferencias> GetROs([FromQuery][Required] string DOI)
        {
            ROCrossRefLogic CrossRefObject = new ROCrossRefLogic();
            List<PubReferencias> listaReferencias = CrossRefObject.getPublications(DOI);
            return listaReferencias;
        }
    }
}

