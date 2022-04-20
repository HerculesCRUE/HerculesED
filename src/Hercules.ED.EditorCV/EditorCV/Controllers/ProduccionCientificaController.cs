using EditorCV.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class ProduccionCientificaController : ControllerBase
    {
        readonly ConfigService _Configuracion;

        public ProduccionCientificaController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        [HttpGet("EnvioPRC")]
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
    }
}
