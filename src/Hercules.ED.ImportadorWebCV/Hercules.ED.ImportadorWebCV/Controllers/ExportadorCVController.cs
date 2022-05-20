using Gnoss.ApiWrapper.Model;
using Hercules.ED.ImportadorWebCV.Controllers;
using ImportadorWebCV;
using ImportadorWebCV.Exporta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

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
        public ActionResult Exportar([FromHeader][Required] string pCVID, [FromHeader] string lang, [FromHeader][Optional] List<string> secciones)
        {
            if (!Utils.UtilityExportar.EsMultiidioma(lang))
            {
                throw new Exception("El lenguaje de exportación es incorrecto");
            }

            ExportaDatos exporta = new ExportaDatos(_cvn, pCVID, lang);
            Entity entity = exporta.GetLoadedEntity(pCVID, "curriculumvitae");

            exporta.ExportaDatosIdentificacion(entity, secciones);
            exporta.ExportaSituacionProfesional(entity, secciones);
            exporta.ExportaFormacionAcademica(entity, secciones);
            exporta.ExportaActividadDocente(entity, secciones);
            exporta.ExportaExperienciaCientificaTecnologica(entity, secciones);
            exporta.ExportaActividadCientificaTecnologica(entity, secciones);
            exporta.ExportaTextoLibre(entity, secciones);

            return Ok();
        }

        [HttpPost("Preexportar")]
        public ActionResult Exportar([FromHeader][Required] string pCVID, [FromHeader] string lang)
        {
            ExportaDatos exporta = new ExportaDatos(_cvn, pCVID, lang);


            return Ok();
        }
    }
}
