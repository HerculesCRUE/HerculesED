using EditorCV.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class EnvioValidacionController : ControllerBase
    {
        readonly ConfigService _Configuracion;

        public EnvioValidacionController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        ///Servicio para obtener todos los proyectos de <paramref name="pIdPersona"/>, junto a su titulo, fecha de inicio, fecha de fin y organización.
        /// </summary>
        /// <param name="pIdPersona"></param>
        /// <returns></returns>
        [HttpGet("ObtenerDatosEnvioPRC")]
        public IActionResult ObtenerDatosEnvioPRC(string pIdPersona)
        {
            try
            {
                AccionesEnvioPRC accionesPRC = new AccionesEnvioPRC(_Configuracion);
                return Ok(accionesPRC.ObtenerDatosEnvioPRC(pIdPersona));
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }

        /// <summary>
        /// Servicio de envío a Producción Científica.
        /// </summary>
        /// <param name="pIdRecurso">ID del recurso que apunta al documento.</param>
        /// <param name="pIdProyecto">ID del recurso del proyecto.</param>
        /// <returns></returns>
        [HttpPost("EnvioPRC")]
        public IActionResult EnvioPRC([FromForm][Required] string pIdRecurso, [FromForm] List<string> pIdProyecto)
        {
            try
            {
                AccionesEnvioPRC accionesPRC = new AccionesEnvioPRC(_Configuracion);
                accionesPRC.EnvioPRC(_Configuracion, pIdRecurso, pIdProyecto);
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Servicio de envío de un proyecto a validación.
        /// </summary>
        /// <param name="pIdRecurso">ID del recurso que apunta al proyecto.</param>
        /// <param name="pIdPersona">ID del recurso de la persona.</param>
        /// <param name="pIdAutorizacion">ID del recurso de la autorización.</param>
        /// <returns></returns>
        [HttpPost("EnvioProyecto")]
        public IActionResult EnvioProyecto([FromForm] string pIdRecurso, [FromForm] string pIdPersona, [FromForm] string pIdAutorizacion)
        {
            try
            {
                AccionesEnvioProyecto accionesProyecto = new AccionesEnvioProyecto();
                accionesProyecto.EnvioProyecto(_Configuracion, pIdRecurso, pIdPersona, pIdAutorizacion);
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }

            return Ok();
        }
    }
}
