using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;
using Newtonsoft.Json;
using PublicationAPI.Controllers;
using System;

namespace PublicationConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("Publication/[action]")]
    public class APIController : ControllerBase
    {
        readonly ConfigService _Configuracion;

        public APIController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Obtiene los datos de las publicaciones por el ORCID del usuario.
        /// </summary>
        /// <param name="orcid">ORCID del usuario.</param>
        /// <param name="date">Fecha de obtención de publicaciones.</param>
        /// <returns>Listado de objetos con los datos de las publicaciones.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<Publication> GetROs([FromQuery][Required] string orcid, string date = "1500-01-01")
        {
            Console.WriteLine("Leyendo Configuración...");
            ROPublicationLogic PublicationObject = new(_Configuracion);
            Console.WriteLine("Obteniendo datos de publicación...");
            List<Publication> publication = PublicationObject.GetPublications(orcid, date);
            return publication;
        }

        /// <summary>
        /// Obtiene los datos de la publicación por DOI.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación a obtener los datos.</param>
        /// <param name="pNombreCompletoAutor">Nombre completo del autor.</param>
        /// <returns>Lista con una única publicación.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<Publication> GetRoPublication([FromQuery][Required] string pDoi, string pNombreCompletoAutor = null)
        {
            Console.WriteLine("Leyendo Configuración...");
            ROPublicationLogic PublicationObject = new(_Configuracion);
            Console.WriteLine("Obteniendo datos de publicación...");
            List<Publication> publication = PublicationObject.GetPublications("", pDoi: pDoi, pNombreCompletoAutor: pNombreCompletoAutor);
            return publication;
        }
    }
}

