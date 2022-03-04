using Microsoft.AspNetCore.Mvc;
using GuardadoCV.Models;
using GuardadoCV.Models.API.Input;
using System;
using Microsoft.AspNetCore.Cors;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using EditorCV.Controllers;
using EditorCV.Models.Enrichment;
using Newtonsoft.Json;

namespace GuardadoCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class EdicionCVController : ControllerBase
    {
        readonly ConfigService _Configuracion;

        public EdicionCVController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Obtiene un listado de sugerencias con datos existentes para esa propiedad en algún item de CV
        /// </summary>
        /// <param name="q">Texto por el que se van a buscar sugerencias</param>
        /// <param name="pProperty">Propiedad en la que se quiere buscar</param>
        /// <param name="pRdfType">Rdf:type de la entidad en la que se quiere buscar</param>
        /// <param name="pGraph">Grafo en el que se encuentra la propiedad</param>
        /// <param name="pGetEntityID">Obtiene el ID de la entidad además del valor de la propiedad</param>
        /// <returns></returns>
        [HttpPost("GetAutocomplete")]
        public IActionResult GetAutocomplete([FromForm] string q, [FromForm] string pProperty, [FromForm] string pRdfType, [FromForm] string pGraph, [FromForm] bool pGetEntityID, [FromForm] string lista)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetAutocomplete(q.ToLower(), pProperty, pRdfType, pGraph, pGetEntityID, lista?.Split(',').ToList()));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        /// <summary>
        /// Obtiene los datos de una pestaña dentro del editor
        /// </summary>
        /// <param name="pId">Identificador de la entidad a recuperar</param>
        /// <param name="pRdfType">Rdf:type de la entidad a recuperar</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <returns></returns>
        [HttpGet("GetTab")]
        public IActionResult GetTab(string pId, string pRdfType, string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetTab(pId, pRdfType, pLang));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        /// <summary>
        /// Obtiene una minificha de una entidad de un listado de una pestaña
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntityID">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        [HttpGet("GetItemMini")]
        public IActionResult GetItemMini(string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetItemMini(pIdSection, pRdfTypeTab, pEntityID, pLang));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }

        }

        /// <summary>
        /// Obtiene una ficha de edición de una entidad de un listado de una pestaña
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntityID">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        [HttpGet("GetEdit")]
        public IActionResult GetEdit(string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetEdit(pIdSection, pRdfTypeTab, pEntityID, pLang));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una serie de propiedades de una serie de entidades
        /// </summary>
        /// <param name="pItemsLoad">Elementos de los que buscar las propiedades</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        [HttpPost("LoadProps")]
        public IActionResult LoadProps([FromForm] ItemsLoad pItemsLoad, [FromForm] string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.LoadProps(pItemsLoad, pLang));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }
        }


        [HttpPost("ValidateSignatures")]
        public IActionResult ValidateSignatures([FromForm] string pSignatures, [FromForm] string pCVID, [FromForm] string pPersonID, [FromForm] string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.ValidateSignatures(pSignatures, pCVID, pPersonID, pLang));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los descriptores específicos y temáticos haciendo una petición a un servicio.
        /// </summary>
        /// <param name="pData">Objeto con los datos a querer obtener (Título, Descripción y URL del PDF).</param>
        /// <returns>Categorías y Tags.</returns>
        [HttpPost("EnrichmentTopics")]
        public IActionResult EnrichmentTopics([FromForm] EnrichmentInput pData)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetEnrichment(_Configuracion, pData.pTitulo, pData.pDesc, pData.pUrlPdf));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }
        }

        //TODO es obligatorio añadirse a uno mismo como autor
        //TODO crear servicio que incluya/elimie en los CV a los autores
        //TODO entidades del propio CV
        //GEstión multiidioma




    }
}
