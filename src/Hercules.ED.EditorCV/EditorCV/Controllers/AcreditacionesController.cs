using EditorCV.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class AcreditacionesController : Controller
    {
        readonly ConfigService _Configuracion;
        public AcreditacionesController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        [HttpPost("ConseguirAcreditaciones")]
        public IActionResult ConseguirAcreditaciones([FromForm] string comision, [FromForm] string tipo_acreditacion, [FromForm][Optional] string categoria_acreditacion, [FromForm] string idInvestigador)
        {
            try
            {
                AccionesAcreditaciones accionesAcreditaciones = new AccionesAcreditaciones();
                accionesAcreditaciones.GetAcreditaciones(_Configuracion, comision, tipo_acreditacion, categoria_acreditacion, idInvestigador);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost("Notify")]
        public IActionResult NotifyAcreditaciones([Required] string url_cdn, [Required] string idUsuario)
        {
            try
            {
                AccionesAcreditaciones accionesAcreditaciones = new AccionesAcreditaciones();
                accionesAcreditaciones.NotifyAcreditaciones(url_cdn, idUsuario);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
