using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;
using Newtonsoft.Json;
using PublicationAPI.Controllers;
using System;
using System.Linq;
using PublicationAPI.Models;
using Microsoft.Net.Http.Headers;

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

        /// <summary>
        /// Obtiene los ficheros en la carpeta de lectura.
        /// </summary>
        /// <returns>Lista con los ficheros en la carpeta de lectura.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<FileInfoEDMA> GetFilesLectura()
        {
            List<FileInfoEDMA> filesInfo = new List<FileInfoEDMA>();
            List<string> files = System.IO.Directory.EnumerateFiles(_Configuracion.GetRutaDirectorioLectura()).ToList();
            foreach (string file in files)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                filesInfo.Add(new FileInfoEDMA() { Name = fileInfo.Name, LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,BytesSize=fileInfo.Length });
            }
            filesInfo = filesInfo.OrderByDescending(x => x.LastWriteTimeUtc).ToList();
            return filesInfo;
        }

        /// <summary>
        /// Obtiene un fichero de la carpeta de lectura.
        /// </summary>
        /// <returns>Lista con los ficheros en la carpeta de lectura.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetFileLectura(string fileName)
        {            
            string path = _Configuracion.GetRutaDirectorioLectura() + fileName;
            return File(System.IO.File.ReadAllBytes(path), "application/force-download", fileName);
        }

        /// <summary>
        /// Elimina un fichero de la carpeta de escritura.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult RemoveFileLectura(string fileName)
        {
            string path = _Configuracion.GetRutaDirectorioLectura() + fileName;
            System.IO.File.Delete(path);
            return Ok();
        }

        /// <summary>
        /// Obtiene los ficheros en la carpeta de escritura.
        /// </summary>
        /// <returns>Lista con los ficheros en la carpeta de escritura.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<FileInfoEDMA> GetFilesEscritura()
        {
            List<FileInfoEDMA> filesInfo = new List<FileInfoEDMA>();
            List<string> files = System.IO.Directory.EnumerateFiles(_Configuracion.GetRutaDirectorioEscritura()).ToList();
            foreach (string file in files)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                filesInfo.Add(new FileInfoEDMA() { Name = fileInfo.Name, LastWriteTimeUtc = fileInfo.LastWriteTimeUtc, BytesSize = fileInfo.Length });
            }
            filesInfo = filesInfo.OrderByDescending(x => x.LastWriteTimeUtc).ToList();
            return filesInfo;
        }

        /// <summary>
        /// Obtiene un fichero de la carpeta de escritura.
        /// </summary>
        /// <returns>Lista con los ficheros en la carpeta de escritura.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetFileEscritura(string fileName)
        {
            string path = _Configuracion.GetRutaDirectorioEscritura() + fileName;
            return File(System.IO.File.ReadAllBytes(path), "application/force-download", fileName);
        }

        /// <summary>
        /// Elimina un fichero de la carpeta de escritura.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult RemoveFileEscritura(string fileName)
        {
            string path = _Configuracion.GetRutaDirectorioEscritura() + fileName;
            System.IO.File.Delete(path);
            return Ok();
        }
    }
}

