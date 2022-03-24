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
    public class GuardadoCVController : ControllerBase
    {
        /// <summary>
        /// Cambia la privacidad de un item
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntity">Identificador de la entidad</param>
        /// <param name="pIsPublic">TRUE si es público</param>
        /// <returns></returns>
        [HttpPost("ChangePrivacityItem")]
        public IActionResult ChangePrivacityItem([FromForm]string pIdSection, [FromForm] string pRdfTypeTab, [FromForm] string pEntity, [FromForm] bool pIsPublic)
        {
            try
            {
                AccionesGuardado accionesGuardado = new AccionesGuardado();
                return Ok(accionesGuardado.ChangePrivacityItem(pIdSection, pRdfTypeTab, pEntity, pIsPublic));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un item de un listado
        /// </summary>
        /// <param name="pEntity">Entidad a eliminar</param>
        /// <returns></returns>
        [HttpPost("RemoveItem")]
        public IActionResult RemoveItem([FromForm] string pEntity)
        {
            try
            {
                AccionesGuardado accionesGuardado = new AccionesGuardado();
                return Ok(accionesGuardado.RemoveItem(pEntity));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }
        }

        /// <summary>
        /// Crea o actualiza una entidad
        /// TODO
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost("UpdateEntity")]
        public IActionResult UpdateEntity([FromForm] Entity entity, [FromForm] string cvID, [FromForm] string sectionID, [FromForm] string rdfTypeTab, [FromForm] string pLang)
        {
            try
            {
                AccionesGuardado accionesGuardado = new AccionesGuardado();
                return Ok(accionesGuardado.ActualizarEntidad(entity, cvID, sectionID, rdfTypeTab,pLang));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }
        }


        [HttpPost("ValidateORCID")]
        public IActionResult ValidateORCID([FromForm] string pOrcid)
        {
            try
            {
                AccionesGuardado accionesGuardado = new AccionesGuardado();
                return Ok(accionesGuardado.ValidateORCID(pOrcid));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }
        }

        [HttpPost("CreatePerson")]
        public IActionResult CreatePerson([FromForm] string pName, [FromForm] string pSurname)
        {
            try
            {
                AccionesGuardado accionesGuardado = new AccionesGuardado();
                return Ok(accionesGuardado.CreatePerson(pName, pSurname));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message });
            }
        }


        //Añadir entidad

        //Eliminar entidad
    }
}
