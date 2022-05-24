using Gnoss.ApiWrapper.Model;
using Hercules.ED.ImportadorWebCV.Controllers;
using ImportadorWebCV;
using ImportadorWebCV.Exporta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Linq;
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
        public FileResult Exportar([FromForm][Required] string pCVID, [FromForm][Required] string lang)
        {
            if (!Utils.UtilityExportar.EsMultiidioma(lang))
            {
                throw new Exception("El lenguaje de exportación es incorrecto");
            }

            ExportaDatos exporta = new ExportaDatos(_cvn, pCVID, lang);
            Entity entity = exporta.GetLoadedEntity(pCVID, "curriculumvitae");

            exporta.ExportaDatosIdentificacion(entity, _Configuracion.GetVersion());
            exporta.ExportaSituacionProfesional(entity);
            exporta.ExportaFormacionAcademica(entity);
            exporta.ExportaActividadDocente(entity);
            exporta.ExportaExperienciaCientificaTecnologica(entity);
            exporta.ExportaActividadCientificaTecnologica(entity);
            exporta.ExportaTextoLibre(entity);

            return Ok();
        }

        [HttpPost("ExportarLimitado")]
        public ActionResult Exportar([FromForm][Required] string pCVID, [FromForm][Required] string lang, [Optional] List<string> listaId)
        {
            if (!Utils.UtilityExportar.EsMultiidioma(lang))
            {
                throw new Exception("El lenguaje de exportación es incorrecto");
            }

            ExportaDatos exporta = new ExportaDatos(_cvn, pCVID, lang);
            Entity entity = exporta.GetLoadedEntity(pCVID, "curriculumvitae");

            exporta.ExportaDatosIdentificacion(entity, _Configuracion.GetVersion(), listaId);
            exporta.ExportaSituacionProfesional(entity, listaId);
            exporta.ExportaFormacionAcademica(entity, listaId);
            exporta.ExportaActividadDocente(entity, listaId);
            exporta.ExportaExperienciaCientificaTecnologica(entity, listaId);
            exporta.ExportaActividadCientificaTecnologica(entity, listaId);
            exporta.ExportaTextoLibre(entity, listaId);



            return Ok();
        }
    }
}
