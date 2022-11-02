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

        /// <summary>
        /// Servicio encargado de hacer la petición de acreditaciones.
        /// </summary>
        /// <param name="comision">Comisión</param>
        /// <param name="tipo_acreditacion">Tipo de acreditación</param>
        /// <param name="categoria_acreditacion">Categoría de acreditación</param>
        /// <param name="idInvestigador">Identificador del investigador</param>
        /// <returns>OK si se realiza la petición con exito</returns>
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

        /// <summary>
        /// Servicio encargado de la recepción de la respuesta de la petición de acreditaciones
        /// </summary>
        /// <param name="url_cdn">Dirección url</param>
        /// <param name="idUsuario">Identificador del usuario</param>
        /// <returns>OK si se inserta el triple</returns>
        [HttpPost("Notify")]
        public IActionResult NotifyAcreditaciones([Required] string url_cdn, [Required] string idUsuario)
        {
            try
            {
                if(string.IsNullOrEmpty(url_cdn) || string.IsNullOrEmpty(idUsuario))
                {
                    return BadRequest();
                }

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
