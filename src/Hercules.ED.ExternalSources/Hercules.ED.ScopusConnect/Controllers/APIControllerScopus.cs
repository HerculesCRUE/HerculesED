using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<Publication> GetROs([FromQuery][Required] string orcid, string date = "1500-01-01")
        {
            ROScopusLogic ScopusObject = new ();
            List<Publication> publication = ScopusObject.GetPublications(orcid, date);
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
            ROScopusLogic ScopusObject = new ();
            Publication publication = ScopusObject.GetPublicationDoi(pDoi);
            return publication;
        }
    }
}

