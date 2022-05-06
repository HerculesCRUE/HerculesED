using Hercules.ED.ImportadorWebCV.Controllers;
using ImportadorWebCV;
using ImportadorWebCV.Exporta;
using ImportadorWebCV.Sincro;
using ImportadorWebCV.Sincro.Secciones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Hercules.ED.ExportadorWebCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExportadorCVController : ControllerBase
    {
        private readonly ILogger<ExportadorCVController> _logger;
        private cvnRootResultBean _cvn;
        readonly ConfigService _Configuracion;

        public ExportadorCVController(ILogger<ExportadorCVController> logger, ConfigService pConfig)
        {
            _logger = logger;
            _Configuracion = pConfig;
            _cvn = new cvnRootResultBean();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <returns></returns>
        [HttpPost("Exportar")]
        public ActionResult Exportar([FromHeader][Required] string pCVID, [FromHeader][Optional] List<string> secciones)
        {
            ExportaDatos exporta = new ExportaDatos(_cvn, pCVID);
            Entity entity = exporta.GetLoadedEntity(pCVID, "curriculumvitae");

            exporta.ExportaDatosIdentificacion(entity, secciones);
            exporta.ExportaSituacionProfesional(entity, secciones);
            //exporta.ExportaFormacionAcademica(entity, secciones);
            //exporta.ExportaActividadDocente(entity, secciones);
            //exporta.ExportaExperienciaCientificaTecnologica(entity, secciones);
            //exporta.ExportaActividadCientificaTecnologiaca(entity, secciones);
            exporta.ExportaTextoLibre(entity, secciones);

            return Ok();
        }

        [HttpPost("Preexportar")]
        public ActionResult Exportar([FromHeader][Required] string pCVID)
        {
            ExportaDatos exporta = new ExportaDatos(_cvn, pCVID);


            return Ok();
        }
    }
}
