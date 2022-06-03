﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using EditorCV.Models;
using EditorCV.Models.API.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using EditorCV.Models.Utils;
using EditorCV.Models.API.Templates;
using EditorCV.Models.API.Response;
using System.Net.Http;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class ImportadoCVController : Controller
    {
        readonly ConfigService _Configuracion;

        public ImportadoCVController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        [HttpPost("ImportarCV")]
        public IActionResult ImportarCV([Required][FromForm] string userID, [Required] IFormFile File)
        {
            try
            {
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }
                AccionesImportacion accionesImportacion = new AccionesImportacion();
                accionesImportacion.ImportarCV(pCVId, File);

                return Ok();
            } 
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }
    }
}
