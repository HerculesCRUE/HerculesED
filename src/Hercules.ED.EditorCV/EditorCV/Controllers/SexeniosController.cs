using EditorCV.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class SexeniosController : Controller
    {
        readonly ConfigService _Configuracion;
        public SexeniosController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        [HttpPost("ConseguirSexenios")]
        public IActionResult ConseguirSexenios([FromForm] string comite, [FromForm] string periodo, [FromForm][Optional] string perfil_tecnologico, [FromForm][Optional] string subcomite, [FromForm] string idInvestigador)
        {
            try
            {
                AccionesSexenios accionesSexenios = new AccionesSexenios();
                accionesSexenios.GetSexenios(_Configuracion, comite, periodo, perfil_tecnologico, subcomite, idInvestigador);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost("Notify")]
        public IActionResult NotifySexenios(string url_cdn, string idUsuario)
        {
            try
            {
                AccionesSexenios accionesSexenios = new AccionesSexenios();
                accionesSexenios.NotifySexenios(url_cdn, idUsuario);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
