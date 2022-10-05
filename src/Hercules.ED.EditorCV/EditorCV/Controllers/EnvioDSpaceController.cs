using EditorCV.Models;
using EditorCV.Models.Utils;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class EnvioDSpaceController: ControllerBase
    {
        readonly ConfigService _Configuracion;

        public EnvioDSpaceController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        [HttpPost("EnvioDSpace")]
        public IActionResult EnvioDSpace([FromForm][Required] string pIdRecurso, IFormFile file)
        {
            try
            {
                if (!Security.CheckUsers(UtilityCV.GetUsersFromDocument(pIdRecurso), Request))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
                AccionesEnvioDSpace accionesEnvioDSpace = new AccionesEnvioDSpace(_Configuracion);
                accionesEnvioDSpace.EnvioDSpace(pIdRecurso, file);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }

            return Ok();
        }
    }
}
