using EditorCV.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EditorCV.Controllers
{
    public class AcreditacionesController : Controller
    {
        readonly ConfigService _Configuracion;
        public AcreditacionesController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        [HttpPost("ConseguirAcreditaciones")]
        public IActionResult ConseguirAcreditaciones(string comision, string tipo_acreditacion, string categoria_acreditacion, string investigadore)
        {
            try
            {
                AccionesAcreditaciones accionesAcreditaciones = new AccionesAcreditaciones();
                accionesAcreditaciones.GetAcreditaciones(_Configuracion, comision, tipo_acreditacion, categoria_acreditacion, investigadore);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        
        [HttpPost("Notify")]
        public IActionResult NotifyAcreditaciones(string url, string idUsuario)
        {
            try
            {
                AccionesAcreditaciones accionesAcreditaciones = new AccionesAcreditaciones();
                accionesAcreditaciones.NotifyAcreditaciones(url, idUsuario);
                
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
