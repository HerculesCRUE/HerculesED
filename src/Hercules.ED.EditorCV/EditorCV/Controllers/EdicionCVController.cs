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
using System.Net.Http;
using System.Net;
using System.Web.Http;
using Microsoft.AspNetCore.Http;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class EdicionCVController : ControllerBaseService
    {
        readonly ConfigService _Configuracion;

        public EdicionCVController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }


        #region Eliminar
        private static readonly Gnoss.ApiWrapper.ResourceApi mResourceApi = new Gnoss.ApiWrapper.ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        /// <summary>
        /// Obtiene la URL de un CV a partir de un usuario
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpGet("Test")]
        public IActionResult Test()
        {
            //Guid usuarioID = Guid.Empty;
            //if (!base.HavePermission(Request, usuarioID))
            //{
            //    return StatusCode(StatusCodes.Status401Unauthorized);
            //}
            DateTime inicio = DateTime.Now;
            mResourceApi.VirtuosoQuery("select *", "where{?s ?p ?o}limit 1", "curriculumvitae");
            DateTime fin = DateTime.Now;
            string response = (fin - inicio).TotalMilliseconds.ToString()+ Request.Cookies["_UsuarioActual"];
            return Ok((fin - inicio).TotalMilliseconds);
        }
        #endregion

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

        /// <summary>
        /// Obtiene datos de una entidad
        /// </summary>
        /// <param name="pGraph">Grafo</param>
        /// <param name="pEntity">Entidad de la que obtener datos</param>
        /// <param name="pProperties">Propiedades a obtener</param>
        /// <returns></returns>
        [HttpPost("GetPropertyEntityData")]
        public IActionResult GetPropertyEntityData([FromForm] string pGraph, [FromForm] string pEntity, [FromForm] List<string> pProperties)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetPropertyEntityData(pGraph, pEntity, pProperties));
            }
            catch (Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        [HttpGet("GetItemsDuplicados")]
        public IActionResult GetItemsDuplicados(string pCVId, float pMinSimilarity = 0.9f, string pItemId = null)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetItemsDuplicados(pCVId, pMinSimilarity, pItemId));
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
        public IActionResult GetTab(string pCVId, string pId, string pRdfType, string pLang, string pSection = null, bool pOnlyPublic = false)
        {
            try
            {
                Gnoss.ApiWrapper.UserApi mUserApi = new Gnoss.ApiWrapper.UserApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
                string cookie = Request.Cookies["_UsuarioActual"];

                try
                {
                    Guid userID = mUserApi.GetUserIDFromCookie(Request.Cookies["_UsuarioActual"]);
                    cookie += "-" + userID;
                }
                catch (Exception)
                {
                    cookie += "-ERROR";
                }

                return Ok(cookie);
                Guid usuarioCV = UtilityCV.GetUserFromCV(pCVId);
                if (!base.CheckUser(Request, usuarioCV))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetTab(_Configuracion, pCVId, pId, pRdfType, pLang, pSection, pOnlyPublic));
            }
            catch (Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }
        /// <summary>
        /// Obtiene todos los datos marcados como públicos de la persona
        /// </summary>
        /// <param name="pPersonID">Identificador de la persona</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <returns></returns>
        [HttpGet("GetAllPublicData")]
        public IActionResult GetAllPublicData(string pPersonID, string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetAllPublicData(_Configuracion, pPersonID, pLang));
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

        /// <summary>
        /// Devuelve el tesauro pedido <paramref name="tesaurus"/> en el idioma marcado <paramref name="pLang"/>
        /// </summary>
        /// <param name="tesaurus"></param>
        /// <param name="pLang"></param>
        /// <returns></returns>
        [HttpGet("GetTesaurus")]
        public IActionResult GetTesaurus(string tesaurus, string pLang)
        {
            try
            {
                AccionesEdicion accionesEdicion = new AccionesEdicion();
                return Ok(accionesEdicion.GetTesauros(accionesEdicion.ConseguirNombreTesauro(tesaurus), pLang).Values);
            }
            catch (Exception ex)
            {
                return Ok(new EditorCV.Models.API.Response.JsonResult() { error = ex.Message });
            }
        }

        //TODO entidades del propio CV
        //GEstión multiidioma




    }
}
