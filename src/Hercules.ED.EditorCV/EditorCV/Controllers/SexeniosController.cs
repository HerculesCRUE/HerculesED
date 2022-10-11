using EditorCV.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public IActionResult ConseguirSexenios(string comite, string periodo, string perfil_tecnologico, string subcomite, string idInvestigador)
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
        public IActionResult NotifySexenios(string url_cdn, string identifier)
        {
            try
            {
                AccionesSexenios accionesSexenios = new AccionesSexenios();
                accionesSexenios.NotifySexenios(url_cdn, identifier);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
