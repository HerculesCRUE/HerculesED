using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Cors;
using System.Linq;
using EditorCV.Controllers;
using EditorCV.Models.Enrichment;
using EditorCV.Models;
using EditorCV.Models.API.Input;
using EditorCV.Models.Utils;
using System.Collections.Generic;
using EditorCV.Models.Similarity;

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
        /// Obtiene la URL de un CV a partir de un usuario
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpGet("GetCVUrl")]
        public IActionResult GetCVUrl(string userID, string lang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetCVUrl(userID, lang));
            }
            catch (Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }


        /// <summary>
        /// Obtiene un listado de sugerencias con datos existentes para esa propiedad en algún item de CV
        /// </summary>
        /// <param name="q">Texto por el que se van a buscar sugerencias</param>
        /// <param name="pProperty">Propiedad en la que se quiere buscar</param>
        /// <param name="pRdfType">Rdf:type de la entidad en la que se quiere buscar</param>
        /// <param name="pGraph">Grafo en el que se encuentra la propiedad</param>
        /// <param name="pGetEntityID">Obtiene el ID de la entidad además del valor de la propiedad</param>
        /// <param name="lista">Lista de valores ya introducidos</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pCache">Indica si hay que cachear</param>
        /// <returns></returns>
        [HttpPost("GetAutocomplete")]
        public IActionResult GetAutocomplete([FromForm] string q, [FromForm] string pProperty, [FromForm] List<string> pPropertiesAux, [FromForm] string pPrint, [FromForm] string pRdfType, [FromForm] string pGraph, [FromForm] bool pGetEntityID, [FromForm] string lista, [FromForm] string pLang, [FromForm] bool pCache)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetAutocomplete(q.ToLower(), pProperty, pPropertiesAux, pPrint, pRdfType, pGraph, pGetEntityID, lista?.Split(',').ToList(), pLang, pCache));
            }
            catch (Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        [HttpGet("GetItemsDuplicados")]
        public IActionResult GetItemsDuplicados(string pCVId, float pMinSimilarity = 0.9f)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetItemsDuplicados(pCVId, pMinSimilarity));
            }
            catch (Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }


        /// <summary>
        /// Obtiene los datos de una pestaña dentro del editor
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <param name="pId">Identificador de la entidad a recuperar</param>
        /// <param name="pRdfType">Rdf:type de la entidad a recuperar</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <param name="pSection">Sección</param>
        /// <returns></returns>
        [HttpGet("GetTab")]
        public IActionResult GetTab(string pCVId, string pId, string pRdfType, string pLang, string pSection = null)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetTab(_Configuracion, pCVId, pId, pRdfType, pLang, pSection));
            }
            catch (Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        /// <summary>
        /// Obtiene una minificha de una entidad de un listado de una pestaña
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntityID">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        [HttpGet("GetItemMini")]
        public IActionResult GetItemMini(string pCVId, string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetItemMini(_Configuracion, pCVId, pIdSection, pRdfTypeTab, pEntityID, pLang));
            }
            catch (Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message });
            }

        }

        /// <summary>
        /// Obtiene una ficha de edición de una entidad de un listado de una pestaña
        /// </summary>
        /// <param name="pCVId">Identificador del CV</param>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntityID">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        [HttpGet("GetEdit")]
        public IActionResult GetEdit(string pCVId, string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetEdit(pCVId, pIdSection, pRdfTypeTab, pEntityID, pLang));
            }
            catch (Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message });
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
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message });
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
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message });
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
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message });
            }
        }

        [HttpGet("GetTesaurus")]
        public IActionResult GetTesaurus(string tesaurus, string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetTesauros(accionesEdicion.ConseguirNombreTesauro(tesaurus), pLang).Values);
            }
            catch(Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message });
            }
        }

        //TODO entidades del propio CV
        //GEstión multiidioma




    }
}
