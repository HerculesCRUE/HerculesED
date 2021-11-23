using Microsoft.AspNetCore.Mvc;
using GuardadoCV.Models;
using GuardadoCV.Models.API.Input;
using System;
using Microsoft.AspNetCore.Cors;

namespace GuardadoCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class EdicionCVController : ControllerBase
    {
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
        /// Obtiene una ficha de edición de una entidad desde otra ficha de edición
        /// </summary>
        /// <param name="pRdfType">Rdftype de la entidad</param>
        /// <param name="pEntityID">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        [HttpGet("GetEditEntity")]
        public IActionResult GetEditEntity(string pRdfType, string pEntityID, string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetEditEntity(pRdfType, pEntityID, pLang));
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
        public IActionResult ValidateSignatures([FromForm] string pSignatures, [FromForm] string pCVID, [FromForm] string pPersonID)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.ValidateSignatures(pSignatures, pCVID, pPersonID));
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
