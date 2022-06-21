using EditorCV.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;

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

        [HttpGet("ObtenerDatosEnvioPRC")]
        public IActionResult ObtenerDatosEnvioPRC(string pIdDocumento, string pIdPersona, string pIdProyecto)
        {
            try
            {
                AccionesEnvioPRC accionesPRC = new AccionesEnvioPRC();
                accionesPRC.ObtenerDatosEnvioPRC(_Configuracion, pIdDocumento, pIdPersona, pIdProyecto);
            }
            catch (Exception)
            {
                
            }
            return Ok();
        }

        [HttpPost("EnvioPRC")]
        public IActionResult EnvioPRC(string pIdDocumento, string pIdProyecto)
        {
            try
            {
                AccionesEnvioPRC accionesPRC = new AccionesEnvioPRC();
                accionesPRC.EnvioPRC(_Configuracion, pIdDocumento, pIdProyecto);
            }
            catch (Exception)
            {
                throw;
            }

            return Ok();
        }

        [HttpPost("EnvioProyecto")]
        public IActionResult EnvioProyecto(string pIdProyecto, string pIdPersona, string pIdAutorizacion)
        {
            try
            {
                AccionesEnvioProyecto accionesProyecto = new AccionesEnvioProyecto();
                accionesProyecto.EnvioProyecto(_Configuracion, pIdProyecto, pIdPersona, pIdAutorizacion);
            }
            catch (Exception)
            {
                throw;
            }

            return Ok();
        }
    }
}
